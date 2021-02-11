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

        public Pr0file(Dictionary<string, Relation> Rel, Dictionary<string, Bind> Bnds)
        {
            Relations = Rel;
            Binds = Bnds;
        }
    }
}
