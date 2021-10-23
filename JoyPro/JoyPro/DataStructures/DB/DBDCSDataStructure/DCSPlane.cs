using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class DCSPlane
    {

        public string Name;
        public Dictionary<string, DCSInput> Axis;
        public Dictionary<string, DCSInput> Buttons;

        public DCSPlane(string name)
        {
            this.Name = name;
            Axis = new Dictionary<string, DCSInput>();
            Buttons = new Dictionary<string, DCSInput>();
        }

        public DCSPlane Copy()
        {
            DCSPlane dp = new DCSPlane(Name);
            foreach(KeyValuePair<string, DCSInput> kvp in Axis)
            {
                dp.Axis.Add(kvp.Key, kvp.Value.Copy());
            }
            foreach (KeyValuePair<string, DCSInput> kvp in Buttons)
            {
                dp.Buttons.Add(kvp.Key, kvp.Value.Copy());
            }
            return dp;
        }


    }
}
