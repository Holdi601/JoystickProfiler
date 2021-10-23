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
        public string Game;
        public bool HasDifferentControlsForDifferentPlanes;

        public OtherGame(string name, string game, bool hasDifferentPlaneControls)
        {
            this.Name = name;
            Axis = new Dictionary<string, OtherGameInput>();
            Buttons = new Dictionary<string, OtherGameInput>();
            Game = game;
            HasDifferentControlsForDifferentPlanes = hasDifferentPlaneControls;
        }

        public OtherGame Copy()
        {
            OtherGame og = new OtherGame(Name, Game, HasDifferentControlsForDifferentPlanes);
            foreach(KeyValuePair<string, OtherGameInput> kvp in Axis)
            {
                og.Axis.Add(kvp.Key, kvp.Value.Copy());
            }
            foreach (KeyValuePair<string, OtherGameInput> kvp in Buttons)
            {
                og.Buttons.Add(kvp.Key, kvp.Value.Copy());
            }
            return og;
        }
    }
}
