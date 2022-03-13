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
    public partial class StickMention : Window
    {
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;
        List<string> sticks;

        public StickMention()
        {
            InitializeComponent();
            sticks = InternalDataManagement.GetAllMentionSticks();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;
            if (MainStructure.msave != null && MainStructure.msave._JoystickMentionWindow != null)
            {
                if (MainStructure.msave._JoystickMentionWindow.Top > 0) this.Top = MainStructure.msave._JoystickMentionWindow.Top;
                if (MainStructure.msave._JoystickMentionWindow.Left > 0) this.Left = MainStructure.msave._JoystickMentionWindow.Left;
                if (MainStructure.msave._JoystickMentionWindow.Width > 0) this.Width = MainStructure.msave._JoystickMentionWindow.Width;
                if (MainStructure.msave._JoystickMentionWindow.Height > 0) this.Height = MainStructure.msave._JoystickMentionWindow.Height;
            }
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);

            ListSticks();
        }


        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }

        void DeleteAllStickMentionsFromJoyProAndDCS(object sender, EventArgs e)
        {
            int joyToDelete = Convert.ToInt32(((Button)sender).Name.Replace("b",""));
            bool deleteFiles = false;
            deleteFiles=deleteFilesCB.IsChecked==true?true:false;
            InternalDataManagement.DeleteAllReferencesOfJoystick(sticks[joyToDelete], deleteFiles);
            sticks = InternalDataManagement.GetAllMentionSticks();
            ListSticks();
        }

        void ListSticks()
        {
            Grid g = BaseGrid();
            for(int i=0; i<sticks.Count; i++)
            {
                Label lbl = new Label();
                lbl.Name = "lbl" + i.ToString();
                lbl.Content = sticks[i];
                lbl.Foreground = Brushes.White;
                lbl.HorizontalAlignment = HorizontalAlignment.Left;
                lbl.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(lbl, 0);
                Grid.SetRow(lbl, i);
                g.Children.Add(lbl);

                Button dltBtn = new Button();
                dltBtn.Name = "b" + i.ToString();
                dltBtn.Content = "Delete";
                dltBtn.Click += new RoutedEventHandler(DeleteAllStickMentionsFromJoyProAndDCS);
                dltBtn.HorizontalAlignment = HorizontalAlignment.Right;
                dltBtn.VerticalAlignment = VerticalAlignment.Center;
                dltBtn.Width = 50;
                Grid.SetColumn(dltBtn, 1);
                Grid.SetRow(dltBtn, i);
                g.Children.Add(dltBtn);
            }
            g.ShowGridLines = true;
            sv.Content = g;
        }


        Grid BaseGrid()
        {
            int gridCols = 2;
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
