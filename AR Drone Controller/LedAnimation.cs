

using System.Collections.Generic;

namespace AR_Drone_Controller
{
    public class LedAnimation : ILedAnimation
    {
        private const float DefaultFrequencyInHz = 2f;
        private const int DefautlDurationInSeconds = 5;

        private string Title { get; set; }
        public LedAnimations Animation { get; set; }
        public float FrequencyInHz { get; set; }
        public int DurationInSeconds { get; set; }

        internal static List<LedAnimation> GenerateLedAnimationList()
        {
            return new List<LedAnimation>
                {
                    new LedAnimation
                        {
                            Animation = LedAnimations.Blank,
                            Title = "Blank"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.BlinkGreen,
                            Title = "Blink Green"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.BlinkGreenRed,
                            Title = "Blink Green Red"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.BlinkOrange,
                            Title = "Blink Orange"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.BlinkRed,
                            Title = "Blink Red"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.BlinkStandard,
                            Title = "Blink Standard"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.DoubleMissile,
                            Title = "Double Missile"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.Fire,
                            Title = "Fire"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.FrontLeftGreenOthersRed,
                            Title = "Front Left Green Others Red"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.FrontRightGreenOthersRed,
                            Title = "Front Right Green Others Red"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.Green,
                            Title = "Green"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.LeftGreenRightRed,
                            Title = "Left Green Right Red"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.LeftMissile,
                            Title = "Left Missile"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.LeftRedRightGreen,
                            Title = "Left Red Right Green"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.RearLeftGreenOthersRed,
                            Title = "Rear Left Green Others Red"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.RearRightGreenOthersRed,
                            Title = "Rear Right Green Others Red"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.Red,
                            Title = "Red"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.RedSnake,
                            Title = "Red Snake"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.RightMissile,
                            Title = "Right Missile"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.SnakeGreenRed,
                            Title = "Snake Green Red"
                        },
                    new LedAnimation
                        {
                            Animation = LedAnimations.Standard,
                            Title = "Standard"
                        }
                };
        }

        private LedAnimation()
        {
            FrequencyInHz = DefaultFrequencyInHz;
            DurationInSeconds = DefautlDurationInSeconds;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
