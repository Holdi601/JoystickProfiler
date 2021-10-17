using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Principal;
using System.Threading;

namespace JoyPro
{
    public static class Updater
    {
        public static event EventHandler DownloadCompletedEvent;
        static int downloadFails = 0;
        static string newestAvailableVersion;
        const string externalWebUrl = "https://raw.githubusercontent.com/Holdi601/JoystickProfiler/master/JoyPro/JoyPro/ver.txt";
        const string buildPath = "https://github.com/Holdi601/JoystickProfiler/raw/master/Builds/JoyPro_WinX64_v";
        static object _Lock= new object();
        static int _consolePosition;
        static readonly CancellationTokenSource source = new CancellationTokenSource();


        public static async Task DownloadAsync(Uri requestUri, string filename)
        {
            if (filename == null)
                return;

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    using (
                        Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await contentStream.CopyToAsync(stream);
                        
                    }
                }
            }
            DownloadCompletedEvent.Invoke(null, null);
        }

        public static void DownloadNewerVersion()
        {
            if (downloadFails < 10)
            {
                try
                {
                    Uri uri = new Uri(buildPath + newestAvailableVersion + ".zip");
                    Console.WriteLine(buildPath + newestAvailableVersion + ".zip");
                    DownloadCompletedEvent += new EventHandler(DownloadCompleted);
                    Task.Run(() => DownloadAsync(uri, "NewerVersion.zip"));
                }
                catch
                {
                    downloadFails++;
                    MessageBox.Show("Download failed: " + downloadFails.ToString() + " times.");
                    DownloadNewerVersion();
                }
            }
            else
            {
                MessageBox.Show("Failed to download after 10 tries");
            }
        }

        public static void DownloadCompleted(object o, EventArgs e)
        {

            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            if (!isElevated)
            {
                MessageBox.Show("User does not run the program with admin priviledges, it might not be able to overwrite itself. Please rerun with admin priviledges");
            }
            MessageBoxResult mrop = MessageBox.Show("Update downloaded. Last chance to cancel. Press Yes to continue, Press no to cancel.", "Update ready to install", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (mrop == MessageBoxResult.No)
            {
                return;
            }
            Console.WriteLine(MainStructure.PROGPATH);
            if (Directory.Exists(MainStructure.PROGPATH + "\\TOOLS\\Unzipper\\"))
            {
                if (!Directory.Exists(MainStructure.PROGPATH + "\\TOOLS\\temp\\"))
                {
                    Directory.CreateDirectory(MainStructure.PROGPATH + "\\TOOLS\\temp\\");
                }
                try
                {
                    MainStructure.CopyFolderIntoFolder(MainStructure.PROGPATH + "\\TOOLS\\Unzipper\\", MainStructure.PROGPATH + "\\TOOLS\\temp\\");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not make temperory copy of unzipper");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Source);
                    Console.WriteLine(ex.HelpLink);
                    return;
                }
                
                ProcessStartInfo startInfo = new ProcessStartInfo(MainStructure.PROGPATH + "\\TOOLS\\temp\\UnzipMeHereWin.exe");
                startInfo.Arguments = "\"" + MainStructure.PROGPATH + "\\NewerVersion.zip\" \"" + MainStructure.PROGPATH + "\" \"" + MainStructure.PROGPATH + "\\JoyPro.exe\"";
                try
                {
                    Process.Start(startInfo);
                    Environment.Exit(0);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Could not start unzipper");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Source);
                    Console.WriteLine(ex.HelpLink);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Updater/Unzipper not found, please manually update");
            }
            
        }
        public static int GetNewestVersionNumber()
        {
            try
            {
                WebClient web = new WebClient();
                System.IO.Stream stream = web.OpenRead(externalWebUrl);
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    newestAvailableVersion = reader.ReadToEnd().Replace("v", "");
                    return Convert.ToInt32(newestAvailableVersion);
                }
            }
            catch
            {


            }
            return -1;
        }

        

    }

    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, IProgress<long> progress, CancellationToken cancellationToken = default(CancellationToken), int bufferSize = 0x1000)
        {
            var buffer = new byte[bufferSize];
            int bytesRead;
            long totalRead = 0;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                //Thread.Sleep(10);
                progress.Report(totalRead);
            }
        }
    }
}
