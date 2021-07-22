using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using SlimDX.DirectInput;
using System.Windows;
using System.Net;
using Microsoft.Win32;
using System.Net.Http;
using System.Web.Hosting;
using System.Diagnostics;

namespace JoyPro
{

    //IL2 steam path registry 
    //Computer\HKEY_CURRENT_USER\System\GameConfigStore\Children\92e042fb-d93a-46df-8872-04039ab6d802
    //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 307960
    public enum Game { DCS, StarCitizen }
    public enum JoystickAxis { JOY_X, JOY_Y, JOY_Z, JOY_RX, JOY_RY, JOY_RZ, JOY_SLIDER1, JOY_SLIDER2, NONE }
    public enum LuaDataType { String, Number, Dict, Bool, Error };

    public enum SortType { NAME_NORM, NAME_DESC, STICK_NORM, STICK_DESC, BTN_NORM, BTN_DESC }

    public enum ModExists { NOT_EXISTENT, BINDNAME_EXISTS, KEYBIND_EXISTS, ALL_EXISTS, ERROR }
    public static class MainStructure
    {
        const string runningBackupFolder = "\\Config\\JP_Backup";
        const string subInputPath = "\\Config\\Input";
        const string inputFolderName = "\\Input";
        const string initialBackupFolder = "\\Config\\JP_InitBackup";
        const string externalWebUrl = "https://raw.githubusercontent.com/Holdi601/JoystickProfiler/master/JoyPro/JoyPro/ver.txt";
        const string buildPath = "https://github.com/Holdi601/JoystickProfiler/raw/master/Builds/JoyPro_WinX64_v";
        const int version = 39;
        public static MainWindow mainW;
        public static string PROGPATH;
        public static Dictionary<string, DCSPlane> DCSLib = new Dictionary<string, DCSPlane>();
        public static Dictionary<string, OtherGame> OtherLib = new Dictionary<string, OtherGame>();
        public static string[] LocalJoysticks;
        public static string SaveGamesPath;
        public static string[] DCSInstances;
        public static string[] Planes;
        static Dictionary<string, Relation> AllRelations = new Dictionary<string, Relation>();
        static Dictionary<string, Bind> AllBinds = new Dictionary<string, Bind>();
        public static List<string> AllGroups = new List<string>();
        public static Dictionary<string, string> JoystickAliases = new Dictionary<string, string>();
        public static Dictionary<string, DCSLuaInput> EmptyOutputsDCS = new Dictionary<string, DCSLuaInput>();
        static Dictionary<string, DCSExportPlane> LocalBindsDCS = new Dictionary<string, DCSExportPlane>();
        static Dictionary<string, DCSExportPlane> ToExportDCS = new Dictionary<string, DCSExportPlane>();
        static List<string> defaultToOverwrite = new List<string>();
        public static MetaSave msave = null;
        public static string selectedInstancePath = "";
        static Dictionary<string, Modifier> AllModifiers = new Dictionary<string, Modifier>();
        public static string[] installPathsDCS;
        static string newestAvailableVersion;
        static int downloadFails = 0;
        public static string IL2Instance = "";
        public static Dictionary<string, int> IL2JoystickId = new Dictionary<string, int>();

        static event EventHandler DownloadCompletedEvent;

        public static void CorrectModifiersInBinds()
        {
            foreach(KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if(kvp.Value.AllReformers!=null)
                    for(int i= kvp.Value.AllReformers.Count-1; i>=0; --i)
                    {
                        string[] reformParts = kvp.Value.AllReformers[i].Split('§');
                        if (reformParts.Length > 2)
                        {
                            Modifier m = GetModifierWithKeyCombo(reformParts[1], reformParts[2]);
                            kvp.Value.AllReformers[i] = m.toReformerString();
                        }
                        else
                        {
                            kvp.Value.AllReformers.RemoveAt(i);
                        }
                    }
            }
            ResyncRelations();
        }
        public static void LoadIL2Path()
        {
            string pth = GetRegistryValue("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 307960", "InstallLocation", "LocalMachine");
            if (pth != null) IL2Instance = pth;
        }
        public static List<string> GetAllModsAsString()
        {
            List<string> result = new List<string>();
            foreach (KeyValuePair<string, Modifier> kvp in AllModifiers)
            {
                result.Add(kvp.Key);
            }
            result = result.OrderBy(o => o).ToList();
            return result;
        }
        public static Modifier GetModifierWithKeyCombo(string device, string key)
        {
            foreach (KeyValuePair<string, Modifier> kvp in AllModifiers)
                if (kvp.Value.device.ToUpper() == device.ToUpper() && kvp.Value.key.ToUpper() == key.ToUpper()) return kvp.Value;
            return null;
        }

        static void DownloadCompleted(object o, EventArgs e)
        {
            Console.WriteLine(PROGPATH);
            ProcessStartInfo startInfo = new ProcessStartInfo(PROGPATH + "\\TOOLS\\Unzipper\\UnzipMeHereWin.exe");
            startInfo.Arguments = "\"" + PROGPATH + "\\NewerVersion.zip\" \"" + PROGPATH + "\" \"" + PROGPATH + "\\JoyPro.exe\"";
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        static int GetNewestVersionNumber()
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

        public static void RemoveBind(Bind b)
        {
            foreach (KeyValuePair<string, Relation> kvp in AllRelations)
            {
                if (kvp.Value.bind == b)
                    kvp.Value.bind = null;
            }
            if (AllBinds.ContainsKey(b.Rl.NAME))
                AllBinds.Remove(b.Rl.NAME);
        }
        public static void ChangeReformerName(string oldName, string newName)
        {
            if (AllModifiers.ContainsKey(oldName))
            {
                Modifier m = AllModifiers[oldName];
                string oldRefKey = m.toReformerString();
                AllModifiers.Remove(oldName);
                m.name = newName;
                foreach (KeyValuePair<string, Bind> kvp in AllBinds)
                {
                    if (kvp.Value.AllReformers.Contains(oldRefKey))
                    {
                        kvp.Value.AllReformers.Remove(oldRefKey);
                        kvp.Value.AllReformers.Add(m.toReformerString());
                    }
                }
                AllModifiers.Add(m.name, m);
            }
        }

        public static void RemoveReformer(string name)
        {
            if (AllModifiers.ContainsKey(name))
            {
                Modifier m = AllModifiers[name];
                AllModifiers.Remove(name);
                foreach (KeyValuePair<string, Bind> kvp in AllBinds)
                {
                    if (kvp.Value.AllReformers.Contains(m.toReformerString()))
                        kvp.Value.AllReformers.Remove(m.toReformerString());
                }
            }
        }

        public static bool ModifiersContainKeyCombo(string device, string key)
        {
            bool found = false;
            foreach (KeyValuePair<string, Modifier> kvp in AllModifiers)
            {
                if (kvp.Value.device.ToUpper() == device.ToUpper() && kvp.Value.key.ToUpper() == key.ToUpper()) return true;
            }
            return found;
        }
        public static void LoadMetaLast()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            if (File.Exists(pth + "\\meta.info"))
            {
                try
                {
                    msave = ReadFromBinaryFile<MetaSave>(pth + "\\meta.info");
                }
                catch
                {
                    msave = new MetaSave();
                }
            }
            else
            {
                msave = new MetaSave();
            }
        }

        public static void AfterLoad()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            if (File.Exists(pth + "\\last.pr0file"))
            {
                LoadProfile(pth + "\\last.pr0file");
            }
        }

