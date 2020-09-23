using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using EnvDTE;

using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Models;
using System.IO;

namespace Nulah.VSIX.TaskTool.StandardLib
{
    public class VisualStudioInternals
    {
        /// <summary>
        /// Returns an instance of the current visual studio IDE
        /// </summary>
        /// <returns></returns>
        public DTE2 GetActiveIDE()
        {
            // Get an instance of currently running Visual Studio IDE.
            DTE2 dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            return dte2;
        }

        /// <summary>
        /// Returns a list
        /// </summary>
        /// <returns></returns>
        public List<SolutionProject> GetProjectsForSolution()
        {
            //_projects = new List<SolutionProject>();
            ThreadHelper.ThrowIfNotOnUIThread();

            Projects solutionProjects = GetActiveIDE().Solution.Projects;
            List<Project> foundProjects = new List<Project>();

            var item = solutionProjects.GetEnumerator();
            // Interate over all project COM objects
            while (item.MoveNext())
            {
                var project = item.Current as Project;
                if (project == null)
                {
                    continue;
                }
                // Recurse down project folders in the solution
                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    foundProjects.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    foundProjects.Add(project);
                }
            }

            // Trim project details to whats relevant
            var projects = new List<SolutionProject>();
            foreach (Project proj in foundProjects)
            {
                projects.Add(new SolutionProject
                {
                    Name = proj.Name,
                    FilePath = proj.FileName,
                    ParentDirectory = new FileInfo(proj.FullName).Directory.FullName,
                    Kind = proj.Kind
                });
            }

            return projects;
        }

        /// <summary>
        /// Returns a list of projects that may be nested one or more project solution folders deep 
        /// </summary>
        /// <param name="solutionFolder"></param>
        /// <returns></returns>
        private List<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var list = new List<Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                {
                    continue;
                }

                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    // If this is another solution folder, drill down and get projects
                    list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    list.Add(subProject);
                }
            }
            return list;
        }
    }

    public class SolutionProject
    {
        /// <summary>
        /// Project name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Project location on disk
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// Also project location on disk, but may differ
        /// </summary>
        public string ParentDirectory { get; set; }
        public string Kind { get; set; }
        public NulahDBMeta Database { get; set; }
    }
}
