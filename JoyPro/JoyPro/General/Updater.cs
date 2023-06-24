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
using System.Security.Cryptography;

namespace JoyPro
{
    public static class Updater
    {
        public static event EventHandler DownloadCompletedEvent;
        static int downloadFails = 0;
        public static string newestAvailableVersion;
        public static string newestAvailableVersionFingerprint="";
        const string externalWebUrl = "https://raw.githubusercontent.com/Holdi601/JoystickProfiler/master/JoyPro/JoyPro/ver.txt";
        const string buildPath = "https://github.com/Holdi601/JoystickProfiler/raw/master/Builds/JoyPro_WinX64_v";
        static object _Lock= new object();
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
                    MainStructure.Write(buildPath + newestAvailableVersion + ".zip");
                    DownloadCompletedEvent += new EventHandler(DownloadCompleted);
                    Task.Run(() => DownloadAsync(uri, "NewerVersion.zip"));
                }
                catch(Exception ex)
                {
                    MainStructure.NoteError(ex);
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
            MainStructure.Write(MainStructure.PROGPATH);
            if (Directory.Exists(MainStructure.PROGPATH + "\\TOOLS\\Unzip\\"))
            {
                if (!Directory.Exists(MainStructure.PROGPATH + "\\TOOLS\\temp\\"))
                {
                    Directory.CreateDirectory(MainStructure.PROGPATH + "\\TOOLS\\temp\\");
                }
                try
                {
                    MainStructure.CopyFolderIntoFolder(MainStructure.PROGPATH + "\\TOOLS\\Unzip\\", MainStructure.PROGPATH + "\\TOOLS\\temp\\");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not make temperory copy of unzipper");
                    MainStructure.Write(ex.Message);
                    MainStructure.Write(ex.StackTrace);
                    MainStructure.Write(ex.Source);
                    MainStructure.Write(ex.HelpLink);
                    return;
                }
                
                ProcessStartInfo startInfo = new ProcessStartInfo(MainStructure.PROGPATH + "\\TOOLS\\temp\\UnzipMeHereWin.exe");
                startInfo.Arguments = "\"" + MainStructure.PROGPATH + "\\NewerVersion.zip\" \"" + MainStructure.PROGPATH + "\" \"" + MainStructure.PROGPATH + "\\JoyPro.exe\"";
                string fngrprntFilePath = MainStructure.PROGPATH + "\\NewerVersion.zip";
                //byte[] fileHash = MainStructure.GetFileHash(fngrprntFilePath);
                //string fileHashString = BitConverter.ToString(fileHash).Replace("-", string.Empty);
                //if (fileHashString != newestAvailableVersionFingerprint)
                //{
                //    MainStructure.Write("Fingerprint mismatches. Cancelling update");
                //    MessageBox.Show("The fingerprint of the downloaded File mismatches with the remote file");
                //    return;
                //}
                try
                {
                    Process.Start(startInfo);
                    Environment.Exit(0);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Could not start unzipper");
                    MainStructure.Write(ex.Message);
                    MainStructure.Write(ex.StackTrace);
                    MainStructure.Write(ex.Source);
                    MainStructure.Write(ex.HelpLink);
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
                    newestAvailableVersion = reader.ReadLine().Replace("v", "");
                    //string filePath = "D:\\Dropbox\\Programmierung\\c#\\JoyPro\\JoystickProfiler\\Builds\\JoyPro_WinX64_v0088.zip";
                    //byte[] fileHash = MainStructure.GetFileHash(filePath);
                    //string fileHashString = BitConverter.ToString(fileHash).Replace("-", string.Empty);
                    //MainStructure.Write(fileHashString);
                    newestAvailableVersionFingerprint = reader.ReadLine();
                    return Convert.ToInt32(newestAvailableVersion);
                }
            }
            catch(Exception ex)
            {
                MainStructure.NoteError(ex);
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
