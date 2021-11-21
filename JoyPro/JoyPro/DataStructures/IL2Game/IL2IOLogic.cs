using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public enum OutputType { Clean, Add, Merge, MergeOverwrite};
    public enum IL2ActionComponent { ID, Input, Inverted, Description, JRelationName, JAlias, JGroups}
    public static class IL2IOLogic
    {
        static string ActionsPreFile = 
            "// Input map preset.\r\n"+
            "// It is not recommended to change anything here,\r\n"+
            "// but it is still possible on your own risk.\r\n"+
            "// I am trying my best not to break it cheers the Joypro dev :-P\r\n"+
            "\r\n"+
            "&actions = action, command, invert|\r\n";

        //Map file gets recreated everytime you go into the options so its worthless to manipulate.
        //static string MapPreFile = 
        //    "// KOS GENERATED INPUT MAPFILE\r\n"+
        //    "// IT IS NOT RECOMMENDED TO CHANGE ANYTHING HERE!\r\n"+
        //    "// (Well, it is still possible, but on your own risk ;o))\r\n"+
        //    "// Well I am trying shoutout to the IL Devs, cheers the JoyPro devs ;) , Btw please explain what State and Event is for in the action argument functions below kthxbye\r\n"+
        //    "\r\n";

        static string ResponsesPreFile = 
            "type4Devices=actionId%2ChighValuesDeadZone%2ClowValuesDeadZone&" +
            "type2Devices=actionId%2ChighValuesDeadZone%2ClowValuesDeadZone%7C%0D%0A" +
            "rpc_all_engines_throttles%2C0%2C0&" +
            "type1Devices=actionId%2CcenterDeadZone%2CsensitivityBallance%2CsideDeadZone";

        static string DevicesPreFile = "configId,guid,model|\r\r\n";
        public static List<string> ActionsFileContentOutput = new List<string>();
        public static List<string> ActionsFileKeyboardContent = new List<string>();
        public static Dictionary<string, List<string>> ActionsFileJoystickContent = new Dictionary<string, List<string>>();
        public static List<string> MapActionKeyboard = new List<string>();
        public static List<string> MapActionContent = new List<string>();
        public static List<string> MapEndOfFile = new List<string>();
        public static List<string> usedCommands = new List<string>();
        public static Dictionary<string, Bind> axisSettings = new Dictionary<string, Bind>();
        public static Dictionary<string, IL2AxisSetting> axisSettingRead = new Dictionary<string, IL2AxisSetting>();
        static string InputPath = "\\data\\input\\";
        static List<string> Modifier = new List<string>();
        static Dictionary<string, IL2AxisReplacementButtons> axesButtonCommandOtherHalfMissing = new Dictionary<string, IL2AxisReplacementButtons>();
        static Dictionary<string, IL2AxisSetting> axisResponsedRead = new Dictionary<string, IL2AxisSetting>();
        public static void WriteOut(List<Bind> toExport, OutputType ot)
        {
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            if (MainStructure.msave.IL2OR == null) MainStructure.msave.IL2OR = "";
            if (((MainStructure.msave.IL2OR == null || !Directory.Exists(MainStructure.msave.IL2OR)) &&
                (MiscGames.IL2Instance == null || !Directory.Exists(MiscGames.IL2Instance))|| toExport==null|| toExport.Count<1))
                return;
            clearAll();
            CreateDeviceFile();
            ReadActionsFromActions(ActionsFileKeyboardContent, ActionsFileJoystickContent);
            foreach(string s in ActionsFileKeyboardContent)
            {
                ActionsFileContentOutput.Add(s);
            }

            Dictionary<string, string> generatedOutputFromBinds = new Dictionary<string, string>();
            for (int i=0; i<toExport.Count; ++i)
            {
                Dictionary<string, string> lines = BindToActionString(toExport[i]);
                foreach(KeyValuePair<string, string> kvp in lines)
                {
                    if (!generatedOutputFromBinds.ContainsKey(kvp.Key))
                        generatedOutputFromBinds.Add(kvp.Key, kvp.Value);
                    else
                        generatedOutputFromBinds[kvp.Key] = kvp.Value;
                }
            }
            switch (ot)
            {
                case OutputType.Add:
                    foreach (KeyValuePair<string, List<string>> kvp in ActionsFileJoystickContent)
                    {
                        for(int i=0; i<kvp.Value.Count; ++i)
                            ActionsFileContentOutput.Add(kvp.Value[i]);
                    }
                    foreach (KeyValuePair<string, string> kvp in generatedOutputFromBinds)
                        ActionsFileContentOutput.Add(kvp.Value);
                    break;
                case OutputType.Clean:
                    foreach (KeyValuePair<string, string> kvp in generatedOutputFromBinds)
                        ActionsFileContentOutput.Add(kvp.Value);
                    break;
                case OutputType.Merge:
                    foreach (KeyValuePair<string, List<string>> kvp in ActionsFileJoystickContent)
                    {
                        for (int i = 0; i < kvp.Value.Count; ++i)
                            ActionsFileContentOutput.Add(kvp.Value[i]);
                    }
                    foreach (KeyValuePair<string, string> kvp in generatedOutputFromBinds)
                        if(!ActionsFileJoystickContent.ContainsKey(kvp.Key))
                            ActionsFileContentOutput.Add(kvp.Value);
                    break;
                case OutputType.MergeOverwrite:
                    foreach (KeyValuePair<string, string> kvp in generatedOutputFromBinds)
                        ActionsFileContentOutput.Add(kvp.Value);
                    foreach (KeyValuePair<string, List<string>> kvp in ActionsFileJoystickContent)
                    {
                        if (!generatedOutputFromBinds.ContainsKey(kvp.Key))
                        {
                            for(int i=0; i<kvp.Value.Count; ++i)
                                ActionsFileContentOutput.Add(kvp.Value[i]);
                        }
                            
                    }  
                    break;
            }
            CreateActionsFile();
            CreateActionsFile("jp");
            CreateResponsesFile();

        }
        static void clearAll()
        {
            ActionsFileContentOutput = new List<string>();
            ActionsFileKeyboardContent = new List<string>();
            MapActionKeyboard = new List<string>();
            MapActionContent = new List<string>();
            MapEndOfFile = new List<string>();
            usedCommands = new List<string>();
            axisSettings = new Dictionary<string, Bind>();
            ActionsFileJoystickContent = new Dictionary<string, List<string>>();
        }
        static void AddModifierToOutput(string joystick, string btn, string modName)
        {
            string result = "modifier(\"";
            string buttonRaw = DCStoIL2String(joystick, btn, false);
            string[] splitBtn = buttonRaw.Split('_');
            result = result + splitBtn[0];
            for(int i=1; i<splitBtn.Length; ++i)
            {
                result = result + "_" + splitBtn[i].ToUpper();
            }
            result = result + "\");   //JP:"+modName;
            if (!Modifier.Contains(result))
                Modifier.Add(result);
        }
        static void ReadActionsFromActions(List<string> KeyboardOutput, Dictionary<string, List<string>> JoystickOutput, string name="current")
        {
            string path = GetInputPath();
            if (!File.Exists(path + InputPath + name + ".actions")) return;
            StreamReader sr = new StreamReader(path + InputPath + name+".actions");
            bool commandsStart = false;
            List<string> datata = new List<string>();
            while (!commandsStart)
            {
                string line = sr.ReadLine();
                if (line == null)
                {
                    sr.Close();
                    sr.Dispose();
                    return;
                }
                datata.Add(line);
                if (line.ToLower().Contains(("&actions").ToLower()))
                    commandsStart = true;
            }
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();
                if (!(currentLine.Contains("joy0") ||
                    currentLine.Contains("joy1")   ||
                    currentLine.Contains("joy2")   ||
                    currentLine.Contains("joy3")   ||
                    currentLine.Contains("joy4")   ||
                    currentLine.Contains("joy5")   ||
                    currentLine.Contains("joy6")   ||
                    currentLine.Contains("joy7")   ||
                    currentLine.Contains("joy8")   ||
                    currentLine.Contains("joy9")   ||
                    currentLine.Contains("joy10")  ||
                    currentLine.Contains("joy11")))
                {
                    KeyboardOutput.Add(currentLine);
                }
                else
                {
                    string command = currentLine.Split(',')[0];
                    if (!JoystickOutput.ContainsKey(command))
                    {
                        JoystickOutput.Add(command, new List<string>());
                    }
                    JoystickOutput[command].Add(currentLine);

                }
            }
            sr.Close();
            sr.Dispose();
        }
        static Dictionary<string, string> BindToActionString(Bind b)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            List<RelationItem> ri = b.Rl.AllRelations();
            for(int i=0; i<ri.Count; ++i)
            {
                if (ri[i].Game == "IL2Game")
                {
                    string id = ri[i].ID;
                    bool dualSetNeeded = false;
                    string newId=null;
                    if (id.EndsWith("+") || id.EndsWith("-"))
                    {
                        newId = id.Substring(0, id.Length - 1);
                        bool positive;
                        if (id.EndsWith("+")) positive = true;
                        else positive = false;
                        if (!axesButtonCommandOtherHalfMissing.ContainsKey(newId))
                        {
                            axesButtonCommandOtherHalfMissing.Add(newId, new IL2AxisReplacementButtons());
                        }
                        if (positive)
                        {
                            axesButtonCommandOtherHalfMissing[newId].bp = b;
                            axesButtonCommandOtherHalfMissing[newId].positive = ToIL2JoystickKeyAxisString(b);
                        }
                        else
                        {
                            axesButtonCommandOtherHalfMissing[newId].bn = b;
                            axesButtonCommandOtherHalfMissing[newId].negative = ToIL2JoystickKeyAxisString(b);
                        }
                        axesButtonCommandOtherHalfMissing[newId].id = newId;
                        dualSetNeeded = true;
                        id = newId;
                    }
                    newId = id;
                    id = id + ",";
                    for (int j = id.Length; j <= 49; ++j)
                    {
                        id = id + " ";
                    }
                    if (dualSetNeeded)
                    {
                        if(axesButtonCommandOtherHalfMissing[newId].bn!=null&&
                            axesButtonCommandOtherHalfMissing[newId].bp!=null&&
                            axesButtonCommandOtherHalfMissing[newId].negative.Length>2&&
                            axesButtonCommandOtherHalfMissing[newId].positive.Length > 2)
                        {
                            id = id + axesButtonCommandOtherHalfMissing[newId].positive + "/" + axesButtonCommandOtherHalfMissing[newId].negative+",";
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        id=id+ ToIL2JoystickKeyAxisString(b)+",";
                    }
                    if (!usedCommands.Contains(newId))
                        usedCommands.Add(newId);
                    for (int j = id.Length; j <= 99; ++j)
                    {
                        id = id + " ";
                    }
                    if (b.Rl.ISAXIS||dualSetNeeded)
                    {
                        if (!axisSettings.ContainsKey(newId))
                        {
                            axisSettings.Add(newId, b);
                        }
                        if (b.Inverted == true)
                        {
                    
                            if (axesButtonCommandOtherHalfMissing.ContainsKey(newId))
                            {
                                id = id + "1| //JP:" + axesButtonCommandOtherHalfMissing[newId].bp.Rl.NAME + ";";
                                if (axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups != null &&
                                    axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups.Count > 0)
                                {
                                    id = id + axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups[0];
                                    for (int z = 1; z < axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups.Count; ++z)
                                    {
                                        id = id + "," + axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups[z];
                                    }
                                }
                                id = id + ";";
                                if (InternalDataManagement.JoystickAliases.ContainsKey(axesButtonCommandOtherHalfMissing[newId].bp.Joystick) &&
                                    InternalDataManagement.JoystickAliases[axesButtonCommandOtherHalfMissing[newId].bp.Joystick].Length > 2)
                                {
                                    id = id + InternalDataManagement.JoystickAliases[axesButtonCommandOtherHalfMissing[newId].bp.Joystick];
                                }

                                id = id + "//JP:" + axesButtonCommandOtherHalfMissing[newId].bn.Rl.NAME + ";";
                                if (axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups != null &&
                                    axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups.Count > 0)
                                {
                                    id = id + axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups[0];
                                    for (int z = 1; z < axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups.Count; ++z)
                                    {
                                        id = id + "," + axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups[z];
                                    }
                                }
                                id = id + ";";
                                if (InternalDataManagement.JoystickAliases.ContainsKey(axesButtonCommandOtherHalfMissing[newId].bn.Joystick) &&
                                    InternalDataManagement.JoystickAliases[axesButtonCommandOtherHalfMissing[newId].bn.Joystick].Length > 2)
                                {
                                    id = id + InternalDataManagement.JoystickAliases[axesButtonCommandOtherHalfMissing[newId].bn.Joystick];
                                }
                            }
                            else
                            {
                                id = id + "1| //JP:" + b.Rl.NAME + ";";
                                if (b.Rl.Groups != null && b.Rl.Groups.Count > 0)
                                {
                                    id = id + b.Rl.Groups[0];
                                    for (int z = 1; z < b.Rl.Groups.Count; ++z)
                                    {
                                        id = id + "," + b.Rl.Groups[z];
                                    }
                                }
                                id = id + ";";
                                if (InternalDataManagement.JoystickAliases.ContainsKey(b.Joystick) &&
                                    InternalDataManagement.JoystickAliases[b.Joystick].Length > 2)
                                {
                                    id = id + InternalDataManagement.JoystickAliases[b.Joystick];
                                }
                            }
                        }
                        else
                        {
                            if (axesButtonCommandOtherHalfMissing.ContainsKey(newId))
                            {
                                id = id + "0| //JP:" + axesButtonCommandOtherHalfMissing[newId].bp.Rl.NAME + ";";
                                if (axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups != null &&
                                    axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups.Count > 0)
                                {
                                    id = id + axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups[0];
                                    for (int z = 1; z < axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups.Count; ++z)
                                    {
                                        id = id + "," + axesButtonCommandOtherHalfMissing[newId].bp.Rl.Groups[z];
                                    }
                                }
                                id = id + ";";
                                if (InternalDataManagement.JoystickAliases.ContainsKey(axesButtonCommandOtherHalfMissing[newId].bp.Joystick) &&
                                    InternalDataManagement.JoystickAliases[axesButtonCommandOtherHalfMissing[newId].bp.Joystick].Length > 2)
                                {
                                    id = id + InternalDataManagement.JoystickAliases[axesButtonCommandOtherHalfMissing[newId].bp.Joystick];
                                }

                                id = id + "//JP:" + axesButtonCommandOtherHalfMissing[newId].bn.Rl.NAME + ";";
                                if (axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups != null &&
                                    axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups.Count > 0)
                                {
                                    id = id + axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups[0];
                                    for (int z = 1; z < axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups.Count; ++z)
                                    {
                                        id = id + "," + axesButtonCommandOtherHalfMissing[newId].bn.Rl.Groups[z];
                                    }
                                }
                                id = id + ";";
                                if (InternalDataManagement.JoystickAliases.ContainsKey(axesButtonCommandOtherHalfMissing[newId].bn.Joystick) &&
                                    InternalDataManagement.JoystickAliases[axesButtonCommandOtherHalfMissing[newId].bn.Joystick].Length > 2)
                                {
                                    id = id + InternalDataManagement.JoystickAliases[axesButtonCommandOtherHalfMissing[newId].bn.Joystick];
                                }
                            }
                            else
                            {
                                id = id + "0| //JP:" + b.Rl.NAME + ";";
                                if (b.Rl.Groups != null && b.Rl.Groups.Count > 0)
                                {
                                    id = id + b.Rl.Groups[0];
                                    for (int z = 1; z < b.Rl.Groups.Count; ++z)
                                    {
                                        id = id + "," + b.Rl.Groups[z];
                                    }
                                }
                                id = id + ";";
                                if (InternalDataManagement.JoystickAliases.ContainsKey(b.Joystick) &&
                                    InternalDataManagement.JoystickAliases[b.Joystick].Length > 2)
                                {
                                    id = id + InternalDataManagement.JoystickAliases[b.Joystick];
                                }
                            }
                        }
                    }
                    else
                    {
                        id = id + "0| //JP:" + b.Rl.NAME + ";";
                        if (b.Rl.Groups != null && b.Rl.Groups.Count > 0)
                        {
                            id = id + b.Rl.Groups[0];
                            for(int z=1; z<b.Rl.Groups.Count; ++z)
                            {
                                id = id + "," + b.Rl.Groups[z];
                            }
                        }
                        id = id + ";";
                        if(InternalDataManagement.JoystickAliases.ContainsKey(b.Joystick)&&
                            InternalDataManagement.JoystickAliases[b.Joystick].Length > 2)
                        {
                            id = id + InternalDataManagement.JoystickAliases[b.Joystick];
                        }
                    }
                    if (!result.ContainsKey(newId))
                        result.Add(newId, id);
                    else
                        result[newId] = id;
                }
            }
            return result;
        }
        static string DCStoIL2String(string joystick, string axisButton, bool ax)
        {
            int i = -1;
            string result=null;
            if (joystick.ToLower() == "Keyboard")
            {
                result = "key_";
                axisButton = axisButton.ToLower().Replace("numberpad", "numpad").Replace("minus","subtract").Replace("plus","add").Replace("arrow","");
                if (axisButton.Length == 2) axisButton.Replace("d", "");
                switch (axisButton)
                {
                    case "leftalt": result = result + "lmenu";break;
                    case "rightalt": result = result + "rmenu";break;
                    case "leftcontrol": result = result + "lcontrol";break;
                    case "rightcontrol": result = result + "rcontrol";break;
                    case "leftshift": result = result + "lshift";break;
                    case "rightshift": result = result + "rshift"; break;
                    case "leftbracket": result = result + "lbracket";break;
                    case "rightbracket": result = result + "rbracket";break;
                    case "printscreen": result = result + "sysrq";break;
                    case "applications":result = result + "apps";break;
                    case "capslock": result = result + "capital";break;
                    case "pageup":result = result + "prior";break;
                    case "pagedown": result = result + "next"; break;
                    case "scrolllock":result = result + "scroll";break;
                    default: result = result + axisButton;break;
                }
            }
            else
            {
                result = "joy";
                for (int j = 0; j < InternalDataManagement.LocalJoysticks.Length; ++j)
                {
                    if (joystick == InternalDataManagement.LocalJoysticks[j])
                    {
                        i = j;
                        break;
                    }
                }
                if (i == -1) return null;
                result = result + i.ToString() + "_";
                if (ax)
                {
                    result = result + "axis_";
                    string axis = axisButton.Replace("JOY_", "").ToLower();
                    string filler = "";
                    switch (axis)
                    {
                        case "x": filler = "x"; break;
                        case "y": filler = "y"; break;
                        case "z": filler = "z"; break;
                        case "rx": filler = "w"; break;
                        case "ry": filler = "s"; break;
                        case "rz": filler = "t"; break;
                        case "slider1": filler = "p"; break;
                        case "slider2": filler = "q"; break;
                    }
                    result = result + filler;
                }
                else
                {
                    if (axisButton.ToLower().Contains("pov"))
                    {
                        result = result + "pov";
                        string blt = axisButton.Replace("JOY_BTN_", "");
                        string[] parts = blt.ToLower().Replace("pov", "").Split('_');
                        int idPart = Convert.ToInt32(parts[0]) - 1;
                        string angle = "";
                        switch (parts[1])
                        {
                            case "u":
                                angle = "0";
                                break;
                            case "ur":
                                angle = "45";
                                break;
                            case "r":
                                angle = "90";
                                break;
                            case "dr":
                                angle = "135";
                                break;
                            case "d":
                                angle = "180";
                                break;
                            case "dl":
                                angle = "225";
                                break;
                            case "l":
                                angle = "270";
                                break;
                        }
                        result = result + idPart.ToString() + "_" + angle;
                    }
                    else
                    {
                        int num = Convert.ToInt32(axisButton.Replace("JOY_BTN", "")) - 1;
                        if (num < 63)
                        {
                            result = result + "b" + num.ToString();
                        }
                        else
                        {
                            string filler = "";
                            switch (num)
                            {
                                case 63: filler = "b63_povnu"; break;
                                case 64: filler = "pov0"; break;
                                case 65: filler = "pov1"; break;
                                case 66: filler = "pov2"; break;
                                case 67: filler = "pov3"; break;
                                case 68: filler = "pov4"; break;
                                case 69: filler = "pov5"; break;
                                case 70: filler = "pov6"; break;
                                case 71: filler = "pov7"; break;
                                case 72: filler = "pov0_0"; break;
                                case 73: filler = "pov0_45"; break;
                                case 74: filler = "pov0_90"; break;
                                case 75: filler = "pov0_135"; break;
                                case 76: filler = "pov0_180"; break;
                                case 77: filler = "pov0_225"; break;
                                case 78: filler = "pov0_270"; break;
                                case 79: filler = "pov0_315"; break;
                                case 80: filler = "pov1_0"; break;
                                case 81: filler = "pov1_45"; break;
                                case 82: filler = "pov1_90"; break;
                                case 83: filler = "pov1_135"; break;
                                case 84: filler = "pov1_180"; break;
                                case 85: filler = "pov1_225"; break;
                                case 86: filler = "pov1_270"; break;
                                case 87: filler = "pov1_315"; break;
                                case 88: filler = "pov2_0"; break;
                                case 89: filler = "pov2_45"; break;
                                case 90: filler = "pov2_90"; break;
                                case 91: filler = "pov2_135"; break;
                                case 92: filler = "pov2_180"; break;
                                case 93: filler = "pov2_225"; break;
                                case 94: filler = "pov2_270"; break;
                                case 95: filler = "pov2_315"; break;
                                case 96: filler = "pov3_0"; break;
                                case 97: filler = "pov3_45"; break;
                                case 98: filler = "pov3_90"; break;
                                case 99: filler = "pov3_135"; break;
                                case 100: filler = "pov3_180"; break;
                                case 101: filler = "pov3_225"; break;
                                case 102: filler = "pov3_270"; break;
                                case 103: filler = "pov3_315"; break;
                                case 104: filler = "pov4_0"; break;
                                case 105: filler = "pov4_45"; break;
                                case 106: filler = "pov4_90"; break;
                                case 107: filler = "pov4_135"; break;
                                case 108: filler = "pov4_180"; break;
                                case 109: filler = "pov4_225"; break;
                                case 110: filler = "pov4_270"; break;
                                case 111: filler = "pov4_315"; break;
                                case 112: filler = "pov5_0"; break;
                                case 113: filler = "pov5_45"; break;
                                case 114: filler = "pov5_90"; break;
                                case 115: filler = "pov5_135"; break;
                                case 116: filler = "pov5_180"; break;
                                case 117: filler = "pov5_225"; break;
                                case 118: filler = "pov5_270"; break;
                                case 119: filler = "pov5_315"; break;
                                case 120: filler = "pov6_0"; break;
                                case 121: filler = "pov6_45"; break;
                                case 122: filler = "pov6_90"; break;
                                case 123: filler = "pov6_135"; break;
                                case 124: filler = "pov6_180"; break;
                                case 125: filler = "pov6_225"; break;
                                case 126: filler = "pov6_270"; break;
                                case 127: filler = "pov6_315"; break;
                            }
                            result = result + filler;
                        }
                    }
                } 
            }
            return result;
        }
        static string ToIL2JoystickKeyAxisString(Bind b)
        {
            string mainBind = DCStoIL2String(b.Joystick, b.Rl.ISAXIS ? b.JAxis : b.JButton, b.Rl.ISAXIS);
            //Modifier work here
            if (b.AllReformers.Count > 0)
            {
                string[] splitReformer = b.AllReformers[0].Split('§');
                string result = DCStoIL2String(splitReformer[1], splitReformer[2], false);
                for (int z = 1; z < b.AllReformers.Count; ++z)
                {
                    splitReformer = b.AllReformers[z].Split('§');
                    AddModifierToOutput(splitReformer[1], splitReformer[2], splitReformer[0]);
                    result =result+"+"+ DCStoIL2String(splitReformer[1], splitReformer[2], false);
                }
                return result + "+" + mainBind;
            }
            else
            {
                return mainBind;
            }
        }
        public static void CreateDeviceFile(string name= "devices")
        {
            string path = GetInputPath();
            StreamWriter swr = new StreamWriter(path + InputPath + name + ".txt");
            swr.Write(DevicesPreFile);
            for(int i=0; i< InternalDataManagement.LocalJoysticks.Length-1; ++i)
            {
                swr.Write(MiscGames.DCSJoyIdToIL2JoyId(InternalDataManagement.LocalJoysticks[i], i)+"|\r\r\n");
            }
            swr.Write(MiscGames.DCSJoyIdToIL2JoyId(
                InternalDataManagement.LocalJoysticks[InternalDataManagement.LocalJoysticks.Length-1],
                InternalDataManagement.LocalJoysticks.Length - 1) + "\r\r");
            swr.Flush();
            swr.Close();
            swr.Dispose();
        }
        public static void CreateResponsesFile(string name="current")
        {
            string path = GetInputPath();
            StreamWriter swr = new StreamWriter(path + InputPath + name+ ".responses");
            swr.Write(ResponsesPreFile);
            foreach(KeyValuePair<string, Bind> kvp in axisSettings)
            {
                swr.Write("%7C%0D%0A"+kvp.Key+ "%2C");
                if (kvp.Value.Slider == true)
                {
                    swr.Write("0.0%2C");
                    if (kvp.Value.Curvature.Count == 1)
                    {
                        
                        swr.Write(Math.Round(kvp.Value.Curvature[0] * 0.5, 2).ToString() + "%2C" +
                            Math.Round(kvp.Value.Deadzone * 0.25, 2).ToString());
                    }
                    else
                    {
                        //Custom curves implementation needed here if they dont get overwritten
                        swr.Write(Math.Round(Bind.Curvature_Default[0]*0.5).ToString() + "%2C" +
                            Math.Round(kvp.Value.Deadzone * 0.25, 2).ToString());
                    }
                }
                else
                {
                    swr.Write(Math.Round(kvp.Value.Deadzone * 0.25, 2).ToString() + "%2C");
                    if (kvp.Value.Curvature.Count == 1)
                    {
                        swr.Write(Math.Round(kvp.Value.Curvature[0] * 0.5, 2).ToString() + "%2C0.0");
                    }
                    else
                    {
                        //Custom curves implementation needed here if they dont get overwritten
                        swr.Write(Math.Round(Bind.Curvature_Default[0] * 0.5).ToString() + "%2C0.0");
                    }
                }
            }
            swr.Flush();
            swr.Close();
            swr.Dispose();
        }
        static string GetInputPath()
        {
            string path = "";
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            if (MainStructure.msave.IL2OR == null) MainStructure.msave.IL2OR = "";
            if (MainStructure.msave.IL2OR.Length > 2) path = MainStructure.msave.IL2OR;
            else path = MiscGames.IL2Instance;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static void CreateActionsFile(string name = "current")
        {
            string path = GetInputPath();
            StreamWriter swr = new StreamWriter(path + InputPath + name + ".actions");
            swr.Write(ActionsPreFile);
            for(int i=0; i<ActionsFileContentOutput.Count-1; ++i)
            {
                swr.Write(ActionsFileContentOutput[i]+"\r\n");
            }
            swr.Write(ActionsFileContentOutput[ActionsFileContentOutput.Count - 1]);
            swr.Flush();
            swr.Close();
            swr.Dispose();

        } 
        static void ReadResponsesFile()
        {
            string path = GetInputPath();
            axisResponsedRead = new Dictionary<string, IL2AxisSetting>();
            if (!File.Exists(path + InputPath + "current" + ".responses")) return;
            StreamReader sr = new StreamReader(path + InputPath + "current" + ".responses");
            const string cursor= "type1Devices=actionId%2CcenterDeadZone%2CsensitivityBallance%2CsideDeadZone";
            string responsesContent = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            int index = responsesContent.IndexOf(cursor);
            if (index < 0) return;
            string toAnalyze = responsesContent.Substring(index + cursor.Length);
            string[] parts = MainStructure.SplitBy(toAnalyze, "%7C%0D%0A");
            foreach(string aSettingString in parts)
            {
                if (aSettingString == null || aSettingString.Length < 4) continue;
                string[] innerParts = MainStructure.SplitBy(aSettingString, "%2C");
                IL2AxisSetting AxSetting = new IL2AxisSetting();
                AxSetting.Name = innerParts[0];
                AxSetting.Sensitivity = Convert.ToDouble(innerParts[2], new CultureInfo("en-US"));
                AxSetting.DeadZoneCenter = Convert.ToDouble(innerParts[1], new CultureInfo("en-US"));
                AxSetting.DeadZoneSide = Convert.ToDouble(innerParts[3], new CultureInfo("en-US"));
                axisResponsedRead.Add(innerParts[0], AxSetting);
            }
        }
        static string ConvertIL2ToDCSButtonAxis(string button)
        {
            string IL2Button = button.Substring(button.IndexOf("_") + 1);
            if (IL2Button.Substring(0, 1) == "b"&&IL2Button!= "b63_povnu")
            {
                return "JOY_BTN" + (Convert.ToInt32(IL2Button.Substring(1)) + 1).ToString();
            }else if(IL2Button== "b63_povnu")
            {
                return "JOY_BTN64";
            }
            else if (IL2Button.Substring(0,4)=="pov0")
            {
                string result = "JOY_BTN_POV0_";
                IL2Button = IL2Button.Substring(4);
                string direction = IL2Button.Substring(IL2Button.IndexOf("_") + 1);
                switch (direction)
                {
                    case "0": result = result + "U";break;
                    case "45": result = result + "UR"; break;
                    case "90": result = result + "R"; break;
                    case "135": result = result + "DR"; break;
                    case "180": result = result + "D"; break;
                    case "225": result = result + "DL"; break;
                    case "270": result = result + "L"; break;
                    case "315": result = result + "UL"; break;
                }
                return result;
            }
            else if (IL2Button.Substring(0, 3) == "pov")
            {
                int povHead = Convert.ToInt32(IL2Button.Substring(3, 1));
                //(pov-1)*8+80+direction
                string direction = IL2Button.Substring(IL2Button.IndexOf("_") + 1);
                int addition = -1;
                switch (direction)
                {
                    case "0": addition = 0; break;
                    case "45": addition = 1; break;
                    case "90": addition = 2; break;
                    case "135": addition = 3; break;
                    case "180": addition = 4; break;
                    case "225": addition = 5; break;
                    case "270": addition = 6; break;
                    case "315": addition = 7; break;
                }
                int finalButton = (povHead - 1) * 8 + 80 + addition;
                return "JOY_BTN" + finalButton.ToString();
            }
            else if (IL2Button.Substring(0, 4) == "axis")
            {
                string result = "JOY_";
                IL2Button = IL2Button.Substring(5);
                switch (IL2Button)
                {
                    case "x":  result=result+"X"; break;
                    case "y":  result = result + "Y"; break;
                    case "z":   result = result + "Z"; break;
                    case "w":  result = result + "RX"; break;
                    case "s":  result = result + "RY"; break;
                    case "t":  result = result + "RZ"; break;
                    case "p": result = result + "SLIDER1"; break;
                    case "q": result = result + "SLIDER2"; break;
                }
                return result;
            }
            return null;
        }

        public static string GetDCSNameForStick(string guid, string rawstick)
        {
            string DCSstick = InternalDataManagement.GetJoystickByIL2GUID(guid); ;
            if(DCSstick == null)
            {
                DCSstick = rawstick.Split('{')[0] + "{";
                string[] parameterCells = guid.Split('-');
                DCSstick = DCSstick + parameterCells[0].ToUpper() + "-" + parameterCells[1].ToUpper() + "-" + parameterCells[2].ToLower() + "N0TF-0000000000000000";
            }
            return DCSstick;
        }

        public static void ImportInputs(bool sensitivity, bool deadzone, bool inverted)
        {
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            if (MainStructure.msave.IL2OR == null) MainStructure.msave.IL2OR = "";
            if (((MainStructure.msave.IL2OR == null || !Directory.Exists(MainStructure.msave.IL2OR)) &&
                (MiscGames.IL2Instance == null || !Directory.Exists(MiscGames.IL2Instance))))
                return;
            List<string> KeyboardInputs = new List<string>();
            Dictionary<string, List<string>> JoystickInputs = new Dictionary<string, List<string>>();
            List<string> JPKeyboardInputs = new List<string>();
            Dictionary<string, List<string>> JPJoystickInputs = new Dictionary<string, List<string>>();
            ReadActionsFromActions(JPKeyboardInputs, JPJoystickInputs, "jp");
            ReadActionsFromActions(KeyboardInputs, JoystickInputs);
            ReadResponsesFile();
            Dictionary<int, string> IL2Sticks= new Dictionary<int, string>();
            LoadIL2Joysticks(IL2Sticks);
            foreach(KeyValuePair<string, List<string>> kvp in JoystickInputs)
            {
                for(int i=0; i<kvp.Value.Count; ++i)
                {
                    string input = GetComponentFromActionLine(kvp.Value[i], IL2ActionComponent.Input);
                    string invertRaw = GetComponentFromActionLine(kvp.Value[i], IL2ActionComponent.Inverted);
                    string positive, negative=null, mod_positive=null, mod_negative=null, rawPositive=null, rawNegative=null;
                    bool invert;
                    if (invertRaw == "0") invert = false;
                    else invert = true;
                    bool noMatchPos = true;
                    bool noMatchNeg = true;
                    Bind bpos = null;
                    Bind bneg = null;
                    string jp2Name = null;
                    if (input.Contains('/'))
                    {
                        string[] splitted = input.Split('/');
                        positive = splitted[0];
                        negative = splitted[1];
                        rawPositive = positive;
                        rawNegative = negative;
                    }
                    else
                    {
                        positive = input;
                        rawPositive = positive;
                    }
                    if (positive.Contains('+'))
                    {
                        string[] splitted = positive.Split('+');
                        mod_positive = splitted[0];
                        positive = splitted[1];
                    }
                    if (negative!=null&&negative.Contains('+'))
                    {
                        string[] splitted = negative.Split('+');
                        mod_negative = splitted[0];
                        negative = splitted[1];
                    }
                    int pv = Convert.ToInt32(positive.Split('_')[0].Replace("joy", ""));
                    string stick = IL2Sticks[pv];
                    string DCSstick = GetDCSNameForStick(stick.Substring(stick.IndexOf("{") + 1).Replace("}", ""),stick);
                    string btnInput = ConvertIL2ToDCSButtonAxis(positive);
                    string mod_pos_stick = null, mod_pos_btn = null, mod_neg_stick = null, mod_neg_btn = null, DCSstickneg=null, btnInputneg=null;
                    if (mod_positive != null)
                    {
                        if (mod_positive.Contains("key_"))
                        {
                            mod_pos_stick = "Keyboard";
                        }
                        else
                        {
                            int mpv = Convert.ToInt32(mod_positive.Split('_')[0].Replace("joy", ""));
                            string tempstick = IL2Sticks[mpv];
                            mod_pos_stick = GetDCSNameForStick(tempstick.Substring(tempstick.IndexOf("{") + 1).Replace("}", ""),tempstick);
                        }
                        
                        mod_pos_btn = ConvertIL2ToDCSButtonAxis(mod_positive);
                    }
                    if (negative != null)
                    {
                        int mpv = Convert.ToInt32(negative.Split('_')[0].Replace("joy", ""));
                        string tempstick = IL2Sticks[mpv];
                        DCSstickneg = GetDCSNameForStick(tempstick.Substring(tempstick.IndexOf("{") + 1).Replace("}", ""),tempstick);
                        btnInputneg = ConvertIL2ToDCSButtonAxis(negative);
                        if (mod_negative != null)
                        {
                            if (mod_negative.Contains("key_"))
                            {
                                int mpvn = Convert.ToInt32(mod_negative.Split('_')[0].Replace("joy", ""));
                                tempstick = IL2Sticks[mpvn];
                                mod_neg_stick = GetDCSNameForStick(tempstick.Substring(tempstick.IndexOf("{") + 1).Replace("}", ""),tempstick);
                                if (mod_neg_stick == null) mod_neg_stick = InternalDataManagement.GetJoystickByName(tempstick.Substring(0, tempstick.IndexOf("{") - 1));
                            }
                            else
                            {
                                mod_neg_stick = "Keyboard";
                            }
                            
                            mod_neg_btn = ConvertIL2ToDCSButtonAxis(mod_negative); 
                        }
                    }
                    string sid = kvp.Key;
                    string sidneg = kvp.Key;
                    if (negative != null)
                    {
                        sid += "+";
                        sidneg += "-";
                    }
                    OtherGameInput[] found = DBLogic.GetAllOtherGameInputsWithId(sid, "IL2Game");
                    bool axis;
                    string rawGroupsNeg = null;
                    if (found == null && found.Length < 1) axis = false;
                    else
                    {
                        axis = found[0].IsAxis;
                    }
                    if (JPJoystickInputs.ContainsKey(kvp.Key))
                    {
                        for(int j=0; j< JPJoystickInputs[kvp.Key].Count; ++j)
                        {
                            string jpInput = GetComponentFromActionLine(JPJoystickInputs[kvp.Key][j], IL2ActionComponent.Input);
                            string jpName = GetComponentFromActionLine(JPJoystickInputs[kvp.Key][j], IL2ActionComponent.JRelationName);
                            string jAlias = GetComponentFromActionLine(JPJoystickInputs[kvp.Key][j], IL2ActionComponent.JAlias);

                            if (jpName.Contains('/'))
                            {
                                jp2Name = jpName.Substring(jpName.IndexOf('/') + 1);
                                jpName = jpName.Substring(0, jpName.IndexOf('/'));
                            }
                            string jppositive, jpnegative=null, jpmod_positive = null, jpmod_negative = null, alias_neg=null;
                            if (jpInput.Contains('/'))
                            {
                                string[] splitted = jpInput.Split('/');
                                jppositive = splitted[0];
                                jpnegative = splitted[1];
                            }
                            else
                            {
                                jppositive = jpInput;
                                jpName = jpName + "/" + jp2Name;
                                jp2Name = null;
                            }
                            if (positive.Contains('+'))
                            {
                                string[] splitted = jppositive.Split('+');
                                jpmod_positive = splitted[0];
                                jppositive = splitted[1];
                            }
                            if (negative!=null&&negative.Contains('+'))
                            {
                                string[] splitted = negative.Split('+');
                                mod_negative = splitted[0];
                                negative = splitted[1];
                            }
                            
                            if (jAlias.Contains('/'))
                            {
                                alias_neg = jAlias.Substring(jAlias.IndexOf('/') + 1);
                                jAlias = jAlias.Substring(0, jAlias.IndexOf('/'));
                            }
                            
                            if (jpName.Length>1&&positive==jppositive&&mod_positive==jpmod_positive)
                            {
                                bpos = InternalDataManagement.GetBindForRelation(jpName);
                                if (bpos != null)
                                {

                                    if (DCSstick == bpos.Joystick && ((bpos.Rl.ISAXIS && bpos.JAxis == btnInput) || (!bpos.Rl.ISAXIS && bpos.JButton == btnInput)) &&
                                        (!sensitivity || (axisResponsedRead.ContainsKey(kvp.Key) && bpos.Curvature[0] == axisResponsedRead[kvp.Key].Sensitivity * 2)) &&
                                        (!deadzone || (axisResponsedRead.ContainsKey(kvp.Key) && ((bpos.Slider == true && bpos.Deadzone == axisResponsedRead[kvp.Key].DeadZoneSide * 4) ||
                                        ((bpos.Slider == false || bpos.Slider == null) && bpos.Deadzone == axisResponsedRead[kvp.Key].DeadZoneCenter * 4))))&&
                                        ((!inverted || !bpos.Rl.ISAXIS)||(bpos.Rl.ISAXIS&&bpos.Inverted==invert)))
                                    {
                                        if (bpos.Rl.GetRelationItem(sid, "IL2Game") == null)
                                        {
                                            bpos.Rl.AddNode(sid, "IL2Game", axis, "IL2Game");
                                        }
                                    }
                                    else
                                    {
                                        Relation r = new Relation();
                                        r.AddNode(sid, "IL2Game", axis, "IL2Game");
                                        r.ISAXIS = axis;
                                        string rawGroups = GetComponentFromActionLine(JPJoystickInputs[kvp.Key][j], IL2ActionComponent.JGroups);
                                        if (rawGroups != null)
                                        {
                                            if (rawGroups.Contains('/'))
                                            {
                                                rawGroupsNeg = rawGroups.Substring(rawGroups.IndexOf('/') + 1);
                                                rawGroups = rawGroups.Substring(0, rawGroups.IndexOf('/'));
                                            }
                                            string[] groupsp = rawGroups.Split(',');
                                            for (int k = 0; k < groupsp.Length; ++k)
                                            {
                                                r.Groups.Add(groupsp[k]);
                                                if (!InternalDataManagement.AllGroups.Contains(groupsp[k]))
                                                {
                                                    InternalDataManagement.AllGroups.Add(groupsp[k]);
                                                }
                                            }
                                        }
                                        string name = bpos.Rl.NAME;
                                        while (InternalDataManagement.AllRelations.ContainsKey(name))
                                        {
                                            name += "ILGAME(COPY)";
                                        }
                                        r.NAME = name;
                                        Bind b = new Bind(r);
                                        r.bind = b;
                                        if (axis)
                                        {
                                            b.JAxis = btnInput;
                                        }
                                        else
                                        {
                                            b.JButton = btnInput;
                                        }
                                        b.Joystick = DCSstick;
                                        if (axisResponsedRead.ContainsKey(kvp.Key))
                                        {
                                            b.Curvature[0] = axisResponsedRead[kvp.Key].Sensitivity * 2;
                                            if (axisResponsedRead[kvp.Key].DeadZoneCenter > 0.00)
                                            {
                                                b.Slider = false;
                                                b.Deadzone = axisResponsedRead[kvp.Key].DeadZoneCenter * 4;
                                            }
                                            else
                                            {
                                                b.Slider = true;
                                                b.Deadzone = axisResponsedRead[kvp.Key].DeadZoneSide * 4;
                                            }
                                        }
                                        if (mod_positive != null)
                                        {
                                            string reformer = mod_pos_btn + "§" + mod_pos_stick + "§" + mod_pos_btn;
                                            if (!b.AllReformers.Contains(reformer))
                                                b.AllReformers.Add(reformer);
                                        }
                                        if (jAlias.Length > 1)
                                        {
                                            b.aliasJoystick = jAlias;
                                            if (!InternalDataManagement.JoystickAliases.ContainsKey(DCSstick))
                                                InternalDataManagement.JoystickAliases.Add(DCSstick, jAlias);
                                            else
                                                b.aliasJoystick = InternalDataManagement.JoystickAliases[DCSstick];
                                        }

                                        InternalDataManagement.AllBinds.Add(r.NAME, b);
                                        InternalDataManagement.AllRelations.Add(r.NAME, r);
                                        //Create new Relation with different name
                                    }
                                }
                                else
                                {
                                    //Create new
                                    Relation r = new Relation();
                                    r.AddNode(sid, "IL2Game", axis, "IL2Game");
                                    r.ISAXIS = axis;
                                    string rawGroups = GetComponentFromActionLine(JPJoystickInputs[kvp.Key][j], IL2ActionComponent.JGroups);
                                    if (rawGroups != null)
                                    {
                                        if (rawGroups.Contains('/'))
                                        {
                                            rawGroupsNeg = rawGroups.Substring(rawGroups.IndexOf('/') + 1);
                                            rawGroups = rawGroups.Substring(0, rawGroups.IndexOf('/'));
                                        }
                                        string[] groupsp = rawGroups.Split(',');
                                        for (int k = 0; k < groupsp.Length; ++k)
                                        {
                                            r.Groups.Add(groupsp[k]);
                                            if (!InternalDataManagement.AllGroups.Contains(groupsp[k]))
                                            {
                                                InternalDataManagement.AllGroups.Add(groupsp[k]);
                                            }
                                        }
                                    }
                                    string name = jpName;
                                    while (InternalDataManagement.GetBindForRelation(name) != null)
                                    {
                                        name = name + "1";
                                    }
                                    r.NAME = name;
                                    Bind b = new Bind(r);
                                    r.bind = b;
                                    if (axis)
                                    {
                                        b.JAxis = btnInput;
                                    }
                                    else
                                    {
                                        b.JButton = btnInput;
                                    }
                                    b.Joystick = DCSstick;
                                    if (axisResponsedRead.ContainsKey(kvp.Key))
                                    {
                                        b.Curvature[0] = axisResponsedRead[kvp.Key].Sensitivity * 2;
                                        if (axisResponsedRead[kvp.Key].DeadZoneCenter > 0.00)
                                        {
                                            b.Slider = false;
                                            b.Deadzone = axisResponsedRead[kvp.Key].DeadZoneCenter * 4;
                                        }
                                        else
                                        {
                                            b.Slider = true;
                                            b.Deadzone = axisResponsedRead[kvp.Key].DeadZoneSide * 4;
                                        }
                                    }
                                    if (mod_positive != null)
                                    {
                                        string reformer = mod_pos_btn + "§" + mod_pos_stick + "§" + mod_pos_btn;
                                        if (!b.AllReformers.Contains(reformer))
                                            b.AllReformers.Add(reformer);
                                    }
                                    if (jAlias.Length > 1)
                                    {
                                        b.aliasJoystick = jAlias;
                                        if (!InternalDataManagement.JoystickAliases.ContainsKey(DCSstick))
                                            InternalDataManagement.JoystickAliases.Add(DCSstick, jAlias);
                                        else
                                            b.aliasJoystick = InternalDataManagement.JoystickAliases[DCSstick];
                                    }

                                    InternalDataManagement.AllBinds.Add(r.NAME, b);
                                    InternalDataManagement.AllRelations.Add(r.NAME, r);
                                }
                                noMatchPos = false;
                            }
                            if ((jp2Name!=null&&jp2Name.Length > 1 && negative == jpnegative && mod_negative == jpmod_negative))
                            {
                                bneg = InternalDataManagement.GetBindForRelation(jp2Name);
                                if (bneg != null)
                                {

                                    if (DCSstickneg == bneg.Joystick && ((bneg.Rl.ISAXIS && bneg.JAxis == btnInputneg) || (!bneg.Rl.ISAXIS && bneg.JButton == btnInputneg)) &&
                                        (!sensitivity || (axisResponsedRead.ContainsKey(kvp.Key) && bneg.Curvature[0] == axisResponsedRead[kvp.Key].Sensitivity * 2)) &&
                                        (!deadzone || (axisResponsedRead.ContainsKey(kvp.Key) && ((bneg.Slider == true && bneg.Deadzone == axisResponsedRead[kvp.Key].DeadZoneSide * 4) ||
                                        ((bneg.Slider == false || bneg.Slider == null) && bneg.Deadzone == axisResponsedRead[kvp.Key].DeadZoneCenter * 4)))) &&
                                        ((!inverted || !bneg.Rl.ISAXIS) || (bneg.Rl.ISAXIS && bneg.Inverted == invert)))
                                    {
                                        if (bneg.Rl.GetRelationItem(sidneg, "IL2Game") == null)
                                        {
                                            bneg.Rl.AddNode(sidneg, "IL2Game", axis, "IL2Game");
                                        }
                                    }
                                    else
                                    {
                                        Relation r = new Relation();
                                        r.AddNode(sid, "IL2Game", axis, "IL2Game");
                                        r.ISAXIS = axis;
                                        string rawGroups = GetComponentFromActionLine(JPJoystickInputs[kvp.Key][j], IL2ActionComponent.JGroups);
                                        if (rawGroups != null)
                                        {
                                            if (rawGroups.Contains('/'))
                                            {
                                                rawGroupsNeg = rawGroups.Substring(rawGroups.IndexOf('/') + 1);
                                                rawGroups = rawGroups.Substring(0, rawGroups.IndexOf('/'));
                                            }
                                            string[] groupsp = rawGroupsNeg.Split(',');
                                            for (int k = 0; k < groupsp.Length; ++k)
                                            {
                                                r.Groups.Add(groupsp[k]);
                                                if (!InternalDataManagement.AllGroups.Contains(groupsp[k]))
                                                {
                                                    InternalDataManagement.AllGroups.Add(groupsp[k]);
                                                }
                                            }
                                        }
                                        string name = bneg.Rl.NAME;
                                        while (InternalDataManagement.AllRelations.ContainsKey(name))
                                        {
                                            name += "ILGAME(COPY)";
                                        }
                                        r.NAME = name;
                                        Bind b = new Bind(r);
                                        r.bind = b;
                                        if (axis)
                                        {
                                            b.JAxis = btnInputneg;
                                        }
                                        else
                                        {
                                            b.JButton = btnInputneg;
                                        }
                                        b.Joystick = DCSstickneg;
                                        if (axisResponsedRead.ContainsKey(kvp.Key))
                                        {
                                            b.Curvature[0] = axisResponsedRead[kvp.Key].Sensitivity * 2;
                                            if (axisResponsedRead[kvp.Key].DeadZoneCenter > 0.00)
                                            {
                                                b.Slider = false;
                                                b.Deadzone = axisResponsedRead[kvp.Key].DeadZoneCenter * 4;
                                            }
                                            else
                                            {
                                                b.Slider = true;
                                                b.Deadzone = axisResponsedRead[kvp.Key].DeadZoneSide * 4;
                                            }
                                        }
                                        if (mod_negative != null)
                                        {
                                            string reformer = mod_neg_btn + "§" + mod_neg_stick + "§" + mod_neg_btn;
                                            if (!b.AllReformers.Contains(reformer))
                                                b.AllReformers.Add(reformer);
                                        }
                                        if (alias_neg.Length > 1)
                                        {
                                            b.aliasJoystick = alias_neg;
                                            if (!InternalDataManagement.JoystickAliases.ContainsKey(DCSstickneg))
                                                InternalDataManagement.JoystickAliases.Add(DCSstickneg, alias_neg);
                                            else
                                                b.aliasJoystick = InternalDataManagement.JoystickAliases[DCSstickneg];
                                        }

                                        InternalDataManagement.AllBinds.Add(r.NAME, b);
                                        InternalDataManagement.AllRelations.Add(r.NAME, r);
                                        //Create new Relation with different name
                                    }
                                }
                                else
                                {
                                    //Create new
                                    Relation r = new Relation();
                                    r.AddNode(sid, "IL2Game", axis, "IL2Game");
                                    r.ISAXIS = axis;
                                    string rawGroups = GetComponentFromActionLine(JPJoystickInputs[kvp.Key][j], IL2ActionComponent.JGroups);
                                    if (rawGroups != null)
                                    {
                                        if (rawGroups.Contains('/'))
                                        {
                                            rawGroupsNeg = rawGroups.Substring(rawGroups.IndexOf('/') + 1);
                                            rawGroups = rawGroups.Substring(0, rawGroups.IndexOf('/'));
                                        }
                                        if (rawGroupsNeg != null)
                                        {
                                            string[] groupsp = rawGroupsNeg.Split(',');
                                            for (int k = 0; k < groupsp.Length; ++k)
                                            {
                                                r.Groups.Add(groupsp[k]);
                                                if (!InternalDataManagement.AllGroups.Contains(groupsp[k]))
                                                {
                                                    InternalDataManagement.AllGroups.Add(groupsp[k]);
                                                }
                                            }
                                        }
                                    }
                                    string name = jp2Name;
                                    r.NAME = name;
                                    Bind b = new Bind(r);
                                    r.bind = b;
                                    if (axis)
                                    {
                                        b.JAxis = btnInputneg;
                                    }
                                    else
                                    {
                                        b.JButton = btnInputneg;
                                    }
                                    b.Joystick = DCSstickneg;
                                    if (axisResponsedRead.ContainsKey(kvp.Key))
                                    {
                                        b.Curvature[0] = axisResponsedRead[kvp.Key].Sensitivity * 2;
                                        if (axisResponsedRead[kvp.Key].DeadZoneCenter > 0.00)
                                        {
                                            b.Slider = false;
                                            b.Deadzone = axisResponsedRead[kvp.Key].DeadZoneCenter * 4;
                                        }
                                        else
                                        {
                                            b.Slider = true;
                                            b.Deadzone = axisResponsedRead[kvp.Key].DeadZoneSide * 4;
                                        }
                                    }
                                    if (mod_negative != null)
                                    {
                                        string reformer = mod_neg_btn + "§" + mod_neg_stick + "§" + mod_neg_btn;
                                        if (!b.AllReformers.Contains(reformer))
                                            b.AllReformers.Add(reformer);
                                    }
                                    if (alias_neg!=null&&alias_neg.Length > 1)
                                    {
                                        b.aliasJoystick = alias_neg;
                                        if (!InternalDataManagement.JoystickAliases.ContainsKey(DCSstickneg))
                                            InternalDataManagement.JoystickAliases.Add(DCSstickneg, alias_neg);
                                        else
                                            b.aliasJoystick = InternalDataManagement.JoystickAliases[DCSstickneg];
                                    }
                                    InternalDataManagement.AllBinds.Add(r.NAME, b);
                                    InternalDataManagement.AllRelations.Add(r.NAME, r);
                                }
                                noMatchNeg = false;
                            }
                        }
                    }
                    bool toSendInv, sliderToSend;
                    double deadZoneToSet = double.NaN;
                    if (axis && inverted) toSendInv = invert;
                    else toSendInv = Bind.Inverted_Default;
                    if (axis && deadzone)
                    {
                        if (axisResponsedRead.ContainsKey(kvp.Key))
                        {
                            if (axisResponsedRead[kvp.Key].DeadZoneCenter > 0.00)
                            {
                                sliderToSend = false;
                                deadZoneToSet = axisResponsedRead[kvp.Key].DeadZoneCenter * 4;
                            }
                            else
                            {
                                sliderToSend = true;
                                deadZoneToSet = axisResponsedRead[kvp.Key].DeadZoneSide * 4;
                            }
                        }
                        else
                        {
                            sliderToSend = Bind.Slider_Default;
                            deadZoneToSet = Bind.Deadzone_Default;
                        }
                    }
                    else
                    {
                        sliderToSend = Bind.Slider_Default;
                        deadZoneToSet = Bind.Deadzone_Default;
                    }
                    if (noMatchPos)
                    {
                        Bind bT = null;
                        if (axis)
                        {
                            List<Bind> matchingb = InternalDataManagement.GetBindsByJoystickAndKey(DCSstick, btnInput, axis, toSendInv, sliderToSend, Bind.SaturationX_Default, Bind.SaturationY_Default, deadZoneToSet);
                            if (matchingb.Count > 0) bT = matchingb[0];
                        }
                        else
                        {
                            string rlname= InternalDataManagement.GetRelationNameForJostickButton(DCSstick, rawPositive);
                            if (rlname != null)
                                bT = InternalDataManagement.GetBindForRelation(rlname);
                        }
                        
                        if (bT != null)
                        {
                            bT.Rl.AddNode(sid, "IL2Game", axis, "IL2Game");
                        }
                        else
                        {
                            Relation r = new Relation();
                            r.AddNode(sid, "IL2Game", axis, "IL2Game");
                            r.ISAXIS = axis;
                            string shorten = MainStructure.ShortenDeviceName(DCSstick);
                            shorten = shorten + btnInput;
                            while (InternalDataManagement.AllRelations.ContainsKey(shorten))
                            {
                                shorten += "1";
                            }
                            string name = shorten;
                            r.NAME = name;
                            Bind b = new Bind(r);
                            r.bind = b;
                            if (axis)
                            {
                                b.JAxis = btnInput;
                            }
                            else
                            {
                                b.JButton = btnInput;
                            }
                            b.Joystick = DCSstick;
                            if (axisResponsedRead.ContainsKey(kvp.Key))
                            {
                                b.Curvature[0] = axisResponsedRead[kvp.Key].Sensitivity * 2;
                                b.Slider = sliderToSend;
                                b.Deadzone = deadZoneToSet;
                            }
                            if (mod_positive != null)
                            {
                                string reformer = mod_pos_btn + "§" + mod_pos_stick + "§" + mod_pos_btn;
                                if (!b.AllReformers.Contains(reformer))
                                    b.AllReformers.Add(reformer);
                            }

                            InternalDataManagement.AllBinds.Add(r.NAME, b);
                            InternalDataManagement.AllRelations.Add(r.NAME, r);
                        }
                        InternalDataManagement.ResyncRelations();
                    }
                    if (noMatchNeg&&negative!=null)
                    {
                        Bind bT = null;
                        if (axis)
                        {
                            List<Bind> matchingb = InternalDataManagement.GetBindsByJoystickAndKey(DCSstickneg, btnInputneg, axis, toSendInv, sliderToSend, Bind.SaturationX_Default, Bind.SaturationY_Default, deadZoneToSet);
                            if (matchingb.Count > 0) bT = matchingb[0];
                        }
                        else
                        {
                            string rlname = InternalDataManagement.GetRelationNameForJostickButton(DCSstick, rawNegative);
                            if (rlname != null)
                                bT = InternalDataManagement.GetBindForRelation(rlname);
                        }
                        if (bT != null)
                        {
                            bT.Rl.AddNode(sid, "IL2Game", axis, "IL2Game");
                        }
                        else
                        {
                            Relation r = new Relation();
                            r.AddNode(sid, "IL2Game", axis, "IL2Game");
                            r.ISAXIS = axis;
                            string shorten = MainStructure.ShortenDeviceName(DCSstickneg);
                            shorten = shorten + btnInputneg;
                            while (InternalDataManagement.AllRelations.ContainsKey(shorten))
                            {
                                shorten += "1";
                            }
                            string name = shorten;
                            r.NAME = name;
                            Bind b = new Bind(r);
                            r.bind = b;
                            if (axis)
                            {
                                b.JAxis = btnInputneg;
                            }
                            else
                            {
                                b.JButton = btnInputneg;
                            }
                            b.Joystick = DCSstickneg;
                            if (axisResponsedRead.ContainsKey(kvp.Key))
                            {
                                b.Curvature[0] = axisResponsedRead[kvp.Key].Sensitivity * 2;
                                b.Slider = sliderToSend;
                                b.Deadzone = deadZoneToSet;
                            }
                            if (mod_negative != null)
                            {
                                string reformer = mod_pos_btn + "§" + mod_pos_stick + "§" + mod_pos_btn;
                                if (!b.AllReformers.Contains(reformer))
                                    b.AllReformers.Add(reformer);
                            }
                            InternalDataManagement.AllBinds.Add(r.NAME, b);
                            InternalDataManagement.AllRelations.Add(r.NAME, r);
                        }
                        InternalDataManagement.ResyncRelations();
                    }
                }
                
            }
        }
        static string GetComponentFromActionLine(string line, IL2ActionComponent comp)
        {
            if(comp== IL2ActionComponent.ID)
            {
                return line.Split(',')[0];
            }else if (comp == IL2ActionComponent.Input)
            {
                return line.Split(',')[1].TrimStart();
            }else if(comp == IL2ActionComponent.Inverted)
            {
                return line.Substring(line.IndexOf("|") - 1, 1);
            }else if(comp == IL2ActionComponent.JRelationName)
            {
                string cmp = "//JP:";
                int fst = line.IndexOf(cmp);
                if (fst < 0) return "";
                string j1 = line.Substring(fst + cmp.Length);
                string jp1 = j1.Substring(0, j1.IndexOf(';'));
                string rest = j1.Substring(j1.IndexOf(';') + 1);
                int scnd = rest.IndexOf(cmp);
                if (scnd > -1)
                {
                    string j2 = rest.Substring(rest.IndexOf(cmp) + cmp.Length);
                    string jp2 = j2.Substring(0, j2.IndexOf(';'));
                    return jp1 + "/" + jp2;
                }
                else
                {
                    return jp1;
                }
            }else if(comp == IL2ActionComponent.JGroups)
            {
                string cmp = "//JP:";
                int fst = line.IndexOf(cmp);
                if (fst < 0) return "";
                string j1 = line.Substring(fst + cmp.Length);
                string rest = j1.Substring(j1.IndexOf(';') + 1);
                string g1 = rest.Substring(0, rest.IndexOf(';'));
                rest = rest.Substring(rest.IndexOf(';') + 1);
                int scnd = rest.IndexOf(cmp);
                if (scnd > -1)
                {
                    string j2 = rest.Substring(rest.IndexOf(cmp) + cmp.Length);
                    rest = j2.Substring(j2.IndexOf(';')+1);
                    string g2 = rest.Substring(0, rest.IndexOf(';'));
                    return g1 + "/" + g2;
                }
                else
                {
                    return g1;
                }
            }else if(comp == IL2ActionComponent.JAlias)
            {
                string cmp = "//JP:";
                int fst = line.IndexOf(cmp);
                if (fst < 0) return "";
                string j1 = line.Substring(fst + cmp.Length);
                string rest = j1.Substring(j1.IndexOf(';') + 1);
                string jal1 = rest.Substring(rest.IndexOf(';') + 1);
                int scnd = rest.IndexOf(cmp);
                if (scnd < 0)
                {
                    return jal1;
                }
                else
                {
                    rest = jal1.Substring(jal1.IndexOf(cmp) + cmp.Length);
                    jal1 = jal1.Substring(0, jal1.IndexOf(cmp));
                    rest = rest.Substring(rest.IndexOf(';') + 1);
                    string jal2 = rest.Substring(rest.IndexOf(';') + 1);
                    return jal1 + "/" + jal2;
                }
            }
            else
            {
                string cmp = "//";
                int fst = line.IndexOf(cmp);
                if (fst < 0) return "";
                string rest = line.Substring(line.IndexOf(cmp) + cmp.Length);
                int scnd = rest.IndexOf("//JP:");
                if (scnd < 0) return rest;
                else return rest.Substring(0, scnd);
            }
        }
        public static void LoadIL2Joysticks(Dictionary<int, string> output)
        {
            string path = GetInputPath();
            if (File.Exists(path +InputPath+"devices.txt"))
            {
                StreamReader sr = new StreamReader(MiscGames.IL2Instance + "\\data\\input\\devices.txt");
                string content = sr.ReadToEnd().Replace("\r", "").Replace("|", "");
                string[] lines = content.Split('\n');
                for (int i = 1; i < lines.Length; ++i)
                {
                    string[] parts = lines[i].Split(',');
                    if (parts.Length > 2 && int.TryParse(parts[0], out int id) == true)
                    {
                        string joy = MiscGames.IL2JoyIdToDCSJoyId(parts[1], parts[2]);
                        if (!output.ContainsKey(id) && !output.ContainsValue(joy))
                        {
                            output.Add(id, joy);
                        }
                    }
                }
            }
        }
    }
}
