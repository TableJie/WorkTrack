using Dapper;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WorkTrack
{
    public partial class MainWindow : Window
    {

        public ChartValues<double> TaskDurations { get; set; } = new ChartValues<double>();
        public List<string> TaskLabels { get; set; } = new List<string>();

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            TaskDurations = new ChartValues<double>();
            DataContext = this;

            ip_TaskDate.SelectedDate = DateTime.Today;
            DatabaseInitializer dbInitializer = new DatabaseInitializer();
            dbInitializer.Initialize();

            Loaded += MainWindow_Loaded;
            ip_TaskDate.SelectedDateChanged += (s, e) => LoadData(); // 設置日期選擇變更事件

            InitializeStackedColumnChart(); //
        }
        private void InitializeStackedColumnChart()
        {
            SeriesCollection = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Values = new ChartValues<double> {4, 5, 6, 8},
                    StackMode = StackMode.Values,
                    DataLabels = true
                },
                new StackedColumnSeries
                {
                    Values = new ChartValues<double> {2, 5, 6, 7},
                    StackMode = StackMode.Values,
                    DataLabels = true
                }
            };

            SeriesCollection.Add(new StackedColumnSeries
            {
                Values = new ChartValues<double> { 6, 2, 7 },
                StackMode = StackMode.Values
            });

            SeriesCollection[2].Values.Add(4d);

            Labels = new[] { "Chrome", "Mozilla", "Opera", "IE" };
            Formatter = value => value + " Mill";
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await DefaultSearch_TaskBody();
            await LoadUnitNames();
            LoadData();

        }

        private async void LoadData()
        {
            var selectedDate = ip_TaskDate.SelectedDate;
            if (selectedDate == null)
            {
                MessageBox.Show("Please select a date.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var taskDurations = await GetTaskDurations(selectedDate.Value);
            var taskLabels = await GetTaskLabels(selectedDate.Value);
            UpdateChart(taskDurations, taskLabels);
            await DefaultSearch_TaskBody();
        }

        private async Task<List<double>> GetTaskDurations(DateTime selectedDate)
        {
            using var connection = new SqliteConnection(App.ConnectionString);
            await connection.OpenAsync();

            var query = "SELECT Duration FROM TaskBody WHERE TaskDate = @TaskDate";
            var durations = (await connection.QueryAsync<int>(query, new { TaskDate = selectedDate })).ToList();

            return durations.Select(d => (double)d).ToList();
        }

        private void UpdateChart(List<double> taskDurations, List<string> taskLabels)
        {
            TaskDurations.Clear();
            TaskLabels.Clear();
            double totalDuration = taskDurations.Sum();
            if (totalDuration > 0)
            {
                foreach (var duration in taskDurations)
                {
                    TaskDurations.Add(duration / totalDuration); // 轉換為百分比
                }
                TaskLabels.AddRange(taskLabels);
            }
        }
        private async Task<List<string>> GetTaskLabels(DateTime selectedDate)
        {
            using var connection = new SqliteConnection(App.ConnectionString);
            await connection.OpenAsync();

            var query = "SELECT TaskName FROM TaskBody WHERE TaskDate = @TaskDate";
            var taskNames = (await connection.QueryAsync<string>(query, new { TaskDate = selectedDate })).ToList();

            return taskNames;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshTaskBodyAsync();
            LoadData(); // 更新圖表和表格
        }

        private void dt_TaskBody_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dt_TaskBody.SelectedItem is TaskBody selectedTask)
            {
                ip_TaskID.Text = selectedTask.TaskID.ToString();
                ip_TaskName.Text = selectedTask.TaskName;
                ip_Describe.Text = selectedTask.Description;
                ip_DurationLevel.Text = selectedTask.DurationLevel;
                ip_Duration.Text = selectedTask.Duration.ToString();
                ip_UnitName.Text = selectedTask.UnitID;
                ip_ApplicationID.Text = selectedTask.ApplicationID;
            }
        }


        private async Task DefaultSearch_TaskBody()
        {
            try
            {
                DateTime? selectedDate = ip_TaskDate.SelectedDate;

                if (selectedDate == null)
                {
                    MessageBox.Show("請選擇日期。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                var taskBodyData = (await connection.QueryAsync<TaskBody>(
                    "SELECT p1.*, t1.UnitName FROM TaskBody p1 LEFT JOIN Unit t1 ON p1.UnitID = t1.UnitID WHERE TaskDate = @TaskDate",
                    new { TaskDate = selectedDate.Value.Date }
                )).ToList();

                dt_TaskBody.ItemsSource = taskBodyData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load task body: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadUnitNames()
        {
            try
            {
                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                var unitNames = (await connection.QueryAsync<string>("SELECT UnitName FROM Unit")).ToList();
                //unitNames.Insert(0, "-Add-");
                unitNames.Add("-Add-");

                ip_UnitName.ItemsSource = unitNames;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load unit names: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                string durationLevel = ip_DurationLevel.Text;
                int? duration = string.IsNullOrEmpty(ip_Duration.Text) ? (int?)null : int.Parse(ip_Duration.Text);
                string unitID = ip_UnitName.Text;
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
                        UnitID = unitID,
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
                        UnitID = unitID,
                        ApplicationID = applicationID,
                        TaskID = taskID
                    });
                }

                var insertOrUpdateTaskHeader = $$"""
                    INSERT INTO TaskHeader (TaskDate, OverTime)
                    VALUES (@TaskDate, @OverTime)
                    ON CONFLICT (TaskDate)
                    DO UPDATE SET OverTime = @OverTime;

                    WITH NetMin as (
                	    SELECT (8 + p1.OverTime) * 60 - sum(coalesce(p2.Duration,0)) as NetMin
                	    FROM
                		    TaskHeader p1
                		    LEFT JOIN TaskBody p2
                			    on p1.TaskDate = p2.TaskDate
                			    and not p2.DeleteFlag
                			    and p2.DurationLevel = '-Customize-'
                	    WHERE p1.TaskDate = @TaskDate
                    ),BasicPoint as (
                	    SELECT (SELECT NetMin FROM NetMin) / sum(p2.Points) as BasicPoint
                	    FROM
                		    TaskBody p1
                		    LEFT JOIN DurationLevel p2 on p1.DurationLevel = p2.DurationLevel
                	    WHERE
                		    not p1.DurationLevel = '-Customize-'
                		    and not p1.DeleteFlag
                		    and p1.TaskDate = @TaskDate
                    ),GetPoint as (
                	    SELECT
                		    p1.TaskID
                		    ,t1.Points * (SELECT BasicPoint FROM BasicPoint) as Points
                	    FROM
                		    TaskBody p1
                		    LEFT JOIN DurationLevel t1 on p1.DurationLevel = t1.DurationLevel
                	    WHERE
                            p1.TaskDate = @TaskDate
                            and not p1.DurationLevel = '-Customize-'
                    )
                    UPDATE TaskBody as p1
                    SET Duration = foo.Points
                    FROM GetPoint foo
                    WHERE
                	    p1.TaskID = foo.TaskID
                	    and p1.TaskDate = @TaskDate
                        and not p1.DurationLevel = '-Customize-'
                
                    ;
                """;

                await connection.ExecuteAsync(insertOrUpdateTaskHeader, new
                {
                    TaskDate = taskDate,
                    OverTime = 0
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update task body: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            await DefaultSearch_TaskBody();
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



        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(ip_DurationLevel.Text);
        }


    }


}