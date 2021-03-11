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

namespace JoyPro
{
    public enum Game { DCS, StarCitizen }
    public enum JoystickAxis { JOY_X, JOY_Y, JOY_Z, JOY_RX, JOY_RY, JOY_RZ, JOY_SLIDER1, JOY_SLIDER2, NONE }
    public enum LuaDataType { String, Number, Dict, Bool, Error };
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
        static Dictionary<string, DCSLuaInput> EmptyOutputs = new Dictionary<string, DCSLuaInput>();
        static Dictionary<string, DCSExportPlane> LocalBinds = new Dictionary<string, DCSExportPlane>();
        static Dictionary<string, DCSExportPlane> ToExport = new Dictionary<string, DCSExportPlane>();
        public static string selectedInstancePath = "";
        public static string lastOpenedLocation = "";

        public static void LoadMetaLast()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JoyPro\\meta.info";
            if (File.Exists(pth))
            {
                StreamReader sr = new StreamReader(pth);
                lastOpenedLocation = sr.ReadLine();
                sr.Close();
            }
        }
        public static void SaveMetaLast()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JoyPro";
            if (!Directory.Exists(pth))
            {
                Directory.CreateDirectory(pth);
            }
            StreamWriter sw = new StreamWriter(pth + "\\meta.info");
            sw.WriteLine(lastOpenedLocation);
            sw.Close();
        }
        public static void PushCleanToExportForBinds()
        {
            ToExport.Clear();
            string[] planes = GetPlanesFromCustomBinds();
            string[] sticks = GetJoysticksFromCustomBinds();

            for (int i = 0; i < planes.Length; ++i)
            {
                DCSExportPlane planeCurrent = new DCSExportPlane();
                planeCurrent.plane = planes[i];
                if (EmptyOutputs.ContainsKey(planes[i]))
                {
                    ToExport.Add(planes[i], planeCurrent);
                    for (int j = 0; j < sticks.Length; ++j)
                    {
                        planeCurrent.joystickConfig.Add(sticks[j], EmptyOutputs[planes[i]].Copy());
                        planeCurrent.joystickConfig[sticks[j]].JoystickName = sticks[j];
                    }
                }
            }
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
        public static void WriteProfileCleanNotOverwriteLocal()
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            PushCleanToExportForBinds();
            OverwriteExportWith(LocalBinds, true);
            PushAllBindsToExport(false);
            writeFiles();
            mainW.ShowMessageBox("It appears to have successfully exported");
        }
        public static void WriteProfileCleanAndLoadedOverwritten()
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            PushCleanToExportForBinds();
            OverwriteExportWith(LocalBinds, true);
            PushAllBindsToExport(true);
            writeFiles();
            mainW.ShowMessageBox("It appears to have successfully exported");
        }
        public static void WriteProfileClean()
        {
            if (!Directory.Exists(selectedInstancePath)) return;
            PushCleanToExportForBinds();
            PushAllBindsToExport(true);
            writeFiles();
            mainW.ShowMessageBox("It appears to have successfully exported");
        }
        public static void PushAllBindsToExport(bool oride)
        {
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick.Length > 0 &&
                    ((kvp.Value.Rl.ISAXIS && kvp.Value.JAxis.Length > 0) ||
                    (!kvp.Value.Rl.ISAXIS && kvp.Value.JButton.Length > 0)))
                    OverwriteExportWith(bindToExportFormat(kvp.Value), oride);
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
                        //work on the modifiers
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
        public static void OverwriteExportWith(Dictionary<string, DCSExportPlane> attr, bool overwrite = true)
        {
            foreach (KeyValuePair<string, DCSExportPlane> kvp in attr)
            {
                if (!ToExport.ContainsKey(kvp.Key))
                {
                    ToExport.Add(kvp.Key, kvp.Value);
                }
                else
                {
                    foreach(KeyValuePair<string, Modifier> kMod in kvp.Value.modifiers)
                    {
                        if (!ToExport[kvp.Key].modifiers.ContainsKey(kMod.Key))
                        {
                            ToExport[kvp.Key].modifiers.Add(kMod.Key, kMod.Value);
                            //Work for modifiers needed
                        }else if (overwrite)
                        {
                            ToExport[kvp.Key].modifiers[kMod.Key] = kMod.Value;
                        }
                    }
                    foreach (KeyValuePair<string, DCSLuaInput> kvpIn in kvp.Value.joystickConfig)
                    {
                        if (!ToExport[kvp.Key].joystickConfig.ContainsKey(kvpIn.Key))
                        {
                            ToExport[kvp.Key].joystickConfig.Add(kvpIn.Key, kvpIn.Value);
                        }
                        else
                        {
                            foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvpDiffsAxisElement in kvpIn.Value.axisDiffs)
                            {
                                if (!ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs.ContainsKey(kvpDiffsAxisElement.Key))
                                {
                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs.Add(kvpDiffsAxisElement.Key, kvpDiffsAxisElement.Value);
                                }
                                else if (overwrite)
                                {
                                    DCSLuaDiffsAxisElement old = ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key];
                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key] = kvpDiffsAxisElement.Value;
                                    for (int i = 0; i < old.added.Count; ++i)
                                    {
                                        if (!kvpDiffsAxisElement.Value.doesAddedContainKey(old.added[i].key) && !kvpDiffsAxisElement.Value.doesRemovedContainKey(old.added[i].key))
                                        {
                                            ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].added.Add(old.added[i]);
                                        }
                                    }
                                    for (int i = 0; i < old.removed.Count; ++i)
                                    {
                                        if (!kvpDiffsAxisElement.Value.doesAddedContainKey(old.removed[i].key) && !kvpDiffsAxisElement.Value.doesRemovedContainKey(old.removed[i].key))
                                        {
                                            ToExport[kvp.Key].joystickConfig[kvpIn.Key].axisDiffs[kvpDiffsAxisElement.Key].removed.Add(old.removed[i]);
                                        }
                                    }
                                }
                            }
                            foreach (KeyValuePair<string, DCSLuaDiffsButtonElement> kvpDiffsButtonsElement in kvpIn.Value.keyDiffs)
                            {
                                if (!ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs.ContainsKey(kvpDiffsButtonsElement.Key))
                                {
                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs.Add(kvpDiffsButtonsElement.Key, kvpDiffsButtonsElement.Value);
                                }
                                else if (overwrite)
                                {
                                    DCSLuaDiffsButtonElement old = ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key];
                                    ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key] = kvpDiffsButtonsElement.Value;
                                    for (int i = 0; i < old.added.Count; ++i)
                                    {
                                        if (!kvpDiffsButtonsElement.Value.doesAddedContainKey(old.added[i].key) && !kvpDiffsButtonsElement.Value.doesRemovedContainKey(old.added[i].key))
                                        {
                                            ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].added.Add(old.added[i]);
                                        }
                                    }
                                    for (int i = 0; i < old.removed.Count; ++i)
                                    {
                                        if (!kvpDiffsButtonsElement.Value.doesAddedContainKey(old.removed[i].key) && !kvpDiffsButtonsElement.Value.doesRemovedContainKey(old.removed[i].key))
                                        {
                                            ToExport[kvp.Key].joystickConfig[kvpIn.Key].keyDiffs[kvpDiffsButtonsElement.Key].added.Add(old.removed[i]);
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
        public static void LoadLocalBinds(string localPath)
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
                                luaInput.AnalyzeRawLuaInput(fileContent);
                            }
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
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }
        public static void LoadRelations(string filePath)
        {
            if (filePath == null || filePath.Length < 1) return;
            AllBinds.Clear();
            AllRelations.Clear();
            AllRelations = ReadFromBinaryFile<Dictionary<string, Relation>>(filePath);
            foreach(KeyValuePair<string, Relation> kvp in AllRelations)
            {
                kvp.Value.CheckNamesAgainstDB();
            }
            ResyncRelations();
        }
        public static void LoadProfile(string filePath)
        {
            if (filePath == null || filePath.Length < 1) return;
            Pr0file pr = ReadFromBinaryFile<Pr0file>(filePath);
            AllBinds.Clear();
            AllRelations.Clear();
            AllRelations = pr.Relations;
            AllBinds = pr.Binds;
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
            for (int i = 0; i < li.Count; i++)
            {
                if (!AllRelations.ContainsKey(li[i].NAME)) AllRelations.Add(li[i].NAME, li[i]);
                for (int j = 0; j < albi.Count; ++j)
                {
                    if (albi[j].Rl == li[i])
                    {
                        AllBinds.Add(li[i].NAME, albi[j]);
                    }
                }
            }
            mainW.SetRelationsToView(li);
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

    }
}
