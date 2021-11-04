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
    /// Interaction logic for CollectSticksForVisual.xaml
    /// </summary>
    public partial class CollectSticksForVisual : Window
    {
        Dictionary<string, string> JoyPaths;
        List<string> AllJoysticks;
        List<string> connectedSticks;
        Label[] allLabel;
        public CollectSticksForVisual()
        {
            InitializeComponent();
            connectedSticks = JoystickReader.GetConnectedJoysticks();
            JoyPaths = new Dictionary<string, string>();
            AllJoysticks = InternalDataMangement.LocalJoysticks.ToList();
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            if (InternalDataMangement.JoystickFileImages != null && InternalDataMangement.JoystickFileImages.Count > 0)
            {
                JoyPaths = InternalDataMangement.JoystickFileImages;
            }
            else
            {
                InternalDataMangement.JoystickFileImages = new Dictionary<string, string>();
                JoyPaths = InternalDataMangement.JoystickFileImages;
            }
            SetupScrollView();
            InternalDataMangement.CleanJoystickNodes();
            this.Closing += new System.ComponentModel.CancelEventHandler(Finalising);
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
            for (int i = 0; i < AllJoysticks.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());
            allLabel = new Label[AllJoysticks.Count];
            return grid;
        }

        void SetupScrollView()
        {
            Grid g = SetupBaseGrid();
            for (int i = 0; i < AllJoysticks.Count; ++i)
            {
                Label lblName = new Label();
                lblName.Name = "lblj" + i.ToString();
                if (connectedSticks.Contains(AllJoysticks[i]))
                {
                    lblName.Foreground = Brushes.GreenYellow;
                }
                else
                {
                    lblName.Foreground = Brushes.White;
                }

                lblName.Content = AllJoysticks[i];
                lblName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                lblName.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(lblName, 0);
                Grid.SetRow(lblName, i);
                g.Children.Add(lblName);

                Label lblout = new Label();
                lblout.Name = "lblj" + i.ToString();
                lblout.Foreground = Brushes.White;
                if (JoyPaths.ContainsKey(AllJoysticks[i]))
                {
                    lblout.Content = JoyPaths[AllJoysticks[i]];
                }
                else
                {
                    lblout.Content = "None";
                }
                lblout.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                lblout.VerticalAlignment = VerticalAlignment.Center;
                lblout.MouseRightButtonUp += new MouseButtonEventHandler(deleteStickReference);
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
                joybtnin.Click += new RoutedEventHandler(searchLayout);
                Grid.SetColumn(joybtnin, 1);
                Grid.SetRow(joybtnin, i);
                g.Children.Add(joybtnin);
            }

            g.ShowGridLines = true;
            sv.Content = g;
        }

        void searchLayout(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Layout files (*.layout)|*.layout";
            ofd.Title = "Search Layout";
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("search", ""));
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
                if (JoyPaths.ContainsKey(AllJoysticks[indx]))
                {
                    JoyPaths[AllJoysticks[indx]] = fileToOpen;
                }
                else
                {
                    JoyPaths.Add(AllJoysticks[indx], fileToOpen);
                }
                allLabel[indx].Content = fileToOpen;
            }

        }
        void deleteStickReference(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            string cont = (string)l.Content;
            if (cont != "None")
            {
                string toRemove = "";
                for (int i = 0; i < JoyPaths.Count; i++)
                {
                    if (cont == JoyPaths.ElementAt(i).Value)
                    {
                        toRemove = JoyPaths.ElementAt(i).Key;
                        break;
                    }
                }
                JoyPaths.Remove(toRemove);
                l.Content = "None";
            }
        }

        void Finalising(object sender, EventArgs e)
        {
            MainStructure.VisualMode = true;
            InternalDataMangement.ResyncRelations();
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }
    }
}
