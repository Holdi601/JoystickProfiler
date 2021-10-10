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


namespace JoyPro
{
    public static class Updater
    {
        public static event EventHandler DownloadCompletedEvent;
        static int downloadFails = 0;
        static string newestAvailableVersion;
        const string externalWebUrl = "https://raw.githubusercontent.com/Holdi601/JoystickProfiler/master/JoyPro/JoyPro/ver.txt";
        const string buildPath = "https://github.com/Holdi601/JoystickProfiler/raw/master/Builds/JoyPro_WinX64_v";


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
                return;
            }
            Console.WriteLine(MainStructure.PROGPATH);
            ProcessStartInfo startInfo = new ProcessStartInfo(MainStructure.PROGPATH + "\\TOOLS\\Unzipper\\UnzipMeHereWin.exe");
            startInfo.Arguments = "\"" + MainStructure.PROGPATH + "\\NewerVersion.zip\" \"" + MainStructure.PROGPATH + "\" \"" + MainStructure.PROGPATH + "\\JoyPro.exe\"";
            Process.Start(startInfo);
            Environment.Exit(0);
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
}
