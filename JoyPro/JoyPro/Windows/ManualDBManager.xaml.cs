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
    /// Interaktionslogik für ManualDBManager.xaml
    /// </summary>
    public partial class ManualDBManager : Window
    {
        List<DCSInput> renderedInputsDCS;
        List<OtherGameInput> renderedInputsOG;
        ScrollViewer old = null;
        public ManualDBManager()
        {
            InitializeComponent();
            renderedInputsDCS = new List<DCSInput>();
            renderedInputsOG = new List<OtherGameInput>();
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            SetupGameList();
            GamesDropDown.SelectionChanged += new SelectionChangedEventHandler(SetupPlaneList);
            RefreshManualEntries();
            AddBtn.Click += new RoutedEventHandler(AddItem);
        }

        void SetupPlaneList(object sender, EventArgs e)
        {
            PlaneDropDown.Items.Clear();
            string selectedGame = (string)GamesDropDown.SelectedItem;
            if (selectedGame != null && DBLogic.Planes.ContainsKey(selectedGame))
            {
                for (int i = 0; i < DBLogic.Planes[selectedGame].Count; ++i)
                    PlaneDropDown.Items.Add(DBLogic.Planes[selectedGame][i]);
            }
            PlaneDropDown.Items.Add("ALL");
        }

        void SetupGameList()
        {
            GamesDropDown.Items.Clear();
            GamesDropDown.Items.Add("DCS");
            foreach (KeyValuePair<string, Dictionary<string, OtherGame>> kvp in DBLogic.OtherLib)
                GamesDropDown.Items.Add(kvp.Key);
        }
        Grid setupBaseGrid(int rowsNeeded)
        {
            ScrollViewer sv = new ScrollViewer();
            sv.Name = "InnerSV";
            Grid.SetColumn(sv, 0);
            Grid.SetRow(sv, 2);
            if (old != null)
            {
                MainGrid.Children.Remove(old);
            }
            MainGrid.Children.Add(sv);
            old = sv;
            sv.SetValue(Grid.ColumnSpanProperty, 7);
            Grid grid = new Grid();
            int columnsNeeded = 7;
            for(int i=0; i<rowsNeeded; ++i)
            grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < columnsNeeded; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                grid.ColumnDefinitions.Add(c);
            }
            grid.ShowGridLines = true;
            sv.Content = grid;
            return grid;
        }
        void RefreshManualEntries()
        {
            
            
            renderedInputsDCS = new List<DCSInput>();
            renderedInputsOG = new List<OtherGameInput>();
            if (DBLogic.ManualDatabase == null) DBLogic.ManualDatabase = new ManualDatabaseAdditions();
            int rows = 0;
            foreach(KeyValuePair<string, DCSPlane> kvp in DBLogic.ManualDatabase.DCSLib)
            {
                foreach(KeyValuePair<string, DCSInput> kvpAx in kvp.Value.Axis)
                {
                    rows++;
                }
                foreach(KeyValuePair<string, DCSInput> kvpBn in kvp.Value.Buttons)
                {
                    rows++;
                }
            }
            foreach(KeyValuePair<string, Dictionary<string, OtherGame>> kvp in DBLogic.ManualDatabase.OtherLib)
            {
                foreach(KeyValuePair<string, OtherGame> kvpInner in kvp.Value)
                {
                    foreach(KeyValuePair<string, OtherGameInput> kvAx in kvpInner.Value.Axis)
                    {
                        rows++;
                    }
                    foreach(KeyValuePair<string, OtherGameInput> kvBn in kvpInner.Value.Buttons)
                    {
                        rows++;
                    }
                }
            }
            Grid g = setupBaseGrid(rows);
            int rowInput = 0;
            foreach (KeyValuePair<string, DCSPlane> kvp in DBLogic.ManualDatabase.DCSLib)
            {
                foreach (KeyValuePair<string, DCSInput> kvpAx in kvp.Value.Axis)
                {
                    renderedInputsDCS.Add(kvpAx.Value);
                    createRow(rowInput, rowInput, kvpAx.Value.ID, "DCS", kvpAx.Value.Plane, kvpAx.Value.Title, true, false, g,"");
                    rowInput++;
                }
                foreach (KeyValuePair<string, DCSInput> kvpBn in kvp.Value.Buttons)
                {
                    renderedInputsDCS.Add(kvpBn.Value);
                    createRow(rowInput, rowInput, kvpBn.Value.ID, "DCS", kvpBn.Value.Plane, kvpBn.Value.Title, false, false, g,"");
                    rowInput++;
                }
            }
            int otherGameIndex = 0;
            foreach (KeyValuePair<string, Dictionary<string, OtherGame>> kvp in DBLogic.ManualDatabase.OtherLib)
            {
                foreach (KeyValuePair<string, OtherGame> kvpInner in kvp.Value)
                {
                    foreach (KeyValuePair<string, OtherGameInput> kvAx in kvpInner.Value.Axis)
                    {                       
                        renderedInputsOG.Add(kvAx.Value);
                        createRow(rowInput, otherGameIndex, kvAx.Value.ID, kvp.Key, kvAx.Value.Plane, kvAx.Value.Title, true,true, g, kvAx.Value.Category);
                        otherGameIndex++;
                        rowInput++;
                    }
                    foreach (KeyValuePair<string, OtherGameInput> kvBn in kvpInner.Value.Buttons)
                    {
                        renderedInputsOG.Add(kvBn.Value);
                        createRow(rowInput, otherGameIndex, kvBn.Value.ID, kvp.Key, kvBn.Value.Plane, kvBn.Value.Title, false, true, g, kvBn.Value.Category);
                        otherGameIndex++;
                        rowInput++;
                    }
                }
            }
            g.ShowGridLines = true;

        }

        void createRow(int rowInput, int listIndex, string id, string game, string plane, string description, bool axis, bool othergame, Grid g, string cat)
        {
            string itemName;
            if (othergame)
                itemName = "o";
            else
                itemName = "d";
            itemName += listIndex.ToString();
            createLabel(itemName + "id", id, 0, rowInput,g);
            createLabel(itemName + "cat", cat, 1, rowInput, g);
            createLabel(itemName + "plane", plane, 2, rowInput,g);
            createLabel(itemName + "desc", description, 3, rowInput, g);
            createLabel(itemName + "ax", axis.ToString(), 4, rowInput, g);
            createLabel(itemName + "game", game, 5, rowInput, g);
            Button Btn = new Button();
            Btn.Name = itemName+"dlt";
            Btn.Content = "Delete";
            Btn.HorizontalAlignment = HorizontalAlignment.Center;
            Btn.VerticalAlignment = VerticalAlignment.Center;
            Btn.Width = 70;
            Btn.Click += new RoutedEventHandler(DeleteItem);
            Grid.SetColumn(Btn, 6);
            Grid.SetRow(Btn, rowInput);
            g.Children.Add(Btn);
        }
        void AddItem(object sender, EventArgs e)
        {
            if(IDTF.Text.Length<1||
                DescriptionTF.Text.Length<1||
                GamesDropDown.SelectedItem==null||
                (GamesDropDown.SelectedItem!=null&&((string)GamesDropDown.SelectedItem).Length<1)||
                PlaneDropDown.SelectedItem==null||
                (PlaneDropDown.SelectedItem != null && ((string)PlaneDropDown.SelectedItem).Length < 1))
            {
                MessageBox.Show("Input incorrect. Please make sure to maker proper selections");
                return;
            }
            bool ax = AxisCB.IsChecked == true ? true : false;
            string plane = (string)PlaneDropDown.SelectedItem;
            string gmae = (string)GamesDropDown.SelectedItem;
            DBLogic.AddItemToManualDB(ax, plane, gmae, IDTF.Text, DescriptionTF.Text, CatTF.Text);
            RefreshManualEntries();
        }
        void DeleteItem(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            int listIndex = Convert.ToInt32(b.Name.Replace("dlt", "").Substring(1));
            if (b.Name.Substring(0, 1) == "d")
            {
                DCSInput toDelete = renderedInputsDCS[listIndex];
                if (DBLogic.ManualDatabase.DCSLib.ContainsKey(toDelete.Plane))
                {
                    if (toDelete.IsAxis)
                    {
                        if (DBLogic.ManualDatabase.DCSLib[toDelete.Plane].Axis.ContainsKey(toDelete.ID))
                            DBLogic.ManualDatabase.DCSLib[toDelete.Plane].Axis.Remove(toDelete.ID);
                    }
                    else
                    {
                        if (DBLogic.ManualDatabase.DCSLib[toDelete.Plane].Buttons.ContainsKey(toDelete.ID))
                            DBLogic.ManualDatabase.DCSLib[toDelete.Plane].Buttons.Remove(toDelete.ID);
                    }
                }
                    
            }
            else
            {
                OtherGameInput toDelete = renderedInputsOG[listIndex];
                if (DBLogic.ManualDatabase.OtherLib.ContainsKey(toDelete.Game)&& DBLogic.ManualDatabase.OtherLib[toDelete.Game].ContainsKey(toDelete.Plane))
                {
                    if (toDelete.IsAxis)
                    {
                        if (DBLogic.ManualDatabase.OtherLib[toDelete.Game][toDelete.Plane].Axis.ContainsKey(toDelete.ID))
                            DBLogic.ManualDatabase.OtherLib[toDelete.Game][toDelete.Plane].Axis.Remove(toDelete.ID);
                    }
                    else
                    {
                        if (DBLogic.ManualDatabase.OtherLib[toDelete.Game][toDelete.Plane].Buttons.ContainsKey(toDelete.ID))
                            DBLogic.ManualDatabase.OtherLib[toDelete.Game][toDelete.Plane].Buttons.Remove(toDelete.ID);
                    }
                }
            }
            RefreshManualEntries();
        }
        void createLabel(string name, string content, int col, int row, Grid g)
        {
            Label lbl = new Label();
            lbl.Name = name;
            lbl.Foreground = Brushes.White;
            lbl.Content = content;
            lbl.HorizontalAlignment = HorizontalAlignment.Center;
            lbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(lbl, col);
            Grid.SetRow(lbl, row);
            g.Children.Add(lbl);
        }
        void CloseThis(object sender, EventArgs e)
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            DBLogic.ManualDatabase.WriteToTextFile(pth + "\\ManualAdditions.txt");
            InitGames.ReloadGameData();
            InitGames.ReloadDatabase();
            MainStructure.SaveMetaLast();
            Close();
        }
    }
}
