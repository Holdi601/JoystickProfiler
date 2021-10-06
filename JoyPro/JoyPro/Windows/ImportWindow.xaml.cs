using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        public List<string> selectedSticks;
        public string[] availableJoysticks;
        public List<string> connectedSticks;
        List<CheckBox> joystickCheckboxes;
        //check for null
        public ImportWindow()
        {
            selectedSticks = new List<string>();
            availableJoysticks = InternalDataMangement.LocalJoysticks;
            connectedSticks = JoystickReader.GetConnectedJoysticks();
            for (int i = 0; i < connectedSticks.Count; ++i)
                connectedSticks[i] = connectedSticks[i].ToLower();
            InitializeComponent();
            CancelBtn.Click += new RoutedEventHandler(CancelImport);
            ImportBtn.Click += new RoutedEventHandler(Import);
            ListSticks();
            if (MainStructure.msave != null&&MainStructure.msave.importWindowLast.Height>0 && MainStructure.msave.importWindowLast.Width > 0)
            {
                this.Top = MainStructure.msave.importWindowLast.Top;
                this.Left = MainStructure.msave.importWindowLast.Left;
                this.Width = MainStructure.msave.importWindowLast.Width;
                this.Height = MainStructure.msave.importWindowLast.Height;
            }
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            SetupGamesDropDown();
            CBselAll.Click += new RoutedEventHandler(SelectAll);
            CBselNone.Click += new RoutedEventHandler(SelectNone);

        }
        void CancelImport(object sender, EventArgs e)
        {
            MainStructure.msave.importWindowLast = MainStructure.GetWindowPosFrom(this);
            MainStructure.SaveMetaLast();
            Close();
        }
        void SelectNone(object sender, EventArgs e)
        {
            if (CBselNone.IsChecked == true)
            {
                CBselNone.IsChecked = false;
                CBinv.IsChecked = false;
                CBcurv.IsChecked = false;
                CBdz.IsChecked = false;
                CBimportDefault.IsChecked = false;
                CBsatx.IsChecked = false;
                CBsaty.IsChecked = false;
                CBslid.IsChecked = false;
            }
        }
        void SelectAll(object sender, EventArgs e)
        {
            if (CBselAll.IsChecked == true)
            {
                CBselAll.IsChecked = false;
                CBinv.IsChecked = true;
                CBcurv.IsChecked = true;
                CBdz.IsChecked = true;
                CBimportDefault.IsChecked = true;
                CBsatx.IsChecked = true;
                CBsaty.IsChecked = true;
                CBslid.IsChecked = true;
            }
        }
        void Import(object sender, EventArgs e)
        {
            MainStructure.msave.importWindowLast = MainStructure.GetWindowPosFrom(this);
            MainStructure.SaveMetaLast();
            bool inv, slid, curv, dz, sx, sy, importDefault;
            if (CBinv.IsChecked == true)
                inv = true;
            else
                inv = false;
            if (CBslid.IsChecked == true)
                slid = true;
            else
                slid = false;
            if (CBcurv.IsChecked == true)
                curv = true;
            else
                curv = false;
            if (CBdz.IsChecked == true)
                dz = true;
            else
                dz = false;
            if (CBsatx.IsChecked == true)
                sx = true;
            else
                sx = false;
            if (CBsaty.IsChecked == true)
                sy = true;
            else
                sy = false;
            if (CBimportDefault.IsChecked == true)
                importDefault = true;
            else
                importDefault = false;
            if(InternalDataMangement.GamesFilter["DCS"])
                DCSIOLogic.BindsFromLocal(selectedSticks ,importDefault, inv, slid, curv, dz, sx, sy);
            if (InternalDataMangement.GamesFilter["IL2Game"])
                IL2IOLogic.ImportInputs(curv, dz);
            
            Close();
        }
        Grid BaseSetupRelationGrid()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            if (availableJoysticks == null) return null;

            for (int i = 0; i < availableJoysticks.Length+2; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());

            return grid;
        }
        void ListSticks()
        {
            Grid grid = BaseSetupRelationGrid();
            if (grid == null) return;
            if (InternalDataMangement.JoystickAliases == null) InternalDataMangement.JoystickAliases = new Dictionary<string, string>();
            CheckBox cbxAll = new CheckBox();
            cbxAll.Name = "cbxjyAll";
            cbxAll.Content = "ALL";
            cbxAll.HorizontalAlignment = HorizontalAlignment.Left;
            cbxAll.VerticalAlignment = VerticalAlignment.Center;
            Thickness marginc = cbxAll.Margin;
            marginc.Left = 10;
            cbxAll.Foreground = Brushes.LightBlue;
            cbxAll.Margin = marginc;
            Grid.SetColumn(cbxAll, 0);
            Grid.SetRow(cbxAll, 0);
            grid.Children.Add(cbxAll);
            cbxAll.Click += new RoutedEventHandler(JoystickSetChanged);

            CheckBox cbxNone = new CheckBox();
            cbxNone.Name = "cbxjyNone";
            cbxNone.Content = "NONE";
            cbxNone.HorizontalAlignment = HorizontalAlignment.Left;
            cbxNone.VerticalAlignment = VerticalAlignment.Center;
            Thickness marginn = cbxNone.Margin;
            cbxNone.Foreground = Brushes.LightBlue;
            marginn.Left = 10;
            cbxNone.Margin = marginn;
            Grid.SetColumn(cbxNone, 0);
            Grid.SetRow(cbxNone, 1);
            grid.Children.Add(cbxNone);
            cbxNone.Click += new RoutedEventHandler(JoystickSetChanged);

            joystickCheckboxes = new List<CheckBox>();

            for (int i=0; i<availableJoysticks.Length; ++i)
            {
                CheckBox cbx = new CheckBox();
                cbx.Name = "cbxjy" + i.ToString();
                if (InternalDataMangement.JoystickAliases.ContainsKey(availableJoysticks[i]) && InternalDataMangement.JoystickAliases[availableJoysticks[i]].Length > 0)
                {
                    cbx.Content = InternalDataMangement.JoystickAliases[availableJoysticks[i]];
                }
                else
                {
                    cbx.Content = availableJoysticks[i];
                }        
                if (connectedSticks.Contains(availableJoysticks[i].ToLower()))
                {
                    cbx.Foreground = Brushes.GreenYellow;
                }
                else
                {
                    cbx.Foreground = Brushes.White;
                }
                cbx.HorizontalAlignment = HorizontalAlignment.Left;
                cbx.VerticalAlignment = VerticalAlignment.Center;
                Thickness margin = cbx.Margin;
                margin.Left = 10;
                cbx.Margin = margin;
                Grid.SetColumn(cbx, 0);
                Grid.SetRow(cbx, i+2);
                grid.Children.Add(cbx);
                cbx.Click += new RoutedEventHandler(JoystickSetChanged);
                joystickCheckboxes.Add(cbx);
            }
            sv.Content = grid;
        }
        private void SetupGamesDropDown()
        {
            GamesFilterDropDown.Items.Clear();
            if (InternalDataMangement.GamesFilter == null || InternalDataMangement.GamesFilter.Count == 0)
            {
                for (int i = 0; i < MiscGames.Games.Count; ++i)
                {
                    InternalDataMangement.GamesFilter.Add(MiscGames.Games[i], true);
                }
            }
            foreach (KeyValuePair<string, bool> kvp in InternalDataMangement.GamesFilter)
            {
                CheckBox cbx = new CheckBox();
                cbx.Name = kvp.Key + "game";
                cbx.Content = kvp.Key;
                cbx.Foreground = Brushes.Black;
                cbx.IsChecked = kvp.Value;
                cbx.HorizontalAlignment = HorizontalAlignment.Left;
                cbx.VerticalAlignment = VerticalAlignment.Center;
                cbx.Click += new RoutedEventHandler(gameFilterChanged);
                GamesFilterDropDown.Items.Add(cbx);
            }
        }
        private void gameFilterChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.IsChecked == true)
            {
                InternalDataMangement.GamesFilter[(string)cb.Content] = true;
            }
            else
            {
                InternalDataMangement.GamesFilter[(string)cb.Content] = false;
            }
        }
        void JoystickSetChanged(object sender, EventArgs e)
        {
            MainStructure.msave.importWindowLast = MainStructure.GetWindowPosFrom(this);
            MainStructure.SaveMetaLast();
            CheckBox cx = (CheckBox)sender;
            if ((string)cx.Content == "ALL")
            {
                selectedSticks.Clear();
                for(int i=0; i<joystickCheckboxes.Count; ++i)
                {
                    joystickCheckboxes[i].IsChecked = true;
                    selectedSticks.Add(availableJoysticks[i]);
                }
            }
            else if ((string)cx.Content=="NONE")
            {
                for (int i = 0; i < joystickCheckboxes.Count; ++i)
                {
                    joystickCheckboxes[i].IsChecked = false;
                }
                selectedSticks.Clear();
            }
            else
            {
                int indx = Convert.ToInt32(cx.Name.Replace("cbxjy", ""));
                if (indx < 0 || indx >= availableJoysticks.Length) return;
                string stick = availableJoysticks[indx];
                if (cx.IsChecked == true && !selectedSticks.Contains(stick))
                {
                    selectedSticks.Add(stick);
                }
                else if (cx.IsChecked == false && selectedSticks.Contains(stick))
                {
                    selectedSticks.Remove(stick);
                }
            }
            
        }

    }
}
