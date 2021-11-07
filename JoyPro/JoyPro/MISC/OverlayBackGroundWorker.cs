using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace JoyPro
{
    public class TextAliveField
    {
        public string Text { get; set; }
        public int Time { get; set; }
    }

    public struct DeviceButtonName
    {
        public string Name;
        public string Device;
        public string Btn;
    }
    public class OverlayBackGroundWorker
    {
        public OverlayWindow overlay;    
        public string CurrentGame = "";
        public string CurrentPlane = "";
        public bool keepDisplayBackgroundWorkerRunning = true;
        public bool keepDisplayDispatcherRunning = true;
        public TextAliveField[] TextTimeAlive = null;
        public static ConcurrentDictionary<string, List<string>> currentPressed = new ConcurrentDictionary<string, List<string>>();
        public static ConcurrentDictionary<string, List<string>> currentPressedNonSwitched = new ConcurrentDictionary<string, List<string>>();
        public void GameRunningCheck()
        {
            while (true)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Process[] processes = Process.GetProcessesByName("DCS");
                if (processes == null || processes.Length < 1)
                {
                    processes = Process.GetProcessesByName("Il-2");
                    if (processes == null || processes.Length < 1)
                    {
                        if (CurrentGame != "")
                        {
                            CurrentGame = "";
                            SetButtonMapping();
                        }
                        CurrentGame = "";
                    }
                    else
                    {
                        CurrentGame = "IL2Game";
                        SetButtonMapping();
                    }
                }
                else
                {
                    CurrentGame = "DCS";
                }
                stopwatch.Stop();
                //Console.WriteLine("Checking Game took: " + stopwatch.ElapsedMilliseconds + "ms");
                Thread.Sleep(500);
            }
        }

        public void StartDCSListener()
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 1992);
                listener.Start();
                Console.WriteLine("DCS Listener started");
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Connected");
                    StreamReader reader = new StreamReader(client.GetStream());
                    StreamWriter writer = new StreamWriter(client.GetStream());
                    string s = string.Empty;
                    while (true)
                    {
                        s = reader.ReadLine();
                        if (s == "exit") break;
                        else
                        {
                            CurrentPlane = s;
                            Console.WriteLine(s + " New plane set");
                            SetButtonMapping();
                        }
                    }
                    reader.Close();
                    writer.Close();
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                StartDCSListener();
            }
        }

        public void SetButtonMapping()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TextTimeAlive = new TextAliveField[MainStructure.msave.OvlElementsToShow];
            InternalDataManagement.CurrentButtonMapping.Clear();
            Console.WriteLine("ButtonSet called: " + CurrentGame + " " + CurrentPlane);
            if (CurrentGame == "DCS")
            {
                InternalDataManagement.CurrentButtonMapping = InternalDataManagement.GetAirCraftLayout(CurrentGame, CurrentPlane);
            }
            else if (CurrentGame == "")
            {
                InternalDataManagement.CurrentButtonMapping = InternalDataManagement.GetAirCraftLayout(CurrentGame, CurrentPlane);
            }
            else
            {
                InternalDataManagement.CurrentButtonMapping = InternalDataManagement.GetAirCraftLayout(CurrentGame, CurrentGame);
            }
            stopwatch.Stop();
            Console.WriteLine("Assigning new ButtonLayout took: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public void StartDisplayDispatcher()
        {
            while (keepDisplayDispatcherRunning)
            {
                if (TextTimeAlive != null)
                    overlay.Dispatcher.Invoke(new Action(() =>
                    {
                        for (int i = 0; i < TextTimeAlive.Length; ++i)
                        {
                            if (TextTimeAlive[i] != null)
                            {
                                overlay.shownLabels[i].Content = TextTimeAlive[i].Text;
                            }
                            else
                            {
                                overlay.shownLabels[i].Content = "";
                            }
                        }
                    }));
                Thread.Sleep(MainStructure.msave.OvlPollTime);
            }
        }

        int TimeAliveContains(string oof)
        {
            int found = -1;
            for (int i = 0; i < TextTimeAlive.Length; ++i)
            {
                if (TextTimeAlive[i] == null) return found;
                if (TextTimeAlive[i].Text == oof)
                {
                    return found;
                }
            }
            return found;
        }

        void AddToTimeAliveArray(TextAliveField taf)
        {
            for (int i = 0; i < TextTimeAlive.Length; i++)
            {
                if (TextTimeAlive[i] == null)
                {
                    TextTimeAlive[i] = taf;
                    return;
                }
            }
        }

        void RemovedTimedOutFromArray()
        {
            for (int i = TextTimeAlive.Length - 1; i >= 0; i = i - 1)
            {
                if ((MainStructure.msave.OvlFade && TextTimeAlive[i] != null && TextTimeAlive[i].Time < 0) ||
                    (!MainStructure.msave.OvlFade && TextTimeAlive[i] != null && TextTimeAlive[i].Time != MainStructure.msave.TextTimeAlive))
                {
                    TextTimeAlive[i] = null;
                }
            }
        }

        void CloseNullGapsInArray()
        {
            int offset = 0;
            for(int i = 0;i+offset < TextTimeAlive.Length; i++)
            {
                if(TextTimeAlive[i+offset] == null)
                {
                    int counter=i+offset+1;
                    while (counter < TextTimeAlive.Length)
                    {
                        if (TextTimeAlive[counter] != null)
                        {
                            TextTimeAlive[i] = TextTimeAlive[counter];
                            TextTimeAlive[counter]=null;
                            break;
                        }
                        counter++;
                    }
                    offset = counter - i;
                }
            }
        }

        public void StartDisplayBackgroundWorker()
        {
            try
            {
                while (keepDisplayBackgroundWorkerRunning)
                {
                    if (TextTimeAlive != null)
                    {
                        for (int i = 0; i < TextTimeAlive.Length; i++)
                        {
                            TextAliveField old = TextTimeAlive[i];
                            if (old != null) old.Time = old.Time - MainStructure.msave.OvlPollTime;
                        }
                        foreach (KeyValuePair<string, List<string>> kvp in currentPressed)
                        {
                            for (int i = 0; i < kvp.Value.Count; i++)
                            {
                                string text = InternalDataManagement.GetTextForPressedButton(kvp.Key, kvp.Value[i], currentPressed, currentPressedNonSwitched);
                                if (text != null)
                                {
                                    if (MainStructure.msave.stackedMode)
                                    {
                                        while (TimeAliveContains(text) >= 0)
                                        {
                                            text = text + " ";
                                        }
                                        TextAliveField taf = new TextAliveField() { Text = text, Time = MainStructure.msave.TextTimeAlive };
                                        AddToTimeAliveArray(taf);
                                    }
                                    else
                                    {
                                        int index = TimeAliveContains(text);
                                        if (index >= 0)
                                        {
                                            TextTimeAlive[index].Time = MainStructure.msave.TextTimeAlive;
                                        }
                                        else
                                        {
                                            TextAliveField taf = new TextAliveField() { Text = text, Time = MainStructure.msave.TextTimeAlive };
                                            AddToTimeAliveArray(taf);
                                        }
                                    }

                                }
                            }
                        }
                        RemovedTimedOutFromArray();
                        CloseNullGapsInArray();
                    }
                    Thread.Sleep(MainStructure.msave.OvlPollTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Thread.Sleep(500);
                StartDisplayBackgroundWorker();
            }
        }
    }
}
