using Pomodoro.DB;
using Pomodoro.View;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Pomodoro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _isSettingsVisible;
        private UserControl _currentView;

        /// <summary>
        /// Indicator whether settings are currently visible
        /// </summary>
        public bool IsSettingsVisible
        {
            get => _isSettingsVisible;
            set { _isSettingsVisible = value; OnPropertyChanged(nameof(IsSettingsVisible)); }
        }

        /// <summary>
        /// The view that is currently displayed
        /// </summary>
        public UserControl CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        private SettingsView _settingsView;
        /// <summary>
        /// Lazy loaded settings view
        /// </summary>
        public SettingsView SettingsView
        {
            get
            {
                if (_settingsView == null)
                {
                    lock (this)
                    {
                        if (_settingsView == null)
                            _settingsView = new SettingsView();
                    }
                }
                return _settingsView;
            }
        }

        public TimerView TimerView = new TimerView();

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));


        public MainWindow()
        {
            InitializeComponent();

            // In case we ever want to offer more than one profile, most functionality is already prepared
            PDService.Instance.ChangeProfile(DBAccess.GetProfile("default") ?? new Profile());
            TimerView = new TimerView();
            CurrentView = TimerView;
            this.DataContext = this;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var a = AppSetting.LoadSetting();
            a.Apply(this);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            PDService.Instance.Stop();

            var appS = new AppSetting();
            appS.TakeOver(this);
            appS.SaveSetting();

            DBAccess.SaveProfile(PDService.Instance.Profile);

            // TODO: Remove once fixed in library
            // Bug fix as the notification library currently does not close its own window
            // but only hides it. As it is not running in a background thread, it will keep 
            // running even after the main window was closed.
            foreach (var wd in App.Current.Windows)
                if (wd is Notification.Wpf.NotificationsOverlayWindow nwd)
                    nwd.Close();
        }

        private void ToggleSettingsView(object sender, RoutedEventArgs e)
        {
            IsSettingsVisible ^= true;
            CurrentView = IsSettingsVisible ? (UserControl)SettingsView : (UserControl)TimerView;
        }
    }
}
