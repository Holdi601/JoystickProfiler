using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JoyPro
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        List<Button> ALLBUTTONS;
        List<Window> ALLWINDOWS;
        List<Relation> CURRENTDISPLAYEDRELATION;
        Button[] editBtns;
        Button[] dltBtns;
        Button[] dupBtns;
        Button[] setBtns;
        ComboBox[,] mods;
        Label[] stickLabels;
        Label[] relationLabels;
        ComboBox[] groupComboboxes;
        CheckBox[] invertedcbs;
        CheckBox[] slidercbs;
        CheckBox[] usercurvecbs;
        Button[] userCurveBtn;
        TextBox[,] tboxes; 
        int buttonSetting;
        JoystickReader joyReader;
        public string selectedSort;
        List<ColumnDefinition> colDefs = null;
        List<ColumnDefinition> colHds = null;
        Control[] controls = null;
        List<Button> additional;
        int gridCols;
        List<string> possibleSticks;

        public MainWindow()
        {
            //AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            //Application.Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
            InitializeComponent();
            gridCols = 15;
            CURRENTDISPLAYEDRELATION = new List<Relation>();
            ALLWINDOWS = new List<Window>();
            setGridBordersLightGray();
            MainStructure.mainW = this;
            ButtonsIntoList();
            SetupEventHandlers();
            FirstStart();
            joyReader = null;
            buttonSetting = -1;
            selectedSort = "Relation";
            
        }
        void setGridBordersLightGray()
        {
            var T = Type.GetType("System.Windows.Controls.Grid+GridLinesRenderer," + " PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            var GLR = Activator.CreateInstance(T);
            GLR.GetType().GetField("s_oddDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.LightGray, 0.5));
            GLR.GetType().GetField("s_evenDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.LightGray, 0.5));
        }
        void FilterSearchConfirm(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FilterSearchResult(sender, e);
            }
        }
        void FilterSearchResult(object sender, EventArgs e)
        {
            string rawQry = SearchQueryRelationName.Text;
            if (rawQry.Replace(" ", "").Length < 1)
                InternalDataMangement.RelationWordFilter = null;
            else
            {
                rawQry = rawQry.Trim();
                InternalDataMangement.RelationWordFilter = rawQry.Split(' ');
            }
            InternalDataMangement.ResyncRelations();
        }
        void ButtonsIntoList()
        {
            ALLBUTTONS = new List<Button>();
            ALLBUTTONS.Add(LoadRelationsBtn);
            ALLBUTTONS.Add(SaveRelationsBtn);
            ALLBUTTONS.Add(AddRelationsBtn);
            ALLBUTTONS.Add(IncludeRelationsBtn);
            ALLBUTTONS.Add(MEBWEAKEBtn);
            ALLBUTTONS.Add(CEBAEBtn);
            ALLBUTTONS.Add(MEBWEAOEBtn);
            ALLBUTTONS.Add(SaveProfileBtn);
            ALLBUTTONS.Add(LoadProfileBtn);
            ALLBUTTONS.Add(MEBWEAABBtn);
            ALLBUTTONS.Add(ImportProfileBtn);
            ALLBUTTONS.Add(NewFileBtn);
            ALLBUTTONS.Add(ModManagerBtn);
            ALLBUTTONS.Add(ValidateBtn);
            ALLBUTTONS.Add(ReinstateBUBtn);
            ALLBUTTONS.Add(GroupManagerBtn);
            ALLBUTTONS.Add(ExchStickBtn);
            ALLBUTTONS.Add(SettingsBtn);
        }
        void SetupEventHandlers()
        {
            SearchQueryRelationName.AcceptsReturn = true;
            App.Current.MainWindow.Closing += new System.ComponentModel.CancelEventHandler(ProgramClosing);
            SetupButtonsEventHandler();
            SetupDropDownsEventHandlers();
            this.SizeChanged += new SizeChangedEventHandler(sizeChanged);
            sv.ScrollChanged += new ScrollChangedEventHandler(sizeChanged);
            this.ContentRendered += new EventHandler(setWindowPosSize);
            this.Loaded += new RoutedEventHandler(AfterLoading);
            SearchQueryRelationName.LostFocus += new RoutedEventHandler(FilterSearchResult);
            SearchQueryRelationName.KeyUp += new KeyEventHandler(FilterSearchConfirm);
            CBNukeUnused.Click += new RoutedEventHandler(MainStructure.SaveWindowState);
        }
        void SetupDropDownsEventHandlers()
        {
            DropDownInstanceSelection.SelectionChanged += new SelectionChangedEventHandler(InstanceSelectionChanged);
        }
        void SetupButtonsEventHandler()
        {
            AddRelationsBtn.Click += new RoutedEventHandler(OpenRelation);
            LoadRelationsBtn.Click += new RoutedEventHandler(OpenLoadRelations);
            IncludeRelationsBtn.Click += new RoutedEventHandler(OpenIncludeRelations);
            LoadProfileBtn.Click += new RoutedEventHandler(OpenLoadProfile);
            SaveRelationsBtn.Click += new RoutedEventHandler(OpenSaveReleations);
            SaveProfileBtn.Click += new RoutedEventHandler(OpenSaveProfile);
            MEBWEAKEBtn.Click += new RoutedEventHandler(LoadExistingExportKeepExisting);
            MEBWEAOEBtn.Click += new RoutedEventHandler(LoadExistingExportOverwrite);
            CEBAEBtn.Click += new RoutedEventHandler(CleanAndExport);
            MEBWEAABBtn.Click += new RoutedEventHandler(LoadExistingExportAndAdd);
            ImportProfileBtn.Click += new RoutedEventHandler(OpenImportProf);
            NewFileBtn.Click += new RoutedEventHandler(NewFileEvent);
            ModManagerBtn.Click += new RoutedEventHandler(OpenModifierManager);
            ValidateBtn.Click += new RoutedEventHandler(OpenValidation);
            ExchStickBtn.Click += new RoutedEventHandler(OpenExchangeStick);
            SettingsBtn.Click += new RoutedEventHandler(OpenChangeJoystickSettings);
            ReinstateBUBtn.Click += new RoutedEventHandler(OpenBackupWindow);
            GroupManagerBtn.Click += new RoutedEventHandler(OpenGroupManager);
            OTCBtn.Click += new RoutedEventHandler(OpenSaveCSV);
        }
        void OpenBackupWindow(object sender, EventArgs e)
        {
            DisableInputs();
            ReinstateBackup ri = new ReinstateBackup();
            ri.Closing += new CancelEventHandler(ActivateInputs);
            ri.Show();
        }
        void OpenChangeJoystickSettings(object sender, EventArgs e)
        {
            DisableInputs();
            StickSettings stickSettings = new StickSettings();
            stickSettings.Show();
            stickSettings.Closing += new CancelEventHandler(ActivateInputs);
        }
        void OpenExchangeStick(object sender, EventArgs e)
        {
            List<string> sticksInBind = new List<string>();
            List<Bind> allBinds = InternalDataMangement.GetAllBinds();
            for (int i = 0; i < allBinds.Count; ++i)
            {
                if (allBinds[i].Joystick.Length > 0)
                {
                    if (!sticksInBind.Contains(allBinds[i].Joystick))
                    {
                        sticksInBind.Add(allBinds[i].Joystick);
                    }
                }
            }
            List<Modifier> mods = InternalDataMangement.GetAllModifiers();
            for (int i = 0; i < mods.Count; ++i)
            {
                string otherId = mods[i].device;
                if (otherId.Contains('{'))
                {
                    otherId = otherId.Split('{')[0] + "{" + otherId.Split('{')[1].ToUpper();
                }

                if (!sticksInBind.Contains(otherId))
                {
                    sticksInBind.Add(otherId);
                }

            }
            DisableInputs();
            StickToExchange ste = new StickToExchange(sticksInBind);
            ste.Show();
            ste.Closing += new CancelEventHandler(ActivateInputs);
        }
        void OpenValidation(object sender, EventArgs e)
        {
            Validation validate = new Validation();
            ValidationErrors win = new ValidationErrors(validate);
            win.Show();
        }
        void OpenModifierManager(object sender, EventArgs e)
        {
            DisableInputs();
            ModifierManager rw = new ModifierManager();
            ALLWINDOWS.Add(rw);
            rw.Show();
            rw.Closed += new EventHandler(WindowClosing);
        }
        void NewFileEvent(object sender, EventArgs e)
        {
            InternalDataMangement.NewFile();
        }
        void AfterLoading(object sender, EventArgs e)
        {
            setWindowPosSize(sender, e);
            if (MainStructure.msave != null)
            {
                MainStructure.AfterLoad();
            }
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
        }
        void sizeChanged(object sender, EventArgs e)
        {
            if (colDefs != null && colHds != null)
            {
                for(int i=0; i < CURRENTDISPLAYEDRELATION.Count; ++i)
                {
                    //relationLabels[i].ActualWidth
                }
                for (int i = 0; i < colDefs.Count; ++i)
                {
                    colHds[i].MinWidth = colDefs[i].ActualWidth;
                    if (controls != null&&controls[i]!=null)
                    {
                        controls[i].MinWidth= colDefs[i].ActualWidth;
                    }
                }
                svHeader.ScrollToHorizontalOffset(sv.HorizontalOffset);
            }
        }
        void OpenGroupManager(object sender, EventArgs e)
        {
            GroupManagerW gmw = new GroupManagerW();
            DisableInputs();
            gmw.Show();
            gmw.Closing += new CancelEventHandler(InternalDataMangement.ResyncRelations);
            gmw.Closing += new CancelEventHandler(ActivateInputs);            
        }
        void OpenImportProf(object sender, EventArgs e)
        {
            if (MiscGames.DCSselectedInstancePath == null || MiscGames.DCSselectedInstancePath.Length < 1)
            {
                MessageBox.Show("Not Instance selected");
                return;
            }
            ImportWindow iw = new ImportWindow();
            DisableInputs();
            iw.Show();
            iw.Closing += new CancelEventHandler(ActivateInputs);

        }
        void LoadExistingExportKeepExisting(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to export your profile (Keep-Mode)?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No || res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
            bool? overwrite = CBKeepDefault.IsChecked;
            bool param;
            if (overwrite == null)
                param = true;
            else if (overwrite == true)
                param = false;
            else
                param = true;

            InternalDataMangement.WriteProfileCleanNotOverwriteLocal(param);
        }
        void LoadExistingExportOverwrite(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to export your profile (Overwrite-Mode)?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No || res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
            bool? overwrite = CBKeepDefault.IsChecked;
            bool param;
            if (overwrite == null)
                param = true;
            else if (overwrite == true)
                param = false;
            else
                param = true;
            InternalDataMangement.WriteProfileCleanAndLoadedOverwritten(param);
        }
        void LoadExistingExportAndAdd(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to export your profile (Add-Mode)?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No || res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
            bool? overwrite = CBKeepDefault.IsChecked;
            bool param;
            if (overwrite == null)
                param = true;
            else if (overwrite == true)
                param = false;
            else
                param = true;
            InternalDataMangement.WriteProfileCleanAndLoadedOverwrittenAndAdd(param);
        }
        void CleanAndExport(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to export your profile (Clean-Mode)?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No || res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
            bool nukeDevices = false;
            if (CBNukeUnused.IsChecked == true)
                nukeDevices = true;
            InternalDataMangement.WriteProfileClean(nukeDevices);
        }
        void OpenSaveProfile(object sender, EventArgs e)
        {
            if (CURRENTDISPLAYEDRELATION.Count < 1) return;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Pr0file Files (*.pr0file)|*.pr0file|All filed (*.*)|*.*";
            saveFileDialog1.Title = "Save Pr0file";
            if (Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                saveFileDialog1.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            else
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            string filePath;
            saveFileDialog1.ShowDialog();
            filePath = saveFileDialog1.FileName;
            if(filePath.Length>4)
            {
                string[] pathParts = filePath.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }
                InternalDataMangement.SaveProfileTo(filePath);
            }
        }
        void OpenSaveCSV(object sender, EventArgs e)
        {
            if (CURRENTDISPLAYEDRELATION.Count < 1) return;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Comma seperated values (*.csv)|*.csv|All filed (*.*)|*.*";
            saveFileDialog1.Title = "Save Overview";
            if (Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                saveFileDialog1.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            else
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            string filePath;
            saveFileDialog1.ShowDialog();
            filePath = saveFileDialog1.FileName;
            if (filePath.Length > 4)
            {
                string[] pathParts = filePath.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }

                InternalDataMangement.PrintOverviewToCsv(CURRENTDISPLAYEDRELATION, filePath);
                //InternalDataMangement.SaveProfileTo(filePath);
            }
        }
        void OpenSaveReleations(object sender, EventArgs e)
        {
            if (CURRENTDISPLAYEDRELATION.Count < 1) return;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Relation Files (*.rl)|*.rl";
            saveFileDialog1.Title = "Save Relations";
            if (Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                saveFileDialog1.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            else
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            string filePath;
            saveFileDialog1.ShowDialog();
            filePath = saveFileDialog1.FileName;
            string[] pathParts = filePath.Split('\\');
            if (pathParts.Length > 0)
            {
                MainStructure.msave.lastOpenedLocation = pathParts[0];
                for (int i = 1; i < pathParts.Length - 1; ++i)
                {
                    MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                }
            }
            InternalDataMangement.SaveRelationsTo(filePath);

        }
        public bool? RelationAlreadyExists(string relName)
        {
            // MessageBox.Show("The relation "+relName+" already exists. Do you want to overwrite it? Select None for auto renaming", )
            string message = "The relation " + relName + " already exists. Do you want to overwrite it? Select Cancel for auto renaming";
            const string caption = "Relation already exists";
            var result = MessageBox.Show(message, caption, MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes) return true;
            else if (result == MessageBoxResult.No) return false;
            else return null;
        }
        void OpenLoadRelations(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Relation Files (*.rl)|*.rl|All filed (*.*)|*.*";
            ofd.Title = "Load Relations";
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            if (MainStructure.msave.lastOpenedLocation.Length < 1 || !Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                ofd.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            string fileToOpen;
            if (ofd.ShowDialog() == true)
            {
                fileToOpen = ofd.FileName;
                string[] pathParts = fileToOpen.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }
                InternalDataMangement.LoadRelations(fileToOpen);
            }
        }
        void OpenIncludeRelations(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Relation Files (*.rl)|*.rl|All filed (*.*)|*.*";
            ofd.Title = "Include Relations";
            if (MainStructure.msave.lastOpenedLocation.Length < 1 || !Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                ofd.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            string[] filesToInclude;
            if (ofd.ShowDialog() == true)
            {
                filesToInclude = ofd.FileNames;
                string lastFile = filesToInclude[filesToInclude.Length - 1];
                string[] pathParts = lastFile.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }
                InternalDataMangement.InsertRelations(filesToInclude);
            }
        }
        void OpenLoadProfile(object sendder, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Pr0file Files (*.pr0file)|*.pr0file|All filed (*.*)|*.*";
            ofd.Title = "Load Pr0file";
            if (MainStructure.msave.lastOpenedLocation.Length < 1 || !Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                ofd.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }

            string fileToOpen;
            if (ofd.ShowDialog() == true)
            {
                Console.WriteLine(ofd.FileName);
                fileToOpen = ofd.FileName;
                string[] pathParts = fileToOpen.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }
                InternalDataMangement.LoadProfile(fileToOpen);
            }
        }
        void DeleteRelationButton(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            int indx = Convert.ToInt32(b.Name.Replace("deleteBtn", ""));
            MessageBoxResult mr=  MessageBox.Show("Are you sure that you want to delete the Relation: "+CURRENTDISPLAYEDRELATION[indx].NAME, "Joy Pro Relation Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if(mr== MessageBoxResult.Yes)
            {
                Relation r = CURRENTDISPLAYEDRELATION[indx];
                InternalDataMangement.RemoveRelation(r);
            }
        }
        void EditRelationButton(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            Console.WriteLine(b.Name);
            int indx = Convert.ToInt32(b.Name.Replace("editBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            DisableInputs();
            RelationWindow rw = new RelationWindow(r);
            ALLWINDOWS.Add(rw);
            rw.Show();
            rw.Closed += new EventHandler(WindowClosing);
            rw.Refresh();
            DisableInputs();
        }
        Grid BaseSetupRelationGrid()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            colDefs = new List<ColumnDefinition>();
            for (int i = 0; i < gridCols; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                if (i == 6)
                {
                    c.Width = new GridLength(80, GridUnitType.Star);
                }
                grid.ColumnDefinitions.Add(c);
                colDefs.Add(c);
            }
            for (int i = 0; i < CURRENTDISPLAYEDRELATION.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());
            editBtns = new Button[CURRENTDISPLAYEDRELATION.Count];
            dltBtns = new Button[CURRENTDISPLAYEDRELATION.Count];
            setBtns = new Button[CURRENTDISPLAYEDRELATION.Count];
            dupBtns = new Button[CURRENTDISPLAYEDRELATION.Count];
            stickLabels = new Label[CURRENTDISPLAYEDRELATION.Count];
            relationLabels = new Label[CURRENTDISPLAYEDRELATION.Count];
            mods = new ComboBox[CURRENTDISPLAYEDRELATION.Count, 4];
            tboxes = new TextBox[CURRENTDISPLAYEDRELATION.Count, 4];
            groupComboboxes= new ComboBox[CURRENTDISPLAYEDRELATION.Count];
            invertedcbs = new CheckBox[CURRENTDISPLAYEDRELATION.Count];
            slidercbs = new CheckBox[CURRENTDISPLAYEDRELATION.Count];
            usercurvecbs = new CheckBox[CURRENTDISPLAYEDRELATION.Count];
            userCurveBtn = new Button[CURRENTDISPLAYEDRELATION.Count];
            return grid;
        }
        public void ShowMessageBox(string msg)
        {
            MessageBox.Show(msg);
        }
        void SetBtnOrAxisEvent(object sender, EventArgs e)
        {
            DisableInputs();
            Button b = (Button)sender;
            int indx = Convert.ToInt32(b.Name.Replace("assignBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            buttonSetting = indx;
            b.Content = "SETTING";
            b.Background = Brushes.Green;
            BackgroundWorker bw = new BackgroundWorker();
            if (r.ISAXIS)
            {
                bw.DoWork += new DoWorkEventHandler(listenAxis);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(AxisSet);
            }
            else
            {
                bw.DoWork += new DoWorkEventHandler(listenButton);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ButtonSet);
            }
            bw.RunWorkerAsync();
        }
        void listenButton(object sender, EventArgs e)
        {
            joyReader = new JoystickReader(false);
        }
        void listenAxis(object sender, EventArgs e)
        {
            joyReader = new JoystickReader(true);
        }
        void AxisSet(object sender, EventArgs e)
        {
            ActivateInputs();
            int indx = buttonSetting;
            buttonSetting = -1;
            setBtns[indx].Background = Brushes.White;
            Bind cr = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (e == null)
            {
                setBtns[indx].Content = "None";
                stickLabels[indx].Content = "None";
                if (cr != null)
                {
                    InternalDataMangement.RemoveBind(cr);
                }
                return;
            }
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                InternalDataMangement.AddBind(cr.Rl.NAME, cr);
            }
            if (joyReader == null)
            {
                MessageBox.Show("Something went wrong. Joyreader is null try again.");
                return;
            }
            if (joyReader.result == null)
            {
                //Delete Bind
                InternalDataMangement.DeleteBind(cr.Rl.NAME);
                InternalDataMangement.ResyncRelations();
                return;
            }
            cr.Joystick = joyReader.result.Device;
            cr.JAxis = joyReader.result.AxisButton;
            setBtns[indx].Content = joyReader.result.AxisButton.Replace("JOY_", "Axis-");
            stickLabels[indx].Content = joyReader.result.Device;
            joyReader = null;
            Console.WriteLine(setBtns[indx].Content);
        }
        void ButtonSet(object sender, EventArgs e)
        {
            ActivateInputs();
            int indx = buttonSetting;
            buttonSetting = -1;
            setBtns[indx].Background = Brushes.White;
            Bind cr = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (joyReader==null||joyReader.result == null)
            {
                setBtns[indx].Content = "None";
                stickLabels[indx].Content = "None";
                if (cr != null)
                {
                    InternalDataMangement.RemoveBind(cr);
                }
                return;
            }
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                InternalDataMangement.AddBind(cr.Rl.NAME, cr);
            }
            cr.Joystick = joyReader.result.Device;
            cr.JButton = joyReader.result.AxisButton;
            setBtns[indx].Content = joyReader.result.AxisButton.Replace("JOY_BTN", "Button-");
            Console.WriteLine(setBtns[indx].Content);
            stickLabels[indx].Content = joyReader.result.Device;
            joyReader = null;
        }
        void modSelectionChanged(object sender, EventArgs e)
        {
            ComboBox cx = (ComboBox)sender;
            int indx = Convert.ToInt32(cx.Name.Split('x')[1]);
            Bind currentBind = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            int modint = Convert.ToInt32(cx.Name.Split('c')[0].Replace("mod", ""))-1;
            if (currentBind == null)
            {
                MessageBox.Show("Please bind the main key first and then the modifiers");
                return;
            }
            string element = (string)cx.SelectedItem;
            if (element == "Delete")
            {
                if (modint < currentBind.AllReformers.Count)
                {
                    currentBind.AllReformers.RemoveAt(modint);
                }
            }
            else
            {
                Modifier m = InternalDataMangement.ModifierByName(element);
                if (m == null)
                {
                    MessageBox.Show("Something went wrong when trying to assing modifier please report with accurate repro steps or simply retry");
                    return;
                }
                if (!currentBind.AllReformers.Contains(m.toReformerString()))
                {
                    currentBind.AllReformers.Add(m.toReformerString());
                }
            }
            InternalDataMangement.ResyncRelations();
        }
        void changeCurveToUserCurve(object sender, EventArgs e)
        {
            CheckBox cx = (CheckBox)sender;
            int indx = Convert.ToInt32(cx.Name.Split('l')[1]);
            Bind currentBind = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (currentBind == null)
            {
                MessageBox.Show("Please bind the main key first and then the User Curve");
                return;
            }
            currentBind.Curvature = new List<double>();
            currentBind.Curvature.Add(0.0);
            if (cx.IsChecked == true)
            {
                currentBind.Curvature.Add(0.1);
                currentBind.Curvature.Add(0.2);
                currentBind.Curvature.Add(0.3);
                currentBind.Curvature.Add(0.4);
                currentBind.Curvature.Add(0.5);
                currentBind.Curvature.Add(0.6);
                currentBind.Curvature.Add(0.7);
                currentBind.Curvature.Add(0.8);
                currentBind.Curvature.Add(0.9);
                currentBind.Curvature.Add(1.0);
            }
            InternalDataMangement.ResyncRelations();
        }
        void changeUserCurve(object sender, EventArgs e)
        {
            Button cx = (Button)sender;
            int indx = Convert.ToInt32(cx.Name.Split('n')[1]);
            Bind currentBind = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (currentBind == null)
            {
                MessageBox.Show("Please bind the main key first and then the User Curve");
                return;
            }
            UserCurveDCS uc = new UserCurveDCS(currentBind);
            uc.Show();
            DisableInputs();
            uc.Closing += new CancelEventHandler(ActivateInputs);
        }
        void DeviceFilterChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Name == "ALL" && cb.IsChecked == true)
            {
                for (int i = InternalDataMangement.JoystickActivity.Count - 1; i >= 0; i--)
                {
                    string toChange = InternalDataMangement.JoystickActivity.ElementAt(i).Key;
                    InternalDataMangement.JoystickActivity[toChange] = true;
                }
                cb.IsChecked = false;
            }
            else if (cb.Name == "NONE" && cb.IsChecked == true)
            {
                for (int i = InternalDataMangement.JoystickActivity.Count - 1; i >= 0; i--)
                {
                    string toChange = InternalDataMangement.JoystickActivity.ElementAt(i).Key;
                    InternalDataMangement.JoystickActivity[toChange] = false;
                }

                cb.IsChecked = false;
            }
            else if(cb.Name== "UNASSIGNED")
            {
                if (cb.IsChecked == true)
                    InternalDataMangement.showUnassignedRelations = true;
                else
                    InternalDataMangement.showUnassignedRelations = false;
            }
            else
            {
                int indx = Convert.ToInt32(cb.Name.Replace("d", ""));
                if (cb.IsChecked == true)
                    InternalDataMangement.JoystickActivity[possibleSticks[indx]] = true;
                else
                    InternalDataMangement.JoystickActivity[possibleSticks[indx]] = false;
            }

            InternalDataMangement.ResyncRelations();
        }
        void GroupFilterChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Name == "ALL"&&cb.IsChecked==true)
            {
                for (int i = InternalDataMangement.GroupActivity.Count - 1; i >= 0; i--)
                {
                    string toChange = InternalDataMangement.GroupActivity.ElementAt(i).Key;
                    InternalDataMangement.GroupActivity[toChange] = true;
                }
                cb.IsChecked = false;
            }else if(cb.Name == "NONE"&&cb.IsChecked==true)
            {
                for (int i = InternalDataMangement.GroupActivity.Count - 1; i >= 0; i--)
                {
                    string toChange= InternalDataMangement.GroupActivity.ElementAt(i).Key;
                    InternalDataMangement.GroupActivity[toChange] = false;
                }
                
                cb.IsChecked = false;
            }else if (cb.Name == "UNASSIGNED")
            {
                if (cb.IsChecked == true)
                {
                    InternalDataMangement.showUnassignedGroups = true;
                }
                else
                {
                    InternalDataMangement.showUnassignedGroups = false;
                }
            }
            else
            {
                int indx = Convert.ToInt32(cb.Name.Replace("g",""));
                if (cb.IsChecked == true)
                    InternalDataMangement.GroupActivity[InternalDataMangement.AllGroups[indx]] = true;
                else
                    InternalDataMangement.GroupActivity[InternalDataMangement.AllGroups[indx]] = false;
            }

            InternalDataMangement.ResyncRelations();
        }
        void RefreshRelationsToShow()
        {
            DeviceDropdown.Items.Clear();
            CheckBox dvcbAll = new CheckBox();
            dvcbAll.Name = "ALL";
            dvcbAll.Content = "ALL";
            dvcbAll.IsChecked = false;
            dvcbAll.Click += new RoutedEventHandler(DeviceFilterChanged);
            DeviceDropdown.Items.Add(dvcbAll);

            CheckBox dvcbNone = new CheckBox();
            dvcbNone.Name = "NONE";
            dvcbNone.Content = "NONE";
            dvcbNone.IsChecked = false;
            dvcbNone.Click += new RoutedEventHandler(DeviceFilterChanged);
            DeviceDropdown.Items.Add(dvcbNone);

            CheckBox dvcbUnassigned = new CheckBox();
            dvcbUnassigned.Name = "UNASSIGNED";
            dvcbUnassigned.Content = "UNASSIGNED";
            dvcbUnassigned.IsChecked = InternalDataMangement.showUnassignedRelations;
            dvcbUnassigned.Click += new RoutedEventHandler(DeviceFilterChanged);
            DeviceDropdown.Items.Add(dvcbUnassigned);

            possibleSticks = InternalDataMangement.GetAllPossibleJoysticks();
            possibleSticks.Sort();
            if (possibleSticks.Count != InternalDataMangement.JoystickActivity.Count)
            {
                InternalDataMangement.JoystickActivity.Clear();
                for(int i=0; i<possibleSticks.Count; ++i)
                {
                    InternalDataMangement.JoystickActivity.Add(possibleSticks[i], true);
                }
            }
            if (InternalDataMangement.JoystickAliases == null)
                InternalDataMangement.JoystickAliases = new Dictionary<string, string>();
            for (int i=0; i< InternalDataMangement.JoystickActivity.Count; ++i)
            {
                CheckBox dvcbItem = new CheckBox();
                dvcbItem.Name = "d" + i.ToString();
                string deviceNameToShow = InternalDataMangement.JoystickActivity.ElementAt(i).Key;
                if (InternalDataMangement.JoystickAliases.ContainsKey(deviceNameToShow) && InternalDataMangement.JoystickAliases[deviceNameToShow].Length > 0)
                {
                    deviceNameToShow = InternalDataMangement.JoystickAliases[deviceNameToShow];
                }
                dvcbItem.Content = deviceNameToShow;
                if (InternalDataMangement.JoystickActivity.ElementAt(i).Value == true)
                {
                    dvcbItem.IsChecked = true;
                }
                else
                {
                    dvcbItem.IsChecked = false;
                }
                dvcbItem.Click += new RoutedEventHandler(DeviceFilterChanged);
                DeviceDropdown.Items.Add(dvcbItem);
            }

            GroupFilterDropdown.Items.Clear();
            CheckBox cbAll = new CheckBox();
            cbAll.Name = "ALL";
            cbAll.Content = "ALL";
            cbAll.IsChecked = false;
            cbAll.Click += new RoutedEventHandler(GroupFilterChanged);
            GroupFilterDropdown.Items.Add(cbAll);

            CheckBox cbNone = new CheckBox();
            cbNone.Name = "NONE";
            cbNone.Content = "NONE";
            cbNone.IsChecked = false;
            cbNone.Click += new RoutedEventHandler(GroupFilterChanged);
            GroupFilterDropdown.Items.Add(cbNone);

            CheckBox cbUnass = new CheckBox();
            cbUnass.Name = "UNASSIGNED";
            cbUnass.Content = "UNASSIGNED";
            cbUnass.IsChecked = InternalDataMangement.showUnassignedGroups;
            cbUnass.Click += new RoutedEventHandler(GroupFilterChanged);
            GroupFilterDropdown.Items.Add(cbUnass);

            if (InternalDataMangement.AllGroups.Count != InternalDataMangement.GroupActivity.Count)
            {
                InternalDataMangement.GroupActivity.Clear();
                for (int b = 0; b < InternalDataMangement.AllGroups.Count; ++b)
                {
                    InternalDataMangement.GroupActivity.Add(InternalDataMangement.AllGroups[b], true);
                }
            }
            if (InternalDataMangement.JoystickAliases == null) InternalDataMangement.JoystickAliases = new Dictionary<string, string>();
            for (int b=0; b< InternalDataMangement.AllGroups.Count; ++b)
            {
                CheckBox cbItem = new CheckBox();
                cbItem.Name = "g" + b.ToString();
                cbItem.Content = InternalDataMangement.AllGroups[b];
                if (InternalDataMangement.GroupActivity[InternalDataMangement.AllGroups[b]] == true)
                    cbItem.IsChecked = true;
                else
                    cbItem.IsChecked = false;
                cbItem.Click += new RoutedEventHandler(GroupFilterChanged);
                GroupFilterDropdown.Items.Add(cbItem);
            }
            

            additional = new List<Button>();
            List<string> allMods = InternalDataMangement.GetAllModsAsString();
            allMods.Add("Delete");
            Grid grid = BaseSetupRelationGrid();
            for (int i = 0; i < CURRENTDISPLAYEDRELATION.Count; i++)
            {
                Label lblName = new Label();
                lblName.Name = "lblname" + i.ToString();
                lblName.Foreground = Brushes.White;
                lblName.Content = CURRENTDISPLAYEDRELATION[i].NAME;
                lblName.HorizontalAlignment = HorizontalAlignment.Left;
                lblName.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(lblName, 0);
                Grid.SetRow(lblName, i);
                grid.Children.Add(lblName);
                relationLabels[i] = lblName;
                lblName.MouseEnter += new MouseEventHandler(OnHoverRelationLabel);
                lblName.MouseLeave += new MouseEventHandler(OnLeaveRelationLabel);

                Button editBtn = new Button();
                editBtns[i] = editBtn;
                editBtn.Name = "editBtn" + i.ToString();
                editBtn.Content = "Edit";
                editBtn.Click += new RoutedEventHandler(EditRelationButton);
                editBtn.HorizontalAlignment = HorizontalAlignment.Center;
                editBtn.VerticalAlignment = VerticalAlignment.Center;
                editBtn.Width = 50;
                Grid.SetColumn(editBtn, 2);
                Grid.SetRow(editBtn, i);
                grid.Children.Add(editBtn);
                editBtn.MouseEnter += new MouseEventHandler(OnHoverRelationEdit);
                editBtn.MouseLeave += new MouseEventHandler(OnLeaveRelationEdit);

                Button dupBtn = new Button();
                dupBtns[i] = dupBtn;
                dupBtn.Name = "dupBtn" + i.ToString();
                dupBtn.Content = "Duplicate";
                dupBtn.Click += new RoutedEventHandler(duplicateRelation);
                dupBtn.HorizontalAlignment = HorizontalAlignment.Center;
                dupBtn.VerticalAlignment = VerticalAlignment.Center;
                dupBtn.Width = 75;
                Grid.SetColumn(dupBtn, 3);
                Grid.SetRow(dupBtn, i);
                grid.Children.Add(dupBtn);
                dupBtn.MouseEnter += new MouseEventHandler(OnHoverRelationDup);
                dupBtn.MouseLeave += new MouseEventHandler(OnLeaveRelationDup);

                Button deleteBtn = new Button();
                dltBtns[i] = deleteBtn;
                deleteBtn.Name = "deleteBtn" + i.ToString();
                deleteBtn.Content = "Delete Relation";
                deleteBtn.Click += new RoutedEventHandler(DeleteRelationButton);
                deleteBtn.HorizontalAlignment = HorizontalAlignment.Center;
                deleteBtn.VerticalAlignment = VerticalAlignment.Center;
                deleteBtn.Width = 100;
                Grid.SetColumn(deleteBtn, 4);
                Grid.SetRow(deleteBtn, i);
                grid.Children.Add(deleteBtn);
                deleteBtn.MouseEnter += new MouseEventHandler(OnHoverRelationDel);
                deleteBtn.MouseLeave += new MouseEventHandler(OnLeaveRelationDel);

                ComboBox groupDropdown = new ComboBox();
                groupDropdown.Name = "GroupDropDown" + i.ToString();
                groupDropdown.HorizontalAlignment = HorizontalAlignment.Center;
                groupComboboxes[i] = groupDropdown;
                groupDropdown.VerticalAlignment = VerticalAlignment.Center;
                groupDropdown.Width = 150;
                for(int a=0; a< InternalDataMangement.AllGroups.Count; ++a)
                {
                    CheckBox cbxGroup = new CheckBox();
                    cbxGroup.Name = "r" + i.ToString() + "g" + a.ToString();
                    cbxGroup.Content = InternalDataMangement.AllGroups[a];
                    if (CURRENTDISPLAYEDRELATION[i].Groups!=null&&CURRENTDISPLAYEDRELATION[i].Groups.Contains(InternalDataMangement.AllGroups[a]))
                        cbxGroup.IsChecked = true;
                    else
                        cbxGroup.IsChecked = false;
                    cbxGroup.Checked += new RoutedEventHandler(GroupManagementCheckboxChange);
                    groupDropdown.Items.Add(cbxGroup);
                }
                Grid.SetColumn(groupDropdown, 5);
                Grid.SetRow(groupDropdown, i);
                grid.Children.Add(groupDropdown);
                groupDropdown.MouseEnter += new MouseEventHandler(OnHoverRelationGrp);
                groupDropdown.MouseLeave += new MouseEventHandler(OnLeaveRelationGrp);

                Bind currentBind = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[i].NAME);

                Label joystickPick = new Label();
                joystickPick.Name = "joyLbl" + i.ToString();
                joystickPick.Content = "None";
                stickLabels[i] = joystickPick;
                joystickPick.Foreground = Brushes.White;
                if (currentBind != null)
                {                 

                     if (InternalDataMangement.JoystickAliases.ContainsKey(currentBind.Joystick) && InternalDataMangement.JoystickAliases[currentBind.Joystick].Length > 0)
                     {
                         joystickPick.Content = InternalDataMangement.JoystickAliases[currentBind.Joystick];
                     }
                     else
                     {
                         joystickPick.Content = currentBind.Joystick;
                     }
                    joystickPick.MouseLeftButtonUp += new MouseButtonEventHandler(OpenJoystickCreateAlias);                  
                }
                joystickPick.HorizontalAlignment = HorizontalAlignment.Center;
                joystickPick.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(joystickPick, 6);
                Grid.SetRow(joystickPick, i);
                grid.Children.Add(joystickPick);
                joystickPick.MouseEnter += new MouseEventHandler(OnHoverRelationStick);
                joystickPick.MouseLeave += new MouseEventHandler(OnLeaveRelationStick);

                Button joybtnin = new Button();
                joybtnin.Name = "assignBtn" + i.ToString();
                joybtnin.Content = "None";
                joybtnin.HorizontalAlignment = HorizontalAlignment.Center;
                joybtnin.VerticalAlignment = VerticalAlignment.Center;
                joybtnin.Width = 100;
                joybtnin.Click += new RoutedEventHandler(SetBtnOrAxisEvent);
                joybtnin.MouseRightButtonUp += new MouseButtonEventHandler(ManualBtnAxSet);
                setBtns[i] = joybtnin;
                Grid.SetColumn(joybtnin, 7);
                Grid.SetRow(joybtnin, i);
                grid.Children.Add(joybtnin);
                joybtnin.MouseEnter += new MouseEventHandler(OnHoverRelationAssign);
                joybtnin.MouseLeave += new MouseEventHandler(OnLeaveRelationAssign);

                if (CURRENTDISPLAYEDRELATION[i].ISAXIS)
                {

                    CheckBox cbx = new CheckBox();
                    cbx.Name = "cbxrel" + i.ToString();
                    cbx.Content = "Inverted";
                    cbx.Foreground = Brushes.White;
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.VerticalAlignment = VerticalAlignment.Center;
                    invertedcbs[i] = cbx;
                    Grid.SetColumn(cbx, 8);
                    Grid.SetRow(cbx, i);
                    grid.Children.Add(cbx);
                    cbx.MouseEnter += new MouseEventHandler(OnHoverRelationInvCB);
                    cbx.MouseLeave += new MouseEventHandler(OnLeaveRelationInvCB);

                    CheckBox cbxs = new CheckBox();
                    cbxs.Name = "cbxsrel" + i.ToString();
                    cbxs.Content = "Slider";
                    cbxs.Foreground = Brushes.White;
                    cbxs.HorizontalAlignment = HorizontalAlignment.Center;
                    cbxs.VerticalAlignment = VerticalAlignment.Center;
                    slidercbs[i] = cbxs;
                    Grid.SetColumn(cbxs, 9);
                    Grid.SetRow(cbxs, i);
                    grid.Children.Add(cbxs);
                    cbxs.MouseEnter += new MouseEventHandler(OnHoverRelationSliderCB);
                    cbxs.MouseLeave += new MouseEventHandler(OnLeaveRelationSliderCB);

                    CheckBox cbxu = new CheckBox();
                    cbxu.Name = "cbxsrel" + i.ToString();
                    cbxu.Content = "User Curve";
                    cbxu.Foreground = Brushes.White;
                    cbxu.HorizontalAlignment = HorizontalAlignment.Center;
                    cbxu.VerticalAlignment = VerticalAlignment.Center;
                    cbxu.Click += new RoutedEventHandler(changeCurveToUserCurve);
                    usercurvecbs[i] = cbxu;
                    Grid.SetColumn(cbxu, 10);
                    Grid.SetRow(cbxu, i);
                    grid.Children.Add(cbxu);
                    cbxu.MouseEnter += new MouseEventHandler(OnHoverRelationUserCCB);
                    cbxu.MouseLeave += new MouseEventHandler(OnLeaveRelationUserCCB);

                    TextBox txrl = new TextBox();
                    txrl.Name = "txrldz" + i.ToString();
                    txrl.Width = 150;
                    txrl.Height = 24;
                    Grid.SetColumn(txrl, 11);
                    Grid.SetRow(txrl, i);
                    tboxes[i, 0] = txrl;
                    grid.Children.Add(txrl);
                    txrl.MouseEnter += new MouseEventHandler(OnHoverRelationTB1);
                    txrl.MouseLeave += new MouseEventHandler(OnLeaveRelationTB1);

                    TextBox txrlsx = new TextBox();
                    txrlsx.Name = "txrlsatx" + i.ToString();
                    txrlsx.Width = 150;
                    txrlsx.Height = 24;
                    Grid.SetColumn(txrlsx, 12);
                    Grid.SetRow(txrlsx, i);
                    tboxes[i, 1] = txrlsx;
                    grid.Children.Add(txrlsx);
                    txrlsx.MouseEnter += new MouseEventHandler(OnHoverRelationTB2);
                    txrlsx.MouseLeave += new MouseEventHandler(OnLeaveRelationTB2);

                    TextBox txrlsy = new TextBox();
                    txrlsy.Name = "txrlsaty" + i.ToString();
                    txrlsy.Width = 150;
                    txrlsy.Height = 24;
                    Grid.SetColumn(txrlsy, 13);
                    Grid.SetRow(txrlsy, i);
                    tboxes[i, 2] = txrlsy;
                    grid.Children.Add(txrlsy);
                    txrlsy.MouseEnter += new MouseEventHandler(OnHoverRelationTB3);
                    txrlsy.MouseLeave += new MouseEventHandler(OnLeaveRelationTB3);

                    TextBox txrlcv = new TextBox();
                    txrlcv.Name = "txrlsacv" + i.ToString();
                    txrlcv.Width = 150;
                    txrlcv.Height = 24;
                    Grid.SetColumn(txrlcv, 14);
                    Grid.SetRow(txrlcv, i);
                    tboxes[i, 3] = txrlcv;
                    txrlcv.MouseEnter += new MouseEventHandler(OnHoverRelationTB4);
                    txrlcv.MouseLeave += new MouseEventHandler(OnLeaveRelationTB4);

                    Button userCurvBtn = new Button();
                    userCurvBtn.Name = "UsrcvBtn" + i.ToString();
                    userCurvBtn.Content = "User Curve";
                    userCurvBtn.HorizontalAlignment = HorizontalAlignment.Center;
                    userCurvBtn.VerticalAlignment = VerticalAlignment.Center;
                    userCurvBtn.Width = 100;
                    userCurvBtn.Click += new RoutedEventHandler(changeUserCurve);
                    additional.Add(userCurvBtn);
                    Grid.SetColumn(userCurvBtn, 14);
                    Grid.SetRow(userCurvBtn, i);
                    userCurveBtn[i] = userCurvBtn;
                    userCurvBtn.MouseEnter += new MouseEventHandler(OnHoverRelationUCB);
                    userCurvBtn.MouseLeave += new MouseEventHandler(OnLeaveRelationUCB);

                    if (currentBind != null)
                    {
                        if(currentBind.JAxis!=null&& currentBind.JAxis.Length > 0)
                        {
                            joybtnin.Content = currentBind.JAxis.Replace("JOY_","Axis-").ToString();
                        } 
                        else
                        {
                            joybtnin.Content = "ERROR PLEASE REASSIGN";
                        }
                        if(currentBind.Inverted != null)
                            cbx.IsChecked = currentBind.Inverted;
                        else
                        {
                            currentBind.Inverted = false;
                            cbx.IsChecked = false;
                        }
                        if (currentBind.Slider!=null)
                        {
                            cbxs.IsChecked = currentBind.Slider;
                        }
                        else
                        {
                            currentBind.Slider = false;
                            cbxs.IsChecked = false;
                        }
                        if (!double.IsNaN(currentBind.Deadzone))
                        {
                            txrl.Text = currentBind.Deadzone.ToString();
                        }
                        else
                        {
                            currentBind.Deadzone = 0.0;
                            txrl.Text = "0";
                        }
                        if ( !double.IsNaN(currentBind.SaturationX))
                        {
                            txrlsx.Text = currentBind.SaturationX.ToString();
                        }
                        else
                        {
                            currentBind.SaturationX = 1.0;
                            txrlsx.Text = "1";
                        }
                        if ( !double.IsNaN(currentBind.SaturationY))
                        {
                            txrlsy.Text = currentBind.SaturationY.ToString();
                        }
                        else
                        {
                            currentBind.SaturationY = 1.0;
                            txrlsy.Text = "1";
                        }
                        if(currentBind.Curvature!=null&& currentBind.Curvature.Count > 0&&!double.IsNaN(currentBind.Curvature[0]))
                        {
                            if (currentBind.Curvature.Count == 1)
                            {
                                txrlcv.Text = currentBind.Curvature[0].ToString();
                                cbxu.IsChecked = false;
                                grid.Children.Add(txrlcv);
                            }
                            else
                            {
                                cbxu.IsChecked = true;
                                grid.Children.Add(userCurvBtn);
                            }
                            
                        }
                        else
                        {
                            currentBind.Curvature = new List<double>();
                            currentBind.Curvature.Add(0);
                            txrlcv.Text = "0.0";
                        }                        
                    }
                    else
                    {
                        txrl.Text = "Deadzone (Dec)";
                        txrlsx.Text = "SatX (Dec)";
                        txrlsy.Text = "SatY (Dec)";
                        txrlcv.Text = "Curvature (Dec)";
                        cbxu.IsChecked = false;
                        grid.Children.Add(txrlcv);
                    }
                    txrlcv.TextChanged += new TextChangedEventHandler(CurvitureSelectionChanged);
                    txrlsy.TextChanged += new TextChangedEventHandler(SaturationYSelectionChanged);
                    txrlsx.TextChanged += new TextChangedEventHandler(SaturationXSelectionChanged);
                    txrl.TextChanged += new TextChangedEventHandler(DeadzoneSelectionChanged);
                    cbxs.Click += new RoutedEventHandler(SliderAxisSelection);
                    cbx.Click += new RoutedEventHandler(InvertAxisSelection);

                    txrlcv.QueryCursor += new QueryCursorEventHandler(CleanText);
                    txrlsy.QueryCursor += new QueryCursorEventHandler(CleanText);
                    txrlsx.QueryCursor += new QueryCursorEventHandler(CleanText);
                    txrl.QueryCursor += new QueryCursorEventHandler(CleanText);
                    txrlcv.QueryCursor += new QueryCursorEventHandler(CleanText);


                }
                else
                {
                    ComboBox modCbx1 = new ComboBox();
                    modCbx1.Name = "mod1cbx" + i.ToString();
                    modCbx1.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx1.VerticalAlignment = VerticalAlignment.Center;
                    modCbx1.Width = 150;
                    modCbx1.ItemsSource = allMods;
                    mods[i, 0] = modCbx1;
                    Grid.SetColumn(modCbx1, 11);
                    Grid.SetRow(modCbx1, i);
                    grid.Children.Add(modCbx1);
                    modCbx1.MouseEnter += new MouseEventHandler(OnHoverRelationMod1);
                    modCbx1.MouseLeave += new MouseEventHandler(OnLeaveRelationMod1);

                    ComboBox modCbx2 = new ComboBox();
                    modCbx2.Name = "mod2cbx" + i.ToString();
                    modCbx2.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx2.VerticalAlignment = VerticalAlignment.Center;
                    modCbx2.Width = 150;
                    modCbx2.ItemsSource = allMods;
                    mods[i, 1] = modCbx2;
                    Grid.SetColumn(modCbx2, 12);
                    Grid.SetRow(modCbx2, i);
                    grid.Children.Add(modCbx2);
                    modCbx2.MouseEnter += new MouseEventHandler(OnHoverRelationMod2);
                    modCbx2.MouseLeave += new MouseEventHandler(OnLeaveRelationMod2);

                    ComboBox modCbx3 = new ComboBox();
                    modCbx3.Name = "mod3cbx" + i.ToString();
                    modCbx3.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx3.VerticalAlignment = VerticalAlignment.Center;
                    modCbx3.Width = 150;
                    modCbx3.ItemsSource = allMods;
                    mods[i, 2] = modCbx3;
                    Grid.SetColumn(modCbx3, 13);
                    Grid.SetRow(modCbx3, i);
                    grid.Children.Add(modCbx3);
                    modCbx3.MouseEnter += new MouseEventHandler(OnHoverRelationMod3);
                    modCbx3.MouseLeave += new MouseEventHandler(OnLeaveRelationMod3);

                    ComboBox modCbx4 = new ComboBox();
                    modCbx4.Name = "mod4cbx" + i.ToString();
                    modCbx4.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx4.VerticalAlignment = VerticalAlignment.Center;
                    modCbx4.Width = 150;
                    modCbx4.ItemsSource = allMods;
                    mods[i, 3] = modCbx4;
                    Grid.SetColumn(modCbx4, 14);
                    Grid.SetRow(modCbx4, i);
                    grid.Children.Add(modCbx4);
                    modCbx4.MouseEnter += new MouseEventHandler(OnHoverRelationMod4);
                    modCbx4.MouseLeave += new MouseEventHandler(OnLeaveRelationMod4);


                    //Check against mod buttons needed
                    if (currentBind != null)
                    {
                        string btnraw = currentBind.JButton.Replace("JOY_BTN","Button-");
                        joybtnin.Content = btnraw;
                        if (currentBind.AllReformers.Count > 0)
                        {
                            modCbx1.SelectedItem = currentBind.AllReformers[0].Split('§')[0];
                            if (currentBind.AllReformers.Count > 1)
                            {
                                modCbx2.SelectedItem = currentBind.AllReformers[1].Split('§')[0];
                                if (currentBind.AllReformers.Count > 2)
                                {
                                    modCbx3.SelectedItem = currentBind.AllReformers[2].Split('§')[0];
                                    if (currentBind.AllReformers.Count > 3)
                                    {
                                        modCbx4.SelectedItem = currentBind.AllReformers[3].Split('§')[0];
                                    }
                                }
                            }
                        }
                    }
                    modCbx1.SelectionChanged += new SelectionChangedEventHandler(modSelectionChanged);
                    modCbx2.SelectionChanged += new SelectionChangedEventHandler(modSelectionChanged);
                    modCbx3.SelectionChanged += new SelectionChangedEventHandler(modSelectionChanged);
                    modCbx4.SelectionChanged += new SelectionChangedEventHandler(modSelectionChanged);
                }
            }
            grid.ShowGridLines = true;
            sv.Content = grid;
        }
        void OnHoverRelationLabel(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            int indx = Convert.ToInt32(l.Name.Replace("lblname", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationLabel(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            int indx = Convert.ToInt32(l.Name.Replace("lblname", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationEdit(object sender, EventArgs e)
        {
            Button l = (Button)sender;
            int indx = Convert.ToInt32(l.Name.Replace("editBtn", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationEdit(object sender, EventArgs e)
        {
            Button l = (Button)sender;
            int indx = Convert.ToInt32(l.Name.Replace("editBtn", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationDup(object sender, EventArgs e)
        {
            Button l = (Button)sender;
            int indx = Convert.ToInt32(l.Name.Replace("dupBtn", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationDup(object sender, EventArgs e)
        {
            Button l = (Button)sender;
            int indx = Convert.ToInt32(l.Name.Replace("dupBtn", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationDel(object sender, EventArgs e)
        {
            Button l = (Button)sender;
            int indx = Convert.ToInt32(l.Name.Replace("deleteBtn", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationDel(object sender, EventArgs e)
        {
            Button l = (Button)sender;
            int indx = Convert.ToInt32(l.Name.Replace("deleteBtn", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationGrp(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("GroupDropDown", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationGrp(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("GroupDropDown", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationStick(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            int indx = Convert.ToInt32(l.Name.Replace("joyLbl", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationStick(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            int indx = Convert.ToInt32(l.Name.Replace("joyLbl", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationAssign(object sender, EventArgs e)
        {
            Button l = (Button)sender;
            int indx = Convert.ToInt32(l.Name.Replace("assignBtn", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationAssign(object sender, EventArgs e)
        {
            Button l = (Button)sender;
            int indx = Convert.ToInt32(l.Name.Replace("assignBtn", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationInvCB(object sender, EventArgs e)
        {
            CheckBox l = (CheckBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("cbxrel", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationInvCB(object sender, EventArgs e)
        {
            CheckBox l = (CheckBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("cbxrel", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationSliderCB(object sender, EventArgs e)
        {
            CheckBox l = (CheckBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("cbxsrel", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationSliderCB(object sender, EventArgs e)
        {
            CheckBox l = (CheckBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("cbxsrel", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationUserCCB(object sender, EventArgs e)
        {
            CheckBox l = (CheckBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("cbxsrel", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationUserCCB(object sender, EventArgs e)
        {
            CheckBox l = (CheckBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("cbxsrel", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationMod1(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("mod1cbx", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationMod1(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("mod1cbx", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationMod2(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("mod2cbx", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationMod2(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("mod2cbx", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationMod3(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("mod3cbx", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationMod3(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("mod3cbx", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationMod4(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("mod4cbx", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationMod4(object sender, EventArgs e)
        {
            ComboBox l = (ComboBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("mod4cbx", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationTB1(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("txrldz", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationTB1(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("txrldz", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationTB2(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("txrlsatx", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationTB2(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("txrlsatx", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationTB3(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("txrlsaty", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationTB3(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("txrlsaty", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationTB4(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("txrlsacv", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationTB4(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("txrlsacv", ""));
            UncolorRow(indx);
        }
        void OnHoverRelationUCB(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("UsrcvBtn", ""));
            ColorRowOrange(indx);
        }
        void OnLeaveRelationUCB(object sender, EventArgs e)
        {
            TextBox l = (TextBox)sender;
            int indx = Convert.ToInt32(l.Name.Replace("UsrcvBtn", ""));
            UncolorRow(indx);
        }
        void ColorRowOrange(int row)
        {
            relationLabels[row].Foreground = Brushes.Orange;
            editBtns[row].Background = Brushes.Orange;
            dupBtns[row].Background = Brushes.Orange;
            dltBtns[row].Background = Brushes.Orange;
            groupComboboxes[row].Background = Brushes.Orange;
            groupComboboxes[row].BorderBrush = Brushes.Orange;
            groupComboboxes[row].Foreground = Brushes.Orange;
            //groupComboboxes[row].ItemContainerStyle.Setters.Add(new)
            stickLabels[row].Foreground = Brushes.Orange;
            setBtns[row].Background = Brushes.Orange;
            if (invertedcbs[row] != null) invertedcbs[row].Foreground = Brushes.Orange;
            if (slidercbs[row] != null) slidercbs[row].Foreground = Brushes.Orange;
            if (usercurvecbs[row] != null) usercurvecbs[row].Foreground = Brushes.Orange;
            for(int i=0; i<4; ++i)
            {
                if (mods[row, i] != null) mods[row, i].Background = Brushes.Orange;
                if (tboxes[row, i] != null) tboxes[row, i].Background = Brushes.Orange;
            }
            if (userCurveBtn[row] != null) userCurveBtn[row].Background = Brushes.Orange;
        }
        void UncolorRow(int row)
        {
            relationLabels[row].Foreground = Brushes.White;
            editBtns[row].Background = Brushes.White;
            dupBtns[row].Background = Brushes.White;
            dltBtns[row].Background = Brushes.White;
            groupComboboxes[row].Background = Brushes.White;
            stickLabels[row].Foreground = Brushes.White;
            setBtns[row].Background = Brushes.White;
            if (invertedcbs[row] != null) invertedcbs[row].Foreground = Brushes.White;
            if (slidercbs[row] != null) slidercbs[row].Foreground = Brushes.White;
            if (usercurvecbs[row] != null) usercurvecbs[row].Foreground = Brushes.White;
            for (int i = 0; i < 4; ++i)
            {
                if (mods[row, i] != null) mods[row, i].Background = Brushes.White;
                if (tboxes[row, i] != null) tboxes[row, i].Background = Brushes.White;
            }
            if (userCurveBtn[row] != null) userCurveBtn[row].Background = Brushes.White;
        }
        void ManualBtnAxSet(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("assignBtn", ""));
            ManualJoystickAssign mja = new ManualJoystickAssign(CURRENTDISPLAYEDRELATION[indx]);
            DisableInputs();
            ALLWINDOWS.Add(mja);
            mja.Closing += new CancelEventHandler(WindowClosing);
            mja.Show();
        }
        void OpenJoystickCreateAlias(object sender, EventArgs e)
        {
            DisableInputs();
            string name = ((Label)sender).Name;
            int indx = Convert.ToInt32(name.Replace("joyLbl", ""));
            if (CURRENTDISPLAYEDRELATION[indx].bind == null) return;
            string joystick = CURRENTDISPLAYEDRELATION[indx].bind.Joystick;
            CreateJoystickAlias cja = new CreateJoystickAlias(joystick);
            ALLWINDOWS.Add(cja);
            cja.Show();
            
            cja.Closing += new CancelEventHandler(WindowClosing);
        }
        void GroupManagementCheckboxChange(object sender, EventArgs e)
        {
            string name = ((CheckBox)sender).Name;
            string[] nameParts = name.Split('g');
            int relIndex = Convert.ToInt32(nameParts[0].Replace("r", ""));
            int grpIndex = Convert.ToInt32(nameParts[1]);
            if (CURRENTDISPLAYEDRELATION[relIndex].Groups == null) CURRENTDISPLAYEDRELATION[relIndex].Groups = new List<string>();
            if (((CheckBox)sender).IsChecked == true)
            {
                if(!CURRENTDISPLAYEDRELATION[relIndex].Groups.Contains(InternalDataMangement.AllGroups[grpIndex]))
                CURRENTDISPLAYEDRELATION[relIndex].Groups.Add(InternalDataMangement.AllGroups[grpIndex]);
            }
            else
            {
                InternalDataMangement.RemoveGroupFromSpecificRelation(CURRENTDISPLAYEDRELATION[relIndex].NAME, InternalDataMangement.AllGroups[grpIndex]);
            }
        }
        void duplicateRelation(object sender, EventArgs e)
        {
            int indx= Convert.ToInt32(((Button)sender).Name.Replace("dupBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            string name = r.NAME;
            while (InternalDataMangement.DoesRelationAlreadyExist(name))
            {
                name = name + "1";
            }
            Relation nw = r.Copy();
            nw.NAME = name;
            if (nw.bind != null)
            {
                InternalDataMangement.AddBind(nw.NAME, nw.bind);
            }
            InternalDataMangement.AddRelation(nw);
        }
        public void CleanText(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            if (cx.Text == "Deadzone (Dec)" ||
                cx.Text == "SatX (Dec)" ||
                cx.Text == "SatY (Dec)" ||
                cx.Text == "Curvature (Dec)" ||
                cx.Text == "Button (Int o Pov)")
            {
                cx.Text = "";
            }
        }
        public void SetRelationsToView(List<Relation> li)
        {
            CURRENTDISPLAYEDRELATION = li;
            RefreshRelationsToShow();
            SetHeadersForScrollView();
            sizeChanged(null,null);
        }
        void sortName(object o, EventArgs e)
        {
            if (selectedSort == "NAME_NORM")
            {
                selectedSort = "NAME_DESC";
            }
            else
            {
                selectedSort = "NAME_NORM";
            }
            InternalDataMangement.ResyncRelations();
        }
        void sortStick(object o, EventArgs e)
        {
            if (selectedSort == "STICK_NORM")
            {
                selectedSort = "STICK_DESC";
            }
            else
            {
                selectedSort = "STICK_NORM";
            }
            InternalDataMangement.ResyncRelations();
        }
        void sortBtn(object o, EventArgs e)
        {
            if (selectedSort == "BTN_NORM")
            {
                selectedSort = "BTN_DESC";
            }
            else
            {
                selectedSort = "BTN_NORM";
            }
            InternalDataMangement.ResyncRelations();
        }
        void SetHeadersForScrollView()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            colHds = new List<ColumnDefinition>();
            controls = new Control[gridCols];
            for (int i = 0; i < gridCols; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                grid.ColumnDefinitions.Add(c);
                colHds.Add(c);
                c.MinWidth = colDefs[i].ActualWidth;
                c.MaxWidth = colDefs[i].ActualWidth;
            }


            Button relPick = new Button();
            relPick.Name = "joyHdrLblRlName";
            relPick.Content = "Relation Name";
            for (int i = 0; i < 200; ++i) relPick.Content = relPick.Content + " ";
            relPick.Foreground = Brushes.White;
            relPick.HorizontalAlignment = HorizontalAlignment.Stretch;
            relPick.VerticalAlignment = VerticalAlignment.Center;
            relPick.Background = Brushes.Orange;
            relPick.Click += new RoutedEventHandler(sortName);
            Grid.SetColumn(relPick, 0);
            grid.Children.Add(relPick);
            controls[0] = relPick;

            Label abc = new Label();
            abc.Name = "relationManagement";
            abc.Content = "Relation Management";
            abc.Foreground = Brushes.White;
            abc.HorizontalAlignment = HorizontalAlignment.Center;
            abc.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumnSpan(abc, 4);
            Grid.SetColumn(abc, 2);
            grid.Children.Add(abc);

            Button joystickPick = new Button();
            joystickPick.Name = "joyHdrLbldeviceName";
            joystickPick.Content = "Device Name";
            for (int i = 0; i < 200; ++i) joystickPick.Content = joystickPick.Content + " ";
            joystickPick.Foreground = Brushes.White;
            joystickPick.HorizontalAlignment = HorizontalAlignment.Stretch;
            joystickPick.VerticalAlignment = VerticalAlignment.Center;
            joystickPick.Background = Brushes.Orange;
            joystickPick.Click += new RoutedEventHandler(sortStick);
            Grid.SetColumn(joystickPick, 6);
            grid.Children.Add(joystickPick);
            controls[4] = joystickPick;

            Button joystickBtn = new Button();
            joystickBtn.Name = "joyHdrLblaxisname";
            joystickBtn.Content = "Axis/Btn Name";
            joystickBtn.Foreground = Brushes.White;
            joystickBtn.HorizontalAlignment = HorizontalAlignment.Center;
            joystickBtn.VerticalAlignment = VerticalAlignment.Center;
            joystickBtn.Background = Brushes.Orange;
            joystickBtn.Click += new RoutedEventHandler(sortBtn);
            Grid.SetColumn(joystickBtn, 7);
            grid.Children.Add(joystickBtn);
            controls[5] = joystickBtn;

            Label joystickAxisS = new Label();
            joystickAxisS.Content = "Axis Setting";
            joystickAxisS.Foreground = Brushes.White;
            joystickAxisS.HorizontalAlignment = HorizontalAlignment.Center;
            joystickAxisS.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumnSpan(joystickAxisS, 3);
            Grid.SetColumn(joystickAxisS, 8);
            grid.Children.Add(joystickAxisS);

            Label dzlbl = new Label();
            dzlbl.Content = "Mod1/Deadzone";
            dzlbl.Foreground = Brushes.White;
            dzlbl.HorizontalAlignment = HorizontalAlignment.Center;
            dzlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(dzlbl, 11);
            grid.Children.Add(dzlbl);

            Label sxlbl = new Label();
            sxlbl.Content = "Mod2/Sat X";
            sxlbl.Foreground = Brushes.White;
            sxlbl.HorizontalAlignment = HorizontalAlignment.Center;
            sxlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(sxlbl, 12);
            grid.Children.Add(sxlbl);

            Label sylbl = new Label();
            sylbl.Content = "Mod3/Sat Y";
            sylbl.Foreground = Brushes.White;
            sylbl.HorizontalAlignment = HorizontalAlignment.Center;
            sylbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(sylbl, 13);
            grid.Children.Add(sylbl);

            Label curvlbl = new Label();
            curvlbl.Content = "Mod4/Curv";
            curvlbl.Foreground = Brushes.White;
            curvlbl.HorizontalAlignment = HorizontalAlignment.Center;
            curvlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(curvlbl, 14);
            grid.Children.Add(curvlbl);

            svHeader.Content = grid;
        }
        void InvertAxisSelection(object sender, EventArgs e)
        {
            CheckBox cx = (CheckBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("cbxrel", ""));
            Bind cr = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                InternalDataMangement.AddBind(cr.Rl.NAME, cr);
            }
            cr.Inverted = cx.IsChecked;
        }
        void SliderAxisSelection(object sender, EventArgs e)
        {
            CheckBox cx = (CheckBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("cbxsrel", ""));
            Bind cr = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                InternalDataMangement.AddBind(cr.Rl.NAME, cr);
            }
            cr.Slider = cx.IsChecked;
        }
        void SaturationXSelectionChanged(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("txrlsatx", ""));
            Bind cr = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                {
                    MessageBox.Show("Please set first the button or the axis.");
                }
                else
                {
                    InternalDataMangement.ResyncRelations();
                }

                return;
            }
            if (cx.Text.Length < 1 || cx.Text.Replace(" ", "") == ".") return;
            string cleaned = cx.Text.Replace(',', '.');
            try
            {
                cr.SaturationX = Convert.ToDouble(cleaned, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Given SaturationX not a valid double");
            }
        }
        void SaturationYSelectionChanged(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("txrlsaty", ""));
            Bind cr = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                    MessageBox.Show("Please set first the button or the axis.");
                else
                    InternalDataMangement.ResyncRelations();
                return;
            }
            if (cx.Text.Length < 1 || cx.Text.Replace(" ", "") == ".") return;
            string cleaned = cx.Text.Replace(',', '.');
            try
            {
                cr.SaturationY = Convert.ToDouble(cleaned, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Given SaturationY not a valid double");
            }
        }
        void CurvitureSelectionChanged(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("txrlsacv", ""));
            Bind cr = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                    MessageBox.Show("Please set first the button or the axis.");
                else
                    InternalDataMangement.ResyncRelations();
                return;
            }
            if (cx.Text.Length < 1 || cx.Text.Replace(" ", "") == ".") return;
            double curv = double.NaN;
            string cleaned = cx.Text.Replace(',', '.');
            bool succ = false;
            try
            {
                curv = Convert.ToDouble(cleaned, System.Globalization.CultureInfo.InvariantCulture);
                succ = true;
            }
            catch
            {
                MessageBox.Show("Given Curviture not a valid double");
            }
            if (succ == true)
            {
                if (cr.Curvature.Count > 0) cr.Curvature[0] = curv;
                else cr.Curvature.Add(curv);
            }
        }
        void DeadzoneSelectionChanged(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("txrldz", ""));
            Bind cr = InternalDataMangement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                    MessageBox.Show("Please set first the button or the axis.");
                else
                    InternalDataMangement.ResyncRelations();
                return;
            }
            if (cx.Text.Length < 1 || cx.Text.Replace(" ", "") == ".") return;
            string cleaned = cx.Text.Replace(',', '.');
            try
            {
                cr.Deadzone = Convert.ToDouble(cleaned, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Given Deadzone not a valid double");
            }
        }
        void ProgramClosing(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
        void WindowClosing(object sender, EventArgs e)
        {
            Window s = (Window)sender;
            ALLWINDOWS.Remove((Window)sender);
            ActivateInputs();
            InternalDataMangement.ResyncRelations();
        }
        void OpenRelation(object sender, EventArgs e)
        {
            DisableInputs();
            RelationWindow rw = new RelationWindow();
            ALLWINDOWS.Add(rw);
            rw.Show();
            rw.Closed += new EventHandler(WindowClosing);
        }
        void FirstStart()
        {
            MainStructure.InitProgram();
            InternalDataMangement.NewFile(true);
            for (int i = 0; i < ALLBUTTONS.Count; ++i)
                ALLBUTTONS[i].IsEnabled = false;
            MainStructure.LoadMetaLast();
            ActivateInputs();

        }
        void setWindowPosSize(object sender, EventArgs e)
        {
            Console.WriteLine("Should set");
            MainStructure.LoadMetaLast();
            if (MainStructure.msave != null && MainStructure.msave.mainWLast.Width > 0)
            {
                this.Top = MainStructure.msave.mainWLast.Top;
                this.Left = MainStructure.msave.mainWLast.Left;
                this.Width = MainStructure.msave.mainWLast.Width;
                this.Height = MainStructure.msave.mainWLast.Height;
                Console.WriteLine("Done set");
                CBNukeUnused.IsChecked = MainStructure.msave.NukeSticks;
            }
        }
        void ActivateInputs(object sender, EventArgs e)
        {
            ActivateInputs();
        }
        void ActivateInputs()
        {
            for (int i = 0; i < ALLBUTTONS.Count; ++i)
                ALLBUTTONS[i].IsEnabled = true;
            if (dltBtns != null)
                for (int i = 0; i < dltBtns.Length; ++i)
                {
                    dltBtns[i].IsEnabled = true;
                    editBtns[i].IsEnabled = true;
                    setBtns[i].IsEnabled = true;
                    dupBtns[i].IsEnabled = true;
                }
            if (mods != null)
            {
                for(int i=0; i<mods.GetLength(0); ++i)
                {
                    for(int j=0; j<mods.GetLength(1); ++j)
                    {
                        if (mods[i, j] != null)
                            mods[i, j].IsEnabled = true;
                    }
                }
            }
            if (additional != null)
            {
                for(int i=0; i<additional.Count; ++i)
                {
                    if (additional[i] != null)
                        additional[i].IsEnabled = true;
                }
            }
        }
        void DisableInputs()
        {
            for (int i = 0; i < ALLBUTTONS.Count; ++i)
                ALLBUTTONS[i].IsEnabled = false;
            if (dltBtns != null)
                for (int i = 0; i < dltBtns.Length; ++i)
                {
                    dltBtns[i].IsEnabled = false;
                    editBtns[i].IsEnabled = false;
                    setBtns[i].IsEnabled = false;
                    dupBtns[i].IsEnabled = false;
                }
            if (mods != null)
            {
                for (int i = 0; i < mods.GetLength(0); ++i)
                {
                    for (int j = 0; j < mods.GetLength(1); ++j)
                    {
                        if (mods[i, j] != null)
                            mods[i, j].IsEnabled = false;
                    }
                }
            }
            if (additional != null)
            {
                for (int i = 0; i < additional.Count; ++i)
                {
                    if (additional[i] != null)
                        additional[i].IsEnabled = false;
                }
            }

        }
        private void InstanceSelectionChanged(object sender, EventArgs e)
        {
            Console.WriteLine(sender.GetType().ToString());
            if (((ComboBox)sender).SelectedIndex < 0) return;
            MiscGames.DCSInstanceSelectionChanged((string)DropDownInstanceSelection.SelectedItem);
        }    
    }
}
