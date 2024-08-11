using System.Configuration;
using System.Data;
using System.Windows;

namespace WorkTrack
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string ConnectionString = "Data Source=Database/app.db";
    }

}
