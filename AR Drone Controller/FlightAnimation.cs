using System.Collections.Generic;

namespace AR_Drone_Controller
{
    public class FlightAnimation
    {
        private string Title { get; set; }
        private CommandWorker.FlightAnimation Animation { get; set; }
        private int MaydayTimeoutInMilliseconds { get; set; }
        private DroneController DroneController { get; set; }

        internal static List<FlightAnimation> GenerateFlightAnimationList(DroneController droneController)
        {
            return new List<FlightAnimation>
                {
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.DoublePhiThetaMixed,
                            Title = "Double Phi Theta Mixed",
                            MaydayTimeoutInMilliseconds = 5000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.FlipAhead,
                            Title = "Flip Ahead",
                            MaydayTimeoutInMilliseconds = 15,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.FlipBehind,
                            Title = "Flip Behind",
                            MaydayTimeoutInMilliseconds = 15,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.FlipLeft,
                            Title = "Flip Left",
                            MaydayTimeoutInMilliseconds = 15,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.FlipRight,
                            Title = "Flip Right",
                            MaydayTimeoutInMilliseconds = 15,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.PhiDance,
                            Title = "Phi Dance",
                            MaydayTimeoutInMilliseconds = 5000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.PhiMinus30Degrees,
                            Title = "Phi Minus 30 Degrees",
                            MaydayTimeoutInMilliseconds = 1000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.PhiPlus30Degrees,
                            Title = "Phi Plus 30 Degrees",
                            MaydayTimeoutInMilliseconds = 1000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.PhiThetaMixed,
                            Title = "Phi Theta Mixed",
                            MaydayTimeoutInMilliseconds = 5000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.Theta20DegYaw200Degrees,
                            Title = "Theta 20 Degrees, Yaw 200 Degrees",
                            MaydayTimeoutInMilliseconds = 1000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.Theta20DegYawM200Degrees,
                            Title = "Theta 20 Degrees, Yaw Minus 200 Degrees",
                            MaydayTimeoutInMilliseconds = 1000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.ThetaDance,
                            Title = "Theta Dance",
                            MaydayTimeoutInMilliseconds = 5000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.ThetaMinus30Degrees,
                            Title = "Theta Minus 30 Degrees",
                            MaydayTimeoutInMilliseconds = 1000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.ThetaPlus30Degrees,
                            Title = "Theta Plus 30 Degrees",
                            MaydayTimeoutInMilliseconds = 1000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.Turnaround,
                            Title = "Turn Around",
                            MaydayTimeoutInMilliseconds = 5000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.TurnaroundAndGoDown,
                            Title = "Turn Around and Go Down",
                            MaydayTimeoutInMilliseconds = 5000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.VzDance,
                            Title = "Vertical Dance",
                            MaydayTimeoutInMilliseconds = 5000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.Wave,
                            Title = "Wave",
                            MaydayTimeoutInMilliseconds = 5000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.YawDance,
                            Title = "Yaw Dance",
                            MaydayTimeoutInMilliseconds = 5000,
                            DroneController = droneController
                        },
                    new FlightAnimation
                        {
                            Animation = CommandWorker.FlightAnimation.YawShake,
                            Title = "Yaw Shake",
                            MaydayTimeoutInMilliseconds = 2000,
                            DroneController = droneController
                        }
                };
        }


        public void Execute()
        {
            DroneController.SendFlightAnimationCommand(Animation, MaydayTimeoutInMilliseconds);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
