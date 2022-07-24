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
    /// Interaktionslogik für StickToExchange.xaml
    /// </summary>
    public partial class StickToExchange : Window
    {
        List<string> Joysticks = new List<string>();
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;
        string filep = null;

        public StickToExchange(List<string> sticks, string filePath=null)
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;

            if (MainStructure.msave != null && MainStructure.msave._StickExchangeWindow != null)
            {
                if (MainStructure.msave._StickExchangeWindow.Top > 0) this.Top = MainStructure.msave._StickExchangeWindow.Top;
                if (MainStructure.msave._StickExchangeWindow.Left > 0) this.Left = MainStructure.msave._StickExchangeWindow.Left;
                if (MainStructure.msave._StickExchangeWindow.Width > 0) this.Width = MainStructure.msave._StickExchangeWindow.Width;
                if (MainStructure.msave._StickExchangeWindow.Height > 0) this.Height = MainStructure.msave._StickExchangeWindow.Height;
            }

            CancelJoyExchange.Click += new RoutedEventHandler(CancelButton);
            Joysticks = sticks;
            if (InternalDataManagement.JoystickAliases == null) InternalDataManagement.JoystickAliases = new Dictionary<string, string>();
            for (int i = 0; i < sticks.Count; ++i)
            {
                if (InternalDataManagement.JoystickAliases.ContainsKey(sticks[i]) && InternalDataManagement.JoystickAliases[sticks[i]].Length > 0)
                {
                    DDJoysticks.Items.Add(InternalDataManagement.JoystickAliases[sticks[i]]);
                }
                else
                {
                    DDJoysticks.Items.Add(sticks[i]);
                }
            }
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            if(filePath!=null&&filePath.Length>0)
            {
                this.Title = "Select which stick to save";
                filep = filePath;
                OKJoyExchange.Click += new RoutedEventHandler(SaveStickProfile);
            }
            else
            {
                OKJoyExchange.Click += new RoutedEventHandler(OkExchangeNow);
            }
        }

        void CancelButton(object sender, EventArgs e)
        {
            Close();
        }
        void SaveStickProfile(object sender, EventArgs e)
        {
            if (DDJoysticks.SelectedItem != null || ((string)DDJoysticks.SelectedItem).Length > 0)
            {
                InternalDataManagement.SaveProfileOfStickTo(filep, Joysticks[DDJoysticks.SelectedIndex]);
                Close();
            }
            else
            {
                MessageBox.Show("No stick selected");
                return;
            }
        }
        void OkExchangeNow(object sender, EventArgs e)
        {
            if (DDJoysticks.SelectedItem != null || ((string)DDJoysticks.SelectedItem).Length > 0)
            {
                ExchangeStick exs = new ExchangeStick(Joysticks[DDJoysticks.SelectedIndex]);
                exs.Closing += new System.ComponentModel.CancelEventHandler(CancelButton);
                exs.Show();
            }
            else
            {
                MessageBox.Show("No stick selected");
                return;
            }
            DDJoysticks.IsEnabled = false;
            CancelJoyExchange.IsEnabled = false;
            OKJoyExchange.IsEnabled = false;
        }
    }
}
