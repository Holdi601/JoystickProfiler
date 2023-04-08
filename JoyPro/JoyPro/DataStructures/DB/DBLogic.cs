﻿using JoyPro.StarCitizen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public static class DBLogic
    {
        public static Dictionary<string, DCSPlane> DCSLib = new Dictionary<string, DCSPlane>();
        public static Dictionary<string, Dictionary<string, OtherGame>> OtherLib = new Dictionary<string, Dictionary<string, OtherGame>>();
        public static Dictionary<string, List<string>> Planes = new Dictionary<string, List<string>>();
        public static ManualDatabaseAdditions ManualDatabase=null;
        public static List<HtmlBindElement> GetBindElmentsFromCell(string cellContent)
        {
            List<HtmlBindElement> result = new List<HtmlBindElement>();
            int dashIndex = cellContent.IndexOf("- ");
            string[] parts = cellContent.Split(new string[] { "; " }, StringSplitOptions.None);
            for (int i = 0; i < parts.Length; ++i)
            {
                string[] bindParts = parts[i].Split(new string[] { "- " }, StringSplitOptions.None);
                HtmlBindElement current = new HtmlBindElement();
                current.button = bindParts[bindParts.Length - 1];
                current.reformers = new List<string>();
                for (int j = 0; j < bindParts.Length - 1; ++j)
                {
                    current.reformers.Add(bindParts[j]);
                }
            }
            return result;
        }
        public static List<HtmlInputElementDCS> GetElementsFromHTMLDCS(string file)
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
                    string cleanedLine = currentLine.Replace("\t", "").Replace("<td>", "").Replace("</td>", "").Replace("  ", "").Replace("\"","").Trim();
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
        public static void UpdateSCDictionary()
        {
            string addativePathToActions = "LIVE\\USER\\Client\\0\\Profiles\\default\\actionmaps.xml";
            string gameName = "StarCitizen";
            StreamReader sr = new StreamReader(MiscGames.StarCitizen + "\\" + addativePathToActions);
            string lastCat = "";
            string catString = "<actionmap name=\"";
            string idString = "<action name=\"";
            string commentStart = "<!--";
            string id = "";
            OtherGameInput current = null;
            bool SCDBUpdateNeeded = false;
            while (!sr.EndOfStream)
            {
                string rawLine = sr.ReadLine();
                if (rawLine.Contains(catString))
                {
                    string prev = lastCat;
                    lastCat = rawLine.Substring(rawLine.IndexOf(catString) + catString.Length);
                    lastCat = lastCat.Substring(0, lastCat.IndexOf("\""));
                    if(prev!=lastCat)
                    {
                        if(!SCIOLogic.categoryOrder.ContainsKey(lastCat))
                        {
                            int order = 0;
                            if (SCIOLogic.categoryOrder.ContainsKey(prev))
                            {
                                order= SCIOLogic.categoryOrder[prev];
                            }
                            order += 1;
                            SCIOLogic.categoryOrder.Add(lastCat, order);
                        }
                    }
                }
                if (rawLine.Contains(idString) && rawLine.Contains(commentStart))
                {
                    string pv = id;
                    id = rawLine.Substring(rawLine.IndexOf(idString) + idString.Length);
                    id = id.Substring(0, id.IndexOf("\""));
                    string description = id;
                    bool axis = false;
                    string dictkey = lastCat + "___" + id;
                    id = dictkey;
                    current = new OtherGameInput(id, description, axis, gameName, gameName, lastCat);
                    if (!OtherLib[gameName][gameName].Buttons.ContainsKey(dictkey))
                    {
                        OtherLib[gameName][gameName].Buttons.Add(dictkey, current);
                        SCDBUpdateNeeded = true;
                    }
                    if(pv!=id)
                    {
                        if(!SCIOLogic.idOrder.ContainsKey(id))
                        {
                            int mot = 0;
                            if(SCIOLogic.idOrder.ContainsKey(pv))
                            {
                                mot = SCIOLogic.idOrder[pv];
                            }
                            mot = mot + 1;
                            SCIOLogic.idOrder.Add(id, mot);
                        }
                    }
                }
            }
            if(SCDBUpdateNeeded)
            {
                MainStructure.Write("SC DB Update needed");
                UpdateSCDBFile();
            }
            sr.Close();
        }
        public static void UpdateSCDBFile()
        {
            string fileActionPath = MainStructure.PROGPATH + "\\DB\\StarCitizen\\AllBinds.xml";
            string gameName = "StarCitizen";
            string lastCat = "";
            string xmlend = "\">";
            string xmlcellend = "\"/>";
            string le = "\r\n";
            string catString = "<actionmap name=\"";
            string catEnd = "</actionmap>";
            string idString = "<action name=\"";
            string idEnd = "</action>";
            string commentStart = "<!--";
            string commentEnd = "-->";
            string defInput = " defaultInput=\"";
            string rebinds = "<rebind";
            string rebindstr = " input=\"";
            string id = "";
            string mouseStart = "mo";
            string keyboardStart = "kb";
            string joystickStart = "js";
            string gamepadStart = "gp";
            StreamWriter swr = new StreamWriter(fileActionPath);
            var sortedList = SCIOLogic.idOrder.OrderBy(x => x.Value).Select(x => x.Key).ToList();
            foreach(string cstr in sortedList)
            {
                string[] prts = MainStructure.SplitBy(cstr, "___");
                string ncat = prts[0];
                string nid = prts[1];
                OtherGameInput ogi = null;
                if (OtherLib[gameName][gameName].Axis.ContainsKey(cstr))
                {
                    ogi = OtherLib[gameName][gameName].Axis[cstr];
                }
                else
                {
                    ogi = OtherLib[gameName][gameName].Buttons[cstr];
                }
                if (ncat!=lastCat)
                {
                    if(lastCat.Length>0)
                    {
                        swr.WriteLine(catEnd);
                    }
                    string loine = catString + ogi.Category + xmlend;
                    swr.WriteLine(loine);
                }
                string actline=idString+nid+xmlend+commentStart+ogi.Title+"//"+(ogi.IsAxis?"True":"False")+commentEnd;
                swr.WriteLine(actline);
                if (ogi.default_gamepad.Length < 1 && ogi.default_joystick.Length < 1 && ogi.default_keyboard.Length < 1)
                {
                    string rbstr = rebinds + (ogi.default_input.Length < 1 ? "" : (defInput + ogi.default_input + "\"")) + rebindstr + keyboardStart + "1_ " + xmlcellend;
                    swr.WriteLine(rbstr);
                }
                else
                {
                    if (ogi.default_keyboard.Length > 0)
                    {
                        string mookb = (ogi.default_keyboard.Contains("maxis") || ogi.default_keyboard.Contains("mouse")) ? mouseStart : keyboardStart;
                        string rbstr = rebinds + (ogi.default_input.Length < 1 ? "" : (defInput + ogi.default_input + "\"")) + rebindstr + mookb + "1_" + ogi.default_keyboard + xmlcellend;
                        swr.WriteLine(rbstr);
                    }
                    if (ogi.default_gamepad.Length > 0)
                    {
                        string rbstr = rebinds + rebindstr + gamepadStart + "1_" + ogi.default_gamepad + xmlcellend;
                        swr.WriteLine(rbstr);
                    }
                    if (ogi.default_joystick.Length > 0)
                    {
                        string rbstr = rebinds + rebindstr + joystickStart + "1_" + ogi.default_joystick + xmlcellend;
                        swr.WriteLine(rbstr);
                    }
                }
                swr.WriteLine(idEnd);
            }
            swr.WriteLine(catEnd);
            swr.Flush();
            swr.Close();
            swr.Dispose();
        }
        public static void PopulateSCDictionary()
        {
            SCIOLogic.categoryOrder = new Dictionary<string, int>();
            SCIOLogic.idOrder = new Dictionary<string, int>();
            string fileActionPath = MainStructure.PROGPATH + "\\DB\\StarCitizen\\AllBinds.xml";
            string gameName = "StarCitizen";
            if (File.Exists(fileActionPath))
            {
                if (!OtherLib.ContainsKey(gameName))
                {
                    OtherLib.Add(gameName, new Dictionary<string, OtherGame>());
                    OtherLib[gameName].Add(gameName, new OtherGame(gameName, gameName, true));
                }
                if (!Planes["StarCitizen"].Contains(gameName))
                    Planes["StarCitizen"].Add(gameName);
                StreamReader sr = new StreamReader(fileActionPath);
                string lastCat = "";
                string catString = "<actionmap name=\"";
                string idString = "<action name=\"";
                string commentStart = "<!--";
                string commentEnd = "-->";
                string defInput = " defaultInput=\"";
                string rebinds = "<rebind";
                string rebindstr = " input=\"";
                string id = "";
                string mouseStart = "mo";
                string keyboardStart = "kb";
                string joystickStart = "js";
                string gamepadStart = "gp";
                OtherGameInput current=null;
                while (!sr.EndOfStream)
                {
                    string rawLine = sr.ReadLine();
                    if (rawLine.Contains(catString))
                    {
                        string prev = lastCat;
                        lastCat = rawLine.Substring(rawLine.IndexOf(catString) + catString.Length);
                        lastCat = lastCat.Substring(0, lastCat.IndexOf("\""));
                        if(prev!=lastCat)
                        {
                            int order = (SCIOLogic.categoryOrder.Count + 1) * 100;
                            if (!SCIOLogic.categoryOrder.ContainsKey(lastCat))
                                SCIOLogic.categoryOrder.Add(lastCat, order);
                        }
                    }
                    if (rawLine.Contains(idString)&&rawLine.Contains(commentStart))
                    {
                        id = rawLine.Substring(rawLine.IndexOf(idString) + idString.Length);
                        id = id.Substring(0, id.IndexOf("\""));

                        string comment = rawLine.Substring(rawLine.IndexOf(commentStart) + commentStart.Length);
                        comment = comment.Substring(0, comment.IndexOf(commentEnd));
                        string[] parts = MainStructure.SplitBy(comment, "//");
                        string description = id;
                        bool axis = false;
                        if(parts.Length>1)
                        {
                            description = parts[0];
                            axis = parts[1].Trim().ToLower() == "true" ? true : false;
                        }
                        string dictkey = lastCat + "___" + id;
                        id = dictkey;
                        int idOrder = (SCIOLogic.idOrder.Count + 1) * 100;
                        if(!SCIOLogic.idOrder.ContainsKey(dictkey))
                        {
                            SCIOLogic.idOrder.Add(dictkey, idOrder);
                        }
                        current = new OtherGameInput(id, description, axis, gameName, gameName, lastCat);
                        if (axis)
                        {
                            if (!OtherLib[gameName][gameName].Axis.ContainsKey(dictkey))
                                OtherLib[gameName][gameName].Axis.Add(dictkey, current);
                        }
                        else
                        {
                            if (!OtherLib[gameName][gameName].Buttons.ContainsKey(dictkey))
                                OtherLib[gameName][gameName].Buttons.Add(dictkey, current);
                        }
                    }
                    if (rawLine.Contains(rebindstr))
                    {
                        string defin = "";
                        if(rawLine.Contains(defInput))
                        {
                            defin = rawLine.Substring(rawLine.IndexOf(defInput)+defInput.Length);
                            defin=defin.Substring(0, defin.IndexOf("\""));
                        }
                        string input = rawLine.Substring(rawLine.IndexOf(rebindstr) + rebindstr.Length);
                        input = input.Substring(0, input.IndexOf("\""));
                        string[] splitted = input.Split('_');
                        if (splitted.Length > 1 && splitted[1].Trim().Length > 0)
                        {
                            if(input.StartsWith(mouseStart) || input.StartsWith(keyboardStart))
                            {
                                current.default_keyboard = input;
                            }else if(input.StartsWith(gamepadStart))
                            {
                                current.default_gamepad = input;
                            }else if(input.StartsWith(joystickStart))
                            {
                                current.default_joystick = input;
                            }
                        }
                        if(defin.Length>0)current.default_input = defin;
                    }
                }
                sr.Close();
            }
        }
        public static void PopulateIL2Dictionary()
        {
            string fileActionPath = MainStructure.PROGPATH + "\\DB\\IL2\\IL2.actions";
            string gameName = "IL2Game";
            if (File.Exists(fileActionPath))
            {
                if (!OtherLib.ContainsKey(gameName))
                {
                    OtherLib.Add(gameName, new Dictionary<string, OtherGame>());
                    OtherLib[gameName].Add(gameName, new OtherGame(gameName, gameName, true));
                }
                if (!Planes["IL2Game"].Contains(gameName))
                    Planes["IL2Game"].Add(gameName);
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
                        OtherGameInput current = new OtherGameInput(id, descriptor, axis, gameName, gameName, "");
                        if (axis)
                        {
                            if (!OtherLib[gameName][gameName].Axis.ContainsKey(id))
                                OtherLib[gameName][gameName].Axis.Add(id, current);
                            if (!OtherLib[gameName][gameName].Buttons.ContainsKey(id + "+"))
                            {
                                OtherGameInput currentAxisPlus = new OtherGameInput(id + "+", descriptor + " (BUTTON PLUS)", false, gameName, gameName, "");
                                OtherGameInput currentAxisMinus = new OtherGameInput(id + "-", descriptor + " (BUTTON MINUS)", false, gameName, gameName, "");
                                OtherLib[gameName][gameName].Buttons.Add(id + "+", currentAxisPlus);
                                OtherLib[gameName][gameName].Buttons.Add(id + "-", currentAxisMinus);
                            }
                        }
                        else
                        {
                            if (!OtherLib[gameName][gameName].Buttons.ContainsKey(id))
                                OtherLib[gameName][gameName].Buttons.Add(id, current);
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
                else tempPlanes = Planes["DCS"];
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
                                if (!DCSIOLogic.EmptyOutputsDCS.ContainsKey(currentPlane))
                                {
                                    ReadDefaultsFromHTML(currentPlane, filesInDir[j].FullName);
                                }
                            }


                        }
                    }
                }
                Planes["DCS"] = tempPlanes;
            }

        }

        public static void AddItemToManualDB(bool ax, string plane, string game, string id, string description, string category)
        {
            if (game == "DCS")
            {
                if (plane == "ALL")
                {
                    for (int i = 0; i < DBLogic.Planes["DCS"].Count; ++i)
                    {
                        plane = DBLogic.Planes["DCS"][i];
                        DCSInput di = new DCSInput(id, description, ax, plane, category);
                        if (DBLogic.ManualDatabase == null) DBLogic.ManualDatabase = new ManualDatabaseAdditions();
                        if (!DBLogic.ManualDatabase.DCSLib.ContainsKey(plane))
                            DBLogic.ManualDatabase.DCSLib.Add(plane, new DCSPlane(plane));
                        if (ax)
                        {
                            if (!DBLogic.ManualDatabase.DCSLib[plane].Axis.ContainsKey(di.ID))
                                DBLogic.ManualDatabase.DCSLib[plane].Axis.Add(di.ID, di);
                        }
                        else
                        {
                            if (!DBLogic.ManualDatabase.DCSLib[plane].Buttons.ContainsKey(di.ID))
                                DBLogic.ManualDatabase.DCSLib[plane].Buttons.Add(di.ID, di);
                        }
                    }
                }
                else
                {
                    DCSInput di = new DCSInput(id, description, ax, plane, category);
                    if (DBLogic.ManualDatabase == null) DBLogic.ManualDatabase = new ManualDatabaseAdditions();
                    if (!DBLogic.ManualDatabase.DCSLib.ContainsKey(plane))
                        DBLogic.ManualDatabase.DCSLib.Add(plane, new DCSPlane(plane));
                    if (ax)
                    {
                        if (!DBLogic.ManualDatabase.DCSLib[plane].Axis.ContainsKey(di.ID))
                            DBLogic.ManualDatabase.DCSLib[plane].Axis.Add(di.ID, di);
                    }
                    else
                    {
                        if (!DBLogic.ManualDatabase.DCSLib[plane].Buttons.ContainsKey(di.ID))
                            DBLogic.ManualDatabase.DCSLib[plane].Buttons.Add(di.ID, di);
                    }
                }
            }
            else
            {
                if (plane == "ALL")
                {
                    for (int i = 0; i < DBLogic.Planes[game].Count; ++i)
                    {
                        plane = DBLogic.Planes[game][i];
                        OtherGameInput ogi = new OtherGameInput(id, description, ax, game, plane, category);
                        if (!DBLogic.ManualDatabase.OtherLib.ContainsKey(game))
                            DBLogic.ManualDatabase.OtherLib.Add(game, new Dictionary<string, OtherGame>());
                        if (!DBLogic.ManualDatabase.OtherLib[game].ContainsKey(plane))
                            DBLogic.ManualDatabase.OtherLib[game].Add(plane, new OtherGame(plane, game, true));
                        if (ax)
                        {
                            if (!DBLogic.ManualDatabase.OtherLib[game][plane].Axis.ContainsKey(ogi.ID))
                                DBLogic.ManualDatabase.OtherLib[game][plane].Axis.Add(ogi.ID, ogi);
                        }
                        else
                        {
                            if (!DBLogic.ManualDatabase.OtherLib[game][plane].Buttons.ContainsKey(ogi.ID))
                                DBLogic.ManualDatabase.OtherLib[game][plane].Buttons.Add(ogi.ID, ogi);
                        }
                    }
                }
                else
                {
                    OtherGameInput ogi = new OtherGameInput(id, description, ax, game, plane, category);
                    if (!DBLogic.ManualDatabase.OtherLib.ContainsKey(game))
                        DBLogic.ManualDatabase.OtherLib.Add(game, new Dictionary<string, OtherGame>());
                    if (!DBLogic.ManualDatabase.OtherLib[game].ContainsKey(plane))
                        DBLogic.ManualDatabase.OtherLib[game].Add(plane, new OtherGame(plane, game, true));
                    if (ax)
                    {
                        if (!DBLogic.ManualDatabase.OtherLib[game][plane].Axis.ContainsKey(ogi.ID))
                            DBLogic.ManualDatabase.OtherLib[game][plane].Axis.Add(ogi.ID, ogi);
                    }
                    else
                    {
                        if (!DBLogic.ManualDatabase.OtherLib[game][plane].Buttons.ContainsKey(ogi.ID))
                            DBLogic.ManualDatabase.OtherLib[game][plane].Buttons.Add(ogi.ID, ogi);
                    }
                }
            }
        }

        public static void PopulateManualDictionary()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            string manualPath = pth + "\\ManualAdditions.txt";
            if (File.Exists(manualPath))
            {
                ManualDatabase = new ManualDatabaseAdditions();
                if(!File.Exists(pth+"\\ManualAdditions.txt")) return;
                StreamReader streamReader = new StreamReader(pth + "\\ManualAdditions.txt");
                while (!streamReader.EndOfStream)
                {
                    string currentLine=streamReader.ReadLine();
                    string[] parts = currentLine.Split('§');
                    if(parts.Length == 6)
                    {
                        AddItemToManualDB(Convert.ToBoolean(parts[4]), parts[2], parts[5], parts[0],parts[3],parts[1]);
                    }
                }

                streamReader.Close();
                streamReader.Dispose();
            }
            else
            {
                if (!File.Exists(pth + "\\ManualAdditions.bin")) return;
                try
                {
                    ManualDatabase = MainStructure.ReadFromBinaryFile<ManualDatabaseAdditions>(pth + "\\ManualAdditions.bin");
                }
                catch(Exception ex)
                {
                    MainStructure.NoteError(ex);
                    System.Windows.MessageBox.Show("Error loading manual database");
                    return;
                }
            }
            if (ManualDatabase == null || ManualDatabase.DCSLib == null || ManualDatabase.OtherLib == null)
            {
                ManualDatabase = new ManualDatabaseAdditions();
                return;
            }
            foreach(KeyValuePair<string, DCSPlane> kvp in ManualDatabase.DCSLib)
            {
                if (!DCSLib.ContainsKey(kvp.Key))
                {
                    DCSLib.Add(kvp.Key, kvp.Value.Copy());
                    continue;
                }
                foreach (KeyValuePair<string, DCSInput> kvpInPlane in kvp.Value.Axis)
                {
                    if(!DCSLib[kvp.Key].Axis.ContainsKey(kvpInPlane.Key))
                        DCSLib[kvp.Key].Axis.Add(kvpInPlane.Key, kvpInPlane.Value.Copy());          
                }
                foreach (KeyValuePair<string, DCSInput> kvpInPlane in kvp.Value.Buttons)
                {
                    if (!DCSLib[kvp.Key].Buttons.ContainsKey(kvpInPlane.Key))
                        DCSLib[kvp.Key].Buttons.Add(kvpInPlane.Key, kvpInPlane.Value.Copy());
                }
            }
            foreach (KeyValuePair<string, Dictionary<string, OtherGame>> kvp in ManualDatabase.OtherLib)
            {
                if (!OtherLib.ContainsKey(kvp.Key))
                {
                    OtherLib.Add(kvp.Key, new Dictionary<string, OtherGame>());
                }
                foreach(KeyValuePair<string, OtherGame> kvpInner in kvp.Value)
                {
                    if (!OtherLib[kvp.Key].ContainsKey(kvpInner.Key))
                    {
                        OtherLib[kvp.Key].Add(kvpInner.Key, kvpInner.Value.Copy());
                        continue;
                    }
                    foreach (KeyValuePair<string, OtherGameInput> kvpInPlane in kvpInner.Value.Axis)
                    {
                        if(!OtherLib[kvp.Key][kvpInner.Key].Axis.ContainsKey(kvpInPlane.Key))
                            OtherLib[kvp.Key][kvpInner.Key].Axis.Add(kvpInPlane.Key, kvpInPlane.Value.Copy());
                    }
                    foreach (KeyValuePair<string, OtherGameInput> kvpInPlane in kvpInner.Value.Buttons)
                    {
                        if (!OtherLib[kvp.Key][kvpInner.Key].Buttons.ContainsKey(kvpInPlane.Key))
                            OtherLib[kvp.Key][kvpInner.Key].Buttons.Add(kvpInPlane.Key, kvpInPlane.Value.Copy());
                    }
                }
            }
        }
        public static void ReadDefaultsFromHTML(string plane, string file)
        {
            DCSLuaInput def;
            if (!DCSIOLogic.EmptyOutputsDCS.ContainsKey(plane))
            {
                def = new DCSLuaInput();
                DCSIOLogic.EmptyOutputsDCS.Add(plane, def);
                def.plane = plane;
                def.JoystickName = "EMPTY";
            }
            else
            {
                def = DCSIOLogic.EmptyOutputsDCS[plane];
            }
            List<HtmlInputElementDCS> result = GetElementsFromHTMLDCS(file);
            for (int i = 0; i < result.Count; ++i)
            {
                if (result[i].bind != null &&
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
        public static void PopulateDCSDictionaryWithProgram()
        {
            InitGames.DCSDBMatchesClean();
            DirectoryInfo fileStorage = new DirectoryInfo(MainStructure.PROGPATH + "\\DB\\DCS");
            FileInfo[] allFilesShipped = fileStorage.GetFiles();
            List<string> loadedPlanes = new List<string>();
            for (int i = 0; i < allFilesShipped.Length; ++i)
            {
                if (allFilesShipped[i].Name.EndsWith(".html"))
                {
                    MainStructure.Write("Load " + allFilesShipped[i].FullName + "into DCS");
                    PopulateDictionaryWithFile(allFilesShipped[i].FullName);
                    if (!loadedPlanes.Contains(allFilesShipped[i].Name.Replace(".html", "")))
                    {
                        loadedPlanes.Add(allFilesShipped[i].Name.Replace(".html", ""));
                        
                    }
                        
                }
            }
            if (Planes == null)
            {
                Planes["DCS"] = loadedPlanes;
            }
            else
            {
                List<string> pp = Planes["DCS"];
                for (int i = 0; i < loadedPlanes.Count; ++i)
                {
                    if (!pp.Contains(loadedPlanes[i]))
                        pp.Add(loadedPlanes[i]);
                }
                Planes["DCS"] = pp;
            }
        }
        public static void PopulateDictionaryWithFile(string file, string overWrite = "")
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
            for (int i = 0; i < elementsDCS.Count; ++i)
            {
                if (elementsDCS[i].id.Substring(0, 1) == "a")
                {
                    if (!DCSLib[planeName].Axis.ContainsKey(elementsDCS[i].id))
                        DCSLib[planeName].Axis.Add(elementsDCS[i].id, new DCSInput(elementsDCS[i].id, elementsDCS[i].title, true, planeName, elementsDCS[i].category));
                    else
                        DCSLib[planeName].Axis[elementsDCS[i].id].Title = elementsDCS[i].title;
                }
                else
                {
                    if (!DCSLib[planeName].Buttons.ContainsKey(elementsDCS[i].id))
                        DCSLib[planeName].Buttons.Add(elementsDCS[i].id, new DCSInput(elementsDCS[i].id, elementsDCS[i].title, false, planeName, elementsDCS[i].category));
                    else
                        DCSLib[planeName].Buttons[elementsDCS[i].id].Title = elementsDCS[i].title;
                }
            }
        }
        public static OtherGameInput[] GetAllOtherGameInputsWithId(string id, string game)
        {
            List<OtherGameInput> result = new List<OtherGameInput>();
            if (OtherLib.ContainsKey(game))
            {
                foreach (KeyValuePair<string, OtherGame> kvp in OtherLib[game])
                {
                    if (kvp.Value.Axis.ContainsKey(id)) result.Add(kvp.Value.Axis[id]);
                    if (kvp.Value.Buttons.ContainsKey(id)) result.Add(kvp.Value.Buttons[id]);
                }
            }
            return result.ToArray();
        }
        public static OtherGameInput[] GetAllOtherGameInputWithTitleAndPlane(string title,string plane, string game, bool axis)
        {
            List<OtherGameInput> result = new List<OtherGameInput>();
            if (OtherLib.ContainsKey(game))
            {
                if (OtherLib[game].ContainsKey(plane))
                {
                    if (axis)
                    {
                        foreach (KeyValuePair<string, OtherGameInput> kvp in OtherLib[game][plane].Axis)
                        {
                            if (title.ToLower().Trim() == kvp.Value.Title.ToLower().Trim())
                            {
                                result.Add(kvp.Value);
                            }
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, OtherGameInput> kvp in OtherLib[game][plane].Buttons)
                        {
                            if (title.ToLower().Trim() == kvp.Value.Title.ToLower().Trim())
                            {
                                result.Add(kvp.Value);
                            }
                        }
                    }
                    
                }
            }
            return result.ToArray();
        }
        public static OtherGameInput[] GetALLInputsWithExactTitle(string title, bool axis)
        {
            List<OtherGameInput> result = new List<OtherGameInput>();
            foreach (KeyValuePair<string,Dictionary<string, OtherGame>> kvp in OtherLib)
            {
                foreach(KeyValuePair<string, OtherGame> kimi in kvp.Value)
                {
                    if (axis)
                    {
                        foreach (KeyValuePair<string, OtherGameInput> axkvp in kimi.Value.Axis)
                        {
                            if (axkvp.Value.Title.Trim().ToLower() == title.Trim().ToLower())
                            {
                                result.Add(axkvp.Value);
                            }
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, OtherGameInput> bnkvp in kimi.Value.Buttons)
                        {
                            if (bnkvp.Value.Title.Trim().ToLower() == title.Trim().ToLower())
                            {
                                result.Add(bnkvp.Value);
                            }
                        }
                    }
                }
            }
            return result.ToArray();
        }
        public static DCSInput[] GetALLInputsWithExactTitleDCS(string title, bool axis)
        {
            List<DCSInput> result = new List<DCSInput>();
            foreach(KeyValuePair<string, DCSPlane> kvp in DCSLib)
            {
                if(axis)
                {
                    foreach(KeyValuePair<string, DCSInput> axkvp in kvp.Value.Axis)
                    {
                        if(axkvp.Value.Title.Trim().ToLower() == title.Trim().ToLower())
                        {
                            result.Add(axkvp.Value);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, DCSInput> bnkvp in kvp.Value.Buttons)
                    {
                        if (bnkvp.Value.Title.Trim().ToLower() == title.Trim().ToLower())
                        {
                            result.Add(bnkvp.Value);
                        }
                    }
                }
            }
            return result.ToArray();
        }
        public static DCSInput[] GetAllDCSInputWithTitleAndPlane(string title, string plane, bool axis)
        {
            List<DCSInput> result = new List<DCSInput>();
            if (DCSLib.ContainsKey(plane))
            {
                if (axis)
                {
                    foreach(KeyValuePair<string, DCSInput> kvp in DCSLib[plane].Axis)
                    {
                        if (title.ToLower().Trim() == kvp.Value.Title.ToLower().Trim())
                        {
                            result.Add(kvp.Value);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, DCSInput> kvp in DCSLib[plane].Buttons)
                    {
                        if (title.ToLower().Trim() == kvp.Value.Title.ToLower().Trim())
                        {
                            result.Add(kvp.Value);
                        }
                    }
                }
            }
            return result.ToArray();
        }
        public static DCSInput[] GetAllDCSInputsWithId(string id)
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
        public static List<SearchQueryResults> SearchBinds(string[] keywords, bool searchDescription=true, bool filteredSearch=true)
        {
            List<SearchQueryResults> results = new List<SearchQueryResults>();
            if ((filteredSearch&&InternalDataManagement.GamesFilter["DCS"])||!filteredSearch)
            {
                foreach (KeyValuePair<string, DCSPlane> kvp in DCSLib)
                {
                    foreach (KeyValuePair<string, DCSInput> inp in kvp.Value.Axis)
                    {
                        bool hit = true;
                        foreach (string key in keywords)
                        {
                            if (searchDescription && !inp.Value.Title.ToLower().Contains(key))
                            {
                                hit = false;
                                break;
                            }else if (!searchDescription&& inp.Value.ID.ToLower() != key.ToLower())
                            {
                                hit = false;
                                break;
                            }
                        }
                        if (hit)
                            results.Add(new SearchQueryResults() { ID = inp.Value.ID, AIRCRAFT = inp.Value.Plane, DESCRIPTION = inp.Value.Title, AXIS = true, GAME = "DCS", CATEGORY=inp.Value.Cat });
                    }

                    foreach (KeyValuePair<string, DCSInput> inp in kvp.Value.Buttons)
                    {
                        bool hit = true;
                        foreach (string key in keywords)
                        {
                            if (searchDescription && !inp.Value.Title.ToLower().Contains(key))
                            {
                                hit = false;
                                break;
                            }else if (!searchDescription && inp.Value.ID.ToLower() != key.ToLower())
                            {
                                hit = false;
                                break;
                            }
                        }
                        if (hit)
                            results.Add(new SearchQueryResults() { ID = inp.Value.ID, AIRCRAFT = inp.Value.Plane, DESCRIPTION = inp.Value.Title, AXIS = false, GAME = "DCS", CATEGORY = inp.Value.Cat });
                    }
                }
            }

            foreach (KeyValuePair<string, Dictionary<string, OtherGame>> kvpOuter in OtherLib)
            {
                foreach (KeyValuePair<string, OtherGame> kvp in kvpOuter.Value)
                {
                    if ((filteredSearch&& InternalDataManagement.GamesFilter.ContainsKey(kvpOuter.Key) && InternalDataManagement.GamesFilter[kvpOuter.Key])|| !filteredSearch)
                    {
                        foreach (KeyValuePair<string, OtherGameInput> inp in kvp.Value.Axis)
                        {
                            bool hit = true;
                            foreach (string key in keywords)
                            {
                                if (searchDescription && !inp.Value.Title.ToLower().Contains(key))
                                {
                                    hit = false;
                                    break;
                                }
                                else if (!searchDescription && inp.Value.ID.ToLower() != key.ToLower())
                                {
                                    hit = false;
                                    break;
                                }
                            }
                            if (hit)
                                results.Add(new SearchQueryResults() { ID = inp.Value.ID, AIRCRAFT = inp.Value.Game, DESCRIPTION = inp.Value.Title, AXIS = true, GAME = kvp.Key, CATEGORY=inp.Value.Category });
                        }
                        foreach (KeyValuePair<string, OtherGameInput> inp in kvp.Value.Buttons)
                        {
                            bool hit = true;
                            foreach (string key in keywords)
                            {
                                if (searchDescription && !inp.Value.Title.ToLower().Contains(key))
                                {
                                    hit = false;
                                    break;
                                }
                                else if (!searchDescription && inp.Value.ID.ToLower() != key.ToLower())
                                {
                                    hit = false;
                                    break;
                                }
                            }
                            if (hit)
                                results.Add(new SearchQueryResults() { ID = inp.Value.ID, AIRCRAFT = inp.Value.Game, DESCRIPTION = inp.Value.Title, AXIS = false, GAME = kvp.Key, CATEGORY = inp.Value.Category });
                        }
                    }
                }
            }
            return results;
        }
    }
}
