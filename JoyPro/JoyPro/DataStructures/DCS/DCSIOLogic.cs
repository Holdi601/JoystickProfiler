using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public static class DCSIOLogic
    {
        public static Dictionary<string, DCSLuaInput> EmptyOutputsDCS = new Dictionary<string, DCSLuaInput>();
        public static Dictionary<string, DCSLuaInput> EmptyOutputsDCSKeyboard = new Dictionary<string, DCSLuaInput>();
        public static Dictionary<string, DCSLuaInput> EmptyOutputsDCSKeyboard_BU = new Dictionary<string, DCSLuaInput>();
        public static Dictionary<string, DCSExportPlane> LocalBindsDCS = new Dictionary<string, DCSExportPlane>();
        public static Dictionary<string, DCSExportPlane> ToExportDCS = new Dictionary<string, DCSExportPlane>();
        public static List<string> defaultToOverwrite = new List<string>();
        public static Dictionary<string, string> KeyboardConversion_DCS2DX = new Dictionary<string, string>();
        public static Dictionary<string, string> KeyboardConversion_DX2DCS = new Dictionary<string, string>();

        public static void LoadKeyboardConversion()
        {
            string cfile = MainStructure.PROGPATH + "\\TOOLS\\Conversions\\DCS2DX.keyboardconversion";
            if (File.Exists(cfile))
            {
                StreamReader streamReader = new StreamReader(cfile);
                while (!streamReader.EndOfStream)
                {
                    string[] parts = streamReader.ReadLine().Split('§');
                    if (parts.Length > 1)
                    {
                        if (!KeyboardConversion_DCS2DX.ContainsKey(parts[0]))
                        {
                            KeyboardConversion_DCS2DX.Add(parts[0], parts[1]);
                            KeyboardConversion_DX2DCS.Add(parts[1], parts[0]);
                        }
                    }
                }
                streamReader.Close();
                streamReader.Dispose();
            }
        }
        public static void CopyKeyboardDataToBackup()
        {
            EmptyOutputsDCSKeyboard_BU = new Dictionary<string, DCSLuaInput>();
            foreach (KeyValuePair<string, DCSLuaInput> kvp in EmptyOutputsDCSKeyboard)
            {
                EmptyOutputsDCSKeyboard_BU.Add(kvp.Key, kvp.Value.Copy());
            }
        }
        public static void ReinstateKeyboardData()
        {
            EmptyOutputsDCSKeyboard = EmptyOutputsDCSKeyboard_BU;
            EmptyOutputsDCSKeyboard_BU = null;
        }

        public static Dictionary<string, List<string>> GetAllKeyboardKeysForPlaneInUse(List<Bind> allBinds)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            for (int i = 0; i < allBinds.Count; ++i)
            {
                if (allBinds[i].Joystick != "Keyboard") continue;
                allBinds[i].AllReformers.Sort();
                string keyString = allBinds[i].Rl.ISAXIS ? allBinds[i].JAxis.ToLower() : allBinds[i].JButton.ToLower();
                for (int j = 0; j < allBinds[i].AllReformers.Count; ++j)
                {
                    keyString = keyString + "-" + allBinds[i].AllReformers[j].Split('§')[2].ToLower();
                }
                List<RelationItem> allri = allBinds[i].Rl.AllRelations();
                for (int j = 0; j < allri.Count; ++j)
                {
                    foreach (string plane in allri[j].GetActiveAircraftList())
                    {
                        if (!result.ContainsKey(plane))
                        {
                            result.Add(plane, new List<string>());
                        }
                        if (!result[plane].Contains(keyString)) result[plane].Add(keyString);
                    }
                }
            }
            return result;
        }

        public static void RemoveKeyboardKeyssInUse(Dictionary<string, List<string>> completeListOfKeys)
        {
            //Plane - buttons 
            foreach (KeyValuePair<string, DCSLuaInput> kvp in EmptyOutputsDCSKeyboard)
            {
                if (completeListOfKeys.ContainsKey(kvp.Key))
                {
                    for (int i = 0; i < kvp.Value.axisDiffs.Values.Count; i++)
                    {
                        List<int> toRemove = new List<int>();
                        for (int j = 0; j < kvp.Value.axisDiffs.Values.ElementAt(i).removed.Count; j++)
                        {
                            string toCompare = kvp.Value.axisDiffs.Values.ElementAt(i).removed.ElementAt(j).key.ToLower();
                            kvp.Value.axisDiffs.Values.ElementAt(i).removed.ElementAt(j).reformers.Sort();
                            for (int k = 0; k < kvp.Value.axisDiffs.Values.ElementAt(i).removed.ElementAt(j).reformers.Count; ++k)
                            {
                                toCompare = toCompare + "-" + kvp.Value.axisDiffs.Values.ElementAt(i).removed.ElementAt(j).reformers[k].ToLower();
                            }
                            if (!completeListOfKeys[kvp.Key].Contains(toCompare))
                            {
                                toRemove.Add(j);
                            }
                        }
                        for (int j = toRemove.Count - 1; j >= 0; j--)
                        {
                            kvp.Value.axisDiffs.Values.ElementAt(i).removed.RemoveAt(toRemove[j]);
                        }
                    }
                    for (int i = 0; i < kvp.Value.keyDiffs.Values.Count; i++)
                    {
                        List<int> toRemove = new List<int>();
                        for (int j = 0; j < kvp.Value.keyDiffs.Values.ElementAt(i).removed.Count; j++)
                        {
                            string toCompare = kvp.Value.keyDiffs.Values.ElementAt(i).removed.ElementAt(j).key.ToLower();
                            kvp.Value.keyDiffs.Values.ElementAt(i).removed.ElementAt(j).reformers.Sort();
                            for (int k = 0; k < kvp.Value.keyDiffs.Values.ElementAt(i).removed.ElementAt(j).reformers.Count; ++k)
                            {
                                toCompare = toCompare + "-" + kvp.Value.keyDiffs.Values.ElementAt(i).removed.ElementAt(j).reformers[k].ToLower();
                            }
                            if (!completeListOfKeys[kvp.Key].Contains(toCompare))
                            {
                                toRemove.Add(j);
                            }
                        }
                        for (int j = toRemove.Count - 1; j >= 0; j--)
                        {
                            kvp.Value.keyDiffs.Values.ElementAt(i).removed.RemoveAt(toRemove[j]);
                        }
                    }
                }
            }
        }

        public static void LoadLocalDefaultsDCS()
        {
            string install = InitGames.GetDCSInstallationPath();
            string further = "\\Input";
            string modPaths = "\\Mods\\aircraft";
            if (!Directory.Exists(install) || !Directory.Exists(install + modPaths)) return;
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
                if (Directory.Exists(MiscGames.DCSselectedInstancePath + modPaths))
                {
                    DirectoryInfo dirInstance = new DirectoryInfo(MiscGames.DCSselectedInstancePath + modPaths);
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
            MainStructure.Write("Load default DCS Inputs");
            if (File.Exists(MainStructure.PROGPATH + "\\CleanProfile\\DCS\\clean.cf"))
            {
                File.Delete(MainStructure.PROGPATH + "\\CleanProfile\\DCS\\clean.cf");
            }
            if (File.Exists(MainStructure.PROGPATH + "\\CleanProfile\\DCS\\backup.cf"))
            {
                File.Delete(MainStructure.PROGPATH + "\\CleanProfile\\DCS\\backup.cf");
            }
            DirectoryInfo dirInf = new DirectoryInfo(MainStructure.PROGPATH + "\\CleanProfile\\DCS");
            FileInfo[] fileInfos = dirInf.GetFiles();
            foreach (FileInfo file in fileInfos)
            {
                MainStructure.Write("Load default DCS: " + file.FullName);
                StreamReader sr = new StreamReader(file.FullName);
                DCSLuaInput curPlane = null;
                string content = sr.ReadToEnd();
                content = content.Replace("\\\"", "");
                string plane = file.Name.Replace(".cf", "");
                curPlane = new DCSLuaInput();
                if (!EmptyOutputsDCS.ContainsKey(plane))
                {
                    EmptyOutputsDCS.Add(plane, curPlane);
                    curPlane.plane = plane;
                    curPlane.JoystickName = "EMPTY";
                    try
                    {
                        curPlane.AnalyzeRawLuaInput(content);
                    }
                    catch (Exception ex)
                    {
                        MainStructure.NoteError(ex);
                        EmptyOutputsDCS.Remove(plane);
                    }

                    sr.Close();
                    sr.Dispose();
                }
            }

            MainStructure.Write("Clean Joy Data loaded");
        }

        public static void LoadCleanLuasDCSKeyboard()
        {
            DirectoryInfo dirInf = new DirectoryInfo(MainStructure.PROGPATH + "\\KeyboardCleanProfile\\DCS");
            FileInfo[] fileInfos = dirInf.GetFiles();
            foreach (FileInfo file in fileInfos)
            {
                StreamReader sr = new StreamReader(file.FullName);
                DCSLuaInput curPlane = null;
                string content = sr.ReadToEnd();
                string plane = file.Name.Replace(".cf", "");
                curPlane = new DCSLuaInput();
                if (!EmptyOutputsDCSKeyboard.ContainsKey(plane))
                {
                    EmptyOutputsDCSKeyboard.Add(plane, curPlane);
                    curPlane.plane = plane;
                    curPlane.JoystickName = "EMPTY";
                    try
                    {
                        curPlane.AnalyzeRawLuaInput(content);
                    }
                    catch (Exception ex)
                    {
                        MainStructure.NoteError(ex);
                        EmptyOutputsDCSKeyboard.Remove(plane);
                    }

                    sr.Close();
                    sr.Dispose();
                }
            }

            MainStructure.Write("Clean Joy Data Keyboard loaded");
        }
        public static void CorrectExportForAddedRemoved(List<string> planes)
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvpExpPlane in ToExportDCS)
            {
                if (!planes.Contains(kvpExpPlane.Key)) continue;
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
                                if (DCSIOLogic.EmptyOutputsDCS.ContainsKey(kvpExpPlane.Key))
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
        public static void OverwriteDCSExportWith(Dictionary<string, DCSExportPlane> attr, List<string> planes, bool overwrite = true, bool fillBeforeEmpty = true, bool overwriteAdd = false)
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvp in attr)
            {
                if (!planes.Contains(kvp.Key)) continue;
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
                            if (!ToExportDCS[kvp.Key].joystickConfig.ContainsKey(kvpDef.Key) && EmptyOutputsDCSKeyboard.ContainsKey(kvp.Key) && kvpDef.Key == "Keyboard")
                            {
                                ToExportDCS[kvp.Key].joystickConfig.Add(kvpDef.Key, EmptyOutputsDCSKeyboard[kvp.Key].Copy());
                                ToExportDCS[kvp.Key].joystickConfig[kvpDef.Key].JoystickName = kvpDef.Key;
                                ToExportDCS[kvp.Key].joystickConfig[kvpDef.Key].plane = kvp.Key;
                                string toCheck = kvpDef.Key + "§" + kvp.Key;
                                if (!defaultToOverwrite.Contains(toCheck)) defaultToOverwrite.Add(toCheck);
                            }
                            else if (!ToExportDCS[kvp.Key].joystickConfig.ContainsKey(kvpDef.Key) && EmptyOutputsDCS.ContainsKey(kvp.Key) && kvpDef.Key != "Keyboard")
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
                        if (!ToExportDCS[kvp.Key].joystickConfig.ContainsKey(kvpIn.Key) && fillBeforeEmpty && EmptyOutputsDCSKeyboard.ContainsKey(kvp.Key) && kvpIn.Key == "Keyboard")
                        {
                            ToExportDCS[kvp.Key].joystickConfig.Add(kvpIn.Key, EmptyOutputsDCSKeyboard[kvp.Key].Copy());
                            ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].JoystickName = kvpIn.Key;
                            ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].plane = kvp.Key;
                            string toCheck = kvpIn.Key + "§" + kvp.Key;
                            if (!defaultToOverwrite.Contains(toCheck)) defaultToOverwrite.Add(toCheck);
                        }
                        else if (!ToExportDCS[kvp.Key].joystickConfig.ContainsKey(kvpIn.Key) && fillBeforeEmpty && EmptyOutputsDCS.ContainsKey(kvp.Key) && kvpIn.Key != "Keyboard")
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
                                        if (kvpIn.Key == "Keyboard")
                                        {
                                            //Lol tastatur hat keine Achsen
                                        }
                                        else
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
                                        if (EmptyOutputsDCSKeyboard.ContainsKey(kvp.Key) && EmptyOutputsDCSKeyboard[kvp.Key].keyDiffs.ContainsKey(kvpDiffsButtonsElement.Key) && kvpIn.Key == "Keyboard")
                                        {
                                            bool found = false;
                                            for (int r = 0; r < EmptyOutputsDCSKeyboard[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Count; ++r)
                                            {
                                                for (int w = 0; w < kvpDiffsButtonsElement.Value.added.Count; ++w)
                                                {
                                                    if (kvpDiffsButtonsElement.Value.added[w].key == EmptyOutputsDCSKeyboard[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed[r].key)
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
                                                for (int r = 0; r < EmptyOutputsDCSKeyboard[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Count; ++r)
                                                {
                                                    ToExportDCS[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed.Add(EmptyOutputsDCSKeyboard[kvp.Key].keyDiffs[kvpDiffsButtonsElement.Key].removed[r]);
                                                }
                                            }
                                        }
                                        else if (EmptyOutputsDCS.ContainsKey(kvp.Key) && EmptyOutputsDCS[kvp.Key].keyDiffs.ContainsKey(kvpDiffsButtonsElement.Key) && kvpIn.Key != "Keyboard")
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
            CorrectExportForAddedRemoved(planes);
        }
        public static Dictionary<string, DCSExportPlane> BindToExportFormatDCS(Bind b, List<string> planes)
        {
            Dictionary<string, int> pstate = b.Rl.GetPlaneSetState("DCS");
            Dictionary<string, DCSExportPlane> result = new Dictionary<string, DCSExportPlane>();
            foreach (KeyValuePair<string, int> kvpPS in pstate)
            {
                if (kvpPS.Value > 0 && planes.Contains(kvpPS.Key))
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
                        for (int i = 0; i < dab.modifiers.Count; ++i)
                        {
                            if (!result[kvpPS.Key].modifiers.ContainsKey(dab.modifiers[i].name))
                                result[kvpPS.Key].modifiers.Add(dab.modifiers[i].name, dab.modifiers[i]);
                        }
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
        public static void PushAllDCSBindsToExport(bool oride, List<string> planes, bool overwriteAdd = false, List<Bind> manual = null)
        {
            if (MainStructure.msave.KeepKeyboardDefaults == true)
                CopyKeyboardDataToBackup();
            if (manual == null)
            {
                if (MainStructure.msave.KeepKeyboardDefaults == true)
                {
                    List<Bind> allB = new List<Bind>();
                    foreach (KeyValuePair<string, Bind> kvp in InternalDataManagement.AllBinds)
                    {
                        allB.Add(kvp.Value);
                    }
                    Dictionary<string, List<string>> KeyboardKeysInUse = GetAllKeyboardKeysForPlaneInUse(allB);
                    RemoveKeyboardKeyssInUse(KeyboardKeysInUse);
                }
                foreach (KeyValuePair<string, Bind> kvp in InternalDataManagement.AllBinds)
                {
                    if (kvp.Value.Joystick.Length > 0 &&
                        ((kvp.Value.Rl.ISAXIS && kvp.Value.JAxis.Length > 0) ||
                        (!kvp.Value.Rl.ISAXIS && kvp.Value.JButton.Length > 0)))
                        OverwriteDCSExportWith(BindToExportFormatDCS(kvp.Value, planes), planes, oride, true, overwriteAdd);
                }
            }
            else
            {
                if (MainStructure.msave.KeepKeyboardDefaults == true)
                {
                    Dictionary<string, List<string>> KeyboardKeysInUse = GetAllKeyboardKeysForPlaneInUse(manual);
                    RemoveKeyboardKeyssInUse(KeyboardKeysInUse);
                }
                for (int i = 0; i < manual.Count; i++)
                {
                    if (manual[i].Joystick.Length > 0 &&
                        ((manual[i].Rl.ISAXIS && manual[i].JAxis.Length > 0) ||
                        (!manual[i].Rl.ISAXIS && manual[i].JButton.Length > 0)))
                        OverwriteDCSExportWith(BindToExportFormatDCS(manual[i], planes), planes, oride, true, overwriteAdd);
                }
            }
            if (MainStructure.msave.KeepKeyboardDefaults == true)
                ReinstateKeyboardData();
        }
        public static void NukeUnusedButConnectedDevicesToExport(List<string> planes)
        {
            foreach (string g in MiscGames.Games)
            {
                if (g == "DCS")
                {
                    string[] allPlanes = DBLogic.Planes[g].ToArray();
                    Dictionary<string, string> connectedSticks = JoystickReader.GetConnectedJoysticks();
                    for (int i = 0; i < allPlanes.Length; ++i)
                    {
                        if (planes.Contains(allPlanes[i]))
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
                                if (connectedSticks.ElementAt(j).Key == "Keyboard") continue;
                                if (!ToExportDCS[allPlanes[i]].joystickConfig.ContainsKey(connectedSticks.ElementAt(j).Key))
                                {
                                    ToExportDCS[allPlanes[i]].joystickConfig.Add(connectedSticks.ElementAt(j).Key, empty.Copy());
                                    ToExportDCS[allPlanes[i]].joystickConfig[connectedSticks.ElementAt(j).Key].plane = allPlanes[i];
                                    ToExportDCS[allPlanes[i]].joystickConfig[connectedSticks.ElementAt(j).Key].JoystickName = connectedSticks.ElementAt(j).Key;
                                }
                            }
                        }
                    }
                }
            }

        }
        public static void WriteProfileCleanAndLoadedOverwrittenAndAdd(List<string> Planes, List<Bind> manualList = null)
        {
            if (!Directory.Exists(MiscGames.DCSselectedInstancePath)) return;
            ToExportDCS.Clear();
            defaultToOverwrite = new List<string>();
            LoadLocalBinds(MiscGames.DCSselectedInstancePath, Planes, true);
            OverwriteDCSExportWith(LocalBindsDCS, Planes, true, false, false);
            PushAllDCSBindsToExport(true, Planes, true, manualList);
            WriteFilesDCS(Planes);
            WriteFilesDCS(Planes, ".jp");
        }
        public static void WriteProfileClean(bool nukeDevices, List<string> Planes, List<Bind> manualList = null)
        {
            if (!Directory.Exists(MiscGames.DCSselectedInstancePath)) return;
            ToExportDCS.Clear();
            PushAllDCSBindsToExport(true, Planes, true, manualList);
            if (nukeDevices)
                NukeUnusedButConnectedDevicesToExport(Planes);
            WriteFilesDCS(Planes);
            WriteFilesDCS(Planes, ".jp");
        }
        public static void LoadLocalBinds(string localPath, List<string> planes, bool fillWithDefaults = false, string ending = ".diff.lua", Dictionary<string, DCSExportPlane> resultsDict = null)
        {
            Dictionary<string, DCSExportPlane> toOutput;
            toOutput = resultsDict == null ? LocalBindsDCS : resultsDict;
            toOutput.Clear();
            string pathToSearch = localPath + "\\Config\\Input";
            if (Directory.Exists(pathToSearch))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(pathToSearch);
                DirectoryInfo[] allSubs = dirInfo.GetDirectories();
                for (int i = 0; i < allSubs.Length; ++i)
                {
                    string currentPlane = allSubs[i].Name;
                    if (!planes.Contains(currentPlane)) continue;
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
                                if (allFiles[j].Name.ToLower().StartsWith("keyboard")) continue;
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
                    if (Directory.Exists(allSubs[i].FullName + "\\keyboard"))
                    {
                        DirectoryInfo dirPlaneJoy = new DirectoryInfo(allSubs[i].FullName + "\\keyboard");
                        FileInfo[] allFiles = dirPlaneJoy.GetFiles();
                        for (int j = 0; j < allFiles.Length; ++j)
                        {
                            if (allFiles[j].Name.Contains(ending))
                            {

                                string stickName = allFiles[j].Name.Replace(ending, "");
                                MainStructure.Write(stickName);
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
                        if (!planes.Contains(kvp.Key)) continue;
                        foreach (KeyValuePair<string, DCSLuaInput> kiwi in kvp.Value.joystickConfig)
                        {
                            kiwi.Value.FillUpWithDefaults();
                        }
                    }
                }
            }
            MainStructure.Write("Locals loaded lol");
        }
        public static void MergeImport(Dictionary<string, Bind> res)
        {
            foreach (KeyValuePair<string, Bind> kvp in res)
            {
                string name = "";
                if (kvp.Value.additionalImportInfo.Length > 1)
                    name = kvp.Value.additionalImportInfo.Split('§')[kvp.Value.additionalImportInfo.Split('§').Length - 1];
                else
                    name = kvp.Value.Rl.NAME;

                System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(@"\d\d\d\d");
                if (name.Length > 6)
                {
                    System.Text.RegularExpressions.Match match = rgx.Match(name.Substring(1, 4));
                    System.Text.RegularExpressions.Match mch = rgx.Match(name.Substring(2, 5));
                    if (match.Success || mch.Success)
                    {
                        string generatedNameGroup = "GENERATED-NAME";
                        if (!InternalDataManagement.AllGroups.Contains(generatedNameGroup))
                        {
                            InternalDataManagement.AllGroups.Add(generatedNameGroup);
                            InternalDataManagement.GroupActivity.Add(generatedNameGroup, true);
                        }
                        if (!kvp.Value.Rl.Groups.Contains(generatedNameGroup))
                        {
                            kvp.Value.Rl.Groups.Add(generatedNameGroup);
                        }
                        name = kvp.Value.Rl.GetRandomDescription();
                        kvp.Value.Rl.NAME = name;
                    }
                }

                MainStructure.Write(name);
                while (InternalDataManagement.AllRelations.ContainsKey(name))
                {
                    name += "i";
                }
                kvp.Value.Rl.NAME = name;
                InternalDataManagement.AllRelations.Add(name, kvp.Value.Rl);
                InternalDataManagement.AllBinds.Add(name, kvp.Value);
            }
            InternalDataManagement.ResyncRelations();
        }
        public static void WriteProfileCleanNotOverwriteLocal(List<string> planes, List<Bind> manualList = null)
        {
            if (!Directory.Exists(MiscGames.DCSselectedInstancePath)) return;
            ToExportDCS.Clear();
            defaultToOverwrite = new List<string>();
            LoadLocalBinds(MiscGames.DCSselectedInstancePath, planes, true);
            OverwriteDCSExportWith(LocalBindsDCS, planes, true, false, false);
            PushAllDCSBindsToExport(false, planes, false, manualList);
            WriteFilesDCS(planes);
            WriteFilesDCS(planes, ".jp");
        }
        public static void WriteProfileCleanAndLoadedOverwritten(List<string> planes, List<Bind> manualList = null)
        {
            if (!Directory.Exists(MiscGames.DCSselectedInstancePath)) return;
            ToExportDCS.Clear();
            defaultToOverwrite = new List<string>();
            LoadLocalBinds(MiscGames.DCSselectedInstancePath, planes, true);
            OverwriteDCSExportWith(LocalBindsDCS, planes, true, false, false);
            PushAllDCSBindsToExport(true, planes, false, manualList);
            WriteFilesDCS(planes);
            WriteFilesDCS(planes, ".jp");
        }
        public static void BindsFromLocal(List<string> sticks, List<string> planes, bool loadDefaults, bool inv = false, bool slid = false, bool curv = false, bool dz = false, bool sx = false, bool sy = false)
        {
            Dictionary<string, DCSExportPlane> checkAgainst = new Dictionary<string, DCSExportPlane>();
            LoadLocalBinds(MiscGames.DCSselectedInstancePath, planes, loadDefaults, ".jp", checkAgainst);
            LoadLocalBinds(MiscGames.DCSselectedInstancePath, planes, loadDefaults);
            Dictionary<string, Bind> checkRes = LibraryFromLocalDict(checkAgainst, sticks, loadDefaults, inv, slid, curv, dz, sx, sy);
            Dictionary<string, Bind> result = LibraryFromLocalDict(LocalBindsDCS, sticks, loadDefaults, inv, slid, curv, dz, sx, sy);
            foreach (KeyValuePair<string, Bind> kvp in checkRes)
            {
                InternalDataManagement.CorrectBindNames(result, kvp.Value);
            }
            MergeImport(result);
            InternalDataManagement.CorrectModifiersInBinds();
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
                                result[b.Rl.NAME].Rl.AddNode(kvpaxe.Key, "DCS", true, plane);
                                if ((result[b.Rl.NAME].additionalImportInfo == null ||
                                   result[b.Rl.NAME].additionalImportInfo.Length < 1) &&
                                   (b.additionalImportInfo != null &&
                                   b.additionalImportInfo.Length > 0))
                                {
                                    result[b.Rl.NAME].additionalImportInfo = b.additionalImportInfo;
                                }
                                for (int a = 0; a < b.Rl.Groups.Count; ++a)
                                {
                                    if (!result[b.Rl.NAME].Rl.Groups.Contains(b.Rl.Groups[a]))
                                        result[b.Rl.NAME].Rl.Groups.Add(b.Rl.Groups[a]);
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
                                result[b.Rl.NAME].Rl.AddNode(kvpbe.Key, "DCS", false, plane);
                                if ((result[b.Rl.NAME].additionalImportInfo == null ||
                                   result[b.Rl.NAME].additionalImportInfo.Length < 1) &&
                                   (b.additionalImportInfo != null &&
                                   b.additionalImportInfo.Length > 0))
                                {
                                    result[b.Rl.NAME].additionalImportInfo = b.additionalImportInfo;
                                }
                                for (int a = 0; a < b.Rl.Groups.Count; ++a)
                                {
                                    if (!result[b.Rl.NAME].Rl.Groups.Contains(b.Rl.Groups[a]))
                                        result[b.Rl.NAME].Rl.Groups.Add(b.Rl.Groups[a]);
                                }
                            }
                        }
                    }
                }
            }
            if (loadDefaults)
            {
                foreach (KeyValuePair<string, DCSLuaInput> kvp in EmptyOutputsDCSKeyboard)
                {
                    string planeToCheck = kvp.Key;
                    foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvpax in kvp.Value.axisDiffs)
                    {
                        string idToCheck = kvpax.Key;
                        bool found = false;
                        if (checkedIds.ContainsKey(planeToCheck))
                        {
                            if (checkedIds[planeToCheck].ContainsKey("Keyboard"))
                            {
                                string joystick = "Keyboard";
                                found = checkedIds[planeToCheck]["Keyboard"].Contains(idToCheck);
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
                                            result[b.Rl.NAME].Rl.AddNode(idToCheck, "DCS", true, planeToCheck);
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
                        string joystick = "Keyboard";
                        if (checkedIds.ContainsKey(planeToCheck) && checkedIds[planeToCheck].ContainsKey(joystick))
                        {
                            found = checkedIds[planeToCheck]["Keyboard"].Contains(idToCheck);
                            if (!found)
                            {
                                if (kvpbn.Value.removed.Count > 0)
                                {
                                    if (joystick == "Keyboard" && kvpbn.Value.removed[0].key.ToLower().StartsWith("joy")) continue;
                                    Bind b = Bind.GetBindFromButtonElement(kvpbn.Value.removed[0], idToCheck, joystick, planeToCheck);
                                    if (!result.ContainsKey(b.Rl.NAME))
                                    {
                                        result.Add(b.Rl.NAME, b);
                                    }
                                    else
                                    {
                                        result[b.Rl.NAME].Rl.AddNode(idToCheck, "DCS", false, planeToCheck);
                                    }
                                }
                            }
                        }
                    }
                }
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
                                if (joystick == "Keyboard") continue;
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
                                            result[b.Rl.NAME].Rl.AddNode(idToCheck, "DCS", true, planeToCheck);
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
                                        if (joystick == "Keyboard" && kvpbn.Value.removed[0].key.ToLower().StartsWith("joy")) continue;
                                        Bind b = Bind.GetBindFromButtonElement(kvpbn.Value.removed[0], idToCheck, joystick, planeToCheck);
                                        if (!result.ContainsKey(b.Rl.NAME))
                                        {
                                            result.Add(b.Rl.NAME, b);
                                        }
                                        else
                                        {
                                            result[b.Rl.NAME].Rl.AddNode(idToCheck, "DCS", false, planeToCheck);
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
        public static void WriteFilesDCS(List<string> planes, string endingDCS = ".diff.lua")
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvp in ToExportDCS)
            {
                if (!planes.Contains(kvp.Key)) continue;
                string modPath = MiscGames.DCSselectedInstancePath + "\\Config\\Input\\" + kvp.Key;
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
                    if (kvJoy.Key == "Keyboard")
                    {
                        string keypth = modPath + "\\keyboard\\";
                        if (!Directory.Exists(keypth)) Directory.CreateDirectory(keypth);
                        kvJoy.Value.writeLua(keypth, outputName, endingDCS);
                    }
                    else
                    {
                        if (!Directory.Exists(adjustedPath)) Directory.CreateDirectory(adjustedPath);
                        kvJoy.Value.writeLua(adjustedPath, outputName, endingDCS);
                    }
                }
            }
        }

    }
}
