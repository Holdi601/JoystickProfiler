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
        public string stickToReplace;
        List<string> Joysticks;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;
        public static string ManualStick = "Manual Stick Addition";

        public ExchangeStick(string toReplace)
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;
            Dictionary<string, string> cSticks = JoystickReader.GetConnectedJoysticks();
            List<string> sticks = new List<string>();
            foreach(KeyValuePair<string, string> kvp in cSticks) sticks.Add(kvp.Key);
            Joysticks = sticks;
            if (InternalDataManagement.JoystickAliases == null) InternalDataManagement.JoystickAliases = new Dictionary<string, string>();
            for (int i = 0; i < sticks.Count; ++i)
            {
                if (InternalDataManagement.JoystickAliases.ContainsKey(sticks[i]) && InternalDataManagement.JoystickAliases[sticks[i]].Length > 0)
                {
                    DropDownSticks.Items.Add(InternalDataManagement.JoystickAliases[sticks[i]]);
                }
                else
                {
                    DropDownSticks.Items.Add(sticks[i]);
                }
            }
            DropDownSticks.Items.Add(ManualStick);
            JsToReplace.Content = toReplace;
            stickToReplace = toReplace;
            CancelJoyExchange.Click += new RoutedEventHandler(CancelJoystick);
            OKJoyExchange.Click += new RoutedEventHandler(OKNewJoystick);

            if (MainStructure.msave != null&&MainStructure.msave._ExchangeWindow!=null)
            {
                if (MainStructure.msave._ExchangeWindow.Top > 0) this.Top = MainStructure.msave._ExchangeWindow.Top;
                if (MainStructure.msave._ExchangeWindow.Left > 0) this.Left = MainStructure.msave._ExchangeWindow.Left;
                if (MainStructure.msave._ExchangeWindow.Width > 0) this.Width = MainStructure.msave._ExchangeWindow.Width;
                if (MainStructure.msave._ExchangeWindow.Height > 0) this.Height = MainStructure.msave._ExchangeWindow.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }

            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            this.Topmost = true;
            this.Activate();
            this.Loaded += new RoutedEventHandler(loadedEvent);
            DropDownSticks.SelectionChanged+= new SelectionChangedEventHandler(StickSelected);
        }

        void StickSelected(object sender, SelectionChangedEventArgs e)
        {
            if (DropDownSticks.SelectedIndex >= 0)
            {
                OKJoyExchange.IsEnabled = true;
            }
        }

        void loadedEvent(object sender, EventArgs e)
        {
            this.Topmost = true;
            this.Activate();
        }
        void OKNewJoystick(object sender, EventArgs e)
        {
            string selItem = "";
            if(DropDownSticks.SelectedIndex<Joysticks.Count)
            {
                selItem = Joysticks[DropDownSticks.SelectedIndex];
            }
            else
            {
                selItem = (string)DropDownSticks.SelectedItem;
            }
            
            if (selItem == null || selItem.Length < 1)
                MessageBox.Show("No Stick selected");
            else if(selItem==ManualStick)
            {
                ManualJoystickAssign mja = new ManualJoystickAssign(true);
                mja.Show();
                mja.Closing += new System.ComponentModel.CancelEventHandler(Finalize);
            }
            else
            {
                Finalize(selItem, null);
            }
        }

        void Finalize(object sender, EventArgs e)
        {
            string selItem="";
            if(sender.GetType()==typeof(string))
            {
                selItem = (string)sender;
            }
            else
            {
                ManualJoystickAssign mja = (ManualJoystickAssign)sender;
                selItem = mja.SelectedStick;
            }
            InternalDataManagement.ExchangeSticksInBind(stickToReplace, selItem);
            InternalDataManagement.ExchangeStickInModifiers(stickToReplace, selItem);
            if (InternalDataManagement.OpenedExchangedSticks.Contains(stickToReplace))
            {
                InternalDataManagement.OpenedExchangedSticks.Remove(stickToReplace);
            }
            //Replace stick in modifiers
            Close();
        }

        void CancelJoystick(object sender, EventArgs e)
        {
            if (InternalDataManagement.OpenedExchangedSticks.Contains(stickToReplace))
            {
                InternalDataManagement.OpenedExchangedSticks.Remove(stickToReplace);
            }
            Close();
        }
    }
}
