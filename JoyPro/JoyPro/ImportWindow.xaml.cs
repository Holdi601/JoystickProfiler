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
    /// Interaktionslogik für ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        public List<string> selectedSticks;
        public string[] availableJoysticks; 
        //check for null
        public ImportWindow()
        {
            selectedSticks = new List<string>();
            MainStructure.InitDCSJoysticks();
            availableJoysticks = MainStructure.DCSJoysticks;
            InitializeComponent();
            CancelBtn.Click += new RoutedEventHandler(CancelImport);
            ImportBtn.Click += new RoutedEventHandler(Import);
            ListSticks();
            if (MainStructure.importWindowLast != null)
            {
                this.Top = MainStructure.importWindowLast.Top;
                this.Left = MainStructure.importWindowLast.Left;
                this.Width = MainStructure.importWindowLast.Width;
                this.Height = MainStructure.importWindowLast.Height;
            }
        }

        void CancelImport(object sender, EventArgs e)
        {
            MainStructure.importWindowLast = MainStructure.GetWindowPosFrom(this);
            Close();
        }

        void Import(object sender, EventArgs e)
        {
            MainStructure.importWindowLast = MainStructure.GetWindowPosFrom(this);
            bool inv, slid, curv, dz, sx, sy, importDefault;
            if (CBinv.IsChecked == true)
                inv = true;
            else
                inv = false;
            if (CBslid.IsChecked == true)
                slid = true;
            else
                slid = false;
            if (CBcurv.IsChecked == true)
                curv = true;
            else
                curv = false;
            if (CBdz.IsChecked == true)
                dz = true;
            else
                dz = false;
            if (CBsatx.IsChecked == true)
                sx = true;
            else
                sx = false;
            if (CBsaty.IsChecked == true)
                sy = true;
            else
                sy = false;
            if (CBimportDefault.IsChecked == true)
                importDefault = true;
            else
                importDefault = false;
            MainStructure.BindsFromLocal(selectedSticks ,importDefault, inv, slid, curv, dz, sx, sy);
            Close();
        }

        Grid BaseSetupRelationGrid()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            if (availableJoysticks == null) return null;

            for (int i = 0; i < availableJoysticks.Length; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());

            return grid;
        }

        void ListSticks()
        {
            Grid grid = BaseSetupRelationGrid();
            if (grid == null) return;
            for(int i=0; i<availableJoysticks.Length; ++i)
            {
                CheckBox cbx = new CheckBox();
                cbx.Name = "cbxjy" + i.ToString();
                cbx.Content = availableJoysticks[i];
                cbx.Foreground = Brushes.White;
                cbx.HorizontalAlignment = HorizontalAlignment.Left;
                cbx.VerticalAlignment = VerticalAlignment.Center;
                Thickness margin = cbx.Margin;
                margin.Left = 10;
                cbx.Margin = margin;
                Grid.SetColumn(cbx, 0);
                Grid.SetRow(cbx, i);
                grid.Children.Add(cbx);
                cbx.Click += new RoutedEventHandler(JoystickSetChanged);
            }
            sv.Content = grid;
        }

        void JoystickSetChanged(object sender, EventArgs e)
        {
            CheckBox cx = (CheckBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("cbxjy", ""));
            if (indx < 0 || indx >= availableJoysticks.Length) return;
            string stick = availableJoysticks[indx];
            if (cx.IsChecked == true && !selectedSticks.Contains(stick))
            {
                selectedSticks.Add(stick);
            }else if (cx.IsChecked==false && selectedSticks.Contains(stick))
            {
                selectedSticks.Remove(stick);
            }
        }

    }
}
