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
using System.Text.RegularExpressions;

namespace JoyPro
{
    public static class MiscGames
    {
        const string runningBackupFolder = "\\Config\\JP_Backup";
        const string subInputPath = "\\Config";
        const string inputFolderName = "\\Input";
        const string initialBackupFolder = "\\Config\\JP_InitBackup";
        public static string SaveGamesPath;
        public static string[] DCSInstances;
        public static string DCSselectedInstancePath = "";
        public static string[] installPathsDCS;
        public static List<string> Games = new List<string>();
        public static string IL2Instance = "";
        public static string StarCitizen = "";
        

        public static string IL2JoyIdToDCSJoyId(string guid, string device)
        {
            guid = guid.Replace("%22", "");
            device = device.Replace("%20", " ");
            string[] guidParts = guid.Split('-');
            string output = device + " {" + guidParts[0].ToUpper();
            for (int i = 1; i < guidParts.Length; ++i)
            {
                if (i == 2)
                {
                    output += "-" + guidParts[i].ToLower();
                }
                else
                {
                    output += "-" + guidParts[i].ToUpper();
                }
            }
            output += "}";

            return output;
        }
        public static string DCSJoyIdToIL2JoyId(string DCSJoyId, int id)
        {
            if (id < 0 || DCSJoyId == null || DCSJoyId.Length < 12 || !DCSJoyId.Contains("{")) return null;
            string result = id.ToString() + ",%22";
            string guid = DCSJoyId.Split('{')[1].Replace("}", "");
            string name = DCSJoyId.Split('{')[0].Trim().Replace(" ", "%20");
            string[] guidCells = guid.Split('-');
            result = result + guidCells[0].ToLower() + "-" +
                guidCells[1].ToLower() + "-" +
                guidCells[2].ToLower() + "-000054534544" +
                guidCells[3].Substring(2, 2).ToLower() +
                guidCells[3].Substring(0, 2).ToLower() +
                "%22," + name;
            return result;
        }
        public static List<string> GetPossibleFallbacksForInstance(string instance, string game)
        {
            List<string> fallback = new List<string>();
            string initBackup, inputFolder, runBU;
            if (game == "DCS")
            {
                initBackup = initialBackupFolder;
                inputFolder = inputFolderName + inputFolderName;
                runBU = runningBackupFolder;
            }
            else if (game == "IL2Game")
            {
                initBackup = "\\data\\JP_InitBackup";
                inputFolder = "\\input";
                runBU = "\\data\\JP_Backup";
            }
            else
            {
                initBackup = "";
                inputFolder = "";
                runBU = "";
            }
            if (Directory.Exists(instance + initBackup + inputFolder))
            {
                fallback.Add("Initial");
            }
            if (Directory.Exists(instance + runBU))
            {
                DirectoryInfo pFolder = new DirectoryInfo(instance + runBU);
                DirectoryInfo[] subs = pFolder.GetDirectories();
                for (int i = 0; i < subs.Length; ++i)
                    fallback.Add(subs[i].Name);
            }

            return fallback;
        }

        public static void CheckSubfolderForDCSSticks(List<string> result, string path)
        {
            string[] subs = Directory.GetDirectories(path);
            for (int j = 0; j < subs.Length; j++)
            {
                string[] files=null;
                if (path.ToLower().EndsWith("input"))
                {
                    if(Directory.Exists(subs[j] + "\\joystick"))files = Directory.GetFiles(subs[j]+"\\joystick");
                }
                else
                {
                    if (Directory.Exists(subs[j]))files = Directory.GetFiles(subs[j]);
                }
                if(files != null)
                {
                    for (int k = 0; k < files.Length; k++)
                    {
                        string[] parts = files[k].Split('\\');
                        string toCompare = parts[parts.Length - 1];
                        if (toCompare.EndsWith(".html") ||
                            toCompare.EndsWith(".diff.lua") ||
                            toCompare.EndsWith(".jp"))
                        {
                            string toAdd = toCompare.Replace(".html", "").Replace(".diff.lua", "").Replace(".jp", "");
                            if (!result.Contains(DCSStickNaming(toAdd)) && toAdd != "Keyboard" && toAdd != "Mouse" && toAdd != "TrackIR")
                            {
                                result.Add(DCSStickNaming(toAdd));
                            }
                        }
                    }
                }
            }
        }

