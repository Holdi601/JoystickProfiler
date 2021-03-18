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

        public Validation()
        {
            RelationErrors = new List<string>();
            BindErrors = new List<string>();
            ModifierErrors = new List<string>();
            startValidation();
        }

        void startValidation()
        {
            CheckRelationErrors();
            CheckButtonErrors();
            CheckModifierError();
        }

        void CheckRelationErrors()
        {
            List<string> AllKeys = new List<string>();
            List<Relation> AllRel = MainStructure.GetAllRelations();
            for(int i=0; i<AllRel.Count; ++i)
            {
                List<RelationItem> AllRelIt = AllRel[i].AllRelations();
                for(int j=0; j<AllRelIt.Count; j++)
                {
                    List<string> Aircraft = AllRelIt[j].GetActiveAircraftList();
                    for(int k=0; k<Aircraft.Count; k++)
                    {
                        string toAdd = AllRelIt[j].ID + "__" + Aircraft[k];
                        if (AllKeys.Contains(toAdd))
                        {
                            RelationErrors.Add("ERROR, Relation key duplicate: Relation: " + AllRel[i].NAME + " with id: " + AllRelIt[j].ID + " on Aircraft: " + Aircraft[k]);
                        }
                        else
                        {
                            AllKeys.Add(toAdd);
                        }
                    }
                }
            }
        }

        void CheckButtonErrors()
        {
            List<string> allKeys = new List<string>();
            List<Bind> binds = MainStructure.GetAllBinds();

            for(int i=0; i<binds.Count; ++i)
            {
                List<RelationItem> AllRelIt = binds[i].Rl.AllRelations();
                for(int j=0; j<AllRelIt.Count; ++j)
                {
                    List<string> Aircraft = AllRelIt[j].GetActiveAircraftList();
                    for(int k=0; k<Aircraft.Count; ++k)
                    {
                        string toAdd = Aircraft[k] + "__" + binds[i].Joystick;
                        if (binds[i].Rl.ISAXIS)
                        {
                            toAdd = toAdd + "__" + binds[i].JAxis;
                        }
                        else
                        {
                            toAdd = toAdd + "__" + binds[i].JButton;
                            for(int m=0; m<binds[i].AllReformers.Count; ++m)
                            {
                                toAdd = toAdd + "__" + binds[i].AllReformers[m];
                            }
                        }
                        if (allKeys.Contains(toAdd))
                        {
                            BindErrors.Add("ERROR, Joysting bind duplicate: Relation: " + binds[j].Rl.NAME + " with Joystick: " + binds[j] +"on Aircraft: "+Aircraft[k]+ " raw duplicate error - " + toAdd);
                        }
                        else
                        {
                            allKeys.Add(toAdd);
                        }
                    }
                }
            }
        }

        void CheckModifierError()
        {
            List<Bind> binds = MainStructure.GetAllBinds();
            List<Modifier> mods = MainStructure.GetAllModifiers();
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
