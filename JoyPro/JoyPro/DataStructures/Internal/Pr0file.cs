using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class Pr0file
    {
        public Dictionary<string, Relation> Relations;
        public Dictionary<string, Bind> Binds;
        public string LastSelectedDCSInstance;
        public Dictionary<string, string> JoystickAliases;
        public Dictionary<string, string> JoystickFileImages;
        public Dictionary<string, int> JoystickTextSize;
        public Dictionary<string, Dictionary<string, Coordinates>> JoystickTextPosition; 

        public Pr0file(Dictionary<string, Relation> Rel, Dictionary<string, Bind> Bnds, string DCSInstance, Dictionary<string, string> JAlias)
        {
            Relations = Rel;
            Binds = Bnds;
            LastSelectedDCSInstance = DCSInstance;
            JoystickAliases = JAlias;
        }
    }
}
