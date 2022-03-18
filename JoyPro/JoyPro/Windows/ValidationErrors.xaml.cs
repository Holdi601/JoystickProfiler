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
    /// 
    public enum ValidationType { Relation, Bind, Modifier, Double, None }
    public partial class ValidationErrors : Window
    {
        public Validation data;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;

        public ValidationErrors()
        {
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

        void renderErrorList(List<string> errs, ScrollViewer sv, ValidationType vt = ValidationType.None)
        {
            Grid relGrid = BaseSetupGrid(errs);
            for (int i = 0; i < errs.Count; ++i)
            {
                Label cbx = new Label();
                cbx.Name = "bxrel" + i.ToString();
                cbx.Content = errs[i];
                cbx.Foreground = Brushes.White;
                cbx.HorizontalAlignment = HorizontalAlignment.Left;
                cbx.VerticalAlignment = VerticalAlignment.Center;
                if (vt == ValidationType.Bind)
                {
                    ContextMenu menu = new ContextMenu();
                    cbx.ContextMenu = menu;
                    string shortenError = errs[i].Substring(44);
                    string[] splitItem = shortenError.Split('§');
                    string Aircraft = splitItem[0];
                    string Rels = splitItem[splitItem.Length - 1].Substring(splitItem[splitItem.Length - 1].IndexOf(":") + 2);
                    string[] RelsSplit = MainStructure.SplitBy(Rels, ", ");
                    foreach(string r in RelsSplit)
                    {
                        MenuItem mi = new MenuItem();
                        mi.Header = "Deactive: " + Aircraft + "->" + r;
                        mi.Click += new RoutedEventHandler(DeactivatePlaneInRelation);
                        menu.Items.Add(mi);

                        MenuItem mib = new MenuItem();
                        mib.Header = "Delete Bind: " + r;
                        mib.Click += new RoutedEventHandler(DeleteBindingFromRelation);
                        menu.Items.Add(mib);
                    }
                }


                Grid.SetColumn(cbx, 0);
                Grid.SetRow(cbx, i);
                relGrid.Children.Add(cbx);
            }
            sv.Content = relGrid;
        }

        void DeactivatePlaneInRelation(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            string rawData = ((string)mi.Header).Substring(10);
            string[] acRel = MainStructure.SplitBy(rawData, "->");
            string game = acRel[0].Substring(0, acRel[0].IndexOf(':'));
            string plane = acRel[0].Substring(acRel[0].IndexOf(':')+1);
            Relation r = InternalDataManagement.AllRelations[acRel[1]];
            r.DeactivateAllAircraftItems(game, plane);
            fillView();
        }

        void DeleteBindingFromRelation(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            string bindname = ((string)mi.Header).Substring(13);
            InternalDataManagement.RemoveBind(InternalDataManagement.GetBindForRelation(bindname));
            fillView();
        }

        void fillView()
        {
            data=new Validation();
            if (data != null && data.BindErrors != null && data.ModifierErrors != null && data.RelationErrors != null && data.DupActiveError != null)
            {
                renderErrorList(data.RelationErrors, svRel);
                renderErrorList(data.BindErrors, svBind, ValidationType.Bind);
                renderErrorList(data.ModifierErrors, svMod);
                renderErrorList(data.DupActiveError, svDup);
            }
        }
    }
}
