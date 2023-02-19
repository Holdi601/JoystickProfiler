﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using SlimDX.DirectInput;
using System.Windows;
using System.Net;
using Microsoft.Win32;
using System.Net.Http;
using System.Web.Hosting;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Security.Cryptography;

namespace JoyPro
{
    public enum Game { DCS, StarCitizen, IL2 }
    public enum JoystickAxis { JOY_X, JOY_Y, JOY_Z, JOY_RX, JOY_RY, JOY_RZ, JOY_SLIDER1, JOY_SLIDER2, NONE }
    public enum LuaDataType { String, Number, Dict, Bool, Error };
    public enum SortType { NAME_NORM, NAME_DESC, STICK_NORM, STICK_DESC, BTN_NORM, BTN_DESC }
    public enum ModExists { NOT_EXISTENT, BINDNAME_EXISTS, KEYBIND_EXISTS, ALL_EXISTS, ERROR }
    public enum PlaneActivitySelection { Relation, Import, Export, View, Error }

    public struct CurrentBind
    {
        public bool found;
        public string joystick;
        public string modbtn;

        public CurrentBind(Relation r)
        {
            if (InternalDataManagement.AllBinds.ContainsKey(r.NAME))
            {
                found = true;
                joystick = InternalDataManagement.AllBinds[r.NAME].Joystick;
                string toCompare;
                if (r.ISAXIS)
                {
                    toCompare = InternalDataManagement.AllBinds[r.NAME].JAxis;
                }
                else
                {
                    toCompare = InternalDataManagement.AllBinds[r.NAME].JButton;
                }
                string prefix = "";
                if (InternalDataManagement.AllBinds[r.NAME].AllReformers.Count > 0)
                {
                    InternalDataManagement.AllBinds[r.NAME].AllReformers.Sort();
                    prefix = InternalDataManagement.AllBinds[r.NAME].AllReformers[0].Substring(0, InternalDataManagement.AllBinds[r.NAME].AllReformers[0].IndexOf('§'));
                    for (int i = 1; i < InternalDataManagement.AllBinds[r.NAME].AllReformers.Count; ++i)
                    {
                        prefix = prefix + "+" + InternalDataManagement.AllBinds[r.NAME].AllReformers[i].Substring(0, InternalDataManagement.AllBinds[r.NAME].AllReformers[i].IndexOf('§'));
                    }
                    prefix = prefix + "+";
                }
                toCompare = prefix + toCompare;
                modbtn = toCompare;
            }
            else
            {
                found = false;
                joystick = null;
                modbtn = null;
            }
        }
    }
    public partial class App:Application
    {
        private void Start(object sender, StartupEventArgs e)
        {
            if(e.Args!=null)
            {
                MainStructure.Write("Startup Args:");
                for(int i = 0; i < e.Args.Length; i++)
                {
                    MainStructure.Write(e.Args[i]);
                    if (e.Args[i].Length > 0 && e.Args[i].StartsWith("-"))
                    {
                        switch(e.Args[i].ToLower())
                        {
                            case "-nokeyboard":MainStructure.loadKeyboard = false;break;
                            default:break;
                        }
                    }
                }
                MainStructure.Write("End of Args");
            }
        }
    }
    public static class MainStructure
    {
        public static string LogFile = "\\log";
        public const int version = 87;
        public static MainWindow mainW;
        public static string PROGPATH;
        public static MetaSave msave = null;
        public static JoystickReader JrContReading = null;
        public static RelationJumper rlJumper = null;
        public static Thread runningGameCheck = null;
        public static Thread joystickInputRead = null;
        public static Thread joystickInputReadCon = null;
        public static Thread DCSServerSocket = null;
        public static Thread DisplayBackgroundWorker = null;
        public static Thread DisplayDispatcherWorker = null;
        public static Thread HotkeyThread = null;
        public static Thread RelationJumper = null;
        public static OverlayBackGroundWorker OverlayWorker = null;
        public static bool VisualMode = false;
        public static int VisualLayer = 0;
        public static double ScaleFactor = 1.0;
        public static bool MainWindowActive = false;
        public static bool MainWindowTextActive = false;
        public static bool JoystickReadActive = false;
        static int fileDeleteFailureMax = 10;
        static int fileDeleteFailure = 0;
        static int fileDeleteTimeOut = 1000;
        public static bool loadKeyboard = true;


        public static void NoteError(Exception e)
        {
            Write(e.ToString());
            Write(e.Message);
            Write(e.Source);
            Write(e.StackTrace);
        }

