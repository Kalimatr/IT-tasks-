using System;
using System.Threading;

namespace task2
{
    public class AirPlane : AirCraft
    {
        private readonly string _model;
        private readonly double _runwayLength;

        public AirPlane(string model, int numberOfPassengers, double runwayLength, double accelerationCoof) 
            : base(numberOfPassengers, accelerationCoof)
        {
            _model = model;
            _runwayLength = runwayLength;
        }

        public override bool TakeOff()
        {
            if (_isFlying) throw new InvalidOperationException("Самолет уже в полёте");
            if (_runwayLength < 1400) throw new ArgumentException("Взлетно-посадочная полоса слишком короткая");
            
            OnTakingOff(new FlightEventArgs($"{_model}: Начинает разбег по ВПП"));
            
            while (_speed < 250)
            {
                Thread.Sleep(50); 
                _speed += 70 * _accelerationCoof;
                if (_speed > 250) _speed = 250;
                OnTakingOff(new FlightEventArgs($"{_model}: Скорость {_speed} км/ч, разгон продолжается"));
            }
            OnTakingOff(new FlightEventArgs($"{_model}: Скорость {_speed} км/ч, отрыв от земли"));
            _isFlying = true;

            double angle = 0;
            while (_heightATG <= 8000)
            {
                Thread.Sleep(50);
                if (_heightATG < 200) angle = 12;
                else if (_heightATG < 1500) { angle = 22; _speed = Math.Min(_speed + 10 * _accelerationCoof, 350); }
                else if (_heightATG < 7500) { if (angle > 8) angle -= 0.2; _speed = Math.Min(_speed + 100 * _accelerationCoof, 650); }
                else { angle = 3; _speed = Math.Min(_speed + 100 * _accelerationCoof, 950); }
                
                _heightATG += _speed * Math.Sin(angle * Math.PI / 180);
                OnClimbing(new FlightEventArgs($"{_model}: Набирает высоту: {_heightATG:F0} м, скорость: {_speed:F0}"));
            }
            return true;
        }

        public override void Land()
        {
            if (!_isFlying) throw new InvalidOperationException("Самолет пока не взлетел");

            OnDescending(new FlightEventArgs($"{_model}: Начинает снижение. Текущая высота: {_heightATG} м"));
            Thread.Sleep(100); _heightATG = 500; OnDescending(new FlightEventArgs($"{_model}: Снижается до {_heightATG} м"));
            Thread.Sleep(100); _heightATG = 200; _speed *= 0.2; OnDescending(new FlightEventArgs($"{_model}: Высота {_heightATG} м, выпуск шасси"));
            Thread.Sleep(100); _heightATG = 50; OnLanding(new FlightEventArgs($"{_model}: Высота {_heightATG} м, выравнивание"));
            Thread.Sleep(100); _heightATG = 0; _speed *= 0.7; OnLanding(new FlightEventArgs($"{_model}: Скорость снижается..."));
            
            _isFlying = false;  
            _speed = 0;
            OnOnGround(new FlightEventArgs($"{_model}: Посадка завершена"));
        }

        public override string ToString() => $"Самолет {_model} (ВПП: {_runwayLength}м). {base.ToString()}";
    }
}