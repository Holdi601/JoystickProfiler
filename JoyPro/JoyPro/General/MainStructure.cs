using System;
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

namespace JoyPro
{

    //IL2 steam path registry 
    //Computer\HKEY_CURRENT_USER\System\GameConfigStore\Children\92e042fb-d93a-46df-8872-04039ab6d802
    //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 307960
    public enum Game { DCS, StarCitizen }
    public enum JoystickAxis { JOY_X, JOY_Y, JOY_Z, JOY_RX, JOY_RY, JOY_RZ, JOY_SLIDER1, JOY_SLIDER2, NONE }
    public enum LuaDataType { String, Number, Dict, Bool, Error };

    public enum SortType { NAME_NORM, NAME_DESC, STICK_NORM, STICK_DESC, BTN_NORM, BTN_DESC }

    public enum ModExists { NOT_EXISTENT, BINDNAME_EXISTS, KEYBIND_EXISTS, ALL_EXISTS, ERROR }
    public static class MainStructure
    {
        const int version = 49;
        public static MainWindow mainW;
        public static string PROGPATH;
        public static MetaSave msave = null;
        
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
        public static void AfterLoad()
        {
            string pth = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\JoyPro";
            if (File.Exists(pth + "\\last.pr0file"))
            {
                InternalDataMangement.LoadProfile(pth + "\\last.pr0file");
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
            WriteToBinaryFile<MetaSave>(pth + "\\meta.info", msave);
            InternalDataMangement.SaveProfileTo(pth + "\\last.pr0file");
        }
        public static void SaveWindowState(object sender, EventArgs e)
        {
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            if (sender != null && sender is Window)
            {
                WindowPos p = GetWindowPosFrom((Window)sender);
                if (sender is MainWindow)
                {
                    msave.mainWLast = p;
                }
                else if (sender is RelationWindow)
                {
                    msave.relationWindowLast = p;
                }
                else if (sender is ExchangeStick)
                {
                    msave.exchangeW = p;
                }
                else if (sender is ImportWindow)
                {
                    msave.importWindowLast = p;
                }
                else if (sender is ModifierManager)
                {
                    msave.ModifierW = p;
                }
                else if (sender is StickToExchange)
                {
                    msave.stick2ExW = p;
                }
                else if (sender is StickSettings)
                {
                    msave.SettingsW = p;
                }
                else if (sender is ValidationErrors)
                {
                    msave.ValidW = p;
                }
                else if (sender is ReinstateBackup)
                {
                    msave.BackupW = p;
                }
                else if (sender is UserCurveDCS)
                {
                    msave.UsrCvW = p;
                }
                else if(sender is GroupManagerW)
                {
                    msave.GrpMngr = p;
                }else if(sender is CreateJoystickAlias)
                {
                    msave.AliasCr = p;
                }else if(sender is ManualJoystickAssign)
                {
                    msave.JoyManAs = p;
                }
            }
            if (mainW.CBNukeUnused.IsChecked == true)
                msave.NukeSticks = true;
            else if (mainW.CBNukeUnused.IsChecked == false)
                msave.NukeSticks = false;
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
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
            }
        }
        public static void InitProgram()
        {
            if (Updater.GetNewestVersionNumber() > version)
            {
                MessageBoxResult mr = MessageBox.Show("A newer version is available, if you press yes, it will download in the background (Dont close the program please), it will close itself once its done.", "Newer Version Available", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mr == MessageBoxResult.Yes)
                {
                    Updater.DownloadNewerVersion();
                }
            }
            if (!MiscGames.Games.Contains("DCS"))
            {
                MiscGames.Games.Add("DCS");
                DBLogic.Planes.Add("DCS", new List<string>());
            }
            if (!MiscGames.Games.Contains("IL2Game"))
            {
                MiscGames.Games.Add("IL2Game");
                DBLogic.Planes.Add("IL2Game", new List<string>());
            }
                
            MiscGames.DCSInstances = InitGames.GetDCSUserFolders();
            for (int i = 0; i < MiscGames.DCSInstances.Length; ++i)
            {
                MiscGames.BackupConfigsOfDCSInstance(MiscGames.DCSInstances[i]);
            }
            //IL2 Backup needed
            InitGames.LoadIL2Path();
            MiscGames.BackupConfigsOfIL2();
        }
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
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
                }
                if (key != null)
                {
                    string currentKey = key.GetValue(Value, true).ToString();
                    return currentKey;
                }
            }
            catch
            {

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
        public static void DeleteFolder(string folder)
        {
            DirectoryInfo dd = new DirectoryInfo(folder);
            FileInfo[] fi = dd.GetFiles();
            for (int i = 0; i < fi.Length; i++)
            {
                File.Delete(fi[i].FullName);
            }
            DirectoryInfo[] dirs = dd.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                DeleteFolder(dirs[i].FullName);
            }
            Directory.Delete(folder);
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
