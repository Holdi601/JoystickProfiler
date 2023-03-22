using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class OtherGameInput
    {
        public string ID, Game, Title;
        public bool IsAxis;
        public string Plane;
        public string categ;
        public string default_keyboard="";
        public string default_gamepad="";
        public string default_joystick="";
        public string Category {
            get
            {
                if (categ == null) categ = "";
                return categ;
            }
            set
            {
                categ = value;
            }
        }

        [JsonConstructor]
        public OtherGameInput() { }
        public OtherGameInput(string id, string title, bool isAxis, string game, string pln, string cat)
        {
            this.ID = id;
            this.Title = title;
            this.IsAxis = isAxis;
            this.Game = game;
            Plane = pln;
            Category = cat;
            default_gamepad = "";
            default_joystick = "";
            default_keyboard = "";
        }

        public OtherGameInput(string id, string title, bool isAxis, string game, string pln, string cat, string def_keyboard, string def_gamepad, string def_joystick)
        {
            this.ID = id;
            this.Title = title;
            this.IsAxis = isAxis;
            this.Game = game;
            Plane = pln;
            Category = cat;
            default_gamepad = def_gamepad;
            default_joystick = def_joystick;
            default_keyboard = def_keyboard;
        }

        public OtherGameInput Copy()
        {
            OtherGameInput dip = new OtherGameInput(ID, Title, IsAxis, Game, Plane, Category, default_keyboard, default_gamepad, default_joystick);
            return dip;
        }
    }
}
