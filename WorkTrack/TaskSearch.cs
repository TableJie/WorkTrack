﻿using Dapper;
using Microsoft.Data.Sqlite;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace WorkTrack 
{
    public class TaskSearch
    {

        public async Task<List<TaskBody>> GetTasks(DateTime taskDate)
        {
            var query = "SELECT p1.*, t1.UnitName FROM TaskBody p1 LEFT JOIN Unit t1 ON p1.UnitID = t1.UnitID WHERE TaskDate = @TaskDate";

            await using var connection = new SqliteConnection(App.ConnectionString);
            await connection.OpenAsync();

            return (await connection.QueryAsync<TaskBody>(query, new { TaskDate = taskDate })).ToList();
        }

        public async Task UpdateTaskBodyAsync(TaskBody taskBody)
        {
            await using var connection = new SqliteConnection(App.ConnectionString);
            await connection.OpenAsync();

            // 這裡放置更新 TaskBody 的邏輯
        }

        // 其他相關方法
    }

}