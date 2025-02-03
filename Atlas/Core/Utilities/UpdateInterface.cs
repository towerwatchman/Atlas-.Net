using Atlas.UI;
using Newtonsoft.Json.Linq;

namespace Atlas.Core.Utilities
{
    public static class UpdateInterface
    {
        public static bool UpdateCompleted = false;
        public static async Task<Task> ParseUpdate(string data)
        {
            JObject dataObj = JObject.Parse(data);
            InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.UpdateProgressBar.Value = 100;
                InterfaceHelper.UpdateTextBox.Text = "100%";
            }));
            //This to to make sure the update was processed
            System.Threading.Thread.Sleep(1000);

            //We have to import atlas data before we import f95 data 

            InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.UpdateProgressBar.Value = 0;
                InterfaceHelper.UpdateTextBox.Text = "0%";
                InterfaceHelper.LauncherTextBox.Text = "Updating Atlas Metadata";
            }));

            var atlas_data = dataObj["atlas"];
            await Database.SQLiteInterface.InsertJsonData(atlas_data, "atlas_data");

            InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.UpdateProgressBar.Value = 0;
                InterfaceHelper.UpdateTextBox.Text = "0%";
                InterfaceHelper.LauncherTextBox.Text = "Updating F95 Metadata";
            }));

            var f95_data = dataObj["f95_zone"];
            var sqlcmd = Database.SQLiteInterface.InsertJsonData(f95_data, "f95_zone_data");

            UpdateCompleted = true;
            return Task.CompletedTask;
        }

    }
}
