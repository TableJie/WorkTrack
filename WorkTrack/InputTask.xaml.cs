using Dapper;
using LiveCharts.Wpf;
using LiveCharts;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

namespace WorkTrack
{
    /// <summary>
    /// InputTask.xaml 的互動邏輯
    /// </summary>
    public partial class InputTask : Window
    {
        private readonly TaskBody _taskBody;
        private readonly bool _isCopyMode;

        public enum TaskInitializationMode
        {
            Add,    // 用於新增任務
            Edit,   // 用於編輯現有任務
            Copy    // 用於複製現有任務
        }

        public InputTask(TaskBody taskBody, TaskInitializationMode initializationMode)
        {
            InitializeComponent();
            _taskBody = taskBody;
            _isCopyMode = initializationMode == TaskInitializationMode.Copy;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOption();

            // 通用初始化
            ip_TaskDate.SelectedDate = _taskBody.TaskDate != DateTime.MinValue ? _taskBody.TaskDate : DateTime.Today;
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

        private async Task LoadOption()
        {
            try
            {
                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                // 加載 UnitNames 資料
                var unitNames = (await connection.QueryAsync<Unit>("SELECT UnitID, UnitName FROM Unit")).ToList();
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


        private async Task RefreshTaskBodyAsync()
        {
            try
            {
                using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                DateTime? taskDate = ip_TaskDate.SelectedDate;
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

                var insertOrUpdateTaskHeader = $$"""
                   

                    WITH CTE AS (
                        SELECT 
                            sum(CASE WHEN DurationLevelID != 0 THEN DurationLevelID END) as UsedPoints
                            ,sum(CASE WHEN DurationLevelID = 0 THEN Duration END) as CustomizedMins
                        FROM TaskBody
                        WHERE TaskDate = @TaskDate
                    )
                    UPDATE TaskHeader
                    SET
                        UsedPoints = coalesce(CTE.UsedPoints, 0)
                        ,CustomizedMins = coalesce(CTE.CustomizedMins, 0)
                    FROM CTE
                    WHERE TaskHeader.TaskDate = @TaskDate
                    ;

                    UPDATE TaskBody
                    SET Duration = CAST(durationLevelID * (SELECT BasicPoints FROM TaskHeader WHERE TaskDate = @TaskDate) AS INTEGER)
                    WHERE
                        durationLevelID != 0
                        and TaskDate = @TaskDate
                    ;
                """;

                await connection.ExecuteAsync(insertOrUpdateTaskHeader, new{TaskDate = taskDate});
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update task body: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshTaskBodyAsync();

            ip_TaskID.Clear();
            ip_TaskName.Clear();
            ip_Describe.Clear();
            ip_DurationLevelName.SelectedIndex = 2;
            ip_UnitName.SelectedIndex = 0;
            ip_ApplicationID.SelectedIndex = 0;

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                var selectedDate = ip_TaskDate.SelectedDate ?? DateTime.Now;
                await mainWindow.InitializeStackedColumnChart(selectedDate);


                if (mainWindow.MainFrame.Content is Page1_Task page1Task)
                {
                    await page1Task.DefaultSearch_TaskBody(); // 更新 Page1_Task 的 DataGrid
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 關閉當前視窗
        }

    }


}
