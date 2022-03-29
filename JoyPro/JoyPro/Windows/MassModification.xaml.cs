using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for MassModification.xaml
    /// </summary>
    public partial class MassModification : Window
    {
        List<Relation> RelationToDisplay;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;
        CheckBox[] createdCBs;
        List<CheckBox> selectedPlanes;
        public MassModification(List<Relation> Relations)
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;
            RelationToDisplay = Relations;
            if (MainStructure.msave != null && MainStructure.msave._MassOperationWindow != null)
            {
                if (MainStructure.msave._MassOperationWindow.Top > 0) this.Top = MainStructure.msave._MassOperationWindow.Top;
                if (MainStructure.msave._MassOperationWindow.Left > 0) this.Left = MainStructure.msave._MassOperationWindow.Left;
                if (MainStructure.msave._MassOperationWindow.Width > 0) this.Width = MainStructure.msave._MassOperationWindow.Width;
                if (MainStructure.msave._MassOperationWindow.Height > 0) this.Height = MainStructure.msave._MassOperationWindow.Height;
            }
            selectedPlanes = new List<CheckBox>();
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            DeleteBtn.Click += new RoutedEventHandler(DeleteRelations);
            MergeBtn.Click += new RoutedEventHandler(MergeRelations);
            SplitOutBtn.Click += new RoutedEventHandler(SplitRelations);
            DuplicateBtn.Click += new RoutedEventHandler(Duplicate);
            AddPrefixBtn.Click += new RoutedEventHandler(AddPrefix);
            AddPostfixBtn.Click += new RoutedEventHandler(AddPostfix);
            RemoveStringPartBtn.Click += new RoutedEventHandler(RemoveNamePart);
            ExportToRelationBtn.Click += new RoutedEventHandler(SaveRelationsTo);
            ListRelations();
            selectedPlanes = new List<CheckBox>();
            fillPlaneDropDown();
        }

        void SaveRelationsTo(object sender, EventArgs e)
        {
            Dictionary<string, Relation> relations= new Dictionary<string, Relation>();
            for (int i = 0; i < RelationToDisplay.Count; i++)
            {
                if (createdCBs[i].IsChecked == true)
                {
                    relations.Add(RelationToDisplay[i].NAME, RelationToDisplay[i]);
                }
            }
            if (relations.Count < 1)
            {
                MessageBox.Show("No relations to export Selected");
                return;
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Relation Files (*.rl)|*.rl";
            saveFileDialog1.Title = "Save Relations";
            if (Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                saveFileDialog1.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            else
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            string filePath;
            saveFileDialog1.ShowDialog();
            filePath = saveFileDialog1.FileName;
            string[] pathParts = filePath.Split('\\');
            if (pathParts.Length > 0)
            {
                MainStructure.msave.lastOpenedLocation = pathParts[0];
                for (int i = 1; i < pathParts.Length - 1; ++i)
                {
                    MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                }
            }
            InternalDataManagement.SaveRelationsTo(filePath, relations);

            Close();
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }
        void ListRelations()
        {
            Grid g = BaseSetupRelationGrid();
            createdCBs = new CheckBox[RelationToDisplay.Count];
            for (int i=0; i<RelationToDisplay.Count; i++)
            {
                CheckBox cbxu = new CheckBox();
                cbxu.Name = "cb" + i.ToString();
                cbxu.Content = RelationToDisplay[i].NAME;
                cbxu.Foreground = Brushes.White;
                cbxu.HorizontalAlignment = HorizontalAlignment.Left;
                cbxu.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(cbxu, 10);
                Grid.SetRow(cbxu, i);
                createdCBs[i] = cbxu;
                g.Children.Add(cbxu);
            }
            g.ShowGridLines = true;
            sv.Content = g;
        }

        Grid BaseSetupRelationGrid()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            for (int i = 0; i < RelationToDisplay.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());
            return grid;
        }

        void DeleteRelations(object sender, EventArgs e)
        {
            List<Relation> toDelete = new List<Relation>();
            for(int i=0; i<RelationToDisplay.Count; i++)
            {
                if (createdCBs[i].IsChecked == true)
                {
                    toDelete.Add(RelationToDisplay[i]);
                }
            }
            if (toDelete.Count < 1)
            {
                MessageBox.Show("No relations to Delete Selected");
                return;
            }
            for(int i=0; i<toDelete.Count; i++)
            {
                InternalDataManagement.RemoveRelation(toDelete[i]);
            }
            Close();
        }

        void fillPlaneDropDown()
        {
            PlaneDropdown.Items.Clear();
            CheckBox cbpAll = new CheckBox();
            cbpAll.Name = "ALL";
            cbpAll.Content = "ALL";
            cbpAll.IsChecked = false;
            cbpAll.Click += new RoutedEventHandler(PlaneFilterChanged);
            PlaneDropdown.Items.Add(cbpAll);

            CheckBox cbpNone = new CheckBox();
            cbpNone.Name = "NONE";
            cbpNone.Content = "NONE";
            cbpNone.IsChecked = false;
            cbpNone.Click += new RoutedEventHandler(PlaneFilterChanged);
            PlaneDropdown.Items.Add(cbpNone);

            for (int i = 0; i < DBLogic.Planes.Count; ++i)
            {
                CheckBox cbgpAll = new CheckBox();
                cbgpAll.Name = "ALL";
                cbgpAll.Content = DBLogic.Planes.ElementAt(i).Key + ":" + "ALL";
                cbgpAll.IsChecked = false;
                cbgpAll.Click += new RoutedEventHandler(PlaneFilterChanged);
                PlaneDropdown.Items.Add(cbgpAll);

                CheckBox cbgpNone = new CheckBox();
                cbgpNone.Name = "NONE";
                cbgpNone.Content = DBLogic.Planes.ElementAt(i).Key + ":" + "NONE";
                cbgpNone.IsChecked = false;
                cbgpNone.Click += new RoutedEventHandler(PlaneFilterChanged);
                PlaneDropdown.Items.Add(cbgpNone);
            }
            int h = 0;
            for (int i = 0; i < DBLogic.Planes.Count; ++i)
            {
                for (int j = 0; j < DBLogic.Planes.ElementAt(i).Value.Count; ++j)
                {
                    CheckBox pln = new CheckBox();
                    pln.Name = "plane";
                    string k = DBLogic.Planes.ElementAt(i).Key + ":" + DBLogic.Planes.ElementAt(i).Value[j];
                    pln.Content = k;
                    pln.Click += new RoutedEventHandler(PlaneFilterChanged);
                    PlaneDropdown.Items.Add(pln);
                    selectedPlanes.Add(pln);
                }
            }
        }

        private void PlaneFilterChanged(object sender, RoutedEventArgs e)
        {
            CheckBox sndr = (CheckBox)sender;
            if ((string)sndr.Content == "ALL")
            {
                sndr.IsChecked = false;
                for (int i = 0; i < selectedPlanes.Count; ++i)
                {
                    selectedPlanes[i].IsChecked = true;
                }
            }
            else if ((string)sndr.Content == "NONE")
            {
                sndr.IsChecked = false;
                for (int i = 0; i < selectedPlanes.Count; ++i)
                {
                    selectedPlanes[i].IsChecked = false;
                }
            }
            else if (((string)sndr.Content).Contains(":ALL"))
            {
                sndr.IsChecked = false;
                string game = ((string)sndr.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                for (int i = 0; i < selectedPlanes.Count; ++i)
                {
                    string key = (string)selectedPlanes[i].Content;
                    if (key.StartsWith(game)) selectedPlanes[i].IsChecked = true;
                }
            }
            else if (((string)sndr.Content).Contains(":NONE"))
            {
                sndr.IsChecked = false;
                string game = ((string)sndr.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                for (int i = 0; i < selectedPlanes.Count; ++i)
                {
                    string key = (string)selectedPlanes[i].Content;
                    if (key.StartsWith(game)) selectedPlanes[i].IsChecked = false;
                }
            }
        }

        void RemoveNamePart(object sender, EventArgs e)
        {
            List<Relation> toRename = new List<Relation>();
            for (int i = 0; i < RelationToDisplay.Count; i++)
            {
                if (createdCBs[i].IsChecked == true)
                {
                    toRename.Add(RelationToDisplay[i]);
                }
            }
            if (toRename.Count < 1)
            {
                MessageBox.Show("No relations to Remove Name Part");
                return;
            }
            if (inputBox.Text.Replace(" ", "").Length < 1)
            {
                MessageBox.Show("RelationName in the input box cant be shorter than 3 Characters.");
                return;
            }
            for (int i = 0; i < toRename.Count; ++i)
            {
                Relation r = toRename[i].Copy();
                r.NAME = r.NAME.Replace(inputBox.Text, "");
                InternalDataManagement.ReplaceRelation(toRename[i], r);
            }
            Close();
        }

        void AddPrefix(object sender, EventArgs e)
        {
            List<Relation> toRename = new List<Relation>();
            for (int i = 0; i < RelationToDisplay.Count; i++)
            {
                if (createdCBs[i].IsChecked == true)
                {
                    toRename.Add(RelationToDisplay[i]);
                }
            }
            if (toRename.Count < 1)
            {
                MessageBox.Show("No relations to add Prefix Selected");
                return;
            }
            if (inputBox.Text.Replace(" ", "").Length < 1)
            {
                MessageBox.Show("RelationName in the input box cant be shorter than 3 Characters.");
                return;
            }
            for(int i = 0; i < toRename.Count; ++i)
            {
                Relation r = toRename[i].Copy();
                r.NAME = inputBox.Text.Trim() + r.NAME;
                InternalDataManagement.ReplaceRelation(toRename[i], r);
            }
            Close();
        }

        void AddPostfix(object sender, EventArgs e)
        {
            List<Relation> toRename = new List<Relation>();
            for (int i = 0; i < RelationToDisplay.Count; i++)
            {
                if (createdCBs[i].IsChecked == true)
                {
                    toRename.Add(RelationToDisplay[i]);
                }
            }
            if (toRename.Count < 1)
            {
                MessageBox.Show("No relations to add Prefix Selected");
                return;
            }
            if (inputBox.Text.Replace(" ", "").Length < 1)
            {
                MessageBox.Show("RelationName in the input box cant be shorter than 3 Characters.");
                return;
            }
            for (int i = 0; i < toRename.Count; ++i)
            {
                Relation r = toRename[i].Copy();
                r.NAME = r.NAME+ inputBox.Text.Trim();
                InternalDataManagement.ReplaceRelation(toRename[i], r);
            }
            Close();
        }

        void Duplicate(object sender, EventArgs e)
        {
            List<Relation> toDup = new List<Relation>();
            for (int i = 0; i < RelationToDisplay.Count; i++)
            {
                if (createdCBs[i].IsChecked == true)
                {
                    toDup.Add(RelationToDisplay[i]);
                }
            }
            if (toDup.Count < 1)
            {
                MessageBox.Show("No relations to Duplicate Selected");
                return;
            }
            for(int i = 0;i < toDup.Count; ++i)
            {
                InternalDataManagement.DuplicateRelation(toDup[i]);
            }
            Close();
        }

        void MergeRelations(object sender, EventArgs e)
        {
            List<Relation> toMerge = new List<Relation>();
            for (int i = 0; i < RelationToDisplay.Count; i++)
            {
                if (createdCBs[i].IsChecked == true)
                {
                    toMerge.Add(RelationToDisplay[i]);
                }
            }
            if (toMerge.Count < 1)
            {
                MessageBox.Show("No relations to Merge Selected");
                return;
            }
            if (inputBox.Text.Replace(" ", "").Length < 1)
            {
                MessageBox.Show("RelationName in the input box cant be shorter than 3 Characters.");
                return;
            }
            bool isAxisRef = toMerge[0].ISAXIS;
            for(int i = 1; i < toMerge.Count; i++)
            {
                if (isAxisRef != toMerge[i].ISAXIS)
                {
                    MessageBox.Show("Please only select only Buttons or only Axis relations in your list");
                    return;
                }
            }
            //Integrity check if all axis or button
            string newName = inputBox.Text.Trim();
            InternalDataManagement.MergeRelations(toMerge, newName);
            Close();
        }

        void SplitRelations(object sender, EventArgs e)
        {
            List<Relation> toSplit = new List<Relation>();
            List<string> planesToSplitOff = new List<string>();
            for (int i = 0; i < RelationToDisplay.Count; i++)
            {
                if (createdCBs[i].IsChecked == true)
                {
                    toSplit.Add(RelationToDisplay[i]);
                }
            }
            if (toSplit.Count < 1)
            {
                MessageBox.Show("No relations to Split Selected");
                return;
            }
            if (inputBox.Text.Replace(" ", "").Length < 3)
            {
                MessageBox.Show("Relation Postfix in the input box cant be shorter than 3 Characters.");
                return;
            }
            for(int i = 0;i< selectedPlanes.Count; ++i)
            {
                if(selectedPlanes[i].IsChecked == true)
                {
                    planesToSplitOff.Add((string)selectedPlanes[i].Content);
                }
            }
            if (planesToSplitOff.Count < 1)
            {
                MessageBox.Show("No Planes to split off");
                return;
            }
            bool isAxisRef = toSplit[0].ISAXIS;
            for (int i = 1; i < toSplit.Count; i++)
            {
                if (isAxisRef != toSplit[i].ISAXIS)
                {
                    MessageBox.Show("Please only select only Buttons or only Axis relations in your list");
                    return;
                }
            }
            //Integrity check if all axis or button

            string postfix = inputBox.Text.Trim();
            InternalDataManagement.SplitRelations(toSplit, postfix, planesToSplitOff);
            Close();
        }
    }
}
