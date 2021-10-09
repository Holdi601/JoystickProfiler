using System;
using System.Collections.Generic;
using System.IO;
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
        const string endFile = "}\nreturn modifiers";

        public DCSExportPlane()
        {
            joystickConfig = new Dictionary<string, DCSLuaInput>();
            plane = "";
            modifiers = new Dictionary<string, Modifier>();
            initDefaultModifiers();
        }
        public DCSExportPlane Copy()
        {
            DCSExportPlane dep = new DCSExportPlane();
            dep.plane = plane;
            foreach(KeyValuePair<string, DCSLuaInput> kvp in joystickConfig)
            {
                if(!dep.joystickConfig.ContainsKey(kvp.Key))
                    dep.joystickConfig.Add(kvp.Key, kvp.Value.Copy());
            }
            foreach(KeyValuePair<string, Modifier> kvp in modifiers)
            {
                if(!dep.modifiers.ContainsKey(kvp.Key))
                    dep.modifiers.Add(kvp.Key, kvp.Value.Copy());
            }
            return dep;
        }
        public void AnalyzeRawModLua(string content)
        {
            Dictionary<object, object> dct = LUADataRead.CreateAttributeDictFromLua(content);
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
                if (innerDict.ContainsKey("JPK")&&((string)innerDict["JPK"]).Length>1)
                {
                    m.JPN = (string)innerDict["JPK"];
                    m.name = m.JPN;
                    modName = m.JPN;
                }
                if (innerDict.ContainsKey("switch"))
                {
                    m.sw = (bool)innerDict["switch"];
                }
                if (!modifiers.ContainsKey(modName))
                {
                    modifiers.Add(modName, m);
                }
                else
                {

                }
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
                swr.Write("\t\t[\"device\"] = \""+kvp.Value.device+ "\",\n");
                swr.Write("\t\t[\"key\"] = \"" + kvp.Value.key + "\",\n");
                swr.Write("\t\t[\"JPK\"] = \"" + kvp.Key + "\",\n");
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
        public bool ContainsModifier(string device, string key)
        {
            foreach(KeyValuePair<string, Modifier> kvp in modifiers)
            {
                if (device == kvp.Value.device && key == kvp.Value.key)
                    return true;
            }
            return false;
        }
        void initDefaultModifiers()
        {
            string dev = "Keyboard";
            bool sw = false;
            Modifier LAlt = new Modifier();
            LAlt.device = dev;
            LAlt.sw = sw;
            LAlt.key =  "LAlt";
            LAlt.name = "LAlt";

            Modifier LCtrl = new Modifier();
            LCtrl.device = dev;
            LCtrl.sw = sw;
            LCtrl.key =  "LCtrl";
            LCtrl.name = "LCtrl";

            Modifier LShift = new Modifier();
            LShift.device = dev;
            LShift.sw = sw;
            LShift.key =  "LShift";
            LShift.name = "LShift";

            Modifier LWin = new Modifier();
            LWin.device = dev;
            LWin.sw = sw;
            LWin.key = "LWin";
            LWin.name = "LWin";

            Modifier RAlt = new Modifier();
            RAlt.device = dev;
            RAlt.sw = sw;
            RAlt.key =  "RAlt";
            RAlt.name = "RAlt";

            Modifier RCtrl = new Modifier();
            RCtrl.device = dev;
            RCtrl.sw = sw;
            RCtrl.key = "RCtrl";
            RCtrl.name = "RCtrl";

            Modifier RShift = new Modifier();
            RShift.device = dev;
            RShift.sw = sw;
            RShift.key =  "RShift";
            RShift.name = "RShift";

            Modifier RWin = new Modifier();
            RWin.device = dev;
            RWin.sw = sw;
            RWin.key =  "RWin";
            RWin.name = "RWin";
            
            if (!modifiers.ContainsKey(LAlt.name)) modifiers.Add(LAlt.name, LAlt);
            if (!modifiers.ContainsKey(LCtrl.name)) modifiers.Add(LCtrl.name, LCtrl);
            if (!modifiers.ContainsKey(LShift.name)) modifiers.Add(LShift.name, LShift);
            if (!modifiers.ContainsKey(LWin.name)) modifiers.Add(LWin.name, LWin);
            if (!modifiers.ContainsKey(RAlt.name)) modifiers.Add(RAlt.name, RAlt);
            if (!modifiers.ContainsKey(RCtrl.name)) modifiers.Add(RCtrl.name, RCtrl);
            if (!modifiers.ContainsKey(RShift.name)) modifiers.Add(RShift.name, RShift);
            if (!modifiers.ContainsKey(RWin.name)) modifiers.Add(RWin.name, RWin);
        }
    }
}
