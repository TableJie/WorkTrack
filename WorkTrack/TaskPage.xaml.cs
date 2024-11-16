using System.Windows.Controls;
using WorkTrack.ViewModel;

namespace WorkTrack
{
    public partial class TaskPage : Page
    {
        public TaskPage(TaskViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}