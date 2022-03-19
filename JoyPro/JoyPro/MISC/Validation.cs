using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class Validation
    {
        public List<string> RelationErrors;
        public List<string> BindErrors;
        public List<string> ModifierErrors;
        public List<string> DupActiveError;

        public Validation()
        {
            RelationErrors = new List<string>();
            BindErrors = new List<string>();
            ModifierErrors = new List<string>();
            DupActiveError = new List<string>();
            startValidation();
        }

        void startValidation()
        {
            CheckRelationErrors();
            CheckButtonErrors();
            CheckModifierError();
            CheckDuplicateActiveErrors();
        }

        void CheckDuplicateActiveErrors()
        {
            List<Relation> rels = InternalDataManagement.GetAllRelations();
            for(int i = 0; i < rels.Count; i++)
            {
                List<string> games = rels[i].GamesInRelation();
                for(int j=0; j< games.Count; j++)
                {
                    Dictionary<string, int> activity = rels[i].GetPlaneSetState(games[j]);
                    if (activity == null) continue;
                    foreach(KeyValuePair<string, int> pair in activity)
                    {
                        if (pair.Value > 1)
                        {
                            string message = "The game " + games[j] + " has multiple active items for the plane " + pair.Key + " on the Relation " + rels[i].NAME;
                            if(!DupActiveError.Contains(message))DupActiveError.Add(message);
                        }
                    }
                }
            }

        }

        void CheckRelationErrors()
        {
            Dictionary<string, List<string>> AllKeys = new Dictionary<string, List<string>>();
            List<Relation> AllRel = InternalDataManagement.GetAllRelations();
            for(int i=0; i<AllRel.Count; ++i)
            {
                List<RelationItem> AllRelIt = AllRel[i].AllRelations();
                for(int j=0; j<AllRelIt.Count; j++)
                {
                    List<string> Aircraft = AllRelIt[j].GetActiveAircraftList();
                    for(int k=0; k<Aircraft.Count; k++)
                    {
                        string toAdd = AllRelIt[j].ID + "__" + Aircraft[k];
                        if (!AllKeys.ContainsKey(toAdd))
                        {
                            AllKeys.Add(toAdd, new List<string>());
                        }
                        AllKeys[toAdd].Add(AllRel[i].NAME);
                    }
                }
            }
            foreach(KeyValuePair<string, List<string>> kvp in AllKeys)
            {
                if (kvp.Value.Count > 1)
                {
                    string res = "Relation Item key found in multiple Relation: "+ kvp.Key+ " The Relations it was found in: "+kvp.Value[0];
                    for(int i=1; i<kvp.Value.Count; ++i)
                    {
                        res = res + ", " + kvp.Value[i];
                    }
                    RelationErrors.Add(res);
                }
            }
        }

        void CheckButtonErrors()
        {
            Dictionary<string, List<string>> AllKeys = new Dictionary<string, List<string>>();
            List<Bind> binds = InternalDataManagement.GetAllBinds();

            for(int i=0; i<binds.Count; ++i)
            {
                List<RelationItem> AllRelIt = binds[i].Rl.AllRelations();
                for(int j=0; j<AllRelIt.Count; ++j)
                {
                    List<string> Aircraft = AllRelIt[j].GetActiveAircraftList();
                    string game = AllRelIt[j].Game;
                    for (int k=0; k<Aircraft.Count; ++k)
                    {
                        string toAdd = game+":"+Aircraft[k] + "§" + binds[i].Joystick;
                        if (binds[i].Rl.ISAXIS)
                        {
                            toAdd = toAdd + "§" + binds[i].JAxis;
                            for (int m = 0; m < binds[i].AllReformers.Count; ++m)
                            {
                                toAdd = toAdd + "§" + binds[i].AllReformers[m];
                            }
                        }
                        else
                        {
                            toAdd = toAdd + "§" + binds[i].JButton;
                            for(int m=0; m<binds[i].AllReformers.Count; ++m)
                            {
                                toAdd = toAdd + "§" + binds[i].AllReformers[m];
                            }
                        }
                        if (!AllKeys.ContainsKey(toAdd))
                        {
                            AllKeys.Add(toAdd, new List<string>());
                        }
                        if(!AllKeys[toAdd].Contains(binds[i].Rl.NAME))
                            AllKeys[toAdd].Add(binds[i].Rl.NAME);
                    }
                }
            }
            foreach(KeyValuePair<string, List<string>>kvp in AllKeys)
            {
                if (kvp.Value.Count > 1)
                {
                    string res = "Button Item key found in multiple Relation: " + kvp.Key + " The Relations it was found in: " + kvp.Value[0];
                    for (int i = 1; i < kvp.Value.Count; ++i)
                    {
                        res = res + ", " + kvp.Value[i];
                    }
                    BindErrors.Add(res);
                }
            }
        }

        void CheckModifierError()
        {
            List<Bind> binds = InternalDataManagement.GetAllBinds();
            List<Modifier> mods = InternalDataManagement.GetAllModifiers();
            Dictionary<Modifier, List<string>> ModifierUsedOnCraft = new Dictionary<Modifier, List<string>>();
            for(int i=0; i<binds.Count; ++i)
            {
                for(int j=0; j<mods.Count; j++)
                {
                    if (binds[i].AllReformers.Contains(mods[j].toReformerString()))
                    {
                        //Now add aircrafts
                        List<RelationItem> AllRelIt = binds[i].Rl.AllRelations();
                        for(int k=0; k<AllRelIt.Count; ++k)
                        {
                            List<string> Aircraft = AllRelIt[k].GetActiveAircraftList();
                            for(int m=0; m<Aircraft.Count; ++m)
                            {
                                if (!ModifierUsedOnCraft.ContainsKey(mods[j]))
                                {
                                    ModifierUsedOnCraft.Add(mods[j], new List<string>());
                                }
                                if (!ModifierUsedOnCraft[mods[j]].Contains(Aircraft[m]))
                                    ModifierUsedOnCraft[mods[j]].Add(Aircraft[m]);
                            }
                        }
                    }
                }
            }
            for(int i=0; i<binds.Count; ++i)
            {
                if (binds[i].Rl.ISAXIS)
                {
                    continue;
                }
                for(int j=0; j<mods.Count; ++j)
                {
                    if (mods[j].device.ToUpper() == binds[i].Joystick.ToUpper() && mods[j].key.ToUpper() == binds[i].JButton.ToUpper())
                    {
                        List<RelationItem> ari = binds[i].Rl.AllRelations();
                        for(int k=0; k<ari.Count; ++k)
                        {
                            List<string> crafts = ari[k].GetActiveAircraftList();
                            for(int h=0; h<crafts.Count; ++h)
                            {
                                if (ModifierUsedOnCraft.ContainsKey(mods[j]))
                                {
                                    if (ModifierUsedOnCraft[mods[j]].Contains(crafts[h]))
                                    {
                                        ModifierErrors.Add("Modifier error, Modifier used on craft, which also uses modifier as a regular button. Relation: " + binds[i].Rl.NAME + " with Joystick: " + binds[i].Joystick + " with Button: " + binds[i].JButton + " and Modifier: " + mods[j].name);
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
    }
}
