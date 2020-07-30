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
        private const string APP_SETTINGS_DB_DATASOURCE_NAME = "APPSETTINGS";

        private List<Task> _currentTaskList { get; set; }
        private string _currentTaskDatabase;

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

            // Default to using Global database for now
            SwitchDatabase(GLOBAL_DB_DATASOURCE_NAME);
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
        public void SwitchDatabase(string newDatabase)
        {
            if (_sqliteProvider.DataSourceExists(newDatabase))
            {
                _currentTaskDatabase = newDatabase;
                IsGlobalDatabaseInUse = _currentTaskDatabase == GLOBAL_DB_DATASOURCE_NAME;

                LoadTasksForCurrentDatabase(newDatabase);
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

            return $"Solution - {_currentTaskDatabase}";
        }

        private void LoadTasksForCurrentDatabase(string taskDatabase)
        {
            _currentTaskList = _sqliteProvider.Query<Task>(taskDatabase, $"SELECT * FROM [{nameof(Task)}] ORDER BY [{nameof(Task.CreatedUTC)}] DESC");
        }

        /// <summary>
        /// Returns all tasks for the currently selected database
        /// </summary>
        /// <returns></returns>
        public List<Task> GetTasks()
        {
            return _currentTaskList;
        }

        public bool CreateTask(string title, string content)
        {
            var taskProps = NulahStandardLib.GetPropertiesForType<Task>();
            var insert = _sqliteProvider.Insert<Task>(_currentTaskDatabase,
                $"INSERT INTO [{nameof(Task)}] ({string.Join(", ", taskProps.Select(x => $"[{ x.Name}]"))}) VALUES ({string.Join(",", taskProps.Select(x => $"@{x.Name}"))})",
                new Task
                {
                    Content = content,
                    Title = title,
                    CreatedUTC = NulahStandardLib.DateTimeNow(),
                    Id = Guid.NewGuid()
                });

            LoadTasksForCurrentDatabase(_currentTaskDatabase);

            return insert;
        }
    }
}
