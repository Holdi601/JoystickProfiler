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
    /// Interaction logic for CreatePlaneAlias.xaml
    /// </summary>
    public partial class CreatePlaneAlias : Window
    {
        string originalName;
        string game;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;
        public CreatePlaneAlias(string original, string Game)
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;
            originalName = original;
            game= Game;
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
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            CloseBtn.Click += new RoutedEventHandler(CloseCreatePlaneAlias);
            PlaneOriginalNameLabel.Content = original;
            if (InternalDataManagement.JoystickAliases == null) InternalDataManagement.JoystickAliases = new Dictionary<string, string>();
            if (InternalDataManagement.JoystickAliases.ContainsKey(original)) NewAliasTF.Text = InternalDataManagement.JoystickAliases[original];
            RestoreBtn.Click += new RoutedEventHandler(RestoreOriginal);
            ApplyBtn.Click += new RoutedEventHandler(ApplyChange);
            this.Closing += new System.ComponentModel.CancelEventHandler(ActionsOnClosing);
        }

        void RestoreOriginal(object sender, EventArgs e)
        {
            if(InternalDataManagement.PlaneAliases ==null)InternalDataManagement.PlaneAliases = new Dictionary<string,Dictionary<string, string>>();
            if(!InternalDataManagement.PlaneAliases.ContainsKey(game))InternalDataManagement.PlaneAliases.Add(game, new Dictionary<string, string>());
            if (!InternalDataManagement.PlaneAliases[game].ContainsKey(originalName)) InternalDataManagement.PlaneAliases[game].Add(originalName, "");
            else InternalDataManagement.PlaneAliases[game][originalName] = "";

            CloseCreatePlaneAlias(sender, e);
        }

        void ApplyChange(object sender, EventArgs e)
        {
            if (NewAliasTF.Text.Replace(" ", "").Length < 2 ||
                NewAliasTF.Text.Replace(" ", "") == "None" ||
                NewAliasTF.Text.Replace(" ", "") == "ALL" ||
                NewAliasTF.Text.Replace(" ", "") == "NONE" ||
                NewAliasTF.Text.Replace(" ", "") == "UNASSIGNED" ||
                NewAliasTF.Text.Replace(" ", "").Contains("\"") ||
                NewAliasTF.Text.Replace(" ", "").Contains("\\") ||
                NewAliasTF.Text.Replace(" ", "").Contains(",") )
            {
                MessageBox.Show("Name to short, or reserved name or symbol or already exists");
                return;
            }

            if (InternalDataManagement.PlaneAliases == null) InternalDataManagement.PlaneAliases = new Dictionary<string, Dictionary<string, string>>();
            if (!InternalDataManagement.PlaneAliases.ContainsKey(game)) InternalDataManagement.PlaneAliases.Add(game, new Dictionary<string, string>());
            if (!InternalDataManagement.PlaneAliases[game].ContainsKey(originalName)) InternalDataManagement.PlaneAliases[game].Add(originalName, NewAliasTF.Text);
            else InternalDataManagement.PlaneAliases[game][originalName] = NewAliasTF.Text;

            CloseCreatePlaneAlias(sender, e);
        }

        void ActionsOnClosing(object sender, EventArgs e)
        {
            InternalDataManagement.ResyncRelations();
        }

        void CloseCreatePlaneAlias(object sender, EventArgs e)
        {
            MainStructure.SaveMetaLast();
            Close();
        }
    }
}
