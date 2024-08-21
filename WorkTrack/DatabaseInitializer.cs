﻿using System;
using System.Diagnostics;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace WorkTrack
{
    public class DatabaseInitializer
    {
        private readonly string DatabasePath = "Database/app.db"; // 加入命名空間並將DatabasePath標記為readonly

        public void Initialize()
        {
            try
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), DatabasePath);

                if (!File.Exists(fullPath))
                {
                    // 確保 DirectoryPath 不為 null
                    string directoryPath = Path.GetDirectoryName(fullPath);
                    if (directoryPath != null)
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    Debug.WriteLine($"創建資料庫路徑: {fullPath}");
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
                            DurationLevel INTEGER,
                            Duration INTEGER,
                            Description TEXT,
                            UnitID INTEGER,
                            ApplicationID INTEGER,
                            RegistDatetime DATETIME DEFAULT CURRENT_TIMESTAMP,
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
                            OverHours REAL DEFAULT 0,
                            TotalHours REAL DEFAULT 8,
                            TotalMins REAL DEFAULT 480,
                            CustomizedMins REAL DEFAULT 0,
                            UsedPoints INT DEFAULT 0,
                            BasicPoints INT DEFAULT 0,
                            UsedMins REAL DEFAULT 0,
                            AvailableMins REAL DEFAULT 480,
                            RegistDatetime DATETIME DEFAULT CURRENT_TIMESTAMP,
                            DeleteFlag BOOLEAN DEFAULT 0
                        );",
                        @"
                        CREATE TABLE IF NOT EXISTS DurationLevel (
                            DurationLevelName TEXT PRIMARY KEY,
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
                        INSERT OR IGNORE INTO DurationLevel (DurationLevelName, Points) VALUES
                            ('Tiny', 1),
                            ('Small', 2),
                            ('Medium', 3),
                            ('Large', 4),
                            ('Huge', 5),
                            ('-Customize-', 0)
                        ;",
                        @"
                        INSERT OR IGNORE INTO Unit (UnitName) VALUES
                            ('IMD'),
                            ('FA'),
                            ('APP'),
                            ('MECT'),
                            ('METRO'),
                            ('CSO'),
                            ('CSM'),
                            ('AC'),
                            ('AR'),
                            ('PCD')
                        ;",
                    };

                    foreach (var query in insertDataQuery)
                    {
                        connection.Execute(query);
                    }

                    // 創建觸發器
                    var createTriggerQuery = @"
                        CREATE TRIGGER IF NOT EXISTS update_TaskHeader_Values
                        AFTER UPDATE OF OverHours, CustomizedMins, UsedPoints ON TaskHeader
                        FOR EACH ROW
                        BEGIN
                            UPDATE TaskHeader
                            SET 
                                TotalHours = NEW.OverHours + 8,
                                TotalMins = (NEW.OverHours + 8) * 60,
                                BasicPoints = (TotalMins - NEW.CustomizedMins) / NULLIF(NEW.UsedPoints, 0),
                                UsedMins = NEW.UsedPoints * (TotalMins - NEW.CustomizedMins) / NULLIF(NEW.UsedPoints, 0),
                                AvailableMins = TotalMins - NEW.CustomizedMins - NEW.UsedPoints * (TotalMins - NEW.CustomizedMins) / NULLIF(NEW.UsedPoints, 0)
                            WHERE TaskDate = NEW.TaskDate;
                        END;
                    ";

                    connection.Execute(createTriggerQuery);

                    Debug.WriteLine("所有資料表和觸發器已創建或已存在。");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"初始化資料庫時發生錯誤: {ex.Message}");
            }
        }
    }
}
