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
        public string device;
        public string key;
        public bool sw;

        public Modifier()
        {
            name = "";
            device = "";
            key = "";
            sw = false;
        }

        public Modifier Copy()
        {
            Modifier m = new Modifier();
            m.name = name;
            m.device = device;
            m.key = key;
            m.sw = sw;
            return m;
        }

        public string toReformerString()
        {
            return name + "§" + device + "§" + key;
        }
    }
}
