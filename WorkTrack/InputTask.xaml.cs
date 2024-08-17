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

namespace WorkTrack
{
    /// <summary>
    /// InputTask.xaml 的互動邏輯
    /// </summary>
    public partial class InputTask : Window
    {
        private DateTime? _taskDate;
        public InputTask(DateTime? taskDate = null)
        {
            InitializeComponent();
            _taskDate = taskDate;

            // 訂閱 Loaded 事件
            Loaded += MainWindow_Loaded;

        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            ip_TaskDate.SelectedDate = _taskDate.Value;
            await LoadUnitNames();
            await LoadDurationLevel();

        }
        private async Task LoadUnitNames()
        {
            try
            {
                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                var unitNames = (await connection.QueryAsync<Unit>("SELECT UnitID, UnitName FROM Unit")).ToList();
                unitNames.Add(new Unit { UnitID = 0, UnitName = "-Add-" });

                ip_UnitName.Items.Clear(); // 確保在設定 ItemsSource 前，清空現有項目
                ip_UnitName.ItemsSource = unitNames;

                if (unitNames.Count > 0)
                {
                    ip_UnitName.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load unit names: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDurationLevel()
        {
            try
            {
                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                var durationNames = (await connection.QueryAsync<DurationLevel>("SELECT DurationLevelName, Points FROM DurationLevel")).ToList();

                ip_DurationLevel.Items.Clear();
                ip_DurationLevel.ItemsSource = durationNames;

                if (durationNames.Count > 0)
                {
                    ip_DurationLevel.SelectedIndex = 2;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load unit names: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            await LoadUnitNames(); // 更新主視窗的UnitName選項
        }
        private void ip_DurationLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ip_DurationLevel.SelectedItem is ComboBoxItem selectedItem)
            {
                ToggleDurationVisibility(selectedItem.Content.ToString());
            }
        }
        private void ToggleDurationVisibility(string durationLevel)
        {
            if (durationLevel == "-Customize-")
            {
                ip_Duration.Visibility = Visibility.Visible;
            }
            else
            {
                ip_Duration.Visibility = Visibility.Collapsed;
            }
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
                int durationLevel = (int)ip_DurationLevel.SelectedValue;
                int? duration = string.IsNullOrEmpty(ip_Duration.Text) ? (int?)null : int.Parse(ip_Duration.Text);
                int selectedUnitID = (int)ip_UnitName.SelectedValue;
                string applicationID = ip_ApplicationID.Text;


                if (string.IsNullOrEmpty(taskID))
                {
                    var insertQuery = $$"""
                        INSERT INTO TaskBody (TaskDate, TaskName, DurationLevel, Duration, Description, UnitID, ApplicationID)
                        VALUES (@TaskDate, @TaskName, @DurationLevel, @Duration, @Description, @UnitID, @ApplicationID);
                    """;

                    await connection.ExecuteAsync(insertQuery, new
                    {
                        TaskDate = taskDate,
                        TaskName = taskName,
                        DurationLevel = durationLevel,
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
                        SET TaskName = @TaskName, DurationLevel = @DurationLevel, Duration = @Duration, Description = @Description,
                            UnitID = @UnitID, ApplicationID = @ApplicationID
                        WHERE TaskID = @TaskID;
                    """;

                    await connection.ExecuteAsync(updateQuery, new
                    {
                        TaskName = taskName,
                        DurationLevel = durationLevel,
                        Duration = duration,
                        Description = description,
                        UnitID = selectedUnitID,
                        ApplicationID = applicationID,
                        TaskID = taskID
                    });
                }

                var insertOrUpdateTaskHeader = $$"""
                    INSERT OR IGNORE INTO TaskHeader (TaskDate) VALUES
                       (@TaskDate)
                    ;

                    WITH CTE AS (
                        SELECT 
                            sum(CASE WHEN DurationLevel != 0 THEN DurationLevel END) as UsedPoints
                            ,sum(CASE WHEN DurationLevel = 0 THEN Duration END) as CustomizedMins
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
                    SET Duration = DurationLevel * (SELECT BasicPoints FROM TaskHeader WHERE TaskDate = @TaskDate)
                    WHERE
                        DurationLevel != 0
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
            ip_DurationLevel.SelectedIndex = 2;
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

    }


}
