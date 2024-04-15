using Lemon.Automation.Framework.Natives;
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
                Point point = default;
                PInvokes.GetCursorPos(ref point);

                if (point.X == aX && point.Y == aY)
                {
                    PInvokes.SetCursorPos(0, 0);
                }
            }
            inputSimulator.Mouse.MoveMouseTo(aX, aY);
        }

        public void MouseMoveAndHover(int aX, int aY, bool isForced)
        {
            if(isForced) 
            {
                Point point = default;
                PInvokes.GetCursorPos(ref point);

                if (point.X == aX && point.Y == aY)
                {
                    PInvokes.SetCursorPos(0, 0);
                }
            }
            inputSimulator.Mouse.MoveMouseTo(aX, aY);
            PInvokes.SetCursorPos(aX, aY);
        }

    }
}
