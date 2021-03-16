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
        Button[] setBtns;
        Dictionary<int, Button[]> modBtns;
        Label[] stickLabels;
        int buttonSetting;
        string modToEdit;
        JoystickReader joyReader;
        public string selectedSort;
        List<ColumnDefinition> colDefs = null;
        List<ColumnDefinition> colHds = null;
        bool started = false;
        public MainWindow()
        {
            //AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            //Application.Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
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
            FirstStart();
            joyReader = null;
            buttonSetting = -1;
            Application.Current.Exit += new ExitEventHandler(SaveMeta);
            //this.SizeChanged += new SizeChangedEventHandler(SaveMeta);
            selectedSort = "Relation";
            SortSelectionDropDown.SelectionChanged += new SelectionChangedEventHandler(SortChanged);
            this.SizeChanged += new SizeChangedEventHandler(sizeChanged);
            sv.ScrollChanged += new ScrollChangedEventHandler(sizeChanged);
            this.ContentRendered += new EventHandler(setWindowPosSize);
            this.Loaded += new RoutedEventHandler(AfterLoading);
            
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
            this.SizeChanged += new SizeChangedEventHandler(SaveMeta);
            this.LocationChanged += new EventHandler(SaveMeta);
        }
        void sizeChanged(object sender, EventArgs e)
        {
            if(colDefs!=null&&colHds!=null)
            {
                double runningWidth = 0;
                for (int i = 0; i < colDefs.Count; ++i)
                {
                    colHds[i].MinWidth = colDefs[i].ActualWidth;
                    runningWidth += colDefs[i].ActualWidth;
                }
                svHeader.ScrollToHorizontalOffset(sv.HorizontalOffset);
            }
            //savepos();
        }
        void SortChanged(object sender, EventArgs e)
        {
            selectedSort = ((ComboBoxItem)SortSelectionDropDown.SelectedItem).Content.ToString();
            Console.WriteLine(selectedSort);
            MainStructure.ResyncRelations();
            savepos();
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
            savepos();

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
            savepos();
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
            savepos();
        }
        void CleanAndExport(object sender, EventArgs e)
        {
            MainStructure.WriteProfileClean();
            savepos();
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
            savepos();
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
            savepos();

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
                savepos();
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
                savepos();
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
                savepos();
            }
        }
        void DeleteRelationButton(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            int indx = Convert.ToInt32(b.Name.Replace("deleteBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            MainStructure.RemoveRelation(r);
            savepos();
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
            savepos();
        }
        Grid BaseSetupRelationGrid()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            colDefs = new List<ColumnDefinition>();
            for (int i = 0; i < 11; ++i)
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
            modBtns = new Dictionary<int, Button[]>();
            for (int i = 0; i < 5; ++i)
            {
                modBtns.Add(i + 1, new Button[CURRENTDISPLAYEDRELATION.Count]);
            }
            stickLabels = new Label[CURRENTDISPLAYEDRELATION.Count];
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
        void listenModBtn(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string[] modAndRelNumber = b.Name.Split('_');
            if (modAndRelNumber.Length < 2) return;
            int indx = Convert.ToInt32(modAndRelNumber[1]);
            int modBtn = Convert.ToInt32(modAndRelNumber[0].Replace("modBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            if (MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME) == null)
            {
                MessageBox.Show("Please do the main Bind first before adding modifiers.");
                return;
            }
            DisableInputs();
            buttonSetting = indx;
            modToEdit = (string)b.Content;
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(listenMod);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(modSet);
            bw.RunWorkerAsync();
        }
        void modSet(object sender, EventArgs e)
        {
            ActivateInputs();
            int indx = buttonSetting;
            buttonSetting = -1;
            Bind cr = MainStructure.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (joyReader == null || cr == null)
            {
                MessageBox.Show("Something went wrong when setting a modifier. Either listener was not started correctly or the main button was not assigend beforehand.");
                return;
            }
            if (modToEdit != "None" && modToEdit.Length > 0)
            {
                cr.deleteReformer(modToEdit);
                modToEdit = "";
            }
            if (joyReader.result == null)
            {
                MainStructure.ResyncRelations();
                return;
            }
            string device;
            if (joyReader.result.Device == "Keyboard")
                device = "Keyboard";
            else
                device = "m"+joyReader.result.Device.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
            string nameToShow = device + joyReader.result.AxisButton;
            string moddedDevice = cr.JoystickGuidToModifierGuid(joyReader.result.Device);
            string toAdd = nameToShow + "§" + moddedDevice + "§" + joyReader.result.AxisButton;
            if (!cr.AllReformers.Contains(toAdd)) cr.AllReformers.Add(toAdd);
            MainStructure.ResyncRelations();
        }
        void listenMod(object sender, EventArgs e)
        {
            joyReader = new JoystickReader(false, true);
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
            savepos();
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
            savepos();
        }
        void RefreshRelationsToShow()
        {
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

                Button deleteBtn = new Button();
                dltBtns[i] = deleteBtn;
                deleteBtn.Name = "deleteBtn" + i.ToString();
                deleteBtn.Content = "Delete Relation";
                deleteBtn.Click += new RoutedEventHandler(DeleteRelationButton);
                deleteBtn.HorizontalAlignment = HorizontalAlignment.Center;
                deleteBtn.VerticalAlignment = VerticalAlignment.Center;
                deleteBtn.Width = 100;
                Grid.SetColumn(deleteBtn, 2);
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
                Grid.SetColumn(joystickPick, 3);
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
                Grid.SetColumn(joybtnin, 4);
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
                    Grid.SetColumn(cbx, 5);
                    Grid.SetRow(cbx, i);
                    grid.Children.Add(cbx);

                    CheckBox cbxs = new CheckBox();
                    cbxs.Name = "cbxsrel" + i.ToString();
                    cbxs.Content = "Slider";
                    cbxs.Foreground = Brushes.White;
                    cbxs.HorizontalAlignment = HorizontalAlignment.Center;
                    cbxs.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(cbxs, 6);
                    Grid.SetRow(cbxs, i);
                    grid.Children.Add(cbxs);

                    TextBox txrl = new TextBox();
                    txrl.Name = "txrldz" + i.ToString();
                    txrl.Width = 100;
                    txrl.Height = 24;
                    Grid.SetColumn(txrl, 7);
                    Grid.SetRow(txrl, i);
                    grid.Children.Add(txrl);

                    TextBox txrlsx = new TextBox();
                    txrlsx.Name = "txrlsatx" + i.ToString();
                    txrlsx.Width = 100;
                    txrlsx.Height = 24;
                    Grid.SetColumn(txrlsx, 8);
                    Grid.SetRow(txrlsx, i);
                    grid.Children.Add(txrlsx);

                    TextBox txrlsy = new TextBox();
                    txrlsy.Name = "txrlsaty" + i.ToString();
                    txrlsy.Width = 100;
                    txrlsy.Height = 24;
                    Grid.SetColumn(txrlsy, 9);
                    Grid.SetRow(txrlsy, i);
                    grid.Children.Add(txrlsy);

                    TextBox txrlcv = new TextBox();
                    txrlcv.Name = "txrlsacv" + i.ToString();
                    txrlcv.Width = 100;
                    txrlcv.Height = 24;
                    Grid.SetColumn(txrlcv, 10);
                    Grid.SetRow(txrlcv, i);
                    grid.Children.Add(txrlcv);

                    if (currentBind != null)
                    {
                        joybtnin.Content = currentBind.JAxis.ToString();
                        cbx.IsChecked = currentBind.Inverted;
                        cbxs.IsChecked = currentBind.Slider;
                        txrl.Text = currentBind.Deadzone.ToString();
                        txrlsx.Text = currentBind.SaturationX.ToString();
                        txrlsy.Text = currentBind.SaturationY.ToString();
                        txrlcv.Text = currentBind.Curviture[0].ToString();
                    }
                    else
                    {
                        txrl.Text = "Deadzone (Dec)";
                        txrlsx.Text = "SatX (Dec)";
                        txrlsy.Text = "SatY (Dec)";
                        txrlcv.Text = "Curviture (Dec)";
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
                    Button modBtn1 = new Button();
                    modBtn1.Name = "modBtn1_" + i.ToString();
                    modBtn1.Content = "None";
                    modBtn1.HorizontalAlignment = HorizontalAlignment.Center;
                    modBtn1.VerticalAlignment = VerticalAlignment.Center;
                    modBtn1.Width = 100;
                    modBtn1.Click += new RoutedEventHandler(listenModBtn);
                    modBtns[1][i] = modBtn1;
                    Grid.SetColumn(modBtn1, 7);
                    Grid.SetRow(modBtn1, i);
                    grid.Children.Add(modBtn1);

                    Button modBtn2 = new Button();
                    modBtn2.Name = "modBtn2_" + i.ToString();
                    modBtn2.Content = "None";
                    modBtn2.HorizontalAlignment = HorizontalAlignment.Center;
                    modBtn2.VerticalAlignment = VerticalAlignment.Center;
                    modBtn2.Width = 100;
                    modBtn2.Click += new RoutedEventHandler(listenModBtn);
                    modBtns[2][i] = modBtn2;
                    Grid.SetColumn(modBtn2, 8);
                    Grid.SetRow(modBtn2, i);
                    grid.Children.Add(modBtn2);

                    Button modBtn3 = new Button();
                    modBtn3.Name = "modBtn3_" + i.ToString();
                    modBtn3.Content = "None";
                    modBtn3.HorizontalAlignment = HorizontalAlignment.Center;
                    modBtn3.VerticalAlignment = VerticalAlignment.Center;
                    modBtn3.Width = 100;
                    modBtn3.Click += new RoutedEventHandler(listenModBtn);
                    modBtns[3][i] = modBtn3;
                    Grid.SetColumn(modBtn3, 9);
                    Grid.SetRow(modBtn3, i);
                    grid.Children.Add(modBtn3);

                    Button modBtn4 = new Button();
                    modBtn4.Name = "modBtn4_" + i.ToString();
                    modBtn4.Content = "None";
                    modBtn4.HorizontalAlignment = HorizontalAlignment.Center;
                    modBtn4.VerticalAlignment = VerticalAlignment.Center;
                    modBtn4.Width = 100;
                    modBtn4.Click += new RoutedEventHandler(listenModBtn);
                    modBtns[4][i] = modBtn4;
                    Grid.SetColumn(modBtn4, 10);
                    Grid.SetRow(modBtn4, i);
                    grid.Children.Add(modBtn4);

                    //Check against mod buttons needed
                    if (currentBind != null)
                    {
                        joybtnin.Content = currentBind.JButton;
                        modBtn1.Content = currentBind.ModToDosplayString(1);
                        modBtn2.Content = currentBind.ModToDosplayString(2);
                        modBtn3.Content = currentBind.ModToDosplayString(3);
                        modBtn4.Content = currentBind.ModToDosplayString(4);
                    }
                }
            }
            sv.Content = grid;
            
        }
        public void CleanText(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            if (cx.Text == "Deadzone (Dec)" ||
                cx.Text == "SatX (Dec)" ||
                cx.Text == "SatY (Dec)" ||
                cx.Text == "Curviture (Dec)" ||
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
            //savepos();
        }
        void SetHeadersForScrollView()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            colHds = new List<ColumnDefinition>();
            for(int i=0; i < 11; ++i)
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
            Grid.SetColumn(joystickPick, 3);
            grid.Children.Add(joystickPick);

            Label joystickBtn = new Label();
            joystickBtn.Name = "joyHdrLblaxisname";
            joystickBtn.Content = "Axis/Btn Name";
            joystickBtn.Foreground = Brushes.White;
            joystickBtn.HorizontalAlignment = HorizontalAlignment.Center;
            joystickBtn.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(joystickBtn, 4);
            grid.Children.Add(joystickBtn);

            Label joystickAxisS = new Label();
            joystickAxisS.Content = "Axis Setting";
            joystickAxisS.Foreground = Brushes.White;
            joystickAxisS.HorizontalAlignment = HorizontalAlignment.Center;
            joystickAxisS.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(joystickAxisS, 5);
            grid.Children.Add(joystickAxisS);

            Label dzlbl = new Label();
            dzlbl.Content = "Mod1/Deadzone";
            dzlbl.Foreground = Brushes.White;
            dzlbl.HorizontalAlignment = HorizontalAlignment.Center;
            dzlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(dzlbl, 7);
            grid.Children.Add(dzlbl);

            Label sxlbl = new Label();
            sxlbl.Content = "Mod2/Sat X";
            sxlbl.Foreground = Brushes.White;
            sxlbl.HorizontalAlignment = HorizontalAlignment.Center;
            sxlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(sxlbl, 8);
            grid.Children.Add(sxlbl);

            Label sylbl = new Label();
            sylbl.Content = "Mod3/Sat Y";
            sylbl.Foreground = Brushes.White;
            sylbl.HorizontalAlignment = HorizontalAlignment.Center;
            sylbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(sylbl, 9);
            grid.Children.Add(sylbl);

            Label curvlbl = new Label();
            curvlbl.Content = "Mod4/Curv";
            curvlbl.Foreground = Brushes.White;
            curvlbl.HorizontalAlignment = HorizontalAlignment.Center;
            curvlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(curvlbl, 10);
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
            if (cx.Text.Length < 1||cx.Text.Replace(" ","")==".") return;
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
            savepos();
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
                if (cr.Curviture.Count > 0) cr.Curviture[0] = curv;
                else cr.Curviture.Add(curv);
            }
            savepos();
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
            savepos();
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
            if (MainStructure.msave != null&&MainStructure.msave.mainWLast.Width>0)
            {
                this.Top = MainStructure.msave.mainWLast.Top;
                this.Left = MainStructure.msave.mainWLast.Left;
                this.Width = MainStructure.msave.mainWLast.Width;
                this.Height = MainStructure.msave.mainWLast.Height;
                Console.WriteLine("Done set");
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
                }
            if(modBtns!=null)
            foreach (KeyValuePair<int, Button[]> kvp in modBtns)
            {
                for (int i = 0; i < kvp.Value.Length; ++i)
                {
                    if (kvp.Value[i] != null)
                    {
                        kvp.Value[i].IsEnabled = true;
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
                }
            if(modBtns!=null)
            foreach (KeyValuePair<int, Button[]> kvp in modBtns)
            {
                for (int i = 0; i < kvp.Value.Length; ++i)
                {
                    if (kvp.Value[i] != null)
                    {
                        kvp.Value[i].IsEnabled = false;
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
                MainStructure.msave.lastInstanceSelected= (string)DropDownInstanceSelection.SelectedItem;
            }
        }
        void SaveMeta(object sender, EventArgs e)
        {
            savepos();
        }

        void savepos()
        {
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            Console.WriteLine(MainStructure.GetWindowPosFrom(this).ToString());
            MainStructure.msave.mainWLast = MainStructure.GetWindowPosFrom(this);
            MainStructure.SaveMetaLast();
        }
    }
}
