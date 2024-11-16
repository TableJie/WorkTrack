using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Windows;
using WorkTrack.Services;
using WorkTrack.ViewModel;
using WorkTrack.Interfaces;
using WorkTrack.Service;

namespace WorkTrack
{
    public partial class App : Application
    {
        private ServiceProvider? serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureServices();
            if (serviceProvider != null)
            {
                var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
                if (mainWindow != null)
                {
                    mainWindow.Show();
                }
                else
                {
                    MessageBox.Show("無法創建主視窗。應用程序將關閉。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                }
            }
            else
            {
                MessageBox.Show("服務提供者初始化失敗。應用程序將關閉。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // 配置 Serilog
            var logger = new LoggerConfiguration()
                .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<DbFactory>();
            services.AddSingleton<IDbFactory>(sp => sp.GetRequiredService<DbFactory>());
            services.AddSingleton<IInitializer>(sp => sp.GetRequiredService<DbFactory>());
            services.AddSingleton<ITaskService, TaskService>();
            services.AddSingleton<IChartService, ChartService>();
            services.AddTransient<TaskViewModel>();
            services.AddTransient<TaskPage>();
            services.AddTransient<MainWindow>();
            services.AddTransient<OverTimeInput>();
            services.AddTransient<TaskInput>();
            services.AddTransient<UnitInput>();

            serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            serviceProvider?.Dispose();
        }
    }
}