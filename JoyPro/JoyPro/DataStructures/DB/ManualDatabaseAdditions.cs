using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class ManualDatabaseAdditions
    {
        public Dictionary<string, DCSPlane> DCSLib;
        public Dictionary<string, Dictionary<string, OtherGame>> OtherLib;
        public ManualDatabaseAdditions()
        {
            DCSLib = new Dictionary<string, DCSPlane>();
            OtherLib = new Dictionary<string, Dictionary<string, OtherGame>>();
        }

        public void WriteToTextFile(string filePath)
        {
            StreamWriter sw = new StreamWriter(filePath);
            foreach (KeyValuePair<string, DCSPlane> keyValuePair in DCSLib)
            {
                string toWrite = "";
                foreach(KeyValuePair<string, DCSInput> input in keyValuePair.Value.Axis)
                {
                    toWrite = input.Value.ID + "§cat§" + keyValuePair.Key + "§" + input.Value.Title + "§" + true.ToString() + "§DCS";
                    sw.WriteLine(toWrite);
                }
                foreach (KeyValuePair<string, DCSInput> input in keyValuePair.Value.Buttons)
                {
                    toWrite = input.Value.ID + "§cat§" + keyValuePair.Key + "§" + input.Value.Title + "§" + false.ToString() + "§DCS";
                    sw.WriteLine(toWrite);
                }
            }
            foreach(KeyValuePair<string, Dictionary<string, OtherGame>> kvp in OtherLib)
            {
                foreach(KeyValuePair<string, OtherGame> kvpPlane in kvp.Value)
                {
                    foreach(KeyValuePair<string, OtherGameInput> input in kvpPlane.Value.Axis)
                    {
                        string toWrite = input.Value.ID + "§" + input.Value.Category + "§" + kvpPlane.Key + "§" + input.Value.Title + "§" + true.ToString() + "§" + kvp.Key;
                        sw.WriteLine(toWrite);
                    }
                    foreach (KeyValuePair<string, OtherGameInput> input in kvpPlane.Value.Buttons)
                    {
                        string toWrite = input.Value.ID + "§" + input.Value.Category + "§" + kvpPlane.Key + "§" + input.Value.Title + "§" + false.ToString() + "§" + kvp.Key;
                        sw.WriteLine(toWrite);
                    }
                }
            }

            sw.Close();
            sw.Dispose();
        }
    }
}
