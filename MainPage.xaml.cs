using CustomObjects;
using Services;
using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyRSSReaderv2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CustomFeedItem _selectedFeedItem;

        public MainPage()
        {
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += TitleBarLayoutMetricsChanged;

            Window.Current.Activated += AppActivated;

            this.Loaded += MainPageLoaded;

            backButton.Click += BackButtonClicked;
            openReaderPageButton.Click += OpenReaderPageButtonClicked;
            openWithBrowserButton.Click += OpenWithBrowserButtonClicked;
            shareButton.Click += ShareButtonClicked;
            openSettingPageButton.Click += OpenSettingButtonClicked;
            frame.Navigated += FrameNavigated;

            HomePage.FeedItemsListViewDoubleTapped += HomePageFeedItemsListViewDoubleTapped;
            HomePage.FeedItemsListViewSelectionChangedOrReset += HomePageFeedItemsListViewSelectionChangedOrReset;

            ReaderPage.ArticleWebViewNavigatingToNewContent += ReaderPageArticleWebViewNavigatingToNewContent;

            DataTransferManager.GetForCurrentView().DataRequested += DataTransferManagerDataRequested; ;
        }

        private void ReaderPageArticleWebViewNavigatingToNewContent(object sender, CustomArticleItem e)
        {
            _selectedFeedItem = new CustomFeedItem()
            {
                Title = e.Title,
                Link = e.Link
            };
        }

        private void AppActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                appTitleTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                appTitleTextBlock.ClearValue(ForegroundProperty);
            }
        }

        private void TitleBarLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            appTitleTextBlock.Height = sender.Height;
        }

        private void FrameNavigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (!frame.CanGoBack)
            {
                backButton.IsEnabled = false;
            }
            else
            {
                backButton.IsEnabled = true;
            }

            if (e.Content.GetType() == typeof(SettingPage))
            {
                openSettingPageButton.IsEnabled = false;
                openReaderPageButton.IsEnabled = false;
                openWithBrowserButton.IsEnabled = false;
                shareButton.IsEnabled = false;
            }
            else
            {
                openSettingPageButton.IsEnabled = true;
                if (e.Content.GetType() == typeof(ReaderPage))
                {
                    openReaderPageButton.IsEnabled = false;
                }
            }
        }

        private void ShareButtonClicked(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManagerDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetWebLink(new Uri(_selectedFeedItem.Link));
            args.Request.Data.Properties.Title = _selectedFeedItem.Title;
        }

        private void OpenWithBrowserButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenLinkInBrowserAsync();
        }

        private void HomePageFeedItemsListViewSelectionChangedOrReset(object sender, CustomFeedItem e)
        {
            if (e == null)
            {
                _selectedFeedItem = null;
                openReaderPageButton.IsEnabled = false;
                openWithBrowserButton.IsEnabled = false;
                shareButton.IsEnabled = false;
            }
            else
            {
                _selectedFeedItem = e;
                openReaderPageButton.IsEnabled = true;
                openWithBrowserButton.IsEnabled = true;
                shareButton.IsEnabled = true;
            }
        }

        private void HomePageFeedItemsListViewDoubleTapped(object sender, CustomFeedItem e)
        {
            NavigateToReaderPage(e.Link);
        }

        private async void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            _ = await FeedServices.GetSavedFeedsAsync();
            NavigateToHomePage();
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            NavigateToPreviousPage();
        }
        private void OpenReaderPageButtonClicked(object sender, RoutedEventArgs e)
        {
            NavigateToReaderPage(_selectedFeedItem.Link);
        }
        private void OpenSettingButtonClicked(object sender, RoutedEventArgs e)
        {
            NavigateToSettingPage();
        }
        private void NavigateToPreviousPage()
        {
            if (frame.CanGoBack)
            {
                frame.GoBack();
            }
        }
        private void NavigateToHomePage()
        {
            frame.Navigate(typeof(HomePage));
        }
        private void NavigateToReaderPage(string link)
        {
            if (frame.Content.GetType() != typeof(ReaderPage))
            {
                frame.Navigate(typeof(ReaderPage), link);
            }
        }
        private void NavigateToSettingPage()
        {
            if (frame.Content.GetType() != typeof(SettingPage))
            {
                frame.Navigate(typeof(SettingPage));
            }
        }
        private async void OpenLinkInBrowserAsync()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(_selectedFeedItem.Link));
        }
    }
}
