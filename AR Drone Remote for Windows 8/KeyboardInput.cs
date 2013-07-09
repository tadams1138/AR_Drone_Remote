using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Drone_Controller;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace AR_Drone_Remote_for_Windows_8
{
    class KeyboardInput
    {
        private VirtualKey RollLeftKey = VirtualKey.A;
        private VirtualKey RollRightKey = VirtualKey.D;
        private VirtualKey PitchForwardKey = VirtualKey.W;
        private VirtualKey PitchBackwardKey = VirtualKey.S;
        private VirtualKey TurnLeftKey = VirtualKey.Left;
        private VirtualKey TurnRightKey = VirtualKey.Right;
        private VirtualKey RiseKey = VirtualKey.Up;
        private VirtualKey DropKey = VirtualKey.Down;

        public DroneController DroneController { get; set; }

        public void UpdateDroneController()
        {
            DroneController.Pitch = GetPitchFromKeyboard();
            DroneController.Roll = GetRollFromKeyboard();
            DroneController.Gaz = GetGazFromKeyboard();
            DroneController.Yaw = GetTurnFromKeyboard();
        }

        private float GetRollFromKeyboard()
        {
            float roll;
            if (IsKeyDown(RollLeftKey) && !IsKeyDown(RollRightKey))
            {
                roll = -1;
            }
            else if (IsKeyDown(RollRightKey) && !IsKeyDown(RollLeftKey))
            {
                roll = 1;
            }
            else
            {
                roll = 0;
            }

            return roll;
        }

        private float GetPitchFromKeyboard()
        {
            float pitch;
            if (IsKeyDown(PitchForwardKey) && !IsKeyDown(PitchBackwardKey))
            {
                pitch = -1;
            }
            else if (IsKeyDown(PitchBackwardKey) && !IsKeyDown(PitchForwardKey))
            {
                pitch = 1;
            }
            else
            {
                pitch = 0;
            }

            return pitch;
        }

        private float GetTurnFromKeyboard()
        {
            float turn;
            if (IsKeyDown(TurnLeftKey) && !IsKeyDown(TurnRightKey))
            {
                turn = -1;
            }
            else if (IsKeyDown(TurnRightKey) && !IsKeyDown(TurnLeftKey))
            {
                turn = 1;
            }
            else
            {
                turn = 0;
            }

            return turn;
        }

        private float GetGazFromKeyboard()
        {
            float upDown;
            if (IsKeyDown(RiseKey) && !IsKeyDown(DropKey))
            {
                upDown = -1;
            }
            else if (IsKeyDown(DropKey) && !IsKeyDown(RiseKey))
            {
                upDown = 1;
            }
            else
            {
                upDown = 0;
            }

            return upDown;
        }

        private static bool IsKeyDown(VirtualKey key)
        {
            return Window.Current.CoreWindow.GetAsyncKeyState(key) != CoreVirtualKeyStates.None;
        }
    }
}