        public static void GetDCSInputJoysticks(List<string> result)
        {
            if (result == null) return;
            foreach (string instance in DCSInstances)
            {
                if (Directory.Exists(instance + "\\InputLayoutsTxt"))
                {
                    CheckSubfolderForDCSSticks(result, instance + "\\InputLayoutsTxt");
                }
                if (Directory.Exists(instance + "\\Config\\Input"))
                {
                    CheckSubfolderForDCSSticks(result, instance + "\\Config\\Input");
                }
            }
            if (MainStructure.msave.DCSInstaceOverride != null &&
                Directory.Exists(MainStructure.msave.DCSInstaceOverride))
            {
                if (Directory.Exists(MainStructure.msave.DCSInstaceOverride + "\\InputLayoutsTxt"))
                {
                    CheckSubfolderForDCSSticks(result, MainStructure.msave.lastInstanceSelected + "\\InputLayoutsTxt");
                }
                if (Directory.Exists(MainStructure.msave.DCSInstaceOverride + "\\Config\\Input"))
                {
                    CheckSubfolderForDCSSticks(result, MainStructure.msave.lastInstanceSelected + "\\Config\\Input");
                }
            }
            if (MainStructure.msave.lastInstanceSelected != null &&
                Directory.Exists(MainStructure.msave.lastInstanceSelected))
            {

                if (Directory.Exists(MainStructure.msave.lastInstanceSelected + "\\InputLayoutsTxt"))
                {
                    CheckSubfolderForDCSSticks(result, MainStructure.msave.lastInstanceSelected + "\\InputLayoutsTxt");
                }
                if (Directory.Exists(MiscGames.DCSselectedInstancePath + "\\Config\\Input"))
                {
                    CheckSubfolderForDCSSticks(result, MainStructure.msave.lastInstanceSelected + "\\Config\\Input");
                }

            }
        }
        public static void RestoreInputsInInstance(string instance, string fallBack, string game)
        {
            MainStructure.Write("Restoring input: " + instance + " " + fallBack + " " + game);
            string toCopy, dest, initFold, runBU, inputFo, subPath;
            if (game == "DCS")
            {
                initFold = initialBackupFolder;
                inputFo = inputFolderName;
                runBU = runningBackupFolder;
                subPath = subInputPath;

            }
            else if (game == "IL2Game")
            {
                initFold = "\\data\\JP_InitBackup";
                inputFo = "\\input";
                runBU = "\\data\\JP_Backup";
                subPath = "\\data";
            }
            else
            {
                initFold = "";
                inputFo = "";
                runBU = "";
                subPath = "";
            }
            dest = instance + subPath;
            if (fallBack.ToLower() == "initial")
            {
                if (Directory.Exists(instance + initFold + inputFo))
                {

                    toCopy = instance + initFold + inputFo;
                }
                else
                {
                    MessageBox.Show("Initial Backup does not exist.");
                    return;
                }
            }
            else
            {
                if (Directory.Exists(instance + runBU + "\\" + fallBack + inputFo))
                {
                    toCopy = instance + runBU + "\\" + fallBack + inputFo;
                }
                else
                {
                    MessageBox.Show("Given Fallback does not exist.");
                    return;
                }
            }
            if (!Directory.Exists(instance + subPath))
                Directory.CreateDirectory(instance + subPath);

            MainStructure.CopyFolderIntoFolder(toCopy, dest);
            MessageBox.Show("Backup has been reinstated: " + fallBack);

        }
        public static void CheckBackupDaysFolder(string inputFolder, string backupFolder)
        {
            if (Directory.Exists(inputFolder))
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd");
                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);
                DirectoryInfo pFolder = new DirectoryInfo(backupFolder);
                DirectoryInfo[] allSubs = pFolder.GetDirectories();
                if (MainStructure.msave == null)
                {
                    MainStructure.LoadMetaLast();
                }
                DeleteFoldersNotFollowingDateScheme(allSubs);
                pFolder = new DirectoryInfo(backupFolder);
                allSubs = pFolder.GetDirectories();
                if (allSubs.Length > MainStructure.msave.backupDays)
                {
                    List<DirectoryInfo> subList = allSubs.ToList();
                    subList = subList.OrderBy(o => o.Name).ToList();
                    for (int a = 0; a < (allSubs.Length - MainStructure.msave.backupDays); ++a)
                    {
                        try
                        {
                            MainStructure.Write("Too many backup days existing. Deleting: " + subList[a].FullName);
                            MainStructure.DeleteFolder(subList[a].FullName);
                        }
                        catch (Exception e)
                        {
                            MainStructure.NoteError(e);
                        }
                    }
                }
                if (!Directory.Exists(backupFolder + "\\" + now))
                {
                    Directory.CreateDirectory(backupFolder + "\\" + now);
                    MainStructure.CopyFolderIntoFolder(inputFolder, backupFolder + "\\" + now);
                }
            }
        }
        public static void DeleteFoldersNotFollowingDateScheme(DirectoryInfo[] allsubs)
        {
            foreach (DirectoryInfo di in allsubs)
            {
                if (!Regex.IsMatch(di.Name, @"\d{4}-\d{2}-\d{2}"))
                {
                    try
                    {
                        MainStructure.Write("Mismatching Backup folder name. Deleting: " + di.FullName);
                        MainStructure.DeleteFolder(di.FullName);
                    }
                    catch (Exception e)
                    {
                        MainStructure.NoteError(e);
                    }
                }
            }
        }
        public static void BackupConfigsOfDCSInstance(string instance)
        {
            MainStructure.Write("DCS backup instance: "+instance);
            if (!Directory.Exists(instance + runningBackupFolder))
            {
                Directory.CreateDirectory(instance + runningBackupFolder);
            }
            if (!Directory.Exists(instance + initialBackupFolder) && Directory.Exists(instance + subInputPath + inputFolderName))
            {
                Directory.CreateDirectory(instance + initialBackupFolder);
                MainStructure.CopyFolderIntoFolder(instance + subInputPath + inputFolderName, instance + initialBackupFolder);
            }
            if (Directory.Exists(instance + subInputPath + inputFolderName))
            {
                CheckBackupDaysFolder(instance + subInputPath + inputFolderName, instance + runningBackupFolder);
            }
        }
        public static string DCSStickNaming(string stName)
        {
            string outputName = stName;
            string[] partsName = outputName.Split('{');
            if (partsName.Length > 1)
            {
                outputName = partsName[0] + '{';
                string[] idParts = partsName[1].Split('-');
                outputName += idParts[0].ToUpper();
                for (int i = 1; i < idParts.Length; ++i)
                {
                    if (i == 2)
                    {
                        outputName = outputName + "-" + idParts[i].ToLower();
                    }
                    else
                    {
                        outputName = outputName + "-" + idParts[i].ToUpper();
                    }
                }
                return outputName;
            }
            return stName;
        }
        public static void DCSInstanceSelectionChanged(string newInstance)
        {
            MainStructure.Write("DCS Instance Selection Changed");
            if (DCSselectedInstancePath == newInstance) return;
            DCSselectedInstancePath = newInstance;
            InternalDataManagement.LocalJoysticks = null;
            InitGames.InitDCSJoysticks();
            if (MainStructure.msave != null)
            {
                MainStructure.msave.lastInstanceSelected = DCSselectedInstancePath;
            }
            if (MainStructure.msave.importLocals == null) MainStructure.msave.importLocals = true;
            if (MainStructure.msave.importLocals == true) DBLogic.PopulateDCSDictionaryWithLocal(DCSselectedInstancePath);
            int indx = -1;
            for (int i = 0; i < DCSInstances.Length; ++i)
            {
                if (DCSInstances[i].ToLower() == newInstance.ToLower())
                {
                    indx = i;
                    break;
                }
            }
            if (indx > -1)
                MainStructure.mainW.DropDownInstanceSelection.SelectedIndex = indx;
        }
        public static void BackupConfigsOfSC()
        {
            MainStructure.Write("Backing up SC");
            if (!Directory.Exists(StarCitizen)) return;
            try
            {
                const string inputFolder = "\\LIVE\\USER\\Client\\0\\Profiles\\default";
                const string initBU = "\\LIVE\\USER\\Client\\0\\Profiles\\JP_InitBackup";
                const string runningBU = "\\LIVE\\USER\\Client\\0\\Profiles\\JP_Backup";
                if (!Directory.Exists(StarCitizen + initBU))
                {
                    Directory.CreateDirectory(StarCitizen + initBU);
                    MainStructure.CopyFolderIntoFolder(StarCitizen + inputFolder, StarCitizen + initBU);
                }
                else
                {
                    CheckBackupDaysFolder(StarCitizen + inputFolder, StarCitizen + runningBU);
                }

            }
            catch (Exception e)
            {
                MainStructure.NoteError(e);
            }
        }
        public static void BackupConfigsOfIL2()
        {
            MainStructure.Write("Backing up IL2");
            if (!Directory.Exists(IL2Instance)) return;
            try
            {
                const string inputFolder = "\\data\\input";
                const string initBU = "\\data\\JP_InitBackup";
                const string runningBU = "\\data\\JP_Backup";
                if (!Directory.Exists(IL2Instance + initBU))
                {
                    Directory.CreateDirectory(IL2Instance + initBU);
                    MainStructure.CopyFolderIntoFolder(IL2Instance + inputFolder, IL2Instance + initBU);
                }
                else
                {
                    CheckBackupDaysFolder(IL2Instance + inputFolder, IL2Instance + runningBU);
                }
            }
            catch (Exception e)
            {
                MainStructure.NoteError(e);
            }
        }

    }


}
