using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WorkTrack;
using WorkTrack.Interfaces;
using WorkTrack.Services;
using Serilog;

namespace WorkTrack
{
    public partial class OverTimeInput : Window
    {
        private const int MAX_TASK_PLANS = 8;
        private const double OVER_HOURS_FACTOR = 0.5;
        private readonly DateTime _taskDate;
        private readonly ILogger _logger;
        private readonly ITaskService _taskService;

        public OverTimeInput(DateTime taskDate, ILogger logger, ITaskService taskService)
        {
            InitializeComponent();
            _taskDate = taskDate;
            _logger = logger;
            _taskService = taskService;
            ip_TaskDate.SelectedDate = _taskDate;
        }

        private void ip_OverHours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ip_OverHours.SelectedItem is ComboBoxItem selectedItem)
            {
                int selectedIndex = ip_OverHours.Items.IndexOf(selectedItem);
                UpdateTaskPlanVisibility(selectedIndex);
            }
        }

        private void UpdateTaskPlanVisibility(int selectedIndex)
        {
            pn_Task.Visibility = selectedIndex >= 0 ? Visibility.Visible : Visibility.Collapsed;
            for (int i = 0; i < pn_Plan.Children.Count; i++)
            {
                ((TextBox)pn_Plan.Children[i]).Visibility = i < selectedIndex + 1 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                MessageBox.Show("請填寫所有顯示的任務計劃", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var overHours = (ip_OverHours.SelectedIndex + 1) * OVER_HOURS_FACTOR;
                var taskPlans = GetTaskPlans();
                var overTime = new OverTime
                {
                    TaskDate = _taskDate,
                    OverHours = overHours,
                    TaskPlan1 = taskPlans[0],
                    TaskPlan2 = taskPlans[1],
                    TaskPlan3 = taskPlans[2],
                    TaskPlan4 = taskPlans[3],
                    TaskPlan5 = taskPlans[4],
                    TaskPlan6 = taskPlans[5],
                    TaskPlan7 = taskPlans[6],
                    TaskPlan8 = taskPlans[7]
                };
                await _taskService.EditOverTimeAsync(overTime);

                MessageBox.Show("加班資訊已更新", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateService.NotifyDataUpdated(_taskDate);
                this.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "更新加班資訊時發生錯誤");
                MessageBox.Show($"更新失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private string[] GetTaskPlans()
        {
            return Enumerable.Range(1, MAX_TASK_PLANS)
                .Select(i => (FindName($"ip_TaskPlan{i}") as TextBox)?.Text ?? string.Empty)
                .ToArray();
        }

        private bool ValidateInput()
        {
            return Enumerable.Range(1, MAX_TASK_PLANS)
                .All(i => (FindName($"ip_TaskPlan{i}") as TextBox)?.Visibility != Visibility.Visible ||
                          !string.IsNullOrWhiteSpace((FindName($"ip_TaskPlan{i}") as TextBox)?.Text));
        }
    }
}