using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class DCSAxisBind
    {
        public DCSAxisFilter filter;
        public string key;
        public string JPRelName;
        public List<string> Groups;
        public Bind relatedBind; //don't 
        public List<string> reformers;
        public List<Modifier> modifiers;

        public DCSAxisBind()
        {
            filter = new DCSAxisFilter();
            key = "";
            JPRelName = "";
            Groups = new List<string>();
            reformers = new List<string>();
            modifiers = new List<Modifier>();
        }
        public DCSAxisBind Copy()
        {
            DCSAxisBind result = new DCSAxisBind();
            result.JPRelName = JPRelName;
            result.key = key;
            result.filter = filter.Copy();
            result.Groups = new List<string>();
            for(int i=0; i<Groups.Count; ++i)
            {
                result.Groups.Add(Groups[i]);
            }
            for(int i=0; i<reformers.Count; ++i)
            {
                result.reformers.Add(reformers[i]);
            }
            for (int i = 0; i < modifiers.Count; ++i)
            {
                result.modifiers.Add(modifiers[i]);
            }
            return result;
        }


    }
}
