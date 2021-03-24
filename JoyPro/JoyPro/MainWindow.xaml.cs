using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        int buttonSetting;
        JoystickReader joyReader;
        public string selectedSort;
        List<ColumnDefinition> colDefs = null;
        List<ColumnDefinition> colHds = null;

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            Application.Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
            InitializeComponent();
            CURRENTDISPLAYEDRELATION = new List<Relation>();
            MessageBox.Show("Please Backup your existing binds. C:\\Users\\USERNAME\\Saved Games\\DCS Please make a backup of these folders somewhere outside your savegames.");
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
            ALLWINDOWS = new List<Window>();
            MainStructure.mainW = this;
            DropDownGameSelection.SelectionChanged += new SelectionChangedEventHandler(Event_GameSelectionChanged);
            AddRelationsBtn.Click += new RoutedEventHandler(Event_AddRelation);
            App.Current.MainWindow.Closing += new System.ComponentModel.CancelEventHandler(ProgramClosing);
            LoadRelationsBtn.Click += new RoutedEventHandler(LoadRelationsEvent);
            IncludeRelationsBtn.Click += new RoutedEventHandler(IncludeRelationsEvent);
            LoadProfileBtn.Click += new RoutedEventHandler(LoadProfileEvent);
            SaveRelationsBtn.Click += new RoutedEventHandler(SaveReleationsEvent);
            SaveProfileBtn.Click += new RoutedEventHandler(SaveProfileEvent);
            DropDownInstanceSelection.SelectionChanged += new SelectionChangedEventHandler(InstanceSelectionChanged);
            MEBWEAKEBtn.Click += new RoutedEventHandler(LoadExistingExportKeepExisting);
            MEBWEAOEBtn.Click += new RoutedEventHandler(LoadExistingExportOverwrite);
            CEBAEBtn.Click += new RoutedEventHandler(CleanAndExport);
            MEBWEAABBtn.Click += new RoutedEventHandler(LoadExistingExportAndAdd);
            ImportProfileBtn.Click += new RoutedEventHandler(ImportProf);
            NewFileBtn.Click += new RoutedEventHandler(NewFileEvent);
            ModManagerBtn.Click += new RoutedEventHandler(OpenModifierManager);
            ValidateBtn.Click += new RoutedEventHandler(DoValidate);
            ExchStickBtn.Click += new RoutedEventHandler(ExchangeStick);
            JoystickSettingsBtn.Click += new RoutedEventHandler(ChangeJoystickSettings);
            FirstStart();
            joyReader = null;
            buttonSetting = -1;
            selectedSort = "Relation";
            SortSelectionDropDown.SelectionChanged += new SelectionChangedEventHandler(SortChanged);
            this.SizeChanged += new SizeChangedEventHandler(sizeChanged);
            sv.ScrollChanged += new ScrollChangedEventHandler(sizeChanged);
            this.ContentRendered += new EventHandler(setWindowPosSize);
            this.Loaded += new RoutedEventHandler(AfterLoading);
            CBNukeUnused.Click += new RoutedEventHandler(MainStructure.SaveWindowState);
        }

        void ChangeJoystickSettings(object sender, EventArgs e)
        {
            DisableInputs();
            StickSettings stickSettings = new StickSettings();
            stickSettings.Show();
            stickSettings.Closing += new CancelEventHandler(ActivateInputs);
        }
        void ExchangeStick(object sender, EventArgs e)
        {
            List<string> sticksInBind = new List<string>();
            List<Bind> allBinds = MainStructure.GetAllBinds();
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
            List<Modifier> mods = MainStructure.GetAllModifiers();
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

        void DoValidate(object sender, EventArgs e)
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
            MainStructure.NewFile();
        }
        void AfterLoading(object sender, EventArgs e)
        {
            setWindowPosSize(sender, e);
            if (MainStructure.msave != null)
            {
                if (MainStructure.msave.lastGameSelected.Length > 0)
                {
                    if (MainStructure.msave.lastGameSelected == "Digital Combat Simulator")
                    {
                        DropDownGameSelection.SelectedIndex = 0;
                    }
                }
                MainStructure.AfterLoad();
            }
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
        }
        void sizeChanged(object sender, EventArgs e)
        {
            if (colDefs != null && colHds != null)
            {
                for (int i = 0; i < colDefs.Count; ++i)
                {
                    colHds[i].MinWidth = colDefs[i].ActualWidth;
                }
                svHeader.ScrollToHorizontalOffset(sv.HorizontalOffset);
            }
        }
        void SortChanged(object sender, EventArgs e)
        {
            selectedSort = ((ComboBoxItem)SortSelectionDropDown.SelectedItem).Content.ToString();
            Console.WriteLine(selectedSort);
            MainStructure.ResyncRelations();
        }
        void ImportProf(object sender, EventArgs e)
        {
            if (MainStructure.selectedInstancePath == null || MainStructure.selectedInstancePath.Length < 1)
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
            bool? overwrite = CBKeepDefault.IsChecked;
            bool param;
            if (overwrite == null)
                param = true;
            else if (overwrite == true)
                param = false;
            else
                param = true;
            MainStructure.WriteProfileCleanNotOverwriteLocal(param);
        }
        void LoadExistingExportOverwrite(object sender, EventArgs e)
        {
            bool? overwrite = CBKeepDefault.IsChecked;
            bool param;
            if (overwrite == null)
                param = true;
            else if (overwrite == true)
                param = false;
            else
                param = true;
            MainStructure.WriteProfileCleanAndLoadedOverwritten(param);
        }
        void LoadExistingExportAndAdd(object sender, EventArgs e)
        {
            bool? overwrite = CBKeepDefault.IsChecked;
            bool param;
            if (overwrite == null)
                param = true;
            else if (overwrite == true)
                param = false;
            else
                param = true;
            MainStructure.WriteProfileCleanAndLoadedOverwrittenAndAdd(param);
        }
        void CleanAndExport(object sender, EventArgs e)
        {
            bool nukeDevices = false;
            if (CBNukeUnused.IsChecked == true)
                nukeDevices = true;
            MainStructure.WriteProfileClean(nukeDevices);
        }
        void SaveProfileEvent(object sender, EventArgs e)
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
                MainStructure.SaveProfileTo(filePath);
            }
        }
        void SaveReleationsEvent(object sender, EventArgs e)
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
            MainStructure.SaveRelationsTo(filePath);

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
        void LoadRelationsEvent(object sender, EventArgs e)
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
                MainStructure.LoadRelations(fileToOpen);
            }
        }
        void IncludeRelationsEvent(object sender, EventArgs e)
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
                MainStructure.InsertRelations(filesToInclude);
            }
        }
        void LoadProfileEvent(object sendder, EventArgs e)
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
                MainStructure.LoadProfile(fileToOpen);
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
                MainStructure.RemoveRelation(r);
            }
        }
        void EditRelationButton(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            Console.WriteLine(b.Name);
            int indx = Convert.ToInt32(b.Name.Replace("editBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            DisableInputs();
            RelationWindow rw = new RelationWindow();
            ALLWINDOWS.Add(rw);
            rw.Current = r;
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
            for (int i = 0; i < 12; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
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
            mods = new ComboBox[CURRENTDISPLAYEDRELATION.Count, 4];
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
            if (e == null)
            {
                setBtns[indx].Content = "None";
                stickLabels[indx].Content = "None";
                return;
            }
            Bind cr = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                MainStructure.AddBind(cr.Rl.NAME, cr);
            }
            if (joyReader == null)
            {
                MessageBox.Show("Something went wrong. Joyreader is null try again.");
                return;
            }
            if (joyReader.result == null)
            {
                //Delete Bind
                MainStructure.DeleteBind(cr.Rl.NAME);
                MainStructure.ResyncRelations();
                return;
            }
            cr.Joystick = joyReader.result.Device;
            cr.JAxis = joyReader.result.AxisButton;
            setBtns[indx].Content = joyReader.result.AxisButton;
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
            if (joyReader.result == null)
            {
                setBtns[indx].Content = "None";
                stickLabels[indx].Content = "None";
                return;
            }
            Bind cr = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                MainStructure.AddBind(cr.Rl.NAME, cr);
            }
            cr.Joystick = joyReader.result.Device;
            cr.JButton = joyReader.result.AxisButton;
            setBtns[indx].Content = joyReader.result.AxisButton;
            Console.WriteLine(setBtns[indx].Content);
            stickLabels[indx].Content = joyReader.result.Device;
            joyReader = null;
        }
        void modSelectionChanged(object sender, EventArgs e)
        {
            ComboBox cx = (ComboBox)sender;
            int indx = Convert.ToInt32(cx.Name.Split('x')[1]);
            Bind currentBind = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
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
                Modifier m = MainStructure.ModifierByName(element);
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
            MainStructure.ResyncRelations();
        }
        void RefreshRelationsToShow()
        {
            List<string> allMods = MainStructure.GetAllModsAsString();
            allMods.Add("Delete");
            Grid grid = BaseSetupRelationGrid();
            for (int i = 0; i < CURRENTDISPLAYEDRELATION.Count; i++)
            {
                Label lblName = new Label();
                lblName.Name = "lblname" + i.ToString();
                lblName.Foreground = Brushes.White;
                lblName.Content = CURRENTDISPLAYEDRELATION[i].NAME;
                lblName.HorizontalAlignment = HorizontalAlignment.Center;
                lblName.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(lblName, 0);
                Grid.SetRow(lblName, i);
                grid.Children.Add(lblName);

                Button editBtn = new Button();
                editBtns[i] = editBtn;
                editBtn.Name = "editBtn" + i.ToString();
                editBtn.Content = "Edit";
                editBtn.Click += new RoutedEventHandler(EditRelationButton);
                editBtn.HorizontalAlignment = HorizontalAlignment.Center;
                editBtn.VerticalAlignment = VerticalAlignment.Center;
                editBtn.Width = 50;
                Grid.SetColumn(editBtn, 1);
                Grid.SetRow(editBtn, i);
                grid.Children.Add(editBtn);

                Button dupBtn = new Button();
                dupBtns[i] = dupBtn;
                dupBtn.Name = "dupBtn" + i.ToString();
                dupBtn.Content = "Duplicate";
                dupBtn.Click += new RoutedEventHandler(duplicateRelation);
                dupBtn.HorizontalAlignment = HorizontalAlignment.Center;
                dupBtn.VerticalAlignment = VerticalAlignment.Center;
                dupBtn.Width = 75;
                Grid.SetColumn(dupBtn, 2);
                Grid.SetRow(dupBtn, i);
                grid.Children.Add(dupBtn);

                Button deleteBtn = new Button();
                dltBtns[i] = deleteBtn;
                deleteBtn.Name = "deleteBtn" + i.ToString();
                deleteBtn.Content = "Delete Relation";
                deleteBtn.Click += new RoutedEventHandler(DeleteRelationButton);
                deleteBtn.HorizontalAlignment = HorizontalAlignment.Center;
                deleteBtn.VerticalAlignment = VerticalAlignment.Center;
                deleteBtn.Width = 100;
                Grid.SetColumn(deleteBtn, 3);
                Grid.SetRow(deleteBtn, i);
                grid.Children.Add(deleteBtn);

                Bind currentBind = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[i].NAME);

                Label joystickPick = new Label();
                joystickPick.Name = "joyLbl" + i.ToString();
                joystickPick.Content = "None";
                stickLabels[i] = joystickPick;
                joystickPick.Foreground = Brushes.White;
                if (currentBind != null)
                {
                    joystickPick.Content = currentBind.Joystick;
                }
                joystickPick.Width = 500;
                joystickPick.HorizontalAlignment = HorizontalAlignment.Center;
                joystickPick.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(joystickPick, 4);
                Grid.SetRow(joystickPick, i);
                grid.Children.Add(joystickPick);

                Button joybtnin = new Button();
                joybtnin.Name = "assignBtn" + i.ToString();
                joybtnin.Content = "None";
                joybtnin.HorizontalAlignment = HorizontalAlignment.Center;
                joybtnin.VerticalAlignment = VerticalAlignment.Center;
                joybtnin.Width = 100;
                joybtnin.Click += new RoutedEventHandler(SetBtnOrAxisEvent);
                setBtns[i] = joybtnin;
                Grid.SetColumn(joybtnin, 5);
                Grid.SetRow(joybtnin, i);
                grid.Children.Add(joybtnin);

                if (CURRENTDISPLAYEDRELATION[i].ISAXIS)
                {

                    CheckBox cbx = new CheckBox();
                    cbx.Name = "cbxrel" + i.ToString();
                    cbx.Content = "Inverted";
                    cbx.Foreground = Brushes.White;
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(cbx, 6);
                    Grid.SetRow(cbx, i);
                    grid.Children.Add(cbx);

                    CheckBox cbxs = new CheckBox();
                    cbxs.Name = "cbxsrel" + i.ToString();
                    cbxs.Content = "Slider";
                    cbxs.Foreground = Brushes.White;
                    cbxs.HorizontalAlignment = HorizontalAlignment.Center;
                    cbxs.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(cbxs, 7);
                    Grid.SetRow(cbxs, i);
                    grid.Children.Add(cbxs);

                    TextBox txrl = new TextBox();
                    txrl.Name = "txrldz" + i.ToString();
                    txrl.Width = 150;
                    txrl.Height = 24;
                    Grid.SetColumn(txrl, 8);
                    Grid.SetRow(txrl, i);
                    grid.Children.Add(txrl);

                    TextBox txrlsx = new TextBox();
                    txrlsx.Name = "txrlsatx" + i.ToString();
                    txrlsx.Width = 150;
                    txrlsx.Height = 24;
                    Grid.SetColumn(txrlsx, 9);
                    Grid.SetRow(txrlsx, i);
                    grid.Children.Add(txrlsx);

                    TextBox txrlsy = new TextBox();
                    txrlsy.Name = "txrlsaty" + i.ToString();
                    txrlsy.Width = 150;
                    txrlsy.Height = 24;
                    Grid.SetColumn(txrlsy, 10);
                    Grid.SetRow(txrlsy, i);
                    grid.Children.Add(txrlsy);

                    TextBox txrlcv = new TextBox();
                    txrlcv.Name = "txrlsacv" + i.ToString();
                    txrlcv.Width = 150;
                    txrlcv.Height = 24;
                    Grid.SetColumn(txrlcv, 11);
                    Grid.SetRow(txrlcv, i);
                    grid.Children.Add(txrlcv);

                    if (currentBind != null)
                    {
                        if(currentBind.JAxis!=null&& currentBind.JAxis.Length>0)
                            joybtnin.Content = currentBind.JAxis.ToString();
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
                            txrlcv.Text = currentBind.Curvature[0].ToString();
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
                    Grid.SetColumn(modCbx1, 8);
                    Grid.SetRow(modCbx1, i);
                    grid.Children.Add(modCbx1);

                    ComboBox modCbx2 = new ComboBox();
                    modCbx2.Name = "mod2cbx" + i.ToString();
                    modCbx2.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx2.VerticalAlignment = VerticalAlignment.Center;
                    modCbx2.Width = 150;
                    modCbx2.ItemsSource = allMods;
                    mods[i, 1] = modCbx2;
                    Grid.SetColumn(modCbx2, 9);
                    Grid.SetRow(modCbx2, i);
                    grid.Children.Add(modCbx2);

                    ComboBox modCbx3 = new ComboBox();
                    modCbx3.Name = "mod3cbx" + i.ToString();
                    modCbx3.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx3.VerticalAlignment = VerticalAlignment.Center;
                    modCbx3.Width = 150;
                    modCbx3.ItemsSource = allMods;
                    mods[i, 2] = modCbx3;
                    Grid.SetColumn(modCbx3, 10);
                    Grid.SetRow(modCbx3, i);
                    grid.Children.Add(modCbx3);

                    ComboBox modCbx4 = new ComboBox();
                    modCbx4.Name = "mod4cbx" + i.ToString();
                    modCbx4.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx4.VerticalAlignment = VerticalAlignment.Center;
                    modCbx4.Width = 150;
                    modCbx4.ItemsSource = allMods;
                    mods[i, 3] = modCbx4;
                    Grid.SetColumn(modCbx4, 11);
                    Grid.SetRow(modCbx4, i);
                    grid.Children.Add(modCbx4);


                    //Check against mod buttons needed
                    if (currentBind != null)
                    {
                        joybtnin.Content = currentBind.JButton;
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
            sv.Content = grid;

        }

        void duplicateRelation(object sender, EventArgs e)
        {
            int indx= Convert.ToInt32(((Button)sender).Name.Replace("dupBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            string name = r.NAME;
            while (MainStructure.DoesRelationAlreadyExist(name))
            {
                name = name + "1";
            }
            Relation nw = r.Copy();
            nw.NAME = name;
            if (nw.bind != null)
            {
                MainStructure.AddBind(nw.NAME, nw.bind);
            }
            MainStructure.AddRelation(nw);
            
                
            
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
        void SetHeadersForScrollView()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            colHds = new List<ColumnDefinition>();
            for (int i = 0; i < 12; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                grid.ColumnDefinitions.Add(c);
                colHds.Add(c);
                c.MinWidth = colDefs[i].ActualWidth;
            }


            Label relPick = new Label();
            relPick.Name = "joyHdrLblRlName";
            relPick.Content = "Relation Name";
            relPick.Foreground = Brushes.White;
            relPick.HorizontalAlignment = HorizontalAlignment.Center;
            relPick.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(relPick, 0);
            grid.Children.Add(relPick);

            Label joystickPick = new Label();
            joystickPick.Name = "joyHdrLbldeviceName";
            joystickPick.Content = "Device Name";
            joystickPick.Foreground = Brushes.White;
            joystickPick.HorizontalAlignment = HorizontalAlignment.Center;
            joystickPick.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(joystickPick, 4);
            grid.Children.Add(joystickPick);

            Label joystickBtn = new Label();
            joystickBtn.Name = "joyHdrLblaxisname";
            joystickBtn.Content = "Axis/Btn Name";
            joystickBtn.Foreground = Brushes.White;
            joystickBtn.HorizontalAlignment = HorizontalAlignment.Center;
            joystickBtn.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(joystickBtn, 5);
            grid.Children.Add(joystickBtn);

            Label joystickAxisS = new Label();
            joystickAxisS.Content = "Axis Setting";
            joystickAxisS.Foreground = Brushes.White;
            joystickAxisS.HorizontalAlignment = HorizontalAlignment.Center;
            joystickAxisS.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(joystickAxisS, 6);
            grid.Children.Add(joystickAxisS);

            Label dzlbl = new Label();
            dzlbl.Content = "Mod1/Deadzone";
            dzlbl.Foreground = Brushes.White;
            dzlbl.HorizontalAlignment = HorizontalAlignment.Center;
            dzlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(dzlbl, 8);
            grid.Children.Add(dzlbl);

            Label sxlbl = new Label();
            sxlbl.Content = "Mod2/Sat X";
            sxlbl.Foreground = Brushes.White;
            sxlbl.HorizontalAlignment = HorizontalAlignment.Center;
            sxlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(sxlbl, 9);
            grid.Children.Add(sxlbl);

            Label sylbl = new Label();
            sylbl.Content = "Mod3/Sat Y";
            sylbl.Foreground = Brushes.White;
            sylbl.HorizontalAlignment = HorizontalAlignment.Center;
            sylbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(sylbl, 10);
            grid.Children.Add(sylbl);

            Label curvlbl = new Label();
            curvlbl.Content = "Mod4/Curv";
            curvlbl.Foreground = Brushes.White;
            curvlbl.HorizontalAlignment = HorizontalAlignment.Center;
            curvlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(curvlbl, 11);
            grid.Children.Add(curvlbl);

            svHeader.Content = grid;
        }
        private void Event_GameSelectionChanged(object sender, EventArgs e)
        {
            string selected = ((ComboBoxItem)DropDownGameSelection.SelectedItem).Content.ToString();
            switch (selected)
            {
                case "Digital Combat Simulator":
                    InitDCS();
                    ActivateInputs();
                    MainStructure.LoadCleanLuas();
                    MainStructure.msave.lastGameSelected = "Digital Combat Simulator";
                    if (MainStructure.msave.lastInstanceSelected.Length > 0)
                    {
                        if (DropDownInstanceSelection.Items.Contains(MainStructure.msave.lastInstanceSelected))
                        {
                            DropDownInstanceSelection.SelectedItem = MainStructure.msave.lastInstanceSelected;
                        }
                    }
                    break;
                default: break;
            }
        }
        void InvertAxisSelection(object sender, EventArgs e)
        {
            CheckBox cx = (CheckBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("cbxrel", ""));
            Bind cr = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                MainStructure.AddBind(cr.Rl.NAME, cr);
            }
            cr.Inverted = cx.IsChecked;
        }
        void SliderAxisSelection(object sender, EventArgs e)
        {
            CheckBox cx = (CheckBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("cbxsrel", ""));
            Bind cr = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                MainStructure.AddBind(cr.Rl.NAME, cr);
            }
            cr.Slider = cx.IsChecked;
        }
        void SaturationXSelectionChanged(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("txrlsatx", ""));
            Bind cr = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                {
                    MessageBox.Show("Please set first the button or the axis.");
                }
                else
                {
                    MainStructure.ResyncRelations();
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
            Bind cr = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                    MessageBox.Show("Please set first the button or the axis.");
                else
                    MainStructure.ResyncRelations();
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
            Bind cr = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                    MessageBox.Show("Please set first the button or the axis.");
                else
                    MainStructure.ResyncRelations();
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
            Bind cr = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                    MessageBox.Show("Please set first the button or the axis.");
                else
                    MainStructure.ResyncRelations();
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
        void InitDCS()
        {
            MainStructure.InitDCSData();
            DropDownInstanceSelection.Items.Clear();
            if (MainStructure.SelectedGame == Game.DCS)
                foreach (string inst in MainStructure.DCSInstances)
                    DropDownInstanceSelection.Items.Add(inst);
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
            MainStructure.ResyncRelations();
        }
        void Event_AddRelation(object sender, EventArgs e)
        {
            DisableInputs();
            RelationWindow rw = new RelationWindow();
            ALLWINDOWS.Add(rw);
            rw.Show();
            rw.Closed += new EventHandler(WindowClosing);
        }

        void FirstStart()
        {
            for (int i = 0; i < ALLBUTTONS.Count; ++i)
                ALLBUTTONS[i].IsEnabled = false;
            MainStructure.LoadMetaLast();

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

            DropDownGameSelection.IsEnabled = true;
            for (int i = 0; i < ALLBUTTONS.Count; ++i)
                ALLBUTTONS[i].IsEnabled = true;
            if (dltBtns != null)
                for (int i = 0; i < dltBtns.Length; ++i)
                {
                    dltBtns[i].IsEnabled = true;
                    editBtns[i].IsEnabled = true;
                    setBtns[i].IsEnabled = true;
                    dupBtns[i].IsEnabled = false;
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
        }
        void DisableInputs()
        {
            DropDownGameSelection.IsEnabled = false;
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

        }
        private void InstanceSelectionChanged(object sender, EventArgs e)
        {
            //MainStructure.LoadLocalBinds((string)DropDownInstanceSelection.SelectedItem);
            MainStructure.selectedInstancePath = (string)DropDownInstanceSelection.SelectedItem;
            if (MainStructure.msave != null)
            {
                MainStructure.msave.lastInstanceSelected = (string)DropDownInstanceSelection.SelectedItem;
            }
        }

    }
}
