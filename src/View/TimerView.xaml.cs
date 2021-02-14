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
            this.DataContext = PDService.Instance;
        }

        private void Btn_ChangeProcess(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string tag = (string)btn.Tag;
                var ptype =
                    tag == "short" ? PDPeriodType.ShortBreak :
                    tag == "long" ? PDPeriodType.LongBreak :
                    tag == "study" ? PDPeriodType.Studying : PDService.Instance.GetNextPeriodType();
                PDService.Instance.ChangePeriodType(ptype);
            }
        }
    }
}
