using System;
using System.Windows.Input;

namespace Pomodoro
{
    /// <summary>
    /// A command that relays the command call to predefined functions
    /// </summary>
    public class RelayCommand : ICommand
    {
        // Predicate that always returns true
        public static Predicate<object> CanAlwaysExecute = (p) => true;
        
        private readonly Action<object> _action;
        private readonly Predicate<object> _canExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> action, Predicate<object> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute(parameter);

        public void Execute(object parameter) => _action(parameter);
    }
}
