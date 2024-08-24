using Serilog;

namespace WorkTrack
{
    public static class LogConfiguration
    {
        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // 設定為 Information 級別
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7) // 保留最近 7 天的日誌
                .CreateLogger();
        }
    }
}