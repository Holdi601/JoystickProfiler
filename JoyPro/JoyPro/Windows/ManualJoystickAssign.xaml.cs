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
using System.Text.RegularExpressions;

namespace JoyPro
{
    /// <summary>
    /// Interaktionslogik für ManualJoystickAssign.xaml
    /// </summary>
    public partial class ManualJoystickAssign : Window
    {
        List<string> sticks;
        Relation rel;
        const string joystickRegexPattern = ".+\\{([a-z]|[A-Z]|[0-9]){8}\\-([a-z]|[A-Z]|[0-9]){4}\\-([a-z]|[A-Z]|[0-9]){4}\\-([a-z]|[A-Z]|[0-9]){4}\\-([a-z]|[A-Z]|[0-9]){12}\\}";
        public ManualJoystickAssign(Relation r)
        {
            InitializeComponent();
            sticks = new List<string>();
            rel = r;
            for(int i=0; i<MainStructure.LocalJoysticks.Length; ++i)
            {
                sticks.Add(MainStructure.LocalJoysticks[i]);
            }
            if (MainStructure.msave != null && MainStructure.msave.JoyManAs != null)
            {
                if (MainStructure.msave.JoyManAs.Top > 0) this.Top = MainStructure.msave.JoyManAs.Top;
                if (MainStructure.msave.JoyManAs.Left > 0) this.Left = MainStructure.msave.JoyManAs.Left;
                if (MainStructure.msave.JoyManAs.Width > 0) this.Width = MainStructure.msave.JoyManAs.Width;
                if (MainStructure.msave.JoyManAs.Height > 0) this.Height = MainStructure.msave.JoyManAs.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            updateJoystickList();
            populateButtonList();
            AddJoystickBtn.Click += new RoutedEventHandler(EnterNewJoystick);
            AddJoystickTF.KeyUp += new KeyEventHandler(EnterNewJoystickEnter);
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            ApplyBtn.Click += new RoutedEventHandler(Apply);
        }

        void updateJoystickList()
        {
            JoystickLB.Items.Clear();
            for(int j=0; j<sticks.Count; ++j)
            {
                JoystickLB.Items.Add(sticks[j]);
            }
        }

        void EnterNewJoystickEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EnterNewJoystick(sender, e);
            }
        }

        void Apply(object sender, EventArgs e)
        {

            if (ButtonsLB.SelectedIndex < 0)
            {
                MessageBox.Show("No Button selected");
                return;
            }
            if (JoystickLB.SelectedIndex < 0)
            {
                MessageBox.Show("No joystick selected");
                return;
            }
            MainStructure.LocalJoysticks = sticks.ToArray();
            Bind cr = MainStructure.GetBindForRelation(rel.NAME);
            if (e == null)
            {
                if (cr != null)
                {
                    MainStructure.RemoveBind(cr);
                }
                return;
            }
            if (cr == null)
            {
                cr = new Bind(rel);
                MainStructure.AddBind(cr.Rl.NAME, cr);
            }
            cr.Joystick = (string)JoystickLB.SelectedItem;
            if (rel.ISAXIS)
            {
                cr.JAxis = (string)ButtonsLB.SelectedItem;
            }
            else
            {
                cr.JButton = (string)ButtonsLB.SelectedItem;
            }
            Close();            
        }

        void populateButtonList()
        {
            if (rel.ISAXIS)
            {
                ButtonsLB.Items.Clear();
                ButtonsLB.Items.Add("JOY_X");
                ButtonsLB.Items.Add("JOY_Y");
                ButtonsLB.Items.Add("JOY_Z");
                ButtonsLB.Items.Add("JOY_RX");
                ButtonsLB.Items.Add("JOY_RY");
                ButtonsLB.Items.Add("JOY_RZ");
                ButtonsLB.Items.Add("JOY_SLIDER1");
                ButtonsLB.Items.Add("JOY_SLIDER2");
            }
            else
            {
                ButtonsLB.Items.Clear();
                ButtonsLB.Items.Add("JOY_BTN_POV1_U");
                ButtonsLB.Items.Add("JOY_BTN_POV1_UR");
                ButtonsLB.Items.Add("JOY_BTN_POV1_R");
                ButtonsLB.Items.Add("JOY_BTN_POV1_DR");
                ButtonsLB.Items.Add("JOY_BTN_POV1_D");
                ButtonsLB.Items.Add("JOY_BTN_POV1_DL");
                ButtonsLB.Items.Add("JOY_BTN_POV1_L");
                ButtonsLB.Items.Add("JOY_BTN_POV1_UL");
                ButtonsLB.Items.Add("JOY_BTN_POV2_U");
                ButtonsLB.Items.Add("JOY_BTN_POV2_UR");
                ButtonsLB.Items.Add("JOY_BTN_POV2_R");
                ButtonsLB.Items.Add("JOY_BTN_POV2_DR");
                ButtonsLB.Items.Add("JOY_BTN_POV2_D");
                ButtonsLB.Items.Add("JOY_BTN_POV2_DL");
                ButtonsLB.Items.Add("JOY_BTN_POV2_L");
                ButtonsLB.Items.Add("JOY_BTN_POV2_UL");
                ButtonsLB.Items.Add("JOY_BTN_POV3_U");
                ButtonsLB.Items.Add("JOY_BTN_POV3_UR");
                ButtonsLB.Items.Add("JOY_BTN_POV3_R");
                ButtonsLB.Items.Add("JOY_BTN_POV3_DR");
                ButtonsLB.Items.Add("JOY_BTN_POV3_D");
                ButtonsLB.Items.Add("JOY_BTN_POV3_DL");
                ButtonsLB.Items.Add("JOY_BTN_POV3_L");
                ButtonsLB.Items.Add("JOY_BTN_POV3_UL");
                ButtonsLB.Items.Add("JOY_BTN_POV4_U");
                ButtonsLB.Items.Add("JOY_BTN_POV4_UR");
                ButtonsLB.Items.Add("JOY_BTN_POV4_R");
                ButtonsLB.Items.Add("JOY_BTN_POV4_DR");
                ButtonsLB.Items.Add("JOY_BTN_POV4_D");
                ButtonsLB.Items.Add("JOY_BTN_POV4_DL");
                ButtonsLB.Items.Add("JOY_BTN_POV4_L");
                ButtonsLB.Items.Add("JOY_BTN_POV4_UL");
                for(int i=1; i<129; ++i)
                {
                    ButtonsLB.Items.Add("JOY_BTN" + i.ToString());
                }
            }
        }

        void EnterNewJoystick(object sender, EventArgs e)
        {
            Match isMatch = Regex.Match(AddJoystickTF.Text, joystickRegexPattern);
            if (isMatch.Success)
            {
                if(!MainStructure.ListContainsCaseInsensitive(sticks, AddJoystickTF.Text))
                {
                    sticks.Add(AddJoystickTF.Text);
                    updateJoystickList();
                }
                else
                {
                    MessageBox.Show("Joystick already part of list");
                }
            }
            else
            {
                MessageBox.Show("Joystick name doesn't follow needed format: EXAMPLE JOYSTICK {UUIDHERE-6g6g-g6g6-6g6g-000000000000}");
            }
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }

    }
}
