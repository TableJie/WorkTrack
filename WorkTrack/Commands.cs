using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;

namespace WorkTrack
{
    public abstract class BaseRelayCommand : ICommand
    {
        protected readonly ILogger _logger;
        protected bool _isExecuting;

        protected BaseRelayCommand(ILogger? logger)
        {
            _logger = logger ?? Log.Logger;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public abstract bool CanExecute(object? parameter);
        public abstract void Execute(object? parameter);

        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class RelayCommand : BaseRelayCommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null, ILogger? logger = null)
            : base(logger)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            try
            {
                _execute();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while executing command");
                // Instead of rethrowing, we'll just log the error
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }

    public class RelayCommand<T> : BaseRelayCommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null, ILogger? logger = null)
            : base(logger)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke((T?)parameter) ?? true);
        }

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            try
            {
                _execute((T?)parameter);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while executing command");
                // Instead of rethrowing, we'll just log the error
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }

    public class AsyncRelayCommand : BaseRelayCommand
    {
        private readonly Func<System.Threading.Tasks.Task> _execute;
        private readonly Func<bool>? _canExecute;

        public AsyncRelayCommand(Func<System.Threading.Tasks.Task> execute, Func<bool>? canExecute = null, ILogger? logger = null)
            : base(logger)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public override async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            try
            {
                await _execute();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while executing async command");
                // Instead of rethrowing, we'll just log the error
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }

    public class AsyncRelayCommand<T> : BaseRelayCommand
    {
        private readonly Func<T?, System.Threading.Tasks.Task> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public AsyncRelayCommand(Func<T?, System.Threading.Tasks.Task> execute, Func<T?, bool>? canExecute = null, ILogger? logger = null)
            : base(logger)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke((T?)parameter) ?? true);
        }

        public override async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            try
            {
                await _execute((T?)parameter);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while executing async command");
                // Instead of rethrowing, we'll just log the error
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }
}