using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class RelationItem
    {
        public string ID;
        Dictionary<string, bool> AIRCRAFT;
        DCSInput[] AllInputs;

        public RelationItem(string id)
        {
            ID = id;
            Init();
        }

        void Init()
        {
            AIRCRAFT = new Dictionary<string, bool>();
            AllInputs = MainStructure.GetAllInputsWithId(ID);
            for (int i = 0; i < AllInputs.Length; ++i)
                if (!AIRCRAFT.ContainsKey(AllInputs[i].Plane))
                    AIRCRAFT.Add(AllInputs[i].Plane, true);
        }

        public PlaneState GetStateAircraft(string plane)
        {
            if (!AIRCRAFT.ContainsKey(plane)) return PlaneState.NOT_EXISTENT;
            else if (AIRCRAFT[plane]) return PlaneState.ACTIVE;
            else return PlaneState.DISABLED;
        }

        public bool SwitchAircraftActivity(string plane)
        {
            if (AIRCRAFT.ContainsKey(plane))
            {
                AIRCRAFT[plane] = !AIRCRAFT[plane];
                return true;
            }
            return false;
        }

        public bool SetAircraftActivity(string plane, bool activity)
        {
            if (!AIRCRAFT.ContainsKey(plane)) return false;
            if (AIRCRAFT[plane] != activity)
                AIRCRAFT[plane] = activity;  
            return true;
        }

        public string GetInputDescription(string plane)
        {
            for (int i = 0; i < AllInputs.Length; ++i)
                if (AllInputs[i].Plane == plane)
                    return AllInputs[i].Title;
            return "";
        }
    }
}
