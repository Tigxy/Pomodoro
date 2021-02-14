using System;

namespace Pomodoro.DB
{
    public class PeriodEntry
    {
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsStudying { get; set; }
        public bool IsPaused { get; set; }
    }
}