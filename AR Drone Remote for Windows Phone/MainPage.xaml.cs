using System;
using AR_Drone_Controller;
using Microsoft.Phone.Controls;
using System.Windows;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public partial class MainPage : PhoneApplicationPage
    {
        private BindableCompass compass;

        // Constructor
        public MainPage()
        {
            DroneController = new DroneController
            {
                IpAddress = "192.168.1.1",
                SocketFactory = new SocketFactory(),
                Dispatcher = new DispatcherWrapper(Dispatcher)
            };

            compass = new BindableCompass {Dispatcher = Dispatcher};
            compass.Start();
            
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        public BindableCompass ControllerDirection
        {
            get { return compass; }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        public DroneController DroneController { get; set; }

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

        private void Emergency_Click(object sender, RoutedEventArgs e)
        {
            DroneController.Emergency();
        }

        private void Blink_Click(object sender, RoutedEventArgs e)
        {
            DroneController.Blink();
        }

        private void MainPage_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            try
            {
                VisualStateManager.GoToState(this, e.Orientation.ToString(), true);
            }
            catch
            {
                // TODO: log this crap
            }
        }
    }
}