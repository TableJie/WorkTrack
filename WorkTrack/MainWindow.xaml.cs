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

        private TaskSearch _taskSearch;  // TaskSearch 的實例
        public DateTime TodayDate { get; set; }
        public ChartValues<double> TaskDurations { get; set; } = new ChartValues<double>();
        public SeriesCollection SeriesCollection { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _taskSearch = new TaskSearch();  // 初始化 TaskSearch
            DataContext = this;
            TodayDate = DateTime.Now;

            MainFrame.NavigationService.Navigate(new Page0_Welcome());

            DatabaseInitializer dbInitializer = new DatabaseInitializer();
            dbInitializer.Initialize();

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



        public async Task InitializeStackedColumnChart()
        {
            SeriesCollection.Clear();

            try
            {
                var tasks = await _taskSearch.GetTasks(DateTime.Now);  // 從 TaskSearch 獲取當天的任務資料

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

                // 顯示空白時間的邏輯


                var Emptyquery = "SELECT AvailableMins FROM TaskHeader WHERE TaskDate = @TaskDate";
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
                MessageBox.Show($"Failed to load task durations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string query, object parameters = null)
        {
            try
            {
                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();
                return await connection.QueryAsync<T>(query, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to execute query: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Enumerable.Empty<T>();
            }
        }

    }


}