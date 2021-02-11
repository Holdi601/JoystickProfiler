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

        public DCSButtonBind()
        {
            key = "";
            reformers = new List<string>();
        }

        public DCSButtonBind Copy()
        {
            DCSButtonBind result = new DCSButtonBind();
            result.key = key;
            for (int i = 0; i < reformers.Count; ++i)
                result.reformers.Add(reformers[i]);
            return result;
        }

    }
}
