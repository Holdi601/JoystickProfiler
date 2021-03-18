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
        public StickToExchange(List<string> sticks)
        {
            InitializeComponent();
            if (MainStructure.msave != null)
            {
                if(MainStructure.msave.stick2ExW.Top>0) this.Top = MainStructure.msave.stick2ExW.Top;
                if (MainStructure.msave.stick2ExW.Left > 0) this.Left = MainStructure.msave.stick2ExW.Left;
                if(MainStructure.msave.stick2ExW.Width > 0)this.Width = MainStructure.msave.stick2ExW.Width;
                if(MainStructure.msave.stick2ExW.Height > 0)this.Height = MainStructure.msave.stick2ExW.Height;
            }

            CancelJoyExchange.Click += new RoutedEventHandler(CancelButton);
            DDJoysticks.ItemsSource = sticks;
        }

        void CancelButton(object sender, EventArgs e)
        {
            Close();
        }

        void OkExchangeNow(object sender, EventArgs e)
        {

        }
    }
}
