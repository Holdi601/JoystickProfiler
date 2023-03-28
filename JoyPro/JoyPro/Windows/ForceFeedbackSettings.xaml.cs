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
    /// Interaction logic for StickMention.xaml
    /// </summary>
    public partial class ForceFeedbackSettings : Window
    {
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;
        public Dictionary<string, ForceFeedbackS> sticks = new Dictionary<string, ForceFeedbackS>();
        public ForceFeedbackSettings()
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;
            if (MainStructure.msave != null && MainStructure.msave._JoystickFF != null)
            {
                if (MainStructure.msave._JoystickFF.Top > 0) this.Top = MainStructure.msave._JoystickFF.Top;
                if (MainStructure.msave._JoystickFF.Left > 0) this.Left = MainStructure.msave._JoystickFF.Left;
                if (MainStructure.msave._JoystickFF.Width > 0) this.Width = MainStructure.msave._JoystickFF.Width;
                if (MainStructure.msave._JoystickFF.Height > 0) this.Height = MainStructure.msave._JoystickFF.Height;
            }
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            GetSticks();
            ListSticks();
        }

        void GetSticks()
        {
            sticks = new Dictionary<string, ForceFeedbackS>();
            foreach (KeyValuePair<string, ForceFeedbackS> val in InternalDataManagement.JoystickFFB)
            {
                if (!sticks.ContainsKey(val.Key))
                {
                    sticks.Add(val.Key, val.Value);
                }
            }
            foreach (Bind b in InternalDataManagement.AllBinds.Values)
            {
                if (!sticks.ContainsKey(b.Joystick))
                {
                    sticks.Add(b.Joystick, new ForceFeedbackS());
                }
            }
        }
        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }

        void ListSticks()
        {
            Grid g = BaseGrid();
            for (int i = 0; i < sticks.Count; i++)
            {
                Label lbl = new Label();
                lbl.Name = "lbl" + i.ToString();
                lbl.Content = sticks.ElementAt(i).Key;
                lbl.Foreground = Brushes.White;
                lbl.HorizontalAlignment = HorizontalAlignment.Left;
                lbl.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(lbl, 0);
                Grid.SetRow(lbl, i);
                g.Children.Add(lbl);

                CheckBox cbSwapAxis = new CheckBox();
                cbSwapAxis.Name = "cbsa" + i.ToString();
                cbSwapAxis.Content = "Swap Axis";
                cbSwapAxis.Foreground = Brushes.White;
                cbSwapAxis.HorizontalAlignment = HorizontalAlignment.Center;
                cbSwapAxis.VerticalAlignment = VerticalAlignment.Center;
                cbSwapAxis.Width = 200;
                cbSwapAxis.IsChecked = sticks.ElementAt(i).Value.swapAxis;
                cbSwapAxis.Click += new RoutedEventHandler(cbClicked);
                Grid.SetColumn(cbSwapAxis, 1);
                Grid.SetRow(cbSwapAxis, i);
                g.Children.Add(cbSwapAxis);

                CheckBox cbinvertX = new CheckBox();
                cbinvertX.Name = "cbix" + i.ToString();
                cbinvertX.Content = "Invert X";
                cbinvertX.Foreground = Brushes.White;
                cbinvertX.HorizontalAlignment = HorizontalAlignment.Center;
                cbinvertX.VerticalAlignment = VerticalAlignment.Center;
                cbinvertX.Width = 200;
                cbinvertX.IsChecked = sticks.ElementAt(i).Value.invertX;
                cbinvertX.Click += new RoutedEventHandler(cbClicked);
                Grid.SetColumn(cbinvertX, 2);
                Grid.SetRow(cbinvertX, i);
                g.Children.Add(cbinvertX);

                CheckBox cbinvertY = new CheckBox();
                cbinvertY.Name = "cbiy" + i.ToString();
                cbinvertY.Content = "Invert Y";
                cbinvertY.Foreground = Brushes.White;
                cbinvertY.HorizontalAlignment = HorizontalAlignment.Center;
                cbinvertY.VerticalAlignment = VerticalAlignment.Center;
                cbinvertY.Width = 200;
                cbinvertY.IsChecked = sticks.ElementAt(i).Value.invertY;
                cbinvertY.Click += new RoutedEventHandler(cbClicked);

                Grid.SetColumn(cbinvertY, 3);
                Grid.SetRow(cbinvertY, i);
                g.Children.Add(cbinvertY);
            }
            g.ShowGridLines = true;
            sv.Content = g;
        }

        void cbClicked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            int num = Convert.ToInt32(cb.Name.Substring(4));
            ForceFeedbackS ffs = sticks.ElementAt(num).Value;
            if (cb.Name.StartsWith("cbsa"))
            {
                ffs.swapAxis = cb.IsChecked==true? true : false;
            }else if (cb.Name.StartsWith("cbix"))
            {
                ffs.invertX = cb.IsChecked == true ? true : false;
            }
            else if (cb.Name.StartsWith("cbiy"))
            {
                ffs.invertY = cb.IsChecked == true ? true : false;
            }
            if(InternalDataManagement.JoystickFFB.ContainsKey(sticks.ElementAt(num).Key))
            {
                InternalDataManagement.JoystickFFB[sticks.ElementAt(num).Key] = ffs;
            }
            else
            {
                InternalDataManagement.JoystickFFB.Add(sticks.ElementAt(num).Key, ffs);
            }
            
        }
        Grid BaseGrid()
        {
            int gridCols = 4;
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            for (int i = 0; i < gridCols; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                grid.ColumnDefinitions.Add(c);
            }
            for (int i = 0; i < sticks.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());
            return grid;
        }
    }
}
