/*
MIT Creator Revision License v1.0 (MITCRL1.0)

Copyright (c) 2023 Tomasz Szynkar (tsx4k [TSX], tszynkar@tlen.pl, https://github.com/tsx4k)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute copies of the Software
and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

1. The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
2. There are no permissions, and/or no rights to fork, make similar Software,
sublicense, and/or sell copies of the Software, and/or any part of it.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls.Window;
using BrowserSelector.ViewModels;
using BrowserSelectorCommon.Interfaces;
using System.Security.Policy;
using BrowserSelectorCommon.Models;

namespace BrowserSelector.Views
{
    /// <summary>
    /// Interaction logic for SelectorWindow.xaml
    /// </summary>
    public partial class SelectorWindow : FluentWindow
    {
        SelectorWindowViewModel model;
        bool firstPrepare = true;
        private int SplashScreenDuration = 2000; // ms
        private int CloseDelay = 2000; // ms
        public bool CanShow { get; set; } = true;

        public SelectorWindow(string url, string optons)
        {
            InitializeComponent();
            url = BrowserSelectorCommon.Common.PrepareUrl(url);
            DataContext = model = new ViewModels.SelectorWindowViewModel(this) { URL = url, OriginalURL = url, Options = optons };
            model.ApplyTheme();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenBrowser(sender);
        }

        public void OpenBrowser(object sender)
        {
            var vm = ((SelectorWindowViewModel)DataContext);
            IBrowser browser = sender as IBrowser;
            ListBox listBox = sender as ListBox;
            if (listBox == null)
            {
                listBox = lbBrowsers;
            }
            if (browser == null && listBox != null)
            {
                browser = listBox.SelectedItem as IBrowser;
            }
            if (browser != null)
            {
                Hide();
                BrowserSelectorCommon.Common.SetLastSelectedBrowser(browser.ProgId);
                if (model.RememberChoice)
                {
                    BrowserSelectorCommon.Common.SaveChoice(browser, model.URL);
                }
                else
                {
                    BrowserSelectorCommon.Common.RemoveChoiceByUrl(model.URL);
                }
                BrowserSelectorCommon.Common.OpenBrowser(browser, vm.URL);

                DelayedClose();
            }
        }

        private async void DelayedClose()
        {
            await Task.Delay(CloseDelay);
            Close();
        }

        private void UiWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    this.DragMove();
                }
                catch { }
            }
        }

        private async Task PrepareWindow()
        {
            model.ApplyTheme();
            model.PrepareBrowsersList(true);

            var vm = ((SelectorWindowViewModel)DataContext);

            if (!firstPrepare)
            {

                // fix extra borders
                SizeToContent = SizeToContent.Manual;
                SizeToContent = SizeToContent.WidthAndHeight;
            }
            firstPrepare = false;
            IBrowser selectedBrowser = null;
            if (lbBrowsers.SelectedIndex < 0)
            {
                var progid = BrowserSelectorCommon.Common.GetLastSelectedBrowser();

                // URL features
                var url = vm.URL;
                var urlForLearning = url;
                if (BrowserSelectorCommon.Common.IsSafeLink(url, out string originalUrl))
                {
                    // Stripe
                    if (bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_STRIP_SAFELINKS) ?? "false"))
                    {
                        url = originalUrl;
                        vm.URL = url;
                        vm.UrlIsStripped = true;
                    }

                    // Handle Safelinks for Learning
                    if (bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_LEARN_HOSTS_SAFELINK) ?? "false"))
                    {
                        urlForLearning = originalUrl;
                    }
                }

                // Get Learning
                if (bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_LEARN_HOSTS) ?? "false"))
                {
                    string learnProgid = BrowserSelectorCommon.Common.GetLearning(urlForLearning);
                    if (!string.IsNullOrEmpty(learnProgid))
                    {
                        progid = learnProgid;
                    }
                }


                // Find Trackers
                url = HandleTrackers(url);


                if (!string.IsNullOrEmpty(progid))
                {
                    int index = -1;
                    foreach (var x in vm.Browsers)
                    {
                        index++;
                        if (x.ProgId == progid)
                        {
                            selectedBrowser = x;
                            lbBrowsers.SelectedIndex = index;
                            break;
                        }
                    }
                    if (lbBrowsers.SelectedIndex < 0 && lbBrowsers.Items.Count > 0)
                    {
                        lbBrowsers.SelectedIndex = 0;
                    }
                }
                else
                {
                    if (lbBrowsers.Items.Count > 0)
                    {
                        lbBrowsers.SelectedIndex = 0;
                    }
                }

                if (selectedBrowser != null)
                {
                    string choice = BrowserSelectorCommon.Common.GetChoice(url);
                    if(!string.IsNullOrEmpty(choice) && choice == progid)
                    {
                        model.SetRememberChoiceSilently(true);
                        KeyStates ksLShift = Keyboard.GetKeyStates(Key.LeftShift);
                        KeyStates ksRShift = Keyboard.GetKeyStates(Key.RightShift);
                        if (!ksLShift.HasFlag(KeyStates.Down) && !ksRShift.HasFlag(KeyStates.Down))
                        {
                            CanShow = false;
                            await ShowSplashScreen(TimeSpan.FromMilliseconds(SplashScreenDuration));
                            OpenBrowser(selectedBrowser);
                        }
                    }
                }
            }
            model.CanRememberChoice = bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_LEARN_HOSTS) ?? "false");
        }

        private string HandleTrackers(string url)
        {
            var vm = ((SelectorWindowViewModel)DataContext);

            var trackers = BrowserSelectorCommon.Common.FindTrackers(url);

            var except = new List<string>();
            

            var nurl = BrowserSelectorCommon.Common.RemoveTrackers(url, out List<Tracker> result, except);
            if (bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_REMOVE_TRACKERS) ?? "false"))
            {
                if (!string.IsNullOrEmpty(nurl) && result.Count > 0)
                {
                    vm.URLForTrackersService = vm.URL;
                    url = nurl;
                    vm.URL = url;
                    vm.UrlHasTrackersRemoved = true;
                }
            }
            vm.UrlHasTrackersFound = trackers.Count > 0;
            vm.UrlTrackersFound = trackers.Count;
            var trackersInfo = string.Empty;
            trackers.ForEach(x => trackersInfo += $"{x.Source} ({x.Key})\r\n");
            vm.UrlTrackersInfo = trackersInfo;
            vm.UrlTrackersData = trackers;

            return url;
        }

        private async Task ShowSplashScreen(TimeSpan duration)
        {
            SplashWindow splashWindow = new SplashWindow(duration);
            await splashWindow.ShowAsync(duration);
        }


        private async void UiWindow_Activated(object sender, EventArgs e)
        {
            await PrepareWindow();
        }

        private void UiWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape) {
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                if (lbBrowsers.SelectedIndex >= 0)
                {
                    OpenBrowser(lbBrowsers);
                }
            }
        }

        private void ListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_USE_SINGLE_CLICK) ?? "false"))
            {
                OpenBrowser(sender);
            }
        }

        private void TitleBar_SettingsClicked(object sender, RoutedEventArgs e)
        {
            model.SettingsCommand.Execute(this);
        }

        internal async Task DetectShowableAsync()
        {
            await PrepareWindow();
        }
    }
}
