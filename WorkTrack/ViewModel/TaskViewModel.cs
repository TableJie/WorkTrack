using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dapper;
using Microsoft.Data.Sqlite;
using Serilog;
using static WorkTrack.InputTask;

namespace WorkTrack.ViewModel
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private readonly ILogger _logger;

        public TaskViewModel()
        {
            // 初始化邏輯
        }

        // Properties
        public DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                    LoadTasksCommand.Execute(null);
                }
            }
        }

        public ObservableCollection<Task>? _taskBodyCollection;
        public ObservableCollection<Task>? TaskBodyCollection
        {
            get => _taskBodyCollection;
            set
            {
                if (_taskBodyCollection != value)
                {
                    _taskBodyCollection = value;
                    OnPropertyChanged(nameof(TaskBodyCollection));
                }
            }
        }

        // Commands
        public AsyncRelayCommand LoadTasksCommand { get; }
        public RelayCommand AddTaskCommand { get; }
        public RelayCommand<Task> EditTaskCommand { get; }
        public RelayCommand<Task> CopyTaskCommand { get; }
        public AsyncRelayCommand<Task> ToggleTaskDeleteCommand { get; }

        public TaskViewModel(ILogger? logger = null)
        {
            _logger = logger ?? Log.Logger;
            LoadTasksCommand = new AsyncRelayCommand(LoadTasksAsync);
            AddTaskCommand = new RelayCommand(AddTask);
            EditTaskCommand = new RelayCommand<Task>(EditTask);
            CopyTaskCommand = new RelayCommand<Task>(CopyTask);
            ToggleTaskDeleteCommand = new AsyncRelayCommand<Task>(ToggleTaskDeleteAsync);
            SelectedDate = DateTime.Today;
            _taskBodyCollection = new ObservableCollection<Task>();
        }

        private async System.Threading.Tasks.Task LoadTasksAsync()
        {
            try
            {
                _logger.Information("Loading tasks for date: {Date}", SelectedDate);
                using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                var taskSearch = new TaskSearch();
                if (SelectedDate.HasValue)
                {
                    var taskBodyData = await taskSearch.GetTasks(SelectedDate.Value.Date);
                    TaskBodyCollection = new ObservableCollection<Task>(taskBodyData);
                    _logger.Information("Loaded {Count} tasks", TaskBodyCollection.Count);
                }
                else
                {
                    _logger.Warning("SelectedDate is null. Unable to load tasks.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load tasks");
            }
        }

        private void AddTask()
        {
            try
            {
                _logger.Information("Adding new task");
                var newTask = new Task { TaskDate = SelectedDate ?? DateTime.Today };
                var inputTaskWindow = new InputTask(newTask, TaskInitializationMode.Add);
                if (inputTaskWindow.ShowDialog() == true)
                {
                    TaskBodyCollection?.Add(newTask);
                    _logger.Information("New task added successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while adding new task");
                
            }
        }

        private void EditTask(Task? task)
        {
            if (task == null)
            {
                _logger.Warning("Attempted to edit null task");
                return;
            }

            try
            {
                _logger.Information("Editing task: {TaskId}", task.TaskID);
                var inputTaskWindow = new InputTask(task, TaskInitializationMode.Edit);
                if (inputTaskWindow.ShowDialog() == true)
                {
                    // Refresh the collection to reflect changes
                    var index = TaskBodyCollection?.IndexOf(task) ?? -1;
                    if (index != -1 && TaskBodyCollection != null)
                    {
                        TaskBodyCollection[index] = task;
                    }
                    _logger.Information("Task edited successfully: {TaskId}", task.TaskID);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while editing task: {TaskId}", task.TaskID);
                MessageBox.Show($"操作失敗：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyTask(Task? task)
        {
            if (task == null)
            {
                _logger.Warning("Attempted to copy null task");
                return;
            }

            try
            {
                _logger.Information("Copying task: {TaskId}", task.TaskID);
                var copyTask = new Task
                {
                    TaskName = task.TaskName,
                    Description = task.Description,
                    DurationLevelID = task.DurationLevelID,
                    Duration = task.Duration,
                    UnitID = task.UnitID,
                    ApplicationID = task.ApplicationID,
                    TaskDate = task.TaskDate
                };
                var inputTaskWindow = new InputTask(copyTask, TaskInitializationMode.Copy);
                if (inputTaskWindow.ShowDialog() == true)
                {
                    TaskBodyCollection?.Add(copyTask);
                    _logger.Information("Task copied successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while copying task: {TaskId}", task.TaskID);
                
            }
        }

        private async System.Threading.Tasks.Task ToggleTaskDeleteAsync(Task? task)
        {
            if (task == null)
            {
                _logger.Warning("Attempted to toggle delete on null task");
                return;
            }

            try
            {
                _logger.Information("Toggling delete flag for task: {TaskId}", task.TaskID);
                task.DeleteFlag = !task.DeleteFlag;

                using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                const string query = "UPDATE TaskBody SET DeleteFlag = @DeleteFlag WHERE TaskID = @TaskID";
                await connection.ExecuteAsync(query, new { task.DeleteFlag, task.TaskID });

                // Refresh the collection to reflect changes
                var index = TaskBodyCollection?.IndexOf(task) ?? -1;
                if (index != -1 && TaskBodyCollection != null)
                {
                    TaskBodyCollection[index] = task;
                }

                _logger.Information("Delete flag toggled successfully for task: {TaskId}", task.TaskID);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while toggling delete flag for task: {TaskId}", task.TaskID);
                // Revert the change in the local object
                task.DeleteFlag = !task.DeleteFlag;
                
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}