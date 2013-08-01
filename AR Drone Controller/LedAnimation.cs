

using System.Collections.Generic;

namespace AR_Drone_Controller
{
    public class LedAnimation
    {
        private const float DefaultFrequencyInHz = 2f;
        private const int DefautlDurationInSeconds = 5;

        private string Title { get; set; }
        private CommandWorker.LedAnimation Animation { get; set; }
        private float FrequencyInHz { get; set; }
        private int DurationInSeconds { get; set; }
        private DroneController DroneController { get; set; }

        internal static List<LedAnimation> GenerateLedAnimationList(DroneController droneController)
        {
            return new List<LedAnimation>
                {
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.Blank,
                            Title = "Blank",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.BlinkGreen,
                            Title = "Blink Green",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.BlinkGreenRed,
                            Title = "Blink Green Red",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.BlinkOrange,
                            Title = "Blink Orange",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.BlinkRed,
                            Title = "Blink Red",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.BlinkStandard,
                            Title = "Blink Standard",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.DoubleMissile,
                            Title = "Double Missile",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.Fire,
                            Title = "Fire",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.FrontLeftGreenOthersRed,
                            Title = "Front Left Green Others Red",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.FrontRightGreenOthersRed,
                            Title = "Front Right Green Others Red",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.Green,
                            Title = "Green",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.LeftGreenRightRed,
                            Title = "Left Green Right Red",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.LeftMissile,
                            Title = "Left Missile",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.LeftRedRightGreen,
                            Title = "Left Red Right Green",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.RearLeftGreenOthersRed,
                            Title = "Rear Left Green Others Red",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.RearRightGreenOthersRed,
                            Title = "Rear Right Green Others Red",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.Red,
                            Title = "Red",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.RedSnake,
                            Title = "Red Snake",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.RightMissile,
                            Title = "Right Missile",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.SnakeGreenRed,
                            Title = "Snake Green Red",
                            DroneController = droneController
                        },
                    new LedAnimation
                        {
                            Animation = CommandWorker.LedAnimation.Standard,
                            Title = "Standard",
                            DroneController = droneController
                        }
                };
        }

        private LedAnimation()
        {
            FrequencyInHz = DefaultFrequencyInHz;
            DurationInSeconds = DefautlDurationInSeconds;
        }

        public void Execute()
        {
            DroneController.SendLedAnimationCommand(Animation, FrequencyInHz, DurationInSeconds);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
