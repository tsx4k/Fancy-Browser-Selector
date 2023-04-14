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
using BrowserSelector.Properties;
using BrowserSelector.Relay;
using BrowserSelector.Views;
using BrowserSelectorCommon.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BrowserSelector.ViewModels
{
    internal class SelectorWindowViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private ObservableCollection<IBrowser> ocbrowsers = new ObservableCollection<IBrowser>();
        private List<IBrowser> browsers = BrowserSelectorCommon.Common.GetBrowsers();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<IBrowser> Browsers { get { return ocbrowsers; } set { ocbrowsers = value; OnPropertyChanged("Browsers"); } }

        public string Title => $"{BrowserSelectorCommon.Common.AppName}";

        private string _url = null;
        public string URL { get { return _url; } set { _url = value; OnPropertyChanged("URL"); } }

        private string _originalUrl = null;
        public string OriginalURL { get { return _originalUrl; } set { _originalUrl = value; OnPropertyChanged("OriginalURL"); } }

        private bool _urlIsStripped = false;
        public bool UrlIsStripped { get { return _urlIsStripped; } set { _urlIsStripped = value; OnPropertyChanged("UrlIsStripped"); } }

        public ICommand OpenBrowserCommand { get; private set; }
        public ICommand CopyToClipboardCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand EditCommentCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }

        public Window Window { get; set; }

        private bool _urlHasTrackersRemoved = false;
        public bool UrlHasTrackersRemoved { get { return _urlHasTrackersRemoved; } set { _urlHasTrackersRemoved = value; OnPropertyChanged("UrlHasTrackersRemoved"); } }

        private bool _urlHasTrackersFound = false;
        public bool UrlHasTrackersFound { get { return _urlHasTrackersFound; } set { _urlHasTrackersFound = value; OnPropertyChanged("UrlHasTrackersFound"); } }

        private int _urlTrackersFound = 0;
        public int UrlTrackersFound { get { return _urlTrackersFound; } set { _urlTrackersFound = value; OnPropertyChanged("UrlTrackersFound"); } }

        private string _urlTrackersInfo = null;
        public string UrlTrackersInfo { get { return _urlTrackersInfo; } set { _urlTrackersInfo = value; OnPropertyChanged("UrlTrackersInfo"); } }

        private bool _canRememberChoice = false;
        public bool CanRememberChoice { get { return _canRememberChoice; } set { _canRememberChoice = value; OnPropertyChanged("CanRememberChoice"); } }

        private bool _rememberChoice = false, _choiceInfoShown = bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_CHOICE_INFO_SHOWN) ?? "false");
        public bool RememberChoice { get { return _rememberChoice; } set { _rememberChoice = ShowChoiceInfo(value); OnPropertyChanged("RememberChoice"); } }

        public string ChoiceInfo => "FBS will remember your choice for this host and next time it won't show this window until you hold a SHIFT key or manage this bahaviour in Settings.";

        public SelectorWindowViewModel(Window window)
        {
            this.Window = window;
            OpenBrowserCommand = new RelayCommand(((SelectorWindow)Window).OpenBrowser);
            CopyToClipboardCommand = new RelayCommand(CopyToClipboard);
            CloseCommand = new RelayCommand(Close);
            EditCommentCommand = new RelayCommand(EditComment);
            SettingsCommand = new RelayCommand(OpenSettings);

            PrepareBrowsersList();
        }

        private bool ShowChoiceInfo(bool value)
        {
            if (!value) return false;
            else if (_choiceInfoShown) return value;
            _choiceInfoShown = true;
            BrowserSelectorCommon.Common.SetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_CHOICE_INFO_SHOWN, _choiceInfoShown.ToString());
            return MessageBox.Show(Window, ChoiceInfo, BrowserSelectorCommon.Common.AppName, MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK) == MessageBoxResult.OK;
        }

        public void SetRememberChoiceSilently(bool value)
        {
            _rememberChoice = value;
        }

        public void ApplyTheme()
        {
            // apply theme from settings (if any)
            string theme = BrowserSelectorCommon.Common.GetTheme();
            if (!string.IsNullOrEmpty(theme))
            {
                if (Enum.TryParse(theme, out Wpf.Ui.Appearance.ThemeType appTheme))
                {
                    Wpf.Ui.Appearance.Theme.Apply(appTheme);
                }
            }
        }

        private void OpenSettings(object param)
        {
            BrowserSelectorCommon.Common.OpenSettings();
        }

        bool browsersArePreparing = false;
        public void PrepareBrowsersList(bool force = false)
        {
            if (browsersArePreparing) return;
            browsersArePreparing = true;
            int index = ((SelectorWindow)Window).lbBrowsers.SelectedIndex;
            browsers = BrowserSelectorCommon.Common.GetBrowsers(force ? null : browsers);
            ocbrowsers.Clear();
            browsers.ForEach(x => {
                ocbrowsers.Add(x);
            });
            if (index < ((SelectorWindow)Window).lbBrowsers.Items.Count)
            {
                ((SelectorWindow)Window).lbBrowsers.SelectedIndex = index;
            }
            browsersArePreparing = false;
        }

        private void Close(object param)
        {
            Window.Close();
        }

        private void EditComment(object param)
        {
            IBrowser browser = param as IBrowser;
            if(browser != null)
            {
                ((SelectorWindow)Window).lbBrowsers.SelectedItem = browser;
                var editWindow = new EditCommentWindow(browser);
                if(editWindow.ShowDialog() == true)
                {
                    string newComment = editWindow.Comment;

                    BrowserSelectorCommon.Common.SetComment(browser, newComment);

                    PrepareBrowsersList();
                }
            }
        }


        private void CopyToClipboard(object param)
        {
            Clipboard.SetText(URL);
            var button = (Wpf.Ui.Controls.Button)param;
            button.Appearance = Wpf.Ui.Controls.ControlAppearance.Success;
            button.Icon = Wpf.Ui.Common.SymbolRegular.Checkmark32;
            button.Background = new SolidColorBrush(Colors.Green);
        }
    }
}
