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

namespace JoyPro
{
    public enum Game { DCS, StarCitizen }
    public enum JoystickAxis { JOY_X, JOY_Y, JOY_Z, JOY_RX, JOY_RY, JOY_RZ, JOY_SLIDER1, JOY_SLIDER2, NONE }
    public enum LuaDataType { String, Number, Dict, Bool, Error };

    public enum ModExists { NOT_EXISTENT, BINDNAME_EXISTS, KEYBIND_EXISTS, ALL_EXISTS, ERROR }
    public static class MainStructure
    {
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

        public static List<string> GetAllModsAsString()
        {
            List<string> result = new List<string>();
            foreach(KeyValuePair<string, Modifier> kvp in AllModifiers)
            {
                result.Add(kvp.Key);
            }
            result = result.OrderBy(o => o).ToList();
            return result;
        }
        public static Modifier GetModifierWithKeyCombo(string device, string key)
        {
            foreach (KeyValuePair<string, Modifier> kvp in AllModifiers)
                if (kvp.Value.device == device && kvp.Value.key == key) return kvp.Value;
            return null;
        }

        public static void ChangeReformerName(string oldName, string newName)
        {
            if (AllModifiers.ContainsKey(oldName))
            {
                Modifier m = AllModifiers[oldName];
                string oldRefKey = m.toReformerString();
                AllModifiers.Remove(oldName);
                m.name = newName;
                foreach(KeyValuePair<string, Bind> kvp in AllBinds)
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
                foreach(KeyValuePair<string, Bind> kvp in AllBinds)
                {
                    if (kvp.Value.AllReformers.Contains(m.toReformerString()))
                        kvp.Value.AllReformers.Remove(m.toReformerString());
                }
            }
        }

        public static bool ModifiersContainKeyCombo(string device, string key)
        {
            bool found = false;
            foreach(KeyValuePair<string, Modifier> kvp in AllModifiers)
            {
                if (kvp.Value.device == device && kvp.Value.key==key) return true;
            }
            return found;
        }
        public static void LoadMetaLast()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            if (File.Exists(pth+ "\\meta.info"))
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

        static void writeFiles()
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvp in ToExport)
            {
                string modPath = selectedInstancePath + "\\Config\\Input\\" + kvp.Key;
                string adjustedPath = modPath + "\\joystick\\";
                if (!Directory.Exists(adjustedPath)) Directory.CreateDirectory(adjustedPath);
                kvp.Value.WriteModifiers(modPath);
                foreach (KeyValuePair<string, DCSLuaInput> kvJoy in kvp.Value.joystickConfig)
                {
                    string finalPath = adjustedPath + kvJoy.Key + ".diff.lua";
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
                if(ModifiersContainKeyCombo(m.device, m.key))
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
                if (AllModifiers[m.name].device == m.device &&
                    AllModifiers[m.name].key == m.key)
                {
                    return ModExists.ALL_EXISTS;
                }
                else
                {
                    return ModExists.BINDNAME_EXISTS;
                }
            }
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
        public static void BindsFromLocal(List<string> sticks, bool loadDefaults, bool inv = false, bool slid = false, bool curv = false, bool dz = false, bool sx = false, bool sy = false)
        {
            LoadLocalBinds(selectedInstancePath, loadDefaults);
            Dictionary<string, Bind> result = new Dictionary<string, Bind>();
            Dictionary<string, JoystickResults> modifiers = new Dictionary<string, JoystickResults>();
            Dictionary<string, Dictionary<string, List<string>>> checkedIds = new Dictionary<string, Dictionary<string, List<string>>>();
            foreach(KeyValuePair<string, DCSExportPlane> kvp in LocalBinds)
            {
                string plane = kvp.Key;
                if (!checkedIds.ContainsKey(plane)) checkedIds.Add(plane, new Dictionary<string, List<string>>());
                foreach(KeyValuePair<string, DCSLuaInput> kvpLua in kvp.Value.joystickConfig)
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
                        for (int i=0; i<kvpaxe.Value.added.Count; ++i)
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
                    foreach(KeyValuePair<string, DCSLuaDiffsButtonElement> kvpbe in kvpLua.Value.keyDiffs)
                    {
                        string k = kvpbe.Key;
                        if (!checkedIds[plane][joystick].Contains(k))
                            checkedIds[plane][joystick].Add(k);
                        for (int i=0; i<kvpbe.Value.added.Count; ++i)
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
                foreach(KeyValuePair<string, DCSLuaInput> kvp in EmptyOutputs)
                {
                    string planeToCheck = kvp.Key;
                    foreach(KeyValuePair<string, DCSLuaDiffsAxisElement> kvpax in kvp.Value.axisDiffs)
                    {
                        string idToCheck = kvpax.Key;
                        bool found = false;
                        if (checkedIds.ContainsKey(planeToCheck))
                        {
                            foreach(KeyValuePair<string, List<string>> kiwi in checkedIds[planeToCheck])
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
                    foreach(KeyValuePair<string, DCSLuaDiffsButtonElement> kvpbn in kvp.Value.keyDiffs)
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
            MergeImport(result);
        }

        static void MergeImport(Dictionary<string, Bind> res)
        {
            foreach(KeyValuePair<string, Bind> kvp in res)
            {
                string name = kvp.Key;
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
            mainW.ShowMessageBox("It appears to have successfully exported");
        }
        public static void WriteProfileClean()
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            PushAllBindsToExport(true, true, false);
            writeFiles();
            mainW.ShowMessageBox("It appears to have successfully exported");
        }
        public static void PushAllBindsToExport(bool oride, bool fillBeforeEmpty=true, bool overwriteAdd=false)
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
                        for(int i=0; i<dab.modifiers.Count; ++i)
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
        public static void OverwriteExportWith(Dictionary<string, DCSExportPlane> attr, bool overwrite = true, bool fillBeforeEmpty = true, bool overwriteAdd=false)
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvp in attr)
            {
                if ((!ToExport.ContainsKey(kvp.Key)&&!fillBeforeEmpty)||(!ToExport.ContainsKey(kvp.Key)&&fillBeforeEmpty&&!EmptyOutputs.ContainsKey(kvp.Key)))
                {
                    ToExport.Add(kvp.Key, kvp.Value.Copy());
                }
                else
                {
                    if (!ToExport.ContainsKey(kvp.Key)&&fillBeforeEmpty)
                    {
                        ToExport.Add(kvp.Key, new DCSExportPlane());
                        ToExport[kvp.Key].plane = kvp.Key;
                        foreach(KeyValuePair<string, DCSLuaInput> kvpDef in kvp.Value.joystickConfig)
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
                    foreach(KeyValuePair<string, Modifier> kMod in kvp.Value.modifiers)
                    {
                        if (!ToExport[kvp.Key].modifiers.ContainsKey(kMod.Key))
                        {
                            ToExport[kvp.Key].modifiers.Add(kMod.Key, kMod.Value);
                        }else if (overwrite)
                        {
                            ToExport[kvp.Key].modifiers[kMod.Key] = kMod.Value;
                        }
                    }
                    foreach (KeyValuePair<string, DCSLuaInput> kvpIn in kvp.Value.joystickConfig)
                    {
                        if (!ToExport[kvp.Key].joystickConfig.ContainsKey(kvpIn.Key) && fillBeforeEmpty&&EmptyOutputs.ContainsKey(kvp.Key))
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
                                else if (overwrite||defaultToOverwrite.Contains(current))
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
                                            if (!kvpDiffsButtonsElement.Value.doesAddedContainKey(old.added[i].key) && !kvpDiffsButtonsElement.Value.doesRemovedContainKey(old.added[i].key))
                                            {
                                                ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].added.Add(old.added[i].Copy());
                                            }
                                        }
                                    }
                                    for (int i = 0; i < old.removed.Count; ++i)
                                    {
                                        if (!kvpDiffsButtonsElement.Value.doesAddedContainKey(old.removed[i].key) && !kvpDiffsButtonsElement.Value.doesRemovedContainKey(old.removed[i].key))
                                        {
                                            ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].added.Add(old.removed[i].Copy());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        static string[] GetJoysticksFromCustomBinds()
        {
            List<string> SticksToBind = new List<string>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (!SticksToBind.Contains(kvp.Value.Joystick))
                    SticksToBind.Add(kvp.Value.Joystick);
            }
            return SticksToBind.ToArray();
        }
        static string[] GetPlanesFromCustomBinds()
        {
            List<string> planes = new List<string>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                Dictionary<string, int> planeCounter = kvp.Value.Rl.GetPlaneSetState();
                foreach (KeyValuePair<string, int> kvpplane in planeCounter)
                {
                    if (kvpplane.Value > 0 && !planes.Contains(kvpplane.Key))
                        planes.Add(kvpplane.Key);
                }
            }
            return planes.ToArray();
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
        public static void LoadLocalBinds(string localPath, bool fillWithDefaults=false)
        {
            LocalBinds.Clear();
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
                    LocalBinds.Add(currentPlane, current);
                    //Here load local modifiers lua
                    if(File.Exists(allSubs[i].FullName+ "\\modifiers.lua"))
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
                            if (allFiles[j].Name.Contains(".diff.lua"))
                            {
                                string stickName = allFiles[j].Name.Replace(".diff.lua", "");
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
                    foreach (KeyValuePair<string, DCSExportPlane> kvp in LocalBinds)
                    {
                        foreach(KeyValuePair<string, DCSLuaInput> kiwi in kvp.Value.joystickConfig)
                        {
                            kiwi.Value.FillUpWithDefaults();
                        }
                    }
                }
            }
            Console.WriteLine("Locals loaded lol");
            //fill up with defaults

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
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }
        public static void LoadRelations(string filePath)
        {
            if (filePath == null || filePath.Length < 1) return;
            NewFile();
            AllRelations = ReadFromBinaryFile<Dictionary<string, Relation>>(filePath);
            foreach(KeyValuePair<string, Relation> kvp in AllRelations)
            {
                kvp.Value.CheckNamesAgainstDB();
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
            foreach(KeyValuePair<string, Bind> kvp in AllBinds)
            {
                for(int i=0; i<kvp.Value.AllReformers.Count; ++i)
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
        public static void LoadProfile(string filePath)
        {
            if (filePath == null || filePath.Length < 1) return;
            Pr0file pr = ReadFromBinaryFile<Pr0file>(filePath);
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
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick != null && kvp.Value.Joystick.Length > 0)
                    if (!connectedSticks.Contains(kvp.Value.Joystick)&& !misMatches.Contains(kvp.Value.Joystick))
                        misMatches.Add(kvp.Value.Joystick);
            }
            foreach (Bind b in toRemove) if(AllBinds.ContainsKey(b.Rl.NAME)) AllBinds.Remove(b.Rl.NAME);
            foreach(string mis in misMatches)
            {
                ExchangeStick es = new ExchangeStick(mis);
                es.Show();
            }
        }
        public static void ExchangeSticksInBind(string old, string newstr)
        {
            foreach(KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick != null && kvp.Value.Joystick.Length > 0 && kvp.Value.Joystick == old&&newstr.Length>0)
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
            for(int i=0; i < li.Count; ++i)
            {
                if (!RelWOBinds.Contains(li[i]) && !relWithBinds.Contains(li[i]))
                    RelWOBinds.Add(li[i]);
            }
            switch (mainW.selectedSort)
            {
                case "Relation":
                    li = li.OrderBy(o => o.NAME).ToList();
                    break;
                case "Joystick":
                    relWithBinds = relWithBinds.OrderBy(o => AllBinds[o.NAME].Joystick).ToList();
                    RelWOBinds = RelWOBinds.OrderBy(o => o.NAME).ToList();
                    for (int i = 0; i < RelWOBinds.Count; ++i)
                    {
                        relWithBinds.Add(RelWOBinds[i]);
                    }
                    li = relWithBinds;
                    break;
            }
            SaveMetaLast();
            mainW.SetRelationsToView(li);
        }

        public static List<Modifier> GetAllModifiers()
        {
            List<Modifier> res = new List<Modifier>();
            foreach(KeyValuePair<string, Modifier> kvp in AllModifiers)
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
            string DcsPath = PROGPATH + "\\DB\\DCS";
            PopulateDCSDictionary(DcsPath + "\\axis.csv", true);
            PopulateDCSDictionary(DcsPath + "\\btn.csv", false);
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
        }
        static void PopulateDCSDictionary(string filePath, bool isAxis)
        {
            StreamReader reader = new StreamReader(filePath);
            Planes = reader.ReadLine().Split(';');
            for (int i = 1; i < Planes.Length; ++i)
                if (!DCSLib.ContainsKey(Planes[i]))
                {
                    DCSLib.Add(Planes[i], new DCSPlane(Planes[i]));
                }
            while (!reader.EndOfStream)
            {
                string[] currentLine = reader.ReadLine().Split(';');
                if (isAxis)
                {
                    for (int i = 1; i < Planes.Length; ++i)
                        if (currentLine[i].Length > 0)
                            DCSLib[Planes[i]].Axis.Add(currentLine[0], new DCSInput(currentLine[0], currentLine[i], isAxis, Planes[i]));
                }
                else
                {
                    for (int i = 1; i < Planes.Length; ++i)
                        if (currentLine[i].Length > 0)
                            DCSLib[Planes[i]].Buttons.Add(currentLine[0], new DCSInput(currentLine[0], currentLine[i], isAxis, Planes[i]));
                }
            }
            reader.Close();
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
            DCSInstances = GetDCSUserFolders();
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
