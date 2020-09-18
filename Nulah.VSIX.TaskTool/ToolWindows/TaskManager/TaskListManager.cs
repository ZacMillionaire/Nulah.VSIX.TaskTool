using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        private const string ALL_DB_DATASOURCE_NAME = "GLOBAL_AND_SOLUTION";
        private const string APP_SETTINGS_DB_DATASOURCE_NAME = "APPSETTINGS";

        public List<DatabaseSource> AvailableTaskLists { get; private set; }
        private Dictionary<string, DatabaseSource> _taskDatabases { get; set; }

        private Dictionary<string, List<Task>> _loadedTasks { get; set; }
        public bool ListChanged { get; private set; }

        private DatabaseSource _currentTaskDatabase;

        public bool IsAllTaskSourcesInUse;

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

            _taskDatabases = new Dictionary<string, DatabaseSource>
            {
                {
                    ALL_DB_DATASOURCE_NAME,
                    new DatabaseSource{
                        DisplayName = "<all lists>",
                        DatabaseName = ALL_DB_DATASOURCE_NAME,
                        Location = null
                    }
                },
                {
                    GLOBAL_DB_DATASOURCE_NAME,
                    new DatabaseSource{
                        DisplayName = "Global",
                        DatabaseName = GLOBAL_DB_DATASOURCE_NAME,
                        Location = Path.Combine(_applicationDataLocation, "Nulah.TaskList.sqlitedb")
                    }
                },
            };

            // Get databases for loaded solution and projects - if a solution/project is open
            AvailableTaskLists = _taskDatabases.Values.ToList();

            // Default to using Global database for now
            SwitchDatabase(_taskDatabases[ALL_DB_DATASOURCE_NAME]);
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
            CreateDatabase(GLOBAL_DB_DATASOURCE_NAME, "Nulah.TaskList");
        }

        /// <summary>
        /// Create a database
        /// </summary>
        /// <param name="datasourceName"></param>
        /// <param name="databaseName"></param>
        public void CreateDatabase(string datasourceName, string databaseName)
        {
            var databaseLocation = Path.Combine(_applicationDataLocation, $"{databaseName}.sqlitedb");

            var databaseCreated = _sqliteProvider.CreateDataSource(datasourceName, databaseLocation);
            // Track the new task list database in the application settings database if this is the first time we're creating it
            if (databaseCreated == true)
            {
                _sqliteProvider.CreateTable<Task>(datasourceName);
                CreateDatabaseEntry(databaseLocation);
            }
        }

        private void CreateApplicationSettingsDatabase()
        {
            var applicationDb = Path.Combine(_applicationDataLocation, "Nulah.Settings.sqlitedb");
            _sqliteProvider.CreateDataSource(APP_SETTINGS_DB_DATASOURCE_NAME, applicationDb);

            _sqliteProvider.CreateTable<Database>(APP_SETTINGS_DB_DATASOURCE_NAME);
        }

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
            // If the database name is the one used to indicate "all for solution"
            if (newDatabase.DatabaseName == ALL_DB_DATASOURCE_NAME)
            {
                IsAllTaskSourcesInUse = true;
                // load all tasks from all datasources

            }
            // Otherwise check if a database exists at the given location
            else if (_sqliteProvider.DataSourceExists(newDatabase.DatabaseName))
            {
                IsAllTaskSourcesInUse = false;
                _currentTaskDatabase = newDatabase;
                LoadTasksForCurrentDatabase(newDatabase.DatabaseName);
            }
            else
            {
                throw new Exception($"{newDatabase} does not exist");
            }
        }

        public string GetCurrentDatabase()
        {
            /*
            if (IsGlobalDatabaseInUse == true)
            {
                return "Global";
            }
            */
            return $"Solution - {_currentTaskDatabase.DisplayName}";
        }

        /// <summary>
        /// Loads all tasks from the sqlite database by its given source key, ordered by task creation descending
        /// </summary>
        /// <param name="databaseSourceKey"></param>
        private void LoadTasksForCurrentDatabase(string databaseSourceKey)
        {
            // Horribly track if the task list is dirty and that the UI should reload to get latest changes
            ListChanged = true;
            if (IsAllTaskSourcesInUse == false)
            {
                _loadedTasks = new Dictionary<string, List<Task>>{
                    {
                        _currentTaskDatabase.DisplayName,
                        _sqliteProvider.Query<Task>(databaseSourceKey, $"SELECT * FROM [{nameof(Task)}] ORDER BY [{nameof(Task.CreatedUTC)}] DESC")
                    }
                };
            }
            else
            {
                throw new Exception("all task list not implemented");
            }
        }

        /// <summary>
        /// Returns all tasks for the currently selected database, and clears the ListChanged flag
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<Task>> GetTasks()
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

            LoadTasksForCurrentDatabase(_currentTaskDatabase.DatabaseName);

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
            LoadTasksForCurrentDatabase(_currentTaskDatabase.DatabaseName);
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
            LoadTasksForCurrentDatabase(_currentTaskDatabase.DatabaseName);
            return update;
        }

    }
}
