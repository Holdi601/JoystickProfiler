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
    /// Interaktionslogik für GroupManagerW.xaml
    /// </summary>
    public partial class GroupManagerW : Window
    {
        List<Button> GroupDeleteBtn;
        public GroupManagerW()
        {
            InitializeComponent();
            if (MainStructure.msave != null && MainStructure.msave.GrpMngr != null)
            {
                if (MainStructure.msave.GrpMngr.Top > 0) this.Top = MainStructure.msave.GrpMngr.Top;
                if (MainStructure.msave.GrpMngr.Left > 0) this.Left = MainStructure.msave.GrpMngr.Left;
                if (MainStructure.msave.GrpMngr.Width > 0) this.Width = MainStructure.msave.GrpMngr.Width;
                if (MainStructure.msave.GrpMngr.Height > 0) this.Height = MainStructure.msave.GrpMngr.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }
            CloseBtn.Click += new RoutedEventHandler(CloseGroupManager);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            GroupDeleteBtn = new List<Button>();
            AddBtn.Click += new RoutedEventHandler(AddGroup);
            NewGroupTF.KeyUp += new KeyEventHandler(GroupAddEnter);
            ListExistingGroups();
        }

        private void GroupAddEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddGroup(sender, e);
            }
        }

        Grid BaseSetupRelationGrid()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = (GridLength)converter.ConvertFromString("110");
            grid.ColumnDefinitions.Add(cd);
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            
            if (MainStructure.AllGroups == null) return null;

            for (int i = 0; i < MainStructure.AllGroups.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());

            return grid;
        }

        void AddGroup(object sender, EventArgs e)
        {
            if(NewGroupTF.Text.Replace(" ", "").Length > 1&&
                NewGroupTF.Text!="ALL" &&
                NewGroupTF.Text!="NONE")
            {
                if (!MainStructure.AllGroups.Contains(NewGroupTF.Text))
                {
                    MainStructure.AllGroups.Add(NewGroupTF.Text);
                    MainStructure.GroupActivity.Add(NewGroupTF.Text, true);
                }
                    
                NewGroupTF.Text = "";

            }
            else
            {
                MessageBox.Show("Name invalid - either to short or reserved name");
            }
            MainStructure.AllGroups.Sort();
            ListExistingGroups();
            
        }

        void DeleteGroup(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("GrpDeleteName", ""));
            if (indx < 0||indx>=MainStructure.AllGroups.Count) return;
            string groupToDelete = MainStructure.AllGroups[indx];
            MainStructure.RemoveGroupFromRelation(groupToDelete);
            MainStructure.AllGroups.Remove(groupToDelete);
            MainStructure.AllGroups.Sort();
            if (MainStructure.GroupActivity.ContainsKey(groupToDelete))
            {
                MainStructure.GroupActivity.Remove(groupToDelete);
            }
            ListExistingGroups();
        }

        void ListExistingGroups()
        {
            GroupDeleteBtn.Clear();
            Grid grid = BaseSetupRelationGrid();
            if (grid == null) return;
            for (int i = 0; i < MainStructure.AllGroups.Count; ++i)
            {
                Label cbx = new Label();
                cbx.Name = "cbxjy" + i.ToString();
                cbx.Content = MainStructure.AllGroups[i];
                cbx.Foreground = Brushes.White;
                cbx.HorizontalAlignment = HorizontalAlignment.Left;
                cbx.VerticalAlignment = VerticalAlignment.Center;
                Thickness margin = cbx.Margin;
                margin.Left = 10;
                cbx.Margin = margin;
                Grid.SetColumn(cbx, 1);
                Grid.SetRow(cbx, i);
                grid.Children.Add(cbx);

                Button GrpDltBtn = new Button();
                GrpDltBtn.Name = "GrpDeleteName"+i.ToString();
                GrpDltBtn.Content = "Delete Grouping";
                GrpDltBtn.Foreground = Brushes.White;
                GrpDltBtn.HorizontalAlignment = HorizontalAlignment.Center;
                GrpDltBtn.VerticalAlignment = VerticalAlignment.Center;
                GrpDltBtn.Background = Brushes.DarkSlateGray;
                GrpDltBtn.Click += new RoutedEventHandler(DeleteGroup);
                Grid.SetColumn(GrpDltBtn, 0);
                Grid.SetRow(GrpDltBtn, i);
                grid.Children.Add(GrpDltBtn);
                GroupDeleteBtn.Add(GrpDltBtn);
            }
            sv.Content = grid;
        }

        void CloseGroupManager(object sender, EventArgs e)
        {
            MainStructure.SaveMetaLast();
            Close();          
            
            
        }
    }
}
