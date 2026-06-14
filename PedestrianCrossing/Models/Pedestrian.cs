namespace PedestrianCrossing.Models
{
    public class Pedestrian : Entity
    {
        public bool CanCross { get; set; } = false;
        public override string Label => "🚶";

        public Pedestrian(double x, double y, double speed) : base(x, y, speed) { }

        public void Move(double canvasHeight)
        {
            if (!IsActive || !CanCross) return;
            Y += Speed;
            if (Y > canvasHeight) Y = -40;
        }
    }
}