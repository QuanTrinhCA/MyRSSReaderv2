using CustomObjects;
using Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MyRSSReaderv2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<CustomFeedItem> FeedItems = new ObservableCollection<CustomFeedItem>();

        public static event EventHandler<CustomFeedItem> FeedItemsListViewDoubleTapped;
        public static event EventHandler<CustomFeedItem> FeedItemsListViewSelectionChangedOrReset;

        public HomePage()
        {
            this.InitializeComponent();
            this.Loaded += HomePageLoaded;

            feedItemsListView.DoubleTapped += ListViewDoubleTapped;
            feedItemsListView.SelectionChanged += ListViewSelectionChanged;
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NotifyListViewSelectionChangedOrReset(feedItemsListView.SelectedItem);
        }

        private void ListViewDoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            FeedItemsListViewDoubleTapped(sender, feedItemsListView.SelectedItem as CustomFeedItem);
        }

        private void HomePageLoaded(object sender, RoutedEventArgs e)
        {
            SetupPageAsync();
        }
        private async void SetupPageAsync()
        {
            NotifyListViewSelectionChangedOrReset(feedItemsListView.SelectedItem);
            await FeedServices.GetSavedFeedsAsync();
            foreach (CustomFeedItem feedItem in await FeedServices.GetFeedItemsAsync(FeedItems.ToList()))
            {
                FeedItems.Insert(0, feedItem);
            }
        }
        private void NotifyListViewSelectionChangedOrReset(object selectedItem)
        {
            FeedItemsListViewSelectionChangedOrReset(this, selectedItem as CustomFeedItem);
        }
    }
}
