using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Reminders;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    public partial class App
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            ApplicationUsageHelper.Init("1.0");
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            ApplicationUsageHelper.OnApplicationActivated();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool _phoneApplicationInitialized;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (_phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            _phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        private static RadTrialApplicationReminder _trialReminder;

        public static RadTrialApplicationReminder TrialReminder
        {
            get
            {
                if (_trialReminder == null)
                {
                    InitializeTrialReminder();
                }

                return _trialReminder;
            }
        }

        private static void InitializeTrialReminder()
        {
            _trialReminder = new RadTrialApplicationReminder
                {
                    AllowedTrialPeriod = TimeSpan.FromDays(30),
                    AreFurtherRemindersSkipped = false,
                    AllowUsersToSkipFurtherReminders = false,
                    OccurrenceUsageCount = 1,
                    TrialExpiredMessageBoxInfo = CreateTrialExpiredMessageBoxInfo()
                };

            _trialReminder.TrialReminderMessageBoxInfo = CreateTrialReminderMessageBoxInfo(_trialReminder);
        }

        private static MessageBoxInfoModel CreateTrialReminderMessageBoxInfo(RadTrialApplicationReminder trialReminder)
        {
            var remainingTrialPeriod = trialReminder.RemainingTrialPeriod ?? TimeSpan.FromDays(0);
            return new MessageBoxInfoModel
                {
                    Buttons = MessageBoxButtons.YesNo,
                    Title = "Trial Reminder",
                    Content = string.Format("You are using the trial version of this application. You have " +
                                            "{0:0} day(s) left.{1}" +
                                            "We hope you are enjoying this app as much as we do. You know what " +
                                            "else we enjoy? Getting paid. You know what annoys us too? Frequent " +
                                            "trial reminders. See how much we have in common? Please consider " +
                                            "supporting new feature development for less than you probably chip " +
                                            "in when you go in on a pizza with friends, and all these trial " +
                                            "reminders will just go away as if by magic.{1}" +
                                            "Do you want to purchase this app now? If you choose Yes - you will " +
                                            "be redirected to the marketplace.",
                                            remainingTrialPeriod.TotalDays, Environment.NewLine)
                };
        }

        private static MessageBoxInfoModel CreateTrialExpiredMessageBoxInfo()
        {
            return new MessageBoxInfoModel
                {
                    Title = "O noz! Trial Expired :(",
                    Content = "The trial period has expired and our kids are getting hungry. To help support " +
                              "the hard work that went into making this app as well as the development of new " +
                              "features, please consider buying this app. Press OK buy the app from the marketplace " +
                              "now."
                };
        }
    }
}