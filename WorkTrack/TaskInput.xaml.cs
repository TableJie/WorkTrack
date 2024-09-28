using Dapper;
using Microsoft.Data.Sqlite;
using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace WorkTrack
{
    /// <summary>
    /// TaskInput.xaml 的互動邏輯
    /// </summary>
    public partial class TaskInput : Window
    {
        private readonly Task _taskBody;
        private readonly bool _isCopyMode;

        public enum TaskInitializationMode
        {
            Add,    // 用於新增任務
            Edit,   // 用於編輯現有任務
            Copy    // 用於複製現有任務
        }

        public TaskInput(Task taskBody, TaskInitializationMode initializationMode)
        {
            InitializeComponent();
            _taskBody = taskBody;
            _isCopyMode = initializationMode == TaskInitializationMode.Copy;

            Loaded += MainWindow_Loaded;
            ip_TaskName.Focus();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOption();

            // 通用初始化
            ip_TaskDate.SelectedDate = _taskBody.TaskDate != default ? _taskBody.TaskDate : DateTime.Today;
            ip_TaskName.Text = _taskBody.TaskName;
            ip_Describe.Text = _taskBody.Description;
            ip_DurationLevelName.SelectedValue = _taskBody.DurationLevelID != 0 ? _taskBody.DurationLevelID : ip_DurationLevelName.Items[2];
            ip_Duration.Text = _taskBody.Duration.ToString();
            ip_UnitName.SelectedValue = _taskBody.UnitID != 0 ? _taskBody.UnitID : ip_UnitName.Items[0];
            ip_ApplicationID.Text = _taskBody.ApplicationID?.ToString();

            // 根據模式設定視窗標題和 TaskID 控件可見性
            this.Title = _isCopyMode ? "Copy Task" : _taskBody.TaskID == 0 ? "Add Task" : "Change Task";
            ip_TaskID.Visibility = _taskBody.TaskID == 0 || _isCopyMode ? Visibility.Collapsed : Visibility.Visible;
            if (!_isCopyMode && _taskBody.TaskID != 0)
            {
                ip_TaskID.Text = _taskBody.TaskID.ToString();
            }
        }

        private async System.Threading.Tasks.Task LoadOption()
        {
            try
            {
                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                // 加載 UnitNames 資料
                var unitNames = (await connection.QueryAsync<Unit>("SELECT UnitID, UnitName FROM Unit WHERE DeleteFlag = @DeleteFlag", new { DeleteFlag = false })).ToList();
                unitNames.Add(new Unit { UnitID = 0, UnitName = "-Add-" });
                ip_UnitName.ItemsSource = unitNames;
                ip_UnitName.SelectedIndex = 0;

                // 加載 DurationLevels 資料
                var durationLevelNames = (await connection.QueryAsync<DurationLevel>("SELECT DurationLevelID, DurationLevelName FROM DurationLevel")).ToList();
                ip_DurationLevelName.ItemsSource = durationLevelNames;
                ip_DurationLevelName.SelectedIndex = 2;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加載選項時發生錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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
        private void ip_UnitName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ip_UnitName.SelectedItem != null && ip_UnitName.SelectedItem.ToString() == "-Add-")
            {
                UnitManagement unitManagementWindow = new UnitManagement();
                unitManagementWindow.Closed += UnitManagementWindow_Closed;
                unitManagementWindow.ShowDialog();
            }
        }
        private async void UnitManagementWindow_Closed(object sender, EventArgs e)
        {
            await LoadOption(); // 更新主視窗的UnitName選項
        }


        private async System.Threading.Tasks.Task RefreshTaskBodyAsync()
        {
            try
            {
                using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                string taskDate = (ip_TaskDate.SelectedDate ?? DateTime.Today).ToString("yyyy-MM-dd");
                string taskID = ip_TaskID.Text;
                string taskName = ip_TaskName.Text;
                string description = ip_Describe.Text;
                int durationLevelID = (int)ip_DurationLevelName.SelectedValue;
                int? duration = string.IsNullOrEmpty(ip_Duration.Text) ? (int?)null : int.Parse(ip_Duration.Text);
                int selectedUnitID = (int)ip_UnitName.SelectedValue;
                string applicationID = ip_ApplicationID.Text;


                if (string.IsNullOrEmpty(taskID))
                {
                    var insertQuery = $$"""
                        INSERT INTO TaskBody (TaskDate, TaskName, DurationLevelID, Duration, Description, UnitID, ApplicationID)
                        VALUES (@TaskDate, @TaskName, @DurationLevelID, @Duration, @Description, @UnitID, @ApplicationID);
                    """;

                    await connection.ExecuteAsync(insertQuery, new
                    {
                        TaskDate = taskDate,
                        TaskName = taskName,
                        DurationLevelID = durationLevelID,
                        Duration = duration,
                        Description = description,
                        UnitID = selectedUnitID,
                        ApplicationID = applicationID,
                    });
                }
                else
                {
                    var updateQuery = $$"""
                        UPDATE TaskBody
                        SET TaskName = @TaskName, DurationLevelID = @DurationLevelID, Duration = @Duration, Description = @Description,
                            UnitID = @UnitID, ApplicationID = @ApplicationID
                        WHERE TaskID = @TaskID;
                    """;

                    await connection.ExecuteAsync(updateQuery, new
                    {
                        TaskName = taskName,
                        DurationLevelID = durationLevelID,
                        Duration = duration,
                        Description = description,
                        UnitID = selectedUnitID,
                        ApplicationID = applicationID,
                        TaskID = taskID
                    });
                }

                UpdateService.NotifyDataUpdated((ip_TaskDate.SelectedDate ?? DateTime.Today));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update task body: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try {
                if (string.IsNullOrWhiteSpace(ip_TaskName.Text))
                {
                    MessageBox.Show("Input TaskName", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ip_TaskDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Input TaskDate", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await RefreshTaskBodyAsync();

                ip_TaskID.Clear();
                ip_TaskName.Clear();
                ip_Describe.Clear();
                ip_DurationLevelName.SelectedIndex = 2;
                ip_UnitName.SelectedIndex = 0;
                ip_ApplicationID.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新任務失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                // 假設您使用 Serilog 或類似的日誌系統
                Log.Error(ex, "更新任務時發生錯誤");
            }

        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 關閉當前視窗
        }

    }


}
