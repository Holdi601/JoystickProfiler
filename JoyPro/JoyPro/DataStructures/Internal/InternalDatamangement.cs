using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace JoyPro
{
    public static class InternalDataMangement
    {
        public static string[] RelationWordFilter;
        public static bool showUnassignedRelations = true;
        public static bool showUnassignedGroups = true;
        public static Dictionary<string, Relation> AllRelations = new Dictionary<string, Relation>();
        public static Dictionary<string, Bind> AllBinds = new Dictionary<string, Bind>();
        public static List<string> AllGroups = new List<string>();
        public static Dictionary<string, bool> GroupActivity = new Dictionary<string, bool>();
        public static Dictionary<string, bool> JoystickActivity = new Dictionary<string, bool>();
        public static Dictionary<string, string> JoystickAliases = new Dictionary<string, string>();
        public static string[] LocalJoysticks;
        public static Dictionary<string, Modifier> AllModifiers = new Dictionary<string, Modifier>();
        public static Dictionary<string, bool> GamesFilter = new Dictionary<string, bool>();
        public static Dictionary<string, string> JoystickFileImages = new Dictionary<string, string>();
        public static string JoystickLayoutExport=null;

        public static void CorrectModifiersInBinds()
        {
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.AllReformers != null)
                    for (int i = kvp.Value.AllReformers.Count - 1; i >= 0; --i)
                    {
                        string[] reformParts = kvp.Value.AllReformers[i].Split('§');
                        if (reformParts.Length > 2)
                        {
                            Modifier m = GetModifierWithKeyCombo(reformParts[1], reformParts[2]);
                            kvp.Value.AllReformers[i] = m.toReformerString();
                        }
                        else
                        {
                            kvp.Value.AllReformers.RemoveAt(i);
                        }
                    }
            }
            ResyncRelations();
        }
        public static List<string> GetAllModsAsString()
        {
            List<string> result = new List<string>();
            foreach (KeyValuePair<string, Modifier> kvp in AllModifiers)
            {
                result.Add(kvp.Key);
            }
            result = result.OrderBy(o => o).ToList();
            return result;
        }
        public static Modifier GetModifierWithKeyCombo(string device, string key)
        {
            foreach (KeyValuePair<string, Modifier> kvp in AllModifiers)
                if (kvp.Value.device.ToUpper() == device.ToUpper() && kvp.Value.key.ToUpper() == key.ToUpper()) return kvp.Value;
            return null;
        }
        public static void RemoveBind(Bind b)
        {
            foreach (KeyValuePair<string, Relation> kvp in AllRelations)
            {
                if (kvp.Value.bind == b)
                    kvp.Value.bind = null;
            }
            if (AllBinds.ContainsKey(b.Rl.NAME))
                AllBinds.Remove(b.Rl.NAME);
        }
        public static void ChangeReformerName(string oldName, string newName)
        {
            if (AllModifiers.ContainsKey(oldName))
            {
                Modifier m = AllModifiers[oldName];
                string oldRefKey = m.toReformerString();
                AllModifiers.Remove(oldName);
                m.name = newName;
                foreach (KeyValuePair<string, Bind> kvp in AllBinds)
                {
                    if (kvp.Value.AllReformers.Contains(oldRefKey))
                    {
                        kvp.Value.AllReformers.Remove(oldRefKey);
                        kvp.Value.AllReformers.Add(m.toReformerString());
                    }
                }
                AllModifiers.Add(m.name, m);
            }
        }
        public static void RemoveReformer(string name)
        {
            if (AllModifiers.ContainsKey(name))
            {
                Modifier m = AllModifiers[name];
                AllModifiers.Remove(name);
                foreach (KeyValuePair<string, Bind> kvp in AllBinds)
                {
                    if (kvp.Value.AllReformers.Contains(m.toReformerString()))
                        kvp.Value.AllReformers.Remove(m.toReformerString());
                }
            }
        }
        public static bool ModifiersContainKeyCombo(string device, string key)
        {
            bool found = false;
            foreach (KeyValuePair<string, Modifier> kvp in AllModifiers)
            {
                if (kvp.Value.device.ToUpper() == device.ToUpper() && kvp.Value.key.ToUpper() == key.ToUpper()) return true;
            }
            return found;
        }
        public static ModExists DoesReformerExistInMods(string reformer)
        {
            Modifier m = Modifier.ReformerToMod(reformer);
            if (m == null) return ModExists.ERROR;
            if (!AllModifiers.ContainsKey(m.name))
            {
                if (ModifiersContainKeyCombo(m.device, m.key))
                {
                    return ModExists.KEYBIND_EXISTS;
                }
                else
                {
                    return ModExists.NOT_EXISTENT;
                }
            }
            else
            {
                if (AllModifiers[m.name].device.ToUpper() == m.device.ToUpper() &&
                    AllModifiers[m.name].key.ToUpper() == m.key.ToUpper())
                {
                    return ModExists.ALL_EXISTS;
                }
                else
                {
                    return ModExists.BINDNAME_EXISTS;
                }
            }
        }
        public static string GetReformerStringFromMod(string name)
        {
            if (AllModifiers.ContainsKey(name)) return AllModifiers[name].toReformerString();
            return null;
        }
        public static void AddReformerToMods(string reformer)
        {
            Modifier m = Modifier.ReformerToMod(reformer);
            AllModifiers.Add(m.name, m);
        }
        public static void CorrectBindNames(Dictionary<string, Bind> lib, Bind b)
        {
            if (b == null || lib == null) return;
            foreach (KeyValuePair<string, Bind> kvp in lib)
            {
                if (b.additionalImportInfo != null && b.additionalImportInfo.Length > 0)
                {
                    if (b.Joystick == kvp.Value.Joystick &&
                        b.Inverted == kvp.Value.Inverted &&
                        b.Deadzone == kvp.Value.Deadzone &&
                        b.JAxis == kvp.Value.JAxis &&
                        b.JButton == kvp.Value.JButton &&
                        b.SaturationX == kvp.Value.SaturationX &&
                        b.SaturationY == kvp.Value.SaturationY &&
                        b.Slider == kvp.Value.Slider &&
                        b.Rl.NAME.ToUpper() == kvp.Value.Rl.NAME.ToUpper())
                    {
                        bool integrity = true;
                        if (b.AllReformers != null && kvp.Value.AllReformers != null)
                        {
                            for (int i = 0; i < b.AllReformers.Count; ++i)
                            {
                                if (b.AllReformers.Count != kvp.Value.AllReformers.Count || !kvp.Value.AllReformers.Contains(b.AllReformers[i]))
                                {
                                    integrity = false;
                                    break;
                                }
                            }
                        }
                        else if ((b.AllReformers != null && kvp.Value.AllReformers == null) || (b.AllReformers == null && kvp.Value.AllReformers != null))
                        {
                            integrity = false;
                        }
                        if (b.Curvature != null && kvp.Value.Curvature != null)
                        {
                            for (int i = 0; i < b.Curvature.Count; ++i)
                            {
                                if (b.Curvature.Count != kvp.Value.Curvature.Count || !kvp.Value.Curvature.Contains(b.Curvature[i]))
                                {
                                    integrity = false;
                                    break;
                                }
                            }
                        }
                        else if ((b.Curvature != null && kvp.Value.Curvature == null) || (b.Curvature == null && kvp.Value.Curvature != null))
                        {
                            integrity = false;
                        }
                        if (integrity)
                        {
                            kvp.Value.Rl.NAME = b.additionalImportInfo.Split('§')[b.additionalImportInfo.Split('§').Length - 1];
                            for (int a = 0; a < b.Rl.Groups.Count; ++a)
                            {
                                if (!kvp.Value.Rl.Groups.Contains(b.Rl.Groups[a]))
                                    kvp.Value.Rl.Groups.Add(b.Rl.Groups[a]);
                            }
                            string[] modNames = b.additionalImportInfo.Split('§');
                            for (int i = 0; i < modNames.Length - 1; ++i)
                            {
                                if (kvp.Value.AllReformers.Count > i)
                                {
                                    string[] refParts = kvp.Value.AllReformers[i].Split('§');
                                    string replacement = modNames[i] + "§";
                                    if (refParts.Length > 2)
                                    {
                                        Modifier m = GetModifierWithKeyCombo(refParts[1], refParts[2]);
                                        string toRemove = m.name;
                                        m.JPN = modNames[i];
                                        m.name = modNames[i];
                                        AllModifiers.Remove(toRemove);
                                        if (!AllModifiers.ContainsKey(m.name))
                                            AllModifiers.Add(m.name, m);
                                        kvp.Value.AllReformers[i] = m.toReformerString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void AddRelation(Relation r)
        {
            if (!AllRelations.ContainsKey(r.NAME))
            {
                AllRelations.Add(r.NAME, r);
            }
            ResyncRelations();
        }
        public static void RemoveRelation(Relation r)
        {
            AllRelations.Remove(r.NAME);
            ResyncRelations();
        }
        public static void LoadRelations(string filePath)
        {
            if (filePath == null || filePath.Length < 1) return;
            NewFile();
            try
            {
                AllRelations = MainStructure.ReadFromBinaryFile<Dictionary<string, Relation>>(filePath);
                foreach (KeyValuePair<string, Relation> kvp in AllRelations)
                {
                    kvp.Value.CheckNamesAgainstDB();
                }
            }
            catch
            {
                MessageBox.Show("Couldn't load relations");
            }
            ResyncRelations();
        }
        public static void NewFile(bool programStart = false)
        {
            AllBinds.Clear();
            AllRelations.Clear();
            AllModifiers.Clear();
            AllGroups.Clear();
            JoystickAliases.Clear();
            GroupActivity.Clear();
            InitGames.ReloadGameData();


            if (!programStart)
                ResyncRelations();
        }
        public static void ResyncBindsToMods()
        {
            AllModifiers.Clear();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                for (int i = 0; i < kvp.Value.AllReformers.Count; ++i)
                {
                    Modifier m = Modifier.ReformerToMod(kvp.Value.AllReformers[i]);
                    if (!AllModifiers.ContainsKey(m.name))
                    {
                        AllModifiers.Add(m.name, m);
                    }
                    else
                    {
                        string counterCase = AllModifiers[m.name].toReformerString();
                        kvp.Value.AllReformers[i] = counterCase;
                    }
                }
            }
        }
        public static Modifier ModifierByName(string name)
        {
            if (AllModifiers.ContainsKey(name)) return AllModifiers[name];
            return null;
        }
        public static List<string> GamesInRelations()
        {
            List<string> result = new List<string>();
            foreach (KeyValuePair<string, Relation> kvp in AllRelations)
            {
                List<string> inRes = kvp.Value.GamesInRelation();
                for (int i = 0; i < inRes.Count; ++i)
                {
                    if (!result.Contains(inRes[i]))
                    {
                        result.Add(inRes[i]);
                    }
                }
            }
            return result;
        }
        public static void RemoveGroupFromSpecificRelation(string relation, string group)
        {
            if (AllRelations.ContainsKey(relation) && AllRelations[relation].Groups.Contains(group))
                AllRelations[relation].Groups.Remove(group);
        }
        public static void RemoveGroupFromRelation(string group)
        {
            foreach (KeyValuePair<string, Relation> kvp in AllRelations)
            {
                if (kvp.Value.Groups == null)
                {
                    kvp.Value.Groups = new List<string>();
                    continue;
                }
                if (kvp.Value.Groups.Contains(group))
                {
                    kvp.Value.Groups.Remove(group);
                }
            }
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Rl.Groups == null)
                {
                    kvp.Value.Rl.Groups = new List<string>();
                    continue;
                }
                if (kvp.Value.Rl.Groups.Contains(group))
                {
                    kvp.Value.Rl.Groups.Remove(group);
                }
            }
        }
        public static void RemoveDeviceAlias(string device)
        {
            if (JoystickAliases.ContainsKey(device))
                JoystickAliases.Remove(device);
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick.ToLower() == device.ToLower())
                {
                    kvp.Value.aliasJoystick = "";
                }
            }
        }
        static void RecreateGroups()
        {
            foreach (KeyValuePair<string, Relation> kvp in AllRelations)
            {
                if (kvp.Value.Groups != null)
                {
                    for (int i = 0; i < kvp.Value.Groups.Count; ++i)
                    {
                        if (!MainStructure.ListContainsCaseInsensitive(AllGroups, kvp.Value.Groups[i]))
                            AllGroups.Add(kvp.Value.Groups[i]);
                    }
                }
            }
        }
        public static void LoadProfile(string filePath)
        {
            if (filePath == null || filePath.Length < 1) return;
            
            try
            {
                Pr0file pr = null;
                pr = MainStructure.ReadFromBinaryFile<Pr0file>(filePath);
                NewFile();
                AllRelations = pr.Relations;
                AllBinds = pr.Binds;
                JoystickAliases = pr.JoystickAliases;
                JoystickFileImages = pr.JoystickFileImages;
                JoystickLayoutExport = pr.JoystickLayoutExport;
                if (pr.LastSelectedDCSInstance != null && Directory.Exists(pr.LastSelectedDCSInstance))
                {
                    MiscGames.DCSInstanceSelectionChanged(pr.LastSelectedDCSInstance);
                }
                ResyncBindsToMods();
                RecreateGroups();
                foreach (KeyValuePair<string, Relation> kvp in AllRelations)
                {
                    kvp.Value.CheckNamesAgainstDB();
                }
                AddLoadedJoysticks();
                CheckConnectedSticksToBinds();
            }
            catch
            {
                MessageBox.Show("Couldn't load profile. Either opened by some program or other error");
            }
            ResyncRelations();
        }
        public static List<string> GetAllPossibleJoysticks()
        {
            List<string> result = JoystickReader.GetConnectedJoysticks();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (!result.Contains(kvp.Value.Joystick)) result.Add(kvp.Value.Joystick);
            }
            return result;
        }
        public static bool DoesJoystickAliasExist(string alias)
        {
            foreach (KeyValuePair<string, string> kvp in JoystickAliases)
            {
                if (alias == kvp.Value) return true;
            }
            return false;
        }
        public static void AddBind(string name, Bind b)
        {
            if (!AllBinds.ContainsKey(name)) AllBinds.Add(name, b);
        }
        public static void DeleteBind(string name)
        {
            if (AllBinds.ContainsKey(name)) AllBinds.Remove(name);
        }
        public static void InsertRelations(string[] files)
        {
            foreach (string s in files)
            {
                if (s == null || s.Length < 1) continue;
                Dictionary<string, Relation> thisRel = MainStructure.ReadFromBinaryFile<Dictionary<string, Relation>>(s);
                foreach (KeyValuePair<string, Relation> kvp in thisRel)
                {
                    string newKey = kvp.Key;
                    while (AllRelations.ContainsKey(newKey))
                    {
                        bool? overwrite = MainStructure.mainW.RelationAlreadyExists(newKey);
                        if (overwrite == true)
                        {
                            AllRelations[newKey] = kvp.Value;
                            break;
                        }
                        else if (overwrite == false)
                        {
                            break;
                        }
                        else
                        {
                            newKey += "1";
                        }
                    }
                    if (!AllRelations.ContainsKey(newKey))
                    {
                        AllRelations.Add(newKey, kvp.Value);
                        kvp.Value.NAME = newKey;
                    }
                }
            }
            ResyncRelations();
        }
        static void AddLoadedJoysticks()
        {
            List<string> sticks = new List<string>();
            if (LocalJoysticks != null)
                for (int i = 0; i < LocalJoysticks.Length; ++i)
                {
                    sticks.Add(MiscGames.DCSStickNaming(LocalJoysticks[i]));
                }
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick != null && kvp.Value.Joystick.Length > 0 && !sticks.Contains<string>(MiscGames.DCSStickNaming(kvp.Value.Joystick)))
                {
                    sticks.Add(MiscGames.DCSStickNaming(kvp.Value.Joystick));
                }
            }
            LocalJoysticks = sticks.ToArray();
        }
        static void CheckConnectedSticksToBinds()
        {
            //Check if joystick is connected and ask for more context
            List<string> connectedSticks = JoystickReader.GetConnectedJoysticks();
            List<string> misMatches = new List<string>();
            List<Bind> toRemove = new List<Bind>();
            List<string> upperConn = new List<string>();
            for (int i = 0; i < connectedSticks.Count; ++i)
                if (!upperConn.Contains(connectedSticks[i].ToUpper()))
                    upperConn.Add(connectedSticks[i].ToUpper());
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick != null && kvp.Value.Joystick.Length > 0)
                    if (!upperConn.Contains(kvp.Value.Joystick.ToUpper()) && !misMatches.Contains(kvp.Value.Joystick))
                        misMatches.Add(kvp.Value.Joystick);
            }
            foreach (Bind b in toRemove) if (AllBinds.ContainsKey(b.Rl.NAME)) AllBinds.Remove(b.Rl.NAME);
            foreach (string mis in misMatches)
            {
                ExchangeStick es = new ExchangeStick(mis);
                es.Show();
            }
        }
        public static void ExchangeSticksInBind(string old, string newstr)
        {
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick != null && kvp.Value.Joystick.Length > 0 && kvp.Value.Joystick == old && newstr.Length > 0)
                {
                    kvp.Value.Joystick = newstr;
                }
                kvp.Value.replaceDeviceInReformers(old, newstr);
            }
            ResyncRelations();
        }
        public static void SaveRelationsTo(string filePath)
        {
            MainStructure.WriteToBinaryFile<Dictionary<string, Relation>>(filePath, AllRelations);
        }
        public static void SaveProfileTo(string filePath)
        {
            Pr0file pr = new Pr0file(AllRelations, AllBinds, MiscGames.DCSselectedInstancePath, JoystickAliases);
            pr.JoystickAliases= JoystickAliases;
            pr.JoystickFileImages = JoystickFileImages;
            pr.JoystickLayoutExport = JoystickLayoutExport;
            MainStructure.WriteToBinaryFile<Pr0file>(filePath, pr);
        }
        public static List<Relation> SyncRelations()
        {
            List<Relation> li = new List<Relation>();
            foreach (KeyValuePair<string, Relation> kvp in AllRelations) li.Add(kvp.Value);
            return li;
        }
        public static Bind GetBindForRelation(string name)
        {
            if (AllBinds.ContainsKey(name)) return AllBinds[name];
            return null;
        }
        public static void ResyncRelations(object sender, EventArgs e)
        {
            ResyncRelations();
        }
        public static void ResyncRelations()
        {
            List<Relation> li = SyncRelations();
            AllRelations.Clear();
            List<Bind> albi = new List<Bind>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                albi.Add(kvp.Value);
                kvp.Value.CorrectJoystickName();
            }
            AllBinds.Clear();
            List<Relation> relWithBinds = new List<Relation>();
            for (int i = 0; i < li.Count; i++)
            {
                if (!AllRelations.ContainsKey(li[i].NAME)) AllRelations.Add(li[i].NAME, li[i]);
                for (int j = 0; j < albi.Count; ++j)
                {
                    if (albi[j].Rl == li[i])
                    {
                        AllBinds.Add(li[i].NAME, albi[j]);
                        li[i].bind = albi[j];
                        if (!relWithBinds.Contains(li[i]))
                            relWithBinds.Add(li[i]);
                    }
                }
            }
            List<Relation> RelWOBinds = new List<Relation>();
            string sortType = "NAME";
            string sortOrder = "NORM";
            if (MainStructure.mainW.selectedSort != null && MainStructure.mainW.selectedSort.Contains('_'))
            {
                sortType = MainStructure.mainW.selectedSort.Split('_')[0];
                sortOrder = MainStructure.mainW.selectedSort.Split('_')[1];
            }

            for (int i = 0; i < li.Count; ++i)
            {
                if (!RelWOBinds.Contains(li[i]) && !relWithBinds.Contains(li[i]))
                    RelWOBinds.Add(li[i]);
            }
            switch (sortType)
            {
                case "NAME":
                    li = li.OrderBy(o => o.NAME).ToList();
                    break;
                case "STICK":
                    relWithBinds = relWithBinds.OrderBy(o => o.NAME).ToList();
                    relWithBinds = relWithBinds.OrderBy(o => AllBinds[o.NAME].Joystick).ToList();
                    RelWOBinds = RelWOBinds.OrderBy(o => o.NAME).ToList();
                    for (int i = 0; i < RelWOBinds.Count; ++i)
                    {
                        relWithBinds.Add(RelWOBinds[i]);
                    }
                    li = relWithBinds;
                    break;
                case "BTN":
                    relWithBinds = relWithBinds.OrderBy(o => o.NAME).ToList();
                    relWithBinds = relWithBinds.OrderBy(o => AllBinds[o.NAME].Joystick).ToList();
                    relWithBinds = relWithBinds.OrderBy(o => (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "").Length < 3 ?
                        (("0" + (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")).Length < 3 ?
                            ("00" + (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")) :
                            "0" + (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")) :
                        (AllBinds[o.NAME].JAxis + AllBinds[o.NAME].JButton).Replace("JOY_BTN", "")
                    ).ToList();
                    RelWOBinds = RelWOBinds.OrderBy(o => o.NAME).ToList();
                    for (int i = 0; i < RelWOBinds.Count; ++i)
                    {
                        relWithBinds.Add(RelWOBinds[i]);
                    }
                    li = relWithBinds;
                    break;
            }
            if (sortOrder == "DESC")
            {
                List<Relation> fi = new List<Relation>();
                for (int i = li.Count - 1; i >= 0; i--)
                {
                    fi.Add(li[i]);
                }
                li = fi;
            }
            MainStructure.SaveMetaLast();
            li = FilterGroups(li);
            li = FilterDevices(li);
            li = FilterWords(li);
            MainStructure.mainW.SetRelationsToView(li);
        }
        static List<Relation> FilterWords(List<Relation> temp)
        {

            if (RelationWordFilter == null || RelationWordFilter.Length == 0 || temp == null) return temp;
            List<Relation> toReturn = new List<Relation>();
            for (int i = 0; i < temp.Count; ++i)
            {
                bool allFound = true;
                for (int j = 0; j < RelationWordFilter.Length; ++j)
                {
                    if (!(temp[i].NAME.ToLower().Contains(RelationWordFilter[j].ToLower())))
                    {
                        allFound = false;
                    }
                }
                if (allFound)
                {
                    toReturn.Add(temp[i]);
                }
            }
            return toReturn;
        }
        static List<Relation> FilterDevices(List<Relation> temp)
        {
            List<Relation> toReturn;
            if (JoystickActivity.Count > 0)
            {
                List<Relation> deviceResult = new List<Relation>();
                bool toCompare = JoystickActivity.ElementAt(0).Value;
                bool allTheSame = true;
                for (int i = 1; i < JoystickActivity.Count; ++i)
                {
                    if (toCompare != JoystickActivity.ElementAt(i).Value)
                    {
                        allTheSame = false;
                        break;
                    }
                }
                if (!allTheSame)
                {
                    List<string> activeDevices = new List<string>();
                    foreach (KeyValuePair<string, bool> kvp in JoystickActivity)
                        if (kvp.Value)
                            activeDevices.Add(kvp.Key);
                    for (int i = 0; i < temp.Count; ++i)
                    {
                        if (temp[i].bind == null)
                        {
                            if (showUnassignedRelations)
                            {
                                deviceResult.Add(temp[i]);
                            }
                            continue;
                        }
                        bool groupFound = activeDevices.Contains(temp[i].bind.Joystick);
                        if (groupFound)
                            deviceResult.Add(temp[i]);
                    }
                    toReturn = deviceResult;
                }
                else
                {
                    if (toCompare)
                    {
                        if (!showUnassignedRelations)
                        {
                            for (int h = 0; h < temp.Count; ++h)
                            {
                                if (temp[h].bind != null)
                                    deviceResult.Add(temp[h]);
                            }
                            toReturn = deviceResult;
                        }
                        else
                        {
                            toReturn = temp;
                        }
                    }
                    else
                    {
                        if (showUnassignedRelations)
                        {
                            for (int h = 0; h < temp.Count; ++h)
                            {
                                if (temp[h].bind == null)
                                    deviceResult.Add(temp[h]);
                            }
                            toReturn = deviceResult;
                        }
                        else
                        {
                            toReturn = temp;
                        }
                    }
                }
            }
            else
            {
                toReturn = temp;
            }

            return toReturn;
        }
        static List<Relation> FilterGroups(List<Relation> temp)
        {
            List<Relation> toReturn;
            if (GroupActivity.Count > 0)
            {
                List<Relation> groupresult = new List<Relation>();
                bool toCompare = GroupActivity[AllGroups[0]];
                bool allTheSame = true;
                for (int i = 1; i < AllGroups.Count; ++i)
                {
                    if (toCompare != GroupActivity[AllGroups[i]])
                    {
                        allTheSame = false;
                        break;
                    }

                }
                if (!allTheSame)
                {
                    List<string> activeGroups = new List<string>();
                    foreach (KeyValuePair<string, bool> kvp in GroupActivity)
                        if (kvp.Value)
                            activeGroups.Add(kvp.Key);
                    for (int i = 0; i < temp.Count; ++i)
                    {
                        bool groupFound = false;
                        for (int j = 0; j < activeGroups.Count; ++j)
                        {
                            if (temp[i].Groups == null)
                            {
                                temp[i].Groups = new List<string>();
                                if (showUnassignedGroups) groupFound = true;
                                break;
                            }
                            else if (temp[i].Groups.Count == 0 && showUnassignedGroups)
                            {
                                groupFound = true;
                            }
                            else
                            {
                                if (temp[i].Groups.Contains(activeGroups[j]))
                                {
                                    groupFound = true;
                                    break;
                                }
                            }
                        }
                        if (groupFound)
                            groupresult.Add(temp[i]);
                    }
                    toReturn = groupresult;
                }
                else
                {
                    if (showUnassignedGroups)
                    {
                        toReturn = temp;
                    }
                    else
                    {
                        for (int i = 0; i < temp.Count; ++i)
                        {
                            if (temp[i].Groups == null)
                            {
                                temp[i].Groups = new List<string>();
                                break;
                            }
                            else if (temp[i].Groups.Count > 0)
                            {
                                groupresult.Add(temp[i]);
                            }
                        }
                        toReturn = groupresult;
                    }

                }
            }
            else
            {
                toReturn = temp;
            }
            return toReturn;
        }
        public static List<Modifier> GetAllModifiers()
        {
            List<Modifier> res = new List<Modifier>();
            foreach (KeyValuePair<string, Modifier> kvp in AllModifiers)
            {
                res.Add(kvp.Value);
            }
            res = res.OrderBy(o => o.name).ToList();
            return res;
        }
        public static bool DoesRelationAlreadyExist(string name)
        {
            return AllRelations.ContainsKey(name);
        }
        public static bool RelationIsTheSame(string Name, Relation r)
        {
            return AllRelations[Name] == r;
        }
        public static List<Relation> GetAllRelations()
        {
            List<Relation> result = new List<Relation>();
            foreach (KeyValuePair<string, Relation> kvp in AllRelations)
                result.Add(kvp.Value);
            return result;
        }
        public static List<Bind> GetAllBinds()
        {
            List<Bind> result = new List<Bind>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
                result.Add(kvp.Value);
            return result;
        }
        public static void WriteProfileClean(bool nukeDevices)
        {
            DCSIOLogic.WriteProfileClean(nukeDevices);
            List<Bind> Il2Binds = new List<Bind>();
            foreach(KeyValuePair<string, Bind> kvp in AllBinds) {
                if (kvp.Value.Rl.GamesInRelation().Contains("IL2Game"))
                    Il2Binds.Add(kvp.Value);
            }
            IL2IOLogic.WriteOut(Il2Binds, OutputType.Clean);
            MainStructure.mainW.ShowMessageBox("Binds exported successfully ☻");
        }
        public static void WriteProfileCleanAndLoadedOverwrittenAndAdd(bool fillBeforeEmpty)
        {
            DCSIOLogic.WriteProfileCleanAndLoadedOverwrittenAndAdd(fillBeforeEmpty);
            List<Bind> Il2Binds = new List<Bind>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Rl.GamesInRelation().Contains("IL2Game"))
                    Il2Binds.Add(kvp.Value);
            }
            IL2IOLogic.WriteOut(Il2Binds, OutputType.Add);
            MainStructure.mainW.ShowMessageBox("Binds exported successfully ☻");
        }
        public static void WriteProfileCleanNotOverwriteLocal(bool fillBeforeEmpty)
        {
            DCSIOLogic.WriteProfileCleanNotOverwriteLocal(fillBeforeEmpty);
            List<Bind> Il2Binds = new List<Bind>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Rl.GamesInRelation().Contains("IL2Game"))
                    Il2Binds.Add(kvp.Value);
            }
            IL2IOLogic.WriteOut(Il2Binds, OutputType.Merge);
            MainStructure.mainW.ShowMessageBox("Binds exported successfully ☻");
        }
        public static void WriteProfileCleanAndLoadedOverwritten(bool fillBeforeEmpty)
        {
            DCSIOLogic.WriteProfileCleanAndLoadedOverwritten(fillBeforeEmpty);
            List<Bind> Il2Binds = new List<Bind>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Rl.GamesInRelation().Contains("IL2Game"))
                    Il2Binds.Add(kvp.Value);
            }
            IL2IOLogic.WriteOut(Il2Binds, OutputType.MergeOverwrite);
            MainStructure.mainW.ShowMessageBox("Binds exported successfully ☻");
        }
        public static List<Bind> GetBindsByJoystickAndKey(string Joystick, string Button, bool axis, bool Inverted, bool Slider, double SaturationX, double SaturationY, double Deadzone, List<string> modifier= null)
        {
            List<Bind> result = new List<Bind>();
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if ((kvp.Value.Joystick.ToLower() == Joystick.ToLower() &&
                   kvp.Value.Inverted == Inverted &&
                   kvp.Value.Deadzone ==Deadzone &&
                   kvp.Value.SaturationX == SaturationX &&
                   kvp.Value.SaturationY == SaturationY &&
                   kvp.Value.Slider == Slider)&&
                   ((axis&& kvp.Value.JAxis.ToLower() == Button.ToLower())||
                   (!axis&&kvp.Value.JButton.ToLower() == Button.ToLower() )))
                {
                    if(((modifier==null||modifier.Count==0)&&(kvp.Value.AllReformers!=null&&kvp.Value.AllReformers.Count>0))||
                        ((modifier != null && modifier.Count > 0) && (kvp.Value.AllReformers == null || kvp.Value.AllReformers.Count == 0))||
                        modifier.Count!=kvp.Value.AllReformers.Count)
                    {
                        continue;
                    }else if (modifier.Count == kvp.Value.AllReformers.Count && modifier.Count > 0)
                    {
                        bool mismatch = false;
                        for(int i=0; i<modifier.Count; ++i)
                        {
                            string[] parts = modifier[i].Split('§');
                            if (parts.Length < 3 ||
                                !Modifier.ButtonComboInReformerList(parts[1], parts[2], kvp.Value.AllReformers))
                            {
                                mismatch = true;
                                break;
                            }                            
                        }
                        if (mismatch)
                            continue;
                    }
                    result.Add(kvp.Value);
                }
            }
            return result;
        }
        public static string GetJoystickByIL2GUID(string guid)
        {
            for( int i=0; i<LocalJoysticks.Length; ++i)
            {
                string guidPart = LocalJoysticks[i].Split('{')[1].Replace("}", "");
                string[] guidCells = guidPart.Split('-');
                string[] parameterCells = guid.Split('-');
                if(guidCells[0].ToLower() == parameterCells[0].ToLower()&&
                    guidCells[1].ToLower()== parameterCells[1].ToLower()&&
                    guidCells[2].ToLower() == parameterCells[2].ToLower())
                {
                    return LocalJoysticks[i];
                }
            }
            return null;
        }
        public static void PrintOverviewToCsv(List<Relation> toPrint, string file)
        {
            StreamWriter swr = new StreamWriter(file);
            for(int i=0; i<toPrint.Count; ++i)
            {
                string line = toPrint[i].NAME+";";
                if (toPrint[i].Groups.Count > 0)
                {
                    line = line + toPrint[i].Groups[0];
                    for (int j = 1; j < toPrint[i].Groups.Count; ++j)
                    {
                        line = line + "," + toPrint[i].Groups[j];
                    }
                    line = line + ";";
                }
                Bind b=null;
                if (AllBinds.ContainsKey(toPrint[i].NAME))
                {
                    b = AllBinds[toPrint[i].NAME];
                }
                if (b == null)
                {
                    line = line + "None;";
                }
                else
                {
                    if (JoystickAliases.ContainsKey(b.Joystick))
                    {
                        line = line + JoystickAliases[b.Joystick] + ";";
                    }
                    else
                    {
                        line = line + b.Joystick + ";";
                    }
                }
                if (b == null)
                {
                    line = line + "None;";
                }
                else
                {
                    if (toPrint[i].ISAXIS)
                    {
                        string curvature=b.Curvature[0].ToString(new CultureInfo("en-US"));
                        if (b.Curvature.Count > 1)
                        {
                            for (int z = 0; z < b.Curvature.Count; ++z)
                                curvature = curvature + "," + b.Curvature[z].ToString(new CultureInfo("en-US"));
                        }
                        line = line + b.JAxis + ";" + b.Inverted.ToString() + ";" + b.Slider.ToString() + ";" + (b.Curvature.Count > 1 ? true : false).ToString() + ";" + b.Deadzone.ToString(new CultureInfo("en-US")+";"+b.SaturationX.ToString(new CultureInfo("en-US"))+";"+ b.SaturationY.ToString(new CultureInfo("en-US"))+";"+curvature);
                    }
                    else
                    {
                        string[] mod = new string[4];
                        for (int u = 0; u < 4; u++) mod[u] = "None";
                        if (b.AllReformers.Count > 0) mod[0] = b.AllReformers[0].Substring(b.AllReformers[0].IndexOf('§'));
                        if (b.AllReformers.Count > 1) mod[1] = b.AllReformers[1].Substring(b.AllReformers[1].IndexOf('§'));
                        if (b.AllReformers.Count > 2) mod[2] = b.AllReformers[2].Substring(b.AllReformers[2].IndexOf('§'));
                        if (b.AllReformers.Count > 3) mod[3] = b.AllReformers[3].Substring(b.AllReformers[3].IndexOf('§'));
                        line = line + b.JButton + ";null;null;null;"+mod[0]+";"+mod[1]+";"+mod[2]+";"+mod[3];
                    }
                }
                swr.WriteLine(line);
            }
            swr.Close();
            swr.Dispose();
        }
        public static List<string> GetJoysticksActiveInBinds()
        {
            List<string> result = new List<string>();
            foreach(KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (!result.Contains(kvp.Value.Joystick))
                    result.Add(kvp.Value.Joystick);
            }
            return result;
        }
        public static List<string> GetButtonsAxisInUseForStick(string Joystick)
        {
            List<string> result = new List<string>();
            foreach(KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (Joystick.ToLower() == kvp.Value.Joystick.ToLower())
                {
                    string toCompare;
                    if (kvp.Value.Rl.ISAXIS)
                    {
                        toCompare = kvp.Value.JAxis;
                    }
                    else
                    {
                        toCompare = kvp.Value.JButton;
                    }
                    string prefix = "";
                    if (kvp.Value.AllReformers.Count > 0)
                    {
                        prefix = kvp.Value.AllReformers[0].Substring(0, kvp.Value.AllReformers[0].IndexOf('§'));
                        for(int i=1; i<kvp.Value.AllReformers.Count; ++i)
                        {
                            prefix=prefix+"+" + kvp.Value.AllReformers[i].Substring(0, kvp.Value.AllReformers[i].IndexOf('§'));
                        }
                    }
                    toCompare = prefix + toCompare;
                    if (!result.Contains(toCompare)) result.Add(toCompare);
                }
            }
            return result;
        }
        public static string GetDescriptionForJoystickButtonGamePlane(string Joystick, string ModBtn, string Game, string Plane)
        {
            bool isAxis;
            string realBtn;
            string[] modSplit = null;
            if (ModBtn.Contains('+'))
            {
                modSplit = ModBtn.Split('+');
                realBtn = modSplit[modSplit.Length - 1];
                string[] shortend = new string[modSplit.Length - 1];
                for (int i = 0; i < shortend.Length; ++i)
                    shortend[i] = modSplit[i];
                modSplit = shortend;
            }
            else
            {
                realBtn = ModBtn;
            }
            if (realBtn.ToLower().Contains("btn") || realBtn.ToLower().Contains("pov")) isAxis = false;
            else isAxis = true;
            foreach(KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick == Joystick&&((isAxis&&realBtn==kvp.Value.JAxis)||(!isAxis&&realBtn==kvp.Value.JButton)))
                {
                    if (modSplit != null)
                    {
                        bool notFound = false;
                        for(int i=0; i<modSplit.Length; ++i)
                        {
                            if (!kvp.Value.ReformerInBind(modSplit[i]))
                            {
                                notFound = true;
                                break;
                            }
                        }
                        if (notFound) continue;
                    }
                    string result = kvp.Value.Rl.GetDescriptionForGamePlane(Game, Plane);
                    if (result.Length > 1) return result;
                }
            }
            return "";
        }
        public static string GetRelationNameForJostickButton(string Joystick, string ModBtn)
        {
            bool isAxis;
            string realBtn;
            string[] modSplit = null;
            if (ModBtn.Contains('+'))
            {
                modSplit = ModBtn.Split('+');
                realBtn = modSplit[modSplit.Length - 1];
                string[] shortend = new string[modSplit.Length - 1];
                for (int i = 0; i < shortend.Length; ++i)
                    shortend[i] = modSplit[i];
                modSplit = shortend;
            }
            else
            {
                realBtn = ModBtn;
            }
            if (realBtn.ToLower().Contains("btn") || realBtn.ToLower().Contains("pov")) isAxis = false;
            else isAxis = true;
            foreach (KeyValuePair<string, Bind> kvp in AllBinds)
            {
                if (kvp.Value.Joystick == Joystick && ((isAxis && realBtn == kvp.Value.JAxis) || (!isAxis && realBtn == kvp.Value.JButton)))
                {
                    if (modSplit != null)
                    {
                        bool notFound = false;
                        for (int i = 0; i < modSplit.Length; ++i)
                        {
                            if (!kvp.Value.ReformerInBind(modSplit[i]))
                            {
                                notFound = true;
                                break;
                            }
                        }
                        if (notFound) continue;
                    }
                    return kvp.Key;
                }
            }
            return "";
        }
    }
}
