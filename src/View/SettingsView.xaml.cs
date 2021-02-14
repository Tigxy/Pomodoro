using Pomodoro.DB;
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
            this.DataContext = PDService.Instance.Profile;
        }
    }
}
