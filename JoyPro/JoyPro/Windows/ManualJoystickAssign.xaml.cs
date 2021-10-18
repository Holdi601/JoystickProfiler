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
            for(int i=0; i< InternalDataMangement.LocalJoysticks.Length; ++i)
            {
                sticks.Add(InternalDataMangement.LocalJoysticks[i]);
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
            InternalDataMangement.LocalJoysticks = sticks.ToArray();
            Bind cr = InternalDataMangement.GetBindForRelation(rel.NAME);
            if (e == null)
            {
                if (cr != null)
                {
                    InternalDataMangement.RemoveBind(cr);
                }
                return;
            }
            if (cr == null)
            {
                cr = new Bind(rel);
                InternalDataMangement.AddBind(cr.Rl.NAME, cr);
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
