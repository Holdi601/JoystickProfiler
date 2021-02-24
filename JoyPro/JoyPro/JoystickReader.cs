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
        bool detectionEventActiveButton;
        bool detectionEventActiveAxis;
        bool quit;
        const int timeToSet = 5000;
        const int axisThreshold = 2000;
        int warmupTime;
        int timeLeftToSet = timeToSet;
        public JoystickResults result;
        int pollWaitTime;

        public JoystickReader(bool axis)
        {
            pollWaitTime = 10;
            warmupTime = 100;
            directInputList = new List<DeviceInstance>();
            directInput = new DirectInput();
            contrl = new SlimDX.XInput.Controller(SlimDX.XInput.UserIndex.Any);
            gamepads = new List<Joystick>();
            state = new Dictionary<Joystick, JoyAxisState>();
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
                if (timeLeftToSet > -1&&(detectionEventActiveAxis||detectionEventActiveButton))
                {
                    timeLeftToSet = timeLeftToSet - pollWaitTime;
                    warmupTime = warmupTime - pollWaitTime;
                    tick();
                }else if(timeLeftToSet<0 && (detectionEventActiveAxis || detectionEventActiveButton))
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
            args.AxisButton = "JOY_";
            if (xDiff > axisThreshold&&last.x>10&&current.x>10)
            {
                Console.WriteLine(last.x.ToString() + " " + current.x.ToString());
                args.AxisButton += "X";
                if(warmupTime<0)ResultFound(args);
            }
            else if (yDiff > axisThreshold && last.y > 10 && current.y > 10)
            {
                Console.WriteLine(last.y.ToString() + " " + current.y.ToString());
                args.AxisButton += "Y";
                if (warmupTime < 0) ResultFound(args);
            }
            else if (zDiff > axisThreshold && last.z > 10 && current.z > 10)
            {
                Console.WriteLine(last.z.ToString() + " " + current.z.ToString());
                args.AxisButton += "Z";
                if (warmupTime < 0) ResultFound(args);
            }
            else if (xrDiff > axisThreshold && last.xr > 10 && current.xr > 10)
            {
                Console.WriteLine(last.xr.ToString() + " " + current.xr.ToString());
                args.AxisButton += "RX";
                if (warmupTime < 0) ResultFound(args);
            }
            else if (yrDiff > axisThreshold && last.yr > 10 && current.yr > 10)
            {
                Console.WriteLine(last.yr.ToString() + " " + current.yr.ToString());
                args.AxisButton += "RY";
                if (warmupTime < 0) ResultFound(args);
            }
            else if (zrDiff > axisThreshold && last.zr > 10 && current.zr > 10)
            {
                Console.WriteLine(last.zr.ToString() + " " + current.zr.ToString());
                args.AxisButton += "RZ";
                if (warmupTime < 0) ResultFound(args);
            }
            else if (s1rDiff > axisThreshold && last.s1r > 10 && current.s1r > 10)
            {
                Console.WriteLine(last.s1r.ToString() + " " + current.s1r.ToString());
                args.AxisButton += "SLIDER1";
                if (warmupTime < 0) ResultFound(args);
            }
            else if (s2rDiff > axisThreshold && last.s2r > 10 && current.s2r > 10)
            {
                Console.WriteLine(last.s2r.ToString() + " " + current.s2r.ToString());
                args.AxisButton += "SLIDER2";
                if (warmupTime < 0) ResultFound(args);
            }
        }
        void CheckIfButtonGotPressed(Joystick pad, JoystickState js)
        {
            JoyAxisState last = state[pad];
            bool[] curBtns = js.GetButtons();
            for(int i=0; i<curBtns.Length; ++i)
            {
                if (last.btns[i] && !curBtns[i])
                {
                    JoystickResults args = new JoystickResults();
                    args.Device = ToDeviceString(pad);
                    args.AxisButton = (i + 1).ToString();
                    ResultFound(args);
                    return;
                }
            }
            int[] povs = js.GetPointOfViewControllers();
            for(int i=0; i<povs.Length; ++i)
            {
                if (povs[i] > -1)
                {
                    string dir = "POV"+(i+1).ToString()+"_";
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
                    JoystickResults args = new JoystickResults();
                    args.Device = ToDeviceString(pad);
                    args.AxisButton = dir;
                    ResultFound(args);
                    return;
                }
            }

        }
        string ToDeviceString(Joystick pad)
        {
            return pad.Information.InstanceName + " {" + pad.Information.ProductGuid.ToString().ToUpper() + "}";
        }
        void tick()
        {
            Console.WriteLine("Tick");
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
                }
            }
        }

        void ResultFound(JoystickResults e)
        {
            quit = true;
            result = e;
        }
    }
}
