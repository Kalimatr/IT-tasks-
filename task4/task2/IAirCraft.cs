namespace task2
{
    public interface IAirCraft
    {
        bool TakeOff();
        void Land();
        double HeightATG { get; }
        double Speed { get; }
        bool IsFlying { get; }
    }
}