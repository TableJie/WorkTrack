using System;

namespace WorkTrack
{
    public static class UpdateService
    {
        // 定義一個靜態事件，用於通知主窗口更新
        public static event Action<DateTime> DataUpdated;

        // 定義一個靜態方法，用於觸發更新
        public static void NotifyDataUpdated(DateTime date)
        {
            DataUpdated?.Invoke(date);
        }

        // 可選：添加一個防抖動機制
        private static System.Timers.Timer debounceTimer;
        private static DateTime lastUpdateDate;

        public static void NotifyDataUpdatedWithDebounce(DateTime date)
        {
            lastUpdateDate = date;

            if (debounceTimer == null)
            {
                debounceTimer = new System.Timers.Timer(300); // 300毫秒的延遲
                debounceTimer.Elapsed += (sender, e) =>
                {
                    DataUpdated?.Invoke(lastUpdateDate);
                    debounceTimer.Stop();
                };
            }

            debounceTimer.Stop();
            debounceTimer.Start();
        }

        // 可選：添加一個方法來清理資源
        public static void Cleanup()
        {
            debounceTimer?.Dispose();
            debounceTimer = null;
        }
    }
}