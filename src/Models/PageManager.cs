using System;
using System.Windows;
using Pomodoro.View;

namespace Pomodoro.Models
{
    public class PageManager : BaseModel
    {
        private string _currentViewKey;
        private DependencyObject _currentView;
        private readonly ResourceDictionary _pageDict = new ResourceDictionary();

        private static PageManager _instance;
        /// <summary>
        /// Singleton instance of <see cref="PageManager"/>
        /// </summary>
        public static PageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (new object())
                    {
                        if (_instance == null)
                            _instance = new PageManager();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Indicator whether <see cref="TimerView"/> is currently visible
        /// </summary>
        public bool IsTimerDisplayed => CurrentViewKey == "timer";

        /// <summary>
        /// Indicator whether <see cref="StatsView"/> is currently visible
        /// </summary>
        public bool IsStatsDisplayed => CurrentViewKey == "stats";

        /// <summary>
        /// Indicator whether <see cref="SettingsView"/> is currently visible
        /// </summary>
        public bool IsSettingsDisplayed => CurrentViewKey == "settings";

        /// <summary>
        /// UI Command to change the currently displayed view
        /// </summary>
        public RelayCommand ChangeViewCommand { get; }

        /// <summary>
        /// The key of the view that should currently be displayed
        /// </summary>
        public string CurrentViewKey
        {
            get => _currentViewKey;
            private set { _currentViewKey = value; OnPropertyChanged(nameof(CurrentViewKey)); }
        }

        /// <summary>
        /// The view that should currently be displayed
        /// </summary>
        public DependencyObject CurrentView
        {
            get => _currentView;
            private set { _currentView = value; OnViewChanged(); }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PageManager"/>
        /// </summary>
        public PageManager()
        {
            RegisterPage("stats", new StatsView());
            RegisterPage("timer", new TimerView());
            RegisterPage("settings", new SettingsView());
            ChangeViewCommand = new RelayCommand((p) => SelectPage((string)p));
            SelectPage("timer");
        }

        /// <summary>
        /// Selects a page to be set as <see cref="CurrentView"/>
        /// </summary>
        /// <param name="key">The page identifier</param>
        public void SelectPage(string key)
        {
            if (!_pageDict.Contains(key))
                throw new ArgumentException("No page matches the specified key");
            CurrentViewKey = key;
            CurrentView = (DependencyObject)_pageDict[key];
        }

        /// <summary>
        /// Registers a page in order to be set as <see cref="CurrentView"/>
        /// </summary>
        /// <param name="key">The page identifier</param>
        /// <param name="page">The page itself</param>
        public void RegisterPage(string key, DependencyObject page)
        {
            if (_pageDict.Contains(key))
                throw new ArgumentException("A page with the specified key is already registered");
            _pageDict.Add(key, page);
        }

        /// <summary>
        /// Fires the PropertyChanged event for all properties related to <see cref="CurrentView"/>
        /// </summary>
        private void OnViewChanged()
        {
            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(IsTimerDisplayed));
            OnPropertyChanged(nameof(IsStatsDisplayed));
            OnPropertyChanged(nameof(IsSettingsDisplayed));
        }
    }
}
