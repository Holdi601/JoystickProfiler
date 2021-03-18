﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class Bind
    {
        public Relation Rl;
        public string Joystick;
        public string JAxis;
        public string JButton;
        public string Reformer_depr;
        public bool? Inverted;
        public bool? Slider;
        public double Deadzone;
        public List<double> Curvature;
        public double SaturationX;
        public double SaturationY;
        public List<string> AllReformers;

        public string[] PovHeads = new string[]
        {
            "POV1_D"
            ,"POV1_U"
            ,"POV1_L"
            ,"POV1_R"
            ,"POV1_DL"
            ,"POV1_UL"
            ,"POV1_DR"
            ,"POV1_UR"
            ,"POV2_D"
            ,"POV2_U"
            ,"POV2_L"
            ,"POV2_R"
            ,"POV2_DL"
            ,"POV2_UL"
            ,"POV2_DR"
            ,"POV2_UR"
            ,"POV3_D"
            ,"POV3_U"
            ,"POV3_L"
            ,"POV3_R"
            ,"POV3_DL"
            ,"POV3_UL"
            ,"POV3_DR"
            ,"POV3_UR"
            ,"POV4_D"
            ,"POV4_U"
            ,"POV4_L"
            ,"POV4_R"
            ,"POV4_DL"
            ,"POV4_UL"
            ,"POV4_DR"
            ,"POV4_UR"
        };
        public Bind(Relation r)
        {
            Joystick = "";
            JAxis = "";
            JButton = "";
            Inverted = false;
            Slider = false;
            Curvature = new List<double>();
            Curvature.Add(0.0);
            SaturationX = 1.0;
            SaturationY = 1.0;
            Rl = r;
            Deadzone = 0;
            Reformer_depr = "";
            AllReformers = new List<string>();
        }

        public DCSAxisBind toDCSAxisBind()
        {
            DCSAxisBind result = new DCSAxisBind();
            if (JAxis.Length <1 || Joystick.Length < 1) return null;
            result.key = JAxis.ToString();
            DCSAxisFilter daf = new DCSAxisFilter();
            result.filter = daf;
            daf.curvature = Curvature;
            daf.deadzone = Deadzone;
            daf.inverted = Inverted ?? false;
            daf.slider = Slider ?? false;
            daf.saturationX = SaturationX;
            daf.saturationY = SaturationY;
            return result;
        }

        
        public static Bind GetBindFromAxisElement(DCSAxisBind dab,string id, string joystick, string plane, bool inv=false, bool slid=false, bool curv=false, bool dz=false, bool sx=false, bool sy=false)
        {
            Relation r = new Relation();
            Bind b = new Bind(r);
            string shorten = MainStructure.ShortenDeviceName(joystick);
            string relationName = shorten + dab.key;
            r.ISAXIS = true;
            b.JAxis = dab.key;
            b.Joystick = joystick;
            if (dab.filter != null)
            {
                b.Inverted = dab.filter.inverted;
                b.Curvature = dab.filter.curvature;
                if (b.Curvature == null) b.Curvature = new List<double>();
                if (b.Curvature.Count < 1) b.Curvature.Add(0.0);
                b.Deadzone = dab.filter.deadzone;
                b.Slider = dab.filter.slider;
                b.SaturationX = dab.filter.saturationX;
                b.SaturationY = dab.filter.saturationY;
            }
            else
            {
                b.Inverted = false;
                b.Slider = false;
                b.Curvature = new List<double>();
                b.Curvature.Add(0.0);
                b.SaturationX = 1.0;
                b.SaturationY = 1.0;
                b.Deadzone = 0;
            }
            if (inv) relationName = relationName + "i" + b.Inverted.ToString();
            if (slid) relationName = relationName + "s" + b.Slider.ToString();
            if (curv && b.Curvature.Count > 0)
            {
                if (b.Curvature[0].ToString(new CultureInfo("en-US")).Length > 3)
                    relationName = relationName + "c" + b.Curvature[0].ToString(new CultureInfo("en-US")).Substring(0, 4);
                else
                    relationName = relationName + "c" + b.Curvature[0].ToString(new CultureInfo("en-US"));
            }
            if (dz)
            {
                if (b.Deadzone.ToString(new CultureInfo("en-US")).Length > 3)
                    relationName = relationName + "d" + b.Deadzone.ToString(new CultureInfo("en-US")).Substring(0, 4);
                else
                    relationName = relationName + "d" + b.Deadzone.ToString(new CultureInfo("en-US"));
            }
            if (sx)
            {
                if(b.SaturationX.ToString(new CultureInfo("en-US")).Length>3)
                    relationName = relationName + "x" + b.SaturationX.ToString(new CultureInfo("en-US")).Substring(0, 4);
                else
                    relationName = relationName + "x" + b.SaturationX.ToString(new CultureInfo("en-US"));
            }
            if (sy)
            {
                if(b.SaturationY.ToString(new CultureInfo("en-US")).Length>3)
                    relationName = relationName + "y" + b.SaturationY.ToString(new CultureInfo("en-US")).Substring(0, 4);
                else
                    relationName = relationName + "y" + b.SaturationY.ToString(new CultureInfo("en-US"));
            }
            r.NAME = relationName;
            r.AddNode(id, plane);
            return b;
        }

        public static Bind GetBindFromButtonElement(DCSButtonBind dab, string id, string joystick, string plane)
        {
            Relation r = new Relation();
            Bind b = new Bind(r);
            r.ISAXIS = false;
            string shorten = MainStructure.ShortenDeviceName(joystick);
            string relationName = shorten + dab.key;
            if(dab.modifiers!=null)
                foreach(Modifier m in dab.modifiers)
                {
                    //same code as mainwindow
                    string reform = m.name + "§" + m.device + "§" + m.key;

                    string device;
                    if (m.device == "Keyboard")
                        device = "Keyboard";
                    else
                        device = "m" + m.device.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
                    string nameToShow = device + m.key;
                    string moddedDevice = Bind.JoystickGuidToModifierGuid(m.device);
                    string toAdd = nameToShow + "§" + moddedDevice + "§" + m.key;
                    if (!b.AllReformers.Contains(toAdd))
                    {
                        
                        ModExists alreadyExists = MainStructure.DoesReformerExistInMods(toAdd);
                        if (alreadyExists == ModExists.NOT_EXISTENT)
                        {
                            MainStructure.AddReformerToMods(toAdd);
                            b.AllReformers.Add(toAdd);
                        }
                        else if (alreadyExists == ModExists.BINDNAME_EXISTS||alreadyExists== ModExists.ALL_EXISTS)
                        {
                            toAdd = MainStructure.GetReformerStringFromMod(m.name);
                            if (!b.AllReformers.Contains(toAdd)) b.AllReformers.Add(toAdd);
                        }else if (alreadyExists == ModExists.KEYBIND_EXISTS)
                        {
                            Modifier mnew = MainStructure.GetModifierWithKeyCombo(m.device, m.key);
                            if (mnew != null)
                            {
                                toAdd = mnew.toReformerString();
                                if (!b.AllReformers.Contains(toAdd)) b.AllReformers.Add(toAdd);
                            }
                        }
                        else if (alreadyExists == ModExists.ERROR)
                        {
                           
                        }
                    }
                    relationName = nameToShow + relationName;

                }
            r.NAME = relationName;
            b.JButton = dab.key;
            b.Joystick = joystick;
            r.AddNode(id, plane);
            return b;
        }

        public DCSButtonBind toDCSButtonBind()
        {
            DCSButtonBind dbb = new DCSButtonBind();
            dbb.key = JButton;
            if (JButton.Length < 1||Joystick.Length<1) return null;
            if (dbb.reformers == null) dbb.reformers = new List<string>();
            for (int i=0; i<AllReformers.Count; ++i)
            {
                if (AllReformers[i].Length > 0)
                {
                    string[] parts = AllReformers[i].Split('§');
                    if (parts.Length == 3)
                    {
                        dbb.reformers.Add(parts[0]);
                        Modifier m = new Modifier();
                        m.name = parts[0];
                        m.device = parts[1];
                        m.sw = false;
                        m.key = parts[2];
                        dbb.modifiers.Add(m);
                    }
                }
            }
            return dbb;
        }
        public void replaceDeviceInReformers(string oldDevice, string newDevice)
        {
            string shortenOld = "m"+oldDevice.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
            string shortenNew = "m"+newDevice.Split('{')[1].Split('}')[0].GetHashCode().ToString().Substring(0, 5);
            oldDevice = JoystickGuidToModifierGuid(oldDevice);
            newDevice = JoystickGuidToModifierGuid(newDevice);
            for (int i=0; i<AllReformers.Count; ++i)
            {
                if (AllReformers[i].Contains("§"+oldDevice))
                {
                    AllReformers[i] = AllReformers[i].Replace("§" + oldDevice, "§" + newDevice);
                    AllReformers[i] = AllReformers[i].Replace(shortenOld, shortenNew);
                }
            }
        }

        public static string JoystickGuidToModifierGuid(string id)
        {
            if (id == "Keyboard") return "Keyboard";
            string[] parts = id.Split('{');
            string nonIdPart = parts[0];
            if (parts.Length > 1)
            {
                string idToMod = parts[1].Split('}')[0];
                string[] idParts = idToMod.Split('-');
                string finishedID = "";
                if (idParts.Length > 0)
                    finishedID = idParts[0].ToUpper();
                for(int i=1; i<idParts.Length; ++i)
                {
                    if (i == 2)
                    {
                        finishedID = finishedID + "-" + idParts[i].ToLower();
                    }
                    else
                    {
                        finishedID = finishedID + "-" + idParts[i].ToUpper();
                    }
                }
                string result = nonIdPart + "{" + finishedID + "}";
                return result;
            }
            return "";
        }

    }
}
