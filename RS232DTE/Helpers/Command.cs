using System;
using System.Windows.Input;

namespace RS232DTE.Helpers
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);

        public void Execute(object parameter) => execute(parameter);

        public Command(Action<object> execute, Func<object, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
    }
}
