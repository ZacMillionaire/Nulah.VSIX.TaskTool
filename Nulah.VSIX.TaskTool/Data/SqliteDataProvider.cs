﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nulah.VSIX.TaskTool.StandardLib;
using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.Data
{
    public class SqliteProvider
    {
        /// <summary>
        /// A mapping of databaseName,databaseConnectionStrings
        /// <para>This provides the means to store multiple databases and allow mutliple databases to be used once registered</para>
        /// </summary>
        private Dictionary<string, string> _dataSourceConnectionStrings;

        private Dictionary<Type, string> _sqliteTypes = new Dictionary<Type, string>
        {
            // Mapping built from: https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/types
            // Integer
            { typeof(bool), "INTEGER" },
            { typeof(byte), "INTEGER" },
            { typeof(float), "REAL" }, // Single
            { typeof(short), "INTEGER" }, // Int16
            { typeof(int), "INTEGER" }, // Int32
            { typeof(long), "INTEGER" }, // Int64
            { typeof(ushort), "INTEGER" }, // UInt16
            // Real
            { typeof(sbyte), "REAL" },
            { typeof(double), "REAL" },
            // Blob
            { typeof(byte[]), "BLOB" },
            { typeof(uint), "BLOB" }, // UInt32
            { typeof(ulong), "BLOB" }, // Uint64 - Will overflow on large values
            { typeof(char), "BLOB" }, // Chars are stored as a byte to reduce useless conversions
            // Text
            { typeof(DateTime), "TEXT" },
            { typeof(DateTimeOffset), "TEXT" },
            { typeof(decimal), "TEXT" },
            { typeof(Guid), "TEXT" },
            { typeof(string), "TEXT" },
            { typeof(TimeSpan), "TEXT" },
        };

        public SqliteProvider()
        {
            _dataSourceConnectionStrings = new Dictionary<string, string>();
        }

        /// <summary>
        /// If the datasource does not already exist, an empty database will be created and will return true. Otherwise, registers an existing database with the given <paramref name="sourceName"/> data source keyname
        /// <para>A return of false means the database already existed and was not created, but the datasource will be registered (as the database exists and is valid)</para>
        /// </summary>
        /// <param name="sourceName">Used to identify the database connection string by name</param>
        /// <param name="dataSource">Location for the sqlite database to be stored</param>
        /// <returns></returns>
        public bool CreateOrRegisterDataSource(string sourceName, string dataSource)
        {
            // Do nothing if the datasource has already been registered during constructor or another method
            if (_dataSourceConnectionStrings.ContainsKey(sourceName) == true)
            {
                return false;
            }

            var createdNewDatabase = false;
            if (SqliteFileExists(dataSource) == false)
            {
                CreateSqliteFile(dataSource);
                createdNewDatabase = true;
            }

            var sqliteConnBuilder = new SQLiteConnectionStringBuilder();

            sqliteConnBuilder.DataSource = dataSource;

            if (sqliteConnBuilder.Version < 3)
            {
                sqliteConnBuilder.Version = 3;
            }

            // Set the connection string for this instance to the result of the string builder, not user input
            _dataSourceConnectionStrings.Add(sourceName, sqliteConnBuilder.ToString());

            return createdNewDatabase;
        }

        /// <summary>
        /// Removes a data source registration based on given sourceName key
        /// </summary>
        /// <param name="sourceName"></param>
        public void UnregisterDatasource(string sourceName)
        {
            if (_dataSourceConnectionStrings.ContainsKey(sourceName) == true)
            {
                _dataSourceConnectionStrings.Remove(sourceName);
            }
        }

        /// <summary>
        /// Checks for the existance of a database connection string, based on the name of the database given
        /// </summary>
        /// <param name="sourceName"></param>
        /// <returns></returns>
        public bool DataSourceExists(string sourceName)
        {
            return _dataSourceConnectionStrings.ContainsKey(sourceName);
        }

        /// <summary>
        /// Returns the connection for a given database name
        /// </summary>
        /// <param name="sourceName"></param>
        /// <returns></returns>
        private SQLiteConnection GetConnection(string sourceName)
        {
            return new SQLiteConnection(_dataSourceConnectionStrings[sourceName]);
        }

        /// <summary>
        /// Creates a table of given type <typeparamref name="T"/>, for the given datasource name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public bool CreateTable<T>(string dataSource)
        {
            var typeDetails = NulahStandardLib.GetPropertiesForType<T>();

            var columnDefinitions = typeDetails.Select(x => $"[{x.Name}] {TypeToSQLType(x.ValueType)} {(x.IsNullableType ? string.Empty : "NOT NULL")}");

            var createQuery = $"CREATE table IF NOT EXISTS [{typeof(T).Name}] ({string.Join(",", columnDefinitions)});";

            using (var conn = GetConnection(dataSource))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(createQuery, conn))
                {
                    var res = cmd.ExecuteNonQuery();
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if a sqlite file exists at the given location
        /// </summary>
        /// <param name="sqliteFileLocation"></param>
        /// <returns></returns>
        private bool SqliteFileExists(string sqliteFileLocation)
        {
            var fileInfo = new FileInfo(sqliteFileLocation);
            // TODO: actually check that the file is an .sqlite file
            return fileInfo.Exists;
        }

        /// <summary>
        /// Creates a sqlite file at the given location
        /// </summary>
        /// <param name="dbLocation"></param>
        /// <returns></returns>
        private string CreateSqliteFile(string dbLocation)
        {
            var di = new DirectoryInfo(dbLocation);
            Directory.CreateDirectory(di.Parent.FullName);

            using (var f = File.Create(dbLocation))
            {
                return dbLocation;
            }
        }

        /// <summary>
        /// Adds a parameter to the given parameter collection, serialising to the types respective DB type, performing serialisation as needed
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="reflectedValueInfo"></param>
        /// <param name="queryParameters"></param>
        private void AddValueToQueryParameters(string paramName, ReflectedValueInfo reflectedValueInfo, SQLiteParameterCollection queryParameters)
        {
            if (reflectedValueInfo.IsNull == true)
            {
                queryParameters.AddWithValue(paramName, DBNull.Value);
            }
            else if (reflectedValueInfo.ValueType == typeof(DateTime))
            {
                queryParameters.AddWithValue(paramName, DateTimeToISO8601String((DateTime)reflectedValueInfo.Value));
            }
            else if (reflectedValueInfo.ValueType == typeof(DateTimeOffset))
            {
                queryParameters.AddWithValue(paramName, DateTimeOffsetToString((DateTimeOffset)reflectedValueInfo.Value));
            }
            else if (reflectedValueInfo.ValueType == typeof(decimal))
            {
                queryParameters.AddWithValue(paramName, ValueToLosslessDecimal((decimal)reflectedValueInfo.Value));
            }
            else if (reflectedValueInfo.ValueType == typeof(TimeSpan))
            {
                queryParameters.AddWithValue(paramName, TimeSpanToString((TimeSpan)reflectedValueInfo.Value));
            }
            else if (reflectedValueInfo.ValueType == typeof(Guid))
            {
                queryParameters.AddWithValue(paramName, reflectedValueInfo.Value.ToString());
            }
            else if (reflectedValueInfo.ValueType == typeof(uint))
            {
                var byteValue = BitConverter.GetBytes((uint)reflectedValueInfo.Value);
                queryParameters.AddWithValue(paramName, byteValue);
            }
            else if (reflectedValueInfo.ValueType == typeof(ulong))
            {
                var byteValue = BitConverter.GetBytes((ulong)reflectedValueInfo.Value);
                queryParameters.AddWithValue(paramName, byteValue);
            }
            else if (reflectedValueInfo.ValueType == typeof(char))
            {
                // Chars are stored as a byte pair BLOB to reduce string conversions
                queryParameters.AddWithValue(paramName, BitConverter.GetBytes((char)reflectedValueInfo.Value));
            }
            else
            {
                queryParameters.AddWithValue(paramName, reflectedValueInfo.Value);
            }
        }

        private string DateTimeToISO8601String(DateTime dateTime)
        {
            return dateTime.ToString("o");
        }

        private string DateTimeOffsetToString(DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString("o");
        }

        private string TimeSpanToString(TimeSpan timeSpan)
        {
            return timeSpan.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        }

        private string ValueToLosslessDecimal(decimal decimalValue)
        {
            return decimalValue.ToString("0.############################");
        }

        /// <summary>
        /// Deserialises a value retrieved from an sqlite value back to its C# type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination"></param>
        /// <param name="valueName"></param>
        /// <param name="objectValue"></param>
        /// <param name="valueType"></param>
        private void SetValueOnObject<T>(T destination, string valueName, object objectValue, Type valueType)
        {
            object o = null;

            if (typeof(bool).Equals(valueType) == true)
            {
                o = Convert.ToBoolean(objectValue);
            }
            else if (typeof(byte).Equals(valueType) == true)
            {
                o = Convert.ToByte(objectValue);
            }
            else if (typeof(float).Equals(valueType) == true)
            {
                o = Convert.ToSingle(objectValue);
            }
            else if (typeof(short).Equals(valueType) == true)
            {
                o = Convert.ToInt16(objectValue);
            }
            else if (typeof(int).Equals(valueType) == true)
            {
                o = Convert.ToInt32(objectValue);
            }
            else if (typeof(long).Equals(valueType) == true)
            {
                o = Convert.ToInt64(objectValue);
            }
            else if (typeof(ushort).Equals(valueType) == true)
            {
                o = Convert.ToUInt16(objectValue);
            }
            else if (typeof(uint).Equals(valueType) == true)
            {
                // In order to side step issues with sqlite and its weird fucking support for unsigned 8-byte numbers,
                // we store and retrieve them as byte[], and convert as needed.
                // Yes its dumb, but both System.Data.SQLite and Microsoft.Data.Sqlite overflow ulongs, and only
                // the latter can correctly insert uints.
                // I don't give a shit though and I need both to store correctly without bullshit.
                o = BitConverter.ToUInt32((byte[])objectValue, 0);
            }
            else if (typeof(ulong).Equals(valueType) == true)
            {
                // In order to side step issues with sqlite and its weird fucking support for unsigned 8-byte numbers,
                // we store and retrieve them as byte[], and convert as needed.
                // Yes its dumb, but both System.Data.SQLite and Microsoft.Data.Sqlite overflow ulongs, and only
                // the latter can correctly insert uints.
                // I don't give a shit though and I need both to store correctly without bullshit.
                o = BitConverter.ToUInt64((byte[])objectValue, 0);
            }
            else if (typeof(sbyte).Equals(valueType) == true)
            {
                o = Convert.ToSByte(objectValue);
            }
            else if (typeof(double).Equals(valueType) == true)
            {
                // SQLiteDataReader returns doubles as doubles so we can also use those directly
                o = objectValue;
            }
            else if (typeof(byte[]).Equals(valueType) == true)
            {
                // SQLiteDataReader returns byte arrays as a byte array so the value can be set 1 for 1
                o = objectValue;
            }
            else if (typeof(char).Equals(valueType) == true)
            {
                // Chars are stored as a BLOB to reduce string conversions, so they'll be returned as a byte pair
                o = BitConverter.ToChar((byte[])objectValue, 0);
            }
            else if (typeof(DateTime).Equals(valueType) == true)
            {
                // Parse datetimes using the same ToString we use when storing, using RoundtripKind to prevent conversion to LocalCulture
                // which would cause datetimes inserted to be automatically adjusted using the machines timezoneinfo.
                // This ensures the DateTime returned from a query matches the DateTime inserted exactly (even though technically even with
                // timezoneinfo factored in, DateTime.Ticks would be equal, but people don't use that value for obvious reasons)
                o = DateTime.ParseExact((string)objectValue, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
            else if (typeof(DateTimeOffset).Equals(valueType) == true)
            {
                // Parse datetimes using the same ToString we use when storing, using RoundtripKind to prevent conversion to LocalCulture
                // which would cause datetimes inserted to be automatically adjusted using the machines timezoneinfo.
                // This ensures the DateTime returned from a query matches the DateTime inserted exactly (even though technically even with
                // timezoneinfo factored in, DateTime.Ticks would be equal, but people don't use that value for obvious reasons)
                o = DateTimeOffset.ParseExact((string)objectValue, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
            else if (typeof(decimal).Equals(valueType) == true)
            {
                o = Convert.ToDecimal(objectValue);
            }
            else if (typeof(Guid).Equals(valueType) == true)
            {
                o = Guid.Parse((string)objectValue);
            }
            else if (typeof(string).Equals(valueType) == true)
            {
                // SQLiteDataReader returns any TEXT field as a string, so we can use the value directly here
                o = objectValue;
            }
            else if (typeof(TimeSpan).Equals(valueType) == true)
            {
                o = TimeSpan.Parse((string)objectValue);
            }

            if (o == null)
            {
                throw new Exception();
            }

            typeof(T).GetProperty(valueName).SetValue(destination, o);

        }

        /// <summary>
        /// Takes the results of an SQLite query, and serialises into a List <typeparamref name="T"/>
        /// <para>This method always returns a list object, so null checks are not required</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private List<T> ReaderToType<T>(SQLiteDataReader reader)
        {
            var typeProps = NulahStandardLib.GetPropertiesForType<T>();
            var l = new List<T>();
            var dt = new DataTable();

            dt.Load(reader);

            foreach (DataRow dr in dt.Rows)
            {
                var o = Activator.CreateInstance<T>();
                foreach (var prop in typeProps)
                {
                    if (dr.Table.Columns.Contains(prop.Name))
                    {
                        var rowValue = dr[prop.Name];
                        if (rowValue != DBNull.Value)
                        {
                            SetValueOnObject(o, prop.Name, rowValue, prop.ValueType);
                        }
                    }
                }

                l.Add(o);
            }

            return l;
        }

        /// <summary>
        /// Returns the SQLite type for a given C# type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string TypeToSQLType(Type type)
        {
            if (_sqliteTypes.ContainsKey(type))
            {
                return _sqliteTypes[type];
            }

            throw new Exception($"Given type {type.FullName} does not have a valid map to a SQLite type.");
        }

        /// <summary>
        /// Inserts the object into the database given its data store key, returning true if any rows were created
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataStore"></param>
        /// <param name="query"></param>
        /// <param name="insertObject"></param>
        /// <returns></returns>
        public bool Insert<T>(string dataStore, string query, T insertObject)
        {
            using (var conn = GetConnection(dataStore))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    var objectParams = NulahStandardLib.GetPropertiesAndValuesForObject(insertObject);
                    foreach (var param in objectParams)
                    {
                        AddValueToQueryParameters(param.Key, param.Value, cmd.Parameters);
                    }
                    var insert = cmd.ExecuteNonQuery();
                    if (insert > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Queries the database from the given data store key, returning all results found.
        /// <para>List will be empty if no results are found</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataStore"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<T> Query<T>(string dataStore, string query)
        {
            using (var conn = GetConnection(dataStore))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    var q = cmd.ExecuteReader();
                    return ReaderToType<T>(q);
                }
            }
        }

        /// <summary>
        /// Queries the database from the given data store key, returning all results found.
        /// <para>List will be empty if no results are found</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataStore"></param>
        /// <param name="query"></param>
        /// <param name="queryParameters"></param>
        /// <returns></returns>
        public List<T> Query<T>(string dataStore, string query, object queryParameters)
        {
            using (var conn = GetConnection(dataStore))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    var objectParams = NulahStandardLib.GetPropertiesAndValuesForObject(queryParameters);
                    foreach (var param in objectParams)
                    {
                        AddValueToQueryParameters(param.Key, param.Value, cmd.Parameters);
                    }
                    var q = cmd.ExecuteReader();
                    return ReaderToType<T>(q);
                }
            }
        }

        /// <summary>
        /// Updates the value in the given database by its data store key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataStore"></param>
        /// <param name="query"></param>
        /// <param name="queryParameters"></param>
        /// <returns></returns>
        public bool Update<T>(string dataStore, string query, object queryParameters)
        {
            using (var conn = GetConnection(dataStore))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    var objectParams = NulahStandardLib.GetPropertiesAndValuesForObject(queryParameters);
                    foreach (var param in objectParams)
                    {
                        AddValueToQueryParameters(param.Key, param.Value, cmd.Parameters);
                    }
                    var update = cmd.ExecuteNonQuery();
                    if (update > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Empties all data from the database from its given data store key.
        /// <para>This method is destructive and it should be assumed cannot be undone</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataStore"></param>
        public void Truncate<T>(string dataStore)
        {
            var createQuery = $"DELETE FROM [{typeof(T).Name}]; VACUUM";

            using (var conn = GetConnection(dataStore))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(createQuery, conn))
                {
                    var res = cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
