using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public enum PlaneState { ACTIVE, DISABLED, NOT_EXISTENT, ERROR }
    [Serializable]
    public class Relation
    {
        public string NAME;
        List<RelationItem> NODES;
        public bool ISAXIS;
        public Bind bind = null;
        public List<string> Groups;

        public Relation()
        {
            NODES = new List<RelationItem>();
            Groups = new List<string>();
        }
        public List<string> GamesInRelation()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < NODES.Count; ++i)
            {
                string gameCorrected = NODES[i].Game;
                if (gameCorrected == null || gameCorrected == "")
                    gameCorrected = "DCS";
                if (!result.Contains(gameCorrected))
                {
                    result.Add(gameCorrected);
                }
            }
            return result;
        }

        public string GetRandomDescription()
        {
            if(NODES.Count > 0)
            {
                Random r = new Random();
                return NODES[r.Next(NODES.Count)].RandomDescription();
            }
            return "ERROR"; 
        }
        public Dictionary<string, int> GetPlaneSetState(string game)
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            if (!DBLogic.Planes.ContainsKey(game)) return null;
            for (int i = 0; i < DBLogic.Planes[game].Count; ++i)
            {
                results.Add(DBLogic.Planes[game][i], GetPlaneRelationState(DBLogic.Planes[game][i], game));
            }
            return results;
        }
        public Relation Copy()
        {
            Relation r = new Relation();
            r.ISAXIS = ISAXIS;
            r.NAME = NAME;
            for (int i = 0; i < NODES.Count; ++i)
            {
                r.NODES.Add(NODES[i].Copy());
            }
            if (bind != null)
                r.bind = bind.Copy(r);
            if (Groups != null)
            {
                for(int i=0; i<Groups.Count; ++i)
                {
                    r.Groups.Add(Groups[i]);
                }
            }
            return r;
        }
        public void CheckNamesAgainstDB()
        {
            List<DCSInput> toAdd = new List<DCSInput>();
            List<OtherGameInput> oToAdd = new List<OtherGameInput>();
            foreach (RelationItem r in NODES)
            {
                List<object> toAddInner = r.CheckAgainstDB(this);
                for(int i=0; i<toAddInner.Count; ++i)
                {
                    if(toAddInner[i] is DCSInput)
                    {
                        toAdd.Add((DCSInput)toAddInner[i]);
                    }else if(toAddInner[i] is OtherGameInput)
                    {
                        oToAdd.Add((OtherGameInput)toAddInner[i]);
                    }
                }
            }
            for(int i=0; i<toAdd.Count; ++i)
            {
                AddNode(toAdd[i].ID, "DCS", toAdd[i].IsAxis, toAdd[i].Plane);
            }
            for(int i=0; i<oToAdd.Count; ++i)
            {
                AddNode(oToAdd[i].ID, oToAdd[i].Game, oToAdd[i].IsAxis, oToAdd[i].Plane);
            }

        }
        public int GetPlaneRelationState(string plane, string game)
        {
            int counter = 0;
            for (int i = 0; i < NODES.Count; ++i)
            {
                if (NODES[i].Game == null || NODES[i].Game.Length < 1) NODES[i].Game = "DCS";
                PlaneState ps = NODES[i].GetStateAircraft(plane);
                if (ps == PlaneState.ACTIVE&&NODES[i].Game==game) ++counter;
            }
            return counter;
        }
        public bool AddNode(string id, string game, bool axis, string plane = "")
        {
            if (game == null || game.Length < 1) game = "DCS";
            if (NodesContainId(id) && plane.Length < 1) return false;
            if (NODES.Count < 1)
            {
                ISAXIS = axis;
            }
            else
            {
                if (ISAXIS != axis) return false;
            }
            if (plane.Length < 1)
                NODES.Add(new RelationItem(id, game));
            else
            {
                bool found = false;
                int oof = -1;
                for (int i = 0; i < NODES.Count; ++i)
                {
                    PlaneState ps = NODES[i].GetStateAircraft(plane);
                    if (NODES[i].ID == id && (ps == PlaneState.ACTIVE || ps == PlaneState.DISABLED))
                    {
                        found = true;
                        oof = i;
                        break;
                    }
                }
                if (found)
                {
                    NODES[oof].SetAircraftActivity(plane, true);
                }
                else
                {
                    NODES.Add(new RelationItem(id, plane, game));
                }
            }
            Console.WriteLine("Relation Item Added");
            return true;
        }
        public bool RemoveNode(string id, string game)
        {
            if (!NodesContainId(id)) return false;
            RelationItem ri = GetRelationItem(id, game);
            NODES.Remove(ri);
            return true;
        }
        public RelationItem GetRelationItemForPlaneDCS(string plane)
        {
            for (int i = 0; i < NODES.Count; ++i)
            {
                PlaneState ps = NODES[i].GetStateAircraft(plane);
                if (ps == PlaneState.ACTIVE) return NODES[i];
            }
            return null;
        }
        public RelationItem GetRelationItem(string id, string game)
        {
            for (int i = 0; i < NODES.Count; i++)
            {
                if (id.ToUpper() == NODES[i].ID.ToUpper()&&
                    game.ToUpper()==NODES[i].Game.ToUpper()) return NODES[i];
            }
            return null;
        }
        bool NodesContainId(string id)
        {
            for (int i = 0; i < NODES.Count; i++)
            {
                if (id == NODES[i].ID) return true;
            }
            return false;
        }
        public bool IsEmpty()
        {
            if (NODES.Count > 0) return false;
            return true;
        }
        public List<RelationItem> AllRelations()
        {
            return NODES;
        }
        public string GetDescriptionForGamePlane(string Game, string Plane)
        {
            for(int i=0; i<NODES.Count; ++i)
            {
                if (NODES[i].Game == null) NODES[i].Game = "DCS";
                if (NODES[i].Game.ToLower() == Game.ToLower())
                {
                    string desc= NODES[i].GetInputDescription(Plane);
                    if (NODES[i].GetStateAircraft(Plane) == PlaneState.ACTIVE && desc.Length > 1)
                        return desc;
                }
            }
            return "";
        }
        public KeyValuePair<int, int> CleanRelation()
        {
            List<RelationItem> toDelete = new List<RelationItem>();
            int itemsToDelete = 0;
            int aircraftToDelete = 0;
            for(int i=0; i<NODES.Count; ++i)
            {
                List<SearchQueryResults> results = DBLogic.SearchBinds(new string[] { NODES[i].ID }, false, false);
                if (results.Count == 0)
                {
                    toDelete.Add(NODES[i]);
                    break;
                }
                bool found = false;
                List<string> planes = NODES[i].GetActiveAircraftList();
                Dictionary<string, bool> planesStillExist = new Dictionary<string, bool>();
                foreach (string p in planes) if (!planesStillExist.ContainsKey(p)) planesStillExist.Add(p, false);
                for(int j=0; j<results.Count; ++j)
                {
                    if (NODES[i].Game == null) NODES[i].Game = "DCS";
                    if (results[j].GAME.ToLower() == NODES[i].Game.ToLower())
                    {
                        found = true;
                        if (planesStillExist.ContainsKey(results[j].AIRCRAFT)) planesStillExist[results[j].AIRCRAFT] = true;
                    } 
                }
                if(!found)
                    toDelete.Add(NODES[i]);
                else
                {
                    foreach(KeyValuePair<string, bool> kvp in planesStillExist)
                    {
                        if (!kvp.Value)
                        {
                            NODES[i].DeleteAircraftFromActivity(kvp.Key);
                            aircraftToDelete++;
                        }
                    }
                }
            }
            for(int i=0; i<toDelete.Count; ++i)
            {
                NODES.Remove(toDelete[i]);
            }
            itemsToDelete = toDelete.Count;
            return new KeyValuePair<int, int>(itemsToDelete, aircraftToDelete);
        }
    }
}
