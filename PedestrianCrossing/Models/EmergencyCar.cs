using PedestrianCrossing.Interfaces;

namespace PedestrianCrossing.Models
{
    public class EmergencyCar : Entity, IEmergencyService
    {
        public double TargetX { get; private set; }
        public double TargetY { get; private set; }
        public bool IsOnMission { get; private set; } = false;
        public override string Label => "🚑";

        public EmergencyCar(double x, double y, double speed) : base(x, y, speed) { }

        public void Dispatch(double targetX, double targetY)
        {
            TargetX = targetX;
            TargetY = targetY;
            IsOnMission = true;
            IsActive = true;
        }

        public void MoveTowardsTarget()
        {
            if (!IsActive || !IsOnMission) return;

            double dx = TargetX - X;
            double dy = TargetY - Y;
            double distance = System.Math.Sqrt(dx * dx + dy * dy);

            if (distance < 5.0)
            {
                IsOnMission = false; 
                IsActive = false;    
                return;
            }

            X += (dx / distance) * Speed;
            Y += (dy / distance) * Speed;
        }
    }
}