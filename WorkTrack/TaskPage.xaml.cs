using System.Windows.Controls;
using WorkTrack.ViewModel;

namespace WorkTrack
{
    public partial class TaskPage : Page
    {
        public TaskPage()
        {
            InitializeComponent();
            this.DataContext = new WorkTrack.ViewModel.TaskViewModel();
        }
    }
}