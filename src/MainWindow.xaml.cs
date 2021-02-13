

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
    public partial class MainWindow : Window
    {
        const string logfile = "studylog.txt";
        const string appname = "pomodoro";
        private readonly string appdata_path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public readonly PDService PomodoroService;

        public MainWindow()
        {
            InitializeComponent();
            PomodoroService = new PDService();
                        
            // We provide our own data
            this.DataContext = PomodoroService;
        }

        private void Btn_PlayPause(object sender, RoutedEventArgs e)
        {
            PomodoroService.ToggleStartPause();
        }

        private void Btn_Reset(object sender, RoutedEventArgs e)
        {
            PomodoroService.Reset();
        }

        private void Btn_ChangeProcess(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string tag = (string)btn.Tag;
                var ptype =
                    tag == "short" ? PDPeriodType.ShortBreak :
                    tag == "long" ? PDPeriodType.LongBreak : 
                    tag == "study" ? PDPeriodType.Studying : PomodoroService.GetNextPeriodType();
                PomodoroService.ChangePeriod(ptype);
            }
        }

        //private void Log()
        //{
        //    var log = $"{System.DateTime.UtcNow} - InProgress: {InProgress}, IsStudying: {IsStudying}";
        //    var path = System.IO.Path.Combine(appdata_path, appname);

        //    System.IO.Directory.CreateDirectory(path);
        //    path = System.IO.Path.Combine(path, logfile);

        //    System.IO.File.AppendAllLines(path, new[] { log });
        //}

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            PomodoroService.Stop();
        }
    }
}
