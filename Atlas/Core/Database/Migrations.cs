using Microsoft.Data.Sqlite;
using System;
using System.Data.Common;
using System.IO;
using System.Transactions;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Atlas.Core.Database
{
    static class Migrations
    {
        public static void Run()
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
                        transaction.Save("Migrations Update");

                        foreach (var cmd in migrations)
                        {
                            var updateCommand = connection.CreateCommand();
                            updateCommand.CommandText = cmd;

                            var recordsAffected = updateCommand.ExecuteNonQuery();
                        }
                            // Update succeeded. Commit savepoint and continue with the transaction
                            transaction.Release("Migrations Update");

                            updated = true;
                      
                    }
                    while (!updated);

                    // Additional statements may be included after the savepoint

                    transaction.Commit();
                }

            }
        }

        private static string[] migrations = { @"
            CREATE TABLE IF NOT EXISTS games
            (
                record_id       INTEGER PRIMARY KEY,
                title           TEXT NOT NULL,
                creator         TEXT NOT NULL,
                engine          TEXT,
                last_played_r   DATE    DEFAULT 0,
                total_playtime  INTEGER DEFAULT 0,
                description     TEXT,
                UNIQUE (title, creator, engine)
            );",
            @"CREATE TABLE IF NOT EXISTS versions
                (
                record_id        INTEGER REFERENCES games (record_id),
                version          TEXT,
                game_path        TEXT,
                exec_path        TEXT,
                in_place         BOOLEAN,
                last_played      DATE,
                version_playtime INTEGER,
                folder_size      INTEGER,
                date_added       INTEGER,
                UNIQUE (record_id, version)
               );",
            @"  CREATE VIEW IF NOT EXISTS last_import_times (record_id, last_import) AS
                SELECT DISTINCT record_id, versions.date_added
                FROM games
                NATURAL JOIN versions
                ORDER BY versions.date_added DESC;",
            @"  CREATE TABLE IF NOT EXISTS atlas_data
			    (
				    atlas_id       INTEGER PRIMARY KEY,
				    id_name        STRING UNIQUE,
				    short_name     STRING,
				    title          STRING,
				    original_name  STRING,
				    category       STRING,
				    engine         STRING,
				    status         STRING,
				    version        STRING,
				    developer      STRING,
				    creator        STRING,
				    overview       STRING,
				    censored       STRING,
				    language       STRING,
				    translations   STRING,
				    genre          STRING,
				    tags           STRING,
				    voice          STRING,
				    os             STRING,
				    release_date   DATE,
				    length         STRING,
				    banner         STRING,
				    banner_wide    STRING,
				    cover          STRING,
				    logo           STRING,
				    wallpaper      STRING,
				    previews       STRING,
				    last_db_update STRING
			    );",
                @"CREATE TABLE IF NOT EXISTS atlas_previews
                    (
                        atlas_id    INTEGER REFERENCES atlas_data (atlas_id),
                        preview_url STRING NOT NULL,
                        UNIQUE (atlas_id, preview_url)
                    );",
                @"CREATE TABLE IF NOT EXISTS atlas_mappings
				(
					record_id INTEGER REFERENCES games (record_id),
					atlas_id  INTEGER REFERENCES atlas_data (atlas_id),
					UNIQUE (record_id, atlas_id)
				);",
                @"CREATE TABLE IF NOT EXISTS f95_zone_data
				(
					f95_id              INTEGER UNIQUE PRIMARY KEY,
					atlas_id            INTEGER REFERENCES atlas_data (atlas_id) UNIQUE,
					banner_url          STRING,
					site_url            STRING,
					last_thread_comment STRING,
					thread_publish_date STRING,
					last_record_update  STRING,
					views               STRING,
					likes               STRING,
					tags                STRING,
					rating              STRING,
					screens             STRING,
					replies             STRING
				);",
                @"CREATE TABLE IF NOT EXISTS f95_zone_screens
				(
					f95_id     INTEGER REFERENCES f95_zone_data (f95_id),
					screen_url TEXT NOT NULL
				);",
                @"CREATE TABLE IF NOT EXISTS updates
				(
					update_time    INTEGER PRIMARY KEY,
					processed_time INTEGER,
					md5            BLOB
				);",
                @"CREATE TABLE IF NOT EXISTS tags
				(
					tag_id INTEGER PRIMARY KEY,
					tag    TEXT UNIQUE
				);",
                @"CREATE TABLE IF NOT EXISTS tag_mappings
				(
					record_id INTEGER REFERENCES games (record_id),
					tag_id REFERENCES tags (tag_id),
					UNIQUE (record_id, tag_id)
				);",
                @"CREATE TABLE IF NOT EXISTS atlas_tags
				(
					tag_id   INTEGER REFERENCES tags (tag_id),
					atlas_id INTEGER REFERENCES atlas_data (atlas_id),
					UNIQUE (atlas_id, tag_id)
				);",
                @"CREATE TABLE IF NOT EXISTS f95_zone_tags
				(
					f95_id INTEGER REFERENCES f95_zone_data (f95_id),
					tag_id INTEGER REFERENCES tags (tag_id)
				);",
                @"CREATE TABLE IF NOT EXISTS previews
				(
					record_id REFERENCES games (record_id),
					path     TEXT UNIQUE,
					position INTEGER DEFAULT 256,
					UNIQUE (record_id, path)
				);",
                @"CREATE TABLE IF NOT EXISTS banners
				(
					record_id REFERENCES games (record_id) UNIQUE PRIMARY KEY,
					path TEXT UNIQUE,
					type INTEGER,
					UNIQUE (record_id, path, type)
				);",
                @"CREATE TABLE IF NOT EXISTS data_change
				(
					timestamp INTEGER,
					delta     INTEGER
				);",
                @"CREATE TABLE IF NOT EXISTS f95_zone_mappings
				(
					record_id INTEGER REFERENCES games(record_id),
					f95_id INTEGER REFERENCES f95_zone_data(f95_id)
				);"
        };

    }
}
