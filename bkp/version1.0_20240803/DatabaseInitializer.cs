﻿using System;
using System.Diagnostics;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

public class DatabaseInitializer
{
    private string DatabasePath = "Database/app.db"; // 確保這裡的路徑正確

    public void Initialize()
    {
        try
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), DatabasePath);

            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)); // 確保資料夾存在
                Debug.WriteLine($"資料庫文件創建於: {fullPath}");
            }

            using (var connection = new SqliteConnection($"Data Source={fullPath};"))
            {
                connection.Open();

                var createTableQueries = new[]
                {
                    @"
                    CREATE TABLE IF NOT EXISTS TaskBody (
                        TaskID INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskDate DATE NOT NULL,
                        TaskName TEXT NOT NULL,
                        DurationLevel TEXT,
                        Duration INTEGER,
                        Description TEXT,
                        UnitID INTEGER,
                        ApplicationID INTEGER,
                        RegistDatetime DATETIME DEFAULT CURRENT_TIMESTAMP,
                        DetailFlag BOOLEAN DEFAULT 0,
                        DeleteFlag BOOLEAN DEFAULT 0
                    );",
                    @"
                    CREATE TABLE IF NOT EXISTS Application (
                        ApplicationID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ApplicationName TEXT,
                        ApplicationSubName TEXT,
                        ApplicationStatus TEXT,
                        ApplicationDatetime DATETIME,
                        PCDFlag BOOLEAN DEFAULT 0,
                        UnitID INTEGER,
                        RegistDatetime DATETIME DEFAULT CURRENT_TIMESTAMP,
                        DeleteFlag BOOLEAN DEFAULT 0
                    );",
                    @"
                    CREATE TABLE IF NOT EXISTS Unit (
                        UnitID INTEGER PRIMARY KEY AUTOINCREMENT,
                        UnitName TEXT,
                        RegistDatetime DATETIME DEFAULT CURRENT_TIMESTAMP,
                        DeleteFlag BOOLEAN DEFAULT 0
                    );",
                    @"
                    CREATE TABLE IF NOT EXISTS TaskHeader (
                        TaskDate DATE PRIMARY KEY,
                        OverTime REAL,
                        BasicPoint REAL,
                        Tiny REAL,
                        Small REAL,
                        Median REAL,
                        Large REAL,
                        Huge REAL,
                        RegistDatetime DATETIME DEFAULT CURRENT_TIMESTAMP,
                        DeleteFlag BOOLEAN DEFAULT 0
                    );",
                    @"
                    CREATE TABLE DurationLevel (
                        DurationLevel TEXT PRIMARY KEY,
                        Points INT NOT NULL
                    );"
                };

                foreach (var query in createTableQueries)
                {
                    connection.Execute(query);
                }

                // 插入初始資料
                var insertDataQuery = new[]
                {
                    @"
                    INSERT OR IGNORE INTO DurationLevel (DurationLevel, Points) VALUES
                        ('Tiny', 1)
                        ,('Small', 2)
                        ,('Median', 3)
                        ,('Large', 4)
                        ,('Huge', 5)
                        ,('-Customize-', 6)
                    ;",
                    @"
                    INSERT OR IGNORE INTO Unit (UnitName) VALUES
                        ('IMD')
                        ,('FA')
                        ,('APP')
                        ,('MECT')
                        ,('METRO')
                        ,('CSO')
                        ,('CSM')
                        ,('AC')
                        ,('AR')
                        ,('PCD')
                    ;",
                };

                foreach (var query in insertDataQuery)
                {
                    connection.Execute(query);
                }

                Debug.WriteLine("所有資料表已創建或已存在。");
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"初始化資料庫時發生錯誤: {ex.Message}");
        }
    }
}