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

        public RelationItem(string id, string plane)
        {
            ID = id;
            Init(plane);
        }

        public RelationItem()
        {

        }

        public void CheckAgainstDB()
        {
            DCSInput[] dbItems = MainStructure.GetAllInputsWithId(ID);
            List<DCSInput> toRemove = new List<DCSInput>();
            List<DCSInput> toKeep = new List<DCSInput>();
            for(int i=0; i<AllInputs.Length; ++i)
            {
                DCSInput di = InputsContainPlane(dbItems, AllInputs[i].Plane);
                if (di!=null)
                {
                    if (di.Title==AllInputs[i].Title)
                    {
                        toKeep.Add(AllInputs[i]);
                    }
                    else
                    {
                        toKeep.Add(di);
                    }
                }
                else
                {
                    toRemove.Add(AllInputs[i]);
                }
            }
            for(int i=0; i<dbItems.Length; ++i)
            {
                DCSInput di = InputsContainPlane(AllInputs, dbItems[i].Plane);
                if (di == null)
                {
                    toKeep.Add(dbItems[i]);
                    AIRCRAFT.Add(dbItems[i].Plane, true);
                }
            }
            for(int i=0; i<toRemove.Count; ++i)
            {
                AIRCRAFT.Remove(toRemove[i].Plane);
            }
            AllInputs = toKeep.ToArray();
        }

        public RelationItem Copy()
        {
            RelationItem ri = new RelationItem();
            ri.ID = ID;
            ri.AIRCRAFT = new Dictionary<string, bool>();
            foreach(KeyValuePair<string, bool> kvp in AIRCRAFT)
            {
                ri.AIRCRAFT.Add(kvp.Key, kvp.Value);
            }
            ri.AllInputs = new DCSInput[AllInputs.Length];
            for(int i=0; i<AllInputs.Length; ++i)
            {
                ri.AllInputs[i] = AllInputs[i].Copy();
            }
            return ri;
        }

        DCSInput InputsContainPlane(DCSInput[] inputs, string plane)
        {
            for(int i=0; i<inputs.Length; ++i)
            {
                if (inputs[i].Plane == plane) return inputs[i];
            }
            return null;
        }
        void Init()
        {
            AIRCRAFT = new Dictionary<string, bool>();
            AllInputs = MainStructure.GetAllInputsWithId(ID);
            for (int i = 0; i < AllInputs.Length; ++i)
                if (!AIRCRAFT.ContainsKey(AllInputs[i].Plane))
                    AIRCRAFT.Add(AllInputs[i].Plane, true);
        }

        void Init(string plane)
        {
            AIRCRAFT = new Dictionary<string, bool>();
            AllInputs = MainStructure.GetAllInputsWithId(ID);
            for (int i = 0; i < AllInputs.Length; ++i)
                if (!AIRCRAFT.ContainsKey(AllInputs[i].Plane))
                    if (AllInputs[i].Plane == plane)
                        AIRCRAFT.Add(AllInputs[i].Plane, true);
                    else
                        AIRCRAFT.Add(AllInputs[i].Plane, false);
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

        public List<string> GetActiveAircraftList()
        {
            List<string> result = new List<string>();
            foreach(KeyValuePair<string, bool> kvp in AIRCRAFT)
            {
                if (kvp.Value)
                    result.Add(kvp.Key);
            }
            return result;
        }
    }
}
