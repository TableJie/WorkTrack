using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;
using System.Runtime.ExceptionServices;

namespace WorkTrack
{
    public class AsyncRelayCommand : BaseRelayCommand
    {
        private readonly Func<System.Threading.Tasks.Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<System.Threading.Tasks.Task> execute, Func<bool>? canExecute = null, ILogger? logger = null)
            : base(logger ?? Log.Logger)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            bool result = !_isExecuting && (_canExecute == null || _canExecute());
            _logger.Information("CanExecute called: result={Result}, isExecuting={IsExecuting}", result, _isExecuting);
            return result;
        }

        public override async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                _logger.Warning("Execute called but command cannot be executed.");
                return;
            }

            _isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                _logger.Information("Executing async command.");
                await _execute();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while executing async command.");
                ExceptionDispatchInfo.Capture(ex).Throw();  // 重新拋出異常並保留原始堆疊資訊
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            _logger.Debug("Raising CanExecuteChanged.");
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
