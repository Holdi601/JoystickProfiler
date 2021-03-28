using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaktionslogik für StickSettings.xaml
    /// </summary>
    public partial class StickSettings : Window
    {
        public StickSettings()
        {
            InitializeComponent();
            if (MainStructure.msave != null && MainStructure.msave.SettingsW != null)
            {
                if (MainStructure.msave.SettingsW.Top > 0) this.Top = MainStructure.msave.SettingsW.Top;
                if (MainStructure.msave.SettingsW.Left > 0) this.Left = MainStructure.msave.SettingsW.Left;
                if (MainStructure.msave.SettingsW.Width > 0) this.Width = MainStructure.msave.SettingsW.Width;
                if (MainStructure.msave.SettingsW.Height > 0) this.Height = MainStructure.msave.SettingsW.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }

            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            ttsBox.Text = MainStructure.msave.timeToSet.ToString();
            pollitBox.Text = MainStructure.msave.pollWaitTime.ToString();
            itBox.Text = MainStructure.msave.warmupTime.ToString();
            athBox.Text = MainStructure.msave.axisThreshold.ToString();
            BackupDaysBox.Text = MainStructure.msave.backupDays.ToString();
            string installPath = MainStructure.GetInstallationPath();
            if (installPath != null)
            {
                installPathBox.Text = installPath;
            }

            ttsBox.LostFocus += new RoutedEventHandler(ChangeTimeToSet);
            pollitBox.LostFocus += new RoutedEventHandler(ChangePollTime);
            itBox.LostFocus += new RoutedEventHandler(ChangeWarmUp);
            athBox.LostFocus += new RoutedEventHandler(ChangeAxisThreshhold);
            installPathBox.LostFocus += new RoutedEventHandler(SetInstallPath);
            BackupDaysBox.LostFocus += new RoutedEventHandler(changeBackupDays);
        }

        void changeBackupDays(object sender, EventArgs e)
        {
            int days = 90;
            bool? succ = int.TryParse(BackupDaysBox.Text, out days);
            if (succ == false || succ == null)
            {
                MessageBox.Show("Not a valid integer for backup days");
                BackupDaysBox.Text = MainStructure.msave.backupDays.ToString();
            }
            MainStructure.msave.backupDays = days;
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }

        void SetInstallPath(object sender, EventArgs e)
        {
            if (Directory.Exists(installPathBox.Text))
            {
                MainStructure.installPaths = new string[1];
                MainStructure.installPaths[0] = installPathBox.Text;
            }
            else
            {
                MessageBox.Show("Folder doesn't exist");
            }
        }

        void ChangeTimeToSet(object sender, EventArgs e)
        {
            int val;
            bool succ = int.TryParse(ttsBox.Text, out val);
            if (succ&&val>=0)
            {
                MainStructure.msave.timeToSet = val;
            }
            else
            {
                MessageBox.Show("Time to Set not a valid integer");
                ttsBox.Text = MainStructure.msave.timeToSet.ToString();
            }
        }

        void ChangePollTime(object sender, EventArgs e)
        {
            int val;
            bool succ = int.TryParse(pollitBox.Text, out val);
            if (succ && val >= 0)
            {
                MainStructure.msave.pollWaitTime = val;
            }
            else
            {
                MessageBox.Show("Time to Set not a valid integer");
                pollitBox.Text = MainStructure.msave.pollWaitTime.ToString();
            }
        }

        void ChangeWarmUp(object sender, EventArgs e)
        {
            int val;
            bool succ = int.TryParse(itBox.Text, out val);
            if (succ && val >= 0)
            {
                MainStructure.msave.warmupTime = val;
            }
            else
            {
                MessageBox.Show("Time to Set not a valid integer");
                itBox.Text = MainStructure.msave.warmupTime.ToString();
            }
        }

        void ChangeAxisThreshhold(object sender, EventArgs e)
        {
            int val;
            bool succ = int.TryParse(athBox.Text, out val);
            if (succ && val >= 0&& val<65536)
            {
                MainStructure.msave.axisThreshold = val;
            }
            else
            {
                MessageBox.Show("Time to Set not a valid integer");
                athBox.Text = MainStructure.msave.axisThreshold.ToString();
            }
        }
    }
}
