using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            LoadSolutionProjects();

        }

        private void LoadSolutionProjects()
        {
            SolutionProjects = _taskListManager.GetProjectsForOpenSolution()
                .Select(x => new ProjectListViewItem(CreateTaskDatabase, OpenProjectLocation)
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

        private void OpenProjectLocation(SolutionProject solutionProject)
        {
            Process.Start(solutionProject.ParentDirectory);
        }
    }

    public class ProjectListViewItem : SolutionProject
    {

        public ICommand CreateTaskDatabaseCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }

        public ProjectListViewItem(Action<SolutionProject> createTaskDatabase, Action<SolutionProject> openProjectLocation)
        {
            CreateTaskDatabaseCommand = new RelayCommand((x) => createTaskDatabase(this), x => true);
            OpenProjectCommand = new RelayCommand((x) => openProjectLocation(this), x => true);
        }
    }
}
