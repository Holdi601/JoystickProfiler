﻿using System;
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
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;

        public GroupManagerW()
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;

            if (MainStructure.msave != null && MainStructure.msave._GroupManagerWindow != null)
            {
                if (MainStructure.msave._GroupManagerWindow.Top > 0) this.Top = MainStructure.msave._GroupManagerWindow.Top;
                if (MainStructure.msave._GroupManagerWindow.Left > 0) this.Left = MainStructure.msave._GroupManagerWindow.Left;
                if (MainStructure.msave._GroupManagerWindow.Width > 0) this.Width = MainStructure.msave._GroupManagerWindow.Width;
                if (MainStructure.msave._GroupManagerWindow.Height > 0) this.Height = MainStructure.msave._GroupManagerWindow.Height;
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
            
            if (InternalDataManagement.AllGroups == null) return null;

            for (int i = 0; i < InternalDataManagement.AllGroups.Count; i++)
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
                NewGroupTF.Text!="NONE" &&
                NewGroupTF.Text != "UNASSIGNED"&&
                !NewGroupTF.Text.Contains("\"") &&
                !NewGroupTF.Text.Contains("\\") &&
                !NewGroupTF.Text.Contains(","))
            {
                if (!InternalDataManagement.AllGroups.Contains(NewGroupTF.Text))
                {
                    InternalDataManagement.AllGroups.Add(NewGroupTF.Text);
                    InternalDataManagement.GroupActivity.Add(NewGroupTF.Text, true);
                }
                    
                NewGroupTF.Text = "";

            }
            else
            {
                MessageBox.Show("Name invalid - either to short or reserved name");
            }
            InternalDataManagement.AllGroups.Sort();
            ListExistingGroups();
            
        }

        void DeleteGroup(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("GrpDeleteName", ""));
            if (indx < 0||indx>= InternalDataManagement.AllGroups.Count) return;
            string groupToDelete = InternalDataManagement.AllGroups[indx];
            InternalDataManagement.RemoveGroupFromRelation(groupToDelete);
            InternalDataManagement.AllGroups.Remove(groupToDelete);
            InternalDataManagement.AllGroups.Sort();
            if (InternalDataManagement.GroupActivity.ContainsKey(groupToDelete))
            {
                InternalDataManagement.GroupActivity.Remove(groupToDelete);
            }
            ListExistingGroups();
        }

        void ListExistingGroups()
        {
            GroupDeleteBtn.Clear();
            Grid grid = BaseSetupRelationGrid();
            if (grid == null) return;
            for (int i = 0; i < InternalDataManagement.AllGroups.Count; ++i)
            {
                Label cbx = new Label();
                cbx.Name = "cbxjy" + i.ToString();
                cbx.Content = InternalDataManagement.AllGroups[i];
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
