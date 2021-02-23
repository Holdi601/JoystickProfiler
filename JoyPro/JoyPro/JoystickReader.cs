using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace JoyPro
{
    public class JoystickReader
    {
        List<DeviceInstance> directInputList;
        SlimDX.XInput.Controller contrl;
        DirectInput directInput;
        List<SlimDX.DirectInput.Joystick> gamepads;
        Dictionary<Joystick, JoystickState> state;
        public bool detectionEventActiveButton;
        public bool detectionEventActiveAxis;
        bool quit;
        public JoystickReader()
        {
            directInputList = new List<DeviceInstance>();
            directInput = new DirectInput();
            contrl = new SlimDX.XInput.Controller(SlimDX.XInput.UserIndex.Any);
            gamepads = new List<Joystick>();
            state = new Dictionary<Joystick, JoystickState>();
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
                tick();
                Thread.Sleep(10);
            }
        }

        List<string> GetButtonDiff(JoystickState state1, JoystickState state2)
        {
            List<string> result = new List<string>();

            return result;
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
                bool[] btns = currentState.GetButtons();
                int[] povs = currentState.GetPointOfViewControllers();
                //Console.WriteLine(gamepad.Information.InstanceName);
                //for (int i = 0; i < povs.Length; ++i) Console.WriteLine(i.ToString() + " " + povs[i].ToString());
            }
        }
    }
}
