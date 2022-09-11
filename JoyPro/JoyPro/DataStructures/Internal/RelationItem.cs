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
        public string Game;
        OtherGameInput[] OtherInputs;

        public string RandomDescription()
        {
            Random r = new Random();

            List<string> planes = GetActiveAircraftList();
            while (planes.Count > 0)
            {
                int rnd = r.Next(planes.Count);
                string result = GetInputDescription(planes[rnd]);
                if (result.Length > 0) return result;
                else planes.RemoveAt(rnd);
            }

            return "ERROR";
        }
        public RelationItem(string id, string game, Relation r)
        {
            AIRCRAFT = new Dictionary<string, bool>();
            ID = id;
            Game = game;
            if (game == "DCS") InitDCS(r);
            else InitOtherGame();

        }
        public RelationItem(string id, string plane, string game)
        {
            AIRCRAFT = new Dictionary<string, bool>();
            ID = id;
            Game = game;
            if (game == null || game == "" || game == "DCS")
            {
                InitDCS(plane);
            }
            else
            {
                InitOtherGame();
            }

        }
        public RelationItem()
        {
            AIRCRAFT = new Dictionary<string, bool>();
        }
        //Confirming names because it has been saved against DB
        public List<object> CheckAgainstDB(Relation r)
        {
            List<object> toReturn = new List<object>();
            CurrentBind cb = new CurrentBind(r);
            if (Game == null || Game == "DCS" || Game == "")
            {
                DCSInput[] dbItems = DBLogic.GetAllDCSInputsWithId(ID);
                Dictionary<DCSInput, bool> toRemove = new Dictionary<DCSInput, bool>();
                List<DCSInput> toKeep = new List<DCSInput>();
                for (int i = AllInputs.Length-1; i >=0; i--)
                {
                    DCSInput di = InputsContainPlane(dbItems, AllInputs[i].Plane);
                    if (di != null)
                    {
                        if (di.Title.ToLower() == AllInputs[i].Title.ToLower())
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
                        //does item exist under a different id and plane
                        DCSInput[] replacements = DBLogic.GetAllDCSInputWithTitleAndPlane(AllInputs[i].Title, AllInputs[i].Plane, AllInputs[i].IsAxis);
                        if (replacements.Length > 0)
                        {
                            toReturn.Add(replacements[0]);
                        }
                        if(AIRCRAFT.ContainsKey(AllInputs[i].Plane))AIRCRAFT.Remove(AllInputs[i].Plane);
                    }
                }
                for (int i = 0; i < dbItems.Length; ++i)
                {
                    DCSInput di = InputsContainPlane(AllInputs, dbItems[i].Plane);
                    if (di == null)
                    {
                        DCSInput da = InputsContainPlane(toKeep.ToArray(), dbItems[i].Plane);
                        toKeep.Add(dbItems[i]);
                        bool nothingElseBound = true;
                        if(cb.found==true&& MainStructure.msave.AutoAddDBItems == true)
                        {
                            string desc = InternalDataManagement.GetDescriptionForJoystickButtonGamePlane(cb.joystick, cb.modbtn, "DCS", dbItems[i].Plane);
                            if(desc.Length>0)nothingElseBound = false;
                        }

                        if (da == null&&r.GetPlaneRelationState(dbItems[i].Plane, Game, this)<1&&MainStructure.msave.AutoAddDBItems==true&&nothingElseBound)
                        {
                            AIRCRAFT.Add(dbItems[i].Plane, true);
                        }
                        else
                        {
                            AIRCRAFT.Add(dbItems[i].Plane, false);
                        }
                    }
                }
                AllInputs = toKeep.ToArray();
            }
            else
            {
                OtherGameInput[] dbItems = DBLogic.GetAllOtherGameInputsWithId(ID, Game);
                Dictionary<OtherGameInput, bool> toRemove = new Dictionary<OtherGameInput, bool>();
                List<OtherGameInput> toKeep = new List<OtherGameInput>();
                for (int i = 0; i < OtherInputs.Length; ++i)
                {
                    OtherGameInput di = InputsContainPlane(dbItems, OtherInputs[i].Plane);
                    if (di != null)
                    {
                        if (di.Title.ToLower() == OtherInputs[i].Title.ToLower())
                        {
                            toKeep.Add(OtherInputs[i]);
                        }
                        else
                        {
                            toKeep.Add(di);
                        }
                    }
                    else
                    {
                        OtherGameInput[] replacements = DBLogic.GetAllOtherGameInputWithTitleAndPlane(OtherInputs[i].Title, OtherInputs[i].Plane, OtherInputs[i].Game, OtherInputs[i].IsAxis);
                        //does item exist under a different id and plane
                        if (replacements.Length > 0)
                        {
                            toReturn.Add(replacements[0]);
                        }
                        if (AIRCRAFT.ContainsKey(OtherInputs[i].Plane)) AIRCRAFT.Remove(OtherInputs[i].Plane);
                    }
                }
                for (int i = 0; i < dbItems.Length; ++i)
                {
                    OtherGameInput di = InputsContainPlane(OtherInputs, dbItems[i].Plane);
                    if (di == null)
                    {
                        OtherGameInput da = InputsContainPlane(toKeep.ToArray(), dbItems[i].Plane);
                        if (da == null)
                        {
                            toKeep.Add(dbItems[i]);
                            bool nothingElseBound = true;
                            if (cb.found == true && MainStructure.msave.AutoAddDBItems == true)
                            {
                                string desc = InternalDataManagement.GetDescriptionForJoystickButtonGamePlane(cb.joystick, cb.modbtn, Game, dbItems[i].Plane);
                                if (desc.Length > 0) nothingElseBound = false;
                            }

                            if (da == null && r.GetPlaneRelationState(dbItems[i].Plane, Game, this) < 1 && MainStructure.msave.AutoAddDBItems == true)
                            {
                                AIRCRAFT.Add(dbItems[i].Plane, true);
                            }
                            else
                            {
                                AIRCRAFT.Add(dbItems[i].Plane, false);
                            }
                        }
                    }
                }
                OtherInputs = toKeep.ToArray();
            }
            return toReturn;
        }
        public RelationItem Copy()
        {
            RelationItem ri = new RelationItem();
            ri.ID = ID;
            ri.Game = Game;
            ri.AIRCRAFT = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, bool> kvp in AIRCRAFT)
            {
                ri.AIRCRAFT.Add(kvp.Key, kvp.Value);
            }
            if (AllInputs != null)
            {
                ri.AllInputs = new DCSInput[AllInputs.Length];
                for (int i = 0; i < AllInputs.Length; ++i)
                {
                    ri.AllInputs[i] = AllInputs[i].Copy();
                }
            }
            else
            {
                ri.OtherInputs = new OtherGameInput[OtherInputs.Length];
                for (int i = 0; i < OtherInputs.Length; ++i)
                {
                    ri.OtherInputs[i] = OtherInputs[i].Copy();
                }
            }


            return ri;
        }
        OtherGameInput InputsContainPlane(OtherGameInput[] inputs, string plane)
        {
            for (int i = 0; i < inputs.Length; ++i)
                if (inputs[i].Plane.ToLower() == plane.ToLower()) return inputs[i];
            return null;
        }
        DCSInput InputsContainPlane(DCSInput[] inputs, string plane)
        {
            for (int i = 0; i < inputs.Length; ++i)
            {
                if (inputs[i].Plane.ToLower() == plane.ToLower()) return inputs[i];
            }
            return null;
        }
        void InitDCS(Relation r)
        {
            AIRCRAFT = new Dictionary<string, bool>();
            AllInputs = DBLogic.GetAllDCSInputsWithId(ID);
            for (int i = 0; i < AllInputs.Length; ++i)
            {
                if (!AIRCRAFT.ContainsKey(AllInputs[i].Plane)&& r.GetPlaneRelationState(AllInputs[i].Plane, "DCS") < 1)
                {
                    AIRCRAFT.Add(AllInputs[i].Plane, true);
                }
            } 
        }
        void InitOtherGame()
        {
            AIRCRAFT = new Dictionary<string, bool>();
            OtherInputs = DBLogic.GetAllOtherGameInputsWithId(ID, Game);
            for (int i = 0; i < OtherInputs.Length; ++i)
                if (!AIRCRAFT.ContainsKey(Game))
                    AIRCRAFT.Add(Game, true);
        }
        void InitDCS(string plane)
        {
            AIRCRAFT = new Dictionary<string, bool>();
            AllInputs = DBLogic.GetAllDCSInputsWithId(ID);
            for (int i = 0; i < AllInputs.Length; ++i)
                if (!AIRCRAFT.ContainsKey(AllInputs[i].Plane))
                    if (AllInputs[i].Plane == plane)
                        AIRCRAFT.Add(AllInputs[i].Plane, true);
                    else
                        AIRCRAFT.Add(AllInputs[i].Plane, false);
        }
        public PlaneState GetStateAircraft(string plane)
        {

            if (AIRCRAFT == null) AIRCRAFT = new Dictionary<string, bool>();
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
        public void DeleteAircraftFromActivity(string plane)
        {
            if (AIRCRAFT.ContainsKey(plane)) AIRCRAFT.Remove(plane);
        }
        public string GetInputDescription(string plane)
        {
            if (Game == null || Game == "" || Game == "DCS")
            {
                for (int i = 0; i < AllInputs.Length; ++i)
                    if (AllInputs[i].Plane.ToLower() == plane.ToLower())
                        return AllInputs[i].Title;
            }
            else
            {
                for (int i = 0; i < OtherInputs.Length; ++i)
                    if (OtherInputs[i].Plane.ToLower() == plane.ToLower())
                        return OtherInputs[i].Title;
            }

            return "";
        }
        public List<string> GetActiveAircraftList()
        {
            List<string> result = new List<string>();
            foreach (KeyValuePair<string, bool> kvp in AIRCRAFT)
            {
                if (kvp.Value)
                    result.Add(kvp.Key);
            }
            return result;
        }
    }
}
