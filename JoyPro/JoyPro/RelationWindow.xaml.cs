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
        DataTable InWorkTable=null;
        public Relation Current = null;
        bool editMode;
        public RelationWindow()
        {
            InitializeComponent();
            SearchQueryTF.TextChanged += new TextChangedEventHandler(SearchQueryChanged);
            RelationNameTF.TextChanged += new TextChangedEventHandler(NameChanged);
            AddItemBtn.Click += new RoutedEventHandler(AddItemBtnHit);
            RemoveItemBtn.Click += new RoutedEventHandler(DeleteButtonEvent);
            CancelRelationBtn.Click += new RoutedEventHandler(CloseThis);
            FinishRelationBtn.Click += new RoutedEventHandler(FinishRelation);
            DGAdded.CanUserAddRows = false;

            if (Current == null)
            {
                Current = new Relation();
            }
            else
            {
                editMode = true;
                RefreshDGSelected();
                RelationNameTF.Text = Current.NAME;
            }
                
        }

        public void Refresh()
        {
            editMode = true;
            RefreshDGSelected();
            RelationNameTF.Text = Current.NAME;
        }

        void FinishRelation(object sender, EventArgs e)
        {
            if (Current.NAME==null || Current.NAME.Length < 1)
            {
                MessageBox.Show("No Relation name set.");
                return;
            }
            if (Current.IsEmpty())
            {
                MessageBox.Show("Relation has no nodes.");
                return;
            }
            if (MainStructure.DoesRelationAlreadyExist(Current.NAME)&& !MainStructure.RelationIsTheSame(Current.NAME, Current))
            {
                MessageBox.Show("Relation with same Name already exists.");
                return;               
            }
            foreach(KeyValuePair<string, int> kvp in Current.GetPlaneSetState())
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
            Close();
        }

        void DeleteButtonEvent (object sender, EventArgs e)
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
            for(int i=1; i<selected.Count; i++)
            {
                if(axis!= (selected[i].ID.Substring(0, 1) == "a"))
                {
                    MessageBox.Show("Axis and Buttons mixed. One relation cannot have IDs that start with 'a' and 'd' mixed.");
                    return;
                }
            }
            for(int i=0; i<selected.Count; ++i)
            {
                Current.AddNode(selected[i].ID);
            }
            RefreshDGSelected();
        }

        DataTable GetEmptyDTForSelection()
        {
            System.Data.DataTable dt = new DataTable("Data");
            dt.Columns.Add("ID", typeof(string));
            for (int i = 1; i < MainStructure.Planes.Length; ++i)
            {
                dt.Columns.Add(MainStructure.Planes[i]+"_cb", typeof(bool));
                dt.Columns.Add(MainStructure.Planes[i]+"_desc", typeof(string));
            }
            return dt;
        }

        void RefreshDGSelected()
        {
            Console.WriteLine("Got Here");
            //It seems to crash sometime in Microsofts libs, I have no clue why as it doesn't give me a reason other than a nullptr, So thats my best way to cope with it....
            try
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
                    object[] row = new object[1 + (MainStructure.Planes.Length - 1) * 2];
                    row[0] = ri[i].ID;
                    int k = 1;
                    for (int j = 1; j < MainStructure.Planes.Length; ++j)
                    {
                        if (ri[i].GetStateAircraft(MainStructure.Planes[j]) == PlaneState.ACTIVE)
                            row[k] = true;
                        else
                            row[k] = false;
                        ++k;
                        row[k] = ri[i].GetInputDescription(MainStructure.Planes[j]);
                        ++k;
                    }
                    dt.Rows.Add(row);
                }
                DGAdded.ItemsSource = dt.DefaultView;
            }
            catch(Exception e)
            {
                Console.WriteLine("BLOODY EXCEPTION WHY?!");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Source);
            }            
        }

        private void DGAdded_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string planeRaw = e.Column.Header.ToString();
            string plane = planeRaw.Substring(0, planeRaw.Length - 3);
            string item = (string)((DataRowView)e.Row.Item)[0];
            bool succ = Current.GetRelationItem(item).SwitchAircraftActivity(plane);
            if (!succ)
            {
                MessageBox.Show("The item that you wanted to enable, doesn't have an implementation for that Plane");
                return;
            }
        }
    }
}
