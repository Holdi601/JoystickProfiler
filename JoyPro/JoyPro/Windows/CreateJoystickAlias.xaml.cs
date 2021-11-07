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
    /// Interaktionslogik für CreateJoystickAlias.xaml
    /// </summary>
    public partial class CreateJoystickAlias : Window
    {
        string originalName;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;
        public CreateJoystickAlias(string original)
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;
            originalName = original;
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
            CloseBtn.Click += new RoutedEventHandler(CloseCreateJoystickAlias);
            DeviceOriginalNameLabel.Content = original;
            if (InternalDataManagement.JoystickAliases == null) InternalDataManagement.JoystickAliases = new Dictionary<string, string>();
            if (InternalDataManagement.JoystickAliases.ContainsKey(original)) NewAliasTF.Text = InternalDataManagement.JoystickAliases[original];
            RestoreBtn.Click += new RoutedEventHandler(RestoreOriginal);
            ApplyBtn.Click += new RoutedEventHandler(ApplyChange);
        }

        void RestoreOriginal(object sender, EventArgs e)
        {
            if (InternalDataManagement.JoystickAliases.ContainsKey(originalName))
            {
                InternalDataManagement.JoystickAliases[originalName] = "";
            }
            CloseCreateJoystickAlias(sender, e);
        }

        void ApplyChange(object sender, EventArgs e)
        {
            if(NewAliasTF.Text.Replace(" ","").Length<2|| 
                NewAliasTF.Text.Replace(" ", "") == "None"||
                NewAliasTF.Text.Replace(" ", "") == "ALL" ||
                NewAliasTF.Text.Replace(" ", "") == "NONE" ||
                NewAliasTF.Text.Replace(" ", "") == "UNASSIGNED" ||
                NewAliasTF.Text.Replace(" ", "").Contains("\"") ||
                NewAliasTF.Text.Replace(" ", "").Contains("\\") ||
                NewAliasTF.Text.Replace(" ", "").Contains(",") ||
                InternalDataManagement.DoesJoystickAliasExist(NewAliasTF.Text))
            {
                MessageBox.Show("Name to short, or reserved name or symbol or already exists");
                return;
            }
            if (InternalDataManagement.JoystickAliases.ContainsKey(originalName))
            {
                InternalDataManagement.JoystickAliases[originalName] = NewAliasTF.Text;
            }
            else
            {
                InternalDataManagement.JoystickAliases.Add(originalName, NewAliasTF.Text);
            }
            CloseCreateJoystickAlias(sender, e);
        }

        void CloseCreateJoystickAlias(object sender, EventArgs e)
        {
            MainStructure.SaveMetaLast();
            Close();
        }
    }
}
