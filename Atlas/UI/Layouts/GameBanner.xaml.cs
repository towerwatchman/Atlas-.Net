using System.Diagnostics;
using System.Windows.Controls;

namespace Atlas.UI
{
    /// <summary>
    /// Interaction logic for GameBanner.xaml
    /// </summary>
    public partial class GameBanner : UserControl
    {
        public GameBanner()
        {
            InitializeComponent();
        }

        /*private void UpdateAvailable_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button obButton = e.OriginalSource as Button;

            Process.Start(new ProcessStartInfo
            {
                FileName = obButton.Tag.ToString(),
                UseShellExecute = true
            });
        }*/
    }
}
