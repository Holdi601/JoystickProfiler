using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public enum OutputType { Clean, Add, Merge};
    public static class IL2IOLogic
    {
        static string ActionsPreFile = 
            "// Input map preset.\r\n"+
            "// It is not recommended to change anything here,\r\n"+
            "// but it is still possible on your own risk.\r\n"+
            "// I am trying my best not to break it cheers the Joypro dev :-P\r\n"+
            "\r\n"+
            "&actions = action, command, invert|\r\n";

        static string MapPreFile = 
            "// KOS GENERATED INPUT MAPFILE\r\n"+
            "// IT IS NOT RECOMMENDED TO CHANGE ANYTHING HERE!\r\n"+
            "// (Well, it is still possible, but on your own risk ;o))\r\n"+
            "// Well I am trying shoutout to the IL Devs, cheers the JoyPro devs ;) , Btw please explain what State and Event is for in the action argument functions below kthxbye\r\n"+
            "\r\n";

        static string ResponsesPreFile = 
            "type4Devices=actionId%2ChighValuesDeadZone%2ClowValuesDeadZone&" +
            "type2Devices=actionId%2ChighValuesDeadZone%2ClowValuesDeadZone%7C%0D%0A" +
            "rpc_all_engines_throttles%2C0%2C0&" +
            "type1Devices=actionId%2CcenterDeadZone%2CsensitivityBallance%2CsideDeadZone";

        static string DevicesPreFile = "configId,guid,model|\r\r\n";
        public static List<string> ActionsFileKeyContent = new List<string>();
        public static List<string> ActionsFileKeyboardContent = new List<string>();
        public static List<string> MapFileKeyboardModifier = new List<string>();
        public static List<string> MapFileContentModifier = new List<string>();
        public static List<string> MapActionKeyboard = new List<string>();
        public static List<string> MapActionContent = new List<string>();
        public static List<string> MapEndOfFile = new List<string>();
        public static List<string> usedCommands = new List<string>();
        public static Dictionary<string, Bind> axisSettings = new Dictionary<string, Bind>();
        static string InputPath = "\\data\\input\\";
        static List<string> Modifier = new List<string>();
        static Dictionary<string, IL2AxisReplacementButtons> axesButtonCommandOtherHalfMissing = new Dictionary<string, IL2AxisReplacementButtons>();
        public static void FillBindsIntoOutput(List<Bind> toExport)
        {
            if (((MiscGames.IL2PathOverride == null || !Directory.Exists(MiscGames.IL2PathOverride)) &&
                (MiscGames.IL2Instance == null || !Directory.Exists(MiscGames.IL2Instance))|| toExport==null))
                return;
            clearAll();
            createDeviceFile();
            for(int i=0; i<toExport.Count; ++i)
            {
                List<string> lines = BindToActionString(toExport[i]);
                for(int j=0; j<lines.Count; ++j)
                {
                    ActionsFileKeyContent.Add(lines[j]);
                }
            }
            ReadKeyboardActions();


            createResponsesFile();
        }
        static void clearAll()
        {
            ActionsFileKeyContent = new List<string>();
            ActionsFileKeyboardContent = new List<string>();
            MapFileKeyboardModifier = new List<string>();
            MapFileContentModifier = new List<string>();
            MapActionKeyboard = new List<string>();
            MapActionContent = new List<string>();
            MapEndOfFile = new List<string>();
            usedCommands = new List<string>();
            axisSettings = new Dictionary<string, Bind>();
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
        static void ReadKeyboardActions()
        {
            string path = GetInputPath();
            StreamReader sr = new StreamReader(path + InputPath + "current.actions");
            bool commandsStart = false;
            while (!commandsStart)
            {
                string line = sr.ReadLine();
                if (line.ToLower().Contains(("&actions = action, command, invert |").ToLower()))
                    commandsStart = true;
            }
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();
                if (!(currentLine.Contains("joy0") ||
                    currentLine.Contains("joy1") ||
                    currentLine.Contains("joy2") ||
                    currentLine.Contains("joy3") ||
                    currentLine.Contains("joy4") ||
                    currentLine.Contains("joy5") ||
                    currentLine.Contains("joy6") ||
                    currentLine.Contains("joy7") ||
                    currentLine.Contains("joy8") ||
                    currentLine.Contains("joy9") ||
                    currentLine.Contains("joy10") ||
                    currentLine.Contains("joy11")))
                {
                    ActionsFileKeyboardContent.Add(currentLine);
                }
            }
            sr.Close();
            sr.Dispose();
        }
        static List<string> BindToActionString(Bind b)
        {
            List<string> result = new List<string>();
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
                            axesButtonCommandOtherHalfMissing[newId].positive = toIL2JoystickKeyAxisString(b);
                        }
                        else
                        {
                            axesButtonCommandOtherHalfMissing[newId].bn = b;
                            axesButtonCommandOtherHalfMissing[newId].negative = toIL2JoystickKeyAxisString(b);
                        }
                        axesButtonCommandOtherHalfMissing[newId].id = newId;
                        dualSetNeeded = true;
                        id = newId;
                    }
                    newId = id;
                    id = id + ",";
                    for (int j = id.Length; j <= 50; ++j)
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
                        id=id+ toIL2JoystickKeyAxisString(b)+",";
                    }
                    if (!usedCommands.Contains(newId))
                        usedCommands.Add(newId);
                    for (int j = id.Length; j <= 100; ++j)
                    {
                        id = id + " ";
                    }
                    if (b.Rl.ISAXIS)
                    {
                        if (!axisSettings.ContainsKey(newId))
                        {
                            axisSettings.Add(newId, b);
                        }
                        if (b.Inverted == true)
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
                            if (InternalDataMangement.JoystickAliases.ContainsKey(b.Joystick) &&
                                InternalDataMangement.JoystickAliases[b.Joystick].Length > 2)
                            {
                                id = id + InternalDataMangement.JoystickAliases[b.Joystick];
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
                            if (InternalDataMangement.JoystickAliases.ContainsKey(b.Joystick) &&
                                InternalDataMangement.JoystickAliases[b.Joystick].Length > 2)
                            {
                                id = id + InternalDataMangement.JoystickAliases[b.Joystick];
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
                        if(InternalDataMangement.JoystickAliases.ContainsKey(b.Joystick)&&
                            InternalDataMangement.JoystickAliases[b.Joystick].Length > 2)
                        {
                            id = id + InternalDataMangement.JoystickAliases[b.Joystick];
                        }
                    }
                    id = id + "\r\n";
                    result.Add(id);
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
                for (int j = 0; j < InternalDataMangement.LocalJoysticks.Length; ++j)
                {
                    if (joystick == InternalDataMangement.LocalJoysticks[j])
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
        static string toIL2JoystickKeyAxisString(Bind b)
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
        public static void createDeviceFile()
        {
            string path = GetInputPath();
            StreamWriter swr = new StreamWriter(path + InputPath + "devices.txt");
            swr.Write(DevicesPreFile);
            for(int i=0; i<InternalDataMangement.LocalJoysticks.Length-1; ++i)
            {
                swr.Write(MiscGames.DCSJoyIdToIL2JoyId(InternalDataMangement.LocalJoysticks[i], i)+"|\r\r\n");
            }
            swr.Write(MiscGames.DCSJoyIdToIL2JoyId(
                InternalDataMangement.LocalJoysticks[InternalDataMangement.LocalJoysticks.Length-1],
                InternalDataMangement.LocalJoysticks.Length - 1) + "\r\r");
            swr.Flush();
            swr.Close();
            swr.Dispose();
        }
        public static void createResponsesFile()
        {
            string path = GetInputPath();
            StreamWriter swr = new StreamWriter(path + InputPath + "current.responses");
            swr.Write(ResponsesPreFile);
            foreach(KeyValuePair<string, Bind> kvp in axisSettings)
            {
                swr.Write("%7C%0D%0A"+kvp.Key+ "%2C");
                if (kvp.Value.Slider == true)
                {
                    swr.Write("0.0%2C");
                    if (kvp.Value.Curvature.Count == 1)
                    {
                        swr.Write(Math.Round(kvp.Value.Curvature[1] * 0.5, 2).ToString() + "%2C" +
                            Math.Round(kvp.Value.Deadzone * 0.25, 2).ToString());
                    }
                    else
                    {
                        //Custom curves implementation needed here if they dont get overwritten
                        swr.Write("0.2" + "%2C" +
                            Math.Round(kvp.Value.Deadzone * 0.25, 2).ToString());
                    }
                }
                else
                {
                    swr.Write(Math.Round(kvp.Value.Deadzone * 0.25, 2).ToString() + "%2C");
                    if (kvp.Value.Curvature.Count == 1)
                    {
                        swr.Write(Math.Round(kvp.Value.Curvature[1] * 0.5, 2).ToString() + "%2C0.0");
                    }
                    else
                    {
                        //Custom curves implementation needed here if they dont get overwritten
                        swr.Write("0.2" + "%2C0.0");
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
            if (MiscGames.IL2PathOverride.Length > 2) path = MiscGames.IL2PathOverride;
            else path = MiscGames.IL2Instance;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}
