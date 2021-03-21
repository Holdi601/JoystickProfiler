using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class DCSInput
    {

        public string ID, Plane, Title;
        public bool IsAxis;

        public DCSInput(string id, string title, bool isAxis, string plane)
        {
            this.ID = id;
            this.Title = title;
            this.IsAxis = isAxis;
            this.Plane = plane;
        }

        public DCSInput Copy()
        {
            DCSInput dip = new DCSInput(ID, Title, IsAxis, Plane);
            return dip;
        }
    }

}
