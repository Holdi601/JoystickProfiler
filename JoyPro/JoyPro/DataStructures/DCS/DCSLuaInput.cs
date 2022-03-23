using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class DCSLuaInput
    {
        public string JoystickName;
        public string plane;
        public Dictionary<string, DCSLuaDiffsAxisElement> axisDiffs;
        public Dictionary<string, DCSLuaDiffsButtonElement> keyDiffs;
        const string filestart = "local diff = {\n";
        const string fileend = "}\nreturn diff";
        public void writeLua(string path, string filename, string ending)
        {
            StreamWriter swr = new StreamWriter(path+"\\"+filename+ending);
            swr.Write(filestart);
            if (axisDiffs.Count > 0)
            {
                string joyAl = "";
                if (InternalDataManagement.JoystickAliases.ContainsKey(JoystickName))
                {
                    joyAl = InternalDataManagement.JoystickAliases[JoystickName];
                }
                swr.Write("\t[\"JAL\"] = \"" + joyAl.Replace("\"","") + "\",\n");
                swr.Write("\t[\"axisDiffs\"] = {\n");
                foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvp in axisDiffs)
                {
                    swr.Write("\t\t[\"" + kvp.Key.Replace("\"", "") + "\"] = {\n");
                    swr.Write("\t\t\t[\"name\"] = \"" + kvp.Value.Title.Replace("\"", "") + "\",\n");
                    if (kvp.Value.added.Count > 0)
                    {
                        swr.Write("\t\t\t[\"added\"] = {\n");
                        for (int y = 0; y < kvp.Value.added.Count; ++y)
                        {
                            swr.Write("\t\t\t\t[" + (y + 1).ToString() + "] = {\n");
                            swr.Write("\t\t\t\t\t[\"key\"] = \"" + kvp.Value.added[y].key.Replace("\"", "") + "\",\n");
                            if (kvp.Value.added[y].reformers.Count > 0)
                            {
                                swr.Write("\t\t\t\t\t[\"reformers\"] = {\n");
                                for (int a = 0; a < kvp.Value.added[y].reformers.Count; ++a)
                                {
                                    swr.Write("\t\t\t\t\t\t[" + (a + 1).ToString() + "] = \"" + kvp.Value.added[y].reformers[a].Replace("\"", "") + "\",\n");
                                }
                                swr.Write("\t\t\t\t\t},\n");
                            }
                            swr.Write("\t\t\t\t\t[\"JPK\"] = \"" + kvp.Value.added[y].JPRelName.Replace("\"", "") + "\",\n");
                            if (kvp.Value.added[y].Groups.Count > 0)
                            {
                                swr.Write("\t\t\t\t\t[\"JGRP\"] = {\n");
                                for (int r = 0; r < kvp.Value.added[y].Groups.Count; ++r)
                                {
                                    swr.Write("\t\t\t\t\t\t[" + (r + 1).ToString() + "] = \"" + kvp.Value.added[y].Groups[r].Replace("\"", "") + "\",\n");
                                }
                                swr.Write("\t\t\t\t\t},\n");
                            }                      
                            if (kvp.Value.added[y].filter != null)
                            {
                                swr.Write("\t\t\t\t\t[\"filter\"] = {\n");
                                swr.Write("\t\t\t\t\t\t[\"curvature\"] = {\n");
                                for (int z = 0; z < kvp.Value.added[y].filter.curvature.Count; ++z)
                                {
                                    swr.Write("\t\t\t\t\t\t\t[" + (z + 1).ToString() + "] = " + kvp.Value.added[y].filter.curvature[z].ToString(new CultureInfo("en-US")) + ",\n");
                                }
                                swr.Write("\t\t\t\t\t\t},\n");
                                swr.Write("\t\t\t\t\t\t[\"deadzone\"] = " + kvp.Value.added[y].filter.deadzone.ToString(new CultureInfo("en-US")) + ",\n");
                                swr.Write("\t\t\t\t\t\t[\"invert\"] = " + kvp.Value.added[y].filter.inverted.ToString().ToLower() + ",\n");
                                swr.Write("\t\t\t\t\t\t[\"saturationX\"] = " + kvp.Value.added[y].filter.saturationX.ToString(new CultureInfo("en-US")) + ",\n");
                                swr.Write("\t\t\t\t\t\t[\"saturationY\"] = " + kvp.Value.added[y].filter.saturationY.ToString(new CultureInfo("en-US")) + ",\n");
                                swr.Write("\t\t\t\t\t\t[\"slider\"] = " + kvp.Value.added[y].filter.slider.ToString().ToLower() + ",\n");
                                swr.Write("\t\t\t\t\t},\n");
                            }
                            swr.Write("\t\t\t\t},\n");
                        }
                        swr.Write("\t\t\t},\n");
                    }
                    if (kvp.Value.removed.Count > 0)
                    {
                        swr.Write("\t\t\t[\"removed\"] = {\n");
                        for (int w = 0; w < kvp.Value.removed.Count; ++w)
                        {
                            swr.Write("\t\t\t\t[" + (w + 1).ToString() + "] = {\n");
                            swr.Write("\t\t\t\t\t[\"key\"] = \"" + kvp.Value.removed[w].key.Replace("\"", "") + "\",\n");
                            if (kvp.Value.removed[w].reformers.Count > 0)
                            {
                                swr.Write("\t\t\t\t\t[\"reformers\"] = {\n");
                                for (int a = 0; a < kvp.Value.removed[w].reformers.Count; ++a)
                                {
                                    swr.Write("\t\t\t\t\t\t[" + (a + 1).ToString() + "] = \"" + kvp.Value.removed[w].reformers[a].Replace("\"", "") + "\",\n");
                                }
                                swr.Write("\t\t\t\t\t},\n");
                            }
                            swr.Write("\t\t\t\t},\n");
                        }
                        swr.Write("\t\t\t},\n");
                    }
                    swr.Write("\t\t},\n");
                }
                swr.Write("\t},\n");
            }
            if (keyDiffs.Count > 0)
            {
                swr.Write("\t[\"keyDiffs\"] = {\n");
                foreach (KeyValuePair<string, DCSLuaDiffsButtonElement> kvp in keyDiffs)
                {
                    swr.Write("\t\t[\"" + kvp.Key.Replace("\"", "") + "\"] = {\n");
                    swr.Write("\t\t\t[\"name\"] = \"" + kvp.Value.Title.Replace("\"", "") + "\",\n");
                    if (kvp.Value.added.Count > 0)
                    {
                        swr.Write("\t\t\t[\"added\"] = {\n");
                        for (int z = 0; z < kvp.Value.added.Count; ++z)
                        {
                            swr.Write("\t\t\t\t[" + (z + 1).ToString() + "] = {\n");
                            swr.Write("\t\t\t\t\t[\"key\"] = \"" + kvp.Value.added[z].key.Replace("\"", "") + "\",\n");
                            swr.Write("\t\t\t\t\t[\"JPK\"] = \"" + kvp.Value.added[z].JPRelName.Replace("\"", "") + "\",\n");
                            if (kvp.Value.added[z].Groups.Count > 0)
                            {
                                swr.Write("\t\t\t\t\t[\"JGRP\"] = {\n");
                                for (int r = 0; r < kvp.Value.added[z].Groups.Count; ++r)
                                {
                                    swr.Write("\t\t\t\t\t\t[" + (r + 1).ToString() + "] = \"" + kvp.Value.added[z].Groups[r].Replace("\"", "") + "\",\n");
                                }
                                swr.Write("\t\t\t\t\t},\n");
                            }
                            if (kvp.Value.added[z].reformers.Count > 0)
                            {
                                swr.Write("\t\t\t\t\t[\"reformers\"] = {\n");
                                for (int a = 0; a < kvp.Value.added[z].reformers.Count; ++a)
                                {
                                    swr.Write("\t\t\t\t\t\t[" + (a + 1).ToString() + "] = \"" + kvp.Value.added[z].reformers[a].Replace("\"", "") + "\",\n");
                                }
                                swr.Write("\t\t\t\t\t},\n");
                            }
                            swr.Write("\t\t\t\t},\n");
                        }
                        swr.Write("\t\t\t},\n");
                    }
                    if (kvp.Value.removed.Count > 0)
                    {
                        swr.Write("\t\t\t[\"removed\"] = {\n");
                        for (int z = 0; z < kvp.Value.removed.Count; ++z)
                        {
                            swr.Write("\t\t\t\t[" + (z + 1).ToString() + "] = {\n");
                            swr.Write("\t\t\t\t\t[\"key\"] = \"" + kvp.Value.removed[z].key.Replace("\"", "") + "\",\n");
                            if (kvp.Value.removed[z].reformers.Count > 0)
                            {
                                swr.Write("\t\t\t\t\t[\"reformers\"] = {\n");
                                for (int a = 0; a < kvp.Value.removed[z].reformers.Count; ++a)
                                {
                                    swr.Write("\t\t\t\t\t\t[" + (a + 1).ToString() + "] = \"" + kvp.Value.removed[z].reformers[a].Replace("\"", "") + "\",\n");
                                }
                                swr.Write("\t\t\t\t\t},\n");
                            }
                            swr.Write("\t\t\t\t},\n");
                        }
                        swr.Write("\t\t\t},\n");
                    }
                    swr.Write("\t\t},\n");
                }
                swr.Write("\t},\n");
            }
            swr.Write(fileend);
            swr.Flush();
            swr.Close();
            swr.Dispose();
        }
        public DCSLuaInput Copy()
        {
            DCSLuaInput result = new DCSLuaInput();
            result.JoystickName = JoystickName;
            result.plane = plane;
            foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvp in axisDiffs)
            {
                result.axisDiffs.Add(kvp.Key, kvp.Value.Copy());
            }
            foreach (KeyValuePair<string, DCSLuaDiffsButtonElement> kvp in keyDiffs)
            {
                result.keyDiffs.Add(kvp.Key, kvp.Value.Copy());
            }
            return result;
        }
        public void AdditionalAnalyzationRawLuaInvert(string content)
        {
            if (!content.Contains('{')) return;
            string cleaned = LUADataRead.GetContentBetweenSymbols(content, "{", "}");
            Dictionary<object, object> dct = LUADataRead.CreateAttributeDictFromLua(content);
            if (dct.ContainsKey("axisDiffs"))
            {
                foreach (KeyValuePair<object, object> kvp in (Dictionary<object, object>)dct["axisDiffs"])
                {
                    DCSLuaDiffsAxisElement current = new DCSLuaDiffsAxisElement();
                    current.Keyname = (string)kvp.Key;
                    if (axisDiffs.ContainsKey(current.Keyname)) current= axisDiffs[current.Keyname];
                    else axisDiffs.Add(current.Keyname, current);
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("name"))
                        current.Title = (string)((Dictionary<object, object>)kvp.Value)["name"];
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("added"))
                    {
                        Dictionary<object, object> dictAdded = (Dictionary<object, object>)((Dictionary<object, object>)kvp.Value)["added"];
                        foreach (KeyValuePair<object, object> kvpAdded in dictAdded)
                        {
                            if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("key"))
                            {
                                DCSAxisBind dab = new DCSAxisBind();
                                dab.key = (string)((Dictionary<object, object>)kvpAdded.Value)["key"];
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("reformers"))
                                {
                                    foreach (KeyValuePair<object, object> kvpReformers in (Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["reformers"])
                                    {
                                        if (!dab.reformers.Contains((string)kvpReformers.Value))
                                            dab.reformers.Add((string)kvpReformers.Value);
                                    }
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("filter"))
                                {
                                    DCSAxisFilter daf = new DCSAxisFilter();
                                    dab.filter = daf;
                                    daf.deadzone = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["deadzone"];
                                    daf.inverted = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["invert"];
                                    daf.slider = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["slider"];
                                    daf.saturationX = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationX"];
                                    daf.saturationY = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationY"];
                                    foreach (KeyValuePair<object, object> kvpCurve in (Dictionary<object, object>)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["curvature"])
                                    {
                                        daf.curvature.Add((double)kvpCurve.Value);
                                    }
                                }
                                if (!current.doesRemovedContainKey(dab.key))
                                {
                                    current.removed.Add(dab);
                                    current.removeItemFromAdded(dab.key);
                                }
                            }
                        }
                    }
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("changed"))
                    {
                        Dictionary<object, object> dictAdded = (Dictionary<object, object>)((Dictionary<object, object>)kvp.Value)["changed"];
                        foreach (KeyValuePair<object, object> kvpAdded in dictAdded)
                        {
                            if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("key"))
                            {
                                DCSAxisBind dab = new DCSAxisBind();
                                dab.key = (string)((Dictionary<object, object>)kvpAdded.Value)["key"];
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("reformers"))
                                {
                                    foreach (KeyValuePair<object, object> kvpReformers in (Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["reformers"])
                                    {
                                        if (!dab.reformers.Contains((string)kvpReformers.Value))
                                            dab.reformers.Add((string)kvpReformers.Value);
                                    }
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("filter"))
                                {
                                    DCSAxisFilter daf = new DCSAxisFilter();
                                    dab.filter = daf;
                                    daf.deadzone = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["deadzone"];
                                    daf.inverted = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["invert"];
                                    daf.slider = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["slider"];
                                    daf.saturationX = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationX"];
                                    daf.saturationY = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationY"];
                                    foreach (KeyValuePair<object, object> kvpCurve in (Dictionary<object, object>)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["curvature"])
                                    {
                                        daf.curvature.Add((double)kvpCurve.Value);
                                    }
                                }
                                if (!current.doesRemovedContainKey(dab.key))
                                {
                                    current.removed.Add(dab);
                                    current.removeItemFromAdded(dab.key);
                                }
                            }
                        }
                    }
                }
            }
            if (dct.ContainsKey("keyDiffs"))
            {
                foreach (KeyValuePair<object, object> kvp in (Dictionary<object, object>)dct["keyDiffs"])
                {
                    DCSLuaDiffsButtonElement current = new DCSLuaDiffsButtonElement();
                    current.Keyname = (string)kvp.Key;
                    if (keyDiffs.ContainsKey(current.Keyname)) current= keyDiffs[current.Keyname];
                    else keyDiffs.Add(current.Keyname, current);
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("name"))
                        current.Title = (string)((Dictionary<object, object>)kvp.Value)["name"];
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("added"))
                    {
                        Dictionary<object, object> dictAdded = (Dictionary<object, object>)((Dictionary<object, object>)kvp.Value)["added"];
                        foreach (KeyValuePair<object, object> kvpAdded in dictAdded)
                        {
                            if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("key"))
                            {
                                DCSButtonBind dab = new DCSButtonBind();
                                dab.key = (string)((Dictionary<object, object>)kvpAdded.Value)["key"];
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("reformers"))
                                {
                                    foreach (KeyValuePair<object, object> kvpReformers in (Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["reformers"])
                                    {
                                        if (!dab.reformers.Contains((string)kvpReformers.Value))
                                            dab.reformers.Add((string)kvpReformers.Value);
                                        
                                    }
                                }
                                if (!current.doesRemovedContainKey(dab.key, dab.reformers))
                                {
                                    current.removed.Add(dab);
                                    current.removeItemFromAdded(dab.key, dab.reformers);
                                }
                            }
                        }
                    }                  
                }
            }
        }
        public void AnalyzeRawLuaInput(string content,DCSExportPlane refMod = null)
        {
            if (!content.Contains('{')) return;
            string cleaned = LUADataRead.GetContentBetweenSymbols(content, "{", "}");
            Dictionary<object, object> dct = LUADataRead.CreateAttributeDictFromLua(content);
            string joyAlias = "";
            if (dct.ContainsKey("JAL"))
            {
                joyAlias = (string)dct["JAL"];
                if (JoystickName.Length > 0 && joyAlias.Length > 0)
                {
                    if (InternalDataManagement.JoystickAliases.ContainsKey(JoystickName))
                    {
                        InternalDataManagement.JoystickAliases[JoystickName] = joyAlias;
                    }
                    else
                    {
                        InternalDataManagement.JoystickAliases.Add(JoystickName, joyAlias);
                    }
                }
            }
            if (dct.ContainsKey("axisDiffs"))
            {
                foreach (KeyValuePair<object, object> kvp in (Dictionary<object, object>)dct["axisDiffs"])
                {
                    DCSLuaDiffsAxisElement current = new DCSLuaDiffsAxisElement();
                    current.Keyname = (string)kvp.Key;
                    if (axisDiffs.ContainsKey(current.Keyname)) axisDiffs[current.Keyname] = current;
                    else axisDiffs.Add(current.Keyname, current);
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("name"))
                        current.Title = (string)((Dictionary<object, object>)kvp.Value)["name"];
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("added"))
                    {
                        Dictionary<object, object> dictAdded = (Dictionary<object, object>)((Dictionary<object, object>)kvp.Value)["added"];
                        foreach (KeyValuePair<object, object> kvpAdded in dictAdded)
                        {
                            if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("key"))
                            {
                                DCSAxisBind dab = new DCSAxisBind();
                                dab.key = (string)((Dictionary<object, object>)kvpAdded.Value)["key"];
                                if(((Dictionary<object, object>)kvpAdded.Value).ContainsKey("JPK")&& ((string)((Dictionary<object, object>)kvpAdded.Value)["JPK"]).Length>1)
                                {
                                    dab.JPRelName= (string)((Dictionary<object, object>)kvpAdded.Value)["JPK"];
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("JGRP"))
                                {
                                    foreach (KeyValuePair<object, object> kvpGroup in ((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["JGRP"]))
                                    {
                                        dab.Groups.Add((string)kvpGroup.Value);
                                        if (!InternalDataManagement.AllGroups.Contains((string)kvpGroup.Value))
                                        {
                                            InternalDataManagement.AllGroups.Add((string)kvpGroup.Value);
                                        }
                                    }
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("reformers"))
                                {
                                    foreach (KeyValuePair<object, object> kvpReformers in (Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["reformers"])
                                    {
                                        if (!dab.reformers.Contains((string)kvpReformers.Value))
                                            dab.reformers.Add((string)kvpReformers.Value);
                                        if (refMod != null)
                                        {
                                            if (refMod.modifiers.ContainsKey((string)kvpReformers.Value))
                                            {
                                                Modifier m = refMod.modifiers[(string)kvpReformers.Value];
                                                dab.modifiers.Add(m);
                                            }
                                        }
                                    }
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("filter"))
                                {
                                    DCSAxisFilter daf = new DCSAxisFilter();
                                    dab.filter = daf;
                                    daf.deadzone = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["deadzone"];
                                    daf.inverted = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["invert"];
                                    daf.slider = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["slider"];
                                    daf.saturationX = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationX"];
                                    daf.saturationY = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationY"];
                                    foreach (KeyValuePair<object, object> kvpCurve in (Dictionary<object, object>)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["curvature"])
                                    {
                                        daf.curvature.Add((double)kvpCurve.Value);
                                    }
                                }
                                if (!current.doesAddedContainKey(dab.key))
                                {
                                    current.added.Add(dab);
                                    current.removeItemFromRemoved(dab.key);
                                }
                            }
                        }
                    }
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("changed"))
                    {
                        Dictionary<object, object> dictAdded = (Dictionary<object, object>)((Dictionary<object, object>)kvp.Value)["changed"];
                        foreach (KeyValuePair<object, object> kvpAdded in dictAdded)
                        {
                            if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("key"))
                            {
                                DCSAxisBind dab = new DCSAxisBind();
                                dab.key = (string)((Dictionary<object, object>)kvpAdded.Value)["key"];
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("JPK") && ((string)((Dictionary<object, object>)kvpAdded.Value)["JPK"]).Length > 1)
                                {
                                    dab.JPRelName = (string)((Dictionary<object, object>)kvpAdded.Value)["JPK"];
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("JGRP"))
                                {
                                    foreach (KeyValuePair<object, object> kvpGroup in ((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["JGRP"]))
                                    {
                                        dab.Groups.Add((string)kvpGroup.Value);
                                        if (!InternalDataManagement.AllGroups.Contains((string)kvpGroup.Value))
                                        {
                                            InternalDataManagement.AllGroups.Add((string)kvpGroup.Value);
                                        }
                                    }
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("reformers"))
                                {
                                    foreach (KeyValuePair<object, object> kvpReformers in (Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["reformers"])
                                    {
                                        if (!dab.reformers.Contains((string)kvpReformers.Value))
                                            dab.reformers.Add((string)kvpReformers.Value);
                                        if (refMod != null)
                                        {
                                            if (refMod.modifiers.ContainsKey((string)kvpReformers.Value))
                                            {
                                                Modifier m = refMod.modifiers[(string)kvpReformers.Value];
                                                dab.modifiers.Add(m);
                                            }
                                        }
                                    }
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("filter"))
                                {
                                    DCSAxisFilter daf = new DCSAxisFilter();
                                    dab.filter = daf;
                                    daf.deadzone = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["deadzone"];
                                    daf.inverted = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["invert"];
                                    daf.slider = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["slider"];
                                    daf.saturationX = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationX"];
                                    daf.saturationY = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationY"];
                                    foreach (KeyValuePair<object, object> kvpCurve in (Dictionary<object, object>)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["curvature"])
                                    {
                                        daf.curvature.Add((double)kvpCurve.Value);
                                    }
                                }
                                if (!current.doesAddedContainKey(dab.key))
                                {
                                    current.added.Add(dab);
                                    current.removeItemFromRemoved(dab.key);
                                }
                            }
                        }
                    }
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("removed"))
                    {
                        Dictionary<object, object> dictRemoved = (Dictionary<object, object>)((Dictionary<object, object>)kvp.Value)["removed"];
                        foreach (KeyValuePair<object, object> kvpRemoved in dictRemoved)
                        {
                            if (((Dictionary<object, object>)kvpRemoved.Value).ContainsKey("key"))
                            {
                                DCSAxisBind dab = new DCSAxisBind();
                                dab.key = (string)((Dictionary<object, object>)kvpRemoved.Value)["key"];
                                if (!current.doesRemovedContainKey(dab.key))
                                {
                                    current.removed.Add(dab);
                                    current.removeItemFromAdded(dab.key);
                                }
                            }
                        }
                    }
                }
            }
            if (dct.ContainsKey("keyDiffs"))
            {
                foreach (KeyValuePair<object, object> kvp in (Dictionary<object, object>)dct["keyDiffs"])
                {
                    DCSLuaDiffsButtonElement current = new DCSLuaDiffsButtonElement();
                    current.Keyname = (string)kvp.Key;
                    if (keyDiffs.ContainsKey(current.Keyname)) keyDiffs[current.Keyname] = current;
                    else keyDiffs.Add(current.Keyname, current);
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("name"))
                        current.Title = (string)((Dictionary<object, object>)kvp.Value)["name"];
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("added"))
                    {
                        Dictionary<object, object> dictAdded = (Dictionary<object, object>)((Dictionary<object, object>)kvp.Value)["added"];
                        foreach (KeyValuePair<object, object> kvpAdded in dictAdded)
                        {
                            if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("key"))
                            {
                                DCSButtonBind dab = new DCSButtonBind();
                                dab.key = (string)((Dictionary<object, object>)kvpAdded.Value)["key"];
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("JPK") && ((string)((Dictionary<object, object>)kvpAdded.Value)["JPK"]).Length > 1)
                                {
                                    dab.JPRelName = (string)((Dictionary<object, object>)kvpAdded.Value)["JPK"];
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("JGRP"))
                                {
                                    foreach (KeyValuePair<object, object> kvpGroup in ((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["JGRP"]))
                                    {
                                        dab.Groups.Add((string)kvpGroup.Value);
                                        if (!InternalDataManagement.AllGroups.Contains((string)kvpGroup.Value))
                                        {
                                            InternalDataManagement.AllGroups.Add((string)kvpGroup.Value);
                                        }
                                    }
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("reformers"))
                                {
                                    foreach (KeyValuePair<object, object> kvpReformers in (Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["reformers"])
                                    {
                                        if (!dab.reformers.Contains((string)kvpReformers.Value))
                                            dab.reformers.Add((string)kvpReformers.Value);
                                        if (refMod != null)
                                        {
                                            if (refMod.modifiers.ContainsKey((string)kvpReformers.Value))
                                            {
                                                Modifier m = refMod.modifiers[(string)kvpReformers.Value];
                                                dab.modifiers.Add(m);
                                            }
                                        }
                                    }
                                }
                                if (!current.doesAddedContainKey(dab.key, dab.reformers))
                                {
                                    current.added.Add(dab);
                                    current.removeItemFromRemoved(dab.key, dab.reformers);
                                }
                            }
                        }
                    }
                    if (((Dictionary<object, object>)kvp.Value).ContainsKey("removed"))
                    {
                        Dictionary<object, object> dictRemoved = (Dictionary<object, object>)((Dictionary<object, object>)kvp.Value)["removed"];
                        foreach (KeyValuePair<object, object> kvpRemoved in dictRemoved)
                        {
                            DCSButtonBind dab=null;
                            if (((Dictionary<object, object>)kvpRemoved.Value).ContainsKey("key"))
                            {
                                dab = new DCSButtonBind();
                                dab.key = (string)((Dictionary<object, object>)kvpRemoved.Value)["key"];
                                
                            }
                            if (((Dictionary<object, object>)kvpRemoved.Value).ContainsKey("reformers")&&dab!=null)
                            {
                                foreach (KeyValuePair<object, object> kvpReformers in (Dictionary<object, object>)((Dictionary<object, object>)kvpRemoved.Value)["reformers"])
                                {
                                    if (!dab.reformers.Contains((string)kvpReformers.Value))
                                        dab.reformers.Add((string)kvpReformers.Value);
                                    if (refMod != null)
                                    {
                                        if (refMod.modifiers.ContainsKey((string)kvpReformers.Value))
                                        {
                                            Modifier m = refMod.modifiers[(string)kvpReformers.Value];
                                            dab.modifiers.Add(m);
                                        }
                                    }
                                }
                            }
                            if (!current.doesRemovedContainKey(dab.key, dab.reformers))
                            {
                                current.removed.Add(dab);
                                current.removeItemFromAdded(dab.key, dab.reformers);
                            }
                        }
                    }
                }
            }
        }
        public void FillUpWithDefaults()
        {
            DCSLuaInput def = JoystickName=="Keyboard"? getDefaultBinds(true):getDefaultBinds(false);
            if (def == null) return;
            foreach(KeyValuePair<string, DCSLuaDiffsAxisElement> kvp in def.axisDiffs)
            {
                if(!axisDiffs.ContainsKey(kvp.Key)) //Then add
                {
                    if (kvp.Value.removed.Count > 0&&CheckIfAxisIsSet(kvp.Value.removed[0].key)==null)
                    {
                        DCSLuaDiffsAxisElement d = new DCSLuaDiffsAxisElement();
                        d.Keyname = kvp.Key;
                        d.Title = kvp.Value.Title;
                        DCSAxisBind dab = new DCSAxisBind();
                        dab.key = kvp.Value.removed[0].key;
                        dab.reformers = kvp.Value.removed[0].reformers;
                        dab.modifiers = kvp.Value.removed[0].modifiers;
                        d.added.Add(dab);
                        
                        axisDiffs.Add(kvp.Key, d);
                    }
                }
            }
            foreach(KeyValuePair<string, DCSLuaDiffsButtonElement> kvp in def.keyDiffs)
            {
                if (!keyDiffs.ContainsKey(kvp.Key))
                {
                    if (kvp.Value.removed.Count > 0 && CheckIfButtonIsSet(kvp.Value.removed[0].key) == null)
                    {
                        DCSLuaDiffsButtonElement d = new DCSLuaDiffsButtonElement();
                        d.Keyname = kvp.Key;
                        d.Title = kvp.Value.Title;
                        DCSButtonBind dab = new DCSButtonBind();
                        dab.key= kvp.Value.removed[0].key;
                        dab.reformers=kvp.Value.removed[0].reformers;
                        dab.modifiers = kvp.Value.removed[0].modifiers;
                        d.added.Add(dab);
                        keyDiffs.Add(kvp.Key, d);
                    }
                }
            }
        }
        string CheckIfButtonIsSet(string btn, string[] refs = null)
        {
            foreach (KeyValuePair<string, DCSLuaDiffsButtonElement> kvp in keyDiffs)
            {
                for (int i = kvp.Value.added.Count - 1; i >= 0; --i)
                {
                    if (kvp.Value.added[i].key == btn)
                    {
                        bool mods = true;
                        if (refs != null)
                        {
                            if (refs.Length == kvp.Value.added[i].reformers.Count)
                            {
                                for (int j = refs.Length - 1; j >= 0; j--)
                                {
                                    if (!kvp.Value.added[i].reformers.Contains(refs[j]))
                                    {
                                        mods = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                mods = false;
                            }
                        }

                        if (mods)
                        {
                            return kvp.Key;
                        }
                    }
                }
            }
            return null;
        }
        string CheckIfAxisIsSet(string axis)
        {
            foreach (KeyValuePair<string, DCSLuaDiffsAxisElement> kvp in axisDiffs)
            {
                for (int i = kvp.Value.added.Count - 1; i >= 0; --i)
                {
                    if (kvp.Value.added[i].key == axis)
                    {
                        return kvp.Key;
                    }
                }
            }
            return null;
        }
        public void InvertedHTMLAnalyzeAxis(HtmlInputElementDCS element)
        {
            DCSLuaDiffsAxisElement diff;
            if (!axisDiffs.ContainsKey(element.id))
            {
                diff = new DCSLuaDiffsAxisElement();
                diff.Title = element.title;
                diff.Keyname = element.id;
            }
            else
            {
                diff = axisDiffs[element.id];
            }
            List<string> binds = new List<string>();
            if (element.bind.Length > 1)
                binds = element.bind.Split(';').ToList();
            else
                binds.Add(element.bind);
            for(int i=0; i<binds.Count; ++i)
            {
                if (!diff.doesRemovedContainKey(binds[i]))
                {
                    DCSAxisBind dab = new DCSAxisBind();
                    dab.key = binds[i];
                    diff.removed.Add(dab);
                }
            }
        }
        public void InvertedHTMLAnalyzeBtn(HtmlInputElementDCS element)
        {
            DCSLuaDiffsButtonElement diff;
            if (!keyDiffs.ContainsKey(element.id))
            {
                diff = new DCSLuaDiffsButtonElement();
                diff.Title = element.title;
                diff.Keyname = element.id;
            }
            else
            {
                diff = keyDiffs[element.id];
            }
            List<HtmlBindElement> binds = DBLogic.GetBindElmentsFromCell(element.bind);
            for(int i=0; i<binds.Count; ++i)
            {
                List<string> refs = new List<string>();
                List<Modifier> ms = new List<Modifier>();
                for (int j=0; j < binds[i].reformers.Count; ++j)
                {
                    string toAdd = binds[i].reformers[j] + "§Keyboard§" + binds[i].reformers[j];
                    refs.Add(toAdd);
                    ms.Add(Modifier.ReformerToMod(toAdd));
                }
                if (!diff.doesRemovedContainKey(binds[i].button))
                {
                    DCSButtonBind dab = new DCSButtonBind();
                    dab.key = binds[i].button;
                    dab.reformers = refs;
                    dab.modifiers = ms;
                    diff.removed.Add(dab);
                }
            }
        }
        DCSLuaInput getDefaultBinds(bool keyboard)
        {
            DCSLuaInput result = null;
            if (DCSIOLogic.EmptyOutputsDCS.ContainsKey(plane)&&!keyboard)
            {
                return DCSIOLogic.EmptyOutputsDCS[plane].Copy();
            }
            else if (DCSIOLogic.EmptyOutputsDCSKeyboard.ContainsKey(plane) && keyboard)
            {
                return DCSIOLogic.EmptyOutputsDCSKeyboard[plane].Copy();
            }
            return result;
        }
        public DCSLuaInput()
        {
            JoystickName = "";
            axisDiffs = new Dictionary<string, DCSLuaDiffsAxisElement>();
            keyDiffs = new Dictionary<string, DCSLuaDiffsButtonElement>();
            plane = "";
        }
    }
}
