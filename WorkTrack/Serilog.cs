using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;

namespace WorkTrack
{
    public static class LogConfiguration
    {
        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // 設定為 Information 級別
                .WriteTo.Console() // 將日誌輸出到控制台
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7) // 保留最近 7 天的日誌
                .CreateLogger();
        }
    }

    public class ConsoleSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            Console.WriteLine(logEvent.RenderMessage());
        }
    }
}
