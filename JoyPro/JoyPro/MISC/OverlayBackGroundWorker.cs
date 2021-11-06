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
        public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Modifier = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> CurrentButtonMapping = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        public string CurrentGame = "";
        public string CurrentPlane = "";
        public ConcurrentDictionary<TextAliveField, object> TextTimeAlive = new ConcurrentDictionary<TextAliveField,object>();
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
            CurrentButtonMapping.Clear();
            Console.WriteLine("ButtonSet called: " + CurrentGame + " " + CurrentPlane);
            if (CurrentGame == "DCS")
            {
                CurrentButtonMapping = InternalDataMangement.GetAirCraftLayout(CurrentGame, CurrentPlane);
            }
            else if(CurrentGame=="")
            {
                CurrentButtonMapping = InternalDataMangement.GetAirCraftLayout(CurrentGame, CurrentPlane);
            }
            else
            {
                CurrentButtonMapping = InternalDataMangement.GetAirCraftLayout(CurrentGame, CurrentGame);
            }
            stopwatch.Stop();
            Console.WriteLine("Assigning new ButtonLayout took: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        void SetupModifierDict()
        {
            Modifier = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
            foreach (KeyValuePair<string, Modifier> kvp in InternalDataMangement.AllModifiers)
            {
                Modifier.TryAdd(kvp.Value.device, new ConcurrentDictionary<string, string>());
                Modifier[kvp.Value.device].TryAdd(kvp.Value.key, kvp.Key);
            }
        }

        public void StartDisplayDispatcher()
        {
            while (true)
            {
                overlay.Dispatcher.Invoke(new Action(() => {
                    int last=0;
                    for (int i = 0; i < TextTimeAlive.Count; ++i)
                    {
                        overlay.shownLabels[i].Content = TextTimeAlive.ElementAt(i).Key.Text;
                        last = i;
                    }
                    last=TextTimeAlive.Count;
                    for(int i = last;i<MainStructure.msave.OvlElementsToShow; ++i)
                    {
                        overlay.shownLabels[i].Content = "";
                    }
                }));
                Thread.Sleep(MainStructure.msave.OvlPollTime);
            }
        }

        List<string> getListOfAllButtonUses(string stick, string btn)
        {
            List<string> result = new List<string>();
            if(!CurrentButtonMapping.ContainsKey(stick))return result;
            foreach(KeyValuePair<string, string> kvp in CurrentButtonMapping[stick])
            {
                if (kvp.Key.EndsWith(btn)) result.Add(kvp.Key);
            }
            return result;
        }

        string IsModifiedBtnPressed(string stick, string btn)
        {
            List<string> modifyoptions = getListOfAllButtonUses(stick, btn);
            modifyoptions.Sort((x,y) => x.Length.CompareTo(y.Length));
            if (modifyoptions.Count < 1) return null;
            else
            {
                for(int i = modifyoptions.Count-1; i >=0; i=i-1)
                {
                    bool allModified = true;
                    string[] parts = modifyoptions[i].Split('+');
                    for(int j=0;j<parts.Length-1; ++j)
                    {
                        string modDevice = InternalDataMangement.AllModifiers[parts[j]].device;
                        string modbtn = InternalDataMangement.AllModifiers[parts[j]].key;
                        if (!(currentPressedNonSwitched.ContainsKey(modDevice) && currentPressedNonSwitched[modDevice].Contains(modbtn)))
                        {
                            allModified = false;
                            break;
                        }
                    }
                    if (allModified)
                    {
                        return CurrentButtonMapping[stick][modifyoptions[i]];
                    }
                }
            }
            return null;
        }

        int TimeAliveContains(string oof)
        {
            int found = -1;
            for(int i = 0; i < TextTimeAlive.Count; ++i)
            {
                if (TextTimeAlive.ElementAt(i).Key.Text == oof)
                {
                    return found;
                }
            }
            return found;
        }

        public void StartDisplayBackgroundWorker()
        {
            SetupModifierDict();
            try
            {
                while (true)
                {
                    for(int i=0; i<TextTimeAlive.Count; i++)
                    {
                        TextAliveField old = TextTimeAlive.ElementAt(i).Key;
                        old.Time = old.Time - MainStructure.msave.OvlPollTime;
                    }
                    foreach(KeyValuePair<string, List<string>> kvp in currentPressed)
                    {
                        for(int i=0; i<kvp.Value.Count; i++)
                        {
                            bool found = false;
                            string text=IsModifiedBtnPressed(kvp.Key, kvp.Value[i]);
                            if (text!=null)
                            {
                                if (MainStructure.msave.stackedMode)
                                {
                                    while (TimeAliveContains(text)>=0)
                                    {
                                        text = text + " ";
                                    }
                                    TextAliveField taf = new TextAliveField() { Text = text, Time=MainStructure.msave.TextTimeAlive };
                                    TextTimeAlive.TryAdd(taf,null);
                                }
                                else
                                {
                                    int index = TimeAliveContains(text);
                                    if (index>=0)
                                    {
                                        TextTimeAlive.ElementAt(index).Key.Time = MainStructure.msave.TextTimeAlive;
                                    }
                                    else
                                    {
                                        TextAliveField taf = new TextAliveField() { Text=text, Time=MainStructure.msave.TextTimeAlive};
                                        TextTimeAlive.TryAdd(taf, null);
                                    }
                                }
                                
                            }
                        }
                    }


                    for (int i = TextTimeAlive.Count-1; i >=0; i=i-1)
                    {
                        if((TextTimeAlive.ElementAt(i).Key.Time!=MainStructure.msave.TextTimeAlive&&!MainStructure.msave.OvlFade)||TextTimeAlive.ElementAt(i).Key.Time<0)
                        {
                            TextAliveField taf = TextTimeAlive.ElementAt(i).Key;
                            object todel=null;
                            while(!TextTimeAlive.TryRemove(taf, out todel))
                            {

                            }
                        }
                        
                    }
                    Thread.Sleep(MainStructure.msave.OvlPollTime);
                }
            }catch (Exception ex)
            {
                Console.WriteLine (ex.ToString());
                Thread.Sleep(500);
                StartDisplayBackgroundWorker();
            }
        }
    }
}
