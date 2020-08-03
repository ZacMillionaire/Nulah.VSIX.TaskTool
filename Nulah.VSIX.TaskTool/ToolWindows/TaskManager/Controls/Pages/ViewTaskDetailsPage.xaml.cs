using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Pages;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls.Pages
{
    /// <summary>
    /// Interaction logic for ViewTaskDetailsPage.xaml
    /// </summary>
    public partial class ViewTaskDetailsPage : Page
    {
        public ViewTaskDetailsPage()
        {
            InitializeComponent();
        }
        private Guid _taskGuid { get; set; }

        public ViewTaskDetailsPage(Guid taskGuid) : this()
        {
            _taskGuid = taskGuid;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Page class events can't be async Task")]
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ((ViewTaskDetailsViewModel)DataContext).LoadTaskAsync(_taskGuid);
        }
    }
}
