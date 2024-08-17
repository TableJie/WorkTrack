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
            ip_TaskDate.SelectedDate = DateTime.Today;
            DefaultSearch_TaskBody();

        }

        public async Task DefaultSearch_TaskBody()
        {
            try
            {
                DateTime? selectedDate = ip_TaskDate.SelectedDate;

                if (selectedDate == null)
                {
                    MessageBox.Show("請選擇日期。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();

                var taskBodyData = (await connection.QueryAsync<TaskBody>(
                    "SELECT p1.*, t1.UnitName FROM TaskBody p1 LEFT JOIN Unit t1 ON p1.UnitID = t1.UnitID WHERE TaskDate = @TaskDate",
                    new { TaskDate = selectedDate.Value.Date }
                )).ToList();

                if (this.FindName("dt_TaskBody") is DataGrid dt_TaskBody)
                {
                    dt_TaskBody.ItemsSource = taskBodyData;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load task body: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Delete");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Edit");
        }

        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Detail");
        }

        private void bt_TaskAdd_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = ip_TaskDate.SelectedDate;
            InputTask inputTaskWindow = new InputTask(selectedDate);
            inputTaskWindow.ShowDialog();
        }
    }

}
