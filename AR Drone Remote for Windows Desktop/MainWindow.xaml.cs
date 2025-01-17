﻿namespace AR_Drone_Remote_for_Windows_Desktop
{
    using AR_Drone_Controller;
    using System;
    using System.Windows;
    using System.Windows.Input;

    public partial class MainWindow
    {
        public MainWindow()
        {
            DroneController = new DroneController
                {
                    SocketFactory = new SocketFactory(),
                    Dispatcher = new DispatcherWrapper(Dispatcher)
                };

            InitializeComponent();
        }

        public DroneController DroneController { get; set; }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                DroneController.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error disposing of the drone controller." + ex);
            }

            base.OnClosing(e);
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            DroneController.Connect();
        }

        private void Land_Click(object sender, RoutedEventArgs e)
        {
            DroneController.Land();
        }

        private void TakeOff_Click(object sender, RoutedEventArgs e)
        {
            DroneController.TakeOff();
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    DroneController.Emergency();
                    break;
                case Key.G:
                    DroneController.Blink();
                    break;
                case Key.W:
                    DroneController.Pitch = -.5f;
                    break;
                case Key.A:
                    DroneController.Roll = -.5f;
                    break;
                case Key.S:
                    DroneController.Pitch = .5f;
                    break;
                case Key.D:
                    DroneController.Roll = .5f;
                    break;
                case Key.Left:
                    DroneController.Yaw = -.5f;
                    break;
                case Key.Right:
                    DroneController.Yaw = .5f;
                    break;
                case Key.Up:
                    DroneController.Gaz = .5f;
                    break;
                case Key.Down:
                    DroneController.Gaz = -.5f;
                    break;
            }
        }

        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    DroneController.Pitch = 0;
                    break;
                case Key.A:
                    DroneController.Roll = 0;
                    break;
                case Key.S:
                    DroneController.Roll = 0;
                    break;
                case Key.D:
                    DroneController.Pitch = 0;
                    break;
                case Key.Left:
                    DroneController.Yaw = 0;
                    break;
                case Key.Right:
                    DroneController.Yaw = 0;
                    break;
                case Key.Up:
                    DroneController.Gaz = 0;
                    break;
                case Key.Down:
                    DroneController.Gaz = 0;
                    break;
            }
        }
    }
}
