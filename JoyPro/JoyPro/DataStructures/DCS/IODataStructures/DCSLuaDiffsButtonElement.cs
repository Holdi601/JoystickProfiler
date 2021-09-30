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
                if (added[i].key == key&&(added[i].reformers==null||added[i].reformers.Count==0)&&(refs==null||refs.Count==0)) return true;
                else if(added[i].key == key&&refs!=null&&added[i].reformers!=null&&refs.Count==added[i].reformers.Count)
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
        public bool doesRemovedContainKey(string key, List<string> refs=null)
        {
            for (int i = 0; i < removed.Count; ++i)
            {
                if (removed[i].key == key && (refs==null||refs.Count==0)&&(removed[i].reformers==null||removed[i].reformers.Count==0)) return true;
                else if (removed[i].key == key&&removed[i].reformers!=null&&refs!=null&&removed[i].reformers.Count==refs.Count)
                {
                    bool hit = true;
                    for(int z=0; z<refs.Count; ++z)
                    {
                        if (!removed[i].reformers.Contains(refs[z]))
                        {
                            hit = false;
                            break;
                        }
                    }
                    if (hit)
                        return true;
                }
            }
            return false;
        }

        public void removeItemFromAdded(string key, List<string> refs = null)
        {
            for (int i = added.Count - 1; i > -1; --i)
            {
                if (added[i].key == key && (added[i].reformers == null || added[i].reformers.Count == 0) && (refs == null || refs.Count == 0)) added.RemoveAt(i);
                else if (added[i].key == key && refs != null && added[i].reformers != null && refs.Count == added[i].reformers.Count)
                {
                    bool hit = true;
                    for (int z = 0; z < refs.Count; ++z)
                    {
                        if (!added[i].reformers.Contains(refs[z]))
                        {
                            hit = false;
                            break;
                        }
                    }
                    if (hit)
                        added.RemoveAt(i);
                }
            }
        }

        public void removeItemFromRemoved(string key, List<string> refs = null)
        {
            for (int i = removed.Count - 1; i > -1; --i)
            {
                if (removed[i].key == key && (refs == null || refs.Count == 0) && (removed[i].reformers == null || removed[i].reformers.Count == 0)) removed.RemoveAt(i);
                else if (removed[i].key == key && removed[i].reformers != null && refs != null && removed[i].reformers.Count == refs.Count)
                {
                    bool hit = true;
                    for (int z = 0; z < refs.Count; ++z)
                    {
                        if (!removed[i].reformers.Contains(refs[z]))
                        {
                            hit = false;
                            break;
                        }
                    }
                    if (hit)
                        removed.RemoveAt(i);
                }
            }
        }
    }
}
