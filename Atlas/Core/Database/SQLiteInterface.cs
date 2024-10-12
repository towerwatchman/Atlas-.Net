using System.Collections.Generic;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Atlas.UI;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using NLog;

namespace Atlas.Core.Database
{
    public static class SQLiteInterface
    {
        public static void Init()
        {
            string DatabasePath = Path.Combine(Directory.GetCurrentDirectory(), Settings.Config.DatabasePath);
            //check if database already exist
            if (Path.Exists(DatabasePath))
            {
                try
                {
                    //Create db file
                    using (var connection = new SqliteConnection($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "data", "data.db")}"))
                    {
                        connection.Open();
                    }
                    Migrations.Run();
                }
                catch (Exception ex)
                {
                    Logging.Logger.Error(ex);
                }
            }
        }
        public static string AddGame(GameDetails gameDetail)
        {
            string RedordID = string.Empty;

            try
            {
                RedordID = InsertOrUpdate($"INSERT OR REPLACE INTO games(title, creator, engine, last_played_r, total_playtime) VALUES('{gameDetail.Title.Replace("\'", "\'\'")}', '{gameDetail.Creator}','{gameDetail.Engine}',0, 0) RETURNING record_id", 0);
            }
            catch (Exception ex)
            {
                Logging.Logger.Error(ex);
            }

            return RedordID;
        }

        public static void AddVersion(GameDetails gameDetail, int RecordID)
        {
            string cmd = @$"INSERT OR REPLACE INTO versions (
                            record_id, 
                            version, 
                            game_path, 
                            exec_path, 
                            in_place, 
                            date_added, 
                            last_played, 
                            version_playtime, 
                            folder_size) 
                            VALUES (
                                {RecordID}, 
                                '{gameDetail.Version}',
                                '{gameDetail.Folder.Replace("\'", "\'\'")}', 
                                '{System.IO.Path.Combine(gameDetail.Folder, gameDetail.Executable[0]).Replace("\'", "\'\'")}',
                                '',
                                '{DateTime.Now}',
                                0,
                                0,
                                0
                                )";
            InsertOrUpdate(cmd, 1);
        }

        public static List<string> SelectData(string query)
        {
            List<string> data = new List<string>();
            using (var connection = new SqliteConnection($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "data", "data.db")}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = query;
                using var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                       data.Add(reader["update_time"].ToString());
                    }
                }
            }
            return data;
        }
        public static string InsertOrUpdate(string cmd, int type)
        {
            string data = string.Empty;
            try
            {

                using (var connection = new SqliteConnection($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "data", "data.db")}"))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        // Transaction may include additional statements before the savepoint

                        var updated = false;
                        do
                        {
                            // Begin savepoint
                            transaction.Save("Insert Data");


                            var updateCommand = connection.CreateCommand();
                            updateCommand.CommandText = cmd;

                            if (type == 0)
                            {
                                data = updateCommand.ExecuteScalar().ToString();
                            }
                            else
                            {
                                updateCommand.ExecuteNonQuery();
                            }

                            // Update succeeded. Commit savepoint and continue with the transaction
                            transaction.Release("Insert Data");

                            updated = true;

                        }
                        while (!updated);

                        // Additional statements may be included after the savepoint

                        transaction.Commit();
                    }

                }
            }
            catch (Exception ex)
            {
                Logging.Logger.Error($"{ex.Message}");
            }
            return data;
        }

        public static string InsertOrUpdateAsync(List<string> queries)
        {
            string data = string.Empty;
            try
            {
                using (var connection = new SqliteConnection($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "data", "data.db")}"))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        // Transaction may include additional statements before the savepoint

                        var updated = false;
                        do
                        {
                            // Begin savepoint
                            transaction.Save("Insert Data");

                            var updateCommand = connection.CreateCommand();
                            double totalQueries = queries.Count();
                            double currentQuery = 0;
                            foreach (var query in queries)
                            {
                                InterfaceHelper.SplashWindow.Dispatcher.Invoke((Action)(() =>
                                {
                                    InterfaceHelper.SplashProgressBar.Value = (currentQuery/ totalQueries)*100;
                                }));

                                //Logging.Logger.Info($"Percent complete {currentQuery} / {totalQueries}");
                                updateCommand.CommandText = query;
                                updateCommand.ExecuteNonQuery();
                                currentQuery++;
                            }
                            
                            // Update succeeded. Commit savepoint and continue with the transaction
                            transaction.Release("Insert Data");
                            updated = true;
                        }
                        while (!updated);
                        transaction.Commit();
                    }

                }
            }
            catch (Exception ex)
            {
                Logging.Logger.Error($"{ex.Message}");
            }
            return data;
        }


        public static Task InsertJsonData(JToken data, string table)
        {
            string sql = string.Empty;          
            List<string> queries = new List<string>();

            foreach (JToken entry in data)
            {
                string sqlItems = string.Empty;
                string sqlValues = string.Empty;

                int itemCount = 1;
                foreach (JProperty item in entry)
                {
                    //skip null or empty values. This will prevent overwritting data that already exist in the database.
                    if (item.Value.ToString() != "")
                    {
                        sqlItems += item.Name;
                        sqlValues += $"'{item.Value.ToString().Replace("\'", "\'\'")}'";
                        if (itemCount < entry.Count())
                        {
                            sqlItems += ",";
                            sqlValues += ",";
                        }                        
                    }
                    itemCount++;
                }
                sql = $"INSERT OR REPLACE INTO {table} ({sqlItems}) Values ({sqlValues})";
                queries.Add(sql);
            }

            InsertOrUpdateAsync(queries);

            return Task.CompletedTask;
        }

        internal static int GetLastUpdateVersion()
        {
            string query = "SELECT update_time FROM updates ORDER BY update_time ASC";
            List<string> data = SelectData(query);
            if (data.Count <= 0)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(data[0]);
            }
        }

        public static void InsertUpdateVersion(string update_time, string md5)
        {
            long processed_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string query = $"INSERT INTO updates (update_time, processed_time, md5) VALUES ('{update_time}','{processed_time}','{md5}')";
            InsertOrUpdate(query, 1);
        }

    }
}
