using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        //Don't remove otherwise loading templates don't work
        public string Reformer_depr;
        public bool? Inverted;
        public bool? Slider;
        public double Deadzone;
        public List<double> Curviture;
        public double SaturationX;
        public double SaturationY;
        public List<string> AllReformers;

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
        public Bind(Relation r)
        {
            Joystick = "";
            JAxis = "";
            JButton = "";
            Inverted = false;
            Slider = false;
            Curviture = new List<double>();
            Curviture.Add(0.0);
            SaturationX = 1.0;
            SaturationY = 1.0;
            Rl = r;
            Deadzone = 0;
            Reformer_depr = "";
            AllReformers = new List<string>();
        }

        public bool? SetButton(string btn)
        {
            if (JAxis.Length<1) return null;
            bool isPov = false;
            string PreFixButton = "JOY_BTN";
            btn = btn.Replace(PreFixButton, "");
            int iFound = -1;
            int btnnmbr = -1;
            bool success = int.TryParse(btn, out btnnmbr);
            if (btnnmbr >= 0 && success)
            {
                JButton = PreFixButton + btnnmbr.ToString();
                return true;
            }
            for (int i=0; i<PovHeads.Length; ++i)
            {
                if (PovHeads[i].ToLower().Contains(btn.ToLower()))
                {
                    isPov = true;
                    iFound = i;
                    break;
                }
            }
            if (isPov)
            {
                JButton = PreFixButton + "_" + PovHeads[iFound];
                return true;
            }
            return false;
        }

        public DCSAxisBind toDCSAxisBind()
        {
            DCSAxisBind result = new DCSAxisBind();
            if (JAxis.Length <1 || Joystick.Length < 1) return null;
            result.key = JAxis.ToString();
            DCSAxisFilter daf = new DCSAxisFilter();
            result.filter = daf;
            daf.curviture = Curviture;
            daf.deadzone = Deadzone;
            daf.inverted = Inverted ?? false;
            daf.slider = Slider ?? false;
            daf.saturationX = SaturationX;
            daf.saturationY = SaturationY;
            return result;
        }

        public static Bind GetBindFromAxisElement(DCSAxisBind dab,string id, string joystick, string plane)
        {
            Relation r = new Relation();
            Bind b = new Bind(r);
            string shorten = MainStructure.ShortenDeviceName(joystick);
            string relationName = shorten + dab.key;
            r.ISAXIS = true;
            r.NAME = relationName;
            b.JAxis = dab.key;
            b.Joystick = joystick;
            if (dab.filter != null)
            {
                b.Inverted = dab.filter.inverted;
                b.Curviture = dab.filter.curviture;
                b.Deadzone = dab.filter.deadzone;
                b.Slider = dab.filter.slider;
                b.SaturationX = dab.filter.saturationX;
                b.SaturationY = dab.filter.saturationY;
            }
            r.AddNode(id, plane);
            return b;
        }

        public static Bind GetBindFromButtonElement(DCSButtonBind dab, string id, string joystick, string plane)
        {
            Relation r = new Relation();
            Bind b = new Bind(r);
            r.ISAXIS = false;
            string shorten = MainStructure.ShortenDeviceName(joystick);
            string relationName = shorten + dab.key;
            foreach(Modifier m in dab.modifiers)
            {
                relationName = m.name + relationName;
                string reform = m.name + "§" + m.device + "§" + m.key;
                if (!b.AllReformers.Contains(reform)) b.AllReformers.Add(reform);
            }
            r.NAME = relationName;
            b.JButton = dab.key;
            b.Joystick = joystick;
            r.AddNode(id, plane);
            return b;
        }

        public DCSButtonBind toDCSButtonBind()
        {
            DCSButtonBind dbb = new DCSButtonBind();
            dbb.key = JButton;
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
                    }
                    
                }
            }
            return dbb;
        }
        public void replaceDeviceInReformers(string oldDevice, string newDevice)
        {
            string shortenOld = "m"+oldDevice.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
            string shortenNew = "m"+newDevice.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
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

        public string JoystickGuidToModifierGuid(string id)
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
        public void deleteReformer(string displayName)
        {
            for(int i=AllReformers.Count-1; i>=0; i--)
            {
                string[] parts = AllReformers[i].Split('§');
                if ((parts.Length >= 3 && parts[0] == displayName)||parts.Length<3)
                {
                    AllReformers.RemoveAt(i);
                }
            }
        }
        public string ModToDosplayString(int mod)
        {
            string result = "None";
            if (mod - 1 < AllReformers.Count && mod - 1 >= 0)
            {
                string[] parts = AllReformers[mod - 1].Split('§');
                result = parts[0];
            }
            return result;
        }
    }
}
