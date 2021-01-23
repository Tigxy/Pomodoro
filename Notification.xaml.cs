using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pomodoro
{
    /// <summary>
    /// Interaktionslogik für Notification.xaml
    /// </summary>
    public partial class Notification : Window
    {
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
