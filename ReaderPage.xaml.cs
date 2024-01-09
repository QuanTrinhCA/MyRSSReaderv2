using CustomObjects;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MyRSSReaderv2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReaderPage : Page
    {
        public static event EventHandler<CustomArticleItem> ArticleWebViewNavigatingToNewContent;

        private bool _isArticleFormated = true;
        private string _originalBaseUrl = "";

        public ReaderPage()
        {
            this.InitializeComponent();
            this.Unloaded += ReaderPageUnloaded;

            articleWebView.NavigationStarting += ArticleWebViewNavigationStarting;
            articleWebView.NavigationCompleted += ArticleWebViewNavigationCompleted;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _originalBaseUrl = new Uri(e.Parameter as string).GetLeftPart(UriPartial.Authority);
            RenderArticle(e.Parameter as string);
        }

        private void ArticleWebViewNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            progressRing.IsActive = false;
        }

        private void ArticleWebViewNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (!_isArticleFormated && args.Uri != new Uri("about:blank"))
            {
                args.Cancel = true;
                string navigationUrl = args.Uri.ToString();
                if (!args.Uri.IsAbsoluteUri)
                {
                    navigationUrl = _originalBaseUrl + navigationUrl;
                }
                RenderArticle(navigationUrl);
            }
        }

        private void ReaderPageUnloaded(object sender, RoutedEventArgs e)
        {
            //Fuckery hack for og webview to somewhat return the memory used.
            for (int i = 0; i < 10; i++)
            {
                articleWebView.Source = new Uri("about:blank");
            }
            GC.Collect();
        }

        private async void RenderArticle(string link)
        {
            progressRing.IsActive = true;
            var articleItem = await HtmlServices.GetFeedItemContentAsync(link, this.ActualTheme);
            _isArticleFormated = true;
            ArticleWebViewNavigatingToNewContent(this, articleItem);
            articleWebView.NavigateToString(articleItem.HtmlContent);
            _isArticleFormated = false;
        }
    }
}