using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    public partial class Joystick
    {
        private const double ControlRadius = 50;
        private const double KnobRadius = 25;
        private const double KnobLowerBound = -25;
        private const double KnobUpperBound = 75;
        private const double SignificantChangeThreshold = 0.001;
        private readonly TranslateTransform _move = new TranslateTransform();
        private readonly TransformGroup _rectangleTransforms = new TransformGroup();

        private double _x;
        private double _y;

        public delegate void ValueChangedEventHandler(object sender, double value);
        public event ValueChangedEventHandler XValueChanged;
        public event ValueChangedEventHandler YValueChanged;

        public Joystick()
        {
            InitializeComponent();

            _rectangleTransforms.Children.Add(_move);
            Knob.RenderTransform = _rectangleTransforms;
            _move.X = _move.Y = ControlRadius - KnobRadius;

            Touch.FrameReported += Touch_FrameReported;
        }

        public double X
        {
            get { return _x; }
            private set
            {
                if (SignificantlyDifferent(_x, value))
                {
                    _x = value;
                    _move.X = (KnobUpperBound - KnobLowerBound) / 2 * _x + ControlRadius - KnobRadius;
                    OnXValueChanged();
                }
            }
        }

        public double Y
        {
            get { return _y; }
            private set
            {
                if (SignificantlyDifferent(_y, value))
                {
                    _y = value;
                    _move.Y = (KnobUpperBound - KnobLowerBound) / 2 * _y + ControlRadius - KnobRadius;
                    OnYValueChanged();
                }
            }
        }

        private void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            var touchPoints = e.GetTouchPoints(LayoutRoot);
            var pointsInBounds = touchPoints.Where(p => IsInBounds(p.Position) && p.Action != TouchAction.Up).Select(p => p.Position);
            var pointArray = pointsInBounds as Point[] ?? pointsInBounds.ToArray();

            if (pointArray.Any())
            {
                SetJoystickToNewPoint(pointArray.First());
            }
            else
            {
                ResetHandleToOrigin();
            }
        }

        private bool IsInBounds(Point position)
        {
            return position.X >= 0 && position.Y >= 0 && position.Y <= LayoutRoot.ActualHeight &&
                   position.X <= LayoutRoot.ActualWidth;
        }

        private bool SignificantlyDifferent(double oldValue, double value)
        {
            return Math.Abs(oldValue - value) > SignificantChangeThreshold;
        }

        private void OnXValueChanged()
        {
            if (XValueChanged != null)
            {
                XValueChanged(this, X);
            }
        }

        private void OnYValueChanged()
        {
            if (YValueChanged != null)
            {
                YValueChanged(this, Y);
            }
        }

        private void ResetHandleToOrigin()
        {
            X = 0.0;
            Y = 0.0;
        }

        private void SetJoystickToNewPoint(Point newPoint)
        {
            X = Normalize((newPoint.X - ControlRadius) / ((KnobUpperBound - KnobLowerBound) / 2));
            Y = Normalize((newPoint.Y - ControlRadius) / ((KnobUpperBound - KnobLowerBound) / 2));
        }

        private double Normalize(double value)
        {
            return Math.Max(Math.Min(value, 1), -1);
        }
    }
}