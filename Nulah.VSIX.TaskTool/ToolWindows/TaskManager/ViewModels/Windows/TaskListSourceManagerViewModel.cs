using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Nulah.VSIX.TaskTool.StandardLib;
using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Windows
{
    public class TaskListSourceManagerViewModel : ViewModelBase
    {
        private List<ProjectListViewItem> _solutionProjects;

        public List<ProjectListViewItem> SolutionProjects
        {
            get { return _solutionProjects; }
            set { _solutionProjects = value; OnPropertyChanged(nameof(SolutionProjects)); }
        }

        private readonly TaskListManager _taskListManager;


        public TaskListSourceManagerViewModel()
        {
            _taskListManager = GetDependency<TaskListManager>();

            if (WPFHelpers.IsDesignTime)
            {
                SolutionProjects = new List<ProjectListViewItem>() {
                    new ProjectListViewItem(CreateTaskDatabase, DeleteTaskDatabase, OpenProjectLocation)
                    {
                        FilePath = "asdf",
                        Name = "test name 1"
                    },
                    new ProjectListViewItem(CreateTaskDatabase, DeleteTaskDatabase, OpenProjectLocation)
                    {
                        FilePath = "asdf",
                        Name = "test name 2"
                    },
                    new ProjectListViewItem(CreateTaskDatabase, DeleteTaskDatabase, OpenProjectLocation)
                    {
                        FilePath = "asdf",
                        Name = "test name 3"
                    }
                };
                return;
            }

            LoadSolutionProjects();
        }

        private void LoadSolutionProjects()
        {
            SolutionProjects = _taskListManager.GetProjectsForOpenSolution()
                .Select(x => new ProjectListViewItem(CreateTaskDatabase, DeleteTaskDatabase, OpenProjectLocation)
                {
                    Database = x.Database,
                    FilePath = x.FilePath,
                    Kind = x.Kind,
                    Name = x.Name,
                    ParentDirectory = x.ParentDirectory
                })
                .ToList();
        }

        private void CreateTaskDatabase(SolutionProject solutionProject)
        {
            var projectDatabase = _taskListManager.CreateProjectDatabase(solutionProject.Name, solutionProject.ParentDirectory);
            if (projectDatabase == true)
            {
                // Refresh the loaded list to reflect the update because I'm too lazy
                // to wire up all the annoying stuff to update in place
                LoadSolutionProjects();
            }
        }

        private void DeleteTaskDatabase(SolutionProject solutionProject)
        {
            throw new NotImplementedException();
        }

        private void OpenProjectLocation(SolutionProject solutionProject)
        {
            Process.Start(solutionProject.ParentDirectory);
        }
    }

    public class ProjectListViewItem : SolutionProject
    {

        public ICommand CreateTaskDatabaseCommand { get; private set; }
        public ICommand DeleteTaskDatabaseCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }

        public bool NoDatabase => Database == null;
        public bool HasDatabase => false; // Database != null;

        public ProjectListViewItem(Action<SolutionProject> createTaskDatabase, Action<SolutionProject> deleteTaskDatabase, Action<SolutionProject> openProjectLocation)
        {
            CreateTaskDatabaseCommand = new RelayCommand((x) => createTaskDatabase(this), x => true);
            DeleteTaskDatabaseCommand = new RelayCommand((x) => deleteTaskDatabase(this), x => true);
            OpenProjectCommand = new RelayCommand((x) => openProjectLocation(this), x => true);
        }
    }

    public class IsLastItemInContainerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (DependencyObject)value;
            var ic = ItemsControl.ItemsControlFromItemContainer(item);

            return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
