using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class DCSLuaDiffsButtonElement
    {
        public string Keyname;
        public string Title;
        public List<DCSButtonBind> added;
        public List<DCSButtonBind> removed;

        public DCSLuaDiffsButtonElement()
        {
            added = new List<DCSButtonBind>();
            removed = new List<DCSButtonBind>();
            Keyname = "";
            Title = "";
        }
        public DCSLuaDiffsButtonElement Copy()
        {
            DCSLuaDiffsButtonElement result = new DCSLuaDiffsButtonElement();
            result.Keyname = Keyname;
            result.Title = Title;
            for(int i=0; i<added.Count; ++i)
            {
                result.added.Add(added[i].Copy());
            }
            for (int i = 0; i < removed.Count; ++i)
            {
                result.removed.Add(removed[i].Copy());
            }
            return result;
        }

        public bool doesAddedContainKey(string key, List<string> refs)
        {
            for (int i = 0; i < added.Count; ++i)
            {
                if (added[i].key == key&&added[i].reformers.Count==0&&(refs==null||refs.Count==0)) return true;
                else if(added[i].key == key&&refs!=null&&refs.Count==added[i].reformers.Count)
                {
                    bool allTrue = true;
                    for(int j=0; j<added[i].reformers.Count; j++)
                    {
                        if (!refs.Contains(added[i].reformers[j]))
                        {
                            allTrue = false;
                            break;
                        }
                    }
                    if (allTrue)
                        return true;
                }
            }
            return false;
        }
        public bool doesRemovedContainKey(string key)
        {
            for (int i = 0; i < removed.Count; ++i)
            {
                if (removed[i].key == key) return true;
            }
            return false;
        }

        public void removeItemFromAdded(string key)
        {
            for (int i = added.Count - 1; i > -1; --i)
            {
                if (added[i].key == key) added.RemoveAt(i);
            }
        }

        public void removeItemFromRemoved(string key)
        {
            for (int i = removed.Count - 1; i > -1; --i)
            {
                if (removed[i].key == key) removed.RemoveAt(i);
            }
        }
    }
}
