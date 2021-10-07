using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        public double DCSScreenWidth;
        public double DCSScreenHeight;
        public double DCSGuiScale;
        public Point DCSScreenpos;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public static void BringProcessToFront(Process process)
        {
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle))
            {
                ShowWindow(handle, SW_RESTORE);
            }

            SetForegroundWindow(handle);
        }

        const int SW_RESTORE = 9;

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

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
            string installPath = InitGames.GetDCSInstallationPath();
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
            IL2InstanceORBox.LostFocus += new RoutedEventHandler(ChangeIL2InstanceOverridePath);
            DCSInstanceORBox.LostFocus += new RoutedEventHandler(ChangeDCSInstanceOverridePath);
            RefreshDCSCleanBtn.Click += new RoutedEventHandler(RefreshCleanDB);
            RefreshDCSIdBtn.Click += new RoutedEventHandler(RefreshIDDB);
            readDCSConfigData();
        }
        void readDCSConfigData()
        {
            string path;
            if (MiscGames.DCSInstanceOverride.Length > 0) path = MiscGames.DCSInstanceOverride;
            else path = MiscGames.DCSselectedInstancePath;
            path = path + "\\Config\\options.lua";
            if (!File.Exists(path)) return;
            StreamReader sr = new StreamReader(path);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Contains("[\"height\"]"))
                {
                    DCSScreenHeight = Convert.ToDouble(line.Substring(line.IndexOf('=') + 1).Replace(",", ""), new CultureInfo("en-US"));
                }else if (line.Contains("[\"width\"]"))
                {
                    DCSScreenWidth = Convert.ToDouble(line.Substring(line.IndexOf('=') + 1).Replace(",", ""), new CultureInfo("en-US"));
                }
                else if (line.Contains("[\"scaleGui\"]"))
                {
                    DCSGuiScale = Convert.ToDouble(line.Substring(line.IndexOf('=') + 1).Replace(",", ""), new CultureInfo("en-US"));
                }
            }
            sr.Close();
            sr.Dispose();

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
        void ChangeDCSInstanceOverridePath(object sender, EventArgs e)
        {
            if (Directory.Exists(DCSInstanceORBox.Text))
            {
                MiscGames.DCSInstanceOverride = DCSInstanceORBox.Text;
                MainStructure.mainW.DropDownInstanceSelection.Items.Clear();
                InitGames.ReloadGameData();
                MainStructure.mainW.DropDownInstanceSelection.SelectedIndex = 0;
                MiscGames.DCSInstanceSelectionChanged(MiscGames.DCSInstanceOverride);

            }
            else if(DCSInstanceORBox.Text.Length>0)
            {
                MessageBox.Show("Invalid Path in DCS Instance");
                MiscGames.DCSInstanceOverride = "";
            }
            else
            {
                MiscGames.DCSInstanceOverride = "";
                InitGames.ReloadGameData();
            }
            readDCSConfigData();
        }
        void ChangeIL2InstanceOverridePath(object sender, EventArgs e)
        {
            if (Directory.Exists(IL2InstanceORBox.Text))
            {
                MiscGames.IL2PathOverride = IL2InstanceORBox.Text;
                MiscGames.IL2Instance = IL2InstanceORBox.Text;
                InitGames.ReloadGameData();
            }
            else if(IL2InstanceORBox.Text.Length>0)
            {
                MessageBox.Show("Invalid Path in IL2 Path");
            }
            else
            {
                MiscGames.IL2PathOverride = "";
                InitGames.ReloadGameData();
            }
        }
        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }
        void SetInstallPath(object sender, EventArgs e)
        {
            if (Directory.Exists(installPathBox.Text))
            {
                MiscGames.installPathsDCS = new string[1];
                MiscGames.installPathsDCS[0] = installPathBox.Text;
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
        void RefreshCleanDB(object sender, EventArgs e)
        {
            string path;
            if (MiscGames.DCSInstanceOverride != null && Directory.Exists(MiscGames.DCSInstanceOverride))
            {
                path = MiscGames.DCSInstanceOverride;
            }else if (Directory.Exists(MiscGames.DCSselectedInstancePath))
            {
                path = MiscGames.DCSselectedInstancePath;
            }
            else
            {
                MessageBox.Show("Incorrect selected instance path");
                path = "";
                return;
            }
            MessageBoxResult mr = MessageBox.Show("Are you sure that you want to refresh the clean DB?" +
                "In Order to get the clean Profiles, all your current bindings will be deleted!!! If you still want to Proceed, please start DCS and go into the Main Menu. Only once you are ready please press Yes." +
                "DCS does not allow for CLI command communication, thus the programm will take over your mouse and will do some automated clicks. Your selected instance must match with the DCS that you run otherwise this will lead to corrupt Data. Please dont touch your mouse and keyboard in the meantime." +
                "Once the process is done, you will be notified. For the affects to take change, please restart JoyPro.This Operation will take 1-2 Minutes Thanks. ", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (mr == MessageBoxResult.Yes)
            {
                Process[] processes = Process.GetProcessesByName("DCS");
                if (processes == null || processes.Length < 1)
                {
                    MessageBox.Show("DCS was not open.Cancelling.");
                    return;
                }
                Process DCSproc = processes[0];
                IntPtr ptr = DCSproc.MainWindowHandle;
                Rect DCSRect = new Rect();
                GetWindowRect(ptr, ref DCSRect);
                BringProcessToFront(DCSproc);
                Thread.Sleep(200);
                ClickIntoControlsDCS(DCSRect);
                Thread.Sleep(1000);
                ClickCenterAnchored(DCSRect, 312, -279);
                Thread.Sleep(1000);
                ClickCenterAnchored(DCSRect, -182, -76);
                Thread.Sleep(1000);
                ClickCenterAnchored(DCSRect, -58, 169);
                Thread.Sleep(30000);
                DCSproc.Kill();
                recreateCleanFile(path);
                MessageBox.Show("Done. Please restart JoyPro now.");
            }
        }
        void RefreshIDDB(object sender, EventArgs e)
        {
            string path;
            if (MiscGames.DCSInstanceOverride != null && Directory.Exists(MiscGames.DCSInstanceOverride))
            {
                path = MiscGames.DCSInstanceOverride;
            }
            else if (Directory.Exists(MiscGames.DCSselectedInstancePath))
            {
                path = MiscGames.DCSselectedInstancePath;
            }
            else
            {
                MessageBox.Show("Incorrect selected instance path");
                path = "";
                return;
            }
            MessageBoxResult mr = MessageBox.Show("Are you sure that you want to refresh the ID DB?" +
                " If you still want to Proceed, please start DCS and go into the Main Menu. Only once you are ready please press Yes." +
                "DCS does not allow for CLI command communication, thus the programm will take over your mouse and will do some automated clicks.Your selected instance must match with the DCS that you run otherwise this will lead to corrupt Data. Please dont touch your mouse and keyboard in the meantime." +
                "Once the process is done, you will be notified. For the affects to take change, please restart JoyPro. This Operation will take 5-10 Minutes. Thanks. ", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (mr == MessageBoxResult.Yes)
            {
                Process[] processes = Process.GetProcessesByName("DCS");
                if (processes == null || processes.Length < 1)
                {
                    MessageBox.Show("DCS was not open.Cancelling.");
                    return;
                }
                Process DCSproc = processes[0];
                IntPtr ptr = DCSproc.MainWindowHandle;
                Rect DCSRect = new Rect();
                GetWindowRect(ptr, ref DCSRect);
                BringProcessToFront(DCSproc);
                Thread.Sleep(200);
                ClickIntoControlsDCS(DCSRect);
            }
        }
        void ClickCenterAnchored(Rect DCSRect, int xOffset, int yOffset)
        {
            if (DCSRect.Right - DCSRect.Left >= 2558)
            {
                xOffset = Convert.ToInt32(Convert.ToDouble(xOffset) * DCSGuiScale);
                yOffset = Convert.ToInt32(Convert.ToDouble(yOffset) * DCSGuiScale);
            }
            int xSettingPos = DCSRect.Left + xOffset + ((DCSRect.Right - DCSRect.Left) / 2);
            int ySettingPos = DCSRect.Top + yOffset + ((DCSRect.Bottom - DCSRect.Top) / 2);
            MouseClickThere(xSettingPos, ySettingPos);
        }
        void ClickTopLeftAnchored(Rect DCSRect, int xOffset, int yOffset)
        {
            if (DCSRect.Right - DCSRect.Left >= 2558)
            {
                xOffset = Convert.ToInt32(Convert.ToDouble(xOffset) * DCSGuiScale);
                yOffset = Convert.ToInt32(Convert.ToDouble(yOffset) * DCSGuiScale);
            }
            int xSettingPos = DCSRect.Left + xOffset;
            int ySettingPos;
            if (DCSRect.Top > 20) ySettingPos = DCSRect.Top + yOffset + 30;
            else ySettingPos = DCSRect.Top + yOffset;
            MouseClickThere(xSettingPos, ySettingPos);
        }
        void ClickIntoControlsDCS(Rect DCSRect)
        {
            //Options Click
            ClickTopLeftAnchored(DCSRect, 564, 26);
            Thread.Sleep(1000);
            //Controls Click
            Thread.Sleep(1000);
            ClickCenterAnchored(DCSRect, -365, -318);
        }
        void MouseClickThere(int x, int y)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(MainStructure.PROGPATH + "\\TOOLS\\AHK\\GoToMouseAndClick.exe");
            startInfo.Arguments = x.ToString()+" "+y.ToString();
            Process.Start(startInfo);
        }
        void recreateCleanFile(string instance)
        {
            instance = instance + "\\Config\\Input";
            string fileToWrite = MainStructure.PROGPATH + "\\CleanProfile\\DCS\\clean.cf";
            if (File.Exists(fileToWrite)&&!File.Exists(MainStructure.PROGPATH + "\\CleanProfile\\DCS\\backup.cf"))
            {
                File.Move(fileToWrite, MainStructure.PROGPATH + "\\CleanProfile\\DCS\\backup.cf");
            }
            StreamWriter writer = new StreamWriter(fileToWrite);
            DirectoryInfo dirInf = new DirectoryInfo(instance);
            DirectoryInfo[] allSubs = dirInf.GetDirectories();
            for(int i=0; i< allSubs.Length; ++i)
            {
                writer.WriteLine("####################" + allSubs[i].Name);
                if (Directory.Exists(allSubs[i].FullName+ "\\joystick"))
                {
                    DirectoryInfo planeFolder = new DirectoryInfo(allSubs[i].FullName + "\\joystick");
                    FileInfo[] allFiles = planeFolder.GetFiles();
                    FileInfo largest = null;
                    for(int j=0; j<allFiles.Length; ++j)
                    {
                        if (largest == null || (largest.Length < allFiles[j].Length && allFiles[j].Name.EndsWith(".diff.lua")))
                        {
                            largest = allFiles[j];
                        }
                    }
                    if (largest != null)
                    {
                        StreamReader reader = new StreamReader(largest.FullName);
                        while (!reader.EndOfStream)
                        {
                            writer.WriteLine(reader.ReadLine());
                        }
                        reader.Close();
                        reader.Dispose();
                    }
                }
            }
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }

    }
}
