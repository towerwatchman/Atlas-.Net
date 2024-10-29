using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Atlas.Core.Utilities;
using Atlas.UI;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.LayoutRenderers;
using static NLog.LayoutRenderers.Wrappers.ReplaceLayoutRendererWrapper;

namespace Atlas.Core.Database
{
    public static class SQLiteInterface
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
                    Logger.Error(ex);
                }
            }
        }
        public static string AddGame(GameDetails gameDetail)
        {
            string RedordID = string.Empty;

            try
            {
                RedordID = InsertOrUpdate($"INSERT INTO games(title, creator, engine, last_played_r, total_playtime) VALUES('{gameDetail.Title.Replace("\'", "\'\'")}', '{gameDetail.Creator.Replace("\'", "\'\'")}','{gameDetail.Engine.Replace("\'", "\'\'")}',0, 0) RETURNING record_id", 0);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return RedordID;
        }

        public static void AddVersion(GameDetails gameDetail, int RecordID)
        {
            string cmd = @$"INSERT INTO versions (
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
                                '{gameDetail.Version.Replace("\'", "\'\'")}',
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
                Logger.Error($"{ex.Message}");
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
                                InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                                {
                                    InterfaceHelper.LauncherProgressBar.Value = (currentQuery / totalQueries) * 100;
                                }));

                                //Logger.Info($"Percent complete {currentQuery} / {totalQueries}");
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
                Logger.Error($"{ex.Message}");
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

        public static void SetAtlasMapping(string recordID, string atlasID)
        {
            string query = $"INSERT OR REPLACE INTO atlas_mappings (record_id, atlas_id) VALUES({recordID},{atlasID})";
            InsertOrUpdate(query, 1);
        }
        public static int FindRecordID(string title, string creator)
        {
            int record = -1;

            string query = $"SELECT record_id from games where title = '{title.Replace("\'", "\'\'")}' AND creator ='{creator.Replace("\'", "\'\'")}'";

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
                        record = Convert.ToInt32(reader["record_id"].ToString());
                    }
                    reader.Close();
                }
            }
            return record;
        }

        public static int FindF95ID(int record_id)
        {
            int record = -1;
            string query = $"SELECT f95_id from f95_zone_data where atlas_id = '{record}";

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
                        record = Convert.ToInt32(reader["record_id"].ToString());
                    }
                    reader.Close();
                }
            }
            return record;
        }

        public static bool CheckIfVersionExist(string record_id, string version)
        {
            int record = -1;

            string query = $"SELECT record_id from versions where record_id = '{record_id}' AND version ='{version.Replace("\'", "\'\'")}'";

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
                        record = Convert.ToInt32(reader["record_id"].ToString());
                    }
                    reader.Close();
                }
            }
            return record > -1 ? true : false;
        }

        public static List<string[]> GetAtlasId(string title, string creator)
        {
            var full_name = $"{Regex.Replace(title, "[\\W_]+", "").ToUpper()}{Regex.Replace(creator, "[\\W_]+", "").ToUpper()}";
            var short_name = Regex.Replace(title, "[\\W_]+", "").ToUpper();

            //Old Method. Only based on title

            string query_title = $"Select " +
                $"atlas_id, " +
                $"title, " +
                $"creator, " +
                $"engine, " +
                $"LENGTH(short_name) - LENGTH('{short_name}') as difference " +
                $"from atlas_data WHERE short_name like '%{short_name}%' Order By LENGTH(short_name) - LENGTH('{short_name}')";


            string query = @$"
WITH data_0 AS(
SELECT 
atlas_id, 
title, 
creator, 
engine, 
UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(title || "" "" || creator, '-', ''),'_',''),'/',''),'\',''),':',''),';',''),'''',''),' ',''),'.','')) as ""full_name""
from atlas_data )

SELECT 
atlas_id, 
title, 
creator, 
engine,
LENGTH(full_name) - LENGTH('{full_name}') as ""difference""
FROM data_0
WHERE full_name like '%{full_name}%' Order By LENGTH(full_name) - LENGTH('{full_name}')";

            List<string[]> data = new List<string[]>();
            using (var connection = new SqliteConnection($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "data", "data.db")}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = query_title;
                using var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {

                        data.Add(new string[] {
                            reader["atlas_id"].ToString(),
                            reader["title"].ToString(),
                            reader["creator"].ToString(),
                            reader["engine"].ToString(),
                            reader["difference"].ToString()
                            });

                    }
                    reader.Close();
                }
                if (data.Count > 1) //try with just the title
                {
                    reader.Close();
                    command.CommandText = query;
                    using var reader_title = command.ExecuteReader();

                    if (reader_title.HasRows)
                    {
                        data.Clear();
                        while (reader_title.Read())
                        {
                            if (Convert.ToInt32(reader_title["difference"].ToString()) <= 7)
                            {
                                data.Add(new string[] {
                            reader_title["atlas_id"].ToString(),
                            reader_title["title"].ToString(),
                            reader_title["creator"].ToString(),
                            reader_title["engine"].ToString(),
                            reader_title["difference"].ToString()
                            });
                            }
                        }
                        reader_title.Close();
                    }
                }
            }
            return data;
        }



        private static List<GameVersion> GetVersions(string record_id)
        {
            List<GameVersion> gameVersions = new List<GameVersion>();
            string query = $"SELECT * FROM versions where record_id = '{record_id}'";


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
                        try
                        {
                            GameVersion gameVersion = new GameVersion
                            {
                                RecordId = Convert.ToInt32(record_id),
                                Version = reader["version"].ToString(),
                                GamePath = reader["game_path"].ToString(),
                                ExePath = reader["exec_path"].ToString(),
                                DateAdded = DateTime.Parse(reader["date_added"].ToString())
                            };
                            gameVersions.Add(gameVersion);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"{ex.Message}");
                        }
                    }
                    reader.Close();
                }
            }
            return gameVersions;
        }

        public static async Task<Task> BuildGameListDetails()
        {
            ObservableCollection<Game> Games = new ObservableCollection<Game>();

            //this will need to be modified for more data in the future
            string query = @"SELECT 
games.record_id as record_id,
atlas_mappings.atlas_id as atlas_id,	
games.title as title,
games.creator as creator,
games.engine as engine,
banners.path as image_path,
f95_zone_data.f95_id as f95_id,
f95_zone_data.site_url as site_url,
f95_zone_data.views as views,
f95_zone_data.likes as likes,
f95_zone_data.tags as tags,
f95_zone_data.rating as rating

FROM
atlas_mappings
LEFT JOIN games on atlas_mappings.record_id = games.record_id
LEFT JOIN banners on atlas_mappings.record_id = banners.record_id
LEFT JOIN f95_zone_data on atlas_mappings.atlas_id = f95_zone_data.atlas_id";

            await using (var connection = new SqliteConnection($"Data Source={Path.Combine(Directory.GetCurrentDirectory(), "data", "data.db")}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = query;
                using var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        try
                        {
                            ImageInterface image = new ImageInterface();
                            Game game = new Game
                            {
                                AtlasID = reader["atlas_id"].ToString(),
                                RecordID = Convert.ToInt32(reader["record_id"].ToString()),
                                F95ID = reader["f95_id"].ToString(),
                                Title = reader["title"].ToString(),
                                Creator = reader["creator"].ToString(),
                                Engine = reader["engine"].ToString(),
                                Views =reader["views"].ToString(),
                                Likes = reader["likes"].ToString(),
                                Tags = reader["tags"].ToString(),
                                BannerPath = reader["image_path"].ToString(),
                                //Versions = null,
                                Versions = GetVersions(reader["record_id"].ToString()),
                                ImageUriAnimated = Path.GetExtension(reader["image_path"].ToString()) == ".gif" ?
                                    new Uri(reader["image_path"].ToString()) :
                                    null,
                                ImageData = image.LoadImage(reader["image_path"].ToString(), Settings.Config.ImageRenderWidth, Settings.Config.ImageRenderHeight)
                            };

                            Games.Add(game);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                    Logger.Warn("Loaded");
                    reader.Close();
                }
            }

            ModelData.Games = Games;

            return Task.CompletedTask;
        }


        public static async Task BuildGameListAsync(List<Game> GameList)
        {
            List<Game> data = new List<Game>();
            string query = "SELECT * FROM games";
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
                        try
                        {
                            ImageInterface image = new ImageInterface();
                            Game game = new Game
                            {
                                //AtlasID = GetAtlasIdMapping(Convert.ToInt32(reader["record_id"].ToString())),
                                RecordID = Convert.ToInt32(reader["record_id"].ToString()),
                                Title = reader["title"].ToString(),
                                Creator = reader["creator"].ToString(),
                                Engine = reader["engine"].ToString(),
                                Versions = null,
                                ImageUriAnimated = null,
                                ImageData = image.LoadImage("", Settings.Config.ImageRenderWidth, Settings.Config.ImageRenderHeight),
                            };       
                            data.Add(game);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                    Logger.Warn("Loaded");
                    reader.Close();
                }
            }
            await Task.Run(() =>
            {
                foreach (var game in data.OrderBy(o=>o.Title).ToList()) 
                {
                    ImageInterface image = new ImageInterface();

                    InterfaceHelper.MainWindow.Dispatcher.Invoke(() =>
                    {
                        game.Versions = GetVersions(game.RecordID.ToString());
                        game.ImageData = image.LoadImage(GetBannerPath(game.RecordID.ToString()), Settings.Config.ImageRenderWidth, Settings.Config.ImageRenderHeight);

                        GameList.Add(game);
                        InterfaceHelper.BannerView.Items.Refresh();

                    });
                }
            });

            Logger.Warn("Second List Loaded");
        }

        private static string GetAtlasIdMapping(int record_id)
        {
            string atlasId = string.Empty;
            string query = $"SELECT atlas_id from atlas_mappings where record_id = '{record_id}'";

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
                        atlasId = reader["atlas_id"].ToString();
                    }
                    reader.Close();
                }
            }
            return atlasId;
        }

        public static string GetBannerUrl(string AtlasId)
        {
            string banner = string.Empty;
            string query = $"SELECT banner_url from f95_zone_data where atlas_id = '{AtlasId}'";

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
                        banner = (reader["banner_url"].ToString());
                    }
                    reader.Close();
                }
            }
            return banner;
        }

        public static string GetBannerPath(string record_id)
        {
            string banner_path = string.Empty;
            string query = $"SELECT path from banners where record_id = '{record_id}'";

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
                        banner_path = (reader["path"].ToString());
                    }
                    reader.Close();
                }
            }
            return banner_path;
        }

        internal static void UpdateBanners(int recordID, string banner_path, string type)
        {
            string query = $"INSERT OR REPLACE INTO banners (record_id, path, type) VALUES('{recordID}','{banner_path}','{type}')";
            InsertOrUpdate(query, 1);
        }
    }
}
