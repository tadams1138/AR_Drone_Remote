using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace AR_Drone_Remote_for_Windows_8
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            base.OnWindowCreated(args);
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            var aboutCommand = new SettingsCommand("OnlineHelpId", "Online Help", OnOnlineHelpCommand);
            var privacyCommand = new SettingsCommand("PrivacyId", "Privacy Statement", OnPrivacyCommand);
            var rateAndReviewCommand = new SettingsCommand("RateAndReviewId", "Rate and Review", OnRateAndReviewCommand);
            eventArgs.Request.ApplicationCommands.Add(aboutCommand);
            eventArgs.Request.ApplicationCommands.Add(privacyCommand);
            eventArgs.Request.ApplicationCommands.Add(rateAndReviewCommand);
        }

        void OnOnlineHelpCommand(IUICommand command)
        {
            var uri = new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote_4115.html");
            Windows.System.Launcher.LaunchUriAsync(uri);
        }

        void OnRateAndReviewCommand(IUICommand command)
        {
            var uri = new Uri("ms-windows-store:REVIEW?PFN=9525TISoftware.ARDroneRemote_bc0wgwa5kk60m");
            Windows.System.Launcher.LaunchUriAsync(uri);
        }

        void OnPrivacyCommand(IUICommand command)
        {
            var uri = new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote-privacy-policy.html");
            Windows.System.Launcher.LaunchUriAsync(uri);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
