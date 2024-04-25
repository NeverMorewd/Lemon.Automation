namespace Lemon.Automation.Framework.Models
{
    public struct Dpi
    {
        public double X
        {
            get; set;
        }
        public double Y
        {
            get; set;
        }
        public Dpi(double aX, double aY)
        {
            X = aX;
            Y = aY;
        }
        public override string ToString()
        {
            return $"[{X},{Y}]";
        }
    }
}
