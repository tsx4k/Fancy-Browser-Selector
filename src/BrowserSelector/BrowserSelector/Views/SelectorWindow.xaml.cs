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

namespace BrowserSelector.Views
{
    /// <summary>
    /// Interaction logic for SelectorWindow.xaml
    /// </summary>
    public partial class SelectorWindow : FluentWindow
    {
        SelectorWindowViewModel model;

        public SelectorWindow()
        {
            InitializeComponent();
            DataContext = model = new ViewModels.SelectorWindowViewModel(this);
            model.ApplyTheme();
        }

        public SelectorWindow(string url)
        {
            InitializeComponent();
            DataContext = model = new ViewModels.SelectorWindowViewModel(this) { URL = url, OriginalURL = url };
            model.ApplyTheme();

            this.Activated += SelectorWindow_Activated;
        }

        private void SelectorWindow_Activated(object sender, EventArgs e)
        {
            model.ApplyTheme();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenBrowser(sender);
        }

        public void OpenBrowser(object sender)
        {
            var listBox = sender as ListBox;
            if (listBox == null) 
            {
                listBox = lbBrowsers;
            }
            if (listBox != null)
            {
                var browser = listBox.SelectedItem as IBrowser;
                if (browser != null)
                {
                    Hide();
                    BrowserSelectorCommon.Common.SetLastSelectedBrowser(browser.ProgId);
                    BrowserSelectorCommon.Common.OpenBrowser(browser, ((SelectorWindowViewModel)DataContext).URL);
                    Close();
                }
            }
        }

        private void UiWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void UiWindow_Activated(object sender, EventArgs e)
        {
            model.PrepareBrowsersList(true);

            if (lbBrowsers.SelectedIndex < 0)
            {
                var progid = BrowserSelectorCommon.Common.GetLastSelectedBrowser();

                // URL features
                var url = ((SelectorWindowViewModel)DataContext).URL;
                var urlForLearning = url;
                if (BrowserSelectorCommon.Common.IsSafeLink(url, out string originalUrl))
                {
                    // Stripe
                    if (bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_STRIP_SAFELINKS) ?? "false"))
                    {
                        url = originalUrl;
                        ((SelectorWindowViewModel)DataContext).URL = url;
                        ((SelectorWindowViewModel)DataContext).UrlIsStripped = true;
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
                    if(!string.IsNullOrEmpty(learnProgid))
                    {
                        progid = learnProgid;
                    }
                }


                // Find Trackers
                var nurl = BrowserSelectorCommon.Common.RemoveTrackers(url, out List<string> result);
                if (bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_REMOVE_TRACKERS) ?? "false"))
                {
                    if (!string.IsNullOrEmpty(nurl) && result.Count > 0)
                    {
                        url = nurl;
                        ((SelectorWindowViewModel)DataContext).URL = url;
                        ((SelectorWindowViewModel)DataContext).UrlHasTrackersRemoved = true;
                    }
                }
                ((SelectorWindowViewModel)DataContext).UrlHasTrackersFound = result.Count > 0;
                ((SelectorWindowViewModel)DataContext).UrlTrackersFound = result.Count;
                var trackersInfo = string.Empty;
                result.ForEach(x => trackersInfo += $"{x}\r\n");
                ((SelectorWindowViewModel)DataContext).UrlTrackersInfo = trackersInfo;




                if (!string.IsNullOrEmpty(progid))
                {
                    int index = -1;
                    foreach(var x in ((SelectorWindowViewModel)DataContext).Browsers)
                    {
                        index++;
                        if(x.ProgId == progid)
                        {
                            lbBrowsers.SelectedIndex = index;
                            break;
                        }
                    }
                }
                else
                {
                    if (lbBrowsers.Items.Count > 0)
                    {
                        lbBrowsers.SelectedIndex = 0;
                    }
                }
            }
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
    }
}
