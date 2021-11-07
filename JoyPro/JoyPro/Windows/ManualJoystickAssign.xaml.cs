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
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;

        const string joystickRegexPattern = ".+\\{([a-z]|[A-Z]|[0-9]){8}\\-([a-z]|[A-Z]|[0-9]){4}\\-([a-z]|[A-Z]|[0-9]){4}\\-([a-z]|[A-Z]|[0-9]){4}\\-([a-z]|[A-Z]|[0-9]){12}\\}";
        public ManualJoystickAssign(Relation r)
        {
            InitializeComponent();

            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;

            sticks = new List<string>();
            rel = r;
            for(int i=0; i< InternalDataManagement.LocalJoysticks.Length; ++i)
            {
                sticks.Add(InternalDataManagement.LocalJoysticks[i]);
            }
            if (MainStructure.msave != null && MainStructure.msave._JoystickManualAssignWindow != null)
            {
                if (MainStructure.msave._JoystickManualAssignWindow.Top > 0) this.Top = MainStructure.msave._JoystickManualAssignWindow.Top;
                if (MainStructure.msave._JoystickManualAssignWindow.Left > 0) this.Left = MainStructure.msave._JoystickManualAssignWindow.Left;
                if (MainStructure.msave._JoystickManualAssignWindow.Width > 0) this.Width = MainStructure.msave._JoystickManualAssignWindow.Width;
                if (MainStructure.msave._JoystickManualAssignWindow.Height > 0) this.Height = MainStructure.msave._JoystickManualAssignWindow.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            updateJoystickList();
            ButtonsLB.Items.Clear();
            ButtonsLB.ItemsSource = JoystickReader.GetAllPossibleStickInputs();
            AddJoystickBtn.Click += new RoutedEventHandler(EnterNewJoystick);
            AddJoystickTF.KeyUp += new KeyEventHandler(EnterNewJoystickEnter);
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            ApplyBtn.Click += new RoutedEventHandler(Apply);

            this.Title = rel.NAME + " - Manual Input Assignment";
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
            InternalDataManagement.LocalJoysticks = sticks.ToArray();
            Bind cr = InternalDataManagement.GetBindForRelation(rel.NAME);
            if (e == null)
            {
                if (cr != null)
                {
                    InternalDataManagement.RemoveBind(cr);
                }
                return;
            }
            if (cr == null)
            {
                cr = new Bind(rel);
                InternalDataManagement.AddBind(cr.Rl.NAME, cr);
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
