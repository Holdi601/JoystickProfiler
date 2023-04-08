﻿using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace JoyPro.StarCitizen
{
    public class SCInputItem
    {
        public string KeyboardInput;
        public string GamepadInput;
        public string JoystickInput;
        public bool KeyboardDoubleTap = false;
        public bool GamepadDoubleTap = false;
        public bool JoystickDoubleTap = false;
    }
    public static class SCIOLogic
    {
        public static Dictionary<string, List<string>> categoryInputs = new Dictionary<string, List<string>>();
        public static Dictionary<string, int> categoryOrder = new Dictionary<string, int>();
        public static Dictionary<string, int> idOrder = new Dictionary<string, int>();
        public static Dictionary<string, int> JoyIDLocal = new Dictionary<string, int>();
        public static Dictionary<string, int> JoyIDExportInstance = new Dictionary<string, int>();
        public static Dictionary<string, int> JoyIDExportProduct = new Dictionary<string, int>();
        public static Dictionary<string, int> GPIDExportInstance = new Dictionary<string, int>();
        public static Dictionary<string, int> GPIDExportProduct = new Dictionary<string, int>();
        public static Dictionary<string, int> KeyboardIDLocal = new Dictionary<string, int>();
        public static Dictionary<string, int> GamepadIDLocal = new Dictionary<string, int>();
        public static Dictionary<string, string> KeyboardConversion_SC2DX = new Dictionary<string, string>();
        public static Dictionary<string, string> KeyboardConversion_DX2SC = new Dictionary<string, string>();
        public static Dictionary<string, Dictionary<string, SCInputItem>> ExportPrep = new Dictionary<string, Dictionary<string, SCInputItem>>();
        public static Dictionary<string, Dictionary<string, SCInputItem>> LocalPrep = new Dictionary<string, Dictionary<string, SCInputItem>>();
        public static string version = "1";
        public static string optionsVersion = "2";
        public static string rebindVersion = "2";
        public static string profileName = "default";
        public static string keyboardProduct = "";
        public static string gamepadProduct = "";
        public const string Game = "StarCitizen";
        public const string Plane = "StarCitizen";

        public static void LoadKeyboardConversion()
        {
            string cfile = MainStructure.PROGPATH + "\\TOOLS\\Conversions\\SC2DX.keyboardconversion";
            if (File.Exists(cfile))
            {
                StreamReader streamReader = new StreamReader(cfile);
                while (!streamReader.EndOfStream)
                {
                    string[] parts = streamReader.ReadLine().Split('§');
                    if (parts.Length > 1)
                    {
                        if (!KeyboardConversion_SC2DX.ContainsKey(parts[0]))
                        {
                            KeyboardConversion_SC2DX.Add(parts[0], parts[1]);
                            KeyboardConversion_DX2SC.Add(parts[1], parts[0]);
                        }
                    }
                }
                streamReader.Close();
                streamReader.Dispose();
            }
        }

        public static void ImportInputs(List<string> selectedSticks)
        {
            ExportPrep= new Dictionary<string, Dictionary<string, SCInputItem>>();
            List<string> translatedSticks = new List<string>();
            foreach(string stick in selectedSticks)
            {
                translatedSticks.Add(GetSCJoystickID(stick));
            }
            ReadLocalActions();
            foreach(KeyValuePair<string, Dictionary<string,SCInputItem>> catkvp in ExportPrep)
            {
                foreach(KeyValuePair<string, SCInputItem> idkvp in catkvp.Value)
                {
                    string[] kb = idkvp.Value.KeyboardInput.Trim().Split('_');
                    string[] gp = idkvp.Value.GamepadInput.Trim().Split('_');
                    string[] js = idkvp.Value.JoystickInput.Trim().Split('_');
                    if (kb.Length > 1 && kb[1].Length > 0&&selectedSticks.Contains("Keyboard"))
                    {
                        string[] allbtns = kb[1].Split('+');
                        List<string> allmodifier = new List<string>();
                        for(int i=0; i<allbtns.Length-1; i++) 
                        {
                            Modifier m = new Modifier();
                            m.device = "Keyboard";
                            m.name = allbtns[i];
                            m.key = SC2JPKBKey(allbtns[i]);
                            allmodifier.Add(m.toReformerString());
                        }
                        string btn = SC2JPKBKey(allbtns[allbtns.Length-1]);
                        List<Bind> bi = InternalDataManagement.GetBindsByJoystickAndKey("Keyboard", btn, false, false, false, 1, 1, 0, allmodifier);
                        if(bi.Count > 0)
                        {
                            for(int i=0; i < bi.Count;i++)
                            {
                                bi[i].Rl.AddNode(new RelationItem(catkvp.Key + "___" + idkvp.Key, Game, Game));
                            }
                        }
                        else
                        {
                            Relation r = new Relation();
                            r.NAME = catkvp.Key + "___" + idkvp.Key+"___Keyboard";
                            r.ISAXIS = false;
                            r.Groups.Add("GENERATED");
                            RelationItem ri = new RelationItem(catkvp.Key + "___" + idkvp.Key, Game, r);
                            r.AddNode(ri);
                            Bind b = new Bind(r);
                            b.Joystick = "Keyboard";
                            b.AllReformers = allmodifier;
                            b.JButton = btn;
                        }
                    }
                    if(gp.Length>1 && gp[1].Length > 0)
                    {
                        
                    }
                    if (js.Length > 1 && js[1].Length > 0)
                    {

                    }
                }
            }
        }

        public static void BindsToExport(List<Bind> allList, OutputType em)
        {
            ExportPrep= new Dictionary<string, Dictionary<string, SCInputItem>>();
            bool keep_keyboard_defaults = MainStructure.msave.KeepKeyboardDefaults==true?true: false;
            Dictionary<string, int> joyOrder = InternalDataManagement.JoystickbindsPerGame(Game, allList);
            JoyIDExportInstance = new Dictionary<string, int>();
            var orderedList = joyOrder.OrderBy(x => x.Value).ToList();
            int index = 1;
            int indexgp = 1;
            for(int i = orderedList.Count - 1; i >= 0; i--)
            {
                if (JoystickReader.IsGamepad(orderedList[i].Key))
                {
                    JoyIDExportInstance.Add(orderedList[i].Key, index);
                    JoyIDExportProduct.Add(GetSCJoystickID(orderedList[i].Key), index);
                    index++;
                }
                else
                {
                    GPIDExportInstance.Add(orderedList[i].Key, indexgp);
                    GPIDExportProduct.Add(GetSCJoystickID(orderedList[i].Key), indexgp);
                    indexgp++;
                }                
            }
            if (em == OutputType.Add || em == OutputType.Merge || em == OutputType.MergeOverwrite)
            {
                ReadLocalActions();
                ConvertExportFromLocalToExport();
            }
            else if (em == OutputType.Clean)
            {
                GenerateCleanOutput(keep_keyboard_defaults);
            }
            
            foreach (Bind bind in allList)
            {
                if(bind.Rl.GamesInRelation().Contains(Game))
                {
                    foreach(RelationItem ri in bind.Rl.NODES)
                    {
                        foreach(OtherGameInput ogi in ri.OtherInputs)
                        {
                            if(ogi.Game==Game)
                            {
                                if(!ExportPrep.ContainsKey(ogi.Category))
                                {
                                    ExportPrep.Add(ogi.Category, new Dictionary<string, SCInputItem>());
                                }
                                string[] prts = MainStructure.SplitBy(ogi.ID, "___");
                                string id = prts[1];
                                int joynum = 1;
                                if (!ExportPrep[ogi.Category].ContainsKey(id))
                                {
                                    ExportPrep[ogi.Category].Add(id, new SCInputItem());
                                    string strt = "";
                                    if(bind.Joystick.ToLower()=="keyboard")
                                    {
                                        strt = "kb";
                                        string outp = strt + joynum.ToString() + "_";
                                        for (int i = 0; i < bind.AllReformers.Count; i++)
                                        {
                                            Modifier m = Modifier.ReformerToMod(bind.AllReformers[i]);
                                            outp += JP2SCKBKey(m.key) + "+";
                                        }
                                        outp+= JP2SCKBKey(bind.JButton).ToLower();
                                        if(ExportPrep[ogi.Category][id].KeyboardInput.Length>2&&em!= OutputType.Merge)
                                        {
                                            ExportPrep[ogi.Category][id].KeyboardInput = outp;
                                        }
                                    }
                                    else if(JoystickReader.IsGamepad(bind.Joystick))
                                    {
                                        strt = "gp";
                                        if(GPIDExportInstance.ContainsKey(bind.Joystick)) joynum= GPIDExportInstance[bind.Joystick];
                                        string outp = strt + joynum.ToString() + "_";
                                        for (int i = 0; i < bind.AllReformers.Count; i++)
                                        {
                                            Modifier m = Modifier.ReformerToMod(bind.AllReformers[i]);
                                            outp += JP2SCGPKey(m.key) + "+";
                                        }
                                        outp+= (bind.Rl.ISAXIS ? JP2SCGPKey(bind.JAxis) : JP2SCGPKey(bind.JButton)).ToLower();
                                        if (ExportPrep[ogi.Category][id].GamepadInput.Length > 2 && em != OutputType.Merge)
                                        {
                                            ExportPrep[ogi.Category][id].GamepadInput = outp;
                                        }
                                    }
                                    else
                                    {
                                        strt = "js";
                                        joynum= JoyIDExportInstance[bind.Joystick];
                                        if (ExportPrep[ogi.Category][id].JoystickInput.Length > 2 && em != OutputType.Merge)
                                        {
                                            ExportPrep[ogi.Category][id].GamepadInput = strt + joynum.ToString() + "_" + (bind.Rl.ISAXIS ? JP2SCJoykey(bind.JAxis) : JP2SCJoykey(bind.JButton)).ToLower();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            WriteExportToFile();
        }
        public static void GenerateCleanOutput(bool keep_keyboad_default)
        {
            foreach(var ogi in DBLogic.OtherLib[Game][Game].Axis)
            {
                string[] prts = MainStructure.SplitBy(ogi.Key, "___");
                string cat= prts[0];
                string id = prts[1];
                if (!ExportPrep.ContainsKey(cat)) ExportPrep.Add(cat, new Dictionary<string, SCInputItem>());
                if (!ExportPrep[cat].ContainsKey(id)) ExportPrep[cat].Add(id, new SCInputItem());
                string[] kbparts = ogi.Value.default_keyboard.Trim().Split('_');
                string[] gpparts = ogi.Value.default_gamepad.Trim().Split('_');
                string[] jsparts = ogi.Value.default_joystick.Trim().Split('_');
                if(kbparts.Length > 1 && kbparts[1].Length>0&&!keep_keyboad_default)
                {
                    ExportPrep[cat][id].KeyboardInput = "kb1_ ";
                }
                if(gpparts.Length > 1 && gpparts[1].Length > 0)
                {
                    ExportPrep[cat][id].GamepadInput = "gp1_ ";
                }
                if(jsparts.Length > 1 && jsparts[1].Length > 0)
                {
                    ExportPrep[cat][id].JoystickInput = "js1_ ";
                }
            }
        }
        public static void WriteExportToFile()
        {
            string addativePathToActions = "LIVE\\USER\\Client\\0\\Profiles\\default\\actionmaps.xml";
            DI_Device di = JoystickReader.GetAllKeyboards()[0];
            StreamWriter swr = new StreamWriter(MiscGames.StarCitizen + "\\" + addativePathToActions);
            swr.WriteLine("<ActionMaps>");
            swr.WriteLine(" <ActionProfiles version=\"1\" optionsVersion=\"2\" rebindVersion=\"2\" profileName=\"default\">");
            swr.WriteLine("  <options type=\"keyboard\" instance=\"1\" Product=\"Keyboard  {"+di.product_guid.ToUpper()+"}\"/>");
            swr.WriteLine("  <options type=\"gamepad\" instance=\"1\" Product=\"Controller (Gamepad)\"/>");
            for(int i=1; i<=(8< JoyIDExportProduct.Count?8:JoyIDExportProduct.Count);++i)
            {
                string name = "";
                for(int j=0; j<JoyIDExportProduct.Count; ++j)
                {
                    if(JoyIDExportProduct.ElementAt(j).Value==i)
                    {
                        name = JoyIDExportProduct.ElementAt(j).Key;
                        break;
                    }
                }
                swr.WriteLine("  <options type=\"joystick\" instance=\""+i.ToString()+"\" Product=\""+name+"\"/>");
            }
            swr.WriteLine("  <modifiers />");

            var sortedList = idOrder.OrderBy(x => x.Value).Select(x => x.Key).ToList();
            string lastCat = "";
            int writtenItems = 0;
            for(int i=0; i<sortedList.Count; i++)
            {
                string[] split = MainStructure.SplitBy(sortedList[i], "___");
                string currentCat = split[0];
                string currentId = split[1];
                if (ExportPrep.ContainsKey(currentCat) 
                    && ExportPrep[currentCat].ContainsKey(currentId) 
                    && (ExportPrep[currentCat][currentId].JoystickInput.Length > 0 
                        || ExportPrep[currentCat][currentId].GamepadInput.Length > 0 
                        || ExportPrep[currentCat][currentId].KeyboardInput.Length > 0))
                {
                    if(writtenItems>0&&lastCat!=currentCat)
                    {
                        swr.WriteLine("  </actionmap>");
                    }
                    if(lastCat!=currentCat)
                    {
                        swr.WriteLine("  <actionmap name=\""+currentCat+"\">");
                    }
                    swr.WriteLine("   <action name=\""+currentId+"\">");
                    if (ExportPrep[currentCat][currentId].KeyboardInput.Length>3)
                    {
                        if (DBLogic.OtherLib[Game][Game].Buttons.ContainsKey(sortedList[i])&&
                            DBLogic.OtherLib[Game][Game].Buttons[sortedList[i]].default_input.Length>3)
                        {
                            OtherGameInput o = DBLogic.OtherLib[Game][Game].Buttons[sortedList[i]];
                            swr.WriteLine("    <rebind defaultInput=\""+o.default_input+"\" input=\""+ ExportPrep[currentCat][currentId].KeyboardInput + "\"/>");
                        }
                        else
                        {
                            swr.WriteLine("    <rebind input=\"" + ExportPrep[currentCat][currentId].KeyboardInput + "\"/>");
                        }
                    }
                    if (ExportPrep[currentCat][currentId].JoystickInput.Length > 3)
                    {
                        swr.WriteLine("    <rebind input=\"" + ExportPrep[currentCat][currentId].JoystickInput + "\"/>");
                    }
                    if (ExportPrep[currentCat][currentId].GamepadInput.Length > 3)
                    {
                        swr.WriteLine("    <rebind input=\"" + ExportPrep[currentCat][currentId].GamepadInput + "\"/>");
                    }
                    swr.WriteLine("   </action>");
                    writtenItems++;
                    lastCat = currentCat;
                }
            }
            swr.WriteLine(" </ActionProfiles>");
            swr.WriteLine("</ActionMaps>");
        }

        public static string JP2SCJoykey(string key)
        {
            string result = "";
            string hatStarter = "JOY_BTN_POV";
            string btnStarter = "JOY_BTN";
            string axsStarter = "JOY_";
            if (key.ToLower().StartsWith(hatStarter.ToLower()))
            {
                string sub = key.Substring(hatStarter.Length);
                result = "hat" + sub.Substring(0,1) + "_";
                string dir = sub.Split('_')[1];
                switch (dir)
                {
                    case "U":result += "up";break;
                    case "L":result += "left";break;
                    case "R":result += "right";break;
                    case "D":result += "down";break;
                }
                
            }else if (key.ToLower().StartsWith(btnStarter.ToLower()))
            {
                result = "button" + key.ToLower().Substring(btnStarter.Length);
            }else if(key.ToLower().StartsWith(axsStarter.ToLower()))
            {
                string subAxis = key.ToLower().Substring(axsStarter.Length);
                if(subAxis.Contains("slider"))
                {
                    result = subAxis;
                }else if(subAxis.Contains("r"))
                {
                    result = "rot" + subAxis.Substring(1);
                }
                else
                {
                    result = subAxis;
                }
            }
            return result;
        }

        public static string JP2SCGPKey(string key)
        {
            switch (key)
            {
                case "JOY_BTN5":return "shoulderl";
                case "JOY_BTN6":return "shoulderr";
                case "JOY_BTN_POV1_U": return "dpad_up";
                case "JOY_BTN_POV1_L": return "dpad_left";
                case "JOY_BTN_POV1_R": return "dpad_right";
                case "JOY_BTN_POV1_D": return "dpad_down";
                case "JOY_BTN2": return "b";
                case "JOY_BTN1": return "a";
                case "JOY_BTN4": return "y";
                case "JOY_BTN3": return "x";
                case "JOY_X":return "thumblx";
                case "JOY_Y":return "thumbly";
                case "JOY_RX": return "thumbrx";
                case "JOY_RY": return "thumbry";
                case "JOY_BTN9": return "thumbl";
                case "JOY_BTN10": return "thumbr";
                case "JOY_Z": return "triggerr";
                case "JOY_RZ": return "triggerl";
                case "JOY_BTN16": return "triggerr_btn";
                case "JOY_BTN17": return "triggerl_btn";
                case "JOY_BTN_POV2_U": return "thumbl_up";
                case "JOY_BTN_POV2_L": return "thumbl_left";
                case "JOY_BTN_POV2_R": return "thumbl_right";
                case "JOY_BTN_POV2_D": return "thumbl_down";
                case "JOY_BTN_POV3_U": return "thumbr_up";
                case "JOY_BTN_POV3_L": return "thumbr_left";
                case "JOY_BTN_POV3_R": return "thumbr_right";
                case "JOY_BTN_POV3_D": return "thumbr_down";
            }
            return " ";
        }

        public static string JP2SCKBKey(string key)
        {
            if(KeyboardConversion_DX2SC.ContainsKey(key))
            {
                return KeyboardConversion_DX2SC[key];
            }
            return key.ToLower();
        }

        public static string SC2JPKBKey(string key)
        {
            if (KeyboardConversion_SC2DX.ContainsKey(key))
            {
                return KeyboardConversion_SC2DX[key];
            }
            return key;
        }

        public static void ConvertExportFromLocalToExport()
        {
            foreach(var category in ExportPrep)
            {
                foreach(var input in category.Value)
                {
                    string joyIn = input.Value.JoystickInput;
                    string j = joyIn.Substring(0, joyIn.IndexOf("_")).ToLower().Replace("js","");
                    string btnpart = joyIn.Substring(joyIn.IndexOf("_") + 1);
                    string key = JoyIDLocal.FirstOrDefault(x=>x.Value==Convert.ToInt32(j)).Key;
                    if (key == null) continue;
                    int num = JoyIDExportProduct.Count;
                    if(JoyIDExportProduct.ContainsKey(key))
                    {
                        num = JoyIDExportProduct[key];
                    }
                    input.Value.JoystickInput = "js" + num.ToString() + "_" + btnpart;
                }
            }
        }


        public static string GetSCJoystickID(string joystick)
        {
            if(InternalDataManagement.LocalJoystickPGUID.ContainsKey(joystick))
            {
                return InternalDataManagement.LocalJoystickPGUID[joystick].Replace(" {", "  {");
            }
            return null;
        }

        public static void ReadLocalActions()
        {
            string addativePathToActions = "LIVE\\USER\\Client\\0\\Profiles\\default\\actionmaps.xml";
            StreamReader sr = new StreamReader(MiscGames.StarCitizen + "\\" + addativePathToActions);
            string instanceStr = "instance=\"";
            string productStr = "Product=\"";
            string endquote = "\"";
            int quoteend = -1;
            string keyboardoptions = "<options type=\"keyboard\"";
            string gamepadoptions = "<options type=\"gamepad\"";
            string joystickoptions = "<options type=\"joystick\"";
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if(line.Contains("<modifiers />"))
                {
                    break;
                }else if (line.Contains("<ActionProfiles version=\""))
                {
                    string versionStr = "version=\"";
                    string optionsStr = "optionsVersion=\"";
                    string rebindVersionStr = "rebindVersion=\"";
                    string profileStr = "profileName=\"";
                    
                    int indver = line.IndexOf(versionStr);
                    string verStart = line.Substring(indver+versionStr.Length);
                    quoteend = verStart.IndexOf(endquote);
                    version = verStart.Substring(0, quoteend);
                    indver = verStart.IndexOf(optionsStr);
                    verStart=verStart.Substring(indver+optionsStr.Length);
                    quoteend = verStart.IndexOf(endquote);
                    optionsVersion=verStart.Substring(0, quoteend);
                    indver = verStart.IndexOf(rebindVersionStr);
                    verStart = verStart.Substring(indver+rebindVersionStr.Length);
                    quoteend=verStart.IndexOf(endquote);
                    rebindVersion=verStart.Substring(0, quoteend);
                    indver = verStart.IndexOf(profileStr);
                    verStart=verStart.Substring(indver+profileStr.Length);
                    quoteend=verStart.IndexOf(endquote);
                    profileName=verStart.Substring(0, quoteend);
                }else if(line.Contains(keyboardoptions)||
                    line.Contains(gamepadoptions)||
                    line.Contains(joystickoptions))
                {
                    string to = line.Contains(keyboardoptions) ? "keyboard" : line.Contains(gamepadoptions) ? "gamepad" : "joystick";
                    int indx = line.IndexOf(instanceStr);
                    line=line.Substring(indx+instanceStr.Length);
                    quoteend=line.IndexOf(endquote);
                    int instanceId = Convert.ToInt32(line.Substring(0,quoteend));
                    indx=line.IndexOf(productStr);
                    line=line.Substring(indx+productStr.Length);
                    quoteend=line.IndexOf(endquote);
                    string product = line.Substring(0, quoteend);
                    Dictionary<string, int> dictRef=null;
                    switch (to)
                    {
                        case "keyboard":
                            dictRef = KeyboardIDLocal;
                            break;
                        case "gamepad":
                            dictRef=GamepadIDLocal; 
                            break;
                        case "joystick":
                            dictRef = JoyIDLocal;
                            break;
                    }
                    if (!dictRef.ContainsKey(product)) dictRef.Add(product, instanceId);
                }
            }
            string lastCategory = "";
            string lastId="";
            string catStart = "<actionmap name=\"";
            string idStart = "<action name=\"";
            string rebindStart = " input=\"";
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Contains(catStart))
                {
                    int indx = line.IndexOf(catStart);
                    line = line.Substring(indx + catStart.Length);
                    quoteend = line.IndexOf(endquote);
                    lastCategory = line.Substring(0, quoteend);
                    if (!ExportPrep.ContainsKey(lastCategory)) ExportPrep.Add(lastCategory, new Dictionary<string, SCInputItem>());
                } else if (line.Contains(idStart))
                {
                    int indx = line.IndexOf(idStart);
                    line = line.Substring(indx + idStart.Length);
                    quoteend = line.IndexOf(endquote);
                    lastId = line.Substring(0, quoteend);
                    if (!ExportPrep[lastCategory].ContainsKey(lastId)) ExportPrep[lastCategory].Add(lastId, new SCInputItem());
                } else if (line.Contains(rebindStart))
                {
                    int indx = line.IndexOf(rebindStart);
                    line = line.Substring(indx + rebindStart.Length);
                    quoteend = line.IndexOf(endquote);
                    string input = line.Substring(0, quoteend);
                    if (input.StartsWith("kb")) ExportPrep[lastCategory][lastId].KeyboardInput = input;
                    else if (input.StartsWith("js")) ExportPrep[lastCategory][lastId].JoystickInput = input;
                    else ExportPrep[lastCategory][lastId].GamepadInput = input;
                }
            }
            
        }
        public static bool HasIllegalModifier(Bind b)
        {
            foreach(string reformer in b.AllReformers)
            {
                string[] prts = reformer.Split('§');
                if (prts[1].ToLower().Trim() != "keyboard") return true;
                if (prts[2].ToLower().Trim() != "leftalt" &&
                    prts[2].ToLower().Trim() != "rightalt" &&
                    prts[2].ToLower().Trim() != "leftshift") return true;
            }
            return false;
        }
        public static void RemoveKey(string key)
        {
            for(int i=ExportPrep.Count-1; i>=0; i--)
            {
                for(int j=ExportPrep.ElementAt(i).Value.Count-1; j>=0; j--)
                {
                    if (key.StartsWith("kb"))
                    {
                        if(ExportPrep.ElementAt(i).Value.ElementAt(j).Value.KeyboardInput.ToLower()==key.ToLower())
                        {
                            ExportPrep.ElementAt(i).Value.ElementAt(j).Value.KeyboardInput = "";
                        }
                    }
                    else if (key.StartsWith("js"))
                    {
                        if (ExportPrep.ElementAt(i).Value.ElementAt(j).Value.JoystickInput.ToLower() == key.ToLower())
                        {
                            ExportPrep.ElementAt(i).Value.ElementAt(j).Value.JoystickInput = "";
                        }
                    }
                    else
                    {
                        if (ExportPrep.ElementAt(i).Value.ElementAt(j).Value.GamepadInput.ToLower() == key.ToLower())
                        {
                            ExportPrep.ElementAt(i).Value.ElementAt(j).Value.GamepadInput = "";
                        }
                    }
                    if(ExportPrep.ElementAt(i).Value.ElementAt(j).Value.GamepadInput.Length==0&&
                        ExportPrep.ElementAt(i).Value.ElementAt(j).Value.KeyboardInput.Length==0&&
                        ExportPrep.ElementAt(i).Value.ElementAt(j).Value.JoystickInput.Length==0)
                    {
                        string toRemov = ExportPrep.ElementAt(i).Value.ElementAt(j).Key;
                        ExportPrep.ElementAt(i).Value.Remove(toRemov);
                    }
                }
            }
        }
    }
}
