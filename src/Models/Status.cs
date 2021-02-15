using System;
using System.Configuration;

namespace Pomodoro.Models
{
    public class Status : BaseModel
    {
        #region Fields
        private PeriodType _periodType = PeriodType.Studying;
        private bool _isPaused = true;
        private TimeSpan _timeRemaining;
        private uint _cyclesDone = 0;
        #endregion

        #region Properties
        public PeriodType PeriodType
        {
            get => _periodType;
            set { _periodType = value; OnPeriodTypeChanged(); }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set { _isPaused = value; OnPropertyChanged(nameof(IsPaused)); }
        }

        public TimeSpan TimeRemaining
        {
            get => _timeRemaining;
            set { _timeRemaining = value; OnTimeChanged(); }
        }

        public uint CyclesDone
        {
            get => _cyclesDone;
            set { _cyclesDone = value; OnPropertyChanged(nameof(CyclesDone)); }
        }
        #endregion

        #region Helper properties
        public bool IsStudying => PeriodType == PeriodType.Studying;
        public bool IsShortBreak => PeriodType == PeriodType.ShortBreak;
        public bool IsLongBreak => PeriodType == PeriodType.LongBreak;
        public bool IsTakingBreak => !IsStudying;
        public string STimeRemaining => ToTimeString(TimeRemaining);
        public bool IsPeriodComplete => TimeRemaining.TotalSeconds < 0;
        public string CurrentPeriod => IsStudying ? "Studying" : "Taking a break";
        #endregion

        private void OnPeriodTypeChanged()
        {
            OnPropertyChanged(nameof(PeriodType));
            OnPropertyChanged(nameof(IsStudying));
            OnPropertyChanged(nameof(IsShortBreak));
            OnPropertyChanged(nameof(IsLongBreak));
            OnPropertyChanged(nameof(IsTakingBreak));
            OnPropertyChanged(nameof(CurrentPeriod));
        }

        private void OnTimeChanged()
        {
            OnPropertyChanged(nameof(TimeRemaining));
            OnPropertyChanged(nameof(STimeRemaining));
            OnPropertyChanged(nameof(IsPeriodComplete));
        }

        /// <summary>
        /// Decreases the remaining time by one second
        /// </summary>
        public void DecreaseOneSecond() => DecreaseTime(seconds: 1);

        /// <summary>
        /// Decreases the remaining time by the specified amount of time
        /// </summary>
        public void DecreaseTime(int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            TimeRemaining = TimeRemaining.Subtract(new TimeSpan(days: 0, hours: 0, minutes: minutes, seconds: seconds, milliseconds: milliseconds));
        }

        /// <summary>
        /// Generates a nice string representation of the remaining time
        /// </summary>
        /// <param name="timer"></param>
        /// <returns></returns>
        private string ToTimeString(TimeSpan timer)
        {
            // Ensure that timer was already initialized
            if (timer != null)
                return $"{Math.Max(0, timer.Minutes):D2}:{Math.Max(0, timer.Seconds):D2}";
            return "00:00";
        }

        public override string ToString() => $"Action: {PeriodType}, IsPaused: {IsPaused}, TimeRemaining: {TimeRemaining}";
    }
}
