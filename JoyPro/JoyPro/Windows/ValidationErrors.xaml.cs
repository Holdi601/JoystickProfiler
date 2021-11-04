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
    /// Interaktionslogik für ValidationErrors.xaml
    /// </summary>
    public partial class ValidationErrors : Window
    {
        public Validation data;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;

        public ValidationErrors(Validation error)
        {
            data = error;
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;

            if (MainStructure.msave != null && MainStructure.msave._ValidationWindow != null)
            {
                if (MainStructure.msave._ValidationWindow.Top > 0) this.Top = MainStructure.msave._ValidationWindow.Top;
                if (MainStructure.msave._ValidationWindow.Left > 0) this.Left = MainStructure.msave._ValidationWindow.Left;
                if (MainStructure.msave._ValidationWindow.Width > 0) this.Width = MainStructure.msave._ValidationWindow.Width;
                if (MainStructure.msave._ValidationWindow.Height > 0) this.Height = MainStructure.msave._ValidationWindow.Height;
            }

            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);

            fillView();
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }

        Grid BaseSetupGrid(List<string> ErrorList)
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            ColumnDefinition c = new ColumnDefinition();
            grid.ColumnDefinitions.Add(c);
            for (int i = 0; i < ErrorList.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());
            return grid;
        }

        void fillView()
        {
            if (data != null && data.BindErrors != null && data.ModifierErrors != null && data.RelationErrors != null)
            {
                Grid relGrid = BaseSetupGrid(data.RelationErrors);
                for(int i=0; i<data.RelationErrors.Count; ++i)
                {
                    Label cbx = new Label();
                    cbx.Name = "bxrel" + i.ToString();
                    cbx.Content = data.RelationErrors[i];
                    cbx.Foreground = Brushes.White;
                    cbx.HorizontalAlignment = HorizontalAlignment.Left;
                    cbx.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(cbx, 0);
                    Grid.SetRow(cbx, i);
                    relGrid.Children.Add(cbx);
                }
                svRel.Content = relGrid;

                Grid bindGrid = BaseSetupGrid(data.BindErrors);
                for(int i=0; i<data.BindErrors.Count; ++i)
                {
                    Label cbx = new Label();
                    cbx.Name = "brel" + i.ToString();
                    cbx.Content = data.BindErrors[i];
                    cbx.Foreground = Brushes.White;
                    cbx.HorizontalAlignment = HorizontalAlignment.Left;
                    cbx.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(cbx, 0);
                    Grid.SetRow(cbx, i);
                    bindGrid.Children.Add(cbx);
                }
                svBind.Content = bindGrid;

                Grid modGrid = BaseSetupGrid(data.ModifierErrors);
                for(int i=0; i<data.ModifierErrors.Count; ++i)
                {
                    Label cbx = new Label();
                    cbx.Name = "rel" + i.ToString();
                    cbx.Content = data.ModifierErrors[i];
                    cbx.Foreground = Brushes.White;
                    cbx.HorizontalAlignment = HorizontalAlignment.Left;
                    cbx.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(cbx, 0);
                    Grid.SetRow(cbx, i);
                    modGrid.Children.Add(cbx);
                }
                svMod.Content = modGrid;

            }
        }
    }
}
