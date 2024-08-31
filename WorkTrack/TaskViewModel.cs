using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dapper;
using Microsoft.Data.Sqlite;
using static WorkTrack.InputTask;

namespace WorkTrack
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        // Properties
        private DateTime? _selectedDate;
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

        private ObservableCollection<Task> _taskBodyCollection;
        public ObservableCollection<Task> TaskBodyCollection
        {
            get => _taskBodyCollection;
            set
            {
                _taskBodyCollection = value;
                OnPropertyChanged(nameof(TaskBodyCollection));
            }
        }

        // Commands
        public ICommand LoadTasksCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand CopyTaskCommand { get; }
        public ICommand ToggleTaskDeleteCommand { get; }

        public TaskViewModel()
        {
            LoadTasksCommand = new AsyncRelayCommand(DefaultSearch_TaskBody);
            AddTaskCommand = new RelayCommand(AddTask);
            EditTaskCommand = new RelayCommand<Task>(EditTask);
            CopyTaskCommand = new RelayCommand<Task>(CopyTask);
            ToggleTaskDeleteCommand = new RelayCommand<Task>(ToggleTaskDelete);
            SelectedDate = DateTime.Today;
        }

        private async System.Threading.Tasks.Task DefaultSearch_TaskBody()
        {
            try
            {
                using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                var taskSearch = new TaskSearch();
                var taskBodyData = await taskSearch.GetTasks(SelectedDate.Value.Date);
                TaskBodyCollection = new ObservableCollection<Task>(taskBodyData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load TaskBody: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTask()
        {
            var inputTaskWindow = new InputTask(new Task { TaskDate = SelectedDate ?? DateTime.Today }, TaskInitializationMode.Add);
            inputTaskWindow.ShowDialog();
        }

        private void EditTask(Task task)
        {
            var inputTaskWindow = new InputTask(task, TaskInitializationMode.Edit);
            inputTaskWindow.ShowDialog();
        }

        private void CopyTask(Task task)
        {
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
            inputTaskWindow.ShowDialog();
        }

        private async void ToggleTaskDelete(Task task)
        {
            task.DeleteFlag = !task.DeleteFlag;
            try
            {
                using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                string query = "UPDATE TaskBody SET DeleteFlag = @DeleteFlag WHERE TaskID = @TaskID";
                await connection.ExecuteAsync(query, new { task.DeleteFlag, task.TaskID });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
