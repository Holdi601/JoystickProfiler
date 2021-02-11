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

        public DCSAxisBind()
        {
            filter = new DCSAxisFilter();
            key = "";
        }
        public DCSAxisBind Copy()
        {
            DCSAxisBind result = new DCSAxisBind();
            result.key = key;
            result.filter = filter.Copy();
            return result;
        }


    }
}
