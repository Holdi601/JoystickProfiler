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
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;

        //check for null
        public ImportWindow()
        {
            selectedSticks = new List<string>();
            availableJoysticks = InternalDataManagement.LocalJoysticks;
            List<string> localSticks = availableJoysticks.ToList();
            MiscGames.GetDCSInputJoysticks(localSticks);
            availableJoysticks = localSticks.ToArray();
            Dictionary<string, string> cSticks = JoystickReader.GetConnectedJoysticks();
            connectedSticks = new List<string>();
            foreach (KeyValuePair<string, string> kvp in cSticks)
            {
                connectedSticks.Add(kvp.Key);
                if(!localSticks.Contains(kvp.Key))localSticks.Add(kvp.Key);
            }
            InternalDataManagement.LocalJoysticks = localSticks.ToArray();
            availableJoysticks=localSticks.ToArray();
            for (int i = 0; i < connectedSticks.Count; ++i)
                connectedSticks[i] = connectedSticks[i].ToLower();
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;

            CancelBtn.Click += new RoutedEventHandler(CancelImport);
            ImportBtn.Click += new RoutedEventHandler(Import);
            ListSticks();
            if (MainStructure.msave != null&&MainStructure.msave._ImportWindow.Height>0 && MainStructure.msave._ImportWindow.Width > 0)
            {
                this.Top = MainStructure.msave._ImportWindow.Top;
                this.Left = MainStructure.msave._ImportWindow.Left;
                this.Width = MainStructure.msave._ImportWindow.Width;
                this.Height = MainStructure.msave._ImportWindow.Height;
            }
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            SetupGamesDropDown();
            CBselAll.Click += new RoutedEventHandler(SelectAll);
            CBselNone.Click += new RoutedEventHandler(SelectNone);

        }
        void CancelImport(object sender, EventArgs e)
        {
            MainStructure.msave._ImportWindow = MainStructure.GetWindowPosFrom(this);
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
        List<string> getActivePlanes()
        {
            List<string> result = new List<string>();
            for(int i = 0; i < GamesFilterDropDown.Items.Count; i++)
            {
                CheckBox element = (CheckBox)GamesFilterDropDown.Items[i];
                string content = (string)element.Content;
                if (!(content == "ALL" || content == "NONE" || content.Contains(":ALL") || content.Contains(":NONE")))
                {
                    string game = content.Substring(0, content.IndexOf(":"));
                    string plane = content.Substring(content.IndexOf(":")+1);
                    bool state = element.IsChecked == true?true:false;
                    MainStructure.msave.PlaneSetLastActivity(PlaneActivitySelection.Import, game, plane, state);
                    if(element.IsChecked == true)
                    {
                        result.Add(content);
                    }
                }
            }
            return result;
        }
        Dictionary<string, List<string>> ActiveGamePlaneDict(List<string> rawList)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            for(int i = 0;i < rawList.Count; i++)
            {
                string[] parts = rawList[i].Split(':');
                if(!result.ContainsKey(parts[0]))result.Add(parts[0], new List<string>());
                result[parts[0]].Add(parts[1]);
            }
            return result;
        }
        void Import(object sender, EventArgs e)
        {
            MainStructure.msave._ImportWindow = MainStructure.GetWindowPosFrom(this);
            MainStructure.SaveMetaLast();
            var list = ActiveGamePlaneDict(getActivePlanes());
            
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
            if(list.ContainsKey("DCS"))
                DCSIOLogic.BindsFromLocal(selectedSticks, list["DCS"] ,importDefault, inv, slid, curv, dz, sx, sy);
            if (list.ContainsKey("IL2Game"))
                IL2IOLogic.ImportInputs(curv, dz, inv);
            
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
            if (InternalDataManagement.JoystickAliases == null) InternalDataManagement.JoystickAliases = new Dictionary<string, string>();
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
                if (InternalDataManagement.JoystickAliases.ContainsKey(availableJoysticks[i]) && InternalDataManagement.JoystickAliases[availableJoysticks[i]].Length > 0)
                {
                    cbx.Content = InternalDataManagement.JoystickAliases[availableJoysticks[i]];
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
            CheckBox cbpAll = new CheckBox();
            cbpAll.Name = "ALL";
            cbpAll.Content = "ALL";
            cbpAll.IsChecked = false;
            cbpAll.Click += new RoutedEventHandler(PlaneFilterChanged);
            GamesFilterDropDown.Items.Add(cbpAll);

            CheckBox cbpNone = new CheckBox();
            cbpNone.Name = "NONE";
            cbpNone.Content = "NONE";
            cbpNone.IsChecked = false;
            cbpNone.Click += new RoutedEventHandler(PlaneFilterChanged);
            GamesFilterDropDown.Items.Add(cbpNone);

            for (int i = 0; i < DBLogic.Planes.Count; ++i)
            {
                //REmove for later SC implementation                
                if (DBLogic.Planes.ElementAt(i).Key.ToLower().Contains("starcitizen")) continue;

                CheckBox cbgpAll = new CheckBox();
                cbgpAll.Name = "ALL";
                cbgpAll.Content = DBLogic.Planes.ElementAt(i).Key + ":" + "ALL";
                cbgpAll.IsChecked = false;
                cbgpAll.Click += new RoutedEventHandler(PlaneFilterChanged);
                GamesFilterDropDown.Items.Add(cbgpAll);

                CheckBox cbgpNone = new CheckBox();
                cbgpNone.Name = "NONE";
                cbgpNone.Content = DBLogic.Planes.ElementAt(i).Key + ":" + "NONE";
                cbgpNone.IsChecked = false;
                cbgpNone.Click += new RoutedEventHandler(PlaneFilterChanged);
                GamesFilterDropDown.Items.Add(cbgpNone);
            }

            for (int i = 0; i < DBLogic.Planes.Count; ++i)
            {
                //REmove for later SC implementation                
                if (DBLogic.Planes.ElementAt(i).Key.ToLower().Contains("starcitizen")) continue;

                for (int j = 0; j < DBLogic.Planes.ElementAt(i).Value.Count; ++j)
                {
                    CheckBox pln = new CheckBox();
                    pln.Name = "plane";
                    string k = DBLogic.Planes.ElementAt(i).Key + ":" + DBLogic.Planes.ElementAt(i).Value[j];
                    pln.Content = k;
                    bool? toCheck = MainStructure.msave.PlaneWasActiveLastTime(PlaneActivitySelection.Import, DBLogic.Planes.ElementAt(i).Key, DBLogic.Planes.ElementAt(i).Value[j]);
                    if (toCheck == null) pln.IsChecked = true;
                    else pln.IsChecked = toCheck;
                    pln.Click += new RoutedEventHandler(PlaneFilterChanged);
                    GamesFilterDropDown.Items.Add(pln);
                }
            }
        }

        private void PlaneFilterChanged(object sender, RoutedEventArgs e)
        {
            CheckBox sndr = (CheckBox)sender;
            if ((string)sndr.Content == "ALL")
            {
                for(int i=0; i< GamesFilterDropDown.Items.Count; ++i)
                {
                    CheckBox element = (CheckBox)GamesFilterDropDown.Items[i];
                    string cnt = (string)element.Content;
                    if (cnt == "ALL" || cnt == "NONE" || cnt.Contains(":ALL") || cnt.Contains(":NONE"))
                    {
                        element.IsChecked = false;
                    }
                    else
                    {
                        element.IsChecked = true;
                    }
                }
            }
            else if ((string)sndr.Content == "NONE")
            {
                for (int i = 0; i < GamesFilterDropDown.Items.Count; ++i)
                {
                    CheckBox element = (CheckBox)GamesFilterDropDown.Items[i];
                    string cnt = (string)element.Content;
                    if (cnt == "ALL" || cnt == "NONE" || cnt.Contains(":ALL") || cnt.Contains(":NONE"))
                    {
                        element.IsChecked = false;
                    }
                    else
                    {
                        element.IsChecked = false;
                    }
                }
            }
            else if (((string)sndr.Content).Contains(":ALL"))
            {
                string game = ((string)sndr.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                for (int i = 0; i < GamesFilterDropDown.Items.Count; ++i)
                {
                    CheckBox element = (CheckBox)GamesFilterDropDown.Items[i];
                    string cnt = (string)element.Content;
                    if (cnt == "ALL" || cnt == "NONE" || cnt.Contains(":ALL") || cnt.Contains(":NONE"))
                    {
                        element.IsChecked = false;
                    }
                    else
                    {
                        string elementGame =((string)element.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                        if(elementGame==game) element.IsChecked = true;
                    }
                }
            }
            else if (((string)sndr.Content).Contains(":NONE"))
            {
                string game = ((string)sndr.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                for (int i = 0; i < GamesFilterDropDown.Items.Count; ++i)
                {
                    CheckBox element = (CheckBox)GamesFilterDropDown.Items[i];
                    string cnt = (string)element.Content;
                    if (cnt == "ALL" || cnt == "NONE" || cnt.Contains(":ALL") || cnt.Contains(":NONE"))
                    {
                        element.IsChecked = false;
                    }
                    else
                    {
                        string elementGame = ((string)element.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                        if (elementGame == game) element.IsChecked = false;
                    }
                }
            }
            else
            {

            }
        }


        void JoystickSetChanged(object sender, EventArgs e)
        {
            MainStructure.msave._ImportWindow = MainStructure.GetWindowPosFrom(this);
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
