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
        public Bind bind=null;
        
        public Relation()
        {
            NODES = new List<RelationItem>();
        }

        public Dictionary<string, int> GetPlaneSetState()
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            for(int i=1; i<MainStructure.Planes.Length; ++i)
            {
                results.Add(MainStructure.Planes[i], GetPlaneRelationState(MainStructure.Planes[i]));
            }
            return results;
        }

        public void CheckNamesAgainstDB()
        {
            foreach (RelationItem r in NODES)
            {
                r.CheckAgainstDB();
            }
        }

        int GetPlaneRelationState(string plane)
        {
            int counter = 0;
            for(int i=0; i<NODES.Count; ++i)
            {
                PlaneState ps = NODES[i].GetStateAircraft(plane);
                if (ps == PlaneState.ACTIVE) ++counter;
            }
            return counter;
        }
        public bool AddNode(string id, string plane="")
        {
            if(NodesContainId(id)&&plane.Length<1) return false;
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
                NODES.Add(new RelationItem(id));
            else
            {
                bool found = false;
                int oof = -1;
                for(int i=0; i<NODES.Count; ++i)
                {
                    PlaneState ps = NODES[i].GetStateAircraft(plane);
                    if(NODES[i].ID==id&&(ps== PlaneState.ACTIVE||ps== PlaneState.DISABLED))
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
                    NODES.Add(new RelationItem(id, plane));
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

        public RelationItem GetRelationItemForPlane(string plane)
        {
            for(int i=0; i<NODES.Count; ++i)
            {
                PlaneState ps = NODES[i].GetStateAircraft(plane);
                if (ps == PlaneState.ACTIVE) return NODES[i];
            }
            return null;
        }

        public RelationItem GetRelationItem(string id)
        {
            for (int i = 0; i < NODES.Count; i++)
            {
                if (id == NODES[i].ID) return NODES[i];
            }
            return null;
        }
        bool NodesContainId(string id)
        {
            for(int i=0; i<NODES.Count; i++)
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
