using System;
using System.Windows.Input;

namespace Pomodoro.Models
{
    /// <summary>
    /// A command that relays commands by calling the specified functions
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged;
        public static Predicate<object> CanAlwaysExecute = (p) => true;

        public virtual void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class
        /// </summary>
        /// <param name="action">The action that should be executed when the command is</param>
        public RelayCommand(Action<object> action): this(action, CanAlwaysExecute) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class
        /// </summary>
        /// <param name="action">The action that should be executed when the command is</param>
        /// <param name="canExecute">Determines whether command may be executed</param>
        public RelayCommand(Action<object> action, Predicate<object> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether command may currently be executed
        /// </summary>
        /// <param name="parameter">An additional parameter for the predicate</param>
        /// <returns>True in case the command may currently be executed</returns>
        public bool CanExecute(object parameter) => _canExecute(parameter);

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">An additional parameter for the command</param>
        public void Execute(object parameter) => _action(parameter);
    }
}
