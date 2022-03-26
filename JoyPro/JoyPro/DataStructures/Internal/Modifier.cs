using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class Modifier
    {
        public string name;
        string devicem;
        public string JPN;
        public string device
        {
            get
            {
                return devicem;
            }
            set
            {
                if (value.Contains("{"))
                {
                    string[] parts = value.Split('{');
                    string nonId = parts[0] + "{";
                    string[] uidParts = parts[1].Replace("}", "").Split('-');
                    string final = nonId + uidParts[0].ToUpper();
                    for(int i=1; i<uidParts.Length; ++i)
                    {
                        if (i == 2)
                        {
                            final = final + "-" + uidParts[i].ToLower();
                        }
                        else
                        {
                            final = final + "-" + uidParts[i].ToUpper();
                        }
                    }
                    final = final + "}";
                    devicem = final;
                }
                else
                {
                    devicem = value;
                }
            }
        }
        public string key;
        public bool sw;

        public Modifier()
        {
            name = "";
            device = "";
            key = "";
            sw = false;
            JPN = "";
        }
        public Modifier Copy()
        {
            Modifier m = new Modifier();
            m.name = name;
            m.device = device;
            m.key = key;
            m.sw = sw;
            m.JPN = JPN;
            return m;
        }
        public string toReformerString()
        {
            return name + "§" + device + "§" + key+"§"+sw.ToString();
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
            }else if(parts.Length == 4)
            {
                Modifier m = new Modifier();
                m.name = parts[0];
                m.device = parts[1];
                m.sw = Convert.ToBoolean(parts[3]);
                m.key = parts[2];
                return m;
            }
            else
            {
                return null;
            }
        } 
        public static bool ButtonComboInReformerList(string Joystick, string Button, List<string> reformers)
        {
            if (reformers == null || Button == null || Joystick == null) return false;
            for(int i=0; i<reformers.Count; ++i)
            {
                string[] parts = reformers[i].Split('§');
                if (parts.Length < 3) continue;
                if (parts[1].ToLower() == Joystick.ToLower() &&
                    parts[2].ToLower() == Button.ToLower())
                    return true;
            }
            return false;
        }

        public static string CreateDefaultReformer(string defRef, Game g)
        {
            Modifier m;
            switch (g)
            {
                case Game.DCS:
                    if (DCSIOLogic.KeyboardConversion_DCS2DX.ContainsKey(defRef)) defRef = DCSIOLogic.KeyboardConversion_DCS2DX[defRef];
                    break;
                case Game.IL2:
                    defRef = defRef.Substring(4);
                    if (IL2IOLogic.KeyboardConversion_IL2DX.ContainsKey(defRef)) defRef = IL2IOLogic.KeyboardConversion_IL2DX[defRef];
                    else defRef = defRef.Substring(0, 1).ToUpper() + defRef.Substring(1);
                    break;
            }
            if (InternalDataManagement.AllModifiers.ContainsKey(defRef))
            {
                m = InternalDataManagement.AllModifiers[defRef];
            }
            else
            {
                m = new Modifier();
                m.sw = false;
                m.device = "Keyboard";
                m.key = defRef;
                m.name = defRef;
                InternalDataManagement.AllModifiers.Add(defRef, m);
            }
            return m.toReformerString();
        }
    }
}
