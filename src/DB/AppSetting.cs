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

        public static AppSetting LoadSetting()
        {
            return new AppSetting()
            {
                WindowTop = DBAccess.LoadSetting<double>(nameof(WindowTop)),
                WindowLeft = DBAccess.LoadSetting<double>(nameof(WindowLeft)),
                WindowHeight = DBAccess.LoadSetting<double>(nameof(WindowHeight)),
                WindowWidth = DBAccess.LoadSetting<double>(nameof(WindowWidth)),
                WindowState = (WindowState)DBAccess.LoadSetting<int>(nameof(WindowState))
            };
        }

        public void SaveSetting()
        {
            DBAccess.SaveSetting(nameof(WindowTop), WindowTop);
            DBAccess.SaveSetting(nameof(WindowLeft), WindowLeft);
            DBAccess.SaveSetting(nameof(WindowHeight), WindowHeight);
            DBAccess.SaveSetting(nameof(WindowWidth), WindowWidth);
            DBAccess.SaveSetting(nameof(WindowState), (int)WindowState);
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
            window.Height = WindowHeight;
            window.Width = WindowWidth;

            // Would not make any sense starting minimized
            if (WindowState != WindowState.Minimized)
                window.WindowState = WindowState;
        }
    }
}
