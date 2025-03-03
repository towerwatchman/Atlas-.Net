using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit;
using System.Xml.Linq;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Reflection;
using System.Linq;
using System.ComponentModel;

namespace Atlas.UI.Windows
{
    public partial class XamlEditor : Window
    {
        private string initialXaml = @"<UserControl 
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <StackPanel>
        <Button Content=""Click Me"" Width=""100"" Height=""30"" Click=""Button_Click""/>
        <Image Source=""pack://application:,,,/Resources/sample.png"" Width=""100"" Height=""100""/>
        <TextBlock Text=""{Binding Message}"" FontSize=""16""/>
    </StackPanel>
</UserControl>";

        private string currentFilePath = null;
        private TestViewModel testViewModel = new TestViewModel();

        public XamlEditor()
        {
            InitializeComponent();
            CustomizeSyntaxHighlighting();
            this.tbXamlEditor.Text = initialXaml;
            UpdatePreview();
        }

        private void CustomizeSyntaxHighlighting()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Atlas.CustomXAML.xshd"))
            {
                if (stream != null)
                {
                    using (var reader = new System.Xml.XmlTextReader(stream))
                    {
                        this.tbXamlEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
        }

        private void XamlEditor_TextChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(UpdatePreview), DispatcherPriority.Background);
        }

        private void UpdatePreview()
        {
            try
            {
                var xDoc = XDocument.Parse(this.tbXamlEditor.Text);
                var root = xDoc.Root;
                if (root != null)
                {
                    // Remove x:Class attribute
                    var xClassAttr = root.Attribute(XName.Get("Class", "http://schemas.microsoft.com/winfx/2006/xaml"));
                    if (xClassAttr != null)
                    {
                        xClassAttr.Remove();
                    }

                    // Remove event handler attributes, but preserve x: attributes
                    foreach (var element in root.Descendants())
                    {
                        var eventAttrs = element.Attributes()
                            .Where(attr =>
                                attr.Name.NamespaceName != "http://schemas.microsoft.com/winfx/2006/xaml" &&
                                (attr.Name.LocalName == "Click" ||
                                 attr.Name.LocalName.StartsWith("Mouse") ||
                                 attr.Name.LocalName.StartsWith("Key") ||
                                 attr.Name.LocalName == "Loaded"))
                            .ToList();
                        foreach (var attr in eventAttrs)
                        {
                            attr.Remove();
                        }
                    }
                }

                var context = new ParserContext
                {
                    XmlnsDictionary =
                    {
                        { "", "http://schemas.microsoft.com/winfx/2006/xaml/presentation" },
                        { "x", "http://schemas.microsoft.com/winfx/2006/xaml" }
                    },
                    BaseUri = new Uri("pack://application:,,,") // Enable pack URI resolution
                };

                string modifiedXaml = xDoc.ToString();
                var newContent = XamlReader.Parse(modifiedXaml, context) as FrameworkElement; // Cast to FrameworkElement
                if (newContent != null)
                {
                    // Set the test view model as DataContext for bindings
                    newContent.DataContext = testViewModel;
                    PreviewControl.Content = newContent;
                }
            }
            catch (Exception ex)
            {
                PreviewControl.Content = new TextBlock
                {
                    Text = $"Error: {ex.Message}",
                    Foreground = System.Windows.Media.Brushes.Red,
                    Margin = new Thickness(5)
                };
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            currentFilePath = null;
            this.tbXamlEditor.Text = initialXaml;
            Title = "XAML Editor - New File";
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XAML files (*.xaml)|*.xaml|All files (*.*)|*.*",
                Title = "Open XAML File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    this.tbXamlEditor.Text = File.ReadAllText(openFileDialog.FileName);
                    currentFilePath = openFileDialog.FileName;
                    Title = $"XAML Editor - {Path.GetFileName(currentFilePath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "XAML files (*.xaml)|*.xaml|All files (*.*)|*.*",
                    Title = "Save XAML File",
                    DefaultExt = ".xaml"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    currentFilePath = saveFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            try
            {
                File.WriteAllText(currentFilePath, this.tbXamlEditor.Text);
                Title = $"XAML Editor - {Path.GetFileName(currentFilePath)}";
                MessageBox.Show("File saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Simple test view model for binding
    public class TestViewModel : INotifyPropertyChanged
    {
        private string _message = "Hello from ViewModel!";

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}