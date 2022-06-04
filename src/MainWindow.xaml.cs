using System;
using System.ComponentModel;
using System.Windows;

using Pomodoro.DB;
using Pomodoro.Models;

namespace Pomodoro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = PageManager.Instance;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var a = AppSetting.LoadSetting();
            a.ApplyTo(this);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            PomodoroService.Instance.Stop();

            var appS = new AppSetting();
            appS.TakeOverFrom(this);
            appS.SaveSetting();

            DBAccess.SaveProfile(PomodoroService.Instance.Profile);

            // TODO: Remove once fixed in library
            // Bug fix as the notification library currently does not close its own window
            // but only hides it. As it is not running in a background thread, it will keep 
            // running even after the main window was closed.
            foreach (var wd in App.Current.Windows)
                if (wd is Notification.Wpf.NotificationsOverlayWindow nwd)
                    nwd.Close();
        }
    }
}
