namespace PedestrianCrossing.Models
{
    public class Car : Entity
    {
        public bool IsStopped { get; set; } = false;
        public override string Label => "🚗";

        public Car(double x, double y, double speed) : base(x, y, speed) { }

        public void Move(double canvasWidth)
        {
            if (!IsActive || IsStopped) return;
            X += Speed;
            if (X > canvasWidth) X = -40; 
        }
    }
}