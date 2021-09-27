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
        public string JPRelName;
        public List<string> Groups;
        public Bind relatedBind; //don't 
        public DCSButtonBind()
        {
            key = "";
            reformers = new List<string>();
            modifiers = new List<Modifier>();
            JPRelName = "";
            Groups = new List<string>();
        }

        public DCSButtonBind Copy()
        {
            DCSButtonBind result = new DCSButtonBind();
            result.JPRelName = JPRelName;
            result.key = key;
            result.relatedBind = relatedBind;
            for (int i = 0; i < Groups.Count; ++i)
                result.Groups.Add(Groups[i]);
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
