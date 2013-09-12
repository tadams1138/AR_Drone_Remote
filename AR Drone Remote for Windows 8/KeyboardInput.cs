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
            Drop
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
                {Keys.Drop, VirtualKey.Down}
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
        }

        private float GetRollFromKeyboard()
        {
            float roll;
            if (IsKeyDown(Keys.RollLeft) && !IsKeyDown(Keys.RollRight))
            {
                roll = -1;
            }
            else if (IsKeyDown(Keys.RollRight) && !IsKeyDown(Keys.RollLeft))
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
            if (IsKeyDown(Keys.PitchForward) && !IsKeyDown(Keys.PitchBackward))
            {
                pitch = -1;
            }
            else if (IsKeyDown(Keys.PitchBackward) && !IsKeyDown(Keys.PitchForward))
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
            if (IsKeyDown(Keys.TurnLeft) && !IsKeyDown(Keys.TurnRight))
            {
                turn = -1;
            }
            else if (IsKeyDown(Keys.TurnRight) && !IsKeyDown(Keys.TurnLeft))
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
            if (IsKeyDown(Keys.Rise) && !IsKeyDown(Keys.Drop))
            {
                upDown = 1;
            }
            else if (IsKeyDown(Keys.Drop) && !IsKeyDown(Keys.Rise))
            {
                upDown = -1;
            }
            else
            {
                upDown = 0;
            }

            return upDown;
        }

        private bool IsKeyDown(Keys key)
        {
            return KeyStateIndicator.IsKeyDown(_keyMap[key]);
        }
    }
}
