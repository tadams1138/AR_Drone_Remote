using AR_Drone_Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using Windows.System;

namespace AR_Drone_Remote_for_Windows_8
{
    [TestClass]
    public class KeyboardInputTests
    {
        private Mock<DroneController> _mockDroneController;
        private Mock<KeyStateIndicator> _mockKeyStateIndicator;
        private KeyboardInput _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _mockDroneController = new Mock<DroneController>();
            _mockKeyStateIndicator = new Mock<KeyStateIndicator>();
            _target = new KeyboardInput
            {
                DroneController = _mockDroneController.Object,
                KeyStateIndicator = _mockKeyStateIndicator.Object,
                KeyMaps = GetStubDictionary()
            };
        }

        private static Dictionary<KeyboardInput.Keys, VirtualKey> GetStubDictionary()
        {
            const VirtualKey unusedKey = VirtualKey.NumberKeyLock;
            return new Dictionary<KeyboardInput.Keys, VirtualKey>
                {
                    {KeyboardInput.Keys.Drop, unusedKey},
                    {KeyboardInput.Keys.PitchBackward, unusedKey},
                    {KeyboardInput.Keys.PitchForward, unusedKey},
                    {KeyboardInput.Keys.Rise, unusedKey},
                    {KeyboardInput.Keys.RollLeft, unusedKey},
                    {KeyboardInput.Keys.RollRight, unusedKey},
                    {KeyboardInput.Keys.TurnLeft, unusedKey},
                    {KeyboardInput.Keys.TurnRight, unusedKey}
                };
        }

        [TestMethod]
        public void UpdateDroneController_NoKeysArePressed_AllFlightControllsAssigned0()
        {
            // Arrange
            // set no key state to down

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Roll = 0);
            _mockDroneController.VerifySet(x => x.Pitch = 0);
            _mockDroneController.VerifySet(x => x.Yaw = 0);
            _mockDroneController.VerifySet(x => x.Gaz = 0);
        }

        #region Roll Tests

        [TestMethod]
        public void UpdateDroneController_RollLeftKeyPressed_RollAssignedNegative1()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.RollLeft, VirtualKey.A);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Roll = -1);
        }

        [TestMethod]
        public void UpdateDroneController_RollRightKeyPressed_RollAssignedPositive1()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.RollRight, VirtualKey.D);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Roll = 1);
        }

        [TestMethod]
        public void UpdateDroneController_BothRollKeysPressed_RollAssignedZero()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.RollLeft, VirtualKey.A);
            SetKeyStateToDown(KeyboardInput.Keys.RollRight, VirtualKey.D);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Roll = 0);
        }

        #endregion

        #region Pitch Tests

        [TestMethod]
        public void UpdateDroneController_PitchBackKeyPressed_PitchAssignedPositive1()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.PitchBackward, VirtualKey.S);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Pitch = 1);
        }

        [TestMethod]
        public void UpdateDroneController_PitchForwardKeyPressed_PitchAssignedNegative1()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.PitchForward, VirtualKey.W);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Pitch = -1);
        }

        [TestMethod]
        public void UpdateDroneController_BothPitchKeysPressed_PitchAssigned0()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.PitchBackward, VirtualKey.S);
            SetKeyStateToDown(KeyboardInput.Keys.PitchForward, VirtualKey.W);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Pitch = 0);
        }

        #endregion

        #region Turn Tests

        [TestMethod]
        public void UpdateDroneController_TurnLeftKeyPressed_YawAssignedNegative1()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.TurnLeft, VirtualKey.Left);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Yaw = -1);
        }

        [TestMethod]
        public void UpdateDroneController_TurnRightKeyPressed_YawAssignedPositive1()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.TurnRight, VirtualKey.Right);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Yaw = 1);
        }

        [TestMethod]
        public void UpdateDroneController_BothTurnKeysPressed_YawAssigned0()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.TurnLeft, VirtualKey.Left);
            SetKeyStateToDown(KeyboardInput.Keys.TurnRight, VirtualKey.Right);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Yaw = 0);
        }

        #endregion

        #region Gaz Tests

        [TestMethod]
        public void UpdateDroneController_DropKeyPressed_GazAssignedNegative1()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.Drop, VirtualKey.Down);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Gaz = -1);
        }

        [TestMethod]
        public void UpdateDroneController_RiseKeyPressed_GazAssignedPositive1()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.Rise, VirtualKey.Up);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Gaz = 1);
        }

        [TestMethod]
        public void UpdateDroneController_BothGazKeysPressed_GazAssigned0()
        {
            // Arrange
            SetKeyStateToDown(KeyboardInput.Keys.Drop, VirtualKey.Down);
            SetKeyStateToDown(KeyboardInput.Keys.Rise, VirtualKey.Up);

            // Act
            _target.UpdateDroneController();

            // Assert
            _mockDroneController.VerifySet(x => x.Gaz = 0);
        }

        #endregion

        private void SetKeyStateToDown(KeyboardInput.Keys keyMap, VirtualKey key)
        {
            _target.KeyMaps[keyMap] = key;
            _mockKeyStateIndicator.Setup(x => x.IsKeyPressed(key)).Returns(true);
        }
    }
}