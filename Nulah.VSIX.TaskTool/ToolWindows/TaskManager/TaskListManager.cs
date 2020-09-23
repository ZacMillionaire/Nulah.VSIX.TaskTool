using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using VSSHell = Microsoft.VisualStudio.Shell;

using Nulah.VSIX.TaskTool.Data;
using Nulah.VSIX.TaskTool.StandardLib;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Models;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Models.Tasks;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager
{
    public class TaskListManager
    {
        private readonly SqliteProvider _sqliteProvider;
        private const string GLOBAL_DB_DATASOURCE_NAME = "GLOBAL";
        private const string APP_SETTINGS_DB_DATASOURCE_NAME = "APPSETTINGS";
        private const string NULAH_DB_EXTENSION = "nulahdb";

        private List<Task> _currentTaskList { get; set; }
        public List<DatabaseSource> AvailableTaskLists { get; private set; }
        /// <summary>
        /// Internal tracking for currently known task databases
        /// </summary>
        private Dictionary<string, DatabaseSource> _taskDatabases { get; set; }
        /// <summary>
        /// All currently loaded tasks, key is the display name for the database
        /// </summary>
        private List<Task> _loadedTasks { get; set; }
        public bool ListChanged { get; private set; }

        private DatabaseSource _currentTaskDatabase;

        public bool IsGlobalDatabaseInUse;

        /// <summary>
        /// %APPDATA%/Nulah
        /// </summary>
        private string _applicationDataLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nulah");

        /// <summary>
        /// Starts a new TaskListManager and defaults to the global task list, and loads all tasks in the database
        /// </summary>
        public TaskListManager()
        {
            _sqliteProvider = new SqliteProvider();
            CreateAppDataFolder();
            CreateAppDatabases();

            // Get databases for loaded solution and projects - if a solution/project is open
            GetDatabaseList();

            // Default to using Global database for now
            SwitchDatabase(_taskDatabases[GLOBAL_DB_DATASOURCE_NAME]);
        }

        /// <summary>
        /// Returns all projects for the loaded solution
        /// </summary>
        /// <returns></returns>
        public List<SolutionProject> GetProjectsForOpenSolution()
        {
            VSSHell.ThreadHelper.ThrowIfNotOnUIThread();
            return GetTaskListsForOpenSolution();
        }

        /// <summary>
        /// Returns all solutions for the given project, populating db metadata if a valid task file is found
        /// </summary>
        /// <returns></returns>
        private List<SolutionProject> GetTaskListsForOpenSolution()
        {
            // Any access to the VS Shell or internals should only be done on the main UI thread
            VSSHell.ThreadHelper.ThrowIfNotOnUIThread();
            var VSI = new VisualStudioInternals();
            var projects = VSI.GetProjectsForSolution();

            foreach (var project in projects)
            {
                GetTaskDatabaseForProject(project);
            }

            return projects;
        }

        private void GetTaskDatabaseForProject(SolutionProject solutionProject)
        {
            var fi = new FileInfo(solutionProject.FilePath);
            var nulahTaskFiles = fi.Directory.EnumerateFiles()
                .FirstOrDefault(x => x.Extension == $".{NULAH_DB_EXTENSION}");

            if (nulahTaskFiles == null)
            {
                return;
            }

            if (_sqliteProvider.DataSourceExists(nulahTaskFiles.Name) == false)
            {
                var extensionlessFileName = Path.GetFileNameWithoutExtension(nulahTaskFiles.Name);
                _sqliteProvider.CreateOrRegisterDataSource(extensionlessFileName, nulahTaskFiles.FullName);
                var dbSchema = _sqliteProvider.Query<NulahDBMeta>(extensionlessFileName, $"SELECT * FROM [{nameof(NulahDBMeta)}] LIMIT 1");

                if (dbSchema.FirstOrDefault() != null)
                {
                    solutionProject.Database = dbSchema.First();
                }
                else
                {
                    // In the unlikely event that the task database isn't valid, unregister it
                    _sqliteProvider.UnregisterDatasource(nulahTaskFiles.Name);
                }
            }
        }

        /// <summary>
        /// Gets all valid task databases, and updates <see cref="AvailableTaskLists"/> with all found
        /// </summary>
        /// <returns></returns>
        public List<DatabaseSource> GetDatabaseList()
        {
            _taskDatabases = new Dictionary<string, DatabaseSource>
            {
                {
                    GLOBAL_DB_DATASOURCE_NAME,
                    new DatabaseSource{
                        DisplayName = "Global",
                        DatabaseName = GLOBAL_DB_DATASOURCE_NAME,
                        Location = Path.Combine(_applicationDataLocation, $"Nulah.TaskList.{NULAH_DB_EXTENSION}")
                    }
                },
            };

            var taskLists = GetTaskListsForOpenSolution();

            foreach (var taskList in taskLists)
            {
                if (taskList.Database != null)
                {
                    _taskDatabases.Add(taskList.Database.TaskListName, new DatabaseSource
                    {
                        DatabaseName = taskList.Database.ProjectName,
                        DisplayName = taskList.Database.TaskListName,
                        Location = Path.Combine(taskList.ParentDirectory, $"{taskList.Database.ProjectName}.{NULAH_DB_EXTENSION}")
                    });
                }
            }

            // Update the available task list
            AvailableTaskLists = _taskDatabases.Values.ToList();

            return AvailableTaskLists;
        }

        /// <summary>
        /// Create our extensions application data directory
        /// </summary>
        private void CreateAppDataFolder()
        {
            if (Directory.Exists(_applicationDataLocation) == false)
            {
                Directory.CreateDirectory(_applicationDataLocation);
            }
        }

        /// <summary>
        /// Create first time databases for the extension: application for tracking created task databases, and the global task list
        /// database
        /// </summary>
        private void CreateAppDatabases()
        {
            CreateApplicationSettingsDatabase();
            CreateAppDataDatabase(GLOBAL_DB_DATASOURCE_NAME, "Nulah.TaskList", new NulahDBMeta
            {
                TaskListName = "Nulah.TaskList"
            });
        }

        /// <summary>
        /// Creates a task database in the given location, if a project database already exists, false is returned
        /// <para>Regardless of database creation, the datasource will be registered and available</para>
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="projectLocation"></param>
        public bool CreateProjectDatabase(string projectName, string projectLocation)
        {
            var databaseLocation = Path.Combine(projectLocation, $"{projectName}.{NULAH_DB_EXTENSION}");
            var databaseCreated = _sqliteProvider.CreateOrRegisterDataSource(projectName, databaseLocation);
            // Track the new task list database in the application settings database if this is the first time we're creating it
            if (databaseCreated == true)
            {
                _sqliteProvider.CreateTable<Task>(projectName);

                // Create metadata
                _sqliteProvider.CreateTable<NulahDBMeta>(projectName);
                var nulahDBMetaPropertyList = NulahStandardLib.GetPropertiesForType<NulahDBMeta>();
                var dbMetadata = new NulahDBMeta
                {
                    IsProjectDatabase = true,
                    ProjectName = projectName,
                    ProjectOriginalLocation = projectLocation,
                    TaskListName = projectName // TODO: Maybe add override to this later for project task lists?
                };

                _sqliteProvider.Insert<NulahDBMeta>(projectName,
                    $"INSERT INTO [{nameof(NulahDBMeta)}] ({string.Join(", ", nulahDBMetaPropertyList.Select(x => $"[{ x.Name}]"))}) VALUES ({string.Join(",", nulahDBMetaPropertyList.Select(x => $"@{x.Name}"))})",
                    dbMetadata
                );

                CreateDatabaseEntry(databaseLocation);
            }

            return databaseCreated;
        }

        /// <summary>
        /// Create a database in the users %appdata%
        /// </summary>
        /// <param name="datasourceName"></param>
        /// <param name="databaseName"></param>
        public void CreateAppDataDatabase(string datasourceName, string databaseName, NulahDBMeta dbMetadata)
        {
            var databaseLocation = Path.Combine(_applicationDataLocation, $"{databaseName}.{NULAH_DB_EXTENSION}");

            var databaseCreated = _sqliteProvider.CreateOrRegisterDataSource(datasourceName, databaseLocation);
            // Track the new task list database in the application settings database if this is the first time we're creating it
            if (databaseCreated == true)
            {
                _sqliteProvider.CreateTable<Task>(datasourceName);

                // Create metadata
                _sqliteProvider.CreateTable<NulahDBMeta>(datasourceName);
                var nulahDBMetaPropertyList = NulahStandardLib.GetPropertiesForType<NulahDBMeta>();
                _sqliteProvider.Insert<NulahDBMeta>(datasourceName,
                    $"INSERT INTO [{nameof(NulahDBMeta)}] ({string.Join(", ", nulahDBMetaPropertyList.Select(x => $"[{ x.Name}]"))}) VALUES ({string.Join(",", nulahDBMetaPropertyList.Select(x => $"@{x.Name}"))})",
                    dbMetadata
                );

                CreateDatabaseEntry(databaseLocation);
            }
        }

        /// <summary>
        /// Creates the database responsible for tracking all created databases, along with any other setting tables
        /// </summary>
        private void CreateApplicationSettingsDatabase()
        {
            var applicationDb = Path.Combine(_applicationDataLocation, $"Nulah.Settings.{NULAH_DB_EXTENSION}");
            _sqliteProvider.CreateOrRegisterDataSource(APP_SETTINGS_DB_DATASOURCE_NAME, applicationDb);

            _sqliteProvider.CreateTable<Database>(APP_SETTINGS_DB_DATASOURCE_NAME);
        }
        /// <summary>
        /// Creates a record of a created nulah database location given in the global app settings database
        /// </summary>
        /// <param name="databaseLocation"></param>
        public void CreateDatabaseEntry(string databaseLocation)
        {
            var appSettingsPropertyList = NulahStandardLib.GetPropertiesForType<Database>();
            _sqliteProvider.Insert<Database>(APP_SETTINGS_DB_DATASOURCE_NAME,
                $"INSERT INTO [{nameof(Database)}] ({string.Join(", ", appSettingsPropertyList.Select(x => $"[{ x.Name}]"))}) VALUES ({string.Join(",", appSettingsPropertyList.Select(x => $"@{x.Name}"))})",
                new Database
                {
                    Id = Guid.NewGuid(),
                    CreatedUTC = NulahStandardLib.DateTimeNow(),
                    LastKnownLocation = databaseLocation
                });
        }

        /// <summary>
        /// Change the active database for tasks to the new database, and loads all tasks stored
        /// </summary>
        /// <param name="newDatabase"></param>
        public void SwitchDatabase(DatabaseSource newDatabase)
        {
            if (_sqliteProvider.DataSourceExists(newDatabase.DatabaseName))
            {
                IsGlobalDatabaseInUse = newDatabase.DatabaseName == GLOBAL_DB_DATASOURCE_NAME;
                _currentTaskDatabase = newDatabase;
                LoadTasksForDatabase(newDatabase);
            }
            else
            {
                throw new Exception($"{newDatabase} does not exist");
            }
        }

        public string GetCurrentDatabase()
        {
            if (IsGlobalDatabaseInUse == true)
            {
                return "Global";
            }
            return $"Solution - {_currentTaskDatabase.DisplayName}";
        }

        /// <summary>
        /// Loads all tasks from the sqlite database by its given source, ordered by task creation descending by default
        /// </summary>
        /// <param name="databaseSourceKey"></param>
        private void LoadTasksForDatabase(DatabaseSource databaseSourceKey)
        {
            // Horribly track if the task list is dirty and that the UI should reload to get latest changes
            ListChanged = true;
            _loadedTasks = _sqliteProvider.Query<Task>(databaseSourceKey.DatabaseName, $"SELECT * FROM [{nameof(Task)}] ORDER BY [{nameof(Task.CreatedUTC)}] DESC");
        }

        /// <summary>
        /// Returns all tasks for the currently selected database, and clears the ListChanged flag
        /// </summary>
        /// <returns></returns>
        public List<Task> GetTasks()
        {
            ListChanged = false;
            return _loadedTasks;
        }

        /// <summary>
        /// Returns the full details of a task by the given task guid, or null if not found in the currently used database
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public Task GetTask(Guid taskId)
        {
            var taskById = _sqliteProvider.Query<Task>(_currentTaskDatabase.DatabaseName, $"SELECT * FROM [{nameof(Task)}] WHERE [{nameof(Task.Id)}] = @TaskGuid",
                new
                {
                    TaskGuid = taskId
                });

            return taskById.FirstOrDefault();
        }

        public bool CreateTask(string title, string content)
        {
            var taskProps = NulahStandardLib.GetPropertiesForType<Task>();
            var insert = _sqliteProvider.Insert<Task>(_currentTaskDatabase.DatabaseName,
                $"INSERT INTO [{nameof(Task)}] ({string.Join(", ", taskProps.Select(x => $"[{ x.Name}]"))}) VALUES ({string.Join(",", taskProps.Select(x => $"@{x.Name}"))})",
                new Task
                {
                    Content = content,
                    Title = title,
                    CreatedUTC = NulahStandardLib.DateTimeNow(),
                    Id = Guid.NewGuid()
                });

            LoadTasksForDatabase(_currentTaskDatabase);

            return insert;
        }

        /// <summary>
        /// Updates the progress state of the given task by id, and sets IsComplete to false
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public bool UpdateTaskProgressState(Guid taskId, bool progressState)
        {
            var update = _sqliteProvider.Update<Task>(_currentTaskDatabase.DatabaseName,
                $"UPDATE [{nameof(Task)}] SET [{nameof(Task.InProgress)}] = @{nameof(Task.InProgress)}" +
                $", [{nameof(Task.UpdatedUTC)}] = @{nameof(Task.UpdatedUTC)}" +
                $", [{nameof(Task.IsComplete)}] = @{nameof(Task.IsComplete)}" +
                $" WHERE [{nameof(Task.Id)}] = @{nameof(Task.Id)}",
                new
                {
                    Id = taskId,
                    InProgress = progressState,
                    IsComplete = false,
                    UpdatedUTC = NulahStandardLib.DateTimeNow()
                });
            LoadTasksForDatabase(_currentTaskDatabase);
            return update;
        }

        /// <summary>
        /// Updates the progress state of the given task by id, and sets InProgress to false
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="completedState"></param>
        /// <returns></returns>
        public bool UpdateTaskCompletedState(Guid taskId, bool completedState)
        {
            var taskProps = NulahStandardLib.GetPropertiesForType<Task>();
            var update = _sqliteProvider.Update<Task>(_currentTaskDatabase.DatabaseName,
                $"UPDATE [{nameof(Task)}] SET [{nameof(Task.IsComplete)}] = @{nameof(Task.IsComplete)}" +
                $", [{nameof(Task.UpdatedUTC)}] = @{nameof(Task.UpdatedUTC)}" +
                $", [{nameof(Task.InProgress)}] = @{nameof(Task.InProgress)}" +
                $" WHERE [{nameof(Task.Id)}] = @{nameof(Task.Id)}",
                new
                {
                    Id = taskId,
                    IsComplete = completedState,
                    InProgress = false,
                    UpdatedUTC = NulahStandardLib.DateTimeNow()
                });
            LoadTasksForDatabase(_currentTaskDatabase);
            return update;
        }
    }
}
