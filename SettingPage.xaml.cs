using CustomObjects;
using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MyRSSReaderv2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public ObservableCollection<CustomFeed> Feeds = new ObservableCollection<CustomFeed>();
        private List<CustomFeed> _originalFeeds = new List<CustomFeed>();
        private CustomFeedEqualityComparer _customFeedEqualityComparer = new CustomFeedEqualityComparer();
        private static bool s_isFirstThemeRadioButtonsUpdate;

        public SettingPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingPageLoaded;

            themeRadioButtons.SelectionChanged += ThemeRadioButtonsSelectionChanged;
            removeButton.Click += RemoveButtonClick;
            addButton.Click += AddButtonClick;
            acceptButton.Click += AcceptButtonClick;
            Feeds.CollectionChanged += FeedsCollectionChanged;
            listView.SelectionChanged += ListViewSelectionChanged;
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count() > 0)
            {
                removeButton.IsEnabled = true;
            }
            else
            {
                removeButton.IsEnabled = false;
            }
        }

        private void FeedsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Feeds.Except(_originalFeeds, _customFeedEqualityComparer).Count() > 0 || _originalFeeds.Except(Feeds, _customFeedEqualityComparer).Count() > 0)
            {
                acceptButton.IsEnabled = true;
            }
            else
            {
                acceptButton.IsEnabled = false;
            }
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            LauchAddFeedDialogAsync();
        }

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFeedsAsync();
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            RemoveFeedsAsync();
        }

        private async void SettingPageLoaded(object sender, RoutedEventArgs e)
        {
            s_isFirstThemeRadioButtonsUpdate = true;
            await UpdateThemeRadioButtonsAsync();
            await UpdateFeedsAsync();
        }

        private void ThemeRadioButtonsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveAppThemeSettingAsync(themeRadioButtons.SelectedIndex);
        }

        private async void SaveAppThemeSettingAsync(int selectedTheme)
        {
            if (!s_isFirstThemeRadioButtonsUpdate && selectedTheme != -1)
            {
                switch (selectedTheme)
                {
                    case 0:
                        ThemeServices.SaveAppThemeSetting(1);
                        break;

                    case 1:
                        ThemeServices.SaveAppThemeSetting(2);
                        break;

                    case 2:
                        ThemeServices.SaveAppThemeSetting(0);
                        break;
                }
                if (await new ContentDialog
                {
                    Title = "Success",
                    Content = "Please restart to see the changes",
                    PrimaryButtonText = "Restart",
                    CloseButtonText = "Cancel"
                }.ShowAsync() == ContentDialogResult.Primary)
                {
                    await CoreApplication.RequestRestartAsync("");
                }
            }
            else if (s_isFirstThemeRadioButtonsUpdate)
            {
                s_isFirstThemeRadioButtonsUpdate = false;
            }
        }
        private async Task UpdateThemeRadioButtonsAsync()
        {
            try
            {
                switch (ThemeServices.GetAppThemeSetting())
                {
                    case 0:
                        themeRadioButtons.SelectedIndex = 2;
                        break;
                    
                    case 1:
                        themeRadioButtons.SelectedIndex = 0;
                        break;

                    case 2:
                        themeRadioButtons.SelectedIndex = 1;
                        break;
                }
            }
            catch (Exception)
            {
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occured while getting app theme",
                    CloseButtonText = "Ok"
                }.ShowAsync();
            }
        }
        private async Task UpdateFeedsAsync()
        {
            try
            {
                foreach (CustomFeed feed in await FeedServices.GetSavedFeedsAsync())
                {
                    _originalFeeds.Add(feed);
                    Feeds.Add(feed);
                } 
            }
            catch (Exception)
            {
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occured while getting saved feeds",
                    CloseButtonText = "Ok"
                }.ShowAsync();
            }
        }
        private async void RemoveFeedsAsync()
        {
            try
            {
                foreach (CustomFeed feed in listView.SelectedItems.ToList())
                {
                    Feeds.Remove(feed);
                }
            }
            catch (Exception)
            {
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occured while removing feeds",
                    CloseButtonText = "Ok"
                }.ShowAsync();
            }
        }
        private async void SaveFeedsAsync()
        {
            acceptButton.IsEnabled = false;
            try
            {
                await Task.Run(() =>
                {
                    _originalFeeds.Clear();
                    foreach (CustomFeed feed in Feeds)
                    {
                        _originalFeeds.Add(feed);
                    }
                    FeedServices.SaveFeedsAsync(_originalFeeds);
                });
            }
            catch (Exception)
            {
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occured while saving changes",
                    CloseButtonText = "Ok"
                }.ShowAsync();
            }
        }

        private async void LauchAddFeedDialogAsync()
        {
            var dialogContent = new AddFeedDialogContent();
            var addFeedDialog = new ContentDialog
            {
                Title = "Add new feed source",
                PrimaryButtonText = "Ok",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                Content = dialogContent
            };
            if (await addFeedDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                progressRing.IsActive = true;
                await CheckFeedUrlAsync(dialogContent.GetFeedUrl());
                progressRing.IsActive = false;
            }
            
        }

        private async Task CheckFeedUrlAsync(string url)
        {
            CustomFeed newFeed = new CustomFeed();
            try
            {
                newFeed = await FeedServices.GetFeedAsync(url);
            }
            catch (Exception)
            {
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occured while adding the feed, please check the URL again",
                    CloseButtonText = "Ok"
                }.ShowAsync();
                return;
            }
            if (Feeds.Where(x => x.Link == newFeed.Link).Count() == 0)
            {
                Feeds.Add(newFeed);
                await new ContentDialog
                {
                    Title = "Success",
                    Content = "The feed has been successfully added",
                    CloseButtonText = "Ok"
                }.ShowAsync();
            }
        }
    }
}
