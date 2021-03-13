using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class DCSButtonBind
    {
        public string key;
        public List<string> reformers;
        public List<Modifier> modifiers;

        public DCSButtonBind()
        {
            key = "";
            reformers = new List<string>();
            modifiers = new List<Modifier>();
        }

        public DCSButtonBind Copy()
        {
            DCSButtonBind result = new DCSButtonBind();
            result.key = key;
            for (int i = 0; i < reformers.Count; ++i)
                result.reformers.Add(reformers[i]);
            for (int i = 0; i < modifiers.Count; ++i)
                result.modifiers.Add(modifiers[i]);
            return result;
        }

        public bool ContainsMod(string device, string key)
        {
            for(int i=0; i<modifiers.Count; ++i)
            {
                if (modifiers[i].device == device && modifiers[i].key == key)
                    return true;
            }
            return false;
        }

    }
}
