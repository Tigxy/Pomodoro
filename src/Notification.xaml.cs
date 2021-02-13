using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Pomodoro
{
    /// <summary>
    /// Interaktionslogik für Notification.xaml
    /// </summary>
    public partial class Notification : Window
    {
        private const int _timeUntilClose = 5; // in seconds
        Timer _timer;

        public Notification(string message, NotificationType notificationType)
        {
            InitializeComponent();

            Brush background;

            switch (notificationType)
            {
                case NotificationType.Information:
                    background = Brushes.LightBlue;
                    break;
                case NotificationType.Success:
                    background = Brushes.LightGreen;
                    break;
                default:
                    background = Brushes.Transparent;
                    break;
            }
            this.Background = background;

            msg.Text = message;

            // We want to close this notification pop up after a certain amount of time
            _timer = new Timer(_timeUntilClose * 1000); // to ms
            _timer.Start();
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workarea = System.Windows.SystemParameters.WorkArea;
            this.Left = workarea.Right - this.Width;
            this.Top = workarea.Bottom - this.Height;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
