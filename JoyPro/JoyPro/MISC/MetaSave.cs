﻿using System;
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
        WindowPos relationWindowLast = null;
        WindowPos importWindowLast = null;
        WindowPos mainWLast = null;
        WindowPos exchangeW = null;
        WindowPos modifierW = null;
        WindowPos validW = null;
        WindowPos settingsW = null;
        WindowPos stick2ExW = null;
        public string lastGameSelected = "";
        public string lastInstanceSelected ="";
        public int timeToSet;
        public int axisThreshold;
        public int warmupTime;
        public int pollWaitTime;
        public bool NukeSticks;
        public int backupDays;
        WindowPos backupW = null;
        WindowPos usrCvW = null;
        WindowPos grpMngr = null;
        WindowPos aliasCr = null;
        WindowPos joyManAs = null;
        public bool? importLocals = null;
        public string DCSInstaceOverride = "";
        public string DCSInstallPathOR = "";
        public string IL2OR = "";
        public int additionModulesToScan;
        WindowPos overlaySW = null;
        WindowPos overlayW=null;
        public double OvlH;
        public double OvlW;
        public string Font { get; set; }
        private byte r, g, b, a;
        WindowPos collectSticks=null;
        public int maxVisualLayers;
        public string SCOR = "";

        public WindowPos _RelationWindow
        {
            get
            {
                if(relationWindowLast==null)relationWindowLast = new WindowPos();
                if( relationWindowLast.Width<=0||
                    relationWindowLast.Height<=0||
                    relationWindowLast.Top<=0||
                    relationWindowLast.Left<=0)
                {
                    relationWindowLast.Width = RelationWindow.DEFAULT_WIDTH;
                    relationWindowLast.Height = RelationWindow.DEFAULT_HEIGHT;
                    relationWindowLast.Top = 0;
                    relationWindowLast.Left = 0;
                }
                return relationWindowLast;
            }
            set
            {
                relationWindowLast = value;
            }
        }
        public WindowPos _ImportWindow
        {
            get
            {
                if (importWindowLast == null) importWindowLast = new WindowPos();
                if (importWindowLast.Width <= 0 ||
                    importWindowLast.Height <= 0 ||
                    importWindowLast.Top < 0 ||
                    importWindowLast.Left < 0)
                {
                    importWindowLast.Width = ImportWindow.DEFAULT_WIDTH;
                    importWindowLast.Height = ImportWindow.DEFAULT_HEIGHT;
                    importWindowLast.Top = 0;
                    importWindowLast.Left = 0;
                }
                return importWindowLast;
            }
            set
            {
                importWindowLast = value;
            }
        }
        public WindowPos _MainWindow
        {
            get
            {
                if (mainWLast == null) mainWLast = new WindowPos();
                if (mainWLast.Width <= 0 ||
                    mainWLast.Height <= 0 ||
                    mainWLast.Top < 0 ||
                    mainWLast.Left < 0)
                {
                    mainWLast.Width = MainWindow.DEFAULT_WIDTH;
                    mainWLast.Height = MainWindow.DEFAULT_HEIGHT;
                    mainWLast.Top = 0;
                    mainWLast.Left = 0;
                }
                return mainWLast;
            }
            set
            {
                mainWLast = value;
            }
        }
        public WindowPos _ExchangeWindow
        {
            get
            {
                if (exchangeW == null) exchangeW = new WindowPos();
                if (exchangeW.Width <= 0 ||
                    exchangeW.Height <= 0 ||
                    exchangeW.Top < 0 ||
                    exchangeW.Left < 0)
                {
                    exchangeW.Width = ExchangeStick.DEFAULT_WIDTH;
                    exchangeW.Height = ExchangeStick.DEFAULT_HEIGHT;
                    exchangeW.Top = 0;
                    exchangeW.Left = 0;
                }
                return exchangeW;
            }
            set
            {
                exchangeW = value;
            }
        }
        public WindowPos _ModifierWindow
        {
            get
            {
                if (modifierW == null) modifierW = new WindowPos();
                if (modifierW.Width <= 0 ||
                    modifierW.Height <= 0 ||
                    modifierW.Top < 0 ||
                    modifierW.Left < 0)
                {
                    modifierW.Width = ModifierManager.DEFAULT_WIDTH;
                    modifierW.Height = ModifierManager.DEFAULT_HEIGHT;
                    modifierW.Top = 0;
                    modifierW.Left = 0;
                }
                return modifierW;
            }
            set
            {
                modifierW = value;
            }
        }
        public WindowPos _ValidationWindow
        {
            get
            {
                if (validW == null) validW = new WindowPos();
                if (validW.Width <= 0 ||
                    validW.Height <= 0 ||
                    validW.Top < 0 ||
                    validW.Left < 0)
                {
                    validW.Width = ValidationErrors.DEFAULT_WIDTH;
                    validW.Height = ValidationErrors.DEFAULT_HEIGHT;
                    validW.Top = 0;
                    validW.Left = 0;
                }
                return validW;
            }
            set
            {
                validW = value;
            }
        }
        public WindowPos _SettingsWindow
        {
            get
            {
                if (settingsW == null) settingsW = new WindowPos();
                if (settingsW.Width <= 0 ||
                    settingsW.Height <= 0 ||
                    settingsW.Top < 0 ||
                    settingsW.Left < 0)
                {
                    settingsW.Width = StickSettings.DEFAULT_WIDTH;
                    settingsW.Height = StickSettings.DEFAULT_HEIGHT;
                    settingsW.Top = 0;
                    settingsW.Left = 0;
                }
                return settingsW;
            }
            set
            {
                settingsW = value;
            }
        }
        public WindowPos _StickExchangeWindow
        {
            get
            {
                if (stick2ExW == null) stick2ExW = new WindowPos();
                if (stick2ExW.Width <= 0 ||
                    stick2ExW.Height <= 0 ||
                    stick2ExW.Top < 0 ||
                    stick2ExW.Left < 0)
                {
                    stick2ExW.Width = StickToExchange.DEFAULT_WIDTH;
                    stick2ExW.Height = StickToExchange.DEFAULT_HEIGHT;
                    stick2ExW.Top = 0;
                    stick2ExW.Left = 0;
                }
                return stick2ExW;
            }
            set
            {
                stick2ExW = value;
            }
        }
        public WindowPos _BackupWindow
        {
            get
            {
                if (backupW == null) backupW = new WindowPos();
                if (backupW.Width <= 0 ||
                    backupW.Height <= 0 ||
                    backupW.Top < 0 ||
                    backupW.Left < 0)
                {
                    backupW.Width = ReinstateBackup.DEFAULT_WIDTH;
                    backupW.Height = ReinstateBackup.DEFAULT_HEIGHT;
                    backupW.Top = 0;
                    backupW.Left = 0;
                }
                return backupW;
            }
            set
            {
                backupW = value;
            }
        }
        public WindowPos _UserCurveWindow
        {
            get
            {
                if (usrCvW == null) usrCvW = new WindowPos();
                if (usrCvW.Width <= 0 ||
                    usrCvW.Height <= 0 ||
                    usrCvW.Top < 0 ||
                    usrCvW.Left < 0)
                {
                    usrCvW.Width = UserCurveDCS.DEFAULT_WIDTH;
                    usrCvW.Height = UserCurveDCS.DEFAULT_HEIGHT;
                    usrCvW.Top = 0;
                    usrCvW.Left = 0;
                }
                return usrCvW;
            }
            set
            {
                usrCvW = value;
            }
        }
        public WindowPos _GroupManagerWindow
        {
            get
            {
                if (grpMngr == null) grpMngr = new WindowPos();
                if (grpMngr.Width <= 0 ||
                    grpMngr.Height <= 0 ||
                    grpMngr.Top < 0 ||
                    grpMngr.Left < 0)
                {
                    grpMngr.Width = GroupManagerW.DEFAULT_WIDTH;
                    grpMngr.Height = GroupManagerW.DEFAULT_HEIGHT;
                    grpMngr.Top = 0;
                    grpMngr.Left = 0;
                }
                return grpMngr;
            }
            set
            {
                grpMngr = value;
            }
        }
        public WindowPos _AliasWindow
        {
            get
            {
                if (aliasCr == null) aliasCr = new WindowPos();
                if (aliasCr.Width <= 0 ||
                    aliasCr.Height <= 0 ||
                    aliasCr.Top < 0 ||
                    aliasCr.Left < 0)
                {
                    aliasCr.Width = CreateJoystickAlias.DEFAULT_WIDTH;
                    aliasCr.Height = CreateJoystickAlias.DEFAULT_HEIGHT;
                    aliasCr.Top = 0;
                    aliasCr.Left = 0;
                }
                return aliasCr;
            }
            set
            {
                aliasCr = value;
            }
        }
        public WindowPos _JoystickManualAssignWindow
        {
            get
            {
                if (joyManAs == null) joyManAs = new WindowPos();
                if (joyManAs.Width <= 0 ||
                    joyManAs.Height <= 0 ||
                    joyManAs.Top < 0 ||
                    joyManAs.Left < 0)
                {
                    joyManAs.Width = ManualJoystickAssign.DEFAULT_WIDTH;
                    joyManAs.Height = ManualJoystickAssign.DEFAULT_HEIGHT;
                    joyManAs.Top = 0;
                    joyManAs.Left = 0;
                }
                return joyManAs;
            }
            set
            {
                joyManAs = value;
            }
        }
        public WindowPos _OverlaySettingsWindow
        {
            get
            {
                if (overlaySW == null) overlaySW = new WindowPos();
                if (overlaySW.Width <= 0 ||
                    overlaySW.Height <= 0 ||
                    overlaySW.Top < 0 ||
                    overlaySW.Left < 0)
                {
                    overlaySW.Width = OverlaySettings.DEFAULT_WIDTH;
                    overlaySW.Height = OverlaySettings.DEFAULT_HEIGHT;
                    overlaySW.Top = 0;
                    overlaySW.Left = 0;
                }
                return overlaySW;
            }
            set
            {
                overlaySW = value;
            }
        }
        public WindowPos _OverlayWindow
        {
            get
            {
                if (overlayW == null) overlayW = new WindowPos();
                if (overlayW.Width <= 0 ||
                    overlayW.Height <= 0 ||
                    overlayW.Top < 0 ||
                    overlayW.Left < 0)
                {
                    overlayW.Width = 1920;
                    overlayW.Height = 1080;
                    overlayW.Top = 0;
                    overlayW.Left = 0;
                }
                return overlayW;
            }
            set
            {
                overlayW = value;
            }
        }
        public WindowPos _CollectSticksVisual
        {
            get
            {
                if (collectSticks == null) overlaySW = new WindowPos();
                if (collectSticks.Width <= 0 ||
                    collectSticks.Height <= 0 ||
                    collectSticks.Top < 0 ||
                    collectSticks.Left < 0)
                {
                    collectSticks.Width = 1920;
                    collectSticks.Height = 1080;
                    collectSticks.Top = 0;
                    collectSticks.Left = 0;
                }
                return collectSticks;
            }
            set
            {
                collectSticks = value;
            }
        }
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
        public bool OvldebugMode;

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
        public const int default_maxVisualLayers = 11;
        public const bool default_stackedMode = false;
        public const bool default_AddAditionalCorrectItems = true;

        public bool? AddAditionalAndCorrectRelationItems = null;
        WindowPos massOpW = null;
        public WindowPos _MassOperationWindow
        {
            get
            {
                if (massOpW == null) massOpW = new WindowPos();
                if (massOpW.Width <= 0 ||
                    massOpW.Height <= 0 ||
                    massOpW.Top < 0 ||
                    massOpW.Left < 0)
                {
                    massOpW.Width = 1920;
                    massOpW.Height = 1080;
                    massOpW.Top = 0;
                    massOpW.Left = 0;
                }
                return massOpW;
            }
            set
            {
                massOpW = value;
            }
        }

        WindowPos joyMention = null;
        public WindowPos _JoystickMentionWindow
        {
            get
            {
                if (joyMention == null) joyMention = new WindowPos();
                if (joyMention.Width <= 0 ||
                    joyMention.Height <= 0 ||
                    joyMention.Top < 0 ||
                    joyMention.Left < 0)
                {
                    joyMention.Width = 1920;
                    joyMention.Height = 1080;
                    joyMention.Top = 0;
                    joyMention.Left = 0;
                }
                return joyMention;
            }
            set
            {
                joyMention = value;
            }
        }

        public Dictionary<string, Dictionary<string, bool>> relationPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
        public Dictionary<string, Dictionary<string, bool>> importPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
        public Dictionary<string, Dictionary<string, bool>> exportPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
        public Dictionary<string, Dictionary<string, bool>> ViewPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
        public bool ExportInView;

        WindowPos exprtWindow = null;
        public WindowPos _ExportWindow
        {
            get
            {
                if (exprtWindow == null) exprtWindow = new WindowPos();
                if (exprtWindow.Width <= 0 ||
                    exprtWindow.Height <= 0 ||
                    exprtWindow.Top < 0 ||
                    exprtWindow.Left < 0)
                {
                    exprtWindow.Width = 1920;
                    exprtWindow.Height = 1080;
                    exprtWindow.Top = 0;
                    exprtWindow.Left = 0;
                }
                return exprtWindow;
            }
            set
            {
                exprtWindow = value;
            }
        }
        public bool? JumpToRelation = false;
        public bool? KeepKeyboardDefaults = true;
        public bool MovedDefaults = false;
        public string LastDCSVersion = "";

        public bool? import_default=false;
        public bool? import_slider = false;
        public bool? import_inverted = false;
        public bool? import_deadzone = false;
        public bool? import_curvature = false;
        public bool? import_satx = false;
        public bool? import_saty = false;
        public bool? AutoAddDBItems = null;
        public bool? SelectedPlaneItemAddOnly = null;

        WindowPos joyFFn = null;
        public WindowPos _JoystickFF
        {
            get
            {
                if (joyFFn == null) joyFFn = new WindowPos();
                if (joyFFn.Width <= 0 ||
                    joyFFn.Height <= 0 ||
                    joyFFn.Top < 0 ||
                    joyFFn.Left < 0)
                {
                    joyFFn.Width = 1920;
                    joyFFn.Height = 1080;
                    joyFFn.Top = 0;
                    joyFFn.Left = 0;
                }
                return joyFFn;
            }
            set
            {
                joyFFn = value;
            }
        }

        public MetaSave()
        {
            lastGameSelected = "";
            lastInstanceSelected = "";
            lastOpenedLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            relationWindowLast = new WindowPos();
            importWindowLast = new WindowPos();
            mainWLast = new WindowPos();
            exchangeW = new WindowPos();
            modifierW = new WindowPos();
            validW = new WindowPos();
            settingsW = new WindowPos();
            stick2ExW = new WindowPos();
            timeToSet = default_timeToSet;
            axisThreshold = default_axisThreshold;
            warmupTime = default_warmupTime;
            pollWaitTime = default_pollWaitTime;
            NukeSticks = false;
            backupDays = default_backupDays;
            backupW = new WindowPos();
            usrCvW = new WindowPos();
            grpMngr = new WindowPos();
            aliasCr = new WindowPos();
            joyManAs = new WindowPos();
            DCSInstaceOverride = "";
            DCSInstallPathOR = "";
            IL2OR = "";
            SCOR = "";
            additionModulesToScan = default_modulesToScan;
            overlaySW = new WindowPos();
            overlayW = new WindowPos();
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
            collectSticks = new WindowPos();
            maxVisualLayers = default_maxVisualLayers;
            AddAditionalAndCorrectRelationItems = default_AddAditionalCorrectItems;
            relationPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
            importPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
            exportPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
            ViewPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
            ExportInView = false;
            exprtWindow = new WindowPos();
            JumpToRelation = false;
            KeepKeyboardDefaults = true;
            MovedDefaults = false;
            LastDCSVersion = "";
        }

        public bool? PlaneWasActiveLastTime(PlaneActivitySelection pas, string game, string plane)
        {
            MakeSureActivityReferencesArentNull();
            Dictionary<string, Dictionary<string, bool>> activity=null;
            if (pas == PlaneActivitySelection.Relation) activity = relationPlaneActivity;
            else if (pas == PlaneActivitySelection.Import) activity = importPlaneActivity;
            else if (pas == PlaneActivitySelection.Export) activity = exportPlaneActivity;
            else if (pas == PlaneActivitySelection.View) activity = ViewPlaneActivity;
            foreach(KeyValuePair<string, Dictionary<string , bool>> pair in activity)
            {
                if(pair.Key.ToLower() == game.ToLower())
                {
                    foreach(KeyValuePair<string, bool> kvp in pair.Value)
                    {
                        if(kvp.Key.ToLower() == plane.ToLower())return kvp.Value;
                    }
                    return null;
                }
            }
            return null;
        }
        void MakeSureActivityReferencesArentNull()
        {
            if (relationPlaneActivity==null) relationPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
            if (importPlaneActivity == null) importPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
            if (exportPlaneActivity == null) exportPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
            if (ViewPlaneActivity == null) ViewPlaneActivity = new Dictionary<string, Dictionary<string, bool>>();
        }
        public void PlaneSetLastActivity(PlaneActivitySelection pas, string game, string plane, bool state)
        {
            MakeSureActivityReferencesArentNull();
            Dictionary<string, Dictionary<string, bool>> activity = null;
            if (pas == PlaneActivitySelection.Relation) activity = relationPlaneActivity;
            else if (pas == PlaneActivitySelection.Import) activity = importPlaneActivity;
            else if (pas == PlaneActivitySelection.Export) activity = exportPlaneActivity;
            else if (pas == PlaneActivitySelection.View) activity = ViewPlaneActivity;
            if(!activity.ContainsKey(game))activity.Add(game, new Dictionary<string, bool>());
            if (!activity[game].ContainsKey(plane)) activity[game].Add(plane, state);
            else activity[game][plane] = state;
        }

    }
}
