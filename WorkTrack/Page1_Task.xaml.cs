using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dapper;
using static WorkTrack.InputTask;
using System.Windows.Controls.Primitives; // 確保命名空間引用正確

namespace WorkTrack
{
    /// <summary>
    /// Page1.xaml 的互動邏輯
    /// </summary>
    public partial class Page1_Task : Page
    {
        public Page1_Task()
        {
            InitializeComponent();
            ip_TaskDate.SelectedDateChanged -= ip_TaskDate_SelectedDateChanged; // 暫時取消事件綁定
            ip_TaskDate.SelectedDate = DateTime.Today; // 設定日期但不觸發事件
            DefaultSearch_TaskBody();
            ip_TaskDate.SelectedDateChanged += ip_TaskDate_SelectedDateChanged; // 重新綁定事件

        }

        public async Task DefaultSearch_TaskBody()
        {
            try
            {
                if (ip_TaskDate.SelectedDate is not DateTime selectedDate)
                {
                    MessageBox.Show("Please Select Date！", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var taskSearch = new TaskSearch();
                var taskBodyData = await taskSearch.GetTasks(selectedDate.Date);

                dt_TaskBody.ItemsSource = taskBodyData;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load TaskBody: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;

            TaskInitializationMode mode = button.Tag switch
            {
                "Add" => TaskInitializationMode.Add,
                "Edit" => TaskInitializationMode.Edit,
                "Copy" => TaskInitializationMode.Copy,
                _ => throw new ArgumentOutOfRangeException()
            };

            TaskBody task = mode == TaskInitializationMode.Add
                ? new TaskBody { TaskDate = ip_TaskDate.SelectedDate ?? DateTime.Today }
                : button.DataContext as TaskBody ?? new TaskBody { TaskDate = DateTime.Today };

            if (mode == TaskInitializationMode.Add && task.TaskDate == DateTime.MinValue)
            {
                MessageBox.Show("Please Select Date！", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var inputTaskWindow = new InputTask(task, mode);
            inputTaskWindow.ShowDialog();
        }

        private async void ip_TaskDate_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            await DefaultSearch_TaskBody();

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var selectedDate = ip_TaskDate.SelectedDate ?? DateTime.Today;

                mainWindow.ChartDate.Text = selectedDate.ToString("yyyy-MM-dd");
                await mainWindow.InitializeStackedColumnChart(selectedDate);
            }
        }

        private async void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.DataContext is TaskBody task)
            {
                task.DeleteFlag = false;
                await UpdateDeleteFlagInDatabase(task.TaskID, true);
            }
        }

        private async void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.DataContext is TaskBody task)
            {
                task.DeleteFlag = true;
                await UpdateDeleteFlagInDatabase(task.TaskID, false);
            }
        }

        private async Task UpdateDeleteFlagInDatabase(int taskId, bool deleteFlag)
        {
            try
            {
                using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                string query = "UPDATE TaskBody SET DeleteFlag = @DeleteFlag WHERE TaskID = @TaskID";
                await connection.ExecuteAsync(query, new { DeleteFlag = deleteFlag, TaskID = taskId });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新刪除標誌時出錯: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }

}
