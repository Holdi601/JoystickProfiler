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
        public double DCSGuiScale;
        public Point DCSScreenpos;
        public bool OriginalFullscreen;
        int modulesToScan = 35;

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
            ModulesToScanBox.Text = modulesToScan.ToString();
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
            ModulesToScanBox.LostFocus += new RoutedEventHandler(objectsToScanChanged);
            readDCSConfigData();
        }
        void objectsToScanChanged(object sender, EventArgs e)
        {
            if (int.TryParse((string)ModulesToScanBox.Text, out modulesToScan))
            {

            }
            else
            {
                MessageBox.Show("Not a valid integer");
                ModulesToScanBox.Text = modulesToScan.ToString();
            }
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
                if (line.Contains("[\"scaleGui\"]"))
                {
                    DCSGuiScale = Convert.ToDouble(line.Substring(line.IndexOf('=') + 1).Replace(",", ""), new CultureInfo("en-US"));
                }
                else if (line.Contains("[\"fullScreen\"]"))
                {
                    string content = line.Substring(line.IndexOf("=") + 1).Replace(",", "").Trim();
                    if (content == "false") OriginalFullscreen = false;
                    else OriginalFullscreen = true;
                }
            }
            sr.Close();
            sr.Dispose();

        }
        void turnFullScreen(bool val)
        {
            string path;
            if (MiscGames.DCSInstanceOverride.Length > 0&&Directory.Exists((MiscGames.DCSInstanceOverride))) path = MiscGames.DCSInstanceOverride;
            else path = MiscGames.DCSselectedInstancePath;
            string output = "";
            path = path + "\\Config\\options.lua";
            if (!File.Exists(path)) return;
            StreamReader sr = new StreamReader(path);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Contains("[\"fullScreen\"]"))
                {
                    string currentline = line.Substring(0, line.IndexOf('=') + 1);
                    if (val) currentline = currentline + " true,\n";
                    else currentline = currentline + " false,\n";
                }
                else
                {
                    output = output + line + "\n";
                }
            }
            sr.Close();
            sr.Dispose();
            StreamWriter swr = new StreamWriter(path);
            swr.Write(output);
            swr.Flush();
            swr.Dispose();
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
            else if (DCSInstanceORBox.Text.Length > 0)
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
            else if (IL2InstanceORBox.Text.Length > 0)
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
                MiscGames.installPathDCSOR = installPathBox.Text;
            }
            else
            {
                MiscGames.installPathDCSOR = "";
                installPathBox.Text = InitGames.GetDCSInstallationPath();
                MessageBox.Show("Folder doesn't exist");
            }
        }
        void ChangeTimeToSet(object sender, EventArgs e)
        {
            int val;
            bool succ = int.TryParse(ttsBox.Text, out val);
            if (succ && val >= 0)
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
            if (succ && val >= 0 && val < 65536)
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

            MessageBoxResult mr = MessageBox.Show("Are you sure that you want to refresh the clean DB?" +
                "In Order to get the clean Profiles, all your current bindings will be deleted!!! If you still want to Proceed, press yes. Another Window opens which asks you whether the Main menu has loaded. Once the Mainmenu has loaded press yes." +
                "DCS does not allow for CLI command communication, thus the programm will take over your mouse and will do some automated clicks. Your selected instance must match with the DCS that you run otherwise this will lead to corrupt Data. Please dont touch your mouse and keyboard in the meantime." +
                "DCS can't be in Fullscreen mode! Once the process is done, you will be notified. For the affects to take change, please restart JoyPro.This Operation will take 1-2 Minutes Thanks. ", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (mr == MessageBoxResult.Yes)
            {
                turnFullScreen(false);
                string executeDCS = InitGames.GetDCSInstallationPath() + "\\bin\\DCS_updater.exe";
                StartProgram(executeDCS);
                bool inMenu = false;
                while (!inMenu)
                {
                    MessageBoxResult mrop = MessageBox.Show("Is it in the Menu?", "Is it in the Menu?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (mrop == MessageBoxResult.Yes)
                    {
                        inMenu = true;
                    }
                }

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
                turnFullScreen(OriginalFullscreen);
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
                " If you still want to Proceed Press yes. DCS will start and another pop up of the program will open asking you if the Main menu has loaded. Once its loaded press yes." +
                "DCS does not allow for CLI command communication, thus the programm will take over your mouse and will do some automated clicks.Your selected instance must match with the DCS that you run otherwise this will lead to corrupt Data. Please dont touch your mouse and keyboard in the meantime." +
                " Once the process is done, you will be notified. For the affects to take change, please restart JoyPro. This Operation will take 5-10 Minutes. Thanks. ", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (mr == MessageBoxResult.Yes)
            {
                turnFullScreen(false);
                string executeDCS = InitGames.GetDCSInstallationPath() + "\\bin\\DCS_updater.exe";
                StartProgram(executeDCS);
                bool inMenu = false;
                while (!inMenu)
                {
                    MessageBoxResult mrop = MessageBox.Show("Is it in the Menu?", "Is it in the Menu?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (mrop == MessageBoxResult.Yes)
                    {
                        inMenu = true;
                    }
                }

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
                ClickCenterAnchored(DCSRect, -554, -280);
                int planesNeeded = CountInstalledCrafts();
                for (int i = 0; i < planesNeeded*2; ++i)
                {
                    ArrowUp();
                    Thread.Sleep(50);
                }
                Thread.Sleep(5000);
                for (int i = 0; i < planesNeeded; ++i)
                {
                    ClickCenterAnchored(DCSRect, 310, 313);
                    Thread.Sleep(2000);
                    BringProcessToFront(DCSproc);
                    Thread.Sleep(500);
                    ClickCenterAnchored(DCSRect, -554, -280);
                    Thread.Sleep(500);
                    ArrowDown();
                }
                DCSproc.Kill();
                Thread.Sleep(5000);
                PostWorkExportedIDs();
                turnFullScreen(OriginalFullscreen);
                MessageBox.Show("Done. Please restart JoyPro now");
            }
        }
        void PostWorkExportedIDs()
        {
            string path;
            if (MiscGames.DCSInstanceOverride.Length > 0 && Directory.Exists((MiscGames.DCSInstanceOverride))) path = MiscGames.DCSInstanceOverride;
            else path = MiscGames.DCSselectedInstancePath;
            path = path + "\\InputLayoutsTxt";
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Something went very wrong, the htmls are not in the selected Instance");
                return;
            }
            string transferTo = MainStructure.PROGPATH + "\\DB\\DCS\\";
            DirectoryInfo dirParent = new DirectoryInfo(path);
            DirectoryInfo[] planes = dirParent.GetDirectories();
            for(int i=0; i<planes.Length; ++i)
            {
                FileInfo[] allFiles = planes[i].GetFiles();
                string planeName = planes[i].Name;
                FileInfo largest = null;
                for(int j=0; j<allFiles.Length; ++j)
                {
                    if ((largest == null &&allFiles[j].Name.ToLower()!="keyboard.html"&& allFiles[j].Name.ToLower() != "mouse.html"&& allFiles[j].Name.ToLower() != "trackir.html") ||
                        (largest!=null&&largest.Length<allFiles[j].Length&& allFiles[j].Name.ToLower() != "keyboard.html" && allFiles[j].Name.ToLower() != "mouse.html" && allFiles[j].Name.ToLower() != "trackir.html"))
                    {
                        largest = allFiles[j];
                    }
                }
                if (largest != null)
                {
                    File.Copy(largest.FullName, transferTo + planeName + ".html",true);
                }
            }
            try
            {
                MainStructure.DeleteFolder(path);
            }
            catch
            {

            }
            
        }
        int CountInstalledCrafts()
        {
            int number = modulesToScan;
            string installedCrafts = InitGames.GetDCSInstallationPath() + "\\Mods\\aircraft";
            string modCrafts;
            if (MiscGames.DCSInstanceOverride.Length > 0 && Directory.Exists((MiscGames.DCSInstanceOverride))) modCrafts = MiscGames.DCSInstanceOverride;
            else modCrafts = MiscGames.DCSselectedInstancePath;
            modCrafts = modCrafts + "\\Mods\\aircraft";
            if (Directory.Exists(installedCrafts))
            {
                DirectoryInfo dir = new DirectoryInfo(installedCrafts);
                number = number + dir.GetDirectories().Length;
            }
            if (Directory.Exists(modCrafts))
            {
                DirectoryInfo dir = new DirectoryInfo(modCrafts);
                number = number + dir.GetDirectories().Length;
            }
            return number;
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
            startInfo.Arguments = x.ToString() + " " + y.ToString();
            Process.Start(startInfo);
        }
        void ArrowUp()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(MainStructure.PROGPATH + "\\TOOLS\\AHK\\ArrowKeyUp.exe");
            Process.Start(startInfo);
        }
        void ArrowDown()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(MainStructure.PROGPATH + "\\TOOLS\\AHK\\ArrowKeyDown.exe");
            Process.Start(startInfo);
        }
        void StartProgram(string prog)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(prog);
            Process.Start(startInfo);
        }
        void recreateCleanFile(string instance)
        {
            instance = instance + "\\Config\\Input";
            string fileToWrite = MainStructure.PROGPATH + "\\CleanProfile\\DCS\\clean.cf";
            if (File.Exists(fileToWrite) && !File.Exists(MainStructure.PROGPATH + "\\CleanProfile\\DCS\\backup.cf"))
            {
                File.Move(fileToWrite, MainStructure.PROGPATH + "\\CleanProfile\\DCS\\backup.cf");
            }
            StreamWriter writer = new StreamWriter(fileToWrite);
            DirectoryInfo dirInf = new DirectoryInfo(instance);
            DirectoryInfo[] allSubs = dirInf.GetDirectories();
            for (int i = 0; i < allSubs.Length; ++i)
            {
                writer.WriteLine("####################" + allSubs[i].Name);
                if (Directory.Exists(allSubs[i].FullName + "\\joystick"))
                {
                    DirectoryInfo planeFolder = new DirectoryInfo(allSubs[i].FullName + "\\joystick");
                    FileInfo[] allFiles = planeFolder.GetFiles();
                    FileInfo largest = null;
                    for (int j = 0; j < allFiles.Length; ++j)
                    {
                        if ((largest == null&& allFiles[j].Name.EndsWith(".diff.lua")) ||
                            (largest.Length < allFiles[j].Length && allFiles[j].Name.EndsWith(".diff.lua")))
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
