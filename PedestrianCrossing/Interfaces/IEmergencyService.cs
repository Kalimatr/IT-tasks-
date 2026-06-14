namespace PedestrianCrossing.Interfaces
{
    public interface IEmergencyService
    {
        void Dispatch(double targetX, double targetY);
        bool IsOnMission { get; }
    }
}