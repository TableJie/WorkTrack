using Dapper;
using Microsoft.Data.Sqlite;
using System.Windows;
using System.Windows.Controls;
using System.Data;

namespace WorkTrack
{
    public partial class OverTimeInput : Window
    {

        private const int MAX_TASK_PLANS = 8;
        private const double OVER_HOURS_FACTOR = 0.5;
        private readonly DateTime _taskDate;

        public OverTimeInput(DateTime taskDate)
        {
            InitializeComponent();
            _taskDate = taskDate;
            ip_TaskDate.SelectedDate = _taskDate;
        }


        private void ip_OverHours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ip_OverHours.SelectedItem == null)
            {
                pn_Task.Visibility = Visibility.Collapsed;
                return;
            }

            pn_Task.Visibility = Visibility.Visible;

            if (ip_OverHours.SelectedItem is ComboBoxItem selectedItem)
            {

                int selectedIndex = ip_OverHours.Items.IndexOf(selectedItem);  // 使用 ComboBox 的 index 而非解析文字

                for (int i = 0; i < pn_Plan.Children.Count; i++)
                {
                    if (i < selectedIndex + 1)  // 根據選擇的 index 顯示對應的 TextBox
                    {
                        ((TextBox)pn_Plan.Children[i]).Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ((TextBox)pn_Plan.Children[i]).Visibility = Visibility.Collapsed;
                    }
                }
            }
        }


        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                MessageBox.Show("請填寫所有顯示的計劃輸入框。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new SqliteConnection(App.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        var overHours = (ip_OverHours.SelectedIndex + 1) * OVER_HOURS_FACTOR;
                        var taskPlans = Enumerable.Range(1, MAX_TASK_PLANS)
                            .Select(i => (FindName($"ip_TaskPlan{i}") as TextBox)?.Visibility == Visibility.Visible
                                ? (FindName($"ip_TaskPlan{i}") as TextBox)?.Text
                                : null)
                            .ToArray();

                        var overTimeQuery = @"
                                            INSERT INTO OverTime (TaskDate, OverHours, TaskPlan1, TaskPlan2, TaskPlan3, TaskPlan4, TaskPlan5, TaskPlan6, TaskPlan7, TaskPlan8)
                                            VALUES (@TaskDate, @OverHours, @TaskPlan1, @TaskPlan2, @TaskPlan3, @TaskPlan4, @TaskPlan5, @TaskPlan6, @TaskPlan7, @TaskPlan8)
                                            ON CONFLICT(TaskDate) DO UPDATE SET
                                            OverHours = @OverHours,
                                            TaskPlan1 = @TaskPlan1, TaskPlan2 = @TaskPlan2, TaskPlan3 = @TaskPlan3, TaskPlan4 = @TaskPlan4,
                                            TaskPlan5 = @TaskPlan5, TaskPlan6 = @TaskPlan6, TaskPlan7 = @TaskPlan7, TaskPlan8 = @TaskPlan8";

                        await connection.ExecuteAsync(overTimeQuery, new
                        {
                            TaskDate = _taskDate.ToString("yyyy-MM-dd")
                            ,OverHours = overHours
                            ,TaskPlan1 = taskPlans[0]
                            ,TaskPlan2 = taskPlans[1]
                            ,TaskPlan3 = taskPlans[2]
                            ,TaskPlan4 = taskPlans[3]
                            ,TaskPlan5 = taskPlans[4]
                            ,TaskPlan6 = taskPlans[5]
                            ,TaskPlan7 = taskPlans[6]
                            ,TaskPlan8 = taskPlans[7]
                        }, transaction);

                        var updateTaskHeaderQuery = @"
                                        UPDATE TaskHeader 
                                        SET OverHours = @OverHours 
                                        WHERE date(TaskDate) = @TaskDate";

                        await connection.ExecuteAsync(updateTaskHeaderQuery, new
                        {
                            TaskDate = _taskDate.ToString("yyyy-MM-dd"),
                            OverHours = overHours
                        }, transaction);

                        transaction.Commit();
                    }

                    MessageBox.Show("資料已成功更新。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"發生錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateService.NotifyDataUpdated(_taskDate);
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 關閉當前視窗
        }

        private bool ValidateInput()
        {
            return Enumerable.Range(1, MAX_TASK_PLANS)
                .All(i => (FindName($"ip_TaskPlan{i}") as TextBox)?.Visibility != Visibility.Visible ||
                          !string.IsNullOrWhiteSpace((FindName($"ip_TaskPlan{i}") as TextBox)?.Text));
        }
    }



}
