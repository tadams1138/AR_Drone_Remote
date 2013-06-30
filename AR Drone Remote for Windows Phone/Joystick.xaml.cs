using AR_Drone_Remote_for_Windows_Phone.Annotations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public partial class Joystick : INotifyPropertyChanged
    {
        private const double CoordinateLowerBound = -0.5;
        private const double CoordinateUpperBound = 2.5;
        private const double SignificantChangeThreshold = 0.001;
        private const double Factor = 2 / (CoordinateUpperBound - CoordinateLowerBound);
        private readonly TranslateTransform _move = new TranslateTransform();
        private readonly TransformGroup _rectangleTransforms = new TransformGroup();

        private double _x;
        private double _y;

        public Joystick()
        {
            InitializeComponent();

            ResetHandleToOrigin();
            _rectangleTransforms.Children.Add(_move);
            Handle.RenderTransform = _rectangleTransforms;
        }

        public double X
        {
            get { return _x; }
            private set
            {
                if (SignificantlyDifferent(_x, value))
                {
                    _x = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                    OnYValueChanged();
                }
            }
        }

        private bool SignificantlyDifferent(double oldValue, double value)
        {
            return Math.Abs(oldValue - value) > SignificantChangeThreshold;
        }

        public event EventHandler<double> XValueChanged;
        public event EventHandler<double> YValueChanged;

        private void OnXValueChanged()
        {
            var handler = XValueChanged;
            if (handler != null)
            {
                Dispatcher.BeginInvoke(() => handler(this, X));
            }
        }

        private void OnYValueChanged()
        {
            var handler = YValueChanged;
            if (handler != null)
            {
                Dispatcher.BeginInvoke(() => handler(this, Y));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                Dispatcher.BeginInvoke(() => handler(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        private void Joystick_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            ResetHandleToOrigin();
        }

        private void ResetHandleToOrigin()
        {
            _move.X = 1;
            _move.Y = 1;
            X = 0.0;
            Y = 0.0;
        }

        private void Joystick_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            _move.X = CalculateNewCoordinate(_move.X, e.DeltaManipulation.Translation.X);
            X = -1 + (_move.X - CoordinateLowerBound) * Factor;

            _move.Y = CalculateNewCoordinate(_move.Y, e.DeltaManipulation.Translation.Y);
            Y = -1 + (_move.Y - CoordinateLowerBound) * Factor;
        }

        private double CalculateNewCoordinate(double oldCoordinate, double delta)
        {
            var newCoordinate = Normalize(oldCoordinate + delta);
            return newCoordinate;
        }

        private double Normalize(double coordinate)
        {
            double result = Math.Max(CoordinateLowerBound, coordinate);
            result = Math.Min(result, CoordinateUpperBound);
            return result;
        }
    }
}
