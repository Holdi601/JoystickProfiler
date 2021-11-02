using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoyPro
{
    public class OverlayBackGroundWorker
    {
        public static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> CurrentButtonMapping = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        public static string CurrentGame = "";
        public static string CurrentPlane = "";

        public void GameRunningCheck()
        {
            while (true)
            {
                Process[] processes = Process.GetProcessesByName("DCS");
                if (processes == null || processes.Length < 1)
                {
                    processes = Process.GetProcessesByName("Il-2");
                    if (processes == null || processes.Length < 1)
                    {
                        CurrentGame = "";
                    }
                    else
                    {
                        CurrentGame = "IL2Game";
                    }
                }
                else
                {
                    CurrentGame = "DCS";
                }
                Thread.Sleep(500);
            }
        }

        public void StartDCSListener()
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1992);
                // Create a Socket that will use Tcp protocol      
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method  
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.  
                // We will listen 10 requests at a time  
                listener.Listen(10);
                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();

                // Incoming data from the client.    
                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    
                    if (data != CurrentPlane) CurrentPlane = data;
                }

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();


                Console.WriteLine("\n Press any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                StartDCSListener();
            }
        }

        public static void SetButtonMapping()
        {

        }

        public static void StartJoystickListening()
        {

        }
    }
}
