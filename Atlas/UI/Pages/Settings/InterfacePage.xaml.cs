using System.Windows.Controls;

namespace Atlas.UI.Pages.Settings
{
    /// <summary>
    /// Interaction logic for InterfacePage.xaml
    /// </summary>
    public partial class InterfacePage : Page
    {
        public InterfacePage()
        {
            InitializeComponent();
        }

        private void ShowDebugConsole_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Atlas.Core.Settings.Config.ShowDebugWindow = (bool)ShowDebugConsole.IsChecked;
        }
    }
}
