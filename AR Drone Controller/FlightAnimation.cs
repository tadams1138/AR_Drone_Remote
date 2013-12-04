using System.Collections.Generic;

namespace AR_Drone_Controller
{
    public class FlightAnimation : IFlightAnimation
    {
        private string Title { get; set; }
        public FlightAnimations Animation { get; set; }
        public int MaydayTimeoutInMilliseconds { get; set; }

        internal static List<FlightAnimation> GenerateFlightAnimationList()
        {
            return new List<FlightAnimation>
                {
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.DoublePhiThetaMixed,
                            Title = "Double Phi Theta Mixed",
                            MaydayTimeoutInMilliseconds = 5000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.FlipAhead,
                            Title = "Flip Ahead",
                            MaydayTimeoutInMilliseconds = 15
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.FlipBehind,
                            Title = "Flip Behind",
                            MaydayTimeoutInMilliseconds = 15
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.FlipLeft,
                            Title = "Flip Left",
                            MaydayTimeoutInMilliseconds = 15
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.FlipRight,
                            Title = "Flip Right",
                            MaydayTimeoutInMilliseconds = 15
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.PhiDance,
                            Title = "Phi Dance",
                            MaydayTimeoutInMilliseconds = 5000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.PhiMinus30Degrees,
                            Title = "Phi Minus 30 Degrees",
                            MaydayTimeoutInMilliseconds = 1000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.PhiPlus30Degrees,
                            Title = "Phi Plus 30 Degrees",
                            MaydayTimeoutInMilliseconds = 1000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.PhiThetaMixed,
                            Title = "Phi Theta Mixed",
                            MaydayTimeoutInMilliseconds = 5000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.Theta20DegYaw200Degrees,
                            Title = "Theta 20 Degrees, Yaw 200 Degrees",
                            MaydayTimeoutInMilliseconds = 1000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.Theta20DegYawM200Degrees,
                            Title = "Theta 20 Degrees, Yaw Minus 200 Degrees",
                            MaydayTimeoutInMilliseconds = 1000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.ThetaDance,
                            Title = "Theta Dance",
                            MaydayTimeoutInMilliseconds = 5000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.ThetaMinus30Degrees,
                            Title = "Theta Minus 30 Degrees",
                            MaydayTimeoutInMilliseconds = 1000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.ThetaPlus30Degrees,
                            Title = "Theta Plus 30 Degrees",
                            MaydayTimeoutInMilliseconds = 1000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.Turnaround,
                            Title = "Turn Around",
                            MaydayTimeoutInMilliseconds = 5000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.TurnaroundAndGoDown,
                            Title = "Turn Around and Go Down",
                            MaydayTimeoutInMilliseconds = 5000,
                            
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.VzDance,
                            Title = "Vertical Dance",
                            MaydayTimeoutInMilliseconds = 5000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.Wave,
                            Title = "Wave",
                            MaydayTimeoutInMilliseconds = 5000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.YawDance,
                            Title = "Yaw Dance",
                            MaydayTimeoutInMilliseconds = 5000
                        },
                    new FlightAnimation
                        {
                            Animation = FlightAnimations.YawShake,
                            Title = "Yaw Shake",
                            MaydayTimeoutInMilliseconds = 2000
                        }
                };
        }
        
        public override string ToString()
        {
            return Title;
        }
    }
}
