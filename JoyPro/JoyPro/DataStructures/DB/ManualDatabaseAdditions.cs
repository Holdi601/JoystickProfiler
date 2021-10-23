using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class ManualDatabaseAdditions
    {
        public Dictionary<string, DCSPlane> DCSLib;
        public Dictionary<string, Dictionary<string, OtherGame>> OtherLib;
        public ManualDatabaseAdditions()
        {
            DCSLib = new Dictionary<string, DCSPlane>();
            OtherLib = new Dictionary<string, Dictionary<string, OtherGame>>();
        }
    }
}
