using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace JoyPro
{
    /// <summary>
    /// Interaktionslogik für ModifierManager.xaml
    /// </summary>
    public partial class ModifierManager : Window
    {
        List<ColumnDefinition> colDefs=null;
        List<ColumnDefinition> colHds = null;
        List<Modifier> CURRENTDISPLAYEDMODS;
        Button[] dltBtns;
        Button addBtn;
        JoystickReader jr;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;

        public ModifierManager()
        {
            
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;

            UpdateView();
            if (MainStructure.msave._ModifierWindow != null)
            {
                if (MainStructure.msave._ModifierWindow.Top != -1) this.Top = MainStructure.msave._ModifierWindow.Top;
                if (MainStructure.msave._ModifierWindow.Left != -1) this.Left = MainStructure.msave._ModifierWindow.Left;
                if (MainStructure.msave._ModifierWindow.Width != -1) this.Width = MainStructure.msave._ModifierWindow.Width;
                if (MainStructure.msave._ModifierWindow.Height != -1) this.Height = MainStructure.msave._ModifierWindow.Height;
            }

            svC.ScrollChanged += new ScrollChangedEventHandler(syncScrolls);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            OkBtn.Click += new RoutedEventHandler(CloseThis);
            this.Loaded += new RoutedEventHandler(syncScrolls);
        }



        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }

        void UpdateView()
        {
            CURRENTDISPLAYEDMODS = InternalDataManagement.GetAllModifiers();
            SetModsToView();
            sizeChanged(null, null);
        }

        Grid BaseSetupModifiersGrid()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            colDefs = new List<ColumnDefinition>();
            for (int i = 0; i < 5; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                grid.ColumnDefinitions.Add(c);
                colDefs.Add(c);
            }
            for (int i = 0; i < CURRENTDISPLAYEDMODS.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());
            dltBtns = new Button[CURRENTDISPLAYEDMODS.Count];
            return grid;
        }

        void syncScrolls(object sender, EventArgs e)
        {
            svHead.ScrollToHorizontalOffset(svC.HorizontalOffset);
            if (colHds != null && colHds.Count > 0)
            {
                for(int i=0; i<colHds.Count; ++i)
                {
                    colHds[i].MinWidth = colDefs[i].ActualWidth;
                }
            }
        }

        void sizeChanged(object sender, EventArgs e)
        {
            if (colDefs != null && colHds != null)
            {
                for (int i = 0; i < colDefs.Count; ++i)
                {
                    colHds[i].MinWidth = colDefs[i].ActualWidth;
                }
                svHead.ScrollToHorizontalOffset(svC.HorizontalOffset);
            }
        }

        public void SetModsToView()
        {
            RefreshRelationsToShow();
            SetHeadersForScrollView();
        }

        void DeleteModBtn(object sender, EventArgs e)
        {
            Button pressed = (Button)sender;
            int indx = Convert.ToInt32(pressed.Name.Replace("deleteBtn", ""));
            InternalDataManagement.RemoveReformer(CURRENTDISPLAYEDMODS[indx].name);
            UpdateView();
        }

        void ChangedName(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int indx = Convert.ToInt32(tb.Name.Replace("txname", ""));
            if (tb.Text.Length > 1)
            {
                if (InternalDataManagement.GetReformerStringFromMod(tb.Text) == null)
                    InternalDataManagement.ChangeReformerName(CURRENTDISPLAYEDMODS[indx].name, tb.Text);
                else
                    MessageBox.Show("Key already exists: " + tb.Text);
            }
            else
            {
                MessageBox.Show("The name must be longer than 3 characters.");
                tb.Text = CURRENTDISPLAYEDMODS[indx].name;
            }
            UpdateView();
        }

        void ActivateButtons()
        {
            if (addBtn != null) addBtn.IsEnabled = true;
            if (dltBtns != null)
                for (int i = 0; i < dltBtns.Length; ++i)
                {
                    dltBtns[i].IsEnabled = true;
                }
        }
        void DisableButtons()
        {
            if (addBtn != null) addBtn.IsEnabled = false;
            if (dltBtns!=null)
                for(int i=0; i<dltBtns.Length; ++i)
                {
                    dltBtns[i].IsEnabled = false;
                }
        }

        void AddNewMod(object sender, EventArgs e)
        {
            DisableButtons();
            if (addBtn != null)
                addBtn.Content = "Click now";
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(listenMod);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(modSet);
            bw.RunWorkerAsync();
        }

        void listenMod(object sender, EventArgs e)
        {
            jr = new JoystickReader(false, true);
        }

        void switchSet(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            string numString = cb.Name.Replace("swCb", "");
            int num = Convert.ToInt32(numString);
            CURRENTDISPLAYEDMODS[num].sw = cb.IsChecked ==true ? true : false;
            InternalDataManagement.ChangeSwitchStateBind(CURRENTDISPLAYEDMODS[num].name, cb.IsChecked == true ? true : false);

        }

        void modSet(object sender, EventArgs e)
        {
            ActivateButtons();
            if (jr == null )
            {
                MessageBox.Show("Something went wrong when setting a modifier. Either listener was not started correctly or the main button was not assigend beforehand.");
                return;
            }
            if (jr.result == null)
            {
                UpdateView();
                return;
            }
            string device;
            if (jr.result.Device == "Keyboard")
                device = "Keyboard";
            else
                device = "m" + jr.result.Device.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
            string nameToShow = device + jr.result.AxisButton;
            string moddedDevice = Bind.JoystickGuidToModifierGuid(jr.result.Device);
            string toAdd = nameToShow + "§" + moddedDevice + "§" + jr.result.AxisButton+"§"+false.ToString();
            ModExists existsAlready = InternalDataManagement.DoesReformerExistInMods(toAdd);
            if (existsAlready == ModExists.NOT_EXISTENT)
            {
                InternalDataManagement.AddReformerToMods(toAdd);
            }

            UpdateView();
        }

        void RefreshRelationsToShow()
        {
            Grid grid = BaseSetupModifiersGrid();
            for (int i = 0; i < CURRENTDISPLAYEDMODS.Count; i++)
            {
                TextBox txname = new TextBox();
                txname.Name = "txname" + i.ToString();
                txname.Width = 200;
                txname.Height = 24;
                txname.Text = CURRENTDISPLAYEDMODS[i].name;
                Grid.SetColumn(txname, 0);
                Grid.SetRow(txname, i);
                txname.HorizontalAlignment = HorizontalAlignment.Left;
                txname.VerticalAlignment = VerticalAlignment.Center;
                txname.LostFocus += new RoutedEventHandler(ChangedName);
                grid.Children.Add(txname);

                Button deleteBtn = new Button();
                dltBtns[i] = deleteBtn;
                deleteBtn.Name = "deleteBtn" + i.ToString();
                deleteBtn.Content = "Delete Modifier";
                deleteBtn.Click += new RoutedEventHandler(DeleteModBtn);
                deleteBtn.HorizontalAlignment = HorizontalAlignment.Left;
                deleteBtn.VerticalAlignment = VerticalAlignment.Center;
                deleteBtn.Width = 100;
                Grid.SetColumn(deleteBtn, 1);
                Grid.SetRow(deleteBtn, i);
                grid.Children.Add(deleteBtn);

                Label joystickPick = new Label();
                joystickPick.Name = "joyLbl" + i.ToString();
                joystickPick.Foreground = Brushes.White;
                joystickPick.Content = CURRENTDISPLAYEDMODS[i].device;
                joystickPick.Width = 500;
                joystickPick.HorizontalAlignment = HorizontalAlignment.Left;
                joystickPick.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(joystickPick, 2);
                Grid.SetRow(joystickPick, i);
                grid.Children.Add(joystickPick);

                Label buttonName = new Label();
                buttonName.Name = "btnLbl" + i.ToString();
                buttonName.Foreground = Brushes.White;
                buttonName.Content = CURRENTDISPLAYEDMODS[i].key;
                buttonName.Width = 100;
                buttonName.HorizontalAlignment = HorizontalAlignment.Left;
                buttonName.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(buttonName, 3);
                Grid.SetRow(buttonName, i);
                grid.Children.Add(buttonName);

                CheckBox cbSwitch = new CheckBox();
                cbSwitch.Name = "swCb" + i.ToString();
                cbSwitch.Click += new RoutedEventHandler(switchSet);
                cbSwitch.Foreground = Brushes.White;
                cbSwitch.Width = 100;
                cbSwitch.HorizontalAlignment = HorizontalAlignment.Left;
                cbSwitch.VerticalAlignment = VerticalAlignment.Center;
                cbSwitch.IsChecked = CURRENTDISPLAYEDMODS[i].sw;
                Grid.SetColumn(cbSwitch, 4);
                Grid.SetRow(cbSwitch, i);
                grid.Children.Add(cbSwitch);
            }
            svC.Content = grid;
        }

        void SetHeadersForScrollView()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            colHds = new List<ColumnDefinition>();
            for (int i = 0; i < 5; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                grid.ColumnDefinitions.Add(c);
                colHds.Add(c);
                c.MinWidth = colDefs[i].ActualWidth;
            }
            Label relPick = new Label();
            relPick.Name = "joyHdrLblRlName";
            relPick.Content = "Mod Name";
            relPick.Foreground = Brushes.White;
            relPick.HorizontalAlignment = HorizontalAlignment.Left;
            relPick.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(relPick, 0);
            grid.Children.Add(relPick);

            addBtn = new Button();
            addBtn.Name = "addBtn";
            addBtn.Content = "Add Modifier";
            addBtn.HorizontalAlignment = HorizontalAlignment.Left;
            addBtn.VerticalAlignment = VerticalAlignment.Center;
            addBtn.Width = 100;
            addBtn.Click += new RoutedEventHandler(AddNewMod);
            Grid.SetColumn(addBtn, 1);
            grid.Children.Add(addBtn);

            Label joystickPick = new Label();
            joystickPick.Name = "joyHdrLbldeviceName";
            joystickPick.Content = "Device Name";
            joystickPick.Foreground = Brushes.White;
            joystickPick.HorizontalAlignment = HorizontalAlignment.Left;
            joystickPick.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(joystickPick, 2);
            grid.Children.Add(joystickPick);

            Label joystickBtn = new Label();
            joystickBtn.Name = "joyHdrLblaxisname";
            joystickBtn.Content = "Button Name";
            joystickBtn.Foreground = Brushes.White;
            joystickBtn.HorizontalAlignment = HorizontalAlignment.Left;
            joystickBtn.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(joystickBtn, 3);
            grid.Children.Add(joystickBtn);

            Label joystickSw = new Label();
            joystickSw.Name = "joyHdrLblsw";
            joystickSw.Content = "Switch";
            joystickSw.Foreground = Brushes.White;
            joystickSw.HorizontalAlignment = HorizontalAlignment.Left;
            joystickSw.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(joystickSw, 4);
            grid.Children.Add(joystickSw);

            svHead.Content = grid;
        }

    }
}
