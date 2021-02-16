using System;
using System.Windows;
using Pomodoro.Models;
using Pomodoro.View;

namespace Pomodoro
{
    public class PageManager : BaseModel
    {
        private string _currentViewKey;
        private int _selectedThemeIndex;
        private DependencyObject _currentView;
        private readonly ResourceDictionary _pageDict = new ResourceDictionary();
        private readonly string[] _themes = new string[] { "../Themes/LightTheme.xaml", "../Themes/DarkTheme.xaml" };

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
        /// Indicator whether application is displayed with dark theme
        /// </summary>
        public bool IsDarkThemeUsed => _selectedThemeIndex == 1;

        /// <summary>
        /// UI Command to change the currently displayed view
        /// </summary>
        public RelayCommand ChangeViewCommand { get; }
        
        /// <summary>
        /// A command that changes the theme of the application
        /// </summary>
        public RelayCommand ChangeThemeCommand { get; }

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
            ChangeThemeCommand = new RelayCommand((p) => ToggleTheme());
            SelectPage("timer");

            _selectedThemeIndex = DBAccess.LoadParameter<int>("selected_theme_index");
            if (_selectedThemeIndex != 0)
                SelectTheme(_selectedThemeIndex);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~PageManager()
        {
            DBAccess.SaveParameter("selected_theme_index", _selectedThemeIndex);
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

            // Not very pretty but we have to make due as plotting library doesn't support bindings
            if (key == "stats")
                StatisticsModel.Instance.RefreshPlot();
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

        /// <summary>
        /// Toggles the theme between dark and light mode
        /// </summary>
        private void ToggleTheme()
        {
            _selectedThemeIndex = (_selectedThemeIndex + 1) % 2;
            SelectTheme(_selectedThemeIndex);
        }

        /// <summary>
        /// Selects the specified theme
        /// </summary>
        /// <param name="themeIndex">The theme to select and display</param>
        private void SelectTheme(int themeIndex)
        {
            _selectedThemeIndex = themeIndex;
            var theme = _themes[themeIndex];
            var resourceDict = new ResourceDictionary
            {
                Source = new Uri(theme, UriKind.Relative)
            };

            if (App.Current.Resources.MergedDictionaries.Count > 0)
                App.Current.Resources.MergedDictionaries.RemoveAt(0);
            App.Current.Resources.MergedDictionaries.Insert(0, resourceDict);
        }
    }
}
