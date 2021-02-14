using System;
using System.ComponentModel;

namespace Pomodoro.DB
{
    /// <summary>
    /// Profile containing all user specific settings
    /// </summary>
    public class Profile : INotifyPropertyChanged
    {
        #region Fields
        private string _name = "default";
        private int _durationLongBreak = 15;
        private int _durationShortBreak = 5;
        private int _durationStudying = 40;
        private int _cyclesUntilLongBreak = 4;
        private bool _autoSwitchModeAfterEnd = true;
        #endregion

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(_name)); }
        }

        public int DurationLongBreak
        {
            get => _durationLongBreak;
            set 
            { 
                // Ensure that value is valid
                if (value is int v)
                    _durationLongBreak = v; 
                OnPropertyChanged(nameof(_durationLongBreak)); 
            }
        }

        public int DurationShortBreak
        {
            get => _durationShortBreak;
            set { _durationShortBreak = value; OnPropertyChanged(nameof(_durationShortBreak)); }
        }

        public int DurationStudying
        {
            get => _durationStudying;
            set { _durationStudying = value; OnPropertyChanged(nameof(_durationStudying)); }
        }

        public int CyclesUntilLongBreak
        {
            get => _cyclesUntilLongBreak;
            set { _cyclesUntilLongBreak = value; OnPropertyChanged(nameof(_cyclesUntilLongBreak)); }
        }

        public bool AutoSwitchModeAfterEnd
        {
            get => _autoSwitchModeAfterEnd;
            set { _autoSwitchModeAfterEnd = value; OnPropertyChanged(nameof(_autoSwitchModeAfterEnd)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}