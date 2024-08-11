using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using Microsoft.Data.Sqlite;

namespace WorkTrack
{

    public class Unit
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; } = string.Empty; // 設置初始值
        public DateTime RegistDatetime { get; set; }
        public bool DeleteFlag { get; set; }
    }

    public partial class UnitManagement : Window
    {
        public UnitManagement()
        {
            InitializeComponent();
            LoadUnitData();
        }

        private async void LoadUnitData()
        {
            try
            {
                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();
                var units = (await connection.QueryAsync<Unit>("SELECT * FROM Unit")).ToList();
                dt_Unit.ItemsSource = units;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load units: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void bt_SaveUnit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string unitName = ip_UnitName.Text;

                if (string.IsNullOrEmpty(unitName))
                {
                    MessageBox.Show("Unit Name cannot be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await using var connection = new SqliteConnection(App.ConnectionString);
                await connection.OpenAsync();
                var insertQuery = "INSERT INTO Unit (UnitName) VALUES (@UnitName)";
                await connection.ExecuteAsync(insertQuery, new { UnitName = unitName });

                MessageBox.Show("Unit saved successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUnitData(); // 重新加載單位數據
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save unit: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int unitId)
            {
                var result = MessageBox.Show("確認要刪除此單位嗎？", "刪除確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await using var connection = new SqliteConnection(App.ConnectionString);
                        await connection.OpenAsync();

                        var deleteQuery = "UPDATE Unit SET DeleteFlag = 1 WHERE UnitID = @UnitID";
                        await connection.ExecuteAsync(deleteQuery, new { UnitID = unitId });

                        MessageBox.Show("資料已刪除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUnitData(); // 刷新表格資料
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"刪除失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


    }




}
