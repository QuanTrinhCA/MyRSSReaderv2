using CodeHollow.FeedReader;
using CustomObjects;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Services
{
    public class SyncServices
    {

    }
    public class FeedServices
    {
        private static List<CustomFeed> s_feeds = new List<CustomFeed>();

        public static async Task<CustomFeed> GetFeedAsync(string inputUrl)
        {
            return await Task.Run(async () =>
            {
                var urls = await FeedReader.GetFeedUrlsFromUrlAsync(inputUrl);
                string feedUrl;
                if (urls.Count() == 0)
                {
                    feedUrl = inputUrl;
                }
                else
                {
                    feedUrl = urls.First().Url;
                }
                var feed = await FeedReader.ReadAsync(feedUrl);
                string imageLink;
                if (feed.ImageUrl != null)
                {
                    imageLink = feed.ImageUrl;
                }
                else
                {
                    imageLink = "ms-appx:///Assets/Placeholder.png";
                }
                return new CustomFeed()
                {
                    Title = feed.Title,
                    ImageLink = imageLink,
                    Description = feed.Description,
                    Link = feedUrl
                };
            });
        }

        public static async Task<List<CustomFeedItem>> GetFeedItemsAsync(List<CustomFeedItem> oldFeedItems)
        {
            return await Task.Run(async () =>
            {
                var newFeedItems = new List<CustomFeedItem>();
                foreach (CustomFeed savedFeed in s_feeds)
                {
                    Feed feed = new Feed();
                    bool readSuccessful = true;
                    try
                    {
                        feed = await FeedReader.ReadAsync(savedFeed.Link);
                    }
                    catch (Exception)
                    {
                        readSuccessful = false;
                    }
                    if (readSuccessful)
                    {
                        foreach (FeedItem feedItem in feed.Items)
                        {
                            if (oldFeedItems.Where(x => x.Link == feedItem.Link).Count() == 0)
                            {
                                var doc = new HtmlDocument();
                                doc.LoadHtml(feedItem.Description);
                                string imageLink;
                                if (doc.DocumentNode.SelectSingleNode("//img[1]") != null)
                                {
                                    imageLink = doc.DocumentNode.SelectSingleNode("//img[1]").Attributes["src"].Value;
                                }
                                else if (feed.ImageUrl != null)
                                {
                                    imageLink = feed.ImageUrl;
                                }
                                else
                                {
                                    imageLink = "ms-appx:///Assets/Placeholder.png";
                                }
                                newFeedItems.Add(new CustomFeedItem()
                                {
                                    Publisher = feed.Title,
                                    ImageLink = imageLink,
                                    Title = feedItem.Title,
                                    Link = feedItem.Link,
                                    PublishingDate = feedItem.PublishingDate.Value,
                                    PublishingDateString = feedItem.PublishingDate.Value.Date.ToLocalTime().ToLongDateString(),
                                    Categories = feedItem.Categories
                                });
                            }
                        }
                    }
                }
                return newFeedItems.OrderBy(x => x.PublishingDate).ToList();
            });
        }

        public static async Task<List<CustomFeed>> GetSavedFeedsAsync()
        {
            return await Task.Run(() =>
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.CreateContainer("feeds", ApplicationDataCreateDisposition.Always);
                foreach (var key in localSettings.Containers["feeds"].Values.Keys)
                {
                    var composite = localSettings.Containers["feeds"].Values[key] as ApplicationDataCompositeValue;
                    var feed = new CustomFeed
                    {
                        Title = composite["Title"].ToString(),
                        Description = composite["Description"].ToString(),
                        ImageLink = composite["ImageLink"].ToString(),
                        Link = composite["Link"].ToString()
                    };
                    if (s_feeds.Where(x => x.Link == feed.Link).Count() == 0)
                    {
                        s_feeds.Add(feed);
                    }
                }
                s_feeds = s_feeds.OrderBy(x => x.Title).ToList();
                return s_feeds;
            });
        }

        public static async void SaveFeedsAsync(List<CustomFeed> feeds)
        {
            await Task.Run(() =>
            {
                s_feeds = feeds;
                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.DeleteContainer("feeds");
                localSettings.CreateContainer("feeds", ApplicationDataCreateDisposition.Always);
                for (int i = 0; i < feeds.Count(); i++)
                {
                    var composite = new ApplicationDataCompositeValue
                    {
                        ["Title"] = feeds[i].Title,
                        ["Description"] = feeds[i].Description,
                        ["ImageLink"] = feeds[i].ImageLink,
                        ["Link"] = feeds[i].Link
                    };
                    localSettings.Containers["feeds"].Values.Add("feed" + i.ToString(), composite);
                }
            });
        }
    }

    public class HtmlServices
    {
        public static async Task<CustomArticleItem> GetFeedItemContentAsync(string link, ElementTheme theme)
        {
            return await Task.Run(async () =>
            {
                string meta = "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">";

                string styles;
                var accentLight3Color = new UISettings().GetColorValue(UIColorType.AccentLight3);
                if (theme == ElementTheme.Dark)
                {
                    styles = string.Format("<style>html{{font-family:'Segoe UI'; color:white; margin: auto; width: 55%;}} @media only screen and (max-width: 1000px){{html{{font-family:'Segoe UI'; color:white; margin: auto; width: 65%;}}}} @media only screen and (max-width: 800px){{html{{font-family:'Segoe UI'; color:white; margin: auto; width: 75%;}}}} @media only screen and (max-width: 600px){{html{{font-family:'Segoe UI'; color:white; margin: auto; width: 90%;}}}} a{{color:rgba({0}, {1}, {2}, {3});}} a:hover{{color:rgba({0}, {1}, {2}, {3});}} a:active{{color:rgba({0}, {1}, {2}, {3});}} iframe{{width:100%!important; height:100%!important;}} img, picture{{max-width:100%!important; height:auto!important;}}</style>", accentLight3Color.R, accentLight3Color.G, accentLight3Color.B, accentLight3Color.A);
                }
                else
                {
                    styles = string.Format("<style>html{{font-family:'Segoe UI'; color:black; margin: auto; width: 55%;}} @media only screen and (max-width: 1000px){{html{{font-family:'Segoe UI'; color:black; margin: auto; width: 65%;}}}} @media only screen and (max-width: 800px){{html{{font-family:'Segoe UI'; color:black; margin: auto; width: 75%;}}}} @media only screen and (max-width: 600px){{html{{font-family:'Segoe UI'; color:black; margin: auto; width: 90%;}}}} a{{color:rgba({0}, {1}, {2}, {3});}} a:hover{{color:rgba({0}, {1}, {2}, {3});}} a:active{{color:rgba({0}, {1}, {2}, {3});}} iframe{{width:100%!important; height:100%!important;}} img, picture{{max-width:100%!important; height:auto!important;}}</style>", accentLight3Color.R, accentLight3Color.G, accentLight3Color.B, accentLight3Color.A);
                }

                string content = "";
                HtmlDocument htmlDocument = new HtmlWeb().Load(link);
                content += htmlDocument.DocumentNode.QuerySelector("h1").OuterHtml;
                string title = htmlDocument.DocumentNode.QuerySelector("h1").InnerText;
                HtmlNode mainContentNode;
                if (htmlDocument.DocumentNode.QuerySelectorAll("article").Count() > 0)
                {
                    mainContentNode = htmlDocument.DocumentNode.QuerySelector("article");
                }
                else if (htmlDocument.DocumentNode.QuerySelectorAll("main").Count() > 0)
                {
                    mainContentNode = htmlDocument.DocumentNode.QuerySelector("main");
                }
                else
                {
                    mainContentNode = htmlDocument.DocumentNode.QuerySelector("body");
                }
                IEnumerable<HtmlNode> allContentNodes;
                IEnumerable<HtmlNode> textNodes = mainContentNode.QuerySelectorAll("h2, p");
                IEnumerable<HtmlNode> figureNodes = mainContentNode.QuerySelectorAll("figure");
                IEnumerable<HtmlNode> imageNodes;
                if (figureNodes.Count() > 0)
                {
                    foreach (var figureNode in figureNodes)
                    {
                        HtmlNodeCollection figureChildren = new HtmlNodeCollection(figureNode);
                        foreach (var node in figureNode.QuerySelectorAll("img, figcaption").OrderBy(x => x.OuterStartIndex))
                        {
                            figureChildren.Add(node);
                        }
                        figureNode.RemoveAllChildren();
                        figureNode.PrependChildren(figureChildren);
                    }
                    allContentNodes = textNodes.Concat(figureNodes);
                }
                else
                {
                    imageNodes = mainContentNode.QuerySelectorAll("img");
                    allContentNodes = textNodes.Concat(imageNodes);
                }

                for (int i = allContentNodes.Count() - 1; i >= 0; i--)
                {
                    var node = allContentNodes.ElementAt(i);
                    if (node.QuerySelector("button") != null)
                    {
                        node.Remove();
                    }
                    if (node.Name == "img" && (await new HttpClient().GetAsync(node.Attributes["src"].Value, HttpCompletionOption.ResponseHeadersRead)).Content.Headers.ContentLength > 75000)
                    {
                        node.Remove();
                    }
                    if (node.Name == "img" && (node.OuterHtml.Contains("placeholder") || node.OuterHtml.Contains("data-src")))
                    {
                        node.Remove();
                        continue;
                    }
                    if (node.Name == "p" && node.InnerText == "")
                    {
                        node.Remove();
                        continue;
                    }
                }
                allContentNodes = allContentNodes.OrderBy(x => x.OuterStartIndex);

                if (allContentNodes.Last().Name == "h2" || allContentNodes.Last().Name == "h1")
                {
                    allContentNodes.Last().Remove();
                }

                for (int i = allContentNodes.Count() - 1; i >= 0; i--)
                {
                    var node = allContentNodes.ElementAt(i);
                    if ((node.Name == "h2" || node.Name == "h1") && node.Name == allContentNodes.ElementAt(i - 1).Name)
                    {
                        allContentNodes.ElementAt(i - 1).Remove();
                    }
                }

                foreach (var node in allContentNodes)
                {
                    content += node.OuterHtml;
                }

                return new CustomArticleItem()
                {
                    Title = title,
                    Link = link,
                    HtmlContent = meta + styles + content
                };
            });
        }
    }
    public class ThemeServices
    {
        public static int GetAppThemeSetting()
        {
            if (ApplicationData.Current.LocalSettings.Values["theme"] != null)
            {
                return (int)ApplicationData.Current.LocalSettings.Values["theme"];
            }
            else
            {
                return 0;
            }
        }
        public static void SaveAppThemeSetting(int theme)
        {
            ApplicationData.Current.LocalSettings.Values["theme"] = theme;
        }
    }
}