using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Framework.Extensions
{
    public static class RectExtension
    {
        public static Rectangle Downscale(this Rectangle rectangle, double factor)
        {
            Rectangle result = new Rectangle(rectangle.Location, rectangle.Size);
            result.X = (int)((double)result.X / factor);
            result.Y = (int)((double)result.Y / factor);
            result.Height = (int)((double)result.Height / factor);
            result.Width = (int)((double)result.Width / factor);
            return result;
        }
    }
}
