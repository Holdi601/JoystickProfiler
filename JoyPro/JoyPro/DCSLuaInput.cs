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
        public void writeLua(string path)
        {
            StreamWriter swr = new StreamWriter(path);
            swr.Write(filestart);
            if (axisDiffs.Count > 0)
            {
                swr.Write("\t[\"axisDiffs\"] = {\n");
                foreach(KeyValuePair<string, DCSLuaDiffsAxisElement> kvp in axisDiffs)
                {
                    swr.Write("\t\t[\"" + kvp.Key + "\"] = {\n");
                    swr.Write("\t\t\t[\"name\"] = \"" + kvp.Value.Title + "\",\n");
                    if (kvp.Value.added.Count > 0)
                    {
                        swr.Write("\t\t\t[\"added\"] = {\n");
                        swr.Write("\t\t\t\t[1] = {\n");
                        swr.Write("\t\t\t\t\t[\"key\"] = \"" + kvp.Value.added[0].key + "\",\n");
                        if (kvp.Value.added[0].filter != null)
                        {
                            swr.Write("\t\t\t\t\t[\"filter\"] = {\n");
                            swr.Write("\t\t\t\t\t\t[\"curvature\"] = {\n");
                            swr.Write("\t\t\t\t\t\t\t[1] = " + kvp.Value.added[0].filter.curviture[0].ToString(new CultureInfo("en-US")) + ",\n");
                            swr.Write("\t\t\t\t\t\t},\n");
                            swr.Write("\t\t\t\t\t\t[\"deadzone\"] = " + kvp.Value.added[0].filter.deadzone.ToString(new CultureInfo("en-US")) + ",\n");
                            swr.Write("\t\t\t\t\t\t[\"invert\"] = " + kvp.Value.added[0].filter.inverted.ToString().ToLower() + ",\n");
                            swr.Write("\t\t\t\t\t\t[\"saturationX\"] = " + kvp.Value.added[0].filter.saturationX.ToString(new CultureInfo("en-US")) + ",\n");
                            swr.Write("\t\t\t\t\t\t[\"saturationY\"] = " + kvp.Value.added[0].filter.saturationY.ToString(new CultureInfo("en-US")) + ",\n");
                            swr.Write("\t\t\t\t\t\t[\"slider\"] = " + kvp.Value.added[0].filter.slider.ToString().ToLower() + ",\n");
                            swr.Write("\t\t\t\t\t},\n");
                        }
                        swr.Write("\t\t\t\t},\n");
                        swr.Write("\t\t\t},\n");
                    }
                    if(kvp.Value.removed.Count>0)
                    {
                        swr.Write("\t\t\t[\"removed\"] = {\n");
                        swr.Write("\t\t\t\t[1] = {\n");
                        swr.Write("\t\t\t\t\t[\"key\"] = \""+kvp.Value.removed[0].key+"\",\n");
                        swr.Write("\t\t\t\t},\n");
                        swr.Write("\t\t\t},\n");
                    }
                    swr.Write("\t\t},\n");
                }
                swr.Write("\t},\n");
            }
            if(keyDiffs.Count > 0)
            {
                swr.Write("\t[\"keyDiffs\"] = {\n");
                foreach(KeyValuePair<string, DCSLuaDiffsButtonElement> kvp in keyDiffs)
                {
                    swr.Write("\t\t[\"" + kvp.Key + "\"] = {\n");
                    swr.Write("\t\t\t[\"name\"] = \"" + kvp.Value.Title + "\",\n");
                    if (kvp.Value.added.Count > 0)
                    {
                        swr.Write("\t\t\t[\"added\"] = {\n");
                        swr.Write("\t\t\t\t[1] = {\n");
                        swr.Write("\t\t\t\t\t[\"key\"] = \"" + kvp.Value.added[0].key + "\",\n");
                        swr.Write("\t\t\t\t},\n");
                        swr.Write("\t\t\t},\n");
                    }
                    else if (kvp.Value.removed.Count > 0)
                    {
                        swr.Write("\t\t\t[\"removed\"] = {\n");
                        swr.Write("\t\t\t\t[1] = {\n");
                        swr.Write("\t\t\t\t\t[\"key\"] = \"" + kvp.Value.removed[0].key + "\",\n");
                        swr.Write("\t\t\t\t},\n");
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
            foreach(KeyValuePair<string, DCSLuaDiffsAxisElement> kvp in axisDiffs)
            {
                result.axisDiffs.Add(kvp.Key, kvp.Value.Copy());
            }
            foreach(KeyValuePair<string, DCSLuaDiffsButtonElement> kvp in keyDiffs)
            {
                result.keyDiffs.Add(kvp.Key, kvp.Value.Copy());
            }
            return result;
        }
        public void AnalyzeRawLuaInput(string content)
        {
            if (!content.Contains('{')) return;
            string cleaned = MainStructure.GetContentBetweenSymbols(content, "{", "}");
            Dictionary<object, object> dct = MainStructure.CreateAttributeDictFromLua(content);
            if (dct.ContainsKey("axisDiffs"))
            {
                foreach(KeyValuePair<object, object> kvp in (Dictionary<object,object>)dct["axisDiffs"])
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
                        foreach(KeyValuePair<object, object> kvpAdded in dictAdded)
                        {
                            if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("key"))
                            {
                                DCSAxisBind dab = new DCSAxisBind();
                                dab.key = (string)((Dictionary<object, object>)kvpAdded.Value)["key"];
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("filter"))
                                {
                                    DCSAxisFilter daf = new DCSAxisFilter();
                                    dab.filter = daf;
                                    daf.deadzone=(double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["deadzone"];
                                    daf.inverted = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["invert"];
                                    daf.slider = (bool)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["slider"];
                                    daf.saturationX = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationX"];
                                    daf.saturationY = (double)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["saturationY"];
                                    foreach(KeyValuePair<object,object> kvpCurve in (Dictionary<object, object>)((Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["filter"])["curvature"])
                                    {
                                        daf.curviture.Add((double)kvpCurve.Value);
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
                                if (!current.doesAddedContainKey(dab.key))
                                {
                                    current.added.Add(dab);
                                    current.removeItemFromRemoved(dab.key);
                                }
                                if (((Dictionary<object, object>)kvpAdded.Value).ContainsKey("reformers"))
                                {
                                    foreach(KeyValuePair<object, object> kvpReformers in (Dictionary<object, object>)((Dictionary<object, object>)kvpAdded.Value)["reformers"])
                                    {
                                        if (!dab.reformers.Contains((string)kvpReformers.Value))
                                            dab.reformers.Add((string)kvpReformers.Value);
                                    }
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
                                DCSButtonBind dab = new DCSButtonBind();
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
