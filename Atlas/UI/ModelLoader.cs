using Atlas.Core.Database;

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
