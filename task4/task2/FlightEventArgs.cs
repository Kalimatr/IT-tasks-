using System;

namespace task2
{
    public class FlightEventArgs : EventArgs
    {
        public string Message { get; set; }
        public DateTime Time { get; set; }

        public FlightEventArgs(string message)
        {
            Message = message;
            Time = DateTime.Now;
        }
    }
}