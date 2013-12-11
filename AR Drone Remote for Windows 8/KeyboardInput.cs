using System.Collections.Generic;
using AR_Drone_Controller;
using Windows.System;

namespace AR_Drone_Remote_for_Windows_8
{
    internal class KeyboardInput
    {
        public enum Keys
        {
            RollLeft,
            RollRight,
            PitchForward,
            PitchBackward,
            TurnLeft,
            TurnRight,
            Rise,
            Drop,
            TakeOffLand,
            Emergency
        }

        private Dictionary<Keys, VirtualKey> _keyMap;

        public static Dictionary<Keys, VirtualKey> GetDefautlKeyMap()
        {
            return new Dictionary<Keys, VirtualKey>
            {
                {Keys.RollLeft, VirtualKey.A},
                {Keys.RollRight, VirtualKey.D},
                {Keys.PitchForward, VirtualKey.W},
                {Keys.PitchBackward, VirtualKey.S},
                {Keys.TurnLeft, VirtualKey.Left},
                {Keys.TurnRight, VirtualKey.Right},
                {Keys.Rise, VirtualKey.Up},
                {Keys.Drop, VirtualKey.Down},
                {Keys.TakeOffLand, VirtualKey.Space},
                {Keys.Emergency, VirtualKey.Escape}
            };
        }

        public Dictionary<Keys, VirtualKey> KeyMaps
        {
            get { return _keyMap; }
            set { _keyMap = value; }
        }

        public DroneController DroneController { get; set; }
        public KeyStateIndicator KeyStateIndicator { get; set; }

        public void UpdateDroneController()
        {
            DroneController.Pitch = GetPitchFromKeyboard();
            DroneController.Roll = GetRollFromKeyboard();
            DroneController.Gaz = GetGazFromKeyboard();
            DroneController.Yaw = GetTurnFromKeyboard();

            if (IsKeyPressed(Keys.TakeOffLand))
            {
                if (DroneController.Flying)
                {
                    DroneController.Land();
                }
                else if (DroneController.Connected)
                {
                    DroneController.TakeOff();
                }
            }

            if (IsKeyPressed(Keys.Emergency) && DroneController.Connected)
            {
                DroneController.Emergency();
            }
        }

        private float GetRollFromKeyboard()
        {
            float roll;
            if (IsKeyPressed(Keys.RollLeft) && !IsKeyPressed(Keys.RollRight))
            {
                roll = -1;
            }
            else if (IsKeyPressed(Keys.RollRight) && !IsKeyPressed(Keys.RollLeft))
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
            if (IsKeyPressed(Keys.PitchForward) && !IsKeyPressed(Keys.PitchBackward))
            {
                pitch = -1;
            }
            else if (IsKeyPressed(Keys.PitchBackward) && !IsKeyPressed(Keys.PitchForward))
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
            if (IsKeyPressed(Keys.TurnLeft) && !IsKeyPressed(Keys.TurnRight))
            {
                turn = -1;
            }
            else if (IsKeyPressed(Keys.TurnRight) && !IsKeyPressed(Keys.TurnLeft))
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
            if (IsKeyPressed(Keys.Rise) && !IsKeyPressed(Keys.Drop))
            {
                upDown = 1;
            }
            else if (IsKeyPressed(Keys.Drop) && !IsKeyPressed(Keys.Rise))
            {
                upDown = -1;
            }
            else
            {
                upDown = 0;
            }

            return upDown;
        }

        private bool IsKeyPressed(Keys key)
        {
            VirtualKey virtualKey = _keyMap[key];
            return KeyStateIndicator.IsKeyPressed(virtualKey);
        }
    }
}
