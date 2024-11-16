using WorkTrack.Interfaces;
using LiveCharts;
using LiveCharts.Wpf;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WorkTrack.Services
{
    public class ChartService : IChartService
    {
        private readonly ITaskService _taskService;
        private readonly ILogger _logger;
        public SeriesCollection SeriesCollection { get; private set; }

        public ChartService(ITaskService taskService, ILogger logger)
        {
            _taskService = taskService;
            _logger = logger;
            SeriesCollection = new SeriesCollection();
        }

        public async Task<SeriesCollection> GetChartDataAsync(DateTime selectedDate)
        {
            var tasks = await _taskService.GetTasksAsync(selectedDate);
            var seriesCollection = new SeriesCollection();
            var chartValues = new ChartValues<double>();
            foreach (var task in tasks)
            {
                chartValues.Add(task.Duration);
            }
            seriesCollection.Add(new StackedColumnSeries
            {
                Values = chartValues,
                StackMode = StackMode.Values,
                DataLabels = false,
                Fill = new SolidColorBrush(Color.FromRgb(0, 128, 128))
            });
            return seriesCollection;
        }

        public async Task<ChartStatistics> GetChartStatisticsAsync(DateTime selectedDate)
        {
            var tasks = await _taskService.GetTasksAsync(selectedDate);
            return new ChartStatistics
            {
                TaskCount = tasks.Count,
                AveragePoint = tasks.Average(t => t.Duration),
                OverHours = tasks.Sum(t => t.Duration)
            };
        }

        public async Task InitializeChartAsync(DateTime selectedDate)
        {
            try
            {
                SeriesCollection = await GetChartDataAsync(selectedDate);
                var chartStatistics = await GetChartStatisticsAsync(selectedDate);

                UpdateChartUI?.Invoke(this, new ChartUpdateEventArgs
                {
                    TaskCount = chartStatistics.TaskCount,
                    AveragePoint = chartStatistics.AveragePoint,
                    OverHours = chartStatistics.OverHours,
                    SeriesCollection = SeriesCollection ?? new SeriesCollection()
                });

                _logger.Information("Chart initialized with {SeriesCount} series", SeriesCollection?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load task durations for {SelectedDate}.", selectedDate);
            }
        }

        public event EventHandler<ChartUpdateEventArgs>? UpdateChartUI;
    }

    public class ChartUpdateEventArgs : EventArgs
    {
        public int TaskCount { get; set; }
        public double AveragePoint { get; set; }
        public double OverHours { get; set; }
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
    }
}