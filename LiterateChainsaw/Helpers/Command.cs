using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LiterateChainsaw.Helpers
{
    /// <summary>
    /// An always executable command
    /// </summary>
    public class Command : ICommand
    {
        private readonly Action _action;

        /// <summary>
        /// Creates a command that can always execute
        /// </summary>
        /// <param name="action">The method to be executed</param>
        public Command(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
}
