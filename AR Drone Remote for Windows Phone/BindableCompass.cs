using System.Windows.Threading;
using AR_Drone_Remote_for_Windows_Phone.Annotations;
using Microsoft.Devices.Sensors;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public class BindableCompass : INotifyPropertyChanged, IDisposable
    {
        private Compass _compass;
        private CompassReading _currentValue;

        public BindableCompass()
        {
            _compass = new Compass();
            _compass.CurrentValueChanged += compass_CurrentValueChanged;
        }

        public Dispatcher Dispatcher { get; set; }

        public CompassReading CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                OnPropertyChanged();
            }
        }

        private void compass_CurrentValueChanged(object sender, SensorReadingEventArgs<CompassReading> e)
        {
            CurrentValue = e.SensorReading;
        }

        public void Start()
        {
            _compass.Start();
        }

        public void Stop()
        {
            _compass.Stop();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                    {

                        handler(this, new PropertyChangedEventArgs(propertyName));
                    }));
            }
        }

        public void Dispose()
        {
            if (_compass != null)
            {
                _compass.Dispose();
                _compass = null;
            }
        }
    }
}
