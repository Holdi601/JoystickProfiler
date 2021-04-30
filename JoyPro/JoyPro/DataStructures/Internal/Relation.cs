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

        public Relation()
        {
            NODES = new List<RelationItem>();
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

        public Dictionary<string, int> GetPlaneSetState()
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            for (int i = 0; i < MainStructure.Planes.Length; ++i)
            {
                results.Add(MainStructure.Planes[i], GetPlaneRelationStateDCS(MainStructure.Planes[i]));
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
            return r;
        }

        public void CheckNamesAgainstDB()
        {
            foreach (RelationItem r in NODES)
            {
                if (MainStructure.mainW.DCSSELECT.IsChecked == false &&
                    (r.Game == null ||
                    r.Game == "" ||
                    r.Game == "DCS"))
                {
                    MainStructure.mainW.DCSSELECT.IsChecked = true;
                    MainStructure.mainW.GameSelectionChanged(null, null);
                    if (r.Game == null || r.Game == "")
                        r.Game = "DCS";
                }
                else if (MainStructure.mainW.IL2SELECT.IsChecked == false &&
                   (r.Game != null &&
                   r.Game == "IL2"))
                {
                    MainStructure.mainW.IL2SELECT.IsChecked = true;
                    MainStructure.mainW.GameSelectionChanged(null, null);
                }
                r.CheckAgainstDB();
            }
        }

        public void ActivateRestForID(string id)
        {
            List<string> planesAll = MainStructure.Planes.ToList();
            List<string> planesActiveInRel = new List<string>();
            for (int i = 0; i < NODES.Count; ++i)
            {
                List<string> planes = NODES[i].GetActiveAircraftList();
                for (int j = 0; j < planes.Count; ++j)
                {
                    if (!planesActiveInRel.Contains(planes[j]))
                    {
                        planesActiveInRel.Add(planes[j]);
                    }
                }
            }
            for (int i = 0; i < planesActiveInRel.Count; ++i)
            {
                if (planesAll.Contains(planesActiveInRel[i]))
                {
                    planesAll.Remove(planesActiveInRel[i]);
                }
            }
            RelationItem node = GetRelationItem(id);
            for(int i=0; i<planesAll.Count; ++i)
            {
                node.SetAircraftActivityDCS(planesAll[i], true);
            }
        }

        public void DeactivateAllID(string id)
        {
            RelationItem node = GetRelationItem(id);
            List<string> planes = node.GetActiveAircraftList();
            for (int j = 0; j < planes.Count; ++j)
            {
                node.SetAircraftActivityDCS(planes[j], false);
            }
        }

        int GetPlaneRelationStateDCS(string plane)
        {
            int counter = 0;
            for (int i = 0; i < NODES.Count; ++i)
            {
                PlaneState ps = NODES[i].GetStateAircraftDCS(plane);
                if (ps == PlaneState.ACTIVE) ++counter;
            }
            return counter;
        }
        public bool AddNodeDCS(string id, string plane = "")
        {
            if (NodesContainId(id) && plane.Length < 1) return false;
            if (NODES.Count < 1)
            {
                string axisID = id.Substring(0, 1);
                if (axisID == "a") ISAXIS = true;
                else ISAXIS = false;
            }
            else
            {
                string axisID = id.Substring(0, 1);
                bool candidateAxis = axisID == "a";
                if (ISAXIS != candidateAxis) return false;
            }
            if (plane.Length < 1)
                NODES.Add(new RelationItem(id, "DCS"));
            else
            {
                bool found = false;
                int oof = -1;
                for (int i = 0; i < NODES.Count; ++i)
                {
                    PlaneState ps = NODES[i].GetStateAircraftDCS(plane);
                    if (NODES[i].ID == id && (ps == PlaneState.ACTIVE || ps == PlaneState.DISABLED))
                    {
                        found = true;
                        oof = i;
                        break;
                    }
                }
                if (found)
                {
                    NODES[oof].SetAircraftActivityDCS(plane, true);
                }
                else
                {
                    NODES.Add(new RelationItem(id, plane, "DCS"));
                }
            }
            Console.WriteLine("Relation Item Added");
            return true;
        }

        public bool RemoveNode(string id)
        {
            if (!NodesContainId(id)) return false;
            RelationItem ri = GetRelationItem(id);
            NODES.Remove(ri);
            return true;
        }

        public RelationItem GetRelationItemForPlaneDCS(string plane)
        {
            for (int i = 0; i < NODES.Count; ++i)
            {
                PlaneState ps = NODES[i].GetStateAircraftDCS(plane);
                if (ps == PlaneState.ACTIVE) return NODES[i];
            }
            return null;
        }

        public RelationItem GetRelationItem(string id)
        {
            for (int i = 0; i < NODES.Count; i++)
            {
                if (id.ToUpper() == NODES[i].ID.ToUpper()) return NODES[i];
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
    }
}
