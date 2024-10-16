using Atlas.Core;
using Atlas.Core.Database;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.UI
{
    public class ModelLoader
    {
        public async Task<Task> CreateGamesList(string defaultPage)
        {
            await SQLiteInterface.BuildGameListDetails();

            return Task.CompletedTask;
        }

    }
}
