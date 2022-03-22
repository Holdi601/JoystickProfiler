using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoyPro

{
    public class RelationJumper
    {
        bool KeepRunning = true;

        public void StartRelationJumper()
        {
            while (KeepRunning)
            {
                Thread.Sleep(MainStructure.msave.OvlPollTime);
                if (MainStructure.MainWindowActive && !MainStructure.MainWindowTextActive && !MainStructure.JoystickReadActive&& !MainStructure.VisualMode)
                {
                    foreach(KeyValuePair<string, List<string>> pair in OverlayBackGroundWorker.currentPressed)
                    {
                        int pressed=-1;
                        bool found = false;
                        string device = "";
                        string button = "";
                        List<int> btns = new List<int>();
                        if (!InternalDataManagement.JoystickButtonsPressed.ContainsKey(pair.Key))
                        {
                            InternalDataManagement.JoystickButtonsPressed.Add(pair.Key, new Dictionary<string, int>());
                        }
                        device = pair.Key;
                        for(int j=0; j<pair.Value.Count; j++)
                        {
                            button=pair.Value[j];
                            if (!InternalDataManagement.JoystickButtonsPressed[pair.Key].ContainsKey(pair.Value[j]))
                                InternalDataManagement.JoystickButtonsPressed[pair.Key].Add(pair.Value[j], 1);
                            else
                                InternalDataManagement.JoystickButtonsPressed[pair.Key][pair.Value[j]]++;
                            pressed = InternalDataManagement.JoystickButtonsPressed[pair.Key][pair.Value[j]];
                            for(int i=0; i<MainStructure.mainW.CURRENTDISPLAYEDRELATION.Count; i++)
                            {
                                if (MainStructure.mainW.CURRENTDISPLAYEDRELATION[i].bind != null &&
                                    MainStructure.mainW.CURRENTDISPLAYEDRELATION[i].bind.Joystick == device&&
                                    MainStructure.mainW.CURRENTDISPLAYEDRELATION[i].bind.JButton == button)
                                {
                                    btns.Add(i);
                                }
                            }
                            if (btns.Count > 0) break;                        
                        }
                        if (btns.Count > 0)
                        {
                            int toSelect = pressed % btns.Count;
                            MainStructure.mainW.Dispatcher.Invoke(new Action(() =>
                            {
                                MainStructure.mainW.JumpToRelation(btns[toSelect]);
                            }));
                            break;
                        }
                    }
                }

            }
        }

    }
}
