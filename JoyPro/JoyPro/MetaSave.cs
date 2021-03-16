using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class MetaSave
    {
        public string lastOpenedLocation = "";
        public WindowPos relationWindowLast = null;
        public WindowPos importWindowLast = null;
        public WindowPos mainWLast = null;
        public string lastGameSelected = "";
        public string lastInstanceSelected ="";
        public MetaSave()
        {
            lastGameSelected = "";
            lastInstanceSelected = "";
            lastOpenedLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            relationWindowLast = new WindowPos();
            importWindowLast = new WindowPos();
            mainWLast = new WindowPos();
        }
    }
}
