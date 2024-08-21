﻿using Microsoft.Data.Sqlite;
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
            ip_TaskDate.SelectedDateChanged -= ip_TaskDate_SelectedDateChanged; // 暫時取消事件綁定
            ip_TaskDate.SelectedDate = DateTime.Today; // 設定日期但不觸發事件
            DefaultSearch_TaskBody();
            ip_TaskDate.SelectedDateChanged += ip_TaskDate_SelectedDateChanged; // 重新綁定事件

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

                var taskSearch = new TaskSearch();
                var taskBodyData = await taskSearch.GetTasks(selectedDate.Value.Date);


                dt_TaskBody.ItemsSource = taskBodyData;

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
            var button = sender as Button;
            if (button == null) return;

            var selectedTask = button.DataContext as TaskBody;
            if (selectedTask == null) return;

            // 創建 InputTask 視窗並傳遞選中的 TaskBody 資料
            InputTask inputTaskWindow = new InputTask(selectedTask);
            inputTaskWindow.ShowDialog();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var selectedTask = button.DataContext as TaskBody;
            if (selectedTask == null) return;

            // 創建新的 TaskBody 副本，保留其他屬性，但將 TaskID 設為 0 或 null
            var newTask = new TaskBody
            {
                TaskID = 0,  // 或者設置為 null
                TaskDate = selectedTask.TaskDate,
                TaskName = selectedTask.TaskName,
                DurationLevel = selectedTask.DurationLevel,
                Duration = selectedTask.Duration,
                Description = selectedTask.Description,
                UnitID = selectedTask.UnitID,
                ApplicationID = selectedTask.ApplicationID
            };

            // 創建 InputTask 視窗並傳遞新的 TaskBody 資料
            InputTask inputTaskWindow = new InputTask(newTask);
            inputTaskWindow.ShowDialog();
        }

        private void bt_TaskAdd_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = ip_TaskDate.SelectedDate;
            if (selectedDate == null)
            {
                MessageBox.Show("請選擇日期。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 傳遞一個新的 TaskBody 實例，並設置 TaskDate
            var newTask = new TaskBody { TaskDate = selectedDate.Value };
            InputTask inputTaskWindow = new InputTask(newTask);
            inputTaskWindow.ShowDialog();
        }

        private async void ip_TaskDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await DefaultSearch_TaskBody();

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var selectedDate = ip_TaskDate.SelectedDate ?? DateTime.Today;

                    // 直接更新 TextBlock 的 Text 屬性
                    mainWindow.ChartDate.Text = selectedDate.ToString("yyyy-MM-dd");
                    await mainWindow.InitializeStackedColumnChart(selectedDate);

            }
        }

    }

}
