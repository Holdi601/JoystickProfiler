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


    }
}
