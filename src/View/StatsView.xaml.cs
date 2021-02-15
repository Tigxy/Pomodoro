using Pomodoro.Models;
using System.Windows.Controls;

namespace Pomodoro.View
{
    /// <summary>
    /// Interaction logic for StatsView.xaml
    /// </summary>
    public partial class StatsView : UserControl
    {
        public StatsView()
        {
            InitializeComponent();
            this.DataContext = StatsModel.Instance;
        }
    }
}
