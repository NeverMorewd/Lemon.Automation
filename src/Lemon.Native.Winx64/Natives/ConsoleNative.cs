using Windows.Win32;

namespace Lemon.Native.Winx64.Natives
{
    public static class ConsoleNative
    {
        public static void Attach()
        {
            PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);
        }
    }
}
