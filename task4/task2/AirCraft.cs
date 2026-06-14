using System;

namespace task2
{
    public abstract class AirCraft : IAirCraft
    {
        protected double _heightATG;
        protected bool _isFlying;
        protected double _speed;
        protected double _accelerationCoof;
        private int _numberOfPassengers;

        public event EventHandler<FlightEventArgs>? TakingOff;
        public event EventHandler<FlightEventArgs>? Acceleration;
        public event EventHandler<FlightEventArgs>? Climbing;
        public event EventHandler<FlightEventArgs>? Descending;
        public event EventHandler<FlightEventArgs>? Landing;
        public event EventHandler<FlightEventArgs>? OnGround;

        public AirCraft(int numberOfPassengers, double accelerationCoof = 1)
        {
            _numberOfPassengers = numberOfPassengers < 0 ? 1 : numberOfPassengers;
            _accelerationCoof = accelerationCoof < 0 ? 1 : accelerationCoof;
            _heightATG = 0;
            _speed = 0;
            _isFlying = false;
        }

        public bool IsFlying => _isFlying;
        public double HeightATG => _heightATG;
        public double Speed => _speed;
        public double AccelerationCoof => _accelerationCoof;
        public int NumberOfPassengers => _numberOfPassengers;

        protected virtual void OnTakingOff(FlightEventArgs e) => TakingOff?.Invoke(this, e);
        protected virtual void OnAcceleration(FlightEventArgs e) => Acceleration?.Invoke(this, e);
        protected virtual void OnClimbing(FlightEventArgs e) => Climbing?.Invoke(this, e);
        protected virtual void OnDescending(FlightEventArgs e) => Descending?.Invoke(this, e);
        protected virtual void OnLanding(FlightEventArgs e) => Landing?.Invoke(this, e);
        protected virtual void OnOnGround(FlightEventArgs e) => OnGround?.Invoke(this, e);

        public abstract bool TakeOff();
        public abstract void Land();

        public override string ToString()
        {
            return $"Высота: {_heightATG:F0} м, Скорость: {_speed:F0} км/ч, Статус: {(_isFlying ? "в полёте" : "на земле")}, Пассажиров: {_numberOfPassengers}";
        }
    }
}