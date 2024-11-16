using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WorkTrack.Interfaces;
using WorkTrack.Services;
using WorkTrack; // 假設 WorkTask 和 Unit 定義在這個命名空間
using Serilog; // 用於 ILogger

namespace WorkTrack
{
    public partial class TaskInput : Window
    {
        private readonly WorkTask _taskBody;
        private readonly bool _isCopyMode;
        private readonly ILogger _logger;
        private readonly ITaskService _taskService;

        public enum TaskInitializationMode
        {
            Add,
            Edit,
            Copy
        }

        public TaskInput(WorkTask taskBody, TaskInitializationMode initializationMode, ILogger logger, ITaskService taskService)
        {
            InitializeComponent();
            _taskBody = taskBody;
            _isCopyMode = initializationMode == TaskInitializationMode.Copy;
            _logger = logger;
            _taskService = taskService;

            Loaded += MainWindow_Loaded;
            ip_TaskName.Focus();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadOptionAsync();
                InitializeFormData();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "加載任務輸入窗口時發生錯誤");
                MessageBox.Show($"加載選項失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadOptionAsync()
        {
            var unitNames = await _taskService.GetUnitNamesAsync();
            unitNames.Add(new Unit { UnitID = 0, UnitName = "-Add-" });
            ip_UnitName.ItemsSource = unitNames;
            ip_UnitName.SelectedIndex = 0;

            var durationLevelNames = await _taskService.GetDurationLevelsAsync();
            ip_DurationLevelName.ItemsSource = durationLevelNames;
            ip_DurationLevelName.SelectedIndex = 2;
        }

        private void InitializeFormData()
        {
            ip_TaskDate.SelectedDate = _taskBody.TaskDate != default ? _taskBody.TaskDate : DateTime.Today;
            ip_TaskName.Text = _taskBody.TaskName;
            ip_Describe.Text = _taskBody.Description;
            ip_DurationLevelName.SelectedValue = _taskBody.DurationLevelID != 0 ? _taskBody.DurationLevelID : ip_DurationLevelName.Items[2];
            ip_Duration.Text = _taskBody.Duration.ToString();
            ip_UnitName.SelectedValue = _taskBody.UnitID != 0 ? _taskBody.UnitID : ip_UnitName.Items[0];
            ip_ApplicationID.Text = _taskBody.ApplicationID?.ToString();
            this.Title = _isCopyMode ? "Copy Task" : _taskBody.TaskID == 0 ? "Add Task" : "Change Task";
            ip_TaskID.Visibility = _taskBody.TaskID == 0 || _isCopyMode ? Visibility.Collapsed : Visibility.Visible;
            if (!_isCopyMode && _taskBody.TaskID != 0)
            {
                ip_TaskID.Text = _taskBody.TaskID.ToString();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                var taskData = GetTaskData();
                await _taskService.SaveTaskAsync(taskData);
                UpdateService.NotifyDataUpdated(taskData.TaskDate);
                ClearForm();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "保存任務時發生錯誤");
                MessageBox.Show($"保存失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ip_TaskName.Text))
            {
                MessageBox.Show("請輸入任務名稱", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!ip_TaskDate.SelectedDate.HasValue)
            {
                MessageBox.Show("請選擇任務日期", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private WorkTask GetTaskData()
        {
            return new WorkTask
            {
                TaskID = string.IsNullOrEmpty(ip_TaskID.Text) ? 0 : int.Parse(ip_TaskID.Text),
                TaskDate = ip_TaskDate.SelectedDate ?? DateTime.Today,
                TaskName = ip_TaskName.Text,
                Description = ip_Describe.Text,
                DurationLevelID = (int)ip_DurationLevelName.SelectedValue,
                Duration = string.IsNullOrEmpty(ip_Duration.Text) ? 0 : double.Parse(ip_Duration.Text),
                UnitID = (int)ip_UnitName.SelectedValue,
                ApplicationID = ip_ApplicationID.Text
            };
        }

        private void ClearForm()
        {
            ip_TaskID.Clear();
            ip_TaskName.Clear();
            ip_Describe.Clear();
            ip_DurationLevelName.SelectedIndex = 2;  // 保持不變
            ip_UnitName.SelectedIndex = 0;  // 保持不變
            ip_Duration.Clear();  // 清除 Duration 文本框
            ip_TaskDate.SelectedDate = DateTime.Today;
        }

        private void ip_DurationLevelName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ip_DurationLevelName.SelectedIndex == 5)
            {
                ip_Duration.Visibility = Visibility.Visible;
                ip_DurationLevelName.Width = 110;
                ip_Duration.Focus();
                ip_Duration.SelectAll();
            }
            else
            {
                ip_Duration.Visibility = Visibility.Collapsed;
                ip_DurationLevelName.Width = 180;
            }
        }
    }
}