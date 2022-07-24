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
    public static class JoystickProfileDownloader
    {
        const string externalWebUrl = "https://raw.githubusercontent.com/Holdi601/JoystickProfiler/master/DEFAULTSTICKS/";
        public static string stick = "";
        public static string stickOg = "";
        public static bool finished = false;
        public static bool DoesFileExistinProfiles()
        {
            try
            {
                string url = externalWebUrl + stick + ".pr0file";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
            
            }
            return false;
        }

        public static void DownloadJoystickProfile()
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += fileDownloaded;
                wc.DownloadFileAsync(
                    new System.Uri(externalWebUrl + stick + ".pr0file"),
                    Environment.CurrentDirectory+"\\"+stick+".pr0file"
                );
            }
        }

        public static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage);
        }

        static void fileDownloaded(object sender, EventArgs e)
        {
            finished = true;
            InternalDataManagement.LoadProfile(Environment.CurrentDirectory + "\\" + stick + ".pr0file", true, stickOg);
            InitGames.CheckIfDevicesNeeded();
        }
    }
}
