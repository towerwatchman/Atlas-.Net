using Atlas.Core;
using Atlas.UI;
using Atlas.UI.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Media.Capture;

namespace Atlas.UI.Pages
{
    /// <summary>
    /// Interaction logic for BannerViewPage.xaml
    /// </summary>
    public partial class BannerViewPage : Page
    {
        public BannerViewPage()
        {
            InitializeComponent();
        }
    }
    public sealed class MyScrollViewer : ScrollViewer
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static int ItemsInView { get; set; }
        public static readonly DependencyProperty IsInViewportProperty =
            DependencyProperty.RegisterAttached("IsInViewport", typeof(bool), typeof(MyScrollViewer));

        public static bool GetIsInViewport(UIElement element)
        {
            return (bool)element.GetValue(IsInViewportProperty);
        }

        public static void SetIsInViewport(UIElement element, bool value, GameViewModel game)
        {
            if(value == false)
            {

            }
            element.SetValue(IsInViewportProperty, value);
            if (game != null)
            {
                GameViewModel gameObj = ModelData.GameCollection.Where(x => x.RecordID == game.RecordID).FirstOrDefault();
                var index = ModelData.GameCollection.IndexOf(gameObj);

                if (gameObj != null)
                {
                    ModelData.GameCollection[index].BannerImage = null;
                    
                }
            }
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            ItemsInView = 0;
            base.OnScrollChanged(e);

            var panel = Content as Panel;
            if (panel == null)
            {
                return;
            }

            Rect viewport = new Rect(new Point(0, 0), RenderSize);

            foreach (UIElement child in panel.Children)
            {
                ListViewItem item = child as ListViewItem;
                if(item.Content is GameViewModel) { 
                GameViewModel game = (GameViewModel)item.Content;

                if (!child.IsVisible)
                {
                    SetIsInViewport(child, false, game );
                    continue;
                }
                else
                {
                    if (item.Content != null)
                    {
                        try
                        {
                            
                            ItemsInView++;
                           // Logger.Info($"Title:{game.Title} ID:{game.RecordID}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex);
                        }

                    }

                }
                }
                //Get the current Game object that is in view and list it


                GeneralTransform transform = child.TransformToAncestor(this);
                Rect childBounds = transform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
                SetIsInViewport(child, viewport.IntersectsWith(childBounds), null);
            }
            Logger.Warn($"Total Items in View: {ItemsInView}");
        }
    }
}
