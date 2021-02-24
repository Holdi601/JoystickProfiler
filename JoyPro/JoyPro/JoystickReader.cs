using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace JoyPro
{
    public class JoystickEventArgs : EventArgs
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
        public bool detectionEventActiveButton;
        public bool detectionEventActiveAxis;
        bool quit;
        public event EventHandler<JoystickEventArgs> AxisSet;
        public event EventHandler<JoystickEventArgs> ButtonSet;
        const int timeToSet = 10000;
        const int axisThreshold = 2000;
        int timeLeftToSet = timeToSet;
        public JoystickReader()
        {
            directInputList = new List<DeviceInstance>();
            directInput = new DirectInput();
            contrl = new SlimDX.XInput.Controller(SlimDX.XInput.UserIndex.Any);
            gamepads = new List<Joystick>();
            state = new Dictionary<Joystick, JoyAxisState>();
            detectionEventActiveButton = false;
            detectionEventActiveAxis = false;
            quit = false;
            initJoystick();
            Thread t = new Thread(startPolling);
            t.Start();
        }
        public void Quit()
        {
            quit = true;
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
            if (jas.x < 1) jas.x = filler.x;
            if (jas.y < 1) jas.y = filler.y;
            if (jas.z < 1) jas.z = filler.z;
            if (jas.xr < 1) jas.xr = filler.xr;
            if (jas.yr < 1) jas.yr = filler.yr;
            if (jas.zr < 1) jas.zr = filler.zr;
            if (jas.s1r < 1) jas.s1r = filler.s1r;
            if (jas.s2r < 1) jas.s2r = filler.s2r;
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
                Thread.Sleep(10);
                tick();
                if (timeLeftToSet > -1&&(detectionEventActiveAxis||detectionEventActiveButton))
                {
                    timeLeftToSet = timeLeftToSet - 10;
                    tick();
                }else if(timeLeftToSet<0 && (detectionEventActiveAxis || detectionEventActiveButton))
                {
                    if (detectionEventActiveAxis) AxisSet.Invoke(this, null);
                    if (detectionEventActiveButton) ButtonSet.Invoke(this, null);
                    detectionEventActiveButton = false;
                    detectionEventActiveAxis = false;
                    timeLeftToSet = timeToSet;
                    state.Clear();
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
            JoystickEventArgs args = new JoystickEventArgs();
            args.Device = ToDeviceString(pad);
            args.AxisButton = "JOY_";
            if (xDiff > axisThreshold)
            {
                args.AxisButton += "X";
                OnAxisSet(args);
            }
            else if (yDiff > axisThreshold)
            {
                args.AxisButton += "Y";
                OnAxisSet(args);
            }
            else if (zDiff > axisThreshold)
            {
                args.AxisButton += "Z";
                OnAxisSet(args);
            }
            else if (xrDiff > axisThreshold)
            {
                args.AxisButton += "RX";
                OnAxisSet(args);
            }
            else if (yrDiff > axisThreshold)
            {
                args.AxisButton += "RY";
                OnAxisSet(args);
            }
            else if (zrDiff > axisThreshold)
            {
                args.AxisButton += "RZ";
                OnAxisSet(args);
            }
            else if (s1rDiff > axisThreshold)
            {
                args.AxisButton += "SLIDER1";
                OnAxisSet(args);
            }
            else if (s2rDiff > axisThreshold)
            {
                args.AxisButton += "SLIDER2";
                OnAxisSet(args);
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
                    JoystickEventArgs args = new JoystickEventArgs();
                    args.Device = ToDeviceString(pad);
                    args.AxisButton = (i + 1).ToString();
                    OnButtonSet(args);
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
                    JoystickEventArgs args = new JoystickEventArgs();
                    args.Device = ToDeviceString(pad);
                    args.AxisButton = dir;
                    OnButtonSet(args);
                    return;
                }
            }

        }
        string ToDeviceString(Joystick pad)
        {
            return pad.Information.InstanceName + " {" + pad.Information.ProductGuid + "}";
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
                }
            }
        }

        void OnAxisSet(JoystickEventArgs e)
        {
            AxisSet.Invoke(this, e);
            detectionEventActiveButton = false;
            detectionEventActiveAxis = false;
            timeLeftToSet = timeToSet;
            state.Clear();
        }

        void OnButtonSet(JoystickEventArgs e)
        {
            ButtonSet.Invoke(this, e);
            detectionEventActiveButton = false;
            detectionEventActiveAxis = false;
            timeLeftToSet = timeToSet;
            state.Clear();
        }
    }
}
