using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class OtherGameInput
    {
        public string ID, Game, Title;
        public bool IsAxis;
        public string Plane;
        string categ;
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

        public OtherGameInput(string id, string title, bool isAxis, string game, string pln, string cat)
        {
            this.ID = id;
            this.Title = title;
            this.IsAxis = isAxis;
            this.Game = game;
            Plane = pln;
            Category = cat;
        }

        public OtherGameInput Copy()
        {
            OtherGameInput dip = new OtherGameInput(ID, Title, IsAxis, Game, Plane, Category);
            return dip;
        }
    }
}
