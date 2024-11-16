using System;
using System.Threading.Tasks;
using LiveCharts;
using WorkTrack.Services;

namespace WorkTrack.Interfaces
{
    public interface IChartService
    {
        Task<SeriesCollection> GetChartDataAsync(DateTime selectedDate);
        Task<ChartStatistics> GetChartStatisticsAsync(DateTime selectedDate);
        Task InitializeChartAsync(DateTime selectedDate); // 新增初始化圖表的方法
        event EventHandler<ChartUpdateEventArgs> UpdateChartUI;
    }

    public class ChartStatistics
    {
        public int TaskCount { get; set; }
        public double AveragePoint { get; set; }
        public double OverHours { get; set; }
        public SeriesCollection? SeriesCollection { get; set; }
    }
}
