using AutoIt;

namespace Lemon.Automation.Framework.Utils
{
    public static class AutoItToolkit
    {
        public static bool MouseClick(IntPtr winHandle, 
            IntPtr controlHandle, 
            string button = "left", 
            int numClicks = 1, 
            int x = -2147483647, 
            int y = -2147483647)
        {
            var ret = AutoItX.ControlClick(winHandle, controlHandle, button, numClicks, x, y);
            return ret == 0;
        }
    }
}
