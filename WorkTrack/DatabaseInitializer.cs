
using System.Diagnostics;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using Serilog;

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
                    // 創建資料庫和表格
                    string directoryPath = Path.GetDirectoryName(fullPath);
                    if (directoryPath != null)
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    Log.Information("創建資料庫路徑: {FullPath}", fullPath);


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
                                DurationLevelID INTEGER,
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
                            CREATE TABLE IF NOT EXISTS OverTime (
                                TaskDate DATE PRIMARY KEY,
                                OverHours REAL DEFAULT 0,
                                TaskPlan1 TEXT DEFAULT null,
                                TaskPlan2 TEXT DEFAULT null,
                                TaskPlan3 TEXT DEFAULT null,
                                TaskPlan4 TEXT DEFAULT null,
                                TaskPlan5 TEXT DEFAULT null,
                                TaskPlan6 TEXT DEFAULT null,
                                TaskPlan7 TEXT DEFAULT null,
                                TaskPlan8 TEXT DEFAULT null,
                                RegistDatetime DATETIME DEFAULT CURRENT_TIMESTAMP,
                                DeleteFlag BOOLEAN DEFAULT 0
                            );",
                            @"
                            CREATE TABLE IF NOT EXISTS DurationLevel (
                                DurationLevelID INTEGER PRIMARY KEY,
                                DurationLevelName TEXT
                            );",
                            @"
                            CREATE TABLE IF NOT EXISTS Calendar (
                                CalendarDate DATE PRIMARY KEY,            
                                Year TEXT NOT NULL,                       
                                YearMonth TEXT NOT NULL,                   
                                YearHalf TEXT NOT NULL,                    
                                YearQuarter TEXT NOT NULL,                 
                                YearMonthSequence INTEGER NOT NULL,        
                                YearQuarterSequence INTEGER NOT NULL,      
                                YearHalfSequence INTEGER NOT NULL,         
                                WorkDayFlag BOOLEAN NOT NULL,              
                                WeeklySequenceMonthly INTEGER NOT NULL     
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
                            INSERT OR IGNORE INTO DurationLevel (DurationLevelID, DurationLevelName) VALUES
                                (1,'Tiny'),
                                (2,'Small'),
                                (3,'Medium'),
                                (4,'Large'),
                                (5,'Huge'),
                                (9,'-Customize-')
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
                            @"
                            WITH RECURSIVE CalendarGenerator AS (
                                SELECT DATE('2024-03-01') AS CalendarDate
                                UNION ALL
                                -- 遞歸產生每一天的日期
                                SELECT DATE(CalendarDate, '+1 day')
                                FROM CalendarGenerator
                                WHERE CalendarDate < DATE('2030-02-28')
                            ),CalendarData AS (
                                SELECT 
                                    CalendarDate,
                                    CASE 
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) >= 3 
                                        THEN STRFTIME('%Y', CalendarDate)
                                        ELSE STRFTIME('%Y', DATE(CalendarDate, '-1 year'))
                                    END AS Year,
                                    CASE 
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) >= 3 
                                        THEN STRFTIME('%Y', CalendarDate) || '/' || STRFTIME('%m', CalendarDate)
                                        ELSE STRFTIME('%Y', DATE(CalendarDate, '-1 year')) || '/' || STRFTIME('%m', CalendarDate)
                                    END AS YearMonth,
                                    CASE 
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) BETWEEN 3 AND 8 THEN STRFTIME('%Y', CalendarDate) || '-1H'
                                        ELSE STRFTIME('%Y', CASE WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) >= 3 THEN CalendarDate ELSE DATE(CalendarDate, '-1 year') END) || '-2H'
                                    END AS YearHalf,
                                    CASE 
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) BETWEEN 3 AND 5 THEN STRFTIME('%Y', CalendarDate) || '-1Q'
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) BETWEEN 6 AND 8 THEN STRFTIME('%Y', CalendarDate) || '-2Q'
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) BETWEEN 9 AND 11 THEN STRFTIME('%Y', CalendarDate) || '-3Q'
                                        ELSE STRFTIME('%Y', CASE WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) >= 3 THEN CalendarDate ELSE DATE(CalendarDate, '-1 year') END) || '-4Q'
                                    END AS YearQuarter,
                                    ROW_NUMBER() OVER (ORDER BY CalendarDate) AS YearMonthSequence,
                                    ROW_NUMBER() OVER (PARTITION BY CASE 
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) >= 3 
                                        THEN STRFTIME('%Y', CalendarDate)
                                        ELSE STRFTIME('%Y', DATE(CalendarDate, '-1 year'))
                                    END, CASE 
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) BETWEEN 3 AND 5 THEN 1
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) BETWEEN 6 AND 8 THEN 2
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) BETWEEN 9 AND 11 THEN 3
                                        ELSE 4
                                    END ORDER BY CalendarDate) AS YearQuarterSequence,
                                    ROW_NUMBER() OVER (PARTITION BY CASE 
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) >= 3 
                                        THEN STRFTIME('%Y', CalendarDate)
                                        ELSE STRFTIME('%Y', DATE(CalendarDate, '-1年'))
                                    END, CASE 
                                        WHEN CAST(STRFTIME('%m', CalendarDate) AS INTEGER) BETWEEN 3 AND 8 THEN 1
                                        ELSE 2
                                    END ORDER BY CalendarDate) AS YearHalfSequence,
                                    CASE 
                                        WHEN STRFTIME('%w', CalendarDate) IN ('0', '6') THEN False -- 週日(0)和週六(6)視為非工作日
                                        ELSE True -- 週一到週五視為工作日
                                    END AS WorkDayFlag,
                                    ROW_NUMBER() OVER (PARTITION BY STRFTIME('%Y-%W', CalendarDate) ORDER BY CalendarDate) AS WeeklySequenceMonthly
                                FROM 
                                    CalendarGenerator
                            )
                            INSERT INTO Calendar
                            SELECT *
                            FROM CalendarData
                            ;",
                        };

                        foreach (var query in insertDataQuery)
                        {
                            connection.Execute(query);
                        }


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
                        Debug.WriteLine("所有資料表和觸發器已創建。");
                    }
                }


                // 如果資料庫存在，或者剛剛創建完畢，插入今日的日期到 TaskHeader
                using (var connection = new SqliteConnection($"Data Source={fullPath};"))
                {
                    connection.Open();

                    var insertTodayDateQuery = @"
                        INSERT OR IGNORE INTO TaskHeader (TaskDate) VALUES
                        (@TaskDate)
                    ";

                    connection.Execute(insertTodayDateQuery, new { TaskDate = DateTime.Today });
                    Log.Information("今日日期登入完成。");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化資料庫時發生錯誤");
            }
        }
    }
}