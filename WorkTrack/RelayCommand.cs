using System;
using System.Runtime.ExceptionServices;
using System.Windows.Input;
using Serilog;

namespace WorkTrack
{
    public abstract class BaseRelayCommand : ICommand
    {
        protected readonly ILogger _logger;

        protected BaseRelayCommand(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                _logger.Debug("CanExecuteChanged event subscribed");
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                _logger.Debug("CanExecuteChanged event unsubscribed");
            }
        }

        public abstract bool CanExecute(object? parameter);
        public abstract void Execute(object? parameter);
    }

    public class RelayCommand : BaseRelayCommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null, ILogger? logger = null)
            : base(logger ?? Log.Logger)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            bool result = _canExecute == null || _canExecute();
            _logger.Information("CanExecute called: result={Result}", result);
            return result;
        }

        public override void Execute(object? parameter)
        {
            try
            {
                _logger.Information("Executing command with no parameter");
                _execute();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while executing command with no parameter");
                ExceptionDispatchInfo.Capture(ex).Throw(); // 使用 ExceptionDispatchInfo 重新拋出異常
            }
        }
    }

    public class RelayCommand<T> : BaseRelayCommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null, ILogger? logger = null)
            : base(logger ?? Log.Logger)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
            {
                _logger.Warning("CanExecute called with null parameter for value type {Type}", typeof(T).Name);
                return false;
            }

            bool result = _canExecute == null || _canExecute((T)parameter!);
            _logger.Information("CanExecute called: result={Result}, parameter={Parameter}", result, parameter);
            return result;
        }

        public override void Execute(object? parameter)
        {
            try
            {
                if (parameter == null && typeof(T).IsValueType)
                {
                    _logger.Error("Attempted to execute command with null parameter for value type {Type}", typeof(T).Name);
                    throw new InvalidOperationException($"The command parameter cannot be null because the expected type is a value type '{typeof(T).Name}'.");
                }

                _logger.Information("Executing command with parameter of type {Type}: {Parameter}", typeof(T).Name, parameter);
                _execute((T)parameter!);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while executing command with parameter of type {Type}", typeof(T).Name);
                ExceptionDispatchInfo.Capture(ex).Throw(); // 使用 ExceptionDispatchInfo 重新拋出異常
            }
        }
    }
}
