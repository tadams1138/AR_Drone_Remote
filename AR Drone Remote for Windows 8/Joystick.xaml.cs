using System;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace AR_Drone_Remote_for_Windows_8
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

        private bool _pointerPressed;
        private double _x;
        private double _y;
        
        public event EventHandler<double> XValueChanged;
        public event EventHandler<double> YValueChanged;

        public Joystick()
        {
            InitializeComponent();

            _rectangleTransforms.Children.Add(_move);
            Knob.RenderTransform = _rectangleTransforms;
            _move.X = _move.Y = ControlRadius - KnobRadius;
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
        
        private void UIElement_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SetJoyStickToPoint(e.GetCurrentPoint(LayoutRoot).Position);
        }
        
        private void UIElement_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            _pointerPressed = false;
            ResetHandleToOrigin();
        }

        private void UIElement_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_pointerPressed)
            {
                SetJoyStickToPoint(e.GetCurrentPoint(LayoutRoot).Position);
            }
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

        private void SetJoyStickToPoint(Point newPoint)
        {
            _pointerPressed = true;
            X = Normalize((newPoint.X - ControlRadius) / ((KnobUpperBound - KnobLowerBound) / 2));
            Y = Normalize((newPoint.Y - ControlRadius) / ((KnobUpperBound - KnobLowerBound) / 2));
        }

        private double Normalize(double value)
        {
            return Math.Max(Math.Min(value, 1), -1);
        }
    }
}