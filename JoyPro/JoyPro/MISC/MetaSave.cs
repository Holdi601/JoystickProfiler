using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
        public WindowPos OverlaySW = null;
        public WindowPos OverlayW=null;
        public int OvlH;
        public int OvlW;
        public string Font { get; set; }
        private byte r, g, b, a;
        public SolidColorBrush ColorSCB
        {
            get
            {
                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }
            set
            {
                a = value.Color.A;
                r = value.Color.R;
                g = value.Color.G;
                b = value.Color.B;
            }
        }
        public int OvlTxtS;
        public int OvlElementsToShow;
        public int OvlPollTime;
        public bool OvlFade;
        public int TextTimeAlive;
        public bool OvlBtnChangeMode;

        public const int default_modulesToScan = 40;
        public const int default_timeToSet = 5000;
        public const int default_axisThreshold = 10000;
        public const int default_warmupTime = 300;
        public const int default_pollWaitTime = 10;
        public const int default_backupDays = 90;
        public const bool default_nukeSticks = false;
        public const int default_OvlH = 600;
        public const int default_OvlW = 600;
        public const string default_Font = "Arial";
        public const byte default_r = 255;
        public const byte default_g = 255;
        public const byte default_b = 255;
        public const byte default_a = 255;
        public const int default_OvlTxtS = 32;
        public const int default_OvlElementsToShow = 16;
        public const int default_OvlPollTime = 16;
        public const bool default_OvlFade = false;
        public const int default_TextAlive = 5000;
        public const bool default_OvlBtnChangeMode = false;
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
            OverlaySW = new WindowPos();
            OverlayW = new WindowPos();
            OvlH = default_OvlH;
            OvlW = default_OvlW;
            Font = default_Font;
            r = default_r;
            g = default_g;
            b = default_b;
            a = default_a;
            OvlTxtS = default_OvlTxtS;
            OvlElementsToShow = default_OvlElementsToShow;
            OvlPollTime = default_OvlPollTime;
            OvlFade= default_OvlFade;
            TextTimeAlive = default_TextAlive;
            OvlBtnChangeMode = default_OvlBtnChangeMode;
        }
    }
}
