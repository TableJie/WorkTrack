using Dapper;
using LiveCharts.Wpf;
using LiveCharts;
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
using System.Windows.Shapes;
using System.Data;

namespace WorkTrack
{
    public partial class OverTimeInput : Window
    {

        public OverTimeInput()  // 添加構造函數
        {
            InitializeComponent();
        }

        private void ip_OverTimeHours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ip_OverTimeHours.SelectedItem == null)
            {
                pn_Task.Visibility = Visibility.Collapsed;
                return;
            }

            pn_Task.Visibility = Visibility.Visible;

            if (ip_OverTimeHours.SelectedItem is ComboBoxItem selectedItem)
            {

                int selectedIndex = ip_OverTimeHours.Items.IndexOf(selectedItem);  // 使用 ComboBox 的 index 而非解析文字

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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 關閉當前視窗
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 關閉當前視窗
        }


    }



}
