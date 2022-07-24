using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CouldNotFindModifier.xaml
    /// </summary>
    public partial class CouldNotFindModifier : Window
    {
        List<Modifier> modifiers;
        string modifierName;
        string device;
        JoystickReader jr;
        public CouldNotFindModifier(List<Modifier> existingMods, string deviceNotFound, string dnfInMod)
        {
            modifiers = existingMods;
            modifierName= dnfInMod;
            device = deviceNotFound;
            for(int i = 0; i < modifiers.Count; i++)
            {
                DropDownMods.Items.Add(modifiers[i].name);
            }
            InitializeComponent();
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            ContinueBtn.Click += new RoutedEventHandler(ContinueAndReplaceWithSelected);
            AssignBtn.Click += new RoutedEventHandler(AcquireNewMod);
        }

        void ContinueAndReplaceWithSelected(object sender, EventArgs e)
        {
            if (DropDownMods.SelectedIndex >= 0)
            {
                InternalDataManagement.ReplaceModifier(modifierName, modifiers[DropDownMods.SelectedIndex].device, modifiers[DropDownMods.SelectedIndex].key);
            }
            else
            {
                return;
            }
            Close();
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }
        void listenMod(object sender, EventArgs e)
        {
            jr = new JoystickReader(false, true);
        }

        void AcquireNewMod(object sender, EventArgs e)
        {
            CloseBtn.IsEnabled = false;
            ContinueBtn.IsEnabled = false;
            DropDownMods.IsEnabled = false;
            if (AssignBtn != null)
                AssignBtn.Content = "Click now Mod Button";
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(listenMod);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(modReplaced);
            bw.RunWorkerAsync();
        }

        void modReplaced(object sender, EventArgs e)
        {
            CloseBtn.IsEnabled = true;
            ContinueBtn.IsEnabled = true;
            DropDownMods.IsEnabled = true;
            if (AssignBtn != null)
                AssignBtn.Content = "Assign";
            if (jr == null)
            {
                MessageBox.Show("Something went wrong when setting a modifier. Either listener was not started correctly or the main button was not assigend beforehand.");
                return;
            }
            if (jr.result == null)
            {
                return;
            }
            InternalDataManagement.ReplaceModifier(modifierName, Bind.JoystickGuidToModifierGuid(jr.result.Device), jr.result.AxisButton);
            Close();
        }
    }
}
