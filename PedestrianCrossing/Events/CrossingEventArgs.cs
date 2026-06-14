using System;

namespace PedestrianCrossing.Events
{
    public enum TrafficLightState { GreenForCars, GreenForPeds }

    public class LightChangedEventArgs : EventArgs
    {
        public TrafficLightState CurrentState { get; }
        public LightChangedEventArgs(TrafficLightState state) => CurrentState = state;
    }

    public class AccidentEventArgs : EventArgs
    {
        public double X { get; }
        public double Y { get; }
        public AccidentEventArgs(double x, double y) { X = x; Y = y; }
    }

    public delegate void LightChangedHandler(object sender, LightChangedEventArgs e);
    public delegate void AccidentHandler(object sender, AccidentEventArgs e);
}