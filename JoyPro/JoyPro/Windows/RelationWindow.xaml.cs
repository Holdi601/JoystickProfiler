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
        DataTable InWorkTable = null;
        public Relation Current = null;
        bool editMode;
        List<ColumnDefinition> headerColumns;
        List<ColumnDefinition> mainColumns;
        List<RelationItem> ri;

        void init()
        {
            SearchQueryTF.TextChanged += new TextChangedEventHandler(SearchQueryChanged);
            RelationNameTF.TextChanged += new TextChangedEventHandler(NameChanged);
            AddItemBtn.Click += new RoutedEventHandler(AddItemBtnHit);
            CancelRelationBtn.Click += new RoutedEventHandler(CloseThis);
            FinishRelationBtn.Click += new RoutedEventHandler(FinishRelation);
            this.Closing += new System.ComponentModel.CancelEventHandler(OnClosing);
            if (MainStructure.msave.relationWindowLast != null)
            {
                if (MainStructure.msave.relationWindowLast.Top != -1) this.Top = MainStructure.msave.relationWindowLast.Top;
                if (MainStructure.msave.relationWindowLast.Left != -1) this.Left = MainStructure.msave.relationWindowLast.Left;
                if (MainStructure.msave.relationWindowLast.Width != -1) this.Width = MainStructure.msave.relationWindowLast.Width;
                if (MainStructure.msave.relationWindowLast.Height != -1) this.Height = MainStructure.msave.relationWindowLast.Height;
            }
            svcCont.ScrollChanged += new ScrollChangedEventHandler(scrollChanged);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            if (Current == null)
            {
                Current = new Relation();
            }
            else
            {
                editMode = true;
                this.Title = "Edit Relation";
                FinishRelationBtn.Visibility = Visibility.Hidden;
                RefreshDGSelected();
                RelationNameTF.Text = Current.NAME;
            }
            svcCont.CanContentScroll = true;
            scrollChanged(null, null);
        }
        public RelationWindow(Relation r)
        {
            InitializeComponent();
            Current = r;
            init();
        }
        public RelationWindow()
        {
            InitializeComponent();
            init();
        }

        void setLastSizeAndPosition()
        {
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            MainStructure.msave.relationWindowLast.Height = this.Height;
            MainStructure.msave.relationWindowLast.Left = this.Left;
            MainStructure.msave.relationWindowLast.Top = this.Top;
            MainStructure.msave.relationWindowLast.Width = this.Width;
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            svcCont.ScrollToVerticalOffset(svcCont.VerticalOffset - e.Delta / 3);
        }

        void scrollChanged(object sender, EventArgs e)
        {
            svcCont.UpdateLayout();
            svHead.ScrollToHorizontalOffset(svcCont.HorizontalOffset);
            for(int i=0; i<headerColumns.Count; ++i)
            {
                headerColumns[i].MinWidth = mainColumns[i].ActualWidth;
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
            if (MainStructure.DoesRelationAlreadyExist(Current.NAME) && !MainStructure.RelationIsTheSame(Current.NAME, Current))
            {
                MessageBox.Show("Relation with same Name already exists.");
                return;
            }
            foreach (KeyValuePair<string, int> kvp in Current.GetPlaneSetState())
            {
                if (kvp.Value > 1)
                {
                    MessageBox.Show("The Plane " + kvp.Key + " has multiple Bindings in this Relation. Either get completly get rid of binding by unchecking all checkboxes of it or reduce it so that the Aircraft has only one appearance");
                    return;
                }
            }

            if (!editMode)
            {
                MainStructure.AddRelation(Current);
                Console.WriteLine("Adds new relation " + Current.NAME);
            }
            else
            {
                MainStructure.ResyncRelations();
                Console.WriteLine("Finished Editing Relation " + Current.NAME);
            }
            setLastSizeAndPosition();
            Close();
        }

        void SearchQueryChanged(object sender, EventArgs e)
        {
            string[] searchwords = SearchQueryTF.Text.ToLower().Split(' ');
            DGSource.ItemsSource = MainStructure.SearchBinds(searchwords);
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
            setLastSizeAndPosition();
        }
       
        void AddItemBtnHit(object sender, EventArgs e)
        {
            List<SearchQueryResults> selected = DGSource.SelectedItems.Cast<SearchQueryResults>().ToList();
            if (selected.Count < 1)
            {
                MessageBox.Show("No Items Selected");
                return;
            }
            bool axis = selected[0].ID.Substring(0, 1) == "a";
            for (int i = 1; i < selected.Count; i++)
            {
                if (axis != (selected[i].ID.Substring(0, 1) == "a"))
                {
                    MessageBox.Show("Axis and Buttons mixed. One relation cannot have IDs that start with 'a' and 'd' mixed.");
                    return;
                }
            }
            for (int i = 0; i < selected.Count; ++i)
            {
                Current.AddNodeDCS(selected[i].ID);
            }
            RefreshDGSelected();
        }

        Grid createBaseGridHeader()
        {
            headerColumns = new List<ColumnDefinition>();
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            int columnsNeeded = MainStructure.Planes.Length + 4;
            grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < columnsNeeded; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                c.Width= (GridLength)converter.ConvertFromString("80");
                grid.ColumnDefinitions.Add(c);
                headerColumns.Add(c);
            }
            return grid;
        }

        Grid createMainGrid(int itemrows)
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            int columnsNeeded = MainStructure.Planes.Length + 4;
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
                c.MinWidth = 80;
                grid.ColumnDefinitions.Add(c);
                mainColumns.Add(c);
            }
            
            return grid;
        }


        void RefreshDGSelected()
        {
            var T = Type.GetType("System.Windows.Controls.Grid+GridLinesRenderer," + " PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            var GLR = Activator.CreateInstance(T);
            GLR.GetType().GetField("s_oddDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.LightGray, 0.5));
            GLR.GetType().GetField("s_evenDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.LightGray, 0.5));
           

            ri = Current.AllRelations();
            Grid grid = createMainGrid(ri.Count);
            for(int i=0; i<ri.Count; ++i)
            {
                Label iditem = new Label();
                iditem.Name = "id_"+ri[i].ID;
                iditem.Foreground = Brushes.White;
                iditem.Content = ri[i].ID;
                iditem.HorizontalAlignment = HorizontalAlignment.Center;
                iditem.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(iditem, 0);
                Grid.SetRow(iditem, i);
                grid.Children.Add(iditem);

                Button deleteBtn = new Button();
                deleteBtn.Name = "deleteBtn" + i.ToString();
                deleteBtn.Content = "Delete";
                deleteBtn.HorizontalAlignment = HorizontalAlignment.Center;
                deleteBtn.VerticalAlignment = VerticalAlignment.Center;
                deleteBtn.Width = 100;
                deleteBtn.Click += new RoutedEventHandler(deleteItem);
                Grid.SetColumn(deleteBtn, 1);
                Grid.SetRow(deleteBtn, i);
                grid.Children.Add(deleteBtn);

                Button selectAllBtn = new Button();
                selectAllBtn.Name = "selectAllBtn" + i.ToString();
                selectAllBtn.Content = "Select All";
                selectAllBtn.HorizontalAlignment = HorizontalAlignment.Center;
                selectAllBtn.VerticalAlignment = VerticalAlignment.Center;
                selectAllBtn.Width = 100;
                selectAllBtn.Click += new RoutedEventHandler(selectAll);
                Grid.SetColumn(selectAllBtn, 2);
                Grid.SetRow(selectAllBtn, i);
                grid.Children.Add(selectAllBtn);

                Button selectNoneBtn = new Button();
                selectNoneBtn.Name = "selectNoneBtn" + i.ToString();
                selectNoneBtn.Content = "Select None";
                selectNoneBtn.HorizontalAlignment = HorizontalAlignment.Center;
                selectNoneBtn.VerticalAlignment = VerticalAlignment.Center;
                selectNoneBtn.Width = 100;
                selectNoneBtn.Click += new RoutedEventHandler(selectNone);
                Grid.SetColumn(selectNoneBtn, 3);
                Grid.SetRow(selectNoneBtn, i);
                grid.Children.Add(selectNoneBtn);

                for(int j=0; j< MainStructure.Planes.Length; ++j)
                {
                    PlaneState ps = ri[i].GetStateAircraftDCS(MainStructure.Planes[j]);
                    if (ps == PlaneState.ACTIVE||ps == PlaneState.DISABLED)
                    {
                        CheckBox cbx = new CheckBox();
                        cbx.Name = "p"+j.ToString()+"r" + i.ToString();
                        cbx.Content = ri[i].GetInputDescription(MainStructure.Planes[j]);
                        cbx.Foreground = Brushes.White;
                        cbx.HorizontalAlignment = HorizontalAlignment.Left;
                        cbx.VerticalAlignment = VerticalAlignment.Center;
                        if (ps == PlaneState.ACTIVE)
                            cbx.IsChecked = true;
                        else
                            cbx.IsChecked = false;
                        cbx.Click += new RoutedEventHandler(planeActiveStateChanged);
                        Grid.SetColumn(cbx, j+4);
                        Grid.SetRow(cbx, i);
                        grid.Children.Add(cbx);
                    }
                    else
                    {
                        Label emptyItem = new Label();
                        emptyItem.Name = "id_" + ri[i].ID;
                        emptyItem.Foreground = Brushes.White;
                        emptyItem.Content = "   ";
                        emptyItem.HorizontalAlignment = HorizontalAlignment.Center;
                        emptyItem.VerticalAlignment = VerticalAlignment.Center;
                        Grid.SetColumn(emptyItem, j+4);
                        Grid.SetRow(emptyItem, i);
                        grid.Children.Add(emptyItem);
                    }
                    
                }
            }
            
            grid.ShowGridLines = true;
            svcCont.Content = grid;

            Grid headerGrid = createBaseGridHeader();

            Label headerID = new Label();
            headerID.Name = "headerID";
            headerID.Foreground = Brushes.White;
            headerID.Content = "ID";
            headerID.HorizontalAlignment = HorizontalAlignment.Left;
            headerID.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(headerID, 0);
            Grid.SetRow(headerID, 0);
            headerGrid.Children.Add(headerID);

            Label deleteBtnLbl = new Label();
            deleteBtnLbl.Name = "deleteBtns";
            deleteBtnLbl.Foreground = Brushes.White;
            deleteBtnLbl.Content = "Delete Buttons";
            deleteBtnLbl.HorizontalAlignment = HorizontalAlignment.Left;
            deleteBtnLbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(deleteBtnLbl, 1);
            Grid.SetRow(deleteBtnLbl, 0);
            headerGrid.Children.Add(deleteBtnLbl);

            Label selectAllBtnLbl = new Label();
            selectAllBtnLbl.Name = "selectAllBtns";
            selectAllBtnLbl.Foreground = Brushes.White;
            selectAllBtnLbl.Content = "Select All";
            selectAllBtnLbl.HorizontalAlignment = HorizontalAlignment.Left;
            selectAllBtnLbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(selectAllBtnLbl, 2);
            Grid.SetRow(selectAllBtnLbl, 0);
            headerGrid.Children.Add(selectAllBtnLbl);

            Label selectNoneBtnLbl = new Label();
            selectNoneBtnLbl.Name = "selectAllBtns";
            selectNoneBtnLbl.Foreground = Brushes.White;
            selectNoneBtnLbl.Content = "Select All";
            selectNoneBtnLbl.HorizontalAlignment = HorizontalAlignment.Left;
            selectNoneBtnLbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(selectNoneBtnLbl, 3);
            Grid.SetRow(selectNoneBtnLbl, 0);
            headerGrid.Children.Add(selectNoneBtnLbl);

            for (int i = 0; i < MainStructure.Planes.Length; ++i)
            {
                Label itemLbl = new Label();
                itemLbl.Name = "Plane" + i.ToString();
                itemLbl.Foreground = Brushes.White;
                itemLbl.Content = MainStructure.Planes[i];
                itemLbl.HorizontalAlignment = HorizontalAlignment.Left;
                itemLbl.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(itemLbl, i + 4);
                Grid.SetRow(itemLbl, 0);
                headerGrid.Children.Add(itemLbl);
            }
            headerGrid.ShowGridLines = true;
            svHead.Content = headerGrid;
            scrollChanged(null, null);
        }
        void planeActiveStateChanged(object sender, EventArgs e)
        {
            CheckBox cbx = (CheckBox)sender;
            string[] idParts = cbx.Name.Split('r');
            string plane = MainStructure.Planes[Convert.ToInt32(idParts[0].Replace("p",""))];
            bool state;
            if (cbx.IsChecked == true) {
                ri[Convert.ToInt32(idParts[1])].SetAircraftActivityDCS(plane, true);
            }
            else {
                ri[Convert.ToInt32(idParts[1])].SetAircraftActivityDCS(plane, false);
            }
            RefreshDGSelected();
        }
        void selectNone(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("selectNoneBtn", ""));
            for(int i=0; i<MainStructure.Planes.Length; ++i)
            {
                ri[indx].SetAircraftActivityDCS(MainStructure.Planes[i], false);
            }
            RefreshDGSelected();
        }
        void selectAll(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("selectAllBtn", ""));
            for (int i = 0; i < MainStructure.Planes.Length; ++i)
            {
                ri[indx].SetAircraftActivityDCS(MainStructure.Planes[i], true);
            }
            RefreshDGSelected();
        }
        void deleteItem(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("deleteBtn", ""));
            Current.RemoveNode(ri[indx].ID);
            RefreshDGSelected();
        }
        
    }
}
