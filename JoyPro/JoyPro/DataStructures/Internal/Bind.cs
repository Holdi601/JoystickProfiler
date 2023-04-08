using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class Bind
    {
        public Relation Rl;
        public string Joystick;
        public string JAxis;
        public string JButton;
        public string Reformer_depr;
        public bool? Inverted;
        public bool? Slider;
        public double Deadzone;
        public List<double> Curvature;
        public double SaturationX;
        public double SaturationY;
        public List<string> AllReformers;
        public string AdditionalImportInfo;
        public string AliasJoystick;
        public static bool Inverted_Default = false;
        public static bool Slider_Default = false;
        public static List<double> Curvature_Default = new List<double>() { 0.0 };
        public static double SaturationX_Default = 1.0;
        public static double SaturationY_Default = 1.0;
        public static double Deadzone_Default = 0.0;
        
        public string PJoystick { 
            get {
                string refStick = Joystick == null ? JoyBackup : Joystick;
                if (InternalDataManagement.LocalJoystickPGUID.ContainsKey(refStick) &&
                    InternalDataManagement.LocalJoystickPGUID[refStick].Length > 2)
                    return InternalDataManagement.LocalJoystickPGUID[refStick];
                else
                    return refStick;
            }
            set
            {
                string refStick = Joystick == null ? JoyBackup : Joystick;
                if (InternalDataManagement.LocalJoystickPGUID.ContainsKey(refStick))
                {
                    InternalDataManagement.LocalJoystickPGUID[refStick] = value;
                }
                else
                {
                    InternalDataManagement.LocalJoystickPGUID.Add(PJoystick, value);
                }
            }
        }
        
        public void CorrectJoystickName()
        {
            if (Joystick.Length > 0 && Joystick.Contains('{') && Joystick.Contains('}') && Joystick.Contains('-'))
            {
                string[] outerParts = Joystick.Split('{');
                string[] innerParts = outerParts[1].Split('-');
                Joystick = outerParts[0] + "{"+innerParts[0].ToUpper();
                for(int i=1; i<innerParts.Length; ++i)
                {
                    if (i == 2)
                    {
                        Joystick += "-" + innerParts[i].ToLower();
                    }
                    else
                    {
                        Joystick += "-" + innerParts[i].ToUpper();
                    }
                }

            }
        }
        public string[] PovHeads = new string[]
        {
            "POV1_D"
            ,"POV1_U"
            ,"POV1_L"
            ,"POV1_R"
            ,"POV1_DL"
            ,"POV1_UL"
            ,"POV1_DR"
            ,"POV1_UR"
            ,"POV2_D"
            ,"POV2_U"
            ,"POV2_L"
            ,"POV2_R"
            ,"POV2_DL"
            ,"POV2_UL"
            ,"POV2_DR"
            ,"POV2_UR"
            ,"POV3_D"
            ,"POV3_U"
            ,"POV3_L"
            ,"POV3_R"
            ,"POV3_DL"
            ,"POV3_UL"
            ,"POV3_DR"
            ,"POV3_UR"
            ,"POV4_D"
            ,"POV4_U"
            ,"POV4_L"
            ,"POV4_R"
            ,"POV4_DL"
            ,"POV4_UL"
            ,"POV4_DR"
            ,"POV4_UR"
        };
        public string JoyBackup = "";
        public bool doubleTap = false;
        public Bind Copy(Relation rl)
        {
            Bind r = new Bind(rl);
            r.Joystick=Joystick;
            r.JoyBackup = Joystick;
            r.JAxis=JAxis;
            r.JButton=JButton;
            r.Reformer_depr=Reformer_depr;
            r.Inverted=Inverted;
            r.Slider=Slider;
            r.Deadzone=Deadzone;
            r.Curvature= new List<double>();
            if (Curvature != null)
                for (int i = 0; i < Curvature.Count; ++i)
                {
                    r.Curvature.Add(Curvature[i]);
                }
            else
                r.Curvature.Add(0);
            r.SaturationX = SaturationX;
            r.SaturationY = SaturationY;
            r.AllReformers = new List<string>();
            for(int i=0; i<AllReformers.Count; ++i)
            {
                r.AllReformers.Add(AllReformers[i]);
            }
            r.Rl = rl;
            rl.bind = r;
            r.doubleTap = doubleTap;
            return r;
        }
        [JsonConstructor]
        public Bind() 
        {

        }
        public Bind(Relation r)
        {
            Joystick = "";
            JAxis = "";
            JButton = "";
            Inverted = Inverted_Default;
            Slider = Slider_Default;
            Curvature = new List<double>();
            for (int i = 0; i < Curvature_Default.Count; ++i)
                Curvature.Add(Curvature_Default[i]);
            SaturationX = SaturationX_Default;
            SaturationY = SaturationY_Default;
            Rl = r;
            Deadzone = Deadzone_Default;
            Reformer_depr = ""; 
            AllReformers = new List<string>();
            AdditionalImportInfo = "";
            r.bind = this;
            doubleTap = false;
            //ffs = new ForceFeedbackS();
        }
        public DCSAxisBind toDCSAxisBind()
        {
            DCSAxisBind result = new DCSAxisBind();
            if (JAxis.Length <1 || Joystick.Length < 1) return null;
            result.key = JAxis.ToString();
            DCSAxisFilter daf = new DCSAxisFilter();
            result.filter = daf;
            result.JPRelName = Rl.NAME;
            daf.curvature = Curvature;
            daf.deadzone = Deadzone;
            daf.inverted = Inverted ?? false;
            daf.slider = Slider ?? false;
            daf.saturationX = SaturationX;
            daf.saturationY = SaturationY;
            if (Rl.Groups != null)
            {
                result.Groups = Rl.Groups;
            }
            if (result.reformers == null) result.reformers = new List<string>();
            for (int i = 0; i < AllReformers.Count; ++i)
            {
                if (AllReformers[i].Length > 0)
                {
                    string[] parts = AllReformers[i].Split('§');
                    if (parts.Length == 3)
                    {
                        result.reformers.Add(parts[0]);
                        Modifier m = new Modifier();
                        m.name = parts[0];
                        m.device = parts[1];
                        m.sw = false;
                        m.key = parts[2];
                        result.modifiers.Add(m);
                    }
                    else if (parts.Length == 4)
                    {
                        result.reformers.Add(parts[0]);
                        Modifier m = new Modifier();
                        m.name = parts[0];
                        m.device = parts[1];
                        m.sw = Convert.ToBoolean(parts[3]);
                        m.key = parts[2];
                        result.modifiers.Add(m);
                    }
                }
            }
            result.relatedBind = this;
            return result;
        }
        public void SetBackupJoystick()
        {
            JoyBackup = Joystick;
        }
        public static Bind GetBindFromAxisElement(DCSAxisBind dab,string id, string joystick, string plane, bool inv=false, bool slid=false, bool curv=false, bool dz=false, bool sx=false, bool sy=false)
        {
            Relation r = new Relation();
            Bind b = new Bind(r);
            string shorten = MainStructure.ShortenDeviceName(joystick);
            string relationName = shorten + dab.key;
            r.ISAXIS = true;
            b.JAxis = dab.key;
            b.Joystick = joystick;
            b.JoyBackup = joystick;
            b.doubleTap=false;
            if (dab.modifiers != null)
            {
                foreach (Modifier m in dab.modifiers)
                {
                    if (DCSIOLogic.KeyboardConversion_DCS2DX.ContainsKey(m.key)) m.key = DCSIOLogic.KeyboardConversion_DCS2DX[m.key];
                    //same code as mainwindow
                    if (m.JPN != null && m.JPN.Length > 1)
                        m.name = m.JPN;

                    string reform = m.name + "§" + m.device + "§" + m.key;

                    string device;
                    if (m.device == "Keyboard")
                        device = "Keyboard";
                    else
                        device = "m" + m.device.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
                    string nameToShow;
                    nameToShow = device + m.key;
                    if (m.JPN != null && m.JPN.Length > 1)
                        b.AdditionalImportInfo += m.JPN + "§";
                    //else
                    //    nameToShow = device + m.key;

                    string moddedDevice = Bind.JoystickGuidToModifierGuid(m.device);
                    string toAdd = nameToShow + "§" + moddedDevice + "§" + m.key;
                    if (!b.AllReformers.Contains(toAdd))
                    {

                        ModExists alreadyExists = InternalDataManagement.DoesReformerExistInMods(toAdd);
                        if (alreadyExists == ModExists.NOT_EXISTENT)
                        {
                            InternalDataManagement.AddReformerToMods(toAdd);
                            b.AllReformers.Add(toAdd);
                        }
                        else if (alreadyExists == ModExists.BINDNAME_EXISTS || alreadyExists == ModExists.ALL_EXISTS)
                        {
                            Modifier mf = Modifier.ReformerToMod(toAdd);
                            toAdd = InternalDataManagement.GetReformerStringFromMod(mf.name);
                            if (!b.AllReformers.Contains(toAdd)) b.AllReformers.Add(toAdd);
                        }
                        else if (alreadyExists == ModExists.KEYBIND_EXISTS)
                        {
                            Modifier mnew = InternalDataManagement.GetModifierWithKeyCombo(m.device, m.key);
                            if (mnew != null)
                            {
                                toAdd = mnew.toReformerString();
                                if (!b.AllReformers.Contains(toAdd)) b.AllReformers.Add(toAdd);
                            }
                        }
                        else if (alreadyExists == ModExists.ERROR)
                        {

                        }
                    }
                    relationName = nameToShow + relationName + m.name;

                }
            }
            if ((dab.modifiers == null || dab.modifiers.Count == 0) && (dab.reformers != null && dab.reformers.Count > 0))
            {
                for (int i = 0; i < dab.reformers.Count; i++)
                {
                    b.AllReformers.Add(Modifier.CreateDefaultReformer(dab.reformers[i], Game.DCS));
                    relationName = relationName + dab.reformers[i];
                }
            }
            if (dab.Groups.Count > 0)
            {
                for(int i=0; i<dab.Groups.Count; ++i)
                {
                    if (!b.Rl.Groups.Contains(dab.Groups[i]))
                    {
                        b.Rl.Groups.Add(dab.Groups[i]);
                    }
                }
            }
            if (dab.JPRelName.Length > 2)
                b.AdditionalImportInfo = dab.JPRelName;
            if (dab.filter != null)
            {
                b.Inverted = dab.filter.inverted;
                b.Curvature = dab.filter.curvature;
                if (b.Curvature == null) b.Curvature = new List<double>();
                if (b.Curvature.Count < 1) b.Curvature.Add(0.0);
                b.Deadzone = dab.filter.deadzone;
                b.Slider = dab.filter.slider;
                b.SaturationX = dab.filter.saturationX;
                b.SaturationY = dab.filter.saturationY;
            }
            else
            {
                b.Inverted = false;
                b.Slider = false;
                b.Curvature = new List<double>();
                b.Curvature.Add(0.0);
                b.SaturationX = 1.0;
                b.SaturationY = 1.0;
                b.Deadzone = 0;
            }
            if (inv) relationName = relationName + "i" + b.Inverted.ToString();
            if (slid) relationName = relationName + "s" + b.Slider.ToString();
            if (curv && b.Curvature.Count > 0)
            {
                for(int i=0; i<b.Curvature.Count; ++i)
                {
                    if (b.Curvature[i].ToString(new CultureInfo("en-US")).Length > 3)
                        relationName = relationName + "c" + b.Curvature[i].ToString(new CultureInfo("en-US")).Substring(0, 4);
                    else
                        relationName = relationName + "c" + b.Curvature[i].ToString(new CultureInfo("en-US"));
                }
            }
            if (dz)
            {
                if (b.Deadzone.ToString(new CultureInfo("en-US")).Length > 3)
                    relationName = relationName + "d" + b.Deadzone.ToString(new CultureInfo("en-US")).Substring(0, 4);
                else
                    relationName = relationName + "d" + b.Deadzone.ToString(new CultureInfo("en-US"));
            }
            if (sx)
            {
                if(b.SaturationX.ToString(new CultureInfo("en-US")).Length>3)
                    relationName = relationName + "x" + b.SaturationX.ToString(new CultureInfo("en-US")).Substring(0, 4);
                else
                    relationName = relationName + "x" + b.SaturationX.ToString(new CultureInfo("en-US"));
            }
            if (sy)
            {
                if(b.SaturationY.ToString(new CultureInfo("en-US")).Length>3)
                    relationName = relationName + "y" + b.SaturationY.ToString(new CultureInfo("en-US")).Substring(0, 4);
                else
                    relationName = relationName + "y" + b.SaturationY.ToString(new CultureInfo("en-US"));
            }
            
            r.NAME = relationName;
            r.AddNode(id, "DCS",true, plane);
            return b;
        }
        public void GenerateDefaultUserCurve()
        {
            Curvature.Clear();
            double val = 0.0;
            while (val <= 1.0)
            {
                Curvature.Add(val);
                val = val + 0.1;
            }
        }
        public static Bind GetBindFromButtonElement(DCSButtonBind dab, string id, string joystick, string plane)
        {
            Relation r = new Relation();
            Bind b = new Bind(r);
            r.ISAXIS = false;
            string shorten = MainStructure.ShortenDeviceName(joystick);
            if (joystick == "Keyboard") shorten = "m000000";
            string relationName = shorten + dab.key;
            if (dab.Groups.Count > 0)
            {
                for (int i = 0; i < dab.Groups.Count; ++i)
                {
                    if (!b.Rl.Groups.Contains(dab.Groups[i]))
                    {
                        b.Rl.Groups.Add(dab.Groups[i]);
                    }
                }
            }
            if (dab.modifiers != null)
            {
                foreach (Modifier m in dab.modifiers)
                {
                    if(DCSIOLogic.KeyboardConversion_DCS2DX.ContainsKey(m.key))m.key = DCSIOLogic.KeyboardConversion_DCS2DX[m.key];
                    //same code as mainwindow
                    if (m.JPN != null && m.JPN.Length > 1)
                        m.name = m.JPN;

                    string reform = m.name + "§" + m.device + "§" + m.key;

                    string device;
                    if (m.device == "Keyboard")
                        device = "Keyboard";
                    else
                        device = "m" + m.device.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
                    string nameToShow;
                    nameToShow = device + m.key;
                    if (m.JPN != null && m.JPN.Length > 1)
                        b.AdditionalImportInfo += m.JPN + "§";
                    //else
                    //    nameToShow = device + m.key;

                    string moddedDevice = JoystickGuidToModifierGuid(m.device);
                    string toAdd = nameToShow + "§" + moddedDevice + "§" + m.key;
                    if (!b.AllReformers.Contains(toAdd))
                    {

                        ModExists alreadyExists = InternalDataManagement.DoesReformerExistInMods(toAdd);
                        if (alreadyExists == ModExists.NOT_EXISTENT)
                        {
                            InternalDataManagement.AddReformerToMods(toAdd);
                            b.AllReformers.Add(toAdd);
                        }
                        else if (alreadyExists == ModExists.BINDNAME_EXISTS || alreadyExists == ModExists.ALL_EXISTS)
                        {
                            Modifier mf = Modifier.ReformerToMod(toAdd);
                            toAdd = InternalDataManagement.GetReformerStringFromMod(mf.name);
                            if (!b.AllReformers.Contains(toAdd)) b.AllReformers.Add(toAdd);
                        }
                        else if (alreadyExists == ModExists.KEYBIND_EXISTS)
                        {
                            Modifier mnew = InternalDataManagement.GetModifierWithKeyCombo(m.device, m.key);
                            if (mnew != null)
                            {
                                toAdd = mnew.toReformerString();
                                if (!b.AllReformers.Contains(toAdd)) b.AllReformers.Add(toAdd);
                            }
                        }
                        else if (alreadyExists == ModExists.ERROR)
                        {

                        }
                    }
                    relationName = nameToShow + relationName + m.name;

                }
            }
            if((dab.modifiers == null || dab.modifiers.Count == 0) && (dab.reformers != null && dab.reformers.Count > 0))
            {
                for(int i=0; i<dab.reformers.Count; i++)
                {
                    b.AllReformers.Add(Modifier.CreateDefaultReformer(dab.reformers[i], Game.DCS));
                    relationName = relationName + dab.reformers[i];
                }
            }
            r.NAME = relationName;
            if(dab.JPRelName.Length>2)
                b.AdditionalImportInfo += dab.JPRelName;
            b.JButton = dab.key;
            if(joystick=="Keyboard"&&DCSIOLogic.KeyboardConversion_DCS2DX.ContainsKey(b.JButton))b.JButton= DCSIOLogic.KeyboardConversion_DCS2DX[b.JButton];
            b.Joystick = joystick;
            b.JoyBackup = joystick;
            r.AddNode(id,"DCS",false, plane);
            return b;
        }
        public DCSButtonBind toDCSButtonBind()
        {
            DCSButtonBind dbb = new DCSButtonBind();
            dbb.key = JButton;
            if (Joystick == "Keyboard" && DCSIOLogic.KeyboardConversion_DX2DCS.ContainsKey(JButton)) dbb.key = DCSIOLogic.KeyboardConversion_DX2DCS[JButton];
            dbb.JPRelName = Rl.NAME;
            dbb.relatedBind = this;
            if(Rl.Groups!=null)
                dbb.Groups = Rl.Groups;
            if (JButton.Length < 1||Joystick.Length<1) return null;
            if (dbb.reformers == null) dbb.reformers = new List<string>();
            for (int i=0; i<AllReformers.Count; ++i)
            {
                if (AllReformers[i].Length > 0)
                {
                    string[] parts = AllReformers[i].Split('§');
                    if (parts.Length == 3)
                    {
                        dbb.reformers.Add(parts[0]);
                        Modifier m = new Modifier();
                        m.name = parts[0];
                        m.device = parts[1];
                        m.sw = false;
                        m.key = parts[2];
                        dbb.modifiers.Add(m);
                    }else if(parts.Length == 4)
                    {
                        dbb.reformers.Add(parts[0]);
                        Modifier m = new Modifier();
                        m.name = parts[0];
                        m.device = parts[1];
                        m.sw = Convert.ToBoolean(parts[3]);
                        m.key = parts[2];
                        dbb.modifiers.Add(m);
                    }
                }
            }
            return dbb;
        }
        public void replaceDeviceInReformers(string oldDevice, string newDevice)
        {
            string shortenOld, shortenNew;
            if(oldDevice.Contains("{")&& oldDevice.Contains("}"))
            {
                shortenOld = "m" + oldDevice.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
            }
            else
            {
                shortenOld = oldDevice;
            }
            if (newDevice.Contains("{") && newDevice.Contains("}"))
            {
                shortenNew = "m" + newDevice.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
            }
            else
            {
                shortenNew = newDevice;
            }

            oldDevice = JoystickGuidToModifierGuid(oldDevice);
            newDevice = JoystickGuidToModifierGuid(newDevice);
            for (int i=0; i<AllReformers.Count; ++i)
            {
                if (AllReformers[i].Contains("§"+oldDevice))
                {
                    AllReformers[i] = AllReformers[i].Replace("§" + oldDevice, "§" + newDevice);
                    AllReformers[i] = AllReformers[i].Replace(shortenOld, shortenNew);
                }
            }
        }
        public static string JoystickGuidToModifierGuid(string id)
        {
            if (id == "Keyboard") return "Keyboard";
            string[] parts = id.Split('{');
            string nonIdPart = parts[0];
            if (parts.Length > 1)
            {
                string idToMod = parts[1].Split('}')[0];
                string[] idParts = idToMod.Split('-');
                string finishedID = "";
                if (idParts.Length > 0)
                    finishedID = idParts[0].ToUpper();
                for(int i=1; i<idParts.Length; ++i)
                {
                    if (i == 2)
                    {
                        finishedID = finishedID + "-" + idParts[i].ToLower();
                    }
                    else
                    {
                        finishedID = finishedID + "-" + idParts[i].ToUpper();
                    }
                }
                string result = nonIdPart + "{" + finishedID + "}";
                return result;
            }
            return "";
        } 
        public bool ReformerInBind(string reformerName)
        {
            for(int i=0; i<AllReformers.Count; ++i)
            {
                if (AllReformers[i].Split('§')[0].ToLower() == reformerName.ToLower())
                    return true;
            }
            return false;
        }
        

    }
}
