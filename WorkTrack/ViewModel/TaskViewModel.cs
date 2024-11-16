using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using WorkTrack.Interfaces;

namespace WorkTrack.ViewModel
{
    public partial class TaskViewModel : ObservableObject
    {
        private readonly ILogger _logger;
        private readonly ITaskService _taskService;
        private readonly IChartService _chartService;

        [ObservableProperty]
        private DateTime selectedDate;

        [ObservableProperty]
        private ObservableCollection<WorkTask> tasks;

        [ObservableProperty]
        private OverTime? _dailyOverTime;

        [RelayCommand]
        private async Task LoadDailyDataAsync()
        {
            await LoadTasksAsync();
            await LoadOverTimeAsync();
        }

        private async Task LoadOverTimeAsync()
        {
            try
            {
                DailyOverTime = await _taskService.GetOverTimeAsync(SelectedDate);
                _logger.Information("OverTime loaded successfully for date: {SelectedDate}", SelectedDate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while loading overtime");
            }
        }

        [RelayCommand]
        private async Task AddOverTimeAsync()
        {
            try
            {
                _logger.Information("Adding new overtime");
                var newOverTime = new OverTime { TaskDate = SelectedDate };
                var result = await _taskService.AddOverTimeAsync(newOverTime);
                if (result)
                {
                    DailyOverTime = newOverTime;
                    _logger.Information("New overtime added successfully");
                    await _chartService.InitializeChartAsync(SelectedDate);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while adding new overtime");
            }
        }

        [RelayCommand]
        private async Task EditOverTimeAsync()
        {
            if (DailyOverTime == null)
            {
                _logger.Warning("Attempted to edit null overtime");
                return;
            }
            try
            {
                _logger.Information("Editing overtime: {TaskDate}", DailyOverTime.TaskDate);
                var result = await _taskService.EditOverTimeAsync(DailyOverTime);
                if (result)
                {
                    _logger.Information("Overtime edited successfully: {TaskDate}", DailyOverTime.TaskDate);
                    await _chartService.InitializeChartAsync(SelectedDate);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while editing overtime: {TaskDate}", DailyOverTime.TaskDate);
            }
        }

        [RelayCommand]
        private async Task DeleteOverTimeAsync()
        {
            if (DailyOverTime == null)
            {
                _logger.Warning("Attempted to delete null overtime");
                return;
            }
            try
            {
                _logger.Information("Deleting overtime: {TaskDate}", DailyOverTime.TaskDate);
                var result = await _taskService.DeleteOverTimeAsync(DailyOverTime.TaskDate);
                if (result)
                {
                    DailyOverTime = null;
                    _logger.Information("Overtime deleted successfully: {Date}", SelectedDate);
                    await _chartService.InitializeChartAsync(SelectedDate);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while deleting overtime: {Date}", SelectedDate);
            }
        }


        public TaskViewModel(ITaskService taskService, IChartService chartService, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _chartService = chartService ?? throw new ArgumentNullException(nameof(chartService));
            SelectedDate = DateTime.Today;
            Tasks = new ObservableCollection<WorkTask>();
            _ = LoadTasksAsync();
        }



        [RelayCommand]
        private async Task LoadTasksAsync()
        {
            try
            {
                Tasks.Clear();
                var loadedTasks = await _taskService.GetTasksAsync(SelectedDate);
                foreach (var task in loadedTasks)
                {
                    Tasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while loading tasks");
            }
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            try
            {
                _logger.Information("Adding new task");
                var newTask = new WorkTask { TaskDate = SelectedDate };
                var result = await _taskService.AddTaskAsync(newTask);
                if (result)
                {
                    Tasks.Add(newTask);
                    _logger.Information("New task added successfully");
                    await _chartService.InitializeChartAsync(SelectedDate);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while adding new task");
            }
        }

        [RelayCommand]
        private async Task EditTaskAsync(WorkTask task)
        {
            if (task == null)
            {
                _logger.Warning("Attempted to edit null task");
                return;
            }
            try
            {
                _logger.Information("Editing task: {TaskId}", task.TaskID);
                var result = await _taskService.EditTaskAsync(task);
                if (result)
                {
                    var index = Tasks.IndexOf(task);
                    if (index != -1)
                    {
                        Tasks[index] = task;
                    }
                    _logger.Information("Task edited successfully: {TaskId}", task.TaskID);
                    await _chartService.InitializeChartAsync(SelectedDate);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while editing task: {TaskId}", task.TaskID);
            }
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(WorkTask task)
        {
            if (task == null)
            {
                _logger.Warning("Attempted to delete null task");
                return;
            }
            try
            {
                _logger.Information("Deleting task: {TaskId}", task.TaskID);
                var result = await _taskService.DeleteTaskAsync(task.TaskID);
                if (result)
                {
                    Tasks.Remove(task);
                    _logger.Information("Task deleted successfully: {TaskId}", task.TaskID);
                    await _chartService.InitializeChartAsync(SelectedDate);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while deleting task: {TaskId}", task.TaskID);
            }
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            _ = LoadTasksAsync();
        }
    }
}