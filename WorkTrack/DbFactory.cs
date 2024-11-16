using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Serilog;
using WorkTrack.Interfaces;



namespace WorkTrack.Services
{
    public class DbFactory : IDbFactory, IInitializer
    {
        private readonly ILogger _logger;
        private readonly string _databasePath = "Database/app.db";

        public DbFactory(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), _databasePath);
            var connection = new SqliteConnection($"Data Source={fullPath}");
            await connection.OpenAsync();
            return connection;
        }

        public void Initialize()
        {
            try
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), _databasePath);

                if (!File.Exists(fullPath))
                {
                    string directoryPath = Path.GetDirectoryName(fullPath);
                    if (directoryPath != null)
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    _logger.Information("創建資料庫路徑: {FullPath}", fullPath);

                    using (var connection = new SqliteConnection($"Data Source={fullPath};"))
                    {
                        connection.Open();
                        CreateTables(connection);
                        InsertInitialData(connection);
                        CreateTriggers(connection);
                    }
                    _logger.Information("資料庫結構初始化完成。");
                }

                // 無論是否新建資料庫，都插入今日日期到 TaskHeader
                using (var connection = new SqliteConnection($"Data Source={fullPath};"))
                {
                    connection.Open();
                    var insertTodayDateQuery = @"
                        INSERT OR IGNORE INTO TaskHeader (TaskDate) VALUES (@TaskDate)
                    ";
                    connection.Execute(insertTodayDateQuery, new { TaskDate = DateTime.Today.ToString("yyyy-MM-dd") });
                    _logger.Information("今日日期登入完成。");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "初始化資料庫時發生錯誤");
                throw;
            }
        }

        private void CreateTables(SqliteConnection connection)
        {
            var createTableQueries = new[]
            {
                @"
                CREATE TABLE IF NOT EXISTS TaskBody (
                    TaskID INTEGER PRIMARY KEY AUTOINCREMENT,
                    TaskDate DATE NOT NULL,
                    TaskName TEXT NOT NULL,
                    DurationLevelID NUMERIC,
                    Duration NUMERIC,
                    Description TEXT,
                    UnitID NUMERIC,
                    ApplicationID NUMERIC,
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
                    UnitID NUMERIC,
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
                    YearMonthSequence NUMERIC NOT NULL,        
                    YearQuarterSequence NUMERIC NOT NULL,      
                    YearHalfSequence NUMERIC NOT NULL,         
                    WorkDayFlag BOOLEAN NOT NULL,              
                    WeeklySequenceMonthly NUMERIC NOT NULL     
                );"
            };

            foreach (var query in createTableQueries)
            {
                connection.Execute(query);
            }
            _logger.Information("所有資料表已創建。");
        }

        private void InsertInitialData(SqliteConnection connection)
        {
            var insertDataQueries = new[]
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
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) >= 3 
                            THEN STRFTIME('%Y', CalendarDate)
                            ELSE STRFTIME('%Y', DATE(CalendarDate, '-1 year'))
                        END AS Year,
                        CASE 
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) >= 3 
                            THEN STRFTIME('%Y', CalendarDate) || '/' || STRFTIME('%m', CalendarDate)
                            ELSE STRFTIME('%Y', DATE(CalendarDate, '-1 year')) || '/' || STRFTIME('%m', CalendarDate)
                        END AS YearMonth,
                        CASE 
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) BETWEEN 3 AND 8 THEN STRFTIME('%Y', CalendarDate) || '-1H'
                            ELSE STRFTIME('%Y', CASE WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) >= 3 THEN CalendarDate ELSE DATE(CalendarDate, '-1 year') END) || '-2H'
                        END AS YearHalf,
                        CASE 
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) BETWEEN 3 AND 5 THEN STRFTIME('%Y', CalendarDate) || '-1Q'
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) BETWEEN 6 AND 8 THEN STRFTIME('%Y', CalendarDate) || '-2Q'
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) BETWEEN 9 AND 11 THEN STRFTIME('%Y', CalendarDate) || '-3Q'
                            ELSE STRFTIME('%Y', CASE WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) >= 3 THEN CalendarDate ELSE DATE(CalendarDate, '-1 year') END) || '-4Q'
                        END AS YearQuarter,
                        ROW_NUMBER() OVER (ORDER BY CalendarDate) AS YearMonthSequence,
                        ROW_NUMBER() OVER (PARTITION BY CASE 
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) >= 3 
                            THEN STRFTIME('%Y', CalendarDate)
                            ELSE STRFTIME('%Y', DATE(CalendarDate, '-1 year'))
                        END, CASE 
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) BETWEEN 3 AND 5 THEN 1
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) BETWEEN 6 AND 8 THEN 2
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) BETWEEN 9 AND 11 THEN 3
                            ELSE 4
                        END ORDER BY CalendarDate) AS YearQuarterSequence,
                        ROW_NUMBER() OVER (PARTITION BY CASE 
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) >= 3 
                            THEN STRFTIME('%Y', CalendarDate)
                            ELSE STRFTIME('%Y', DATE(CalendarDate, '-1年'))
                        END, CASE 
                            WHEN CAST(STRFTIME('%m', CalendarDate) AS NUMERIC) BETWEEN 3 AND 8 THEN 1
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

            foreach (var query in insertDataQueries)
            {
                connection.Execute(query);
            }
            _logger.Information("初始資料已插入。");
        }

        private void CreateTriggers(SqliteConnection connection)
        {
            var createTriggerQueries = new[]
            {
                            @"
                                CREATE TRIGGER insert_TaskHeader_onTaskBody
                                AFTER INSERT ON TaskBody
                                FOR EACH ROW
                                BEGIN
                                    -- 更新 UsedPoints 和 CustomizedMins
                                    UPDATE TaskHeader
                                    SET 
                                        UsedPoints = (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ),
                                        CustomizedMins = (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )
                                    WHERE TaskDate = NEW.TaskDate;

                                    -- 更新 BasicPoints, UsedMins 和 AvailableMins
                                    UPDATE TaskHeader
                                    SET 
                                        BasicPoints = COALESCE((TotalMins -  (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )) / NULLIF((
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0), 0),
                                        UsedMins = ROUND(COALESCE((TotalMins -  (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )) / NULLIF((
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0), 0) * (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0),
                                        AvailableMins = TotalMins - ROUND(COALESCE((TotalMins -  (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )) / NULLIF((
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0), 0) * (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0) - (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )
                                    WHERE TaskDate = NEW.TaskDate;

                                    -- 更新 TaskBody 中 Duration
                                    UPDATE TaskBody
                                    SET Duration = round(DurationLevelID * (SELECT BasicPoints FROM TaskHeader WHERE TaskDate = NEW.TaskDate),0)
                                    WHERE TaskDate = NEW.TaskDate AND DurationLevelID != 9;
                                END;
                            "
                            ,@"
                                CREATE TRIGGER update_TaskHeader_onTaskBody
                                AFTER UPDATE ON TaskBody
                                FOR EACH ROW
                                BEGIN
                                    -- 更新 UsedPoints 和 CustomizedMins
                                    UPDATE TaskHeader
                                    SET 
                                        UsedPoints = (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ),
                                        CustomizedMins = (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )
                                    WHERE TaskDate = NEW.TaskDate;

                                    -- 更新 BasicPoints, UsedMins 和 AvailableMins
                                    UPDATE TaskHeader
                                    SET 
                                        BasicPoints = COALESCE((TotalMins -  (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )) / NULLIF((
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0), 0),
                                        UsedMins = ROUND(COALESCE((TotalMins -  (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )) / NULLIF((
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0), 0) * (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0),
                                        AvailableMins = TotalMins - ROUND(COALESCE((TotalMins -  (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )) / NULLIF((
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0), 0) * (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID != 9 THEN DurationLevelID END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        ), 0) - (
                                            SELECT COALESCE(SUM(CASE WHEN DurationLevelID = 9 THEN Duration END), 0)
                                            FROM TaskBody
                                            WHERE TaskDate = NEW.TaskDate
                                        )
                                    WHERE TaskDate = NEW.TaskDate;

                                    -- 更新 TaskBody 中 Duration
                                    UPDATE TaskBody
                                    SET Duration = round(DurationLevelID * (SELECT BasicPoints FROM TaskHeader WHERE TaskDate = NEW.TaskDate),0)
                                    WHERE TaskDate = NEW.TaskDate AND DurationLevelID != 9;
                                END;
                            "
                            ,@"
                                CREATE TRIGGER insert_TaskHeader_onOverTime
                                AFTER INSERT ON OverTime
                                FOR EACH ROW
                                BEGIN
                                    UPDATE TaskHeader
                                    SET 
                                        OverHours = NEW.OverHours,
                                        TotalHours = 8 + NEW.OverHours,
                                        TotalMins = (8 + NEW.OverHours) * 60,

                                        BasicPoints = round(((8 + NEW.OverHours) * 60 - CustomizedMins) / UsedPoints ,0),
                                        UsedMins = UsedPoints * round(((8 + NEW.OverHours) * 60 - CustomizedMins) / UsedPoints ,0),

                                        AvailableMins = (8 + NEW.OverHours) * 60 -
                                                        UsedPoints * round(((8 + NEW.OverHours) * 60 - CustomizedMins) / UsedPoints ,0)

                                    WHERE TaskDate = NEW.TaskDate;

                                    -- 更新 TaskBody 中 Duration
                                    UPDATE TaskBody
                                    SET Duration = DurationLevelID * (SELECT round(((8 + NEW.OverHours) * 60 - CustomizedMins) / UsedPoints ,0) FROM TaskHeader WHERE TaskDate = NEW.TaskDate)
                                    WHERE TaskDate = NEW.TaskDate AND DurationLevelID != 9;
                                END;
                            "
                            ,@"
                                CREATE TRIGGER update_TaskHeader_onOverTime
                                AFTER UPDATE ON OverTime
                                FOR EACH ROW
                                BEGIN
                                    UPDATE TaskHeader
                                    SET 
                                        OverHours = NEW.OverHours,
                                        TotalHours = 8 + NEW.OverHours,
                                        TotalMins = (8 + NEW.OverHours) * 60,

                                        BasicPoints = round(((8 + NEW.OverHours) * 60 - CustomizedMins) / UsedPoints ,0),
                                        UsedMins = UsedPoints * round(((8 + NEW.OverHours) * 60 - CustomizedMins) / UsedPoints ,0),

                                        AvailableMins = (8 + NEW.OverHours) * 60 -
                                                        UsedPoints * round(((8 + NEW.OverHours) * 60 - CustomizedMins) / UsedPoints ,0)

                                    WHERE TaskDate = NEW.TaskDate;

                                    -- 更新 TaskBody 中 Duration
                                    UPDATE TaskBody
                                    SET Duration = DurationLevelID * (SELECT round(((8 + NEW.OverHours) * 60 - CustomizedMins) / UsedPoints ,0) FROM TaskHeader WHERE TaskDate = NEW.TaskDate)
                                    WHERE TaskDate = NEW.TaskDate AND DurationLevelID != 9;
                                END;
                            "

                    };

            foreach (var query in createTriggerQueries)
            {
                connection.Execute(query);
            }
            _logger.Information("所有觸發器已創建。");
        }
    }
}