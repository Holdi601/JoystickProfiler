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
        public Bind relatedBind; //don't 

        public DCSAxisBind()
        {
            filter = new DCSAxisFilter();
            key = "";
            JPRelName = "";
        }
        public DCSAxisBind Copy()
        {
            DCSAxisBind result = new DCSAxisBind();
            result.JPRelName = JPRelName;
            result.key = key;
            result.filter = filter.Copy();
            return result;
        }


    }
}
