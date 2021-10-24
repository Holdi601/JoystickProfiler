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
        public int backupDays;
        public WindowPos BackupW = null;
        public WindowPos UsrCvW = null;
        public WindowPos GrpMngr = null;
        public WindowPos AliasCr = null;
        public WindowPos JoyManAs = null;
        public bool? importLocals = null;
        public string DCSInstaceOverride = "";
        public string DCSInstallPathOR = "";
        public string IL2OR = "";
        public int additionModulesToScan;

        public const int default_modulesToScan = 40;
        public const int default_timeToSet = 5000;
        public const int default_axisThreshold = 10000;
        public const int default_warmupTime = 300;
        public const int default_pollWaitTime = 10;
        public const int default_backupDays = 90;
        public const bool default_nukeSticks = false;


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
            timeToSet = default_timeToSet;
            axisThreshold = default_axisThreshold;
            warmupTime = default_warmupTime;
            pollWaitTime = default_pollWaitTime;
            NukeSticks = false;
            backupDays = default_backupDays;
            BackupW = new WindowPos();
            UsrCvW = new WindowPos();
            GrpMngr = new WindowPos();
            AliasCr = new WindowPos();
            JoyManAs = new WindowPos();
            DCSInstaceOverride = "";
            DCSInstallPathOR = "";
            IL2OR = "";
            additionModulesToScan = default_modulesToScan;
        }
    }
}
