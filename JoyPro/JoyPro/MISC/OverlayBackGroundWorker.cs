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
        public ConcurrentDictionary<string, int> TextTimeAlive = new ConcurrentDictionary<string, int>();
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

        List<DeviceButtonName> getDescWithModifiers(string modName)
        {
            List<DeviceButtonName> result = new List<DeviceButtonName>();
            foreach(KeyValuePair<string, ConcurrentDictionary<string, string>> kvp in CurrentButtonMapping)
            {
                foreach(KeyValuePair<string, string> kvpInner in kvp.Value)
                {
                    if (kvpInner.Value.ToLower().Contains(modName.ToLower()))
                    {
                        result.Add(new DeviceButtonName() { Device=kvp.Key,Btn=kvpInner.Key, Name=kvpInner.Value});
                    }
                }
            }
            return result;
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

        bool checkIfModsIsPressed(string[] mods)
        {
            for(int i=0; i<mods.Length-1; i++)
            {
                if(!(CurrentButtonMapping.ContainsKey(InternalDataMangement.AllModifiers[mods[i]].device) &&
                    CurrentButtonMapping[InternalDataMangement.AllModifiers[mods[i]].device]
                    .ContainsKey(InternalDataMangement.AllModifiers[mods[i]].key)))
                {
                    return false;
                }
            }
            return true;
        }
        public void StartDisplayDispatcher()
        {
            while (true)
            {
                overlay.Dispatcher.Invoke(new Action(() => {
                    int last=0;
                    for (int i = 0; i < TextTimeAlive.Count; ++i)
                    {
                        overlay.shownLabels[i].Content = TextTimeAlive.ElementAt(i).Key;
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
                if (kvp.Key.Contains(btn)) result.Add(kvp.Key);
            }
            return result;
        }

        string IsModifiedBtnPressed(string stick, string btn)
        {
            List<string> modifyoptions = getListOfAllButtonUses(stick, btn);
            if (modifyoptions.Count < 2) return null;
            else
            {
                
                for(int i = 0;i < modifyoptions.Count; ++i)
                {
                    bool allModified = true;
                    if (!modifyoptions[i].Contains('+')) continue;
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

        public void StartDisplayBackgroundWorker()
        {
            SetupModifierDict();
            try
            {
                while (true)
                {
                    for(int i=0; i<TextTimeAlive.Count; i++)
                    {
                        int rest = TextTimeAlive.ElementAt(i).Value - MainStructure.msave.OvlPollTime;
                        string elem = TextTimeAlive.ElementAt(i).Key;
                        TextTimeAlive[elem] = rest;
                    }
                    foreach(KeyValuePair<string, List<string>> kvp in currentPressed)
                    {
                        for(int i=0; i<kvp.Value.Count; i++)
                        {
                            bool found = false;
                            string text=IsModifiedBtnPressed(kvp.Key, kvp.Value[i]);
                            if (CurrentButtonMapping.ContainsKey(kvp.Key) && CurrentButtonMapping[kvp.Key].ContainsKey(kvp.Value[i])&&text==null)
                            {
                                text = CurrentButtonMapping[kvp.Key][kvp.Value[i]];
                            } 
                            if (text!=null)
                            {
                                if (TextTimeAlive.ContainsKey(text))
                                {
                                    TextTimeAlive[text] = MainStructure.msave.TextTimeAlive;
                                }
                                else
                                {
                                    TextTimeAlive.TryAdd(text, MainStructure.msave.TextTimeAlive);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < TextTimeAlive.Count; i++)
                    {
                        string key = TextTimeAlive.ElementAt(i).Key;
                        if ((TextTimeAlive[key] != MainStructure.msave.TextTimeAlive&&!MainStructure.msave.OvlFade)||
                            (MainStructure.msave.OvlFade&&TextTimeAlive[key]<0))
                        {
                            int test;
                            while (!TextTimeAlive.TryRemove(key, out test))
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