        public static WindowPos GetWindowPosFrom(Window w)
        {
            if (double.IsNaN(w.Height) ||
                double.IsNaN(w.Width) ||
                double.IsNaN(w.Top) ||
                double.IsNaN(w.Left))
                return null;
            WindowPos wp = new WindowPos();
            wp.Height = w.Height;
            wp.Width = w.Width;
            wp.Top = w.Top;
            wp.Left = w.Left;
            return wp;
        }
        public static void SaveMetaLast()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            if (!Directory.Exists(pth))
            {
                Directory.CreateDirectory(pth);
            }
            WriteToBinaryFile<MetaSave>(pth + "\\meta.info", msave);
            SaveProfileTo(pth + "\\last.pr0file");
        }

        static void WriteFilesDCS(string endingDCS = ".diff.lua")
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvp in ToExportDCS)
            {
                string modPath = selectedInstancePath + "\\Config\\Input\\" + kvp.Key;
                string adjustedPath = modPath + "\\joystick\\";
                if (!Directory.Exists(adjustedPath)) Directory.CreateDirectory(adjustedPath);
                kvp.Value.WriteModifiers(modPath);
                foreach (KeyValuePair<string, DCSLuaInput> kvJoy in kvp.Value.joystickConfig)
                {
                    string outputName = kvJoy.Key;
                    string[] partsName = outputName.Split('{');
                    if (partsName.Length > 1)
                    {
                        outputName = partsName[0] + '{';
                        string[] idParts = partsName[1].Split('-');
                        outputName += idParts[0].ToUpper();
                        for (int i = 1; i < idParts.Length; ++i)
                        {
                            if (i == 2)
                            {
                                outputName = outputName + "-" + idParts[i].ToLower();
                            }
                            else
                            {
                                outputName = outputName + "-" + idParts[i].ToUpper();
                            }
                        }

                    }
                    string finalPath = adjustedPath + outputName + endingDCS;
                    kvJoy.Value.writeLua(finalPath);
                }
            }
        }
        public static ModExists DoesReformerExistInMods(string reformer)
        {
            Modifier m = Modifier.ReformerToMod(reformer);
            if (m == null) return ModExists.ERROR;
            if (!AllModifiers.ContainsKey(m.name))
            {
                if (ModifiersContainKeyCombo(m.device, m.key))
                {
                    return ModExists.KEYBIND_EXISTS;
                }
                else
                {
                    return ModExists.NOT_EXISTENT;
                }
            }
            else
            {
                if (AllModifiers[m.name].device.ToUpper() == m.device.ToUpper() &&
                    AllModifiers[m.name].key.ToUpper() == m.key.ToUpper())
                {
                    return ModExists.ALL_EXISTS;
                }
                else
                {
                    return ModExists.BINDNAME_EXISTS;
                }
            }
        }

        public static void SaveWindowState(object sender, EventArgs e)
        {
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            if (sender != null && sender is Window)
            {
                WindowPos p = GetWindowPosFrom((Window)sender);
                if (sender is MainWindow)
                {
                    msave.mainWLast = p;
                }
                else if (sender is RelationWindow)
                {
                    msave.relationWindowLast = p;
                }
                else if (sender is ExchangeStick)
                {
                    msave.exchangeW = p;
                }
                else if (sender is ImportWindow)
                {
                    msave.importWindowLast = p;
                }
                else if (sender is ModifierManager)
                {
                    msave.ModifierW = p;
                }
                else if (sender is StickToExchange)
                {
                    msave.stick2ExW = p;
                }
                else if (sender is StickSettings)
                {
                    msave.SettingsW = p;
                }
                else if (sender is ValidationErrors)
                {
                    msave.ValidW = p;
                }
                else if (sender is ReinstateBackup)
                {
                    msave.BackupW = p;
                }
                else if (sender is UserCurveDCS)
                {
                    msave.UsrCvW = p;
                }
            }
            if (mainW.CBNukeUnused.IsChecked == true)
                msave.NukeSticks = true;
            else if (mainW.CBNukeUnused.IsChecked == false)
                msave.NukeSticks = false;
            SaveMetaLast();
        }

        public static string GetReformerStringFromMod(string name)
        {
            if (AllModifiers.ContainsKey(name)) return AllModifiers[name].toReformerString();
            return null;
        }
        public static void AddReformerToMods(string reformer)
        {
            Modifier m = Modifier.ReformerToMod(reformer);
            AllModifiers.Add(m.name, m);
        }

        public static Dictionary<string, Bind> LibraryFromLocalDict(Dictionary<string, DCSExportPlane> lib, List<string> sticks, bool loadDefaults, bool inv = false, bool slid = false, bool curv = false, bool dz = false, bool sx = false, bool sy = false)
        {
            Dictionary<string, Bind> result = new Dictionary<string, Bind>();
            Dictionary<string, Dictionary<string, List<string>>> checkedIds = new Dictionary<string, Dictionary<string, List<string>>>();
            foreach (KeyValuePair<string, DCSExportPlane> kvp in lib)
            {
                string plane = kvp.Key;
                if (!checkedIds.ContainsKey(plane)) checkedIds.Add(plane, new Dictionary<string, List<string>>());
                foreach (KeyValuePair<string, DCSLuaInput> kvpLua in kvp.Value.joystickConfig)
                {
                    string joystick = kvpLua.Key;
                    if (!sticks.Contains(joystick)) continue;
                    if (!checkedIds[plane].ContainsKey(joystick))
                        checkedIds[plane].Add(joystick, new List<string>());
                    foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvpaxe in kvpLua.Value.axisDiffs)
                    {
                        string k = kvpaxe.Key;
                        if (!checkedIds[plane][joystick].Contains(k))
                            checkedIds[plane][joystick].Add(k);
                        for (int i = 0; i < kvpaxe.Value.added.Count; ++i)
                        {
                            Bind b = Bind.GetBindFromAxisElement(kvpaxe.Value.added[i], kvpaxe.Key, joystick, plane, inv, slid, curv, dz, sx, sy);
                            if (!result.ContainsKey(b.Rl.NAME))
                            {
                                result.Add(b.Rl.NAME, b);
                            }
                            else
                            {
                                result[b.Rl.NAME].Rl.AddNodeDCS(kvpaxe.Key, plane);
                                if ((result[b.Rl.NAME].additionalImportInfo == null ||
                                   result[b.Rl.NAME].additionalImportInfo.Length < 1) &&
                                   (b.additionalImportInfo != null &&
                                   b.additionalImportInfo.Length > 0))
                                {
                                    result[b.Rl.NAME].additionalImportInfo = b.additionalImportInfo;
                                }
                            }
                        }
                    }
                    foreach (KeyValuePair<string, DCSLuaDiffsButtonElement> kvpbe in kvpLua.Value.keyDiffs)
                    {
                        string k = kvpbe.Key;
                        if (!checkedIds[plane][joystick].Contains(k))
                            checkedIds[plane][joystick].Add(k);
                        for (int i = 0; i < kvpbe.Value.added.Count; ++i)
                        {
                            Bind b = Bind.GetBindFromButtonElement(kvpbe.Value.added[i], kvpbe.Key, joystick, plane);
                            if (!result.ContainsKey(b.Rl.NAME))
                            {
                                result.Add(b.Rl.NAME, b);
                            }
                            else
                            {
                                result[b.Rl.NAME].Rl.AddNodeDCS(kvpbe.Key, plane);
                                if ((result[b.Rl.NAME].additionalImportInfo == null ||
                                   result[b.Rl.NAME].additionalImportInfo.Length < 1) &&
                                   (b.additionalImportInfo != null &&
                                   b.additionalImportInfo.Length > 0))
                                {
                                    result[b.Rl.NAME].additionalImportInfo = b.additionalImportInfo;
                                }
                            }
                        }
                    }
                }
            }
            if (loadDefaults)
            {
                foreach (KeyValuePair<string, DCSLuaInput> kvp in EmptyOutputsDCS)
                {
                    string planeToCheck = kvp.Key;
                    foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvpax in kvp.Value.axisDiffs)
                    {
                        string idToCheck = kvpax.Key;
                        bool found = false;
                        if (checkedIds.ContainsKey(planeToCheck))
                        {
                            foreach (KeyValuePair<string, List<string>> kiwi in checkedIds[planeToCheck])
                            {
                                string joystick = kiwi.Key;
                                found = kiwi.Value.Contains(idToCheck);
                                if (!found)
                                {
                                    if (kvpax.Value.removed.Count > 0)
                                    {
                                        Bind b = Bind.GetBindFromAxisElement(kvpax.Value.removed[0], idToCheck, joystick, planeToCheck, inv, slid, curv, dz, sx, sy);
                                        if (!result.ContainsKey(b.Rl.NAME))
                                        {
                                            result.Add(b.Rl.NAME, b);
                                        }
                                        else
                                        {
                                            result[b.Rl.NAME].Rl.AddNodeDCS(idToCheck, planeToCheck);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    foreach (KeyValuePair<string, DCSLuaDiffsButtonElement> kvpbn in kvp.Value.keyDiffs)
                    {
                        string idToCheck = kvpbn.Key;
                        bool found = false;
                        if (checkedIds.ContainsKey(planeToCheck))
                        {
                            foreach (KeyValuePair<string, List<string>> kiwi in checkedIds[planeToCheck])
                            {
                                string joystick = kiwi.Key;
                                found = kiwi.Value.Contains(idToCheck);
                                if (!found)
                                {
                                    if (kvpbn.Value.removed.Count > 0)
                                    {
                                        Bind b = Bind.GetBindFromButtonElement(kvpbn.Value.removed[0], idToCheck, joystick, planeToCheck);
                                        if (!result.ContainsKey(b.Rl.NAME))
                                        {
                                            result.Add(b.Rl.NAME, b);
                                        }
                                        else
                                        {
                                            result[b.Rl.NAME].Rl.AddNodeDCS(idToCheck, planeToCheck);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
            return result;
        }
        public static void BindsFromLocal(List<string> sticks, bool loadDefaults, bool inv = false, bool slid = false, bool curv = false, bool dz = false, bool sx = false, bool sy = false)
        {
            Dictionary<string, DCSExportPlane> checkAgainst = new Dictionary<string, DCSExportPlane>();
            LoadLocalBinds(selectedInstancePath, loadDefaults, ".jp", checkAgainst);
            LoadLocalBinds(selectedInstancePath, loadDefaults);
            Dictionary<string, Bind> checkRes = LibraryFromLocalDict(checkAgainst, sticks, loadDefaults, inv, slid, curv, dz, sx, sy);
            Dictionary<string, Bind> result = LibraryFromLocalDict(LocalBindsDCS, sticks, loadDefaults, inv, slid, curv, dz, sx, sy);
            foreach (KeyValuePair<string, Bind> kvp in checkRes)
            {
                CorrectBindNames(result, kvp.Value);
            }
            MergeImport(result);
            CorrectModifiersInBinds();
        }

        static void CorrectBindNames(Dictionary<string, Bind> lib, Bind b)
        {
            if (b == null || lib == null) return;
            foreach (KeyValuePair<string, Bind> kvp in lib)
            {
                if (b.additionalImportInfo != null && b.additionalImportInfo.Length > 0)
                {
                    if (b.Joystick == kvp.Value.Joystick &&
                        b.Inverted == kvp.Value.Inverted &&
                        b.Deadzone == kvp.Value.Deadzone &&
                        b.JAxis == kvp.Value.JAxis &&
                        b.JButton == kvp.Value.JButton &&
                        b.SaturationX == kvp.Value.SaturationX &&
                        b.SaturationY == kvp.Value.SaturationY &&
                        b.Slider == kvp.Value.Slider &&
                        b.Rl.NAME.ToUpper() == kvp.Value.Rl.NAME.ToUpper())
                    {
                        bool integrity = true;
                        if (b.AllReformers != null && kvp.Value.AllReformers != null)
                        {
                            for (int i = 0; i < b.AllReformers.Count; ++i)
                            {
                                if (b.AllReformers.Count != kvp.Value.AllReformers.Count || !kvp.Value.AllReformers.Contains(b.AllReformers[i]))
                                {
                                    integrity = false;
                                    break;
                                }
                            }
                        }
                        else if ((b.AllReformers != null && kvp.Value.AllReformers == null) || (b.AllReformers == null && kvp.Value.AllReformers != null))
                        {
                            integrity = false;
                        }
                        if (b.Curvature != null && kvp.Value.Curvature != null)
                        {
                            for (int i = 0; i < b.Curvature.Count; ++i)
                            {
                                if (b.Curvature.Count != kvp.Value.Curvature.Count || !kvp.Value.Curvature.Contains(b.Curvature[i]))
                                {
                                    integrity = false;
                                    break;
                                }
                            }
                        }
                        else if ((b.Curvature != null && kvp.Value.Curvature == null) || (b.Curvature == null && kvp.Value.Curvature != null))
                        {
                            integrity = false;
                        }
                        if (integrity)
                        {
                            kvp.Value.Rl.NAME = b.additionalImportInfo.Split('§')[b.additionalImportInfo.Split('§').Length - 1];
                            string[] modNames = b.additionalImportInfo.Split('§');
                            for (int i = 0; i < modNames.Length - 1; ++i)
                            {
                                if (kvp.Value.AllReformers.Count > i)
                                {
                                    string[] refParts = kvp.Value.AllReformers[i].Split('§');
                                    string replacement = modNames[i] + "§";
                                    if (refParts.Length > 2)
                                    {
                                        Modifier m = GetModifierWithKeyCombo(refParts[1], refParts[2]);
                                        string toRemove = m.name;
                                        m.JPN = modNames[i];
                                        m.name = modNames[i];
                                        AllModifiers.Remove(toRemove);
                                        if (!AllModifiers.ContainsKey(m.name))
                                            AllModifiers.Add(m.name, m);
                                        kvp.Value.AllReformers[i] = m.toReformerString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void MergeImport(Dictionary<string, Bind> res)
        {
            foreach (KeyValuePair<string, Bind> kvp in res)
            {
                string name = "";
                if (kvp.Value.additionalImportInfo.Length > 1)
                    name = kvp.Value.additionalImportInfo.Split('§')[kvp.Value.additionalImportInfo.Split('§').Length - 1];
                else
                    name = kvp.Value.Rl.NAME;
                while (AllRelations.ContainsKey(name))
                {
                    name += "i";
                }
                kvp.Value.Rl.NAME = name;
                AllRelations.Add(name, kvp.Value.Rl);
                AllBinds.Add(name, kvp.Value);
            }
            ResyncRelations();
        }
        public static void WriteProfileCleanNotOverwriteLocal(bool fillBeforeEmpty, List<string> games)
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            ToExportDCS.Clear();
            defaultToOverwrite = new List<string>();
            if (games != null && games.Contains("DCS"))
            {
                LoadLocalBinds(selectedInstancePath, true);
                OverwriteDCSExportWith(LocalBindsDCS, true, false, false);
                PushAllDCSBindsToExport(false, fillBeforeEmpty, false);
                WriteFilesDCS();
                WriteFilesDCS(".jp");
            }
            mainW.ShowMessageBox("Binds exported successfully ☺");
        }
        public static void WriteProfileCleanAndLoadedOverwritten(bool fillBeforeEmpty, List<string> games)
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            ToExportDCS.Clear();
            defaultToOverwrite = new List<string>();
            if (games != null && games.Contains("DCS"))
            {
                LoadLocalBinds(selectedInstancePath, true);
                OverwriteDCSExportWith(LocalBindsDCS, true, false, false);
                PushAllDCSBindsToExport(true, fillBeforeEmpty, false);
                WriteFilesDCS();
                WriteFilesDCS(".jp");
            }
            mainW.ShowMessageBox("Binds exported successfully ☻");
        }
        public static bool ListContainsCaseInsensitive(List<string> li, string toCheck)
        {
            for(int i=0; i<li.Count; ++i)
            {
                if (li[i].Replace(" ","").ToUpper() == toCheck.Replace(" ", "").ToUpper()) return true;
            }
            return false;
        }
        public static string ShortenDeviceName(string device)
        {
            if (!device.Contains("{")) return null;
            return device.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
        }
        public static void WriteProfileCleanAndLoadedOverwrittenAndAdd(bool fillBeforeEmpty, List<string> games)
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            ToExportDCS.Clear();
            defaultToOverwrite = new List<string>();
            if (games != null && games.Contains("DCS"))
            {
                LoadLocalBinds(selectedInstancePath, true);
                OverwriteDCSExportWith(LocalBindsDCS, true, false, false);
                PushAllDCSBindsToExport(true, fillBeforeEmpty, true);
                WriteFilesDCS();
                WriteFilesDCS(".jp");
            }
            mainW.ShowMessageBox("Binds exported successfully ☺");
        }
        public static void WriteProfileClean(bool nukeDevices, List<string> games)
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            if (games != null && games.Contains("DCS"))
            {
                ToExportDCS.Clear();
                PushAllDCSBindsToExport(true, true, false);
                if (nukeDevices)
                    NukeUnusedButConnectedDevicesToExport();
                WriteFilesDCS();
                WriteFilesDCS(".jp");
            }
            mainW.ShowMessageBox("Binds exported successfully ☻");
        }
        static void NukeUnusedButConnectedDevicesToExport()
        {
            string[] allPlanes = Planes;
            List<string> connectedSticks = JoystickReader.GetConnectedJoysticks();
            for (int i = 0; i < allPlanes.Length; ++i)
            {
                if (!ToExportDCS.ContainsKey(allPlanes[i]))
                {
                    ToExportDCS.Add(allPlanes[i], new DCSExportPlane());
                    ToExportDCS[allPlanes[i]].plane = allPlanes[i];
                }
                DCSLuaInput empty = null;
                if (EmptyOutputsDCS.ContainsKey(allPlanes[i]))
                {
                    empty = EmptyOutputsDCS[allPlanes[i]];
                }
                else
                    continue;
                for (int j = 0; j < connectedSticks.Count; j++)
                {
                    if (!ToExportDCS[allPlanes[i]].joystickConfig.ContainsKey(connectedSticks[j]))
                    {
                        ToExportDCS[allPlanes[i]].joystickConfig.Add(connectedSticks[j], empty.Copy());
                        ToExportDCS[allPlanes[i]].joystickConfig[connectedSticks[j]].plane = allPlanes[i];
                        ToExportDCS[allPlanes[i]].joystickConfig[connectedSticks[j]].JoystickName = connectedSticks[j];
                    }
                }
            }
        }
        public static void PushAllDCSBindsToExport(bool oride, bool fillBeforeEmpty = true, bool overwriteAdd = false)
        {
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick.Length > 0 &&
                    ((kvp.Value.Rl.ISAXIS && kvp.Value.JAxis.Length > 0) ||
                    (!kvp.Value.Rl.ISAXIS && kvp.Value.JButton.Length > 0)))
                    OverwriteDCSExportWith(BindToExportFormatDCS(kvp.Value), oride, fillBeforeEmpty, overwriteAdd);
            }
        }
        public static Dictionary<string, DCSExportPlane> BindToExportFormatDCS(Bind b)
        {
            Dictionary<string, int> pstate = b.Rl.GetPlaneSetState();
            Dictionary<string, DCSExportPlane> result = new Dictionary<string, DCSExportPlane>();
            foreach (KeyValuePair<string, int> kvpPS in pstate)
            {
                if (kvpPS.Value > 0)
                {
                    RelationItem ri = b.Rl.GetRelationItemForPlaneDCS(kvpPS.Key);
                    if (ri == null) continue;
                    if (!result.ContainsKey(kvpPS.Key)) result.Add(kvpPS.Key, new DCSExportPlane());
                    result[kvpPS.Key].plane = kvpPS.Key;
                    if (!result[kvpPS.Key].joystickConfig.ContainsKey(b.Joystick)) result[kvpPS.Key].joystickConfig.Add(b.Joystick, new DCSLuaInput());
                    result[kvpPS.Key].joystickConfig[b.Joystick].JoystickName = b.Joystick;
                    result[kvpPS.Key].joystickConfig[b.Joystick].plane = kvpPS.Key;
                    if (b.Rl.ISAXIS)
                    {
                        if (!result[kvpPS.Key].joystickConfig[b.Joystick].axisDiffs.ContainsKey(ri.ID)) result[kvpPS.Key].joystickConfig[b.Joystick].axisDiffs.Add(ri.ID, new DCSLuaDiffsAxisElement());
                        result[kvpPS.Key].joystickConfig[b.Joystick].axisDiffs[ri.ID].Keyname = ri.ID;
                        result[kvpPS.Key].joystickConfig[b.Joystick].axisDiffs[ri.ID].Title = ri.GetInputDescription(kvpPS.Key);
                        DCSAxisBind dab = b.toDCSAxisBind();
                        if (dab == null) continue;
                        result[kvpPS.Key].joystickConfig[b.Joystick].axisDiffs[ri.ID].added.Add(dab);
                    }
                    else
                    {
                        if (!result[kvpPS.Key].joystickConfig[b.Joystick].keyDiffs.ContainsKey(ri.ID)) result[kvpPS.Key].joystickConfig[b.Joystick].keyDiffs.Add(ri.ID, new DCSLuaDiffsButtonElement());
                        result[kvpPS.Key].joystickConfig[b.Joystick].keyDiffs[ri.ID].Keyname = ri.ID;
                        result[kvpPS.Key].joystickConfig[b.Joystick].keyDiffs[ri.ID].Title = ri.GetInputDescription(kvpPS.Key);
                        DCSButtonBind dab = b.toDCSButtonBind();
                        if (dab == null) continue;
                        for (int i = 0; i < dab.modifiers.Count; ++i)
                        {
                            if (!result[kvpPS.Key].modifiers.ContainsKey(dab.modifiers[i].name))
                                result[kvpPS.Key].modifiers.Add(dab.modifiers[i].name, dab.modifiers[i]);
                        }
                        result[kvpPS.Key].joystickConfig[b.Joystick].keyDiffs[ri.ID].added.Add(dab);
                    }
                }
            }
            return result;
        }
        public static void OverwriteDCSExportWith(Dictionary<string, DCSExportPlane> attr, bool overwrite = true, bool fillBeforeEmpty = true, bool overwriteAdd = false)
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvp in attr)
            {
                if ((!ToExportDCS.ContainsKey(kvp.Key) && !fillBeforeEmpty) || (!ToExportDCS.ContainsKey(kvp.Key) && fillBeforeEmpty && !EmptyOutputsDCS.ContainsKey(kvp.Key)))
                {
                    ToExportDCS.Add(kvp.Key, kvp.Value.Copy());
                }
                else
                {
                    if (!ToExportDCS.ContainsKey(kvp.Key) && fillBeforeEmpty)
                    {
                        ToExportDCS.Add(kvp.Key, new DCSExportPlane());
                        ToExportDCS[kvp.Key].plane = kvp.Key;
                        foreach (KeyValuePair<string, DCSLuaInput> kvpDef in kvp.Value.joystickConfig)
                        {
                            if (!ToExportDCS[kvp.Key].joystickConfig.ContainsKey(kvpDef.Key) && EmptyOutputsDCS.ContainsKey(kvp.Key))
                            {
                                ToExportDCS[kvp.Key].joystickConfig.Add(kvpDef.Key, EmptyOutputsDCS[kvp.Key].Copy());
                                ToExportDCS[kvp.Key].joystickConfig[kvpDef.Key].JoystickName = kvpDef.Key;
                                ToExportDCS[kvp.Key].joystickConfig[kvpDef.Key].plane = kvp.Key;
                                string toCheck = kvpDef.Key + "§" + kvp.Key;
                                if (!defaultToOverwrite.Contains(toCheck)) defaultToOverwrite.Add(toCheck);
                            }
                        }
                    }
                    foreach (KeyValuePair<string, Modifier> kMod in kvp.Value.modifiers)
                    {
                        if (!ToExportDCS[kvp.Key].modifiers.ContainsKey(kMod.Key))
                        {
                            ToExportDCS[kvp.Key].modifiers.Add(kMod.Key, kMod.Value);
                        }
                        else if (overwrite)
                        {
                            ToExportDCS[kvp.Key].modifiers[kMod.Key] = kMod.Value;
                        }
                    }
                    foreach (KeyValuePair<string, DCSLuaInput> kvpIn in kvp.Value.joystickConfig)
                    {
                        if (!ToExportDCS[kvp.Key].joystickConfig.ContainsKey(kvpIn.Key) && fillBeforeEmpty && EmptyOutputsDCS.ContainsKey(kvp.Key))
                        {
                            ToExportDCS[kvp.Key].joystickConfig.Add(kvpIn.Key, EmptyOutputsDCS[kvp.Key].Copy());
                            ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].JoystickName = kvpIn.Key;
                            ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].plane = kvp.Key;
                            string toCheck = kvpIn.Key + "§" + kvp.Key;
                            if (!defaultToOverwrite.Contains(toCheck)) defaultToOverwrite.Add(toCheck);
                        }
                        if (!ToExportDCS[kvp.Key].joystickConfig.ContainsKey(kvpIn.Key))
                        {
                            ToExportDCS[kvp.Key].joystickConfig.Add(kvpIn.Key, kvpIn.Value);
                        }
                        else
                        {
                            string current = kvpIn.Key + "§" + kvp.Key;
                            foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvpDiffsAxisElement in kvpIn.Value.axisDiffs)
                            {
                                if (!ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs.ContainsKey(kvpDiffsAxisElement.Key))
                                {
                                    ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs.Add(kvpDiffsAxisElement.Key, kvpDiffsAxisElement.Value.Copy());
                                }
                                else if (overwrite || defaultToOverwrite.Contains(current))
                                {
                                    DCSLuaDiffsAxisElement old = ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].Copy();
                                    ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key] = kvpDiffsAxisElement.Value.Copy();
                                    if (overwriteAdd)
                                    {
                                        for (int i = 0; i < old.added.Count; ++i)
                                        {
                                            if (!kvpDiffsAxisElement.Value.doesAddedContainKey(old.added[i].key) && !kvpDiffsAxisElement.Value.doesRemovedContainKey(old.added[i].key))
                                            {
                                                ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].added.Add(old.added[i].Copy());
                                            }
                                        }
                                    }
                                    for (int i = 0; i < old.removed.Count; ++i)
                                    {
                                        if (!kvpDiffsAxisElement.Value.doesAddedContainKey(old.removed[i].key) && !kvpDiffsAxisElement.Value.doesRemovedContainKey(old.removed[i].key))
                                        {
                                            ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].removed.Add(old.removed[i].Copy());
                                        }
                                    }
                                    if (fillBeforeEmpty)
                                    {
                                        if (EmptyOutputsDCS.ContainsKey(kvp.Key) && EmptyOutputsDCS[kvp.Key].axisDiffs.ContainsKey(kvpDiffsAxisElement.Key))
                                        {
                                            bool found = false;
                                            for (int r = 0; r < EmptyOutputsDCS[kvp.Key].axisDiffs[kvpDiffsAxisElement.Key].removed.Count; ++r)
                                            {
                                                for (int w = 0; w < kvpDiffsAxisElement.Value.added.Count; ++w)
                                                {
                                                    if (kvpDiffsAxisElement.Value.added[w].key == EmptyOutputsDCS[kvp.Key].axisDiffs[kvpDiffsAxisElement.Key].removed[r].key)
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    if (found)
                                                        break;
                                                }
                                            }
                                            if (!found)
                                            {
                                                for (int r = 0; r < EmptyOutputsDCS[kvp.Key].axisDiffs[kvpDiffsAxisElement.Key].removed.Count; ++r)
                                                {
                                                    ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].removed.Add(EmptyOutputsDCS[kvp.Key].axisDiffs[kvpDiffsAxisElement.Key].removed[r]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (KeyValuePair<string, DCSLuaDiffsButtonElement> kvpDiffsButtonsElement in kvpIn.Value.keyDiffs)
                            {
                                if (!ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs.ContainsKey(kvpDiffsButtonsElement.Key))
                                {
                                    ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs.Add(kvpDiffsButtonsElement.Key, kvpDiffsButtonsElement.Value.Copy());
                                }
                                else if (overwrite || defaultToOverwrite.Contains(current))
                                {
                                    DCSLuaDiffsButtonElement old = ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].Copy();
                                    ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key] = kvpDiffsButtonsElement.Value.Copy();
                                    if (overwriteAdd)
                                    {
                                        for (int i = 0; i < old.added.Count; ++i)
                                        {
                                            if (!kvpDiffsButtonsElement.Value.doesAddedContainKey(old.added[i].key, old.added[i].reformers) && !kvpDiffsButtonsElement.Value.doesRemovedContainKey(old.added[i].key))
                                            {
                                                ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].added.Add(old.added[i].Copy());
                                            }
                                        }
                                    }
                                    for (int i = 0; i < old.removed.Count; ++i)
                                    {
                                        if (!kvpDiffsButtonsElement.Value.doesAddedContainKey(old.removed[i].key, old.removed[i].reformers) && !kvpDiffsButtonsElement.Value.doesRemovedContainKey(old.removed[i].key))
                                        {
                                            ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Add(old.removed[i].Copy());
                                        }
                                    }
                                    if (fillBeforeEmpty)
                                    {
                                        if (EmptyOutputsDCS.ContainsKey(kvp.Key) && EmptyOutputsDCS[kvp.Key].keyDiffs.ContainsKey(kvpDiffsButtonsElement.Key))
                                        {
                                            bool found = false;
                                            for (int r = 0; r < EmptyOutputsDCS[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Count; ++r)
                                            {
                                                for (int w = 0; w < kvpDiffsButtonsElement.Value.added.Count; ++w)
                                                {
                                                    if (kvpDiffsButtonsElement.Value.added[w].key == EmptyOutputsDCS[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed[r].key)
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    if (found)
                                                        break;
                                                }
                                            }
                                            if (!found)
                                            {
                                                for (int r = 0; r < EmptyOutputsDCS[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Count; ++r)
                                                {
                                                    ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Add(EmptyOutputsDCS[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed[r]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            CorrectExportForAddedRemoved();
        }

        static void CorrectExportForAddedRemoved()
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvpExpPlane in ToExportDCS)
            {
                foreach (KeyValuePair<string, DCSLuaInput> kvpJoyConf in kvpExpPlane.Value.joystickConfig)
                {
                    foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvpAxEl in kvpJoyConf.Value.axisDiffs)
                    {
                        if (kvpAxEl.Value.added.Count > 1)
                        {
                            for (int i = kvpAxEl.Value.added.Count - 1; i >= 0; i--)
                            {
                                bool foundToRemove = false;
                                for (int j = 0; j < kvpAxEl.Value.removed.Count; j++)
                                {
                                    if (kvpAxEl.Value.removed[j].key == kvpAxEl.Value.added[i].key)
                                    {
                                        foundToRemove = true;
                                        break;
                                    }
                                }
                                if (foundToRemove)
                                    kvpAxEl.Value.added.RemoveAt(i);
                            }
                        }
                    }
                    foreach (KeyValuePair<string, DCSLuaDiffsButtonElement> kvpBnEl in kvpJoyConf.Value.keyDiffs)
                    {
                        if (kvpBnEl.Value.added.Count > 1)
                        {
                            for (int i = kvpBnEl.Value.added.Count - 1; i >= 0; i--)
                            {
                                bool foundToRemove = false;
                                for (int j = 0; j < kvpBnEl.Value.removed.Count; j++)
                                {
                                    if (kvpBnEl.Value.removed[j].key == kvpBnEl.Value.added[i].key)
                                    {
                                        foundToRemove = true;
                                        break;
                                    }
                                }
                                if (EmptyOutputsDCS.ContainsKey(kvpExpPlane.Key))
                                {

                                }
                                if (foundToRemove)
                                    kvpBnEl.Value.added.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }

        public static string GetContentBetweenSymbols(string content, string openingSymbol, string closingSymbol = "")
        {
            if (content.Length < 1) return null;
            if (closingSymbol.Length < 1) closingSymbol = openingSymbol;
            if (openingSymbol.Length < 1) return null;
            string result = "";
            int srtindx = content.IndexOf(openingSymbol) + openingSymbol.Length;
            if (srtindx < 0) return null;
            if (!content.Contains(openingSymbol)) return null;
            if (openingSymbol == closingSymbol)
            {
                int closer = content.IndexOf(openingSymbol, srtindx);
                if (closer > 0)
                {
                    result = content.Substring(srtindx, closer - srtindx);
                }
                else
                {
                    result = content.Substring(srtindx);
                }
            }
            else
            {
                int level = 1;
                int initialopener = srtindx;
                int newOpener = srtindx;
                int closer = -1;
                while (level > 0)
                {
                    closer = content.IndexOf(closingSymbol, newOpener);
                    newOpener = content.IndexOf(openingSymbol, newOpener);
                    if (newOpener < closer && newOpener >= 0)
                    {
                        level++;
                        newOpener += openingSymbol.Length;
                    }
                    else
                    {
                        level -= 1;
                        newOpener = closer + closingSymbol.Length;
                    }
                    if (level > 1000000) { break; }
                }
                if ((newOpener - closingSymbol.Length - initialopener) >= 0)
                {
                    result = content.Substring(initialopener, newOpener - closingSymbol.Length - initialopener);
                }
                else
                {
                    result = content.Substring(initialopener);
                }
            }
            return result;
        }
        public static Dictionary<object, object> CreateAttributeDictFromLua(string cont)
        {
            Dictionary<object, object> result = new Dictionary<object, object>();
            if (cont.Length < 1) return null;
            string ltrim = cont.TrimStart();
            object key = null;
            int indxOfBracked = ltrim.IndexOf("[");
            string dtToCheck = ltrim.Substring(indxOfBracked + 1);
            LuaDataType ldtKey = DefineFirstDataTypeInString(dtToCheck);
            if (ldtKey == LuaDataType.String)
            {
                key = GetContentBetweenSymbols(ltrim, "\"");
            }
            else if (ldtKey == LuaDataType.Number)
            {
                key = Convert.ToInt32(GetContentBetweenSymbols(ltrim, "[", "]"));
            }
            else if (ldtKey == LuaDataType.Bool)
            {
                key = Convert.ToBoolean(GetContentBetweenSymbols(ltrim, "[", "]"));
            }
            while (key != null &&
                ((ldtKey == LuaDataType.String && ((string)key).Length > 0) ||
                (ldtKey == LuaDataType.Number && ((int)key) > -1)))
            {
                if (ldtKey == LuaDataType.String)
                {
                    int indexToStart = ltrim.IndexOf("\"" + (string)key + "\"");
                    ltrim = ltrim.Substring(indexToStart + ("\"" + (string)key + "\"").Length);
                }
                int equationInddex = ltrim.IndexOf("=");
                ltrim = ltrim.Substring(equationInddex + 1);
                LuaDataType ldtValue = DefineFirstDataTypeInString(ltrim);
                object val;
                int indxAfter = -1;
                switch (ldtValue)
                {
                    case LuaDataType.Dict:
                        string valRaw = GetContentBetweenSymbols(ltrim, "{", "}");
                        val = CreateAttributeDictFromLua(valRaw);
                        result.Add(key, val);
                        int ind = ltrim.IndexOf("{" + valRaw + "}");
                        indxAfter = ind + ("{" + valRaw + "}").Length;
                        break;
                    case LuaDataType.Number:
                        indxAfter = ltrim.IndexOf(",") + 1;
                        val = Convert.ToDouble(ltrim.Substring(0, ltrim.IndexOf(",")), new CultureInfo("en-US"));
                        result.Add(key, val);
                        break;
                    case LuaDataType.String:
                        string valRw = GetContentBetweenSymbols(ltrim, "\"");
                        indxAfter = ltrim.IndexOf("\"" + valRw + "\"") + ("\"" + valRw + "\"").Length;
                        result.Add(key, valRw);
                        break;
                    case LuaDataType.Bool:
                        indxAfter = ltrim.IndexOf(",") + 1;
                        val = Convert.ToBoolean(ltrim.Substring(0, ltrim.IndexOf(",")));
                        result.Add(key, val);
                        break;
                    case LuaDataType.Error:
                        indxAfter = ltrim.IndexOf(",") + 1;
                        break;
                }
                ltrim = ltrim.Substring(indxAfter);
                indxOfBracked = ltrim.IndexOf("[");
                if (indxOfBracked < 0) break;
                dtToCheck = ltrim.Substring(indxOfBracked + 1);
                ldtKey = DefineFirstDataTypeInString(dtToCheck);
                if (ldtKey == LuaDataType.String)
                {
                    key = GetContentBetweenSymbols(ltrim, "\"");
                }
                else if (ldtKey == LuaDataType.Number)
                {
                    key = Convert.ToInt32(GetContentBetweenSymbols(ltrim, "[", "]"));
                }
                else if (ldtKey == LuaDataType.Bool)
                {
                    key = Convert.ToBoolean(GetContentBetweenSymbols(ltrim, "[", "]"));
                }
            }
            return result;
        }


        public static LuaDataType DefineFirstDataTypeInString(string cont)
        {
            if (cont.Length < 1) return LuaDataType.Error;
            int indxQuotas = cont.IndexOf("\"");
            int indxCurlyBrackets = cont.IndexOf("{");
            int indxBool = cont.IndexOf("true");
            if ((cont.IndexOf("false") > -1 && cont.IndexOf("false") < indxBool) || indxBool < 0)
                indxBool = cont.IndexOf("false");
            int indxNumber = int.MaxValue;
            for (int i = -1; i < 10; ++i)
            {
                int tempIndex = cont.IndexOf(i.ToString().Substring(0, 1));
                if (tempIndex > -1 && tempIndex < indxNumber)
                    indxNumber = tempIndex;
            }
            if (indxNumber == int.MaxValue) indxNumber = -1;
            if (IsFirstValueLowestButNotNegative(indxQuotas, indxCurlyBrackets, indxBool, indxNumber)) return LuaDataType.String;
            if (IsFirstValueLowestButNotNegative(indxCurlyBrackets, indxQuotas, indxBool, indxNumber)) return LuaDataType.Dict;
            if (IsFirstValueLowestButNotNegative(indxNumber, indxQuotas, indxCurlyBrackets, indxBool)) return LuaDataType.Number;
            if (IsFirstValueLowestButNotNegative(indxBool, indxQuotas, indxCurlyBrackets, indxNumber)) return LuaDataType.Bool;
            return LuaDataType.Error;
        }
        static bool IsFirstValueLowestButNotNegative(int val1, int val2, int val3, int val4)
        {
            if (val1 < 0) return false;
            List<int> toCheck = new List<int>();
            toCheck.Add(val1);
            if (val2 > -1) toCheck.Add(val2);
            if (val3 > -1) toCheck.Add(val3);
            if (val4 > -1) toCheck.Add(val4);
            for (int i = 1; i < toCheck.Count; ++i)
                if (toCheck[0] > toCheck[i]) return false;
            return true;
        }
        public static void LoadLocalDefaultsDCS()
        {
            string install = GetDCSInstallationPath();
            string further = "\\Input";
            string modPaths = "\\Mods\\aircraft";
            if (install != null)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(install + modPaths);
                DirectoryInfo[] allMods = dirInfo.GetDirectories();
                for (int i = 0; i < allMods.Length; ++i)
                {
                    if (!Directory.Exists(allMods[i].FullName + further)) continue;
                    DirectoryInfo[] innerPlaneCollection = (new DirectoryInfo(allMods[i].FullName + further)).GetDirectories();
                    for (int j = 0; j < innerPlaneCollection.Length; ++j)
                    {
                        string planeName = innerPlaneCollection[j].Name;
                        if (!EmptyOutputsDCS.ContainsKey(planeName))
                        {
                            EmptyOutputsDCS.Add(planeName, new DCSLuaInput());
                            EmptyOutputsDCS[planeName].plane = planeName;
                            EmptyOutputsDCS[planeName].JoystickName = "EMPTY";
                        }
                        FileInfo[] files = innerPlaneCollection[j].GetFiles();
                        for (int k = 0; k < files.Length; k++)
                        {
                            if (files[k].Name.EndsWith(".diff.lua"))
                            {
                                StreamReader sr = new StreamReader(files[k].FullName);
                                string content = sr.ReadToEnd();
                                EmptyOutputsDCS[planeName].AdditionalAnalyzationRawLuaInvert(content);
                                sr.Close();
                            }
                        }
                    }
                }
                if (Directory.Exists(selectedInstancePath + modPaths))
                {
                    DirectoryInfo dirInstance = new DirectoryInfo(selectedInstancePath + modPaths);
                    DirectoryInfo[] allPlanes = dirInstance.GetDirectories();
                    string stickJoy = "\\Joystick";
                    for (int i = 0; i < allPlanes.Length; ++i)
                    {
                        string planeName = allPlanes[i].Name;
                        if (!EmptyOutputsDCS.ContainsKey(planeName))
                        {
                            EmptyOutputsDCS.Add(planeName, new DCSLuaInput());
                            EmptyOutputsDCS[planeName].plane = planeName;
                            EmptyOutputsDCS[planeName].JoystickName = "EMPTY";
                        }
                        if (Directory.Exists(allPlanes[i].FullName + further + stickJoy))
                        {
                            DirectoryInfo dirBins = new DirectoryInfo(allPlanes[i].FullName + further + stickJoy);
                            FileInfo[] files = dirBins.GetFiles();
                            for (int j = 0; j < files.Length; ++j)
                            {
                                if (files[j].Name.EndsWith(".diff.lua"))
                                {
                                    StreamReader sr = new StreamReader(files[j].FullName);
                                    string content = sr.ReadToEnd();
                                    EmptyOutputsDCS[planeName].AdditionalAnalyzationRawLuaInvert(content);
                                    sr.Close();
                                }
                            }
                        }
                    }
                }
            }

        }
        public static void LoadCleanLuasDCS()
        {
            StreamReader sr = new StreamReader(PROGPATH + "\\CleanProfile\\DCS\\clean.cf");
            DCSLuaInput curPlane = null;
            string content = sr.ReadToEnd();
            string sep = "####################";
            string rtn = GetContentBetweenSymbols(content, sep);
            char nl = '\n';
            while (rtn != null && rtn.Length > 0)
            {
                string plane = rtn.Split(nl)[0];
                content = content.Replace(sep + rtn, "");
                plane = plane.Replace("\r", "");
                curPlane = new DCSLuaInput();
                EmptyOutputsDCS.Add(plane, curPlane);
                curPlane.plane = plane;
                curPlane.JoystickName = "EMPTY";
                curPlane.AnalyzeRawLuaInput(rtn);
                rtn = GetContentBetweenSymbols(content, sep);
            }
            sr.Close();
            Console.WriteLine("Clean Data loaded");
        }
        public static void LoadLocalBinds(string localPath, bool fillWithDefaults = false, string ending = ".diff.lua", Dictionary<string, DCSExportPlane> resultsDict = null)
        {
            Dictionary<string, DCSExportPlane> toOutput;
            if (resultsDict == null)
            {
                toOutput = LocalBindsDCS;
            }
            else
            {
                toOutput = resultsDict;
            }
            toOutput.Clear();
            string pathToSearch = localPath + "\\Config\\Input";
            if (Directory.Exists(pathToSearch))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(pathToSearch);
                DirectoryInfo[] allSubs = dirInfo.GetDirectories();
                for (int i = 0; i < allSubs.Length; ++i)
                {
                    string currentPlane = allSubs[i].Name;
                    DCSExportPlane current = new DCSExportPlane();
                    current.plane = currentPlane;
                    toOutput.Add(currentPlane, current);
                    //Here load local modifiers lua
                    if (File.Exists(allSubs[i].FullName + "\\modifiers.lua"))
                    {
                        StreamReader srmod = new StreamReader(allSubs[i].FullName + "\\modifiers.lua");
                        string modContentRaw = srmod.ReadToEnd();
                        srmod.Close();
                        current.AnalyzeRawModLua(modContentRaw);
                    }
                    if (Directory.Exists(allSubs[i].FullName + "\\joystick"))
                    {
                        DirectoryInfo dirPlaneJoy = new DirectoryInfo(allSubs[i].FullName + "\\joystick");
                        FileInfo[] allFiles = dirPlaneJoy.GetFiles();
                        for (int j = 0; j < allFiles.Length; ++j)
                        {
                            if (allFiles[j].Name.Contains(ending))
                            {
                                string stickName = allFiles[j].Name.Replace(ending, "");
                                DCSLuaInput luaInput = new DCSLuaInput();
                                luaInput.plane = currentPlane;
                                luaInput.JoystickName = stickName;
                                current.joystickConfig.Add(stickName, luaInput);
                                StreamReader sr = new StreamReader(allFiles[j].FullName);
                                string fileContent = sr.ReadToEnd();
                                sr.Close();
                                luaInput.AnalyzeRawLuaInput(fileContent, current);
                            }
                        }
                    }
                }
                if (fillWithDefaults)
                {
                    foreach (KeyValuePair<string, DCSExportPlane> kvp in toOutput)
                    {
                        foreach (KeyValuePair<string, DCSLuaInput> kiwi in kvp.Value.joystickConfig)
                        {
                            kiwi.Value.FillUpWithDefaults();
                        }
                    }
                }
            }
            Console.WriteLine("Locals loaded lol");
        }
        public static void AddRelation(Relation r)
        {
            if (!AllRelations.ContainsKey(r.NAME))
            {
                AllRelations.Add(r.NAME, r);
            }
            ResyncRelations();
        }
        public static void RemoveRelation(Relation r)
        {
            AllRelations.Remove(r.NAME);
            ResyncRelations();
        }

        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            if (filePath == null || filePath.Length < 1) return;
            try
            {
                using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
                {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    binaryFormatter.Serialize(stream, objectToWrite);
                }
            }
            catch
            {

            }
        }
        public static void LoadRelations(string filePath)
        {
            if (filePath == null || filePath.Length < 1) return;
            NewFile();
            try
            {
                AllRelations = ReadFromBinaryFile<Dictionary<string, Relation>>(filePath);
                foreach (KeyValuePair<string, Relation> kvp in AllRelations)
                {
                    kvp.Value.CheckNamesAgainstDB();
                }
            }
            catch
            {
                MessageBox.Show("Couldn't load relations");
            }
            ResyncRelations();
        }
        public static void NewFile()
        {
            AllBinds.Clear();
            AllRelations.Clear();
            AllModifiers.Clear();
            AllGroups.Clear();
            ResyncRelations();
        }

        public static void ResyncBindsToMods()
        {
            AllModifiers.Clear();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                for (int i = 0; i < kvp.Value.AllReformers.Count; ++i)
                {
                    Modifier m = Modifier.ReformerToMod(kvp.Value.AllReformers[i]);
                    if (!AllModifiers.ContainsKey(m.name))
                    {
                        AllModifiers.Add(m.name, m);
                    }
                    else
                    {
                        string counterCase = AllModifiers[m.name].toReformerString();
                        kvp.Value.AllReformers[i] = counterCase;
                    }
                }
            }
        }
        public static Modifier ModifierByName(string name)
        {
            if (AllModifiers.ContainsKey(name)) return AllModifiers[name];
            return null;
        }

        public static void InitProgram()
        {
            if (GetNewestVersionNumber() > version)
            {
                MessageBoxResult mr = MessageBox.Show("A newer version is available, if you press yes, it will download in the background (Dont close the program please), it will close itself once its done.", "Newer Version Available", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mr == MessageBoxResult.Yes)
                {
                    DownloadNewerVersion();
                }
            }
            DCSInstances = GetDCSUserFolders();
            for (int i = 0; i < DCSInstances.Length; ++i)
            {
                BackupConfigsOfInstance(DCSInstances[i]);
            }
        }
       



        public static Task DownloadAsync(string requestUri, string filename)
        {
            return DownloadAsync(new Uri(requestUri), filename);
        }

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

        static void DownloadNewerVersion()
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

        public static List<string> GamesInRelations()
        {
            List<string> result = new List<string>();
            foreach (KeyValuePair<string, Relation> kvp in AllRelations)
            {
                List<string> inRes = kvp.Value.GamesInRelation();
                for (int i = 0; i < inRes.Count; ++i)
                {
                    if (!result.Contains(inRes[i]))
                    {
                        result.Add(inRes[i]);
                    }
                }
            }
            return result;
        }

        public static void DCSInstanceSelectionChanged(string newInstance)
        {
            if (selectedInstancePath == newInstance) return;
            selectedInstancePath = newInstance;
            LocalJoysticks = null;
            InitDCSJoysticks();
            if (msave != null)
            {
                msave.lastInstanceSelected = selectedInstancePath;
            }
            PopulateDCSDictionaryWithLocal(selectedInstancePath);
            int indx = -1;
            for(int i=0; i<DCSInstances.Length; ++i)
            {
                if (DCSInstances[i].ToLower() == newInstance.ToLower())
                {
                    indx = i;
                    break;
                }
            }
            if(indx>-1)
                mainW.DropDownInstanceSelection.SelectedIndex = indx;
        }
        static void RecreateJoystickAliases()
        {
            foreach(KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.aliasJoystick != null && kvp.Value.aliasJoystick.Length > 1)
                {
                    if (!JoystickAliases.ContainsKey(kvp.Value.Joystick))
                    {
                        JoystickAliases.Add(kvp.Value.Joystick, kvp.Value.aliasJoystick);
                    }
                }
            }
        }

        public static void RemoveDeviceAlias(string device)
        {
            if (JoystickAliases.ContainsKey(device))
                JoystickAliases.Remove(device);
            foreach(KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick.ToLower() == device.ToLower())
                {
                    kvp.Value.aliasJoystick = "";
                }
            }
        }

        public static void AddJoystickAlias(string device, string alias)
        {
            if (JoystickAliases.ContainsKey(device))
            {
                RemoveDeviceAlias(device);
            }
            JoystickAliases.Add(device, alias);
            foreach(KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick.ToLower() == device.ToLower())
                {
                    kvp.Value.aliasJoystick = alias;
                }
            }
        }

        static void RecreateGroups()
        {
            foreach(KeyValuePair<string, Relation> kvp in AllRelations)
            {
                if (kvp.Value.Groups != null)
                {
                    for(int i=0; i<kvp.Value.Groups.Count; ++i)
                    {
                        if (!ListContainsCaseInsensitive(AllGroups, kvp.Value.Groups[i]))
                            AllGroups.Add(kvp.Value.Groups[i]);
                    }
                }
            }
        }
        public static void LoadProfile(string filePath)
        {
            if (filePath == null || filePath.Length < 1) return;
            Pr0file pr = null;
            try
            {
                pr = ReadFromBinaryFile<Pr0file>(filePath);
                NewFile();
                AllRelations = pr.Relations;
                AllBinds = pr.Binds;
                if (pr.LastSelectedDCSInstance != null && Directory.Exists(pr.LastSelectedDCSInstance))
                {
                    DCSInstanceSelectionChanged(pr.LastSelectedDCSInstance);
                }
                ResyncBindsToMods();
                RecreateGroups();
                RecreateJoystickAliases();
                foreach (KeyValuePair<string, Relation> kvp in AllRelations)
                {
                    kvp.Value.CheckNamesAgainstDB();
                }
                AddLoadedJoysticks();
                CheckConnectedSticksToBinds();
            }
            catch
            {
                MessageBox.Show("Couldn't load profile. Either opened by some program or other error");
            }


            ResyncRelations();
        }
        public static void AddBind(string name, Bind b)
        {
            if (!AllBinds.ContainsKey(name)) AllBinds.Add(name, b);
        }
        public static void DeleteBind(string name)
        {
            if (AllBinds.ContainsKey(name)) AllBinds.Remove(name);
        }
        public static void InsertRelations(string[] files)
        {
            foreach (string s in files)
            {
                if (s == null || s.Length < 1) continue;
                Dictionary<string, Relation> thisRel = ReadFromBinaryFile<Dictionary<string, Relation>>(s);
                foreach (KeyValuePair<string, Relation> kvp in thisRel)
                {
                    string newKey = kvp.Key;
                    while (AllRelations.ContainsKey(newKey))
                    {
                        bool? overwrite = mainW.RelationAlreadyExists(newKey);
                        if (overwrite == true)
                        {
                            AllRelations[newKey] = kvp.Value;
                            break;
                        }
                        else if (overwrite == false)
                        {
                            break;
                        }
                        else
                        {
                            newKey += "1";
                        }
                    }
                    if (!AllRelations.ContainsKey(newKey))
                    {
                        AllRelations.Add(newKey, kvp.Value);
                        kvp.Value.NAME = newKey;
                    }
                }
            }
            ResyncRelations();
        }
        static void AddLoadedJoysticks()
        {
            List<string> sticks = new List<string>();
            if (LocalJoysticks != null)
                for (int i = 0; i < LocalJoysticks.Length; ++i)
                {
                    sticks.Add(DCSStickNaming(LocalJoysticks[i]));
                }
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick != null && kvp.Value.Joystick.Length > 0 && !sticks.Contains<string>(DCSStickNaming(kvp.Value.Joystick)))
                {
                    sticks.Add(DCSStickNaming(kvp.Value.Joystick));
                }
            }
            LocalJoysticks = sticks.ToArray();

        }

        static string DCSStickNaming(string stName)
        {
            string outputName = stName;
            string[] partsName = outputName.Split('{');
            if (partsName.Length > 1)
            {
                outputName = partsName[0] + '{';
                string[] idParts = partsName[1].Split('-');
                outputName += idParts[0].ToUpper();
                for (int i = 1; i < idParts.Length; ++i)
                {
                    if (i == 2)
                    {
                        outputName = outputName + "-" + idParts[i].ToLower();
                    }
                    else
                    {
                        outputName = outputName + "-" + idParts[i].ToUpper();
                    }
                }
                return outputName;
            }
            return stName;

        }
        static void CheckConnectedSticksToBinds()
        {
            //Check if joystick is connected and ask for more context
            List<string> connectedSticks = JoystickReader.GetConnectedJoysticks();
            List<string> misMatches = new List<string>();
            List<Bind> toRemove = new List<Bind>();
            List<string> upperConn = new List<string>();
            for (int i = 0; i < connectedSticks.Count; ++i)
                if (!upperConn.Contains(connectedSticks[i].ToUpper()))
                    upperConn.Add(connectedSticks[i].ToUpper());
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick != null && kvp.Value.Joystick.Length > 0)
                    if (!upperConn.Contains(kvp.Value.Joystick.ToUpper()) && !misMatches.Contains(kvp.Value.Joystick))
                        misMatches.Add(kvp.Value.Joystick);
            }
            foreach (Bind b in toRemove) if (AllBinds.ContainsKey(b.Rl.NAME)) AllBinds.Remove(b.Rl.NAME);
            foreach (string mis in misMatches)
            {
                ExchangeStick es = new ExchangeStick(mis);
                es.Show();
            }
        }
        public static void ExchangeSticksInBind(string old, string newstr)
        {
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick != null && kvp.Value.Joystick.Length > 0 && kvp.Value.Joystick == old && newstr.Length > 0)
                {
                    kvp.Value.Joystick = newstr;
                }
                kvp.Value.replaceDeviceInReformers(old, newstr);
            }
            ResyncRelations();
        }
        public static void SaveRelationsTo(string filePath)
        {
            WriteToBinaryFile<Dictionary<string, Relation>>(filePath, AllRelations);
        }
        public static void SaveProfileTo(string filePath)
        {
            Pr0file pr = new Pr0file(AllRelations, AllBinds, selectedInstancePath);
            WriteToBinaryFile<Pr0file>(filePath, pr);
        }
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
        public static bool RelationIsTheSame(string Name, Relation r)
        {
            return AllRelations[Name] == r;
        }
        static List<Relation> SyncRelations()
        {
            List<Relation> li = new List<Relation>();
            foreach (KeyValuePair<string, Relation> kvp in AllRelations) li.Add(kvp.Value);
            return li;
        }
        public static Bind GetBindForRelation(string name)
        {
            if (AllBinds.ContainsKey(name)) return AllBinds[name];
            return null;
        }
        public static void ResyncRelations()
        {
            List<Relation> li = SyncRelations();
            AllRelations.Clear();
            List<Bind> albi = new List<Bind>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                albi.Add(kvp.Value);
                kvp.Value.CorrectJoystickName();
            }
            AllBinds.Clear();
            List<Relation> relWithBinds = new List<Relation>();
            for (int i = 0; i < li.Count; i++)
            {
                if (!AllRelations.ContainsKey(li[i].NAME)) AllRelations.Add(li[i].NAME, li[i]);
                for (int j = 0; j < albi.Count; ++j)
                {
                    if (albi[j].Rl == li[i])
                    {
                        AllBinds.Add(li[i].NAME, albi[j]);
                        li[i].bind = albi[j];
                        if (!relWithBinds.Contains(li[i]))
                            relWithBinds.Add(li[i]);
                    }
                }
            }
            List<Relation> RelWOBinds = new List<Relation>();
            string sortType = "NAME";
            string sortOrder = "NORM";
            if (mainW.selectedSort != null && mainW.selectedSort.Contains('_'))
            {
                sortType = mainW.selectedSort.Split('_')[0];
                sortOrder = mainW.selectedSort.Split('_')[1];
            }

            for (int i = 0; i < li.Count; ++i)
            {
                if (!RelWOBinds.Contains(li[i]) && !relWithBinds.Contains(li[i]))
                    RelWOBinds.Add(li[i]);
            }
            switch (sortType)
            {
                case "NAME":
                    li = li.OrderBy(o => o.NAME).ToList();
                    break;
                case "STICK":
                    relWithBinds = relWithBinds.OrderBy(o => o.NAME).ToList();
                    relWithBinds = relWithBinds.OrderBy(o => AllBinds[o.NAME].Joystick).ToList();
                    RelWOBinds = RelWOBinds.OrderBy(o => o.NAME).ToList();
                    for (int i = 0; i < RelWOBinds.Count; ++i)
                    {
                        relWithBinds.Add(RelWOBinds[i]);
                    }
                    li = relWithBinds;
                    break;
                case "BTN":
                    relWithBinds = relWithBinds.OrderBy(o => o.NAME).ToList();
                    relWithBinds = relWithBinds.OrderBy(o => AllBinds[o.NAME].Joystick).ToList();
                    relWithBinds = relWithBinds.OrderBy(o => (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "").Length < 3 ?
                        (("0" + (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")).Length < 3 ?
                            ("00" + (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")) :
                            "0" + (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")) :
                        (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")
                    ).ToList();
                    RelWOBinds = RelWOBinds.OrderBy(o => o.NAME).ToList();
                    for (int i = 0; i < RelWOBinds.Count; ++i)
                    {
                        relWithBinds.Add(RelWOBinds[i]);
                    }
                    li = relWithBinds;
                    break;
            }
            if (sortOrder == "DESC")
            {
                List<Relation> fi = new List<Relation>();
                for (int i = li.Count - 1; i >= 0; i--)
                {
                    fi.Add(li[i]);
                }
                li = fi;
            }
            SaveMetaLast();
            mainW.SetRelationsToView(li);
        }

        public static List<Modifier> GetAllModifiers()
        {
            List<Modifier> res = new List<Modifier>();
            foreach (KeyValuePair<string, Modifier> kvp in AllModifiers)
            {
                res.Add(kvp.Value);
            }
            res = res.OrderBy(o => o.name).ToList();
            return res;
        }
        public static bool DoesRelationAlreadyExist(string name)
        {
            return AllRelations.ContainsKey(name);
        }
        public static void LoadDcsData()
        {
            PopulateDCSDictionaryWithProgram();
        }
        public static List<SearchQueryResults> SearchBinds(string[] keywords)
        {
            List<SearchQueryResults> results = new List<SearchQueryResults>();
            foreach (KeyValuePair<string, DCSPlane> kvp in DCSLib)
            {
                foreach (KeyValuePair<string, DCSInput> inp in kvp.Value.Axis)
                {
                    bool hit = true;
                    foreach (string key in keywords)
                    {
                        if (!inp.Value.Title.ToLower().Contains(key))
                        {
                            hit = false;
                            break;
                        }
                    }
                    if (hit)
                        results.Add(new SearchQueryResults() { ID = inp.Value.ID, AIRCRAFT = inp.Value.Plane, DESCRIPTION = inp.Value.Title });
                }

                foreach (KeyValuePair<string, DCSInput> inp in kvp.Value.Buttons)
                {
                    bool hit = true;
                    foreach (string key in keywords)
                    {
                        if (!inp.Value.Title.ToLower().Contains(key))
                        {
                            hit = false;
                            break;
                        }
                    }
                    if (hit)
                        results.Add(new SearchQueryResults() { ID = inp.Value.ID, AIRCRAFT = inp.Value.Plane, DESCRIPTION = inp.Value.Title });
                }
            }
            foreach(KeyValuePair<string, OtherGame> kvp in OtherLib)
            {
                foreach(KeyValuePair<string, OtherGameInput> inp in kvp.Value.Axis)
                {
                    bool hit = true;
                    foreach (string key in keywords)
                    {
                        if (!inp.Value.Title.ToLower().Contains(key))
                        {
                            hit = false;
                            break;
                        }
                    }
                    if (hit)
                        results.Add(new SearchQueryResults() { ID = inp.Value.ID, AIRCRAFT = inp.Value.Game, DESCRIPTION = inp.Value.Title });
                }
                foreach (KeyValuePair<string, OtherGameInput> inp in kvp.Value.Buttons)
                {
                    bool hit = true;
                    foreach (string key in keywords)
                    {
                        if (!inp.Value.Title.ToLower().Contains(key))
                        {
                            hit = false;
                            break;
                        }
                    }
                    if (hit)
                        results.Add(new SearchQueryResults() { ID = inp.Value.ID, AIRCRAFT = inp.Value.Game, DESCRIPTION = inp.Value.Title });
                }
            }
            return results;
        }
        public static DCSInput[] GetAllInputsWithId(string id)
        {
            List<DCSInput> results = new List<DCSInput>();
            foreach (KeyValuePair<string, DCSPlane> kvp in DCSLib)
            {
                if (id.Substring(0, 1) == "a")
                {
                    if (kvp.Value.Axis.ContainsKey(id)) results.Add(kvp.Value.Axis[id]);
                }
                else
                {
                    if (kvp.Value.Buttons.ContainsKey(id)) results.Add(kvp.Value.Buttons[id]);
                }
            }
            return results.ToArray();
        }
        public static void InitDCSData()
        {
            PROGPATH = Environment.CurrentDirectory;
            Console.WriteLine(PROGPATH);
            LoadDcsData();
            LoadCleanLuasDCS();
            LoadLocalDefaultsDCS();
            List<string> installs = new List<string>();
            string pth = GetRegistryValue("SOFTWARE\\Eagle Dynamics\\DCS World", "Path", "CurrentUser");
            if (pth != null) installs.Add(pth);
            pth = GetRegistryValue("SOFTWARE\\Eagle Dynamics\\DCS World OpenBeta", "Path", "CurrentUser");
            if (pth != null) installs.Add(pth);
            installPathsDCS = installs.ToArray();
        }

        public static void UnloadGameData()
        {
            DCSLib.Clear();
            Planes = new string[0];
            installPathsDCS = new string[0];
            LocalJoysticks = new string[0];
            EmptyOutputsDCS = new Dictionary<string, DCSLuaInput>();
            IL2JoystickId = new Dictionary<string, int>();
            OtherLib = new Dictionary<string, OtherGame>();
            IL2Instance = "";

        }

        public static void InitIL2Data()
        {
            PROGPATH = Environment.CurrentDirectory;
            Console.WriteLine(PROGPATH);
            LoadIL2Path();
            InitIL2Joysticks();
            PopulateIL2Dictionary();
            Console.WriteLine(IL2Instance);
        }

        public static string GetRegistryValue(string Path, string Value, string Locality)
        {
            try
            {
                RegistryKey key = null;
                switch (Locality)
                {
                    case "CurrentUser":
                        key = Registry.CurrentUser.OpenSubKey(Path);
                        break;
                    case "LocalMachine":
                        key = Registry.LocalMachine.OpenSubKey(Path);
                        break;
                }
                if (key != null)
                {
                    string currentKey = key.GetValue(Value, true).ToString();
                    return currentKey;
                }
            }
            catch
            {

            }
            return null;
        }

        public static void BackupConfigsOfInstance(string instance)
        {
            if (!Directory.Exists(instance + runningBackupFolder))
            {
                Directory.CreateDirectory(instance + runningBackupFolder);
            }
            if (!Directory.Exists(instance + initialBackupFolder) && Directory.Exists(instance + subInputPath))
            {
                Directory.CreateDirectory(instance + initialBackupFolder);
                CopyFolderIntoFolder(instance + subInputPath, instance + initialBackupFolder);
            }
            if (Directory.Exists(instance + subInputPath))
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd");
                if (!Directory.Exists(instance + runningBackupFolder))
                    Directory.CreateDirectory(instance + runningBackupFolder);
                DirectoryInfo pFolder = new DirectoryInfo(instance + runningBackupFolder);
                DirectoryInfo[] allSubs = pFolder.GetDirectories();
                if (msave != null)
                    if (allSubs.Length > msave.backupDays)
                    {
                        List<DirectoryInfo> subList = allSubs.ToList();
                        subList = subList.OrderBy(o => o.Name).ToList();
                        DeleteFolder(subList[0].FullName);
                    }
                if (!Directory.Exists(instance + runningBackupFolder + "\\" + now))
                {
                    Directory.CreateDirectory(instance + runningBackupFolder + "\\" + now);
                    CopyFolderIntoFolder(instance + subInputPath, instance + runningBackupFolder + "\\" + now);
                }

            }
        }

        public static void RestoreInputsInInstance(string instance, string fallBack)
        {
            string dir;
            if (fallBack == "initial")
            {
                if (Directory.Exists(instance + initialBackupFolder + inputFolderName))
                {
                    dir = instance + initialBackupFolder + inputFolderName;
                }
                else
                {
                    MessageBox.Show("Initial Backup does not exist.");
                    return;
                }
            }
            else
            {
                if (Directory.Exists(instance + runningBackupFolder + "\\" + fallBack + inputFolderName))
                {
                    dir = instance + runningBackupFolder + "\\" + fallBack + inputFolderName;
                }
                else
                {
                    MessageBox.Show("Given Fallback does not exist.");
                    return;
                }
            }
            if (!Directory.Exists(instance + subInputPath))
                Directory.CreateDirectory(instance + subInputPath);

            DirectoryInfo dirin = new DirectoryInfo(dir);
            DirectoryInfo[] toCopy = dirin.GetDirectories();
            for (int i = 0; i < toCopy.Length; ++i)
                CopyFolderIntoFolder(toCopy[i].FullName, dir);

        }

        public static List<string> GetPossibleFallbacksForInstance(string instance)
        {
            List<string> fallback = new List<string>();
            if (Directory.Exists(instance + initialBackupFolder + inputFolderName))
            {
                fallback.Add("Initial");
            }
            if (Directory.Exists(instance + runningBackupFolder))
            {
                DirectoryInfo pFolder = new DirectoryInfo(instance + runningBackupFolder);
                DirectoryInfo[] subs = pFolder.GetDirectories();
                for (int i = 0; i < subs.Length; ++i)
                    fallback.Add(subs[i].Name);
            }

            return fallback;
        }

        public static void CopyFolderIntoFolder(string source, string dest)
        {
            string[] splitt = source.Split('\\');
            string last_part = splitt[splitt.Length - 1];
            if (!Directory.Exists(dest + "\\" + last_part)) Directory.CreateDirectory(dest + "\\" + last_part);
            DirectoryInfo sor = new DirectoryInfo(source);
            FileInfo[] all_files = sor.GetFiles();
            for (int i = 0; i < all_files.Length; i++)
            {
                File.Copy(all_files[i].FullName, dest + "\\" + last_part + "\\" + all_files[i].Name, true);
            }

            DirectoryInfo[] all_dirs = sor.GetDirectories();
            for (int i = 0; i < all_dirs.Length; i++)
            {
                CopyFolderIntoFolder(all_dirs[i].FullName, dest + "\\" + last_part);
            }
        }

        public static void DeleteFolder(string folder)
        {
            DirectoryInfo dd = new DirectoryInfo(folder);
            FileInfo[] fi = dd.GetFiles();
            for (int i = 0; i < fi.Length; i++)
            {
                File.Delete(fi[i].FullName);
            }
            DirectoryInfo[] dirs = dd.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                DeleteFolder(dirs[i].FullName);
            }
            Directory.Delete(folder);
        }


        public static string GetDCSInstallationPath()
        {
            if (installPathsDCS.Length > 0)
            {
                if (installPathsDCS.Length == 1 && Directory.Exists(installPathsDCS[0]))
                {
                    return installPathsDCS[0];
                }
                else
                {
                    for (int i = 0; i < installPathsDCS.Length; ++i)
                    {
                        if (selectedInstancePath.ToLower().Contains("openbeta") && installPathsDCS[i].ToLower().Contains("beta") && Directory.Exists(installPathsDCS[i]))
                        {
                            return installPathsDCS[i];
                        }
                        else if (!selectedInstancePath.ToLower().Contains("openbeta") && !installPathsDCS[i].ToLower().Contains("beta") && Directory.Exists(installPathsDCS[i]))
                        {
                            return installPathsDCS[i];
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
                    if (selectedInstancePath.ToLower().Contains("openbeta"))
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
        public static void PopulateIL2Dictionary()
        {
            string fileActionPath = PROGPATH + "\\DB\\IL2\\IL2.actions";
            string gameName = "IL2Game";
            if (File.Exists(fileActionPath))
            {
                if (!OtherLib.ContainsKey(gameName))
                {
                    OtherLib.Add(gameName, new OtherGame(gameName));
                }
                List<string> tempPlanes = Planes.ToList();
                if (!tempPlanes.Contains(gameName))
                    tempPlanes.Add(gameName);
                Planes = tempPlanes.ToArray();
                StreamReader sr = new StreamReader(fileActionPath);
                while (!sr.EndOfStream)
                {
                    string rawLine = sr.ReadLine();
                    if (rawLine.Contains("//a") || rawLine.Contains("//b"))
                    {
                        int dataSeperator = rawLine.IndexOf(",");
                        if (dataSeperator < 0)
                            continue;
                        string id = rawLine.Substring(0, dataSeperator);
                        rawLine = rawLine.Substring(dataSeperator + 1);
                        dataSeperator = rawLine.IndexOf("//");
                        if (dataSeperator < 0)
                            continue;
                        rawLine = rawLine.Substring(dataSeperator);
                        bool axis = false;
                        if (rawLine.Contains("//a"))
                            axis = true;
                        string descriptor = rawLine.Substring(4);
                        OtherGameInput current = new OtherGameInput(id, descriptor, axis, gameName);
                        if (axis)
                        {
                            if (!OtherLib[gameName].Axis.ContainsKey(id))
                                OtherLib[gameName].Axis.Add(id, current);
                        }
                        else
                        {
                            if (!OtherLib[gameName].Buttons.ContainsKey(id))
                                OtherLib[gameName].Buttons.Add(id, current);
                        }
                    }

                }
                sr.Close();
            }
        }
        public static void PopulateDCSDictionaryWithLocal(string instance)
        {
            if (Directory.Exists(instance + "\\InputLayoutsTxt"))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(instance + "\\InputLayoutsTxt");
                DirectoryInfo[] allHtmlDirs = dirInfo.GetDirectories();
                List<string> tempPlanes;
                if (Planes == null) tempPlanes = new List<string>();
                else tempPlanes = Planes.ToList();

                for (int i = 0; i < allHtmlDirs.Length; ++i)
                {
                    string currentPlane = allHtmlDirs[i].Name;
                    FileInfo[] filesInDir = allHtmlDirs[i].GetFiles();
                    for (int j = 0; j < filesInDir.Length; j++)
                    {
                        if (filesInDir[j].Name.EndsWith(".html") &&
                            !filesInDir[j].Name.Contains("Keyboard") &&
                            !filesInDir[j].Name.Contains("TrackIR") &&
                            !filesInDir[j].Name.Contains("Mouse"))
                        {
                            PopulateDictionaryWithFile(filesInDir[j].FullName, currentPlane);
                            if (!tempPlanes.Contains(currentPlane))
                            {
                                tempPlanes.Add(currentPlane);
                                if(!EmptyOutputsDCS.ContainsKey(currentPlane))
                                {
                                    ReadDefaultsFromHTML(currentPlane, filesInDir[j].FullName);
                                }
                            }


                        }
                    }
                }
                Planes = tempPlanes.ToArray();
            }
            
        }

        static void ReadDefaultsFromHTML(string plane, string file)
        {
            DCSLuaInput def;
            if (!EmptyOutputsDCS.ContainsKey(plane))
            {
                def = new DCSLuaInput();
                EmptyOutputsDCS.Add(plane, def);
                def.plane = plane;
                def.JoystickName = "EMPTY";
            }
            else
            {
                def = EmptyOutputsDCS[plane];
            }            
            List<HtmlInputElementDCS> result = GetElementsFromHTMLDCS(file);
            for(int i=0; i<result.Count; ++i)
            {
                if(result[i].bind!=null&&
                    result[i].bind.Replace(" ", "").Length > 0)
                {
                    if (result[i].id.Substring(0, 1) == "a")
                    {
                        def.InvertedHTMLAnalyzeAxis(result[i]);
                    }
                    else
                    {
                        def.InvertedHTMLAnalyzeBtn(result[i]);
                    }
                }
            }

        }

    static void PopulateDCSDictionaryWithProgram()
        {
            DirectoryInfo fileStorage = new DirectoryInfo(PROGPATH + "\\DB\\DCS");
            FileInfo[] allFilesShipped = fileStorage.GetFiles();
            List<string> loadedPlanes = new List<string>();
            for (int i = 0; i < allFilesShipped.Length; ++i)
            {
                if (allFilesShipped[i].Name.EndsWith(".html"))
                {
                    PopulateDictionaryWithFile(allFilesShipped[i].FullName);
                    if (!loadedPlanes.Contains(allFilesShipped[i].Name.Replace(".html", "")))
                        loadedPlanes.Add(allFilesShipped[i].Name.Replace(".html", ""));
                }
            }
            if (Planes == null)
                Planes = loadedPlanes.ToArray();
            else
            {
                List<string> pp = Planes.ToList<string>();
                for (int i = 0; i < loadedPlanes.Count; ++i)
                {
                    if (!pp.Contains(loadedPlanes[i]))
                        pp.Add(loadedPlanes[i]);
                }
                Planes = pp.ToArray();
            }
        }
        static void PopulateDictionaryWithFile(string file, string overWrite = "")
        {
            string planeName;
            if (overWrite.Length > 1)
            {
                planeName = overWrite;
            }
            else
            {
                string[] parts = file.Split('\\');
                planeName = parts[parts.Length - 1].Replace(".html", "");
            }
            List<HtmlInputElementDCS> elementsDCS = GetElementsFromHTMLDCS(file);
            if (!DCSLib.ContainsKey(planeName))
                DCSLib.Add(planeName, new DCSPlane(planeName));
            for(int i=0; i<elementsDCS.Count; ++i)
            {
                if (elementsDCS[i].id.Substring(0, 1) == "a")
                {
                    if (!DCSLib[planeName].Axis.ContainsKey(elementsDCS[i].id))
                        DCSLib[planeName].Axis.Add(elementsDCS[i].id, new DCSInput(elementsDCS[i].id, elementsDCS[i].title, true, planeName));
                    else
                        DCSLib[planeName].Axis[elementsDCS[i].id].Title = elementsDCS[i].title;
                }
                else
                {
                    if (!DCSLib[planeName].Buttons.ContainsKey(elementsDCS[i].id))
                        DCSLib[planeName].Buttons.Add(elementsDCS[i].id, new DCSInput(elementsDCS[i].id, elementsDCS[i].title, false, planeName));
                    else
                        DCSLib[planeName].Buttons[elementsDCS[i].id].Title = elementsDCS[i].title;
                }
            }
        }
        public static List<HtmlBindElement> GetBindElmentsFromCell(string cellContent)
        {
            List<HtmlBindElement> result = new List<HtmlBindElement>();
            int dashIndex = cellContent.IndexOf("- ");
            string[] parts = cellContent.Split(new string[] { "; " }, StringSplitOptions.None);
            for(int i=0; i<parts.Length; ++i)
            {
                string[] bindParts = parts[i].Split(new string[] { "- " }, StringSplitOptions.None);
                HtmlBindElement current = new HtmlBindElement();
                current.button = bindParts[bindParts.Length - 1];
                current.reformers = new List<string>();
                for(int j=0; j<bindParts.Length-1; ++j)
                {
                    current.reformers.Add(bindParts[j]);
                }
            }
            return result;
        }

        static List<HtmlInputElementDCS> GetElementsFromHTMLDCS(string file)
        {
            List<HtmlInputElementDCS> result = new List<HtmlInputElementDCS>();
            StreamReader sr = new StreamReader(file);
            int iterator = 0;
            HtmlInputElementDCS current = new HtmlInputElementDCS();
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();
                if (iterator > 0)
                {
                    string cleanedLine = currentLine.Replace("\t", "").Replace("<td>", "").Replace("</td>", "").Replace("  ", "").Trim();
                    switch (iterator)
                    {
                        case 1:
                            current.bind = cleanedLine;
                            break;
                        case 2:
                            current.title = cleanedLine;
                            break;
                        case 3:
                            current.category = cleanedLine;
                            break;
                        case 4:
                            current.id = cleanedLine;
                            iterator = -1;
                            result.Add(current);
                            current = new HtmlInputElementDCS();
                            break;
                    }
                    iterator++;
                }
                if (currentLine.Contains("</tr>"))
                    iterator = 0;
                if (currentLine.Contains("<tr>"))
                    iterator++;
            }
            sr.Close();
            return result;

        }

        static string[] GetDCSUserFolders()
        {
            KnownFolder sg = KnownFolder.SavedGames;
            SaveGamesPath = KnownFolders.GetPath(sg);
            string[] dirs = Directory.GetDirectories(SaveGamesPath);
            List<string> candidates = new List<string>();
            for (int i = 0; i < dirs.Length; ++i)
            {
                string[] parts = dirs[i].Split('\\');
                string lastPart = parts[parts.Length - 1];
                if (lastPart.StartsWith("DCS")) candidates.Add(dirs[i]);
            }
            return candidates.ToArray();
        }
        public static void InitIL2Joysticks()
        {
            List<string> Joysticks = new List<string>();
            if (File.Exists(IL2Instance + "\\data\\input\\devices.txt"))
            {
                StreamReader sr = new StreamReader(IL2Instance + "\\data\\input\\devices.txt");
                string content = sr.ReadToEnd().Replace("\r", "").Replace("|", "");
                string[] lines = content.Split('\n');
                for (int i = 1; i < lines.Length; ++i)
                {
                    string[] parts = lines[i].Split(',');
                    if (parts.Length > 2 && int.TryParse(parts[0], out int id) == true)
                    {
                        string joy = IL2JoyIdToDCSJoyId(parts[1], parts[2]);
                        Joysticks.Add(joy);
                        if (!IL2JoystickId.ContainsKey(joy) && !IL2JoystickId.ContainsValue(id))
                        {
                            IL2JoystickId.Add(joy, id);
                        }
                    }
                }

            }
        }

        static string IL2JoyIdToDCSJoyId(string guid, string device)
        {
            guid = guid.Replace("%22", "");
            device = device.Replace("%20", " ");
            string[] guidParts = guid.Split('-');
            string output = device + " {" + guidParts[0].ToUpper();
            for (int i = 1; i < guidParts.Length; ++i)
            {
                if (i == 2)
                {
                    output += "-" + guidParts[i].ToLower();
                }
                else
                {
                    output += "-" + guidParts[i].ToUpper();
                }
            }
            output += "}";

            return output;
        }
        public static void InitDCSJoysticks()
        {
            List<string> Joysticks = new List<string>();
            if (Directory.Exists(selectedInstancePath + "\\InputLayoutsTxt"))
            {
                string[] subs = Directory.GetDirectories(selectedInstancePath + "\\InputLayoutsTxt");
                for (int j = 0; j < subs.Length; j++)
                {
                    string[] files = Directory.GetFiles(subs[j]);
                    for (int k = 0; k < files.Length; k++)
                    {
                        string[] parts = files[k].Split('\\');
                        string toCompare = parts[parts.Length - 1];
                        if (toCompare.EndsWith(".html"))
                        {
                            string toAdd = toCompare.Replace(".html", "");
                            if (!Joysticks.Contains(DCSStickNaming(toAdd)) && toAdd != "Keyboard" && toAdd != "Mouse" && toAdd != "TrackIR")
                            {
                                Joysticks.Add(DCSStickNaming(toAdd));
                            }
                        }
                    }
                }
            }
            if (Directory.Exists(selectedInstancePath + "\\Config\\Input"))
            {
                string[] subs = Directory.GetDirectories(selectedInstancePath + "\\Config\\Input");
                for (int j = 0; j < subs.Length; j++)
                {
                    string[] inputs = Directory.GetDirectories(subs[j]);
                    for (int k = 0; k < inputs.Length; ++k)
                    {
                        string[] planes = Directory.GetFiles(inputs[k]);
                        for (int z = 0; z < planes.Length; ++z)
                        {
                            string[] parts = planes[z].Split('\\');
                            string toCompare = parts[parts.Length - 1];
                            if (toCompare.EndsWith(".diff.lua"))
                            {
                                string toAdd = toCompare.Replace(".diff.lua", "");
                                if (!Joysticks.Contains(DCSStickNaming(toAdd)) && toAdd != "Keyboard" && toAdd != "Mouse" && toAdd != "TrackIR") Joysticks.Add(DCSStickNaming(toAdd));
                            }
                        }
                    }
                }
            }
            if (LocalJoysticks == null)
                LocalJoysticks = Joysticks.ToArray();
            else
            {
                List<string> pp = LocalJoysticks.ToList();
                for (int i = 0; i < Joysticks.Count; ++i)
                {
                    if (!pp.Contains(Joysticks[i]))
                        pp.Add(Joysticks[i]);
                }
                LocalJoysticks = pp.ToArray();
            }
        }

        public static List<Relation> GetAllRelations()
        {
            List<Relation> result = new List<Relation>();
            foreach (KeyValuePair<string, Relation> kvp in AllRelations)
                result.Add(kvp.Value);
            return result;
        }

        public static List<Bind> GetAllBinds()
        {
            List<Bind> result = new List<Bind>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
                result.Add(kvp.Value);
            return result;
        }

    }
}
