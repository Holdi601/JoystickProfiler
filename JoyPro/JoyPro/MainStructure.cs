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
        const int version = 35;
        public static MainWindow mainW;
        public static string SELECTEDGAME = "";
        public static string PROGPATH;
        public static Dictionary<string, DCSPlane> DCSLib = new Dictionary<string, DCSPlane>();
        public static string[] DCSJoysticks;
        public static string SaveGamesPath;
        public static string[] DCSInstances;
        public static Game SelectedGame;
        public static string[] Planes;
        static Dictionary<string, Relation> AllRelations = new Dictionary<string, Relation>();
        static Dictionary<string, Bind> AllBinds = new Dictionary<string, Bind>();
        public static Dictionary<string, DCSLuaInput> EmptyOutputs = new Dictionary<string, DCSLuaInput>();
        static Dictionary<string, DCSExportPlane> LocalBinds = new Dictionary<string, DCSExportPlane>();
        static Dictionary<string, DCSExportPlane> ToExport = new Dictionary<string, DCSExportPlane>();
        static List<string> defaultToOverwrite = new List<string>();
        public static MetaSave msave = null;
        public static string selectedInstancePath = "";
        static Dictionary<string, Modifier> AllModifiers = new Dictionary<string, Modifier>();
        public static string[] installPaths;
        static string newestAvailableVersion;
        static int downloadFails = 0;
        public static string IL2Instance = "";

        static event EventHandler downloadCompletedEvent;

        public static void loadIL2Path()
        {
            string pth = getRegistryValue("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 307960", "InstallLocation", "LocalMachine");
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

        static void downloadCompleted(object o, EventArgs e)
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
                    File.Delete(pth + "\\meta.info");
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

        static void writeFiles(string ending = ".diff.lua")
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvp in ToExport)
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
                        outputName = outputName + idParts[0].ToUpper();
                        for(int i=1; i< idParts.Length; ++i)
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
                    string finalPath = adjustedPath + outputName + ending;
                    kvJoy.Value.writeLua(finalPath);
                }
            }
        }
        public static ModExists DoesReformerExistInMods(string reformer)
        {
            Modifier m = ReformerToMod(reformer);
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
            Modifier m = ReformerToMod(reformer);
            AllModifiers.Add(m.name, m);
        }

        public static Dictionary<string, Bind> libraryFromLocalDict(Dictionary<string, DCSExportPlane> lib, List<string> sticks, bool loadDefaults, bool inv = false, bool slid = false, bool curv = false, bool dz = false, bool sx = false, bool sy = false)
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
                                result[b.Rl.NAME].Rl.AddNode(kvpaxe.Key, plane);
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
                                result[b.Rl.NAME].Rl.AddNode(kvpbe.Key, plane);
                            }
                        }
                    }
                }
            }
            if (loadDefaults)
            {
                foreach (KeyValuePair<string, DCSLuaInput> kvp in EmptyOutputs)
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
                                            result[b.Rl.NAME].Rl.AddNode(idToCheck, planeToCheck);
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
                                            result[b.Rl.NAME].Rl.AddNode(idToCheck, planeToCheck);
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
            Dictionary<string, Bind> checkRes = libraryFromLocalDict(checkAgainst, sticks, loadDefaults, inv, slid, curv, dz, sx, sy);
            Dictionary<string, Bind> result = libraryFromLocalDict(LocalBinds, sticks, loadDefaults, inv, slid, curv, dz, sx, sy);
            foreach (KeyValuePair<string, Bind> kvp in checkRes)
            {
                correctBindNames(result, kvp.Value);
            }

            MergeImport(result);
        }

        static void correctBindNames(Dictionary<string, Bind> lib, Bind b)
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
                        b.Slider == kvp.Value.Slider)
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
                            kvp.Value.Rl.NAME = b.additionalImportInfo;
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
                    name = kvp.Value.additionalImportInfo;
                else
                    name = kvp.Value.Rl.NAME;
                while (AllRelations.ContainsKey(name))
                {
                    name = name + "i";
                }
                kvp.Value.Rl.NAME = name;
                AllRelations.Add(name, kvp.Value.Rl);
                AllBinds.Add(name, kvp.Value);
            }
            ResyncRelations();
        }
        public static void WriteProfileCleanNotOverwriteLocal(bool fillBeforeEmpty)
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            ToExport.Clear();
            defaultToOverwrite = new List<string>();
            LoadLocalBinds(selectedInstancePath, true);
            OverwriteExportWith(LocalBinds, true, false, false);
            PushAllBindsToExport(false, fillBeforeEmpty, false);
            writeFiles();
            writeFiles(".jp");
            mainW.ShowMessageBox("It appears to have successfully exported");
        }
        public static void WriteProfileCleanAndLoadedOverwritten(bool fillBeforeEmpty)
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            ToExport.Clear();
            defaultToOverwrite = new List<string>();
            LoadLocalBinds(selectedInstancePath, true);
            OverwriteExportWith(LocalBinds, true, false, false);
            PushAllBindsToExport(true, fillBeforeEmpty, false);
            writeFiles();
            writeFiles(".jp");
            mainW.ShowMessageBox("It appears to have successfully exported");
        }
        public static string ShortenDeviceName(string device)
        {
            if (!device.Contains("{")) return null;
            return device.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
        }
        public static void WriteProfileCleanAndLoadedOverwrittenAndAdd(bool fillBeforeEmpty)
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            ToExport.Clear();
            defaultToOverwrite = new List<string>();
            LoadLocalBinds(selectedInstancePath, true);
            OverwriteExportWith(LocalBinds, true, false, false);
            PushAllBindsToExport(true, fillBeforeEmpty, true);
            writeFiles();
            writeFiles(".jp");
            mainW.ShowMessageBox("It appears to have successfully exported");
        }
        public static void WriteProfileClean(bool nukeDevices)
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            ToExport.Clear();
            PushAllBindsToExport(true, true, false);
            if (nukeDevices)
                NukeUnusedButConnectedDevicesToExport();
            writeFiles();
            writeFiles(".jp");
            mainW.ShowMessageBox("It appears to have successfully exported");
        }
        static void NukeUnusedButConnectedDevicesToExport()
        {
            string[] allPlanes = Planes;
            List<string> connectedSticks = JoystickReader.GetConnectedJoysticks();
            for (int i = 0; i < allPlanes.Length; ++i)
            {
                if (!ToExport.ContainsKey(allPlanes[i]))
                {
                    ToExport.Add(allPlanes[i], new DCSExportPlane());
                    ToExport[allPlanes[i]].plane = allPlanes[i];
                }
                DCSLuaInput empty = null;
                if (EmptyOutputs.ContainsKey(allPlanes[i]))
                {
                    empty = EmptyOutputs[allPlanes[i]];
                }
                else
                    continue;
                for (int j = 0; j < connectedSticks.Count; j++)
                {
                    if (!ToExport[allPlanes[i]].joystickConfig.ContainsKey(connectedSticks[j]))
                    {
                        ToExport[allPlanes[i]].joystickConfig.Add(connectedSticks[j], empty.Copy());
                        ToExport[allPlanes[i]].joystickConfig[connectedSticks[j]].plane = allPlanes[i];
                        ToExport[allPlanes[i]].joystickConfig[connectedSticks[j]].JoystickName = connectedSticks[j];
                    }
                }
            }
        }
        public static void PushAllBindsToExport(bool oride, bool fillBeforeEmpty = true, bool overwriteAdd = false)
        {
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick.Length > 0 &&
                    ((kvp.Value.Rl.ISAXIS && kvp.Value.JAxis.Length > 0) ||
                    (!kvp.Value.Rl.ISAXIS && kvp.Value.JButton.Length > 0)))
                    OverwriteExportWith(bindToExportFormat(kvp.Value), oride, fillBeforeEmpty, overwriteAdd);
            }
        }
        public static Dictionary<string, DCSExportPlane> bindToExportFormat(Bind b)
        {
            Dictionary<string, int> pstate = b.Rl.GetPlaneSetState();
            Dictionary<string, DCSExportPlane> result = new Dictionary<string, DCSExportPlane>();
            foreach (KeyValuePair<string, int> kvpPS in pstate)
            {
                if (kvpPS.Value > 0)
                {
                    RelationItem ri = b.Rl.GetRelationItemForPlane(kvpPS.Key);
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
        public static void OverwriteExportWith(Dictionary<string, DCSExportPlane> attr, bool overwrite = true, bool fillBeforeEmpty = true, bool overwriteAdd = false)
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvp in attr)
            {
                if ((!ToExport.ContainsKey(kvp.Key) && !fillBeforeEmpty) || (!ToExport.ContainsKey(kvp.Key) && fillBeforeEmpty && !EmptyOutputs.ContainsKey(kvp.Key)))
                {
                    ToExport.Add(kvp.Key, kvp.Value.Copy());
                }
                else
                {
                    if (!ToExport.ContainsKey(kvp.Key) && fillBeforeEmpty)
                    {
                        ToExport.Add(kvp.Key, new DCSExportPlane());
                        ToExport[kvp.Key].plane = kvp.Key;
                        foreach (KeyValuePair<string, DCSLuaInput> kvpDef in kvp.Value.joystickConfig)
                        {
                            if (!ToExport[kvp.Key].joystickConfig.ContainsKey(kvpDef.Key) && EmptyOutputs.ContainsKey(kvp.Key))
                            {
                                ToExport[kvp.Key].joystickConfig.Add(kvpDef.Key, EmptyOutputs[kvp.Key].Copy());
                                ToExport[kvp.Key].joystickConfig[kvpDef.Key].JoystickName = kvpDef.Key;
                                ToExport[kvp.Key].joystickConfig[kvpDef.Key].plane = kvp.Key;
                                string toCheck = kvpDef.Key + "§" + kvp.Key;
                                if (!defaultToOverwrite.Contains(toCheck)) defaultToOverwrite.Add(toCheck);
                            }
                        }
                    }
                    foreach (KeyValuePair<string, Modifier> kMod in kvp.Value.modifiers)
                    {
                        if (!ToExport[kvp.Key].modifiers.ContainsKey(kMod.Key))
                        {
                            ToExport[kvp.Key].modifiers.Add(kMod.Key, kMod.Value);
                        }
                        else if (overwrite)
                        {
                            ToExport[kvp.Key].modifiers[kMod.Key] = kMod.Value;
                        }
                    }
                    foreach (KeyValuePair<string, DCSLuaInput> kvpIn in kvp.Value.joystickConfig)
                    {
                        if (!ToExport[kvp.Key].joystickConfig.ContainsKey(kvpIn.Key) && fillBeforeEmpty && EmptyOutputs.ContainsKey(kvp.Key))
                        {
                            ToExport[kvp.Key].joystickConfig.Add(kvpIn.Key, EmptyOutputs[kvp.Key].Copy());
                            ToExport[kvp.Key].joystickConfig[kvpIn.Key].JoystickName = kvpIn.Key;
                            ToExport[kvp.Key].joystickConfig[kvpIn.Key].plane = kvp.Key;
                            string toCheck = kvpIn.Key + "§" + kvp.Key;
                            if (!defaultToOverwrite.Contains(toCheck)) defaultToOverwrite.Add(toCheck);
                        }
                        if (!ToExport[kvp.Key].joystickConfig.ContainsKey(kvpIn.Key))
                        {
                            ToExport[kvp.Key].joystickConfig.Add(kvpIn.Key, kvpIn.Value);
                        }
                        else
                        {
                            string current = kvpIn.Key + "§" + kvp.Key;
                            foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvpDiffsAxisElement in kvpIn.Value.axisDiffs)
                            {
                                if (!ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs.ContainsKey(kvpDiffsAxisElement.Key))
                                {
                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs.Add(kvpDiffsAxisElement.Key, kvpDiffsAxisElement.Value.Copy());
                                }
                                else if (overwrite || defaultToOverwrite.Contains(current))
                                {
                                    DCSLuaDiffsAxisElement old = ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].Copy();
                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key] = kvpDiffsAxisElement.Value.Copy();
                                    if (overwriteAdd)
                                    {
                                        for (int i = 0; i < old.added.Count; ++i)
                                        {
                                            if (!kvpDiffsAxisElement.Value.doesAddedContainKey(old.added[i].key) && !kvpDiffsAxisElement.Value.doesRemovedContainKey(old.added[i].key))
                                            {
                                                ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].added.Add(old.added[i].Copy());
                                            }
                                        }
                                    }
                                    for (int i = 0; i < old.removed.Count; ++i)
                                    {
                                        if (!kvpDiffsAxisElement.Value.doesAddedContainKey(old.removed[i].key) && !kvpDiffsAxisElement.Value.doesRemovedContainKey(old.removed[i].key))
                                        {
                                            ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].removed.Add(old.removed[i].Copy());
                                        }
                                    }
                                    if (fillBeforeEmpty)
                                    {
                                        if (EmptyOutputs.ContainsKey(kvp.Key) && EmptyOutputs[kvp.Key].axisDiffs.ContainsKey(kvpDiffsAxisElement.Key))
                                        {
                                            bool found = false;
                                            for (int r = 0; r < EmptyOutputs[kvp.Key].axisDiffs[kvpDiffsAxisElement.Key].removed.Count; ++r)
                                            {
                                                for (int w = 0; w < kvpDiffsAxisElement.Value.added.Count; ++w)
                                                {
                                                    if (kvpDiffsAxisElement.Value.added[w].key == EmptyOutputs[kvp.Key].axisDiffs[kvpDiffsAxisElement.Key].removed[r].key)
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
                                                for (int r = 0; r < EmptyOutputs[kvp.Key].axisDiffs[kvpDiffsAxisElement.Key].removed.Count; ++r)
                                                {
                                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].removed.Add(EmptyOutputs[kvp.Key].axisDiffs[kvpDiffsAxisElement.Key].removed[r]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (KeyValuePair<string, DCSLuaDiffsButtonElement> kvpDiffsButtonsElement in kvpIn.Value.keyDiffs)
                            {
                                if (!ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs.ContainsKey(kvpDiffsButtonsElement.Key))
                                {
                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs.Add(kvpDiffsButtonsElement.Key, kvpDiffsButtonsElement.Value.Copy());
                                }
                                else if (overwrite || defaultToOverwrite.Contains(current))
                                {
                                    DCSLuaDiffsButtonElement old = ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].Copy();
                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key] = kvpDiffsButtonsElement.Value.Copy();
                                    if (overwriteAdd)
                                    {
                                        for (int i = 0; i < old.added.Count; ++i)
                                        {
                                            if (!kvpDiffsButtonsElement.Value.doesAddedContainKey(old.added[i].key, old.added[i].reformers) && !kvpDiffsButtonsElement.Value.doesRemovedContainKey(old.added[i].key))
                                            {
                                                ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].added.Add(old.added[i].Copy());
                                            }
                                        }
                                    }
                                    for (int i = 0; i < old.removed.Count; ++i)
                                    {
                                        if (!kvpDiffsButtonsElement.Value.doesAddedContainKey(old.removed[i].key, old.removed[i].reformers) && !kvpDiffsButtonsElement.Value.doesRemovedContainKey(old.removed[i].key))
                                        {
                                            ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Add(old.removed[i].Copy());
                                        }
                                    }
                                    if (fillBeforeEmpty)
                                    {
                                        if (EmptyOutputs.ContainsKey(kvp.Key) && EmptyOutputs[kvp.Key].keyDiffs.ContainsKey(kvpDiffsButtonsElement.Key))
                                        {
                                            bool found = false;
                                            for (int r = 0; r < EmptyOutputs[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Count; ++r)
                                            {
                                                for (int w = 0; w < kvpDiffsButtonsElement.Value.added.Count; ++w)
                                                {
                                                    if (kvpDiffsButtonsElement.Value.added[w].key == EmptyOutputs[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed[r].key)
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
                                                for (int r = 0; r < EmptyOutputs[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Count; ++r)
                                                {
                                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Add(EmptyOutputs[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed[r]);
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
            foreach (KeyValuePair<string, DCSExportPlane> kvpExpPlane in ToExport)
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
                                if (EmptyOutputs.ContainsKey(kvpExpPlane.Key))
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
            if (isFirstValueLowestButNotNegative(indxQuotas, indxCurlyBrackets, indxBool, indxNumber)) return LuaDataType.String;
            if (isFirstValueLowestButNotNegative(indxCurlyBrackets, indxQuotas, indxBool, indxNumber)) return LuaDataType.Dict;
            if (isFirstValueLowestButNotNegative(indxNumber, indxQuotas, indxCurlyBrackets, indxBool)) return LuaDataType.Number;
            if (isFirstValueLowestButNotNegative(indxBool, indxQuotas, indxCurlyBrackets, indxNumber)) return LuaDataType.Bool;
            return LuaDataType.Error;
        }
        static bool isFirstValueLowestButNotNegative(int val1, int val2, int val3, int val4)
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
        public static void LoadLocalDefaults()
        {
            string install = GetInstallationPath();
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
                        if (!EmptyOutputs.ContainsKey(planeName))
                        {
                            EmptyOutputs.Add(planeName, new DCSLuaInput());
                            EmptyOutputs[planeName].plane = planeName;
                            EmptyOutputs[planeName].JoystickName = "EMPTY";
                        }
                        FileInfo[] files = innerPlaneCollection[j].GetFiles();
                        for (int k = 0; k < files.Length; k++)
                        {
                            if (files[k].Name.EndsWith(".diff.lua"))
                            {
                                StreamReader sr = new StreamReader(files[k].FullName);
                                string content = sr.ReadToEnd();
                                EmptyOutputs[planeName].AdditionalAnalyzationRawLuaInvert(content);
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
                        if (!EmptyOutputs.ContainsKey(planeName))
                        {
                            EmptyOutputs.Add(planeName, new DCSLuaInput());
                            EmptyOutputs[planeName].plane = planeName;
                            EmptyOutputs[planeName].JoystickName = "EMPTY";
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
                                    EmptyOutputs[planeName].AdditionalAnalyzationRawLuaInvert(content);
                                    sr.Close();
                                }
                            }
                        }
                    }
                }
            }

        }
        public static void LoadCleanLuas()
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
                EmptyOutputs.Add(plane, curPlane);
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
                toOutput = LocalBinds;
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
            ResyncRelations();
        }

        public static void ResyncBindsToMods()
        {
            AllModifiers.Clear();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                for (int i = 0; i < kvp.Value.AllReformers.Count; ++i)
                {
                    Modifier m = ReformerToMod(kvp.Value.AllReformers[i]);
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
        public static Modifier ReformerToMod(string reformer)
        {
            string[] parts = reformer.Split('§');
            if (parts.Length == 3)
            {
                Modifier m = new Modifier();
                m.name = parts[0];
                m.device = parts[1];
                m.sw = false;
                m.key = parts[2];
                return m;
            }
            else
            {
                return null;
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
            downloadCompletedEvent.Invoke(null, null);
        }

        static void DownloadNewerVersion()
        {
            if (downloadFails < 10)
            {
                try
                {
                    Uri uri = new Uri(buildPath + newestAvailableVersion + ".zip");
                    Console.WriteLine(buildPath + newestAvailableVersion + ".zip");
                    downloadCompletedEvent += new EventHandler(downloadCompleted);
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
                ResyncBindsToMods();
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
            if (DCSJoysticks != null)
                for (int i = 0; i < DCSJoysticks.Length; ++i)
                {
                    sticks.Add(DCSJoysticks[i]);
                }
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick != null && kvp.Value.Joystick.Length > 0 && !sticks.Contains<string>(kvp.Value.Joystick))
                {
                    sticks.Add(kvp.Value.Joystick);
                }
            }
            DCSJoysticks = sticks.ToArray();

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
            Pr0file pr = new Pr0file(AllRelations, AllBinds);
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
                    relWithBinds = relWithBinds.OrderBy(o => (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN","").Length<3?
                        (("0"+(AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")).Length<3 ? 
                            ("00" + (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")):
                            "0" + (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")):
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
            DCSLib.Clear();
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
            InitDCSJoysticks();
            SelectedGame = Game.DCS;
            List<string> installs = new List<string>();
            string pth = getRegistryValue("SOFTWARE\\Eagle Dynamics\\DCS World", "Path", "CurrentUser");
            if (pth != null) installs.Add(pth);
            pth = getRegistryValue("SOFTWARE\\Eagle Dynamics\\DCS World OpenBeta", "Path", "CurrentUser");
            if (pth != null) installs.Add(pth);
            installPaths = installs.ToArray();
        }

        public static string getRegistryValue(string Path, string Value, string Locality)
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
                copy_folder_into_folder(instance + subInputPath, instance + initialBackupFolder);
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
                        delete_folder(subList[0].FullName);
                    }
                if (!Directory.Exists(instance + runningBackupFolder + "\\" + now))
                {
                    Directory.CreateDirectory(instance + runningBackupFolder + "\\" + now);
                    copy_folder_into_folder(instance + subInputPath, instance + runningBackupFolder + "\\" + now);
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
                copy_folder_into_folder(toCopy[i].FullName, dir);

        }

        public static List<string> getPossibleFallbacksForInstance(string instance)
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

        public static void copy_folder_into_folder(string source, string dest)
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
                copy_folder_into_folder(all_dirs[i].FullName, dest + "\\" + last_part);
            }
        }

        public static void delete_folder(string folder)
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
                delete_folder(dirs[i].FullName);
            }
            Directory.Delete(folder);
        }


        public static string GetInstallationPath()
        {
            if (installPaths.Length > 0)
            {
                if (installPaths.Length == 1 && Directory.Exists(installPaths[0]))
                {
                    return installPaths[0];
                }
                else
                {
                    for (int i = 0; i < installPaths.Length; ++i)
                    {
                        if (selectedInstancePath.ToLower().Contains("openbeta") && installPaths[i].ToLower().Contains("beta") && Directory.Exists(installPaths[i]))
                        {
                            return installPaths[i];
                        }
                        else if (!selectedInstancePath.ToLower().Contains("openbeta") && !installPaths[i].ToLower().Contains("beta") && Directory.Exists(installPaths[i]))
                        {
                            return installPaths[i];
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
                        path = path + openBetaExtention;
                    }
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                }
            }
            return null;
        }
        public static void PopulateDCSDictionaryWithLocal(string instance)
        {
            if (Directory.Exists(instance + "\\InputLayoutsTxt"))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(instance + "\\InputLayoutsTxt");
                DirectoryInfo[] allHtmlDirs = dirInfo.GetDirectories();
                List<string> tempPlanes = Planes.ToList();
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
                                tempPlanes.Add(currentPlane);
                        }
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
            Planes = loadedPlanes.ToArray();
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
            StreamReader sr = new StreamReader(file);
            int iterator = 0;
            string id = "";
            string title = "";
            if (!DCSLib.ContainsKey(planeName))
                DCSLib.Add(planeName, new DCSPlane(planeName));
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();
                if (iterator > 0)
                {
                    string cleanedLine = currentLine.Replace("\t", "").Replace("<td>", "").Replace("</td>", "").Replace("  ", "").Trim();
                    switch (iterator)
                    {
                        case 2:
                            title = cleanedLine;
                            break;
                        case 4:
                            id = cleanedLine;
                            iterator = -1;
                            if (id.Substring(0, 1) == "a")
                            {
                                if (!DCSLib[planeName].Axis.ContainsKey(id))
                                    DCSLib[planeName].Axis.Add(id, new DCSInput(id, title, true, planeName));
                                else
                                    DCSLib[planeName].Axis[id].Title = title;
                            }
                            else
                            {
                                if (!DCSLib[planeName].Buttons.ContainsKey(id))
                                    DCSLib[planeName].Buttons.Add(id, new DCSInput(id, title, false, planeName));
                                else
                                    DCSLib[planeName].Buttons[id].Title = title;
                            }
                            id = "";
                            title = "";
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
        public static void InitDCSJoysticks()
        {
            List<string> Joysticks = new List<string>();

            for (int i = 0; i < DCSInstances.Length; ++i)
            {
                if (Directory.Exists(DCSInstances[i] + "\\InputLayoutsTxt"))
                {
                    string[] subs = Directory.GetDirectories(DCSInstances[i] + "\\InputLayoutsTxt");
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
                                if (!Joysticks.Contains(toAdd) && toAdd != "Keyboard" && toAdd != "Mouse" && toAdd != "TrackIR")
                                {
                                    Joysticks.Add(toAdd);
                                }
                            }
                        }
                    }
                }
                if (Directory.Exists(DCSInstances[i] + "\\Config\\Input"))
                {
                    string[] subs = Directory.GetDirectories(DCSInstances[i] + "\\Config\\Input");
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
                                    if (!Joysticks.Contains(toAdd) && toAdd != "Keyboard" && toAdd != "Mouse" && toAdd != "TrackIR") Joysticks.Add(toAdd);
                                }
                            }
                        }
                    }
                }
            }
            DCSJoysticks = Joysticks.ToArray();
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
