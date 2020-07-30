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
    /// Interaction logic for TaskListPage.xaml
    /// </summary>
    public partial class TaskListPage : Page
    {
        public TaskListPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((TaskListPageViewModel)this.DataContext).OnPageLoaded();
        }
    }
}
