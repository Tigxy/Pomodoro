#define DEBUG_SHORT_TIME
using System;

namespace Pomodoro
{
    public class PDConfig
    {
#if DEBUG_SHORT_TIME
        public int DurationStudying { get; set; } = 10;
        public int DurationShortBreak { get; set; } = 5;
        public int DurationLongBreak { get; set; } = 7;
        public int CyclesUntilLongBreak { get; set; } = 2;

#else
        public int DurationStudying { get; set; } = 40;
        public int DurationShortBreak { get; set; } = 5;
        public int DurationLongBreak { get; set; } = 20;
        public int CyclesUntilLongBreak { get; set; } = 4;
#endif
        public bool AutoSwitchModeAfterEnd { get; set; } = true;


        public TimeSpan GetPeriodTimespan(PDPeriodType periodType)
        {
            int duration = periodType == PDPeriodType.Studying
                ? DurationStudying
                : periodType == PDPeriodType.ShortBreak
                ? DurationShortBreak : DurationLongBreak;

#if DEBUG_SHORT_TIME
            return TimeSpan.FromSeconds(duration);
#else
            return TimeSpan.FromMinutes(duration);
#endif
        }

    }
}
