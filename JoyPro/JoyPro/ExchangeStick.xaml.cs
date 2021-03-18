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
    /// Interaktionslogik für ExchangeStick.xaml
    /// </summary>
    public partial class ExchangeStick : Window
    {
        string stickToReplace;
        public ExchangeStick(string toReplace)
        {
            InitializeComponent();
            List<string> sticks = JoystickReader.GetConnectedJoysticks();
            DropDownSticks.ItemsSource = sticks;
            JsToReplace.Content = toReplace;
            stickToReplace = toReplace;
            CancelJoyExchange.Click += new RoutedEventHandler(CancelJoystick);
            OKJoyExchange.Click += new RoutedEventHandler(OKNewJoystick);

            if (MainStructure.msave != null&&MainStructure.msave.exchangeW!=null)
            {
                if (MainStructure.msave.exchangeW.Top > 0) this.Top = MainStructure.msave.exchangeW.Top;
                if (MainStructure.msave.exchangeW.Left > 0) this.Left = MainStructure.msave.exchangeW.Left;
                if (MainStructure.msave.exchangeW.Width > 0) this.Width = MainStructure.msave.exchangeW.Width;
                if (MainStructure.msave.exchangeW.Height > 0) this.Height = MainStructure.msave.exchangeW.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }

            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
        }

        void OKNewJoystick(object sender, EventArgs e)
        {
            string selItem = (string)DropDownSticks.SelectedItem;
            if (selItem == null || selItem.Length < 1)
                MessageBox.Show("No Stick selected");
            MainStructure.ExchangeSticksInBind(stickToReplace, selItem);
            Close();
        }

        void CancelJoystick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
