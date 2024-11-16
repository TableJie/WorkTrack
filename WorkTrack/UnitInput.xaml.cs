using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WorkTrack.Interfaces;
using WorkTrack.Services;
using Serilog;
using System.Windows.Data;
using System.Globalization;

namespace WorkTrack
{
    public partial class UnitInput : Window
    {
        private readonly ILogger _logger;
        private readonly ITaskService _taskService;

        public UnitInput(ILogger logger, ITaskService taskService)
        {
            InitializeComponent();
            _logger = logger;
            _taskService = taskService;
            Loaded += UnitInput_Loaded;
        }

        private async void UnitInput_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUnitDataAsync();
        }

        private async Task LoadUnitDataAsync()
        {
            try
            {
                var units = await _taskService.GetUnitNamesAsync();
                dt_Unit.ItemsSource = units;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "載入單位資料時發生錯誤");
                MessageBox.Show($"載入資料失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void bt_SaveUnit_Click(object sender, RoutedEventArgs e)
        {
            string unitName = ip_UnitName.Text.Trim();
            if (string.IsNullOrEmpty(unitName))
            {
                MessageBox.Show("請輸入單位名稱", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var newUnit = new Unit { UnitName = unitName };
                await _taskService.SaveUnitAsync(newUnit);
                MessageBox.Show("單位已新增", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadUnitDataAsync();
                ip_UnitName.Clear();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "新增單位時發生錯誤");
                MessageBox.Show($"新增單位失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Unit unit)
            {
                var result = MessageBox.Show("確定要刪除此單位嗎？", "確認刪除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _taskService.DeleteUnitAsync(unit.UnitID);
                        MessageBox.Show("單位已刪除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUnitDataAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "刪除單位時發生錯誤");
                        MessageBox.Show($"刪除失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public class DeleteButtonOpacityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool deleteFlag)
                {
                    return deleteFlag ? 0.5 : 1.0;
                }
                return 1.0;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

    }
}