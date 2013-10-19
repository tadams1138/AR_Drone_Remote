using Windows.UI.Core;

namespace AR_Drone_Remote_for_Windows_8
{
    public class KeyStateIndicator
    {
        public ICoreWindow CoreWindow { get; set; }

        public virtual bool IsKeyPressed(Windows.System.VirtualKey key)
        {
            return CoreWindow.GetAsyncKeyState(key) != CoreVirtualKeyStates.None;
        }
    }
}
