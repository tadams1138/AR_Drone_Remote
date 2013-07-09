using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AR_Drone_Remote_for_Windows_8.Annotations;
using Windows.Devices.Sensors;
using Windows.UI.Core;

namespace AR_Drone_Remote_for_Windows_8
{
    public class BindableCompass : INotifyPropertyChanged, IDisposable
    {
        private Compass _compass;
        private CompassReading _currentValue;

        public event EventHandler<CompassReading> CurrentValueChanged;

        public BindableCompass()
        {
            _compass = Compass.GetDefault();
            if (_compass != null)
            {
                _compass.ReadingChanged += compass_CurrentValueChanged;
                AssignCurrentValue(_compass.GetCurrentReading());
            }
        }

        public CoreDispatcher Dispatcher { get; set; }

        public CompassReading CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                OnPropertyChanged();
            }
        }

        private void compass_CurrentValueChanged(Compass compass, CompassReadingChangedEventArgs e)
        {
            AssignCurrentValue(e.Reading);
        }

        private void AssignCurrentValue(CompassReading compassReading)
        {
            CurrentValue = compassReading;
            OnCurrentValueChanged(compassReading);
        }

        private void OnCurrentValueChanged(CompassReading e)
        {
            EventHandler<CompassReading> handler = CurrentValueChanged;
            if (handler != null)
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                                                         handler(this, e));
                task.AsTask().Wait();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => handler(this, new PropertyChangedEventArgs(propertyName)));
                task.AsTask().Wait();
            }
        }

        public void Dispose()
        {
                _compass = null;
        }
    }
}
