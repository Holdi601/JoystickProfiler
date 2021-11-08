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
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public Label[] shownLabels;
        public Grid mGrid;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;

        public OverlayWindow()
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;
            if (MainStructure.msave != null && MainStructure.msave._OverlayWindow != null)
            {
                if (MainStructure.msave._OverlayWindow.Top > 0) this.Top = MainStructure.msave._OverlayWindow.Top;
                if (MainStructure.msave._OverlayWindow.Left > 0) this.Left = MainStructure.msave._OverlayWindow.Left;
                if (MainStructure.msave._OverlayWindow.Width > 0) this.Width = MainStructure.msave._OverlayWindow.Width;
                if (MainStructure.msave._OverlayWindow.Height > 0) this.Height = MainStructure.msave._OverlayWindow.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }
            shownLabels = new Label[MainStructure.msave.OvlElementsToShow+2];
            this.Deactivated += new EventHandler(Window_Deactivated);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(LMBDown);
            this.PreviewKeyUp += new KeyEventHandler(KBHandler);
            sv.MouseLeftButtonDown += new MouseButtonEventHandler(LMBDown);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            MoveLabel.MouseLeftButtonDown += new MouseButtonEventHandler(LMBDown);
            mGrid = setupMainGrid();
            setupLabels();
            sv.Content= mGrid;
        }
        void setupLabels()
        {
            if (MainStructure.msave.OvldebugMode)
            {
                shownLabels[0] = new Label();
                shownLabels[0].FontSize = MainStructure.msave.OvlTxtS;
                shownLabels[0].Foreground = MainStructure.msave.ColorSCB;
                shownLabels[0].FontFamily = new FontFamily(MainStructure.msave.Font);
                shownLabels[0].HorizontalAlignment = HorizontalAlignment.Left;
                shownLabels[0].VerticalAlignment = VerticalAlignment.Center;
                shownLabels[0].Content = "Current Game: \tCurrent Plane: ";
                shownLabels[0].MouseLeftButtonDown += new MouseButtonEventHandler(LMBDown);
                Grid.SetColumn(shownLabels[0], 0);
                Grid.SetRow(shownLabels[0], 0);
                mGrid.Children.Add(shownLabels[0]);
            }
            for(int i=1; i< MainStructure.msave.OvlElementsToShow+1; i++)
            {
                shownLabels[i] = new Label();
                shownLabels[i].FontSize = MainStructure.msave.OvlTxtS;
                shownLabels[i].Foreground = MainStructure.msave.ColorSCB;
                shownLabels[i].FontFamily = new FontFamily(MainStructure.msave.Font);
                shownLabels[i].HorizontalAlignment = HorizontalAlignment.Left;
                shownLabels[i].VerticalAlignment = VerticalAlignment.Center;
                shownLabels[i].Content = "";
                shownLabels[i].MouseLeftButtonDown += new MouseButtonEventHandler(LMBDown);
                Grid.SetColumn(shownLabels[i], 0);
                Grid.SetRow(shownLabels[i], i);
                mGrid.Children.Add(shownLabels[i]);
            }
        }
        Grid setupMainGrid()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            for(int i = 0; i<MainStructure.msave.OvlElementsToShow; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            return grid;
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        void LMBDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
            
        }

        void KBHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
