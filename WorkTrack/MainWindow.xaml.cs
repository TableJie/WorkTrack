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
using System.Windows.Media;
using System.Windows.Navigation;

namespace WorkTrack
{
    public partial class MainWindow : Window
    {
        public DateTime TodayDate { get; set; }
        public ChartValues<double> TaskDurations { get; set; } = new ChartValues<double>();
        public SeriesCollection SeriesCollection { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            TodayDate = DateTime.Now;

            MainFrame.NavigationService.Navigate(new Page0_Welcome());

            // 初始化數據庫
            DatabaseInitializer dbInitializer = new DatabaseInitializer();
            dbInitializer.Initialize();

            // 訂閱 Loaded 事件
            Loaded += MainWindow_Loaded;

            // 初始化圖表
            TaskDurations = new ChartValues<double>();
            SeriesCollection = new SeriesCollection();
            InitializeStackedColumnChart();
        }
        #region Cd1_Bt
        private void bt_OverTime_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new Page1_Task());
        }

        private void bt_TaskCheck_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new Page1_Task());
        }
        #endregion


        private async Task InitializeStackedColumnChart()
        {
            SeriesCollection.Clear();

            try
            {
                using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                var query = "SELECT p1.TaskName ,p1.DurationLevel ,t1.Points ,CAST(p1.Duration as INT) as Duration FROM TaskBody p1 LEFT JOIN DurationLevel t1 on p1.DurationLevel = t1.DurationLevel WHERE DATE(p1.TaskDate) = DATE('now') ORDER BY p1.Duration DESC";
                var tasks = await connection.QueryAsync<TaskBody>(query);

                int taskCount = 0;
                int pointsCount = 0;

                foreach (var task in tasks)
                {
                    taskCount++;
                    pointsCount += task.Points;

                    SeriesCollection.Add(new StackedRowSeries
                    {
                        Values = new ChartValues<double> { task.Duration },
                        StackMode = StackMode.Values,
                        DataLabels = true,
                        Fill = Brushes.Teal,
                        Stroke = Brushes.White,
                        StrokeThickness = 0.5,
                        MaxRowHeight = 20,
                        Title = task.TaskName
                    });
                }

                Card_Label1.Text = taskCount.ToString();
                Card_Label2.Text = pointsCount.ToString();

                var Emptyquery = "SELECT (coalesce((SELECT coalesce(OverTime,0) * 60 FROM TaskHeader WHERE DATE(TaskDate) = DATE('now')),0) + 480 - (SELECT coalesce(sum(Duration),0) FROM TaskBody WHERE DATE(TaskDate) = DATE('now') and not DeleteFlag)) as Duration";
                var durations = await connection.QueryAsync<int>(Emptyquery);

                foreach (var duration in durations)
                {
                    SeriesCollection.Add(new StackedRowSeries
                    {
                        Values = new ChartValues<double> { duration },
                        StackMode = StackMode.Values,
                        DataLabels = true,
                        Fill = Brushes.Gray,
                        Stroke = Brushes.White,
                        StrokeThickness = 0.5,
                        MaxRowHeight = 20,
                        Title = "Empty",
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load task durations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #region Form
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
           
            await LoadUnitNames();

        }
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshTaskBodyAsync();
            await InitializeStackedColumnChart();

            ip_TaskID.Clear();
            ip_TaskName.Clear();
            ip_Describe.Clear();
            ip_DurationLevel.SelectedIndex = 2;
            ip_UnitName.SelectedIndex = 0;
            ip_ApplicationID.SelectedIndex = 0;

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
                	    SELECT CAST((SELECT NetMin FROM NetMin) / sum(p2.Points) as INT) as BasicPoint
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
        #endregion




        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(ip_DurationLevel.Text);
        }


    }


}