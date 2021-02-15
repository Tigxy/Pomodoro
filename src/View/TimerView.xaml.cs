using Pomodoro.Models;
using System.Windows;
using System.Windows.Controls;

namespace Pomodoro.View
{
    /// <summary>
    /// Interaction logic for TimerView.xaml
    /// </summary>
    public partial class TimerView : UserControl
    {
        public TimerView()
        {
            InitializeComponent();
            this.DataContext = PomodoroService.Instance;
        }

        private void Btn_ChangeProcess(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string tag = (string)btn.Tag;
                var ptype =
                    tag == "short" ? PeriodType.ShortBreak :
                    tag == "long" ? PeriodType.LongBreak :
                    tag == "study" ? PeriodType.Studying : PomodoroService.Instance.GetNextPeriodType();
                PomodoroService.Instance.ChangePeriodType(ptype);
            }
        }
    }
}
