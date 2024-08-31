using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dapper;
using static WorkTrack.InputTask;

namespace WorkTrack
{
    /// <summary>
    /// Page1.xaml 的互動邏輯
    /// </summary>

    public partial class Page1_Task : Page
    {
        private bool isDataLoading = false;

        public Page1_Task()
        {
            InitializeComponent();
            LoadDataAndInitialize();
        }


        private async void LoadDataAndInitialize()
        {
            isDataLoading = true; // 設置標誌位

            ip_TaskDate.SelectedDate = DateTime.Today; // 設定日期
            await DefaultSearch_TaskBody(); // 主動呼叫資料查詢方法

            isDataLoading = false; // 重置標誌位
        }

        public async Task DefaultSearch_TaskBody()
        {
            isDataLoading = true;

            try
            {

                var taskSearch = new TaskSearch();
                var taskBodyData = await taskSearch.GetTasks(selectedDate.Date);

                dt_TaskBody.ItemsSource = taskBodyData;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load TaskBody: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isDataLoading = false;
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

            var inputTaskWindow = new InputTask(task, mode)
            {
                Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.Width, // 設置子視窗顯示在主視窗的右側
                Top = Application.Current.MainWindow.Top + 100 // 垂直位置相對於主視窗往下移動 100
            };

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

        private async void ToggleButton_CheckedOrUnchecked(object sender, RoutedEventArgs e)
        {

            if (isDataLoading) return; // 跳過通知顯示

            if (sender is ToggleButton toggleButton && toggleButton.DataContext is TaskBody task)
            {
                bool deleteFlag = toggleButton.IsChecked == false;
                task.DeleteFlag = deleteFlag;
                    
                try
                {
                    await UpdateDeleteFlagInDatabase(task.TaskID, deleteFlag);

                    // 根據操作結果顯示相應的通知
                    string message = !deleteFlag ? "已刪除此 Task" : "已恢復此 Task";
                    MessageBox.Show(message, "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"更新刪除標誌時出錯: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task UpdateDeleteFlagInDatabase(int taskId, bool deleteFlag)
        {

            bool InputDeleteFlag = !deleteFlag;

            try
            {
                using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                string query = "UPDATE TaskBody SET DeleteFlag = @DeleteFlag WHERE TaskID = @TaskID";
                await connection.ExecuteAsync(query, new { DeleteFlag = InputDeleteFlag, TaskID = taskId });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新刪除標誌時出錯: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



    }

}
