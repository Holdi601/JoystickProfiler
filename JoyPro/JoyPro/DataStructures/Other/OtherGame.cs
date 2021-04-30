using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class OtherGame
    {
        public string Name;
        public Dictionary<string, OtherGameInput> Axis;
        public Dictionary<string, OtherGameInput> Buttons;

        public OtherGame(string name)
        {
            this.Name = name;
            Axis = new Dictionary<string, OtherGameInput>();
            Buttons = new Dictionary<string, OtherGameInput>();
        }
    }
}
