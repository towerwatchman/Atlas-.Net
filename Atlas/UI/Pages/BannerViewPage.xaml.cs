using Atlas.UI.ViewModel;
using NLog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        public static List<int> previousItemsInView = new List<int>();
        //public static List<int> ItemsInView = new List<int>();

        public static readonly DependencyProperty IsInViewportProperty =
            DependencyProperty.RegisterAttached("IsInViewport", typeof(bool), typeof(MyScrollViewer));

        public static bool GetIsInViewport(UIElement element)
        {
            return (bool)element.GetValue(IsInViewportProperty);
        }

        public static void SetIsInViewport(UIElement element, bool value, GameViewModel game)
        {
            element.SetValue(IsInViewportProperty, value);
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            //ItemsInView = 0;
            //base.OnScrollChanged(e);

            var panel = Content as Panel;
            if (panel == null)
            {
                return;
            }

            Rect viewport = new Rect(new Point(0, 0), RenderSize);
            GameViewModel.BannersInView.Clear();
            foreach (UIElement child in panel.Children)
            {

                ListViewItem item = child as ListViewItem;
                if (item.Content is GameViewModel)
                {
                    GameViewModel game = (GameViewModel)item.Content;

                    if (!child.IsVisible)
                    {
                        SetIsInViewport(child, false, game);
                        continue;
                    }
                    else
                    {
                        if (item.Content != null)
                        {
                            try
                            {

                                GameViewModel.BannersInView.Add(game.RecordID);

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


            foreach (int id in previousItemsInView)
            {
                //Logger.Error(id);
                //check if is is not in view
                if (!GameViewModel.BannersInView.Any(s => s == id))
                {
                    GameViewModel gameObj = ModelData.GameCollection.Where(x => x.RecordID == id).FirstOrDefault();
                    var index = ModelData.GameCollection.IndexOf(gameObj);

                    if (gameObj != null)
                    {
                        //Logger.Warn($"Removing image for id: {id}");
                        ModelData.GameCollection[index].BannerImage = null;
                        ModelData.GameCollection[index].InView = false;
                    }
                }
            }
            //Logger.Warn($"Total Items in View: {GameViewModel.BannersInView.Count} previtems: {previousItemsInView.Count}");

            previousItemsInView.Clear();
            previousItemsInView = GameViewModel.BannersInView.ToList();

            //previousItemsInView = ItemsInView;
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
