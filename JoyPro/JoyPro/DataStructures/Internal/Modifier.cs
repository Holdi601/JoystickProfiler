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
            return name + "§" + device + "§" + key;
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
            }
            else
            {
                return null;
            }
        }
    }
}
