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
        public WindowPos exchangeW = null;
        public WindowPos ModifierW = null;
        public WindowPos ValidW = null;
        public WindowPos SettingsW = null;
        public WindowPos stick2ExW = null;
        public string lastGameSelected = "";
        public string lastInstanceSelected ="";
        public int timeToSet;
        public int axisThreshold;
        public int warmupTime;
        public int pollWaitTime;
        public bool NukeSticks;


        public MetaSave()
        {
            lastGameSelected = "";
            lastInstanceSelected = "";
            lastOpenedLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            relationWindowLast = new WindowPos();
            importWindowLast = new WindowPos();
            mainWLast = new WindowPos();
            exchangeW = new WindowPos();
            ModifierW = new WindowPos();
            ValidW = new WindowPos();
            SettingsW = new WindowPos();
            stick2ExW = new WindowPos();
            timeToSet = 5000;
            axisThreshold = 10000;
            warmupTime = 300;
            pollWaitTime = 10;
            NukeSticks = false;
        }
    }
}
