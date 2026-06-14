using System;

namespace task2
{
    public class Helicopter : AirCraft
    {
        private readonly string _model;

        public Helicopter(string model, int numberOfPassengers, double accelerationCoof) 
            : base(numberOfPassengers, accelerationCoof)
        {
            _model = model;
        }

        public override bool TakeOff()
        {
            if (_isFlying) throw new InvalidOperationException("Вертолет уже в полете");

            OnTakingOff(new FlightEventArgs($"{_model}: Начинает разгонять лопасти"));
            OnTakingOff(new FlightEventArgs($"{_model}: Лопасти разогнаны, начинается вертикальный взлет"));
            
            _speed = 50;
            _heightATG = 300;
            OnClimbing(new FlightEventArgs($"{_model}: Вертолет взлетел, высота: {_heightATG} м, скорость: {_speed}"));
            _isFlying = true;
            
            return true;
        }

        public override void Land()
        {
            if (!_isFlying) throw new InvalidOperationException("Вертолет пока не взлетел");

            OnDescending(new FlightEventArgs($"{_model}: Начинает снижение. Текущая высота: {_heightATG} м"));
            _speed = 20;
            _heightATG = 100;
            OnDescending(new FlightEventArgs($"{_model}: Снижение продолжается. Текущая высота: {_heightATG} м, скорость: {_speed}"));

            _speed = 10;
            _heightATG = 50;
            OnLanding(new FlightEventArgs($"{_model}: Высота {_heightATG} м, скорость: {_speed}. Выравнивание"));

            _heightATG = 0;
            _speed = 0;
            OnLanding(new FlightEventArgs($"{_model}: Посадка закончена, торможение лопастей"));
            _isFlying = false;
            OnOnGround(new FlightEventArgs($"{_model}: Вертолет на земле"));
        }

        public override string ToString() => $"Вертолет {_model}. {base.ToString()}";
    }
}