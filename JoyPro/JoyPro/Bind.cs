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
        public JoystickAxis JAxis;
        public string JButton;
        //Don't remove otherwise loading templates don't work
        public string Reformer;
        public bool? Inverted;
        public bool? Slider;
        public double Deadzone;
        public List<double> Curviture;
        public double SaturationX;
        public double SaturationY;
        List<string> AllReformers;

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
        };
        public Bind(Relation r)
        {
            Joystick = "";
            JAxis = JoystickAxis.NONE;
            JButton = "";
            Inverted = false;
            Slider = false;
            Curviture = new List<double>();
            Curviture.Add(0.0);
            SaturationX = 1.0;
            SaturationY = 1.0;
            Rl = r;
            Deadzone = 0;
            Reformer = "";
            AllReformers = new List<string>();
        }

        public bool? SetButton(string btn)
        {
            if (JAxis != JoystickAxis.NONE) return null;
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
            if (JAxis == JoystickAxis.NONE || Joystick.Length < 1) return null;
            result.key = JAxis.ToString();
            DCSAxisFilter daf = new DCSAxisFilter();
            result.filter = daf;
            daf.curviture = Curviture;
            daf.deadzone = Deadzone;
            daf.inverted = Inverted ?? false;
            daf.saturationX = SaturationX;
            daf.saturationY = SaturationY;
            return result;
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
                    dbb.reformers.Add(AllReformers[i]);
                }
            }
            if (Reformer.Length > 0)
            {
                dbb.reformers = new List<string>();
                dbb.reformers.Add(Reformer);
            }
            return dbb;
        }
    }
}
