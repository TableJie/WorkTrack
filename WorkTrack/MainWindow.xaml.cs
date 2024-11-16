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
using WorkTrack.Interfaces;
using WorkTrack.ViewModel;
using Serilog;
using System.Windows.Documents;
using WorkTrack.Services;

namespace WorkTrack
{
    public partial class MainWindow : Window
    {

        private readonly ILogger _logger;
        private readonly IInitializer _dbInitializer;
        private readonly ITaskService _taskService;
        private readonly IChartService _chartService;

        public DateTime TodayDate { get; set; }
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();

        public MainWindow(
            ILogger logger,
            IInitializer dbInitializer,
            ITaskService taskService,
            IChartService chartService)
        {
            InitializeComponent();
            _logger = logger;
            _dbInitializer = dbInitializer;
            _taskService = taskService;
            _chartService = chartService;

            DataContext = this;
            TodayDate = DateTime.Now;

            Loaded += async (s, e) => await InitializeApplicationAsync();
        }

        private async Task InitializeApplicationAsync()
        {
            try
            {
                _logger.Information("Initializing application.");
                InitializeDatabase();

                _logger.Information("Navigating to Page0_Welcome.");
                if (MainFrame?.NavigationService != null)
                {
                    MainFrame.NavigationService.Navigate(new Page0_Welcome());
                }
                else
                {
                    _logger.Warning("MainFrame or its NavigationService is null.");
                }

                if (_chartService != null)
                {
                    await _chartService.InitializeChartAsync(DateTime.Today);
                    _chartService.UpdateChartUI += ChartService_UpdateChartUI;
                }
                else
                {
                    _logger.Warning("ChartService is null.");
                }

                UpdateService.DataUpdated += async (date) =>
                {
                    if (_chartService != null)
                    {
                        await _chartService.InitializeChartAsync(date);
                    }
                };

                _logger.Information("Application initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize application.");
                MessageBox.Show($"Application initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                _logger.Information("Initializing database.");
                _dbInitializer.Initialize();
                _logger.Information("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize database.");
                MessageBox.Show($"Database initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Cd1_Bt

        private void ChartService_UpdateChartUI(object? sender, ChartUpdateEventArgs e)
        {
            SeriesCollection = e.SeriesCollection;
            Card_Label1.Inlines.Clear();
            Card_Label1.Inlines.Add(new Run($"{e.TaskCount} ") { FontSize = 15 });
            Card_Label1.Inlines.Add(new Run($"({e.AveragePoint:F2})") { FontSize = 10 });
            Card_Label2.Text = e.OverHours.ToString("F1");
        }

        private void bt_OverTime_Click(object sender, RoutedEventArgs e)
        {
            var taskViewModel = new TaskViewModel(_taskService, _chartService, _logger);
            MainFrame.NavigationService.Navigate(new TaskPage(taskViewModel));
            DateTime selectedDate = DateTime.Today;
            OverTimeInput overTimeInputWindow = new OverTimeInput(selectedDate, _logger, _taskService)
            {
                Left = this.Left + this.Width,
                Top = this.Top + 100,
                Owner = this
            };
            overTimeInputWindow.ShowDialog();
        }

        private void bt_CardAddTask_Click(object sender, RoutedEventArgs e)
        {
            var taskViewModel = new TaskViewModel(_taskService, _chartService, _logger);
            MainFrame.NavigationService.Navigate(new TaskPage(taskViewModel));
            var newTask = new WorkTask { TaskDate = DateTime.Today };
            var taskCompletionSource = new TaskCompletionSource<WorkTask>();
            taskCompletionSource.SetResult(newTask);

            TaskInput taskInputWindow = new TaskInput(newTask, TaskInput.TaskInitializationMode.Add, _logger, _taskService)
            {
                Left = this.Left + this.Width,
                Top = this.Top + 100,
                Owner = this
            };
            taskInputWindow.ShowDialog();
        }

        private void bt_TaskCheck_Click(object sender, RoutedEventArgs e)
        {
            var taskViewModel = new TaskViewModel(_taskService, _chartService, _logger);
            MainFrame.NavigationService.Navigate(new TaskPage(taskViewModel));
        }

        #endregion
    }
}
