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

        public OtherGameInput(string id, string title, bool isAxis, string game, string pln)
        {
            this.ID = id;
            this.Title = title;
            this.IsAxis = isAxis;
            this.Game = game;
            Plane = pln;
        }

        public OtherGameInput Copy()
        {
            OtherGameInput dip = new OtherGameInput(ID, Title, IsAxis, Game, Plane);
            return dip;
        }
    }
}
