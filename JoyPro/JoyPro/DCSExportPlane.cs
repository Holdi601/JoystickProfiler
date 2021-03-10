using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class DCSExportPlane
    {
        public Dictionary<string, DCSLuaInput> joystickConfig;
        public Dictionary<string, Modifier> modifiers;
        public string plane;
        const string startFile = "local modifiers = {\n";
        const string endFile = "} \nreturn modifiers";

        public DCSExportPlane()
        {
            joystickConfig = new Dictionary<string, DCSLuaInput>();
            plane = "";
            modifiers = new Dictionary<string, Modifier>();
        }

        public void AnalyzeRawModLua(string content)
        {
            Dictionary<object, object> dct = MainStructure.CreateAttributeDictFromLua(content);
            foreach(KeyValuePair<object, object> kvp in dct)
            {
                string modName = (string)kvp.Key;
                Modifier m = new Modifier();
                m.name = modName;
                Dictionary<object, object> innerDict = (Dictionary<object, object>)kvp.Value;
                if (innerDict.ContainsKey("device"))
                    m.device = (string)innerDict["device"];
                if (innerDict.ContainsKey("key"))
                    m.key = (string)innerDict["key"];
                if (innerDict.ContainsKey("switch"))
                {
                    m.sw = (bool)innerDict["switch"];
                }
                modifiers.Add(modName, m);
            }
        }
        public void WriteModifiers(string path)
        {
            if (path == null || path.Length < 1 || !System.IO.Directory.Exists(path) || modifiers.Count < 1) return;
            System.IO.StreamWriter swr = new System.IO.StreamWriter(path+ "\\modifiers.lua");
            swr.Write(startFile);
            foreach(KeyValuePair<string, Modifier> kvp in modifiers)
            {
                swr.Write("\t[\"" + kvp.Key + "\"] = {\n");
                swr.Write("\t\t[\"device\"] = \""+kvp.Value.device+"\",\n");
                swr.Write("\t\t[\"key\"] = \"" + kvp.Value.key + "\",\n");
                if (kvp.Value.sw)
                {
                    swr.Write("\t\t[\"switch\"] = true,\n");
                }
                else
                {
                    swr.Write("\t\t[\"switch\"] = false,\n");
                }
                swr.Write("\t},\n");
            }
            swr.Write(endFile);
            swr.Close();
        }
    }
}
