using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace JoyPro
{
    [Serializable]
    public class Pr0file
    {
        public Dictionary<string, Relation> Relations;
        public Dictionary<string, Bind> Binds;
        public string LastSelectedDCSInstance;
        public Dictionary<string, string> JoystickAliases;
        //public Dictionary<string, string> JoystickFileImages;
        //public Dictionary<string, int> JoystickTextSize;
        //public Dictionary<string, string> JoystickTextFont;
        //public Dictionary<string, SolidColorBrush> JoystickTextColor;
        //public Dictionary<string, Dictionary<string, Point>> JoystickTextPosition; 

        public Pr0file(Dictionary<string, Relation> Rel, Dictionary<string, Bind> Bnds, string DCSInstance, Dictionary<string, string> JAlias)
        {
            Relations = Rel;
            Binds = Bnds;
            LastSelectedDCSInstance = DCSInstance;
            JoystickAliases = JAlias;
        }
    }
}