        public static void Write(string msg)
        {
            msg = "[" + DateTime.UtcNow.ToString() + "]: " + "\t" + msg;
            System.Diagnostics.Debug.WriteLine(msg);
            int maxTries = 100;
            int currentTries = 0;
            while (true)
            {
                try
                {
                    File.AppendAllText(Environment.CurrentDirectory + LogFile, msg + "\r\n");
                    break;
                }
                catch
                {
                    Console.WriteLine("Could not write log file");
                    currentTries++;
                    Thread.Sleep(100);
                }
                if (currentTries > maxTries) break;
            }
        }

        public static void MainWActivated(object sender, EventArgs e)
        {
            MainWindowActive = true;
        }
        public static void MainWDeactivated(object sender, EventArgs e)
        {
            MainWindowActive = false;
        }
        public static void LoadMetaLast()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            if (File.Exists(pth + "\\meta.info"))
            {
                try
                {
                    msave = ReadFromBinaryFile<MetaSave>(pth + "\\meta.info");
                }
                catch
                {
                    msave = new MetaSave();
                }
            }
            else
            {
                msave = new MetaSave();
            }
        }
        public static void WriteCrashInfo(object sender, UnhandledExceptionEventArgs e)
        {
            PROGPATH = Environment.CurrentDirectory;
            DateTime dtNow = DateTime.UtcNow;
            StreamWriter swr = new StreamWriter(PROGPATH + "\\" + dtNow.ToString("yyyyMMddHHmmss") + ".UnhandledException");
            if (e != null)
            {
                swr.WriteLine(e.ToString());
                swr.WriteLine("#");
                if(e.ExceptionObject!=null)swr.WriteLine(e.ExceptionObject.ToString());
            }
            
            swr.Close();
            swr.Dispose();
        }
        public static void WriteCrashInfoDisp(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            PROGPATH = Environment.CurrentDirectory;
            DateTime dtNow = DateTime.UtcNow;
            StreamWriter swr = new StreamWriter(PROGPATH + "\\" + dtNow.ToString("yyyyMMddHHmmss") + ".DispUnhandledException");
            if (e != null)
            {   
                swr.WriteLine(e.ToString());
                swr.WriteLine("#");
                if (e.Exception != null)
                {
                    NoteError(e.Exception);
                    if(e.Exception.Message != null) swr.WriteLine(e.Exception.Message);
                    swr.WriteLine("#");
                    if (e.Exception.StackTrace != null) swr.WriteLine(e.Exception.StackTrace);
                    swr.WriteLine("#");
                    if (e.Exception.Data != null) swr.WriteLine(e.Exception.Data.ToString());
                    swr.WriteLine("#");
                    if (e.Exception.InnerException != null)
                    {
                        if(e.Exception.InnerException.Message!=null) swr.WriteLine(e.Exception.InnerException.Message);
                        swr.WriteLine("#");
                        if (e.Exception.InnerException.StackTrace != null) swr.WriteLine(e.Exception.InnerException.StackTrace.ToString());
                        swr.WriteLine("#");
                        if (e.Exception.InnerException.Data != null) swr.WriteLine(e.Exception.InnerException.Data.ToString());
                    }
                }
            }
            
            swr.Close();
            swr.Dispose();
        }
        public static void AfterLoad()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            if (File.Exists(pth + "\\last.pr0file"))
            {
                InternalDataManagement.LoadProfile(pth + "\\last.pr0file");
            }
        }
        public static WindowPos GetWindowPosFrom(Window w)
        {
            if (double.IsNaN(w.Height) ||
                double.IsNaN(w.Width) ||
                double.IsNaN(w.Top) ||
                double.IsNaN(w.Left))
                return null;
            WindowPos wp = new WindowPos();
            wp.Height = w.Height;
            wp.Width = w.Width;
            wp.Top = w.Top;
            wp.Left = w.Left;
            return wp;
        }
        public static void SaveMetaLast()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            if (!Directory.Exists(pth))
            {
                Directory.CreateDirectory(pth);
            }
            WriteToBinaryFile(pth + "\\meta.info", msave);
            InternalDataManagement.SaveProfileTo(pth + "\\last.pr0file");
        }
        public static void SaveWindowState(object sender, EventArgs e)
        {
            if (msave == null) msave = new MetaSave();
            if (sender != null && sender is Window)
            {
                WindowPos p = GetWindowPosFrom((Window)sender);
                if (sender is MainWindow)
                {
                    msave._MainWindow = p;
                }
                else if (sender is RelationWindow)
                {
                    msave._RelationWindow = p;
                }
                else if (sender is ExchangeStick)
                {
                    msave._ExchangeWindow = p;
                }
                else if (sender is ImportWindow)
                {
                    msave._ImportWindow = p;
                }
                else if (sender is ModifierManager)
                {
                    msave._ModifierWindow = p;
                }
                else if (sender is StickToExchange)
                {
                    msave._StickExchangeWindow = p;
                }
                else if (sender is StickSettings)
                {
                    msave._SettingsWindow = p;
                }
                else if (sender is ValidationErrors)
                {
                    msave._ValidationWindow = p;
                }
                else if (sender is ReinstateBackup)
                {
                    msave._BackupWindow = p;
                }
                else if (sender is UserCurveDCS)
                {
                    msave._UserCurveWindow = p;
                }
                else if(sender is GroupManagerW)
                {
                    msave._GroupManagerWindow = p;
                }else if(sender is CreateJoystickAlias)
                {
                    msave._AliasWindow = p;
                }else if(sender is ManualJoystickAssign)
                {
                    msave._JoystickManualAssignWindow = p;
                }else if(sender is OverlaySettings)
                {
                    msave._OverlaySettingsWindow = p;
                }
                else if (sender is OverlayWindow)
                {
                    msave._OverlayWindow = p;
                }else if(sender is MassModification)
                {
                    msave._MassOperationWindow = p;
                }else if(sender is StickMention)
                {
                    msave._JoystickMentionWindow = p;
                }else if(sender is PlanesToExport)
                {
                    msave._ExportWindow = p;
                }
            }
            if (mainW.CBNukeUnused.IsChecked == true)
                msave.NukeSticks = true;
            else if (mainW.CBNukeUnused.IsChecked == false)
                msave.NukeSticks = false;
            if (mainW.CBExportOnlyView.IsChecked == true)
                msave.ExportInView = true;
            else if(mainW.CBExportOnlyView.IsChecked==false)
                msave.ExportInView = false;
            SaveMetaLast();
        }
        public static string ShortenDeviceName(string device)
        {
            if (!device.Contains("{")) return null;
            return device.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
        }
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            if (filePath == null || filePath.Length < 1) return;
            try
            {
                using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
                {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    binaryFormatter.Serialize(stream, objectToWrite);
                }
             }
            catch(Exception e)
            {
                MainStructure.NoteError(e);
            }
        }
        public static void StartThreads()
        {
            OverlayWorker = new OverlayBackGroundWorker();
            runningGameCheck = new Thread(OverlayWorker.GameRunningCheck);
            DCSServerSocket = new Thread(OverlayWorker.StartDCSListener);
            JrContReading = new JoystickReader();
            JrContReading.KeepDaemonRunning = true;
            HotkeyThread = new Thread(JrContReading.HotKeyRead);
            rlJumper = new RelationJumper();
            RelationJumper = new Thread(rlJumper.StartRelationJumper);
            if (msave == null || msave._OverlaySettingsWindow == null) msave = new MetaSave();
            HotkeyThread.Name = "HotKeyReader";
            runningGameCheck.Name = "GameRunCheck";
            DCSServerSocket.Name = "DCSServerSocket";
            RelationJumper.Name = "Relation Jumper";
            runningGameCheck.Start();
            DCSServerSocket.Start();
            HotkeyThread.Start();
            RelationJumper.Start();
        }
        
        public static byte[] GetFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    return sha256.ComputeHash(fileStream);
                }
            }
        }
        public static void InitProgram()
        {
            Write("Check Newer Version");
            if (Updater.GetNewestVersionNumber() > version)
            {
                MessageBoxResult mr = MessageBox.Show("A newer version is available, if you press yes, it will download in the background (Dont close the program please), You will be notified once its done.", "Newer Version Available", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mr == MessageBoxResult.Yes)
                {
                    Updater.DownloadNewerVersion();
                }
            }
            Write("Adding DCS to games");
            if (!MiscGames.Games.Contains("DCS"))
            {
                MiscGames.Games.Add("DCS");
                DBLogic.Planes.Add("DCS", new List<string>());
            }
            Write("Adding IL2 to games");
            if (!MiscGames.Games.Contains("IL2Game"))
            {
                MiscGames.Games.Add("IL2Game");
                DBLogic.Planes.Add("IL2Game", new List<string>());
            }
            Write("Adding SC to games");
            if (!MiscGames.Games.Contains("StarCitizen"))
            {
                MiscGames.Games.Add("StarCitizen");
                DBLogic.Planes.Add("StarCitizen", new List<string>());
            }
            Write("GET DCS User Fodlers");
            MiscGames.DCSInstances = InitGames.GetDCSUserFolders();
            for (int i = 0; i < MiscGames.DCSInstances.Length; ++i)
            {
                Write("Backup User DCS config: "+ MiscGames.DCSInstances[i]);
                MiscGames.BackupConfigsOfDCSInstance(MiscGames.DCSInstances[i]);
            }
            Write("Start Other Threads");
            StartThreads();
            //IL2 Backup needed
            Write("Load IL2 Path");
            IL2IOLogic.LoadIL2Path();
            Write("Load SC Path");
            InitGames.LoadStarCitizenPath();
            Write("Backup IL2");
            MiscGames.BackupConfigsOfIL2();
            Write("Backup SC");
            MiscGames.BackupConfigsOfSC();
            Write("Load last meta");
            LoadMetaLast();
        }
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }

        public static string SearchKey(string Locality, string keyname, string data, string valueToFind)
        {
            RegistryKey key = null;
            switch (Locality)
            {
                case "CurrentUser":
                    key = Registry.CurrentUser.OpenSubKey(keyname);
                    break;
                case "LocalMachine":
                    key = Registry.LocalMachine.OpenSubKey(keyname);
                    break;
                case "ClassesRoot":
                    key = Registry.ClassesRoot.OpenSubKey(keyname);
                    break;
            }
            var programs = key.GetSubKeyNames();
            if (programs == null || programs.Length < 1) return null;
            foreach (var program in programs)
            {
                RegistryKey subkey = key.OpenSubKey(program);
                string val = (string)subkey.GetValue(data);
                if(val == null||!val.Contains(valueToFind)) continue;
                return val;
            }

            return null;
        }
        public static string GetRegistryValue(string Path, string Value, string Locality)
        {
            try
            {
                RegistryKey key = null;
                switch (Locality)
                {
                    case "CurrentUser":
                        key = Registry.CurrentUser.OpenSubKey(Path);
                        break;
                    case "LocalMachine":
                        key = Registry.LocalMachine.OpenSubKey(Path);
                        break;
                    case "ClassesRoot":
                        key =Registry.ClassesRoot.OpenSubKey(Path);
                        break;
                }
                if (key != null)
                {
                    string currentKey = key.GetValue(Value, true).ToString();
                    return currentKey;
                }
            }
            catch(Exception ex)
            {
                MainStructure.NoteError(ex);
            }
            return null;
        }
        public static void CopyFolderIntoFolder(string source, string dest)
        {
            string[] splitt = source.Split('\\');
            string last_part = splitt[splitt.Length - 1];
            if (!Directory.Exists(dest + "\\" + last_part)) Directory.CreateDirectory(dest + "\\" + last_part);
            DirectoryInfo sor = new DirectoryInfo(source);
            FileInfo[] all_files = sor.GetFiles();
            for (int i = 0; i < all_files.Length; i++)
            {
                File.Copy(all_files[i].FullName, dest + "\\" + last_part + "\\" + all_files[i].Name, true);
            }

            DirectoryInfo[] all_dirs = sor.GetDirectories();
            for (int i = 0; i < all_dirs.Length; i++)
            {
                CopyFolderIntoFolder(all_dirs[i].FullName, dest + "\\" + last_part);
            }
        }
        public static void DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
                fileDeleteFailure = 0;
            }
            catch (Exception ex)
            {
                if (fileDeleteFailure < fileDeleteFailureMax)
                {
                    fileDeleteFailure++;
                    Thread.Sleep(fileDeleteTimeOut);
                    DeleteFile(file);
                }
                else
                {
                    MainStructure.NoteError(ex);
                }
            }

            
        }
        public static void DeleteFolder(string folder)
        {
            try
            {
                DirectoryInfo dd = new DirectoryInfo(folder);
                FileInfo[] fi = dd.GetFiles();
                for (int i = 0; i < fi.Length; i++)
                {
                    DeleteFile(fi[i].FullName);
                }
                DirectoryInfo[] dirs = dd.GetDirectories();
                for (int i = 0; i < dirs.Length; i++)
                {
                    DeleteFolder(dirs[i].FullName);
                }
                Directory.Delete(folder);
                fileDeleteFailure = 0;
            }
            catch (Exception ex)
            {
                if (fileDeleteFailure < fileDeleteFailureMax)
                {
                    fileDeleteFailure++;
                    Thread.Sleep(fileDeleteTimeOut);
                    DeleteFolder(folder);
                }
                else
                {
                    MainStructure.NoteError(ex);
                }
            }
            
        }
        public static bool ListContainsCaseInsensitive(List<string> li, string toCheck)
        {
            for (int i = 0; i < li.Count; ++i)
            {
                if (li[i].Replace(" ", "").ToUpper() == toCheck.Replace(" ", "").ToUpper()) return true;
            }
            return false;
        }
        public static string[] SplitBy(string toSplit, string splitValue)
        {
            List<string> result = new List<string>();
            string temp="";
            int startIndex = 0;
            if (toSplit.IndexOf(splitValue) < 0) return new string[1] { toSplit };
            while (startIndex >= 0)
            {
                int length = toSplit.IndexOf(splitValue);
                temp = toSplit.Substring(0, length);
                toSplit = toSplit.Substring(temp.Length + splitValue.Length);
                result.Add(temp);
                startIndex = toSplit.IndexOf(splitValue);
            }
            result.Add(toSplit);

            return result.ToArray();
        }
    }
}
