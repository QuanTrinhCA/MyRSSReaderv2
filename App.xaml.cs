﻿using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyRSSReaderv2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            ChangeApplicationTheme();
        }

        private void ChangeApplicationTheme()
        {
            switch (ThemeServices.GetAppThemeSetting())
            {
                case 1:
                    this.RequestedTheme = ApplicationTheme.Light;
                    break;
                case 2:
                    this.RequestedTheme = ApplicationTheme.Dark;
                    break;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();

                ChangeTitleBarTheme(rootFrame.ActualTheme);
                rootFrame.ActualThemeChanged += RootFrameActualThemeChanged;
            }
        }

        private void RootFrameActualThemeChanged(FrameworkElement sender, object args)
        {
            ChangeTitleBarTheme(sender.ActualTheme);
        }

        private void ChangeTitleBarTheme(ElementTheme theme)
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            switch (theme)
            {
                case ElementTheme.Light:
                    titleBar.ButtonForegroundColor = Windows.UI.Colors.Black;
                    titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                    titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
                    titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(195, 0, 0, 0);
                    titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.LightGray;
                    titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(188, 0, 0, 0);

                    titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.Gray;
                    titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
                    break;

                case ElementTheme.Dark:
                    titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
                    titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                    titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
                    titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(15, 255, 255, 255);
                    titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.LightGray;
                    titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(08, 255, 255, 255);

                    titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.Gray;
                    titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
                    break;
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
