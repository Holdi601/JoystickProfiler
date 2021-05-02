using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace JoyPro
{
    
    public class JoystickResults
    {
        public string AxisButton { get; set; }
        public string Device { get; set; }

        public List<string> All = new List<string>();
    }
    public class JoyAxisState
    {
        public int x;
        public int y;
        public int z;
        public int xr;
        public int yr;
        public int zr;
        public int s1r;
        public int s2r;

        public bool[] btns;
        public int[] povs;
        public JoyAxisState()
        {
            x = -1;
            y = -1;
            z = -1;
            xr = -1;
            yr = -1;
            zr = -1;
            s1r = -1;
            s2r = -1;
            btns = new bool[128];
            povs = new int[4];
            for (int i = 0; i < povs.Length; ++i) povs[i] = -1;
        }

    }
    public class JoystickReader
    {
        List<DeviceInstance> directInputList;
        SlimDX.XInput.Controller contrl;
        DirectInput directInput;
        List<SlimDX.DirectInput.Joystick> gamepads;
        Dictionary<Joystick, JoyAxisState> state;
        Dictionary<Joystick, JoystickState> lastState;
        KeyboardState lastKbState = null;
        bool detectionEventActiveButton;
        bool detectionEventActiveAxis;
        bool quit;
        bool keybValues;
        int timeToSet;
        int axisThreshold;
        int warmupTime;
        int timeLeftToSet;
        public JoystickResults result;
        int pollWaitTime;
        Keyboard kb;

        public static List<string> GetConnectedJoysticks()
        {
            List<string> result = new List<string>();
            DirectInput di = new DirectInput();
            List<DeviceInstance> dil = new List<DeviceInstance>();
            dil.Clear();
            dil.AddRange(di.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly));
            List<SlimDX.DirectInput.Joystick> pds = new List<Joystick>();
            foreach (var device in dil)
            {
                string deviceToAdd = ToDeviceString(new Joystick(di, device.InstanceGuid));
                if(!MainStructure.ListContainsCaseInsensitive(result, deviceToAdd))
                    result.Add(deviceToAdd);
            }
            return result;
        }
        public JoystickReader(bool axis, bool includeKeyboard=false)
        {
            timeToSet = 5000;
            axisThreshold = 10000;
            warmupTime = 300;
            pollWaitTime = 10;
            if (MainStructure.msave != null&&MainStructure.msave.timeToSet>0&&MainStructure.msave.axisThreshold>0&&MainStructure.msave.warmupTime>0&&MainStructure.msave.pollWaitTime>0)
            {
                timeToSet = MainStructure.msave.timeToSet;
                axisThreshold = MainStructure.msave.axisThreshold;
                warmupTime = MainStructure.msave.warmupTime;
                pollWaitTime = MainStructure.msave.pollWaitTime;
            }
            if (includeKeyboard) axis = false;
            
            timeLeftToSet = timeToSet;
            
            directInputList = new List<DeviceInstance>();
            directInput = new DirectInput();
            contrl = new SlimDX.XInput.Controller(SlimDX.XInput.UserIndex.Any);
            gamepads = new List<Joystick>();
            state = new Dictionary<Joystick, JoyAxisState>();
            lastState = new Dictionary<Joystick, JoystickState>();
            if (axis)
            {
                detectionEventActiveButton = false;
                detectionEventActiveAxis = true;
            }
            else
            {
                detectionEventActiveButton = true;
                detectionEventActiveAxis = false;
            }
            keybValues = includeKeyboard;
            quit = false;
            result = null;
            initJoystick();
            startPolling();
        }

        JoyAxisState StateToJoyAxisState(JoystickState js)
        {
            if (js == null) return null;
            JoyAxisState result = new JoyAxisState();
            result.x = js.X;
            result.y = js.Y;
            result.z = js.Z;
            result.xr = js.RotationX;
            result.yr = js.RotationY;
            result.zr = js.RotationZ;
            int[] sldrs = js.GetSliders();
            if (sldrs.Length > 0) result.s1r = sldrs[0];
            if (sldrs.Length > 1) result.s2r = sldrs[1];
            return result;
        }

        void FillInJoyAxisStateDefaults(JoyAxisState jas, JoystickState js)
        {
            JoyAxisState filler = StateToJoyAxisState(js);
            if (jas.x < 11 || warmupTime>0) jas.x = filler.x;
            if (jas.y < 11 || warmupTime > 0) jas.y = filler.y;
            if (jas.z < 11 || warmupTime > 0) jas.z = filler.z;
            if (jas.xr < 11 || warmupTime > 0) jas.xr = filler.xr;
            if (jas.yr < 11 || warmupTime > 0) jas.yr = filler.yr;
            if (jas.zr < 11 || warmupTime > 0) jas.zr = filler.zr;
            if (jas.s1r < 11 || warmupTime > 0) jas.s1r = filler.s1r;
            if (jas.s2r < 11 || warmupTime > 0) jas.s2r = filler.s2r;
            for(int i=0; i<jas.btns.Length; ++i)
            {
                if (!jas.btns[i]) jas.btns[i] = filler.btns[i];
            }
        }
        void initJoystick()
        {
            directInputList.Clear();
            directInputList.AddRange(directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly));
            gamepads.Clear();
            DirectInput dikb = new DirectInput();
            kb = new Keyboard(directInput);
            kb.Acquire();
            foreach (var device in directInputList)
            {
                gamepads.Add(new SlimDX.DirectInput.Joystick(directInput, device.InstanceGuid));
            }
        }
        void startPolling()
        {
            while (!quit)
            {
                Thread.Sleep(pollWaitTime);
                KeyboardState ks = kb.GetCurrentState();
                if (timeLeftToSet > -1&&(detectionEventActiveAxis||detectionEventActiveButton))
                {
                    timeLeftToSet = timeLeftToSet - pollWaitTime;
                    warmupTime = warmupTime - pollWaitTime;
                    tick();
                }if((timeLeftToSet<0 && (detectionEventActiveAxis || detectionEventActiveButton))||(ks.IsPressed(Key.Escape))||(ks.IsPressed(Key.Delete)) ||(ks.IsPressed(Key.Backspace)))
                {
                    quit = true;
                    detectionEventActiveButton = false;
                    detectionEventActiveAxis = false;
                }
               
            }
        }
        void CheckIfAxisPassedThreshhold(Joystick pad, JoyAxisState current)
        {
            JoyAxisState last = state[pad];
            int xDiff = last.x - current.x;
            int yDiff = last.y - current.y;
            int zDiff = last.z - current.z;
            int xrDiff = last.xr - current.xr;
            int yrDiff = last.yr - current.yr;
            int zrDiff = last.zr - current.zr;
            int s1rDiff = last.s1r - current.s1r;
            int s2rDiff = last.s2r - current.s2r;
            if (xDiff < 0) xDiff *= -1;
            if (yDiff < 0) yDiff *= -1;
            if (zDiff < 0) yDiff *= -1;
            if (xrDiff < 0) xrDiff *= -1;
            if (yrDiff < 0) yrDiff *= -1;
            if (zrDiff < 0) zrDiff *= -1;
            if (s1rDiff < 0) s1rDiff *= -1;
            if (s2rDiff < 0) s2rDiff *= -1;
            JoystickResults args = new JoystickResults();
            args.Device = ToDeviceString(pad);
            string pre= "JOY_";
            args.AxisButton = "JOY_";
            bool triggered = false;
            if (xDiff > axisThreshold&&last.x>10&&current.x>10)
            {
                Console.WriteLine(last.x.ToString() + " " + current.x.ToString());
                args.AxisButton += "X";
                args.All.Add(pre+"X");
                triggered = true;
            }
            else if (yDiff > axisThreshold && last.y > 10 && current.y > 10)
            {
                Console.WriteLine(last.y.ToString() + " " + current.y.ToString());
                args.AxisButton += "Y";
                args.All.Add(pre + "Y");
                triggered = true;
            }
            else if (zDiff > axisThreshold && last.z > 10 && current.z > 10)
            {
                Console.WriteLine(last.z.ToString() + " " + current.z.ToString());
                args.AxisButton += "Z";
                args.All.Add(pre + "Z");
                triggered = true;
            }
            else if (xrDiff > axisThreshold && last.xr > 10 && current.xr > 10)
            {
                Console.WriteLine(last.xr.ToString() + " " + current.xr.ToString());
                args.AxisButton += "RX";
                args.All.Add(pre + "RX");
                triggered = true;
            }
            else if (yrDiff > axisThreshold && last.yr > 10 && current.yr > 10)
            {
                Console.WriteLine(last.yr.ToString() + " " + current.yr.ToString());
                args.AxisButton += "RY";
                args.All.Add(pre + "RY");
                triggered = true;
            }
            else if (zrDiff > axisThreshold && last.zr > 10 && current.zr > 10)
            {
                Console.WriteLine(last.zr.ToString() + " " + current.zr.ToString());
                args.AxisButton += "RZ";
                args.All.Add(pre + "RZ");
                triggered = true;
            }
            else if (s1rDiff > axisThreshold && last.s1r > 10 && current.s1r > 10)
            {
                Console.WriteLine(last.s1r.ToString() + " " + current.s1r.ToString());
                args.AxisButton += "SLIDER1";
                args.All.Add(pre + "SLIDER1");
                triggered = true;
            }
            else if (s2rDiff > axisThreshold && last.s2r > 10 && current.s2r > 10)
            {
                Console.WriteLine(last.s2r.ToString() + " " + current.s2r.ToString());
                args.AxisButton += "SLIDER2";
                args.All.Add(pre + "SLIDER2");
                triggered = true;
            }
            if(triggered&& warmupTime < 0)
                ResultFound(args);

        }
        void CheckIfKeyboardGotPressed(KeyboardState ks)
        {
            if (warmupTime < 0)
            {
                var allPressed = ks.PressedKeys;
                JoystickResults r = new JoystickResults();
                bool found = false;
                foreach (var keyPressed  in allPressed)
                {
                    r.Device = "Keyboard";
                    r.AxisButton = keyPressed.ToString();
                    r.All.Add(keyPressed.ToString());
                    found = true;
                    
                }
                if(found)
                    ResultFound(r);
            }
            lastKbState = ks;
        }
        void CheckIfButtonGotPressed(Joystick pad, JoystickState js)
        {
            JoyAxisState last = state[pad];
            bool[] curBtns = js.GetButtons();
            bool[] lastBtns;
            bool found = false;
            JoystickResults args = new JoystickResults();
            if (warmupTime<0)
            {
                lastBtns = lastState[pad].GetButtons();
                
                for (int i = 0; i < curBtns.Length; ++i)
                {
                    if (curBtns[i] != lastBtns[i])
                    {
                        string pre = "JOY_BTN";
                        args.Device = ToDeviceString(pad);
                        args.AxisButton = "JOY_BTN" + (i + 1).ToString();
                        args.All.Add(args.AxisButton);
                        found = true;
                    }
                }
                
            }
            
            int[] povs = js.GetPointOfViewControllers();
            for(int i=0; i<povs.Length; ++i)
            {
                if (povs[i] > -1)
                {
                    string dir = "JOY_BTN_POV"+(i+1).ToString()+"_";
                    switch (povs[i])
                    {
                        case 0:
                            dir += "U";
                            break;
                        case 4500:
                            dir += "UR";
                            break;
                        case 9000:
                            dir += "R";
                            break;
                        case 13500:
                            dir += "DR";
                            break;
                        case 18000:
                            dir += "D";
                            break;
                        case 22500:
                            dir += "DL";
                            break;
                        case 27000:
                            dir += "L";
                            break;
                        case 31500:
                            dir += "UL";
                            break;
                    }
                    args.Device = ToDeviceString(pad);
                    args.AxisButton = dir;
                    args.All.Add(dir);
                    found = true;
                }
            }
            if (found)
                ResultFound(args);

        }
        static string ToDeviceString(Joystick pad)
        {
            string rawId = pad.Information.InstanceGuid.ToString();
            Console.WriteLine(rawId);
            return pad.Information.InstanceName + " {" + pad.Information.InstanceGuid.ToString().ToUpper() + "}";
        }
        void tick()
        {
            foreach (var gamepad in gamepads)
            {
                if (gamepad.Acquire().IsFailure)
                    continue;
                if (gamepad.Poll().IsFailure)
                    continue;
                if (SlimDX.Result.Last.IsFailure)
                    continue;

                JoystickState currentState = gamepad.GetCurrentState();
                if (state.ContainsKey(gamepad))
                {
                    FillInJoyAxisStateDefaults(state[gamepad], currentState);
                }
                else state.Add(gamepad, StateToJoyAxisState(currentState));
                if (detectionEventActiveAxis)
                {
                    CheckIfAxisPassedThreshhold(gamepad, StateToJoyAxisState(currentState));
                }
                if (detectionEventActiveButton)
                {
                    CheckIfButtonGotPressed(gamepad, currentState);
                    if (keybValues)
                    {
                        CheckIfKeyboardGotPressed(kb.GetCurrentState());
                    }                    
                }
                if (lastState.ContainsKey(gamepad)) lastState[gamepad] = currentState;
                else lastState.Add(gamepad, currentState);
            }
        }

        void ResultFound(JoystickResults e)
        {
            Random r = new Random();
            if (e.All.Count > 0)
                e.AxisButton = e.All[r.Next(0, e.All.Count )];
            quit = true;
            result = e;
        }
    }
}
