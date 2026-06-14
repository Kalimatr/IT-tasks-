using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PedestrianCrossing.Events;
using PedestrianCrossing.Models;

namespace PedestrianCrossing.ViewModel
{
    public class SimulationViewModel
    {
        private readonly List<Entity> _entities = new();
        private readonly object _lock = new();
        private readonly Random _random = new();
        private CancellationTokenSource? _cts;
        private Task? _simulationTask;

        public event LightChangedHandler? LightChanged;
        public event AccidentHandler? AccidentOccurred;
        public event Action? StateUpdated;

        public TrafficLightState CurrentLight { get; private set; } = TrafficLightState.GreenForCars;
        public IReadOnlyList<Entity> Entities => _entities.AsReadOnly();

        public SimulationViewModel() { }

        public Entity AddModel(string fullTypeName, double x, double y, double speed)
        {
            var type = Type.GetType(fullTypeName);
            if (type == null) throw new ArgumentException($"Тип {fullTypeName} не найден.");

            var instance = Activator.CreateInstance(type, x, y, speed) as Entity;
            if (instance == null) throw new InvalidOperationException("Ошибка создания объекта.");

            lock (_lock) { _entities.Add(instance); }
            StateUpdated?.Invoke();
            return instance;
        }

        public void StartSimulation()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            _simulationTask = Task.Run(() => SimulationLoop(_cts.Token), _cts.Token);
        }

        public void StopSimulation()
        {
            _cts?.Cancel();
            _simulationTask?.Wait();
            _cts = null;
        }

        private void SimulationLoop(CancellationToken token)
        {
            int lightTickCounter = 0;
            const int ticksPerLightChange = 100; 

            while (!token.IsCancellationRequested)
            {
                lock (_lock)
                {
                    // 1. Логика светофора 
                    lightTickCounter++;
                    if (lightTickCounter >= ticksPerLightChange)
                    {
                        lightTickCounter = 0;
                        CurrentLight = CurrentLight == TrafficLightState.GreenForCars 
                            ? TrafficLightState.GreenForPeds 
                            : TrafficLightState.GreenForCars;
                        LightChanged?.Invoke(this, new LightChangedEventArgs(CurrentLight));
                    }

                    
                    bool isGreenForCars = CurrentLight == TrafficLightState.GreenForCars;
                    foreach (var p in _entities)
                    {
                        if (p is Pedestrian ped) ped.CanCross = !isGreenForCars;
                        if (p is Car car) car.IsStopped = !isGreenForCars; 
                    }

                    // 2. Движение объектов
                    const double canvasW = 800, canvasH = 600;
                    foreach (var e in _entities)
                    {
                        if (e.IsActive)
                        {
                            if (e is Car c) c.Move(canvasW);
                            if (e is Pedestrian ped) ped.Move(canvasH);
                            if (e is EmergencyCar em) em.MoveTowardsTarget();
                        }
                    }

                    // 3. Проверка вероятности аварии
                    var cars = _entities.OfType<Car>().Where(c => c.IsActive && !c.IsStopped);
                    var peds = _entities.OfType<Pedestrian>().Where(p => p.IsActive && p.CanCross);

                    bool accidentHappened = false; 
                    foreach (var car in cars)
                    {
                        foreach (var ped in peds)
                        {
                            
                            if (car.X > 340 && car.X < 460 && ped.Y > 250 && ped.Y < 350)
                            {
                                if (_random.Next(0, 100) < 2) 
                                {
                                    double accX = (car.X + ped.X) / 2;
                                    double accY = (car.Y + ped.Y) / 2;
                                    
                                    car.IsActive = false;
                                    ped.IsActive = false;
                                    
                                    AccidentOccurred?.Invoke(this, new AccidentEventArgs(accX, accY));
                                    accidentHappened = true;
                                    break; 
                                }
                            }
                        }
                        if (accidentHappened) break; 
                    }
                }

                StateUpdated?.Invoke();
                Thread.Sleep(50); 
            }
        }
    }
}