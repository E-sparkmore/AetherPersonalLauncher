using System;
using System.Windows.Input;

namespace AetherPersonalLauncher.Utils
{
    public class RelayCommand: ICommand
    {
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _action = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        private readonly Action _action;
        private readonly Func<bool> _canExecute;
    }
    
    public class RelayCommand<T>: ICommand
    {
        public RelayCommand(Action<T> execute, Func<bool> canExecute = null)
        {
            _action = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _action((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        private readonly Action<T> _action;
        private readonly Func<bool> _canExecute;
    }
}