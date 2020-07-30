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

        /// <summary>
        /// %APPDATA%/Nulah
        /// </summary>
        private string _applicationDataLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nulah");

        public TaskListManager()
        {
            _sqliteProvider = new SqliteProvider();
            CreateAppDataFolder();
            CreateAppDatabases();

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

        private void CreateAppDatabases()
        {
            CreateApplicationSettingsDatabase();
            CreateGlobalTaskListDatabase();
        }

        private void CreateGlobalTaskListDatabase()
        {
            var globalDb = Path.Combine(_applicationDataLocation, "Nulah.TaskList.sqlitedb");

            var databaseCreated = _sqliteProvider.CreateDataSource(GLOBAL_DB_DATASOURCE_NAME, globalDb);
            // Track the global task list database in the application settings database if this is the first time we're creating it
            if (databaseCreated == true)
            {
                _sqliteProvider.CreateTable<Task>(GLOBAL_DB_DATASOURCE_NAME);
                CreateDatabaseEntry(globalDb);
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

        public void SwitchDatabase(string newDatabase)
        {
            _currentTaskDatabase = newDatabase;
            LoadTasksForCurrentDatabase(newDatabase);
        }

        private void LoadTasksForCurrentDatabase(string taskDatabase)
        {
            _currentTaskList = _sqliteProvider.Query<Task>(taskDatabase, $"SELECT * FROM [{nameof(Task)}] ORDER BY [{nameof(Task.CreatedUTC)}] DESC");
        }

        public List<Task> GetTask()
        {
            return _currentTaskList;
        }
    }
}
