using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        public Dictionary<string, string> JoystickFileImages;
        public string JoystickLayoutExport;
        public Dictionary<string, Dictionary<string, string>> PlaneAliases;
        public Dictionary<string, string> JoysticksPGuids = new Dictionary<string, string>();
        public List<KeyValuePair<string, string>> modifierNameChanges = new List<KeyValuePair<string, string>>();

        [JsonConstructor]
        public Pr0file() { }
        public Pr0file(Dictionary<string, Relation> Rel, Dictionary<string, Bind> Bnds, string DCSInstance, Dictionary<string, string> JAlias, Dictionary<string, Dictionary<string, string>> pAlias, List<KeyValuePair<string, string>> modifierChanges)
        {
            Relations = Rel;
            Binds = Bnds;
            LastSelectedDCSInstance = DCSInstance;
            JoystickAliases = JAlias;
            PlaneAliases = pAlias;
            modifierNameChanges = modifierChanges;
        }
    }
}
