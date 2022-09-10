using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace JoyPro
{
    /// <summary>
    /// Interaktionslogik für RelationWindow.xaml
    /// </summary>
    public partial class RelationWindow : Window
    {
        public Relation Current = null;
        public Relation Original = null;
        bool editMode;
        List<ColumnDefinition> headerColumns;
        List<ColumnDefinition> headerColumnsIds;
        List<ColumnDefinition> mainColumns;
        List<ColumnDefinition> mainColumnsIds;
        List<RelationItem> ri;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;


        void init()
        {
            this.Deactivated += new EventHandler(Window_Deactivated);
            headerColumns = new List<ColumnDefinition>();
            mainColumns = new List<ColumnDefinition>();
            headerColumnsIds = new List<ColumnDefinition>();
            mainColumnsIds = new List<ColumnDefinition>();
            SearchQueryTF.TextChanged += new TextChangedEventHandler(SearchQueryChanged);
            RelationNameTF.TextChanged += new TextChangedEventHandler(NameChanged);
            AddItemBtn.Click += new RoutedEventHandler(AddItemBtnHit);
            CancelRelationBtn.Click += new RoutedEventHandler(CloseThis);
            FinishRelationBtn.Click += new RoutedEventHandler(FinishRelation);
            this.Closing += new System.ComponentModel.CancelEventHandler(OnClosing);
            
            if (MainStructure.msave._RelationWindow != null && InternalDataManagement.AllRelations.Count>1)
            {
                if (MainStructure.msave._RelationWindow.Top != -1) this.Top = MainStructure.msave._RelationWindow.Top;
                if (MainStructure.msave._RelationWindow.Left != -1) this.Left = MainStructure.msave._RelationWindow.Left;
                if (MainStructure.msave._RelationWindow.Width != -1) this.Width = MainStructure.msave._RelationWindow.Width;
                if (MainStructure.msave._RelationWindow.Height != -1) this.Height = MainStructure.msave._RelationWindow.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }
            if (InternalDataManagement.AllRelations.Count < 2)
            {
                this.WindowState= WindowState.Maximized;
                this.Topmost = true;
            }
            svcCont.ScrollChanged += new ScrollChangedEventHandler(scrollChanged);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            SetupGamesDropDown();
            if (Current == null)
            {
                Current = new Relation();
            }
            else
            {
                editMode = true;
                this.Title = "Edit Relation";
                RefreshDGSelected();
                RelationNameTF.Text = Current.NAME;
            }
            svcCont.CanContentScroll = true;
            scrollChanged(null, null);
        }
        public RelationWindow(Relation r)
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;

            Current = r.Copy();
            Original = r;
            init();
        }
        public RelationWindow()
        {
            InitializeComponent();
            init();
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }
        void setLastSizeAndPosition()
        {
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            MainStructure.msave._RelationWindow.Height = this.Height;
            MainStructure.msave._RelationWindow.Left = this.Left;
            MainStructure.msave._RelationWindow.Top = this.Top;
            MainStructure.msave._RelationWindow.Width = this.Width;
        }
        private void SetupGamesDropDown()
        {
            GamesFilterDropDown.Items.Clear();
            if (InternalDataManagement.GamesFilter == null || InternalDataManagement.GamesFilter.Count == 0)
            {
                for(int i=0; i<MiscGames.Games.Count; ++i)
                {
                    InternalDataManagement.GamesFilter.Add(MiscGames.Games[i], true);
                }
            }
            foreach(KeyValuePair<string, bool> kvp in InternalDataManagement.GamesFilter)
            {
                CheckBox cbx = new CheckBox();
                cbx.Name = kvp.Key+"game";
                cbx.Content = kvp.Key;
                cbx.Foreground = Brushes.Black;
                cbx.IsChecked = kvp.Value;
                cbx.HorizontalAlignment = HorizontalAlignment.Left;
                cbx.VerticalAlignment = VerticalAlignment.Center;
                cbx.Click += new RoutedEventHandler(gameFilterChanged);
                GamesFilterDropDown.Items.Add(cbx);
            }
        }
        void gameFilterChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.IsChecked == true)
            {
                InternalDataManagement.GamesFilter[(string)cb.Content] = true;
            }
            else
            {
                InternalDataManagement.GamesFilter[(string)cb.Content] = false;
            }
            SearchQueryChanged(null, null);
        }
        void scrollChanged(object sender, EventArgs e)
        {
            svcCont.UpdateLayout();
            svcContIds.UpdateLayout();
            svHead.ScrollToHorizontalOffset(svcCont.HorizontalOffset);
            svHeadId.ScrollToHorizontalOffset(svcContIds.HorizontalOffset);
            svcContIds.ScrollToVerticalOffset(svcCont.VerticalOffset);
            for(int i=0; i<headerColumns.Count; ++i)
            {
                headerColumns[i].MinWidth = mainColumns[i].ActualWidth;
            }
            for(int i=0; i<headerColumnsIds.Count; ++i)
            {
                headerColumnsIds[i].MinWidth = mainColumnsIds[i].ActualWidth;
                headerColumnsIds[i].Width = mainColumnsIds[i].Width;
            }

        }
        public void Refresh()
        {
            setLastSizeAndPosition();
            editMode = true;
            RefreshDGSelected();
            RelationNameTF.Text = Current.NAME;
        }
        void FinishRelation(object sender, EventArgs e)
        {
            if (Current.NAME == null || Current.NAME.Length < 1)
            {
                MessageBox.Show("No Relation name set.");
                return;
            }
            if (Current.IsEmpty())
            {
                MessageBox.Show("Relation has no nodes.");
                return;
            }
            if (!editMode&& InternalDataManagement.DoesRelationAlreadyExist(Current.NAME) && !InternalDataManagement.RelationIsTheSame(Current.NAME, Current))
            {
                MessageBox.Show("Relation with same Name already exists.");
                return;
            }
            foreach(string g in MiscGames.Games)
            {
                foreach (KeyValuePair<string, int> kvp in Current.GetPlaneSetState(g))
                {
                    if (kvp.Value > 1)
                    {
                        MessageBox.Show("The Plane " + kvp.Key + " has multiple Bindings in this Relation. Either get completly get rid of binding by unchecking all checkboxes of it or reduce it so that the Aircraft has only one appearance");
                        return;
                    }
                }
            }
            if (!editMode)
            {
                InternalDataManagement.AddRelation(Current);
                MainStructure.Write("Adds new relation " + Current.NAME);
            }
            else
            {
                //Here replace logic
                if (InternalDataManagement.AllRelations.ContainsKey(Current.NAME) && Original.NAME != Current.NAME)
                {
                    MessageBox.Show("RelationName already taken. Please take a new one");
                    return;
                }
                InternalDataManagement.ReplaceRelation(Original, Current);
                MainStructure.Write("Finished Editing Relation " + Current.NAME);
            }
            setLastSizeAndPosition();
            Close();
        }
        void SearchQueryChanged(object sender, EventArgs e)
        {
            string[] searchwords = SearchQueryTF.Text.ToLower().Split(' ');
            DGSource.ItemsSource = DBLogic.SearchBinds(searchwords);
        }
        void NameChanged(object sender, EventArgs e)
        {
            Current.NAME = RelationNameTF.Text;
        }
        void CloseThis(object sender, EventArgs e)
        {
            setLastSizeAndPosition();
            Close();
        }
        void OnClosing(object sender, EventArgs e)
        {
            if (Current != null) Current.RecalculateElementCount();
            if (Original != null) Original.RecalculateElementCount();
            setLastSizeAndPosition();
            MainStructure.OverlayWorker.SetButtonMapping();
        }
        void AddItemBtnHit(object sender, EventArgs e)
        {
            List<SearchQueryResults> selected = DGSource.SelectedItems.Cast<SearchQueryResults>().ToList();
            if (selected.Count < 1)
            {
                MessageBox.Show("No Items Selected");
                return;
            }
            bool axis = selected[0].AXIS;
            string pl = selected[0].AIRCRAFT;
            for (int i = 1; i < selected.Count; i++)
            {
                if (axis != (selected[i].AXIS))
                {
                    MessageBox.Show("Axis and Buttons mixed. One relation cannot have IDs that start with 'a' and 'd' mixed.");
                    return;
                }
            }
            for (int i = 0; i < selected.Count; ++i)
            {
                Current.AddNode(selected[i].ID, selected[i].GAME, selected[i].AXIS, selected[i].AIRCRAFT, true);                
            }       
            RefreshDGSelected();
        }
        Grid createBaseGridHeader()
        {
            headerColumns = new List<ColumnDefinition>();
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            int columnsNeeded =  0;
            foreach(KeyValuePair<string, List<string>> kvp in DBLogic.Planes)
            {
                if (kvp.Value != null) columnsNeeded += kvp.Value.Count;
            }
            grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < columnsNeeded; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                c.Width= (GridLength)converter.ConvertFromString("100");
                grid.ColumnDefinitions.Add(c);
                headerColumns.Add(c);
            }
            return grid;
        }
        Grid createBaseGridHeaderId()
        {
            headerColumnsIds = new List<ColumnDefinition>();
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            int columnsNeeded = 4;
            grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < columnsNeeded; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                c.Width = (GridLength)converter.ConvertFromString("80");
                grid.ColumnDefinitions.Add(c);
                headerColumnsIds.Add(c);
            }
            return grid;
        }
        Grid createMainGrid(int itemrows)
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            int columnsNeeded =  0;
            foreach(KeyValuePair<string, List<string>> kvp in DBLogic.Planes)
            {
                if (kvp.Value != null) columnsNeeded += kvp.Value.Count;
            }
            mainColumns = new List<ColumnDefinition>();
            for(int i=0; i<itemrows; ++i)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            for(int i=0; i<columnsNeeded; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                c.MinWidth = 160;
                grid.ColumnDefinitions.Add(c);
                mainColumns.Add(c);
            }
            
            return grid;
        }
        Grid createMainGridId(int itemrows)
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            int columnsNeeded = 4;
            mainColumnsIds = new List<ColumnDefinition>();
            for (int i = 0; i < itemrows; ++i)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            for (int i = 0; i < columnsNeeded; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                c.MinWidth = 30;
                grid.ColumnDefinitions.Add(c);
                mainColumnsIds.Add(c);
            }

            return grid;
        }
        void createLabelOnGrid(string Name, string Content, int column, int row, Grid g, bool headerPlane)
        {
            Label lbl = new Label();
            lbl.Name = Name;
            lbl.Foreground = Brushes.White;
            lbl.Content = Content;
            lbl.HorizontalAlignment = HorizontalAlignment.Center;
            lbl.VerticalAlignment = VerticalAlignment.Center;
            if(headerPlane)lbl.MouseRightButtonUp += new MouseButtonEventHandler(CreatePlaneAlias);
            Grid.SetColumn(lbl, column);
            Grid.SetRow(lbl, row);
            g.Children.Add(lbl);
        }

        void CreatePlaneAlias(object sender, MouseButtonEventArgs e)
        {
            Label l = (Label)sender;
            int num = Convert.ToInt32((l.Name).Replace("Plane", ""));
            string name = GetPlaneByNumber(num);
            string game = GetGameByNumber(num);
            if (name == null || game == null) return;
            CreatePlaneAlias cpa = new CreatePlaneAlias(name, game);
            cpa.Show();
            cpa.Closing += new System.ComponentModel.CancelEventHandler(RefreshDGSelected);
        }

        void createButtonOnGrid(string Name, string Content, int column, int row, RoutedEventHandler evnt, Grid g)
        {
            Button Btn = new Button();
            Btn.Name = Name;
            Btn.Content = Content;
            Btn.HorizontalAlignment = HorizontalAlignment.Center;
            Btn.VerticalAlignment = VerticalAlignment.Center;
            Btn.Width = 70;
            Btn.Click += evnt;
            Grid.SetColumn(Btn, column);
            Grid.SetRow(Btn, row);
            g.Children.Add(Btn);
        }
        void CreateCheckBoxOnGrid(string Name, string Content, bool isChecked, RoutedEventHandler evnt, int column, int row, Grid g)
        {
            CheckBox cbx = new CheckBox();
            cbx.Name = Name;
            cbx.Content = Content;
            cbx.Foreground = Brushes.White;
            cbx.HorizontalAlignment = HorizontalAlignment.Left;
            cbx.VerticalAlignment = VerticalAlignment.Center;
            cbx.IsChecked = isChecked;
            cbx.Click += evnt;
            Grid.SetColumn(cbx, column);
            Grid.SetRow(cbx, row);
            g.Children.Add(cbx);
        }
        void RefreshDGSelected(object sender, EventArgs e)
        {
            RefreshDGSelected();
        }
        void RefreshDGSelected()
        {
            ri = Current.AllRelations();
            Grid grid = createMainGrid(ri.Count);
            Grid gridId = createMainGridId(ri.Count);
            for (int z=0; z<ri.Count; ++z)
            {
                createLabelOnGrid("id_" + z.ToString(), ri[z].ID, 0, z, gridId, false);
                createButtonOnGrid("deleteBtn" + z.ToString(), "Delete", 1, z, new RoutedEventHandler(deleteItem), gridId);
                createButtonOnGrid("selectAllBtn" + z.ToString(), "Select All", 2, z, new RoutedEventHandler(selectAll), gridId);
                createButtonOnGrid("selectNoneBtn" + z.ToString(), "Select None", 3, z, new RoutedEventHandler(selectNone), gridId);

                int j = 0;
                string game;
                if(ri[z].Game == "DCS" || ri[z].Game == null)
                {
                    game = "DCS";
                }
                else
                {
                    game = ri[z].Game;
                }
                foreach(KeyValuePair<string, List<string>> kvp in DBLogic.Planes)
                {
                    if (kvp.Key == game&&kvp.Value!=null)
                    {
                        foreach (string aircraft in DBLogic.Planes[game])
                        {
                            PlaneState ps = ri[z].GetStateAircraft(aircraft);
                            if (ps == PlaneState.ACTIVE || ps == PlaneState.DISABLED)
                            {
                                bool isChkd;
                                if (ps == PlaneState.ACTIVE)
                                    isChkd = true;
                                else
                                    isChkd = false;
                                CreateCheckBoxOnGrid("p" + j.ToString() + "r" + z.ToString(), ri[z].GetInputDescription(aircraft), isChkd, new RoutedEventHandler(planeActiveStateChanged),j,z,grid);
                            }
                            else
                            {
                                createLabelOnGrid("id_" + z.ToString(), "   ", j, z, grid, false);
                            }
                            ++j;
                        }
                    }
                    else if (kvp.Value == null)
                    {
                        continue;
                    }
                    else
                    {
                        j += kvp.Value.Count;
                    }
                }
            }
            grid.ShowGridLines = true;
            gridId.ShowGridLines = true;
            svcCont.Content = grid;
            svcContIds.Content = gridId;

            Grid headerGrid = createBaseGridHeader();
            Grid headerGridId = createBaseGridHeaderId();

            createLabelOnGrid("headerID", "ID", 0, 0, headerGridId, false);
            createLabelOnGrid("deleteBtns", "Delete", 1, 0, headerGridId, false);
            createLabelOnGrid("selectAllBtns", "Select All", 2, 0, headerGridId, false);
            createLabelOnGrid("selectNoneBtns", "Select None", 3, 0, headerGridId, false);
            int i = 0;
            foreach(KeyValuePair<string, List<string>> kvp in DBLogic.Planes)
            {
                if(kvp.Value!=null)
                    for(int j=0; j<kvp.Value.Count; ++j)
                    {
                        string plane = kvp.Value[j];
                        if (InternalDataManagement.PlaneAliases.ContainsKey(kvp.Key) &&
                            InternalDataManagement.PlaneAliases[kvp.Key].ContainsKey(kvp.Value[j]) &&
                            InternalDataManagement.PlaneAliases[kvp.Key][kvp.Value[j]].Length > 0)
                            plane = InternalDataManagement.PlaneAliases[kvp.Key][kvp.Value[j]];
                        createLabelOnGrid("Plane" + i.ToString(), kvp.Key + ":" + plane, i, 0, headerGrid, true);
                        ++i;
                    }
            }

            headerGrid.ShowGridLines = true;
            headerGridId.ShowGridLines = true;
            svHeadId.Content = headerGridId;
            svHead.Content = headerGrid;
            scrollChanged(null, null);
        }

        string GetPlaneByNumber(int num)
        {
            int i = 0;
            foreach (KeyValuePair<string, List<string>> kvp in DBLogic.Planes)
            {
                if (kvp.Value != null)
                    for (int j = 0; j < kvp.Value.Count; ++j)
                    {
                        if(num==i)return kvp.Value[j];
                        ++i;
                    }
            }
            return null;
        }

        string GetGameByNumber(int num)
        {
            int i = 0;
            foreach (KeyValuePair<string, List<string>> kvp in DBLogic.Planes)
            {
                if (kvp.Value != null)
                    for (int j = 0; j < kvp.Value.Count; ++j)
                    {
                        if (num == i) return kvp.Key;
                        ++i;
                    }
            }
            return null;
        }
        void planeActiveStateChanged(object sender, EventArgs e)
        {
            CheckBox cbx = (CheckBox)sender;
            string[] idParts = cbx.Name.Split('r');
            int indx = Convert.ToInt32(idParts[1]);
            string game = ri[indx].Game;
            if (game == null) game = "DCS";
            int planeWork = Convert.ToInt32(idParts[0].Replace("p", ""));
            string plane = null;
            foreach (KeyValuePair<string, List<string>> kvp in DBLogic.Planes)
            {
                if (kvp.Key == game)
                {
                    plane=kvp.Value[planeWork];
                }
                else if (kvp.Value == null)
                {
                    continue;
                }
                else
                {
                    planeWork = planeWork - kvp.Value.Count;
                }
            }
            if (plane == null)
            {
                MessageBox.Show("Something went wrong assigning new state to aircraft");
                RefreshDGSelected();
                return;
            }
            if (cbx.IsChecked == true) {
                ri[indx].SetAircraftActivity(plane, true);
            }
            else {
                ri[indx].SetAircraftActivity(plane, false);
            }
            
        }
        void selectNone(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("selectNoneBtn", ""));
            string game = ri[indx].Game;
            if (game == null) game = "DCS";
            if (DBLogic.Planes[game] != null)
            {
                foreach(string aircraft in DBLogic.Planes[game])
                {
                    ri[indx].SetAircraftActivity(aircraft, false);
                }
                RefreshDGSelected();
            }
        }
        void selectAll(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("selectAllBtn", ""));
            string game = ri[indx].Game;
            if (game == null) game = "DCS";
            if (DBLogic.Planes[game] != null)
            {
                foreach (string aircraft in DBLogic.Planes[game])
                {
                    ri[indx].SetAircraftActivity(aircraft, true);
                }
                RefreshDGSelected();
            }
                
        }
        void deleteItem(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("deleteBtn", ""));
            Current.RemoveNode(ri[indx].ID, ri[indx].Game);
            RefreshDGSelected();
        }
        
    }
}
