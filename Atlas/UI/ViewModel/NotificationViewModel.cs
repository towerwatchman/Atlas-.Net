using Atlas.Core.Database;
using Atlas.Core;
using NLog;
using System.ComponentModel;

namespace Atlas.UI.ViewModel
{
    public sealed class NotificationViewModel : INotifyPropertyChanged
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public NotificationViewModel(Notification notification)
        {
            Id = notification.Id;
            Title = notification.Title;
            Version = notification.Version;
            ProgressBarValue = notification.ProgressBarValue;
            ProgressBarMaximum = notification.ProgressBarMaximum;
            FileProgressBarValue = notification.FileProgressBarValue;
            FileProgressBarMaximum = notification.FileProgressBarMaximum;
            Status = notification.Status;
            StatusUpdate = notification.StatusUpdate;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public int ProgressBarValue { get; set; }
        public int ProgressBarMaximum { get; set; }
        public int FileProgressBarValue { get; set; }
        public int FileProgressBarMaximum { get; set; }
        public string Status { get; set; }
        public string StatusUpdate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Logger.Warn($"Property Changed: {propertyName}");
        }
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public int ProgressBarValue { get; set; }
        public string Status { get; set; }
        public int ProgressBarMaximum { get; set; }
        public int FileProgressBarValue { get; set; }
        public int FileProgressBarMaximum { get; set; }
        public string StatusUpdate { get; internal set; }
    }

    public static class Notifications
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int currentId { get; set; }

        public static void UpdateNotification(int id, int ProgressBarMaximum, int ProgressBarValue, string Status, string StatusUpdate)
        {
            NotificationViewModel notificationObj = ModelData.NotificationCollection.Where(x => x.Id == id).FirstOrDefault();
            var index = ModelData.NotificationCollection.IndexOf(notificationObj);

            if (notificationObj != null)
            {
                try
                {

                    InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                    {
                        ModelData.NotificationCollection[index].ProgressBarMaximum = ProgressBarMaximum;
                        ModelData.NotificationCollection[index].ProgressBarValue = ProgressBarValue;
                        if (Status != "")
                        {
                            ModelData.NotificationCollection[index].Status = Status;
                        }
                        if (StatusUpdate != "")
                        {
                            ModelData.NotificationCollection[index].StatusUpdate = StatusUpdate;
                        }
                        notificationObj.OnPropertyChanged("ProgressBarValue");
                        notificationObj.OnPropertyChanged("ProgressBarMaximum");
                        notificationObj.OnPropertyChanged("Status");
                        notificationObj.OnPropertyChanged("StatusUpdate");
                        InterfaceHelper.NotificationsPage.Items.Refresh();
                    }));
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.ToString());
                }
            }
        }

        public static void UpdateFileNotification(int id, int FileProgressBarMaximum, int FileProgressBarValue)
        {
            NotificationViewModel notificationObj = ModelData.NotificationCollection.Where(x => x.Id == id).FirstOrDefault();
            var index = ModelData.NotificationCollection.IndexOf(notificationObj);

            if (notificationObj != null)
            {
                try
                {
                    InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                    {
                        ModelData.NotificationCollection[index].FileProgressBarMaximum = FileProgressBarMaximum;
                        ModelData.NotificationCollection[index].FileProgressBarValue = FileProgressBarValue;
                        notificationObj.OnPropertyChanged("FileProgressBarValue");
                        notificationObj.OnPropertyChanged("FileProgressBarMaximum");
                        InterfaceHelper.NotificationsPage.Items.Refresh();
                    }));
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.ToString());
                }
            }
        }


    }
}
