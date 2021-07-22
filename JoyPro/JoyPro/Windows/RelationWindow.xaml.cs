using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
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
    /// Interaktionslogik für RelationWindow.xaml
    /// </summary>
    public partial class RelationWindow : Window
    {
        DataTable InWorkTable = null;
        public Relation Current = null;
        bool editMode;

        void init()
        {
            SearchQueryTF.TextChanged += new TextChangedEventHandler(SearchQueryChanged);
            RelationNameTF.TextChanged += new TextChangedEventHandler(NameChanged);
            AddItemBtn.Click += new RoutedEventHandler(AddItemBtnHit);
            RemoveItemBtn.Click += new RoutedEventHandler(DeleteButtonEvent);
            CancelRelationBtn.Click += new RoutedEventHandler(CloseThis);
            FinishRelationBtn.Click += new RoutedEventHandler(FinishRelation);
            DGAdded.CanUserAddRows = false;
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
            svHead.ScrollToHorizontalOffset(svcCont.HorizontalOffset);

            for (int i = 0; i < DGAdded.Columns.Count; ++i)
            {
                DGHead.Columns[i].MinWidth = DGAdded.Columns[i].ActualWidth;
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
        void DeleteButtonEvent(object sender, EventArgs e)
        {
            List<DataRowView> selected = DGAdded.SelectedItems.Cast<DataRowView>().ToList();
            if (selected.Count < 1)
            {
                MessageBox.Show("No Items Selected");
                return;
            }
            for (int i = 0; i < selected.Count; ++i)
            {
                string id = (string)selected[i].Row[0];
                Current.RemoveNode(id);
            }
            RefreshDGSelected();
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

        DataTable GetEmptyDTForSelection()
        {
            System.Data.DataTable dt = new DataTable("Data");
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("Select None", typeof(bool));
            dt.Columns.Add("Select Rest", typeof(bool));
            for (int i = 0; i < MainStructure.Planes.Length; ++i)
            {
                dt.Columns.Add(MainStructure.Planes[i] + "_cb", typeof(bool));
                dt.Columns.Add(MainStructure.Planes[i] + "_desc", typeof(string));
            }
            return dt;
        }

        void RefreshDGSelected()
        {

            if (InWorkTable != null)
            {
                InWorkTable.Clear();
            }
            List<RelationItem> ri = Current.AllRelations();
            System.Data.DataTable dt = GetEmptyDTForSelection();
            InWorkTable = dt;

            for (int i = 0; i < ri.Count; ++i)
            {
                object[] row = new object[3 + (MainStructure.Planes.Length) * 2];
                row[0] = ri[i].ID;
                int k = 3;
                row[1] = false;
                row[2] = false;
                for (int j = 0; j < MainStructure.Planes.Length; ++j)
                {
                    if (ri[i].GetStateAircraftDCS(MainStructure.Planes[j]) == PlaneState.ACTIVE)
                        row[k] = true;
                    else
                        row[k] = false;
                    ++k;
                    row[k] = ri[i].GetInputDescription(MainStructure.Planes[j]);
                    ++k;
                }
                dt.Rows.Add(row);
            }
            dt.Columns[0].ReadOnly = true;
            for (int i = 4; i < dt.Columns.Count; i += 2)
                dt.Columns[i].ReadOnly = true;
            DGAdded.ItemsSource = dt.DefaultView;
            DGHead.ItemsSource = dt.DefaultView;

        }

        private void DGAdded_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                string planeRaw = e.Column.Header.ToString();
                string item = (string)((DataRowView)e.Row.Item)[0];
                if (planeRaw == "Select None")
                {
                    ((CheckBox)e.EditingElement).IsChecked = false;
                    Current.DeactivateAllID(item);

                }
                else if (planeRaw == "Select Rest")
                {
                    ((CheckBox)e.EditingElement).IsChecked = false;
                    Current.ActivateRestForID(item);

                }
                else if (planeRaw.Length > 2)
                {
                    string plane = planeRaw.Substring(0, planeRaw.Length - 3);
                    bool succ = Current.GetRelationItem(item).SwitchAircraftActivityDCS(plane);
                    if (!succ)
                    {
                        MessageBox.Show("The item that you wanted to enable, doesn't have an implementation for that Plane");
                        ((CheckBox)e.EditingElement).IsChecked = false;
                        return;
                    }
                }
            }
            catch
            {

            }
            
            RefreshDGSelected();
        }
    }
}
