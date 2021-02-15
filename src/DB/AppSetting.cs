using System.Windows;

namespace Pomodoro.DB
{
    public class AppSetting
    {
        public double WindowTop;
        public double WindowLeft;
        public double WindowHeight;
        public double WindowWidth;
        public WindowState WindowState;
        public string SelectedProfile;

        public static AppSetting LoadSetting()
        {
            return new AppSetting()
            {
                WindowTop = DBAccess.LoadParameter<double>(nameof(WindowTop)),
                WindowLeft = DBAccess.LoadParameter<double>(nameof(WindowLeft)),
                WindowHeight = DBAccess.LoadParameter<double>(nameof(WindowHeight)),
                WindowWidth = DBAccess.LoadParameter<double>(nameof(WindowWidth)),
                WindowState = (WindowState)DBAccess.LoadParameter<int>(nameof(WindowState)),
                SelectedProfile = DBAccess.LoadParameter<string>(nameof(SelectedProfile))
            };
        }

        public void SaveSetting()
        {
            DBAccess.SaveParameter(nameof(WindowTop), WindowTop);
            DBAccess.SaveParameter(nameof(WindowLeft), WindowLeft);
            DBAccess.SaveParameter(nameof(WindowHeight), WindowHeight);
            DBAccess.SaveParameter(nameof(WindowWidth), WindowWidth);
            DBAccess.SaveParameter(nameof(WindowState), (int)WindowState);
            DBAccess.SaveParameter(nameof(SelectedProfile), SelectedProfile);
        }

        public void TakeOver(Window window)
        {
            WindowTop = window.Top;
            WindowLeft = window.Left;
            WindowHeight = window.Height;
            WindowWidth = window.Width;
            WindowState = window.WindowState;
        }

        public void Apply(Window window)
        {
            window.Top = WindowTop;
            window.Left = WindowLeft;
            
            // Only apply height and width settings when settings aren't default
            // otherwise, just keep it the way the application was designed
            if (WindowHeight != 0)
                window.Height = WindowHeight;
            if (WindowWidth != 0)
                window.Width = WindowWidth;

            // Would not make any sense starting minimized
            if (WindowState != WindowState.Minimized)
                window.WindowState = WindowState;
        }
    }
}
