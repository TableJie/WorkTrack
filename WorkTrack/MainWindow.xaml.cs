﻿using Dapper;
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
using static WorkTrack.InputTask;
using Serilog;

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

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

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

            var defaultDate = DateTime.Today;
            InitializeStackedColumnChart(defaultDate);
        }

        #region Cd1_Bt

        public async Task InitializeStackedColumnChart(DateTime selectedDate)
        {
            SeriesCollection.Clear();

            try
            {
                Log.Information("Fetching tasks for {SelectedDate}", selectedDate);
                var tasks = await _taskSearch.GetTasks(selectedDate);  // 從 TaskSearch 獲取當天的任務資料
                Log.Information("Fetched {TaskCount} tasks.", tasks.Count());

                int taskCount = 0;
                int pointsCount = 0;

                foreach (var task in tasks)
                {
                    taskCount++;
                    pointsCount += task.DurationLevelID;

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

                // 顯示空白時間的邏輯
                var Emptyquery = "SELECT coalesce(AvailableMins,480) FROM TaskHeader WHERE TaskDate = @TaskDate";
                var durations = await ExecuteQueryAsync<int>(Emptyquery, new { TaskDate = DateTime.Now.Date });

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

                Card_Label1.Text = taskCount.ToString();
                Card_Label2.Text = pointsCount.ToString();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load task durations for {SelectedDate}.", selectedDate);
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
            MainFrame.NavigationService.Navigate(new Page1_Task());
        }

        private void bt_CardAddTask_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new Page1_Task());

            var newTask = new TaskBody { TaskDate = DateTime.Today };
            var inputTaskWindow = new InputTask(newTask, TaskInitializationMode.Add);
            inputTaskWindow.ShowDialog();
        }

        private void bt_TaskCheck_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new Page1_Task());
        }

        #endregion
    }
}
