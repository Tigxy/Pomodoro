using Pomodoro.DB;
using Pomodoro.Models;
using System.Windows.Controls;

namespace Pomodoro.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            this.DataContext = PomodoroService.Instance.Profile;
        }
    }
}
