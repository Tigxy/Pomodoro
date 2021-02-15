namespace Pomodoro.Models
{
    /// <summary>
    /// Profile containing all user specific settings
    /// </summary>
    public class Profile : BaseModel
    {
        #region Fields
        private string _name = "default";
        private int _durationShortBreak = 5;
        private int _durationLongBreak = 15;
        private int _durationStudying = 40;
        private int _cyclesUntilLongBreak = 4;
        private bool _autoSwitchModeAfterEnd = true;
        #endregion

        //public string Name { get; set; }
        //public int DurationLongBreak { get; set; }
        //public int DurationShortBreak { get; set; }
        //public int DurationStudying { get; set; }
        //public int CyclesUntilLongBreak { get; set; }
        //public bool AutoSwitchModeAfterEnd { get; set; }


        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public int DurationShortBreak
        {
            get => _durationShortBreak;
            set { _durationShortBreak = value; OnPropertyChanged(nameof(DurationShortBreak)); }
        }

        public int DurationLongBreak
        {
            get => _durationLongBreak;
            set { _durationLongBreak = value; OnPropertyChanged(nameof(DurationLongBreak)); }
        }

        public int DurationStudying
        {
            get => _durationStudying;
            set { _durationStudying = value; OnPropertyChanged(nameof(DurationStudying)); }
        }

        public int CyclesUntilLongBreak
        {
            get => _cyclesUntilLongBreak;
            set { _cyclesUntilLongBreak = value; OnPropertyChanged(nameof(CyclesUntilLongBreak)); }
        }

        public bool AutoSwitchModeAfterEnd
        {
            get => _autoSwitchModeAfterEnd;
            set { _autoSwitchModeAfterEnd = value; OnPropertyChanged(nameof(AutoSwitchModeAfterEnd)); }
        }
    }
}