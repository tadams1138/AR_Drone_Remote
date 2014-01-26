using Windows.UI.Xaml;

namespace AR_Drone_Remote_for_Windows_8
{
    public sealed partial class Settings
    {
        public Settings(MainPage mainPage)
        {
            InitializeComponent();
            DataContext = mainPage;
        }

        private void ResetSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainPage)DataContext).ResetSettings();
        }
    }
}
