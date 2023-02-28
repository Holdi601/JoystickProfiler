using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace JoyPro.StarCitizen
{
    public class SCInputItem
    {
        public string KeyboardInput;
        public string GamepadInput;
        public string JoystickInput;
    }
    public static class SCIOLogic
    {
        public static Dictionary<string, List<string>> categoryInputs = new Dictionary<string, List<string>>();
        public static Dictionary<string, int> JoyIDLocal = new Dictionary<string, int>();
        public static Dictionary<string, int> KeyboardIDLocal = new Dictionary<string, int>();
        public static Dictionary<string, int> GamepadIDLocal = new Dictionary<string, int>();
        public static Dictionary<string, Dictionary<string, SCInputItem>> ExportPrep = new Dictionary<string, Dictionary<string, SCInputItem>>();
        public static string version = "1";
        public static string optionsVersion = "2";
        public static string rebindVersion = "2";
        public static string profileName = "default";
        public static string keyboardProduct = "";
        public static string gamepadProduct = "";
        public static void ReadLocalActions()
        {
            string addativePathToActions = "LIVE\\USER\\Client\\0\\Profiles\\default\\actionmaps.xml";
            StreamReader sr = new StreamReader(MiscGames.StarCitizen + "\\" + addativePathToActions);
            string instanceStr = "instance=\"";
            string productStr = "Product=\"";
            string endquote = "\"";
            int quoteend = -1;
            string keyboardoptions = "<options type=\"keyboard\"";
            string gamepadoptions = "<options type=\"gamepad\"";
            string joystickoptions = "<options type=\"joystick\"";
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if(line.Contains("<modifiers />"))
                {
                    break;
                }else if (line.Contains("<ActionProfiles version=\""))
                {
                    string versionStr = "version=\"";
                    string optionsStr = "optionsVersion=\"";
                    string rebindVersionStr = "rebindVersion=\"";
                    string profileStr = "profileName=\"";
                    
                    int indver = line.IndexOf(versionStr);
                    string verStart = line.Substring(indver+versionStr.Length);
                    quoteend = verStart.IndexOf(endquote);
                    version = verStart.Substring(0, quoteend);
                    indver = verStart.IndexOf(optionsStr);
                    verStart=verStart.Substring(indver+optionsStr.Length);
                    quoteend = verStart.IndexOf(endquote);
                    optionsVersion=verStart.Substring(0, quoteend);
                    indver = verStart.IndexOf(rebindVersionStr);
                    verStart = verStart.Substring(indver+rebindVersionStr.Length);
                    quoteend=verStart.IndexOf(endquote);
                    rebindVersion=verStart.Substring(0, quoteend);
                    indver = verStart.IndexOf(profileStr);
                    verStart=verStart.Substring(indver+profileStr.Length);
                    quoteend=verStart.IndexOf(endquote);
                    profileName=verStart.Substring(0, quoteend);
                }else if(line.Contains(keyboardoptions)||
                    line.Contains(gamepadoptions)||
                    line.Contains(joystickoptions))
                {
                    string to = line.Contains(keyboardoptions) ? "keyboard" : line.Contains(gamepadoptions) ? "gamepad" : "joystick";
                    int indx = line.IndexOf(instanceStr);
                    line=line.Substring(indx+instanceStr.Length);
                    quoteend=line.IndexOf(endquote);
                    int instanceId = Convert.ToInt32(line.Substring(0,quoteend));
                    indx=line.IndexOf(productStr);
                    line=line.Substring(indx+productStr.Length);
                    quoteend=line.IndexOf(endquote);
                    string product = line.Substring(0, quoteend);
                    Dictionary<string, int> dictRef=null;
                    switch (to)
                    {
                        case "keyboard":
                            dictRef = KeyboardIDLocal;
                            break;
                        case "gamepad":
                            dictRef=GamepadIDLocal; 
                            break;
                        case "joystick":
                            dictRef = JoyIDLocal;
                            break;
                    }
                    if (!dictRef.ContainsKey(product)) dictRef.Add(product, instanceId);
                }
            }
            string lastCategory = "";
            string lastId="";
            SCInputItem lastSCI = new SCInputItem();
            string catStart = "<actionmap name=\"";
            string idStart = "<action name=\"";
            string rebindStart = "<rebind input=\"";
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Contains(catStart))
                {
                    int indx = line.IndexOf(catStart);
                    line = line.Substring(indx + catStart.Length);
                    quoteend = line.IndexOf(endquote);
                    lastCategory = line.Substring(0, quoteend);
                    if (!ExportPrep.ContainsKey(lastCategory)) ExportPrep.Add(lastCategory, new Dictionary<string, SCInputItem>());
                } else if (line.Contains(idStart))
                {
                    int indx = line.IndexOf(idStart);
                    line = line.Substring(indx + idStart.Length);
                    quoteend = line.IndexOf(endquote);
                    lastId = line.Substring(0, quoteend);
                    if (!ExportPrep[lastCategory].ContainsKey(lastId)) ExportPrep[lastCategory].Add(lastId, new SCInputItem());
                } else if (line.Contains(rebindStart))
                {
                    int indx = line.IndexOf(rebindStart);
                    line = line.Substring(indx + rebindStart.Length);
                    quoteend = line.IndexOf(endquote);
                    string input = line.Substring(0, quoteend);
                    if (input.StartsWith("kb")) ExportPrep[lastCategory][lastId].KeyboardInput = input;
                    else if (input.StartsWith("js")) ExportPrep[lastCategory][lastId].JoystickInput = input;
                    else ExportPrep[lastCategory][lastId].GamepadInput = input;
                }
            }
            
        }
        public static bool HasIllegalModifier(Bind b)
        {
            foreach(string reformer in b.AllReformers)
            {
                string[] prts = reformer.Split('§');
                if (prts[1].ToLower().Trim() != "keyboard") return true;
                if (prts[2].ToLower().Trim() != "LeftAlt" &&
                    prts[2].ToLower().Trim() != "RightAlt" &&
                    prts[2].ToLower().Trim() != "LeftShift") return true;
            }
            return false;
        }
        public static void WriteOut(List<Bind> binds, OutputType outputType)
        {
            Dictionary<string, string> connectedSticks = JoystickReader.GetConnectedJoysticks();
        }
        public static void RemoveKey(string key)
        {
            for(int i=ExportPrep.Count-1; i>=0; i--)
            {
                for(int j=ExportPrep.ElementAt(i).Value.Count-1; j>=0; j--)
                {
                    if (key.StartsWith("kb"))
                    {
                        if(ExportPrep.ElementAt(i).Value.ElementAt(j).Value.KeyboardInput.ToLower()==key.ToLower())
                        {
                            ExportPrep.ElementAt(i).Value.ElementAt(j).Value.KeyboardInput = "";
                        }
                    }
                    else if (key.StartsWith("js"))
                    {
                        if (ExportPrep.ElementAt(i).Value.ElementAt(j).Value.JoystickInput.ToLower() == key.ToLower())
                        {
                            ExportPrep.ElementAt(i).Value.ElementAt(j).Value.JoystickInput = "";
                        }
                    }
                    else
                    {
                        if (ExportPrep.ElementAt(i).Value.ElementAt(j).Value.GamepadInput.ToLower() == key.ToLower())
                        {
                            ExportPrep.ElementAt(i).Value.ElementAt(j).Value.GamepadInput = "";
                        }
                    }
                    if(ExportPrep.ElementAt(i).Value.ElementAt(j).Value.GamepadInput.Length==0&&
                        ExportPrep.ElementAt(i).Value.ElementAt(j).Value.KeyboardInput.Length==0&&
                        ExportPrep.ElementAt(i).Value.ElementAt(j).Value.JoystickInput.Length==0)
                    {
                        string toRemov = ExportPrep.ElementAt(i).Value.ElementAt(j).Key;
                        ExportPrep.ElementAt(i).Value.Remove(toRemov);
                    }
                }
            }
        }
    }
}
