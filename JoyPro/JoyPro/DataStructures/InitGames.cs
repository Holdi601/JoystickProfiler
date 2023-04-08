﻿using JoyPro.StarCitizen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JoyPro
{
    public static class InitGames
    {
        public static string currentlyDownloadStick="";
        public static string currentlyDownloadStickOG = "";

        public static List<KeyValuePair<string, string>> GetDCSKneeboardPlaneReference()
        {
            if(!File.Exists(MainStructure.PROGPATH+"\\Overlay\\DCS\\rename.plane"))return new List<KeyValuePair<string, string>>();
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            StreamReader sr = new StreamReader(MainStructure.PROGPATH + "\\Overlay\\DCS\\rename.plane");
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] parts = line.Split('§');
                if (parts.Length < 2) continue;
                result.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
            }

            return result;
        }
        public static string GetCurrentDCSVersion()
        {
            string dcsInstallPath = InitGames.GetDCSInstallationPath();
            string savedGames;
            return null;
        }
        public static void DCSDBMatchesClean()
        {
            MainStructure.Write("Check if for every DCS db file is a clean file");
            List<string> missingCleans = new List<string>();
            string htmlPath = MainStructure.PROGPATH + "\\DB\\DCS";
            string cfPath = MainStructure.PROGPATH + "\\CleanProfile\\DCS";
            string cfPathKb = MainStructure.PROGPATH + "\\KeyboardCleanProfile\\DCS";
            DirectoryInfo htmlDirIn = new DirectoryInfo(htmlPath);
            FileInfo[] allFiles = htmlDirIn.GetFiles();
            foreach (FileInfo file in allFiles)
            {
                if (file.Name.EndsWith(".html")&&
                    !File.Exists(cfPath+"\\"+file.Name.Replace(".html",".cf"))&&
                    !missingCleans.Contains(file.Name.Replace(".html", ".cf")))
                {
                    MainStructure.Write("CF not found for " + file.FullName);
                    if (file.Name.Replace(".html","").ToLower()!="uilayer"&&
                        file.Name.Replace(".html", "").ToLower() != "cameraobject")
                        missingCleans.Add(file.Name.Replace(".html", ".cf"));
                }
                if (file.Name.EndsWith(".html") &&MainStructure.loadKeyboard&&
                    !File.Exists(cfPathKb + "\\" + file.Name.Replace(".html", ".cf")) &&
                    !missingCleans.Contains(file.Name.Replace(".html", ".cf")))
                {
                    MainStructure.Write("CF not found for " + file.FullName);
                    if (file.Name.Replace(".html", "").ToLower() != "uilayer"&&
                        file.Name.Replace(".html", "").ToLower() != "cameraobject")
                        missingCleans.Add(file.Name.Replace(".html", ".cf")+"(Keyboard Version)");
                }
            }
            if (missingCleans.Count > 0)
            {
                string errorMessage = "You are missing the Clean Profiles for the following Modules/Planes";
                for(int i=0; i<missingCleans.Count; i++)
                {
                    errorMessage = errorMessage + "\r\n" + missingCleans[i];
                }
                errorMessage = errorMessage + "\r\n" + "Either delete the according HTML files from the DB/DCS folder or create the clean Profile files for those";
                MainStructure.mainW.ShowMessageBox(errorMessage);
            }
        }

        public static void InitOverlayLookup()
        {
            if (File.Exists(MainStructure.PROGPATH + "\\Overlay\\DCS\\plane.rename"))
            {
                StreamReader sr = new StreamReader(MainStructure.PROGPATH + "\\Overlay\\DCS\\plane.rename");
                while (!sr.EndOfStream)
                {
                    string[] pair = sr.ReadLine().Split('§');
                    if (!OverlayBackGroundWorker.LookupCorrection.ContainsKey(pair[0]))
                    {
                        OverlayBackGroundWorker.LookupCorrection.Add(pair[0], pair[1]);
                    }
                }
                sr.Close();
                sr.Dispose();
            }
        }
        public static void InitDCSJoysticks()
        {
            List<string> Joysticks = new List<string>();
            MiscGames.GetDCSInputJoysticks(Joysticks);

            if (InternalDataManagement.LocalJoysticks == null)
                InternalDataManagement.LocalJoysticks = Joysticks.ToArray();
            else
            {
                List<string> pp = InternalDataManagement.LocalJoysticks.ToList();
                for (int i = 0; i < Joysticks.Count; ++i)
                {
                    if (!pp.Contains(Joysticks[i]))
                        pp.Add(Joysticks[i]);
                }
                InternalDataManagement.LocalJoysticks = pp.ToArray();
            }
        }
        public static string GetDCSInstallationPath()
        {
            if (MainStructure.msave.DCSInstallPathOR != null && Directory.Exists(MainStructure.msave.DCSInstallPathOR))
                return MainStructure.msave.DCSInstallPathOR;
            if (MiscGames.installPathsDCS.Length > 0)
            {
                if (MiscGames.installPathsDCS.Length == 1 && Directory.Exists(MiscGames.installPathsDCS[0]))
                {
                    return MiscGames.installPathsDCS[0];
                }
                else
                {
                    for (int i = 0; i < MiscGames.installPathsDCS.Length; ++i)
                    {
                        string instanceToCompare;
                        if (MainStructure.msave.DCSInstaceOverride != null && Directory.Exists(MainStructure.msave.DCSInstaceOverride)) instanceToCompare = MainStructure.msave.DCSInstaceOverride;
                        else instanceToCompare = MiscGames.DCSselectedInstancePath;
                        if (instanceToCompare.ToLower().Contains("openbeta") && MiscGames.installPathsDCS[i].ToLower().Contains("beta") && Directory.Exists(MiscGames.installPathsDCS[i]))
                        {
                            return MiscGames.installPathsDCS[i];
                        }
                        else if (!instanceToCompare.ToLower().Contains("openbeta") && !MiscGames.installPathsDCS[i].ToLower().Contains("beta") && Directory.Exists(MiscGames.installPathsDCS[i]))
                        {
                            return MiscGames.installPathsDCS[i];
                        }
                    }
                }
            }
            else
            {
                string basePath = "Program Files\\Eagle Dynamics\\DCS World";
                string openBetaExtention = " OpenBeta";
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                for (int i = 0; i < allDrives.Length; ++i)
                {
                    string path = allDrives[i].Name + basePath;
                    if (MiscGames.DCSselectedInstancePath.ToLower().Contains("openbeta"))
                    {
                        path += openBetaExtention;
                    }
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                }
            }
            return null;
        }
        public static string[] GetDCSUserFolders()
        {
            
            KnownFolder sg = KnownFolder.SavedGames;
            MiscGames.SaveGamesPath = KnownFolders.GetPath(sg);
            string[] dirs = Directory.GetDirectories(MiscGames.SaveGamesPath);
            List<string> candidates = new List<string>();
            for (int i = 0; i < dirs.Length; ++i)
            {
                string[] parts = dirs[i].Split('\\');
                string lastPart = parts[parts.Length - 1];
                if (lastPart.StartsWith("DCS")) candidates.Add(dirs[i]);
            }
            return candidates.ToArray();
        }

        public static void InitStarCitizen()
        {
            LoadStarCitizenPath();
            SCIOLogic.LoadKeyboardConversion();
        }

        public static void LoadStarCitizenPath()
        {
            string pth = MainStructure.SearchKey("CurrentUser", "System\\GameConfigStore\\Children", "MatchedExeFullPath", "StarCitizen.exe");
            if (pth != null)
            {
                string[] parts = pth.Split('\\');
                pth = parts[0];
                for(int i=1; i < parts.Length - 3; ++i)
                {
                    pth=pth+"\\"+parts[i];
                }
                MainStructure.Write("StarCitizen found in: " + pth);
                MiscGames.StarCitizen = pth;
            }
        }
        public static void InitIL2Data()
        {
            MainStructure.PROGPATH = Environment.CurrentDirectory;
            MainStructure.Write(MainStructure.PROGPATH);
            IL2IOLogic.LoadKeyboardConversion();
            IL2IOLogic.LoadIL2Path();
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            if (MainStructure.msave.IL2OR!=null&&MainStructure.msave.IL2OR.Length > 0)
            {
                MiscGames.IL2Instance = MainStructure.msave.IL2OR;
            }
            MainStructure.Write(MiscGames.IL2Instance);
        }
        public static void InitDCSData()
        {
            MainStructure.PROGPATH = Environment.CurrentDirectory;
            MainStructure.Write(MainStructure.PROGPATH);
            DCSIOLogic.LoadCleanLuasDCS();
            if(MainStructure.loadKeyboard)DCSIOLogic.LoadCleanLuasDCSKeyboard();
            DCSIOLogic.LoadLocalDefaultsDCS();
            DCSIOLogic.LoadKeyboardConversion();
            List<string> installs = new List<string>();
            string pth = MainStructure.GetRegistryValue("SOFTWARE\\Eagle Dynamics\\DCS World", "Path", "CurrentUser");
            if (pth != null) installs.Add(pth);
            pth = MainStructure.GetRegistryValue("SOFTWARE\\Eagle Dynamics\\DCS World OpenBeta", "Path", "CurrentUser");
            if (pth != null) installs.Add(pth);
            pth = MainStructure.GetRegistryValue("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 223750", "InstallLocation", "LocalMachine");
            if (pth != null) installs.Add(pth);
            for(int i =installs.Count - 1; i >= 0; i--)
            {
                if(!Directory.Exists(installs[i]))installs.RemoveAt(i);
            }
            MiscGames.installPathsDCS = installs.ToArray();
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            MainStructure.mainW.DropDownInstanceSelection.Items.Clear();
            if (MainStructure.msave.DCSInstaceOverride!=null && MainStructure.msave.DCSInstaceOverride.Length > 0)
            {
                MainStructure.mainW.DropDownInstanceSelection.Items.Add(MainStructure.msave.DCSInstaceOverride);
            }
            else
            {
                foreach (string inst in MiscGames.DCSInstances)
                    MainStructure.mainW.DropDownInstanceSelection.Items.Add(inst);
            }
            InitOverlayLookup();
        }
        public static void UnloadGameData()
        {
            DBLogic.DCSLib.Clear();
            MiscGames.installPathsDCS = new string[0];
            InternalDataManagement.LocalJoysticks = new string[0];
            DCSIOLogic.EmptyOutputsDCS = new Dictionary<string, DCSLuaInput>();
            DBLogic.OtherLib = new Dictionary<string, Dictionary<string, OtherGame>>();
            MiscGames.IL2Instance = "";

        }
        public static void ReloadGameData()
        {
            UnloadGameData();
            InitDCSData();
            InitIL2Data();
            InitStarCitizen();
            ReloadDatabase();
            try
            {
                Dictionary<string, string> connectedSticks = JoystickReader.GetConnectedJoysticks();
                foreach(KeyValuePair<string, string> pair in connectedSticks)
                {
                    InternalDataManagement.DevicesNeedingProfile.Add(pair.Key);
                }
                InternalDataManagement.DevicesNeedingProfile.Remove("Keyboard");
                List<string> crSticks = new List<string>();
                MiscGames.GetDCSInputJoysticks(crSticks);
                InternalDataManagement.LocalJoysticks = crSticks.ToArray();
                for(int i=0; i<crSticks.Count; i++)
                {
                    if (InternalDataManagement.DevicesNeedingProfile.Contains(crSticks[i]))
                    {
                        InternalDataManagement.DevicesNeedingProfile.Remove(crSticks[i]);
                    }
                }
                for (int i = 0; i < connectedSticks.Count; ++i)
                {
                    if (!crSticks.Contains(connectedSticks.ElementAt(i).Key))
                    {
                        crSticks.Add(connectedSticks.ElementAt(i).Key);
                    } 
                }
                InternalDataManagement.LocalJoysticks = crSticks.ToArray();
                foreach(KeyValuePair<string, string> kvp in connectedSticks)
                {
                    if(InternalDataManagement.LocalJoystickPGUID.ContainsKey(kvp.Key))
                        InternalDataManagement.LocalJoystickPGUID[kvp.Key] = kvp.Value;
                    else
                        InternalDataManagement.LocalJoystickPGUID.Add(kvp.Key, kvp.Value);
                }
            }
            catch(Exception ex)
            {
                MainStructure.NoteError(ex);
            }
            if (!MainStructure.mainW.STARTED&&InternalDataManagement.DevicesNeedingProfile.Count>0)
            {
                CheckIfDevicesNeeded();
            }
        }

        public static void CheckIfDevicesNeeded()
        {
            if (InternalDataManagement.DevicesNeedingProfile.Count > 0)
            {
                string deviceNameToDownload = InternalDataManagement.DevicesNeedingProfile[0].Substring(0, InternalDataManagement.DevicesNeedingProfile[0].IndexOf(" {"));
                JoystickProfileDownloader.stick = deviceNameToDownload;
                JoystickProfileDownloader.stickOg = InternalDataManagement.DevicesNeedingProfile[0];
                InternalDataManagement.DevicesNeedingProfile.RemoveAt(0);
                if (JoystickProfileDownloader.DoesFileExistinProfiles())
                {
                    System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("JoyPro saw you havent bound anything to " + deviceNameToDownload + " .Do you want to download a premade profile for this device?", "Download Profile for Device", System.Windows.Forms.MessageBoxButtons.YesNo);
                    if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        JoystickProfileDownloader.DownloadJoystickProfile();
                    }
                }
                
            }
        }

        public static void ReloadDatabase()
        {
            MainStructure.Write("Clear Databaste");
            DBLogic.OtherLib.Clear();
            DBLogic.DCSLib.Clear();
            MainStructure.Write("Populate DCS DB");
            DBLogic.PopulateDCSDictionaryWithProgram();
            MainStructure.Write("Populate IL2 DB");
            DBLogic.PopulateIL2Dictionary();
            MainStructure.Write("Populate SC DB");
            DBLogic.PopulateSCDictionary();
            MainStructure.Write("Update SC DB");
            DBLogic.UpdateSCDictionary();
            MainStructure.Write("Populate Manual DB");
            DBLogic.PopulateManualDictionary();
        }

    }
}
