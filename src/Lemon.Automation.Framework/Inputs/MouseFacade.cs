using Lemon.Automation.Framework.Natives;
using System.Drawing;
using WindowsInput;

namespace Lemon.Automation.Framework.Inputs
{
    public class MouseFacade
    {
        private readonly InputSimulator inputSimulator = new();
        public void LeftButtonClick()
        {
            inputSimulator.Mouse.LeftButtonClick();
        }
        public void RightButtonClick()
        {
            inputSimulator.Mouse.RightButtonClick();
        }
        public void LeftButtonDoubleClick()
        {
            inputSimulator.Mouse.LeftButtonDoubleClick();
        }

        public void MouseMove(int aX, int aY, bool isForced)
        {
            if (isForced)
            {
                PointWrapper point = default;
                PartialNativeInvoker.GetCursorPos(ref point);

                if (point.X == aX && point.Y == aY)
                {
                    PartialNativeInvoker.SetCursorPos(0, 0);
                }
            }
            inputSimulator.Mouse.MoveMouseTo(aX, aY);
        }

        public void MouseMoveAndHover(int aX, int aY, bool isForced)
        {
            if(isForced) 
            {
                PointWrapper point = default;
                PartialNativeInvoker.GetCursorPos(ref point);

                if (point.X == aX && point.Y == aY)
                {
                    PartialNativeInvoker.SetCursorPos(0, 0);
                }
            }
            inputSimulator.Mouse.MoveMouseTo(aX, aY);
            PartialNativeInvoker.SetCursorPos(aX, aY);
        }

    }
}
