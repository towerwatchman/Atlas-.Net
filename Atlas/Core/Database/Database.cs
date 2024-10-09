using System.IO;
using Microsoft.Data.Sqlite;

namespace Atlas.Core.Database
{
    static class Database
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
                catch(Exception ex)
                {
                    Logging.Logger.Error(ex);
                }
            }
        }

    }
}
