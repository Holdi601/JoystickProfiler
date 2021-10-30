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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace JoyPro
{
    /// <summary>
    /// Interaktionslogik für CollectJoystickImages.xaml
    /// </summary>
    public partial class CollectJoystickImages : Window
    {
        Dictionary<string, string> JoyPaths;
        List<string> JoysticksActiveInBinds;
        Label[] allLabel;
        int openedWindows;
        
        public CollectJoystickImages()
        {
            InitializeComponent();
            openedWindows = 0;
            JoyPaths = new Dictionary<string, string>();
            JoysticksActiveInBinds = InternalDataMangement.GetJoysticksActiveInBinds();
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            SearchExportBtn.Click += new RoutedEventHandler(searchExport);
            ContinueBtn.Click += new RoutedEventHandler(ContinueToNext);
            if (InternalDataMangement.JoystickFileImages != null && InternalDataMangement.JoystickFileImages.Count > 0)
            {
                JoyPaths = InternalDataMangement.JoystickFileImages;
            }
            if (InternalDataMangement.JoystickLayoutExport != null)
            {
                PathToShowLbl.Content = InternalDataMangement.JoystickLayoutExport;
            }
            SetupScrollView();
            InternalDataMangement.CleanJoystickNodes();
        }
        void SetupScrollView()
        {
            Grid g = SetupBaseGrid();
            for(int i=0; i<JoysticksActiveInBinds.Count; ++i)
            {
                Label lblName = new Label();
                lblName.Name = "lblj" + i.ToString();
                lblName.Foreground = Brushes.White;
                lblName.Content = JoysticksActiveInBinds[i];
                lblName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                lblName.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(lblName, 0);
                Grid.SetRow(lblName, i);
                g.Children.Add(lblName);

                Label lblout = new Label();
                lblout.Name = "lblj" + i.ToString();
                lblout.Foreground = Brushes.White;
                if (JoyPaths.ContainsKey(JoysticksActiveInBinds[i]))
                {
                    lblout.Content = JoyPaths[JoysticksActiveInBinds[i]];
                }
                else
                {
                    lblout.Content = "None";
                }
                lblout.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                lblout.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(lblout, 2);
                Grid.SetRow(lblout, i);
                g.Children.Add(lblout);
                allLabel[i] = lblout;

                Button joybtnin = new Button();
                joybtnin.Name = "search" + i.ToString();
                joybtnin.Content = "Search";
                joybtnin.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                joybtnin.VerticalAlignment = VerticalAlignment.Center;
                joybtnin.Width = 100;
                joybtnin.Click += new RoutedEventHandler(searchImage);
                Grid.SetColumn(joybtnin, 1);
                Grid.SetRow(joybtnin, i);
                g.Children.Add(joybtnin);
            }

            g.ShowGridLines = true;
            sv.Content = g;
        }

        void searchExport(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (Directory.Exists(fbd.SelectedPath))
                {
                    PathToShowLbl.Content = fbd.SelectedPath;
                }
            }

        }
        void searchImage(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "PNG Images (*.png)|*.png|Layout files (*.layout)|*.layout";
            ofd.Title = "Search Image";
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("search",""));
            if (MainStructure.msave.lastOpenedLocation.Length < 1 || !Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                ofd.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            string fileToOpen;
            if (ofd.ShowDialog() == true)
            {
                Console.WriteLine(ofd.FileName);
                fileToOpen = ofd.FileName;
                string[] pathParts = fileToOpen.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }
                if (JoyPaths.ContainsKey(JoysticksActiveInBinds[indx]))
                {
                    JoyPaths[JoysticksActiveInBinds[indx]] = fileToOpen;
                }
                else
                {
                    JoyPaths.Add(JoysticksActiveInBinds[indx], fileToOpen);
                }
                allLabel[indx].Content = fileToOpen;
            }

        }
        Grid SetupBaseGrid()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            for (int i = 0; i < 3; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                grid.ColumnDefinitions.Add(c);
            }
            for (int i = 0; i < JoysticksActiveInBinds.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());
            allLabel = new Label[JoysticksActiveInBinds.Count];
            return grid;
        }
        void ContinueToNext(object sender, EventArgs e)
        {
            string exportFolder = (string)PathToShowLbl.Content;
            for (int i = 0; i < allLabel.Length; ++i)
            {
                if ((string)allLabel[i].Content != "None" && !File.Exists((string)allLabel[i].Content))
                {
                    MessageBox.Show("Not all linked files do exist anymore");
                    return;
                }
            }
            if (!Directory.Exists(exportFolder))
            {
                MessageBox.Show("Export folder doesn't exist");
            }
            CloseBtn.IsEnabled = false;
            ContinueBtn.IsEnabled = false;
            foreach(KeyValuePair<string, string> kvp in JoyPaths)
            {
                if (InternalDataMangement.JoystickFileImages == null)
                    InternalDataMangement.JoystickFileImages = new Dictionary<string, string>();
                if (InternalDataMangement.JoystickFileImages.ContainsKey(kvp.Key))
                {
                    //InternalDataMangement.JoystickFileImages[kvp.Key] = kvp.Value;
                }
                else
                {
                    InternalDataMangement.JoystickFileImages.Add(kvp.Key, kvp.Value);
                }
                InternalDataMangement.JoystickLayoutExport = exportFolder;
                EditJoystickLayoutImage ejli = new EditJoystickLayoutImage(kvp.Key, kvp.Value, (string)PathToShowLbl.Content);
                openedWindows++;
                ejli.Closing += new System.ComponentModel.CancelEventHandler(EditWindowClosed);
                ejli.Show();
            }
            CloseBtn.IsEnabled = false;
            ContinueBtn.IsEnabled = false;
            SearchExportBtn.IsEnabled = false;
        }
        void EditWindowClosed(object sender, EventArgs e)
        {
            openedWindows = openedWindows - 1;
            if (openedWindows < 1)
            {
                Close();
            }
        }
        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }
    }
}
