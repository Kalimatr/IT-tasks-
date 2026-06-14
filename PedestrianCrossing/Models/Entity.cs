using System;

namespace PedestrianCrossing.Models
{
    public abstract class Entity
    {
        public Guid Id { get; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }
        public bool IsActive { get; set; } = true;
        public abstract string Label { get; }

        protected Entity(double x, double y, double speed)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
            Speed = speed;
        }
    }
}