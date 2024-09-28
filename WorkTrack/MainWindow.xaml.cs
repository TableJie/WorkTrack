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
using static WorkTrack.TaskInput;
using Serilog;
using System.Windows.Documents;

namespace WorkTrack
{
    public partial class MainWindow : Window
    {
        private TaskSearch _taskSearch;
        public DateTime TodayDate { get; set; }
        public ChartValues<double> TaskDurations { get; set; } = new ChartValues<double>();
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();

        public MainWindow()
        {
            InitializeComponent();
            LogConfiguration.Initialize(); // 初始化日誌配置

            Log.Information("MainWindow initialized.");

            _taskSearch = new TaskSearch();  // 初始化 TaskSearch
            DataContext = this;
            TodayDate = DateTime.Now;

            Log.Information("Navigating to Page0_Welcome.");
            MainFrame.NavigationService.Navigate(new Page0_Welcome());

            try
            {
                Log.Information("Initializing database.");
                DatabaseInitializer dbInitializer = new DatabaseInitializer();
                dbInitializer.Initialize();
                Log.Information("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize database.");
                MessageBox.Show($"Database initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _ = InitializeStackedColumnChart(DateTime.Today);
            UpdateService.DataUpdated += async (date) => await InitializeStackedColumnChart(date);
        }

        #region Cd1_Bt

        public async System.Threading.Tasks.Task InitializeStackedColumnChart(DateTime selectedDate)
        {
            SeriesCollection.Clear();
            var formattedDate = selectedDate.ToString("yyyy-MM-dd");

            try
            {
                Log.Information("Fetching tasks for {FormattedDate}", formattedDate);
                var tasks = await _taskSearch.GetTasks(selectedDate);
                Log.Information("Fetched {TaskCount} tasks.", tasks.Count());

                int taskCount = 0;
                int pointsCount = 0;

                var query = @"
                            SELECT coalesce(OverHours,0) as OverHours ,coalesce(TotalMins,0) as TotalMins ,coalesce(UsedMins,0) as UsedMins ,coalesce(AvailableMins,0) as AvailableMins
                            FROM TaskHeader 
                            WHERE date(TaskDate) = @TaskDate";

                double overHours = 0;
                double totalMins = 0;
                double usedMins = 0;
                double availableMins = 0;

                using (var connection = new SqliteConnection(App.ConnectionString))
                {
                    await connection.OpenAsync();
                    var result = await connection.QueryFirstOrDefaultAsync<(double OverHours, double TotalMins, double UsedMins, double AvailableMins)>(
                        query,
                        new { TaskDate = formattedDate }
                    );

                    if (result != default)
                    {
                        (overHours, totalMins, usedMins, availableMins) = result;
                    }
                    else
                    {
                        Log.Warning("No data found in TaskHeader for date: {FormattedDate}", formattedDate);
                    }
                }

                double scaleFactor = usedMins / totalMins;


                if (tasks.Any() || availableMins > 0)
                {
                    foreach (var task in tasks)
                    {
                        taskCount++;
                        if (task.DurationLevelID != 9)
                        {
                            pointsCount += task.DurationLevelID;
                        }

                        SeriesCollection.Add(new StackedRowSeries
                        {
                            Values = new ChartValues<double> { (int)Math.Truncate(task.Duration * scaleFactor) },
                            StackMode = StackMode.Values,
                            DataLabels = false,
                            Fill = Brushes.Teal,
                            Stroke = Brushes.White,
                            StrokeThickness = 0.5,
                            MaxRowHeight = 20,
                            Title = task.TaskName
                        });
                    }

                    if (availableMins > 0)
                    {
                        SeriesCollection.Add(new StackedRowSeries
                        {
                            Values = new ChartValues<double> { availableMins },
                            StackMode = StackMode.Values,
                            DataLabels = false,
                            Fill = Brushes.Gray,
                            Stroke = Brushes.White,
                            StrokeThickness = 0.5,
                            MaxRowHeight = 20,
                            Title = "Empty",
                        });
                    }
                }
                else
                {
                    Log.Warning("No data available for {SelectedDate}", selectedDate);
                    SeriesCollection.Add(new StackedRowSeries
                    {
                        Values = new ChartValues<double> { availableMins },
                        StackMode = StackMode.Values,
                        DataLabels = true,
                        Fill = Brushes.Gray,
                        Stroke = Brushes.White,
                        StrokeThickness = 0.5,
                        MaxRowHeight = 20,
                        Title = "No Data"
                    });
                }

                double avgpoint = taskCount > 0 ? (double)pointsCount / taskCount : 0;
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Card_Label1.Inlines.Clear();
                    Card_Label1.Inlines.Add(new Run($"{taskCount} ") { FontSize = 15 });
                    Card_Label1.Inlines.Add(new Run($"({avgpoint:F2})") { FontSize = 10 });

                    Card_Label2.Text = overHours.ToString("F1");
                });

                Log.Information("Chart initialized with {SeriesCount} series", SeriesCollection.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load task durations for {SelectedDate}. SeriesCollection count: {Count}", selectedDate, SeriesCollection.Count);
                MessageBox.Show($"Failed to load task durations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string query, object parameters = null)
        {
            try
            {
                Log.Information("Executing query: {Query}", query);
                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();
                var result = await connection.QueryAsync<T>(query, parameters);
                Log.Information("Query executed successfully.");
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to execute query: {Query}", query);
                MessageBox.Show($"Failed to execute query: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Enumerable.Empty<T>();
            }
        }

        private void bt_OverTime_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new TaskPage());
            DateTime selectedDate = DateTime.Today;
            OverTimeInput overTimeInputWindow = new OverTimeInput(selectedDate);

            // 設置子視窗位置
            overTimeInputWindow.Left = this.Width - this.Left; // 設置子視窗顯示在主視窗的右側
            overTimeInputWindow.Top = this.Top + 100; // 垂直位置與主視窗對齊
            overTimeInputWindow.ShowDialog();
        }

        private void bt_CardAddTask_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new TaskPage());

            var newTask = new Task { TaskDate = DateTime.Today };
            TaskInput TaskInputWindow = new TaskInput(newTask, TaskInitializationMode.Add);
            TaskInputWindow.Left = this.Width - this.Left ; // 設置子視窗顯示在主視窗的右側
            TaskInputWindow.Top = this.Top + 100; // 垂直位置與主視窗對齊
            TaskInputWindow.ShowDialog();
        }

        private void bt_TaskCheck_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new TaskPage());
        }

        #endregion
    }
}
