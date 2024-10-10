using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using Microsoft.Data.Sqlite;

namespace Atlas.Core.Database
{
    public static class Database
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
                RedordID = InsertOrUpdate($"INSERT OR REPLACE INTO games(title, creator, engine, last_played_r, total_playtime) VALUES('{gameDetail.Title.Replace("\'", "\'\'")}', '{gameDetail.Creator}','{gameDetail.Engine}',0, 0) RETURNING record_id",0);
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
            InsertOrUpdate(cmd,1);
        }

        public static string InsertOrUpdate(string cmd, int type)
        {
            string data = string.Empty;
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
            return data;
        }
    }
}
