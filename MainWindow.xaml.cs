using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Pomodoro
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        const string logfile = "studylog.txt";
        const string appname = "pomodoro";
        private string appdata_path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public string Topic => IsStudying ? "Studying" : "Taking a break";

        public bool IsStudying { get; private set; } = true;
        public bool IsShortBreak { get; private set; } = true;

        public bool InProgress { get; set; } = false;
        public bool IsTimerReset { get; set; }
        public bool IsProgressComplete { get; set; }

        public int CyclesDone = 0;

        public int DurationStudying = 40;
        public int DurationShortBreak = 5;
        public int DurationLongBreak = 20;
        public int CyclesUntilLongBreak = 4;

        public string RemainingTime
        {
            get
            {
                return ToTimeString(_remainingTime);
            }
        }
        private TimeSpan _remainingTime;

        private Timer _ticker;


        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void UpdateView(string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public MainWindow()
        {
            InitializeComponent();
            _ticker = new Timer(interval: 1000); // tick every second
            _ticker.Elapsed += OnTimerTick;
            ResetTimer();

            // We provide our own data
            this.DataContext = this;
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            IsTimerReset = false;
            _remainingTime = _remainingTime.Subtract(new TimeSpan(hours: 0, minutes: 0, seconds: 1));

            if (_remainingTime.TotalSeconds <= 0)
            {
                if (!IsProgressComplete)
                {
                    App.Current.Dispatcher.Invoke(
                        () =>
                        {
                            if (IsStudying)
                                ShowNotification("Good job studying - take a break now", NotificationType.Success);
                            else
                                ShowNotification("Break is over, lets study again", NotificationType.Information);
                        });

                    IsProgressComplete = true;
                    if (IsStudying)
                        CyclesDone += 1;
                }
            }

            // notify UI that value has changed
            UpdateView();
        }

        private void Btn_PlayPause(object sender, RoutedEventArgs e)
        {
            InProgress ^= true;
            IsTimerReset = false;
            UpdateView(null);

            if (!InProgress)
                _ticker.Stop();
            else
                _ticker.Start();
            Log();
        }

        private void ResetTimer()
        {
            int duration = IsStudying ? DurationStudying : IsShortBreak ? DurationShortBreak : DurationLongBreak;
            if (!IsStudying && !IsShortBreak)
                CyclesDone = 0; // reset

            _remainingTime = new TimeSpan(hours: 0, minutes: duration, seconds: 0);
            IsTimerReset = true;
            IsProgressComplete = false;
            UpdateView();
        }

        private void Btn_Reset(object sender, RoutedEventArgs e)
        {
            InProgress = false;
            _ticker.Stop();
            ResetTimer();
            Log();
        }

        private string ToTimeString(TimeSpan timer)
        {
            if (timer != null)
                return $"{(timer.TotalSeconds < 0 ? "-" : "")} {Math.Abs(timer.Minutes):D2}:{Math.Abs(timer.Seconds):D2}";
            return "00:00";
        }

        private void Btn_ChangeProcess(object sender, RoutedEventArgs e)
        {
            IsStudying ^= true;

            if (IsProgressComplete)
                IsShortBreak = CyclesDone == CyclesUntilLongBreak;
            else
            {
                if (sender is Button btn)
                {
                    if ((string)btn.Tag == "short")
                        IsShortBreak = true;
                    else if ((string)btn.Tag == "long")
                        IsShortBreak = false;
                }
            }

            Log();
            ResetTimer();
        }

        private void Log()
        {
            var log = $"{System.DateTime.Now} - InProgress: {InProgress}, IsStudying: {IsStudying}";
            var path = System.IO.Path.Combine(appdata_path, appname);

            System.IO.Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, logfile);

            System.IO.File.AppendAllLines(path, new[] { log });
        }

        private void ShowNotification(string message, NotificationType type)
        {
            var n = new Notification(message, type);
            n.Show();
            this.BringIntoView();
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            InProgress = false;
            Log();
        }
    }
}
