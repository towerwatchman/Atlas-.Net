using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.UI.ViewModel
{
    public sealed class NotificationViewModel : INotifyPropertyChanged
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public NotificationViewModel(Notification notification) 
        {
            Title = notification.Title;
            Version = notification.Version;
            ProgressBarValue = notification.ProgressBarValue;
            Status = notification.Status;
        }

        public string Title { get; set; }
        public string Version { get; set; }
        public int ProgressBarValue { get; set; }
        public string Status { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Logger.Warn($"Property Changed: {propertyName}");
        }
    }

    public class Notification
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public int ProgressBarValue { get; set; }
        public string Status { get; set; }
    }
}
