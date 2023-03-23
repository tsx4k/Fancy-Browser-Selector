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

        public string URL { get; set; }

        public ICommand OpenBrowserCommand { get; private set; }
        public ICommand CopyToClipboardCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand EditCommentCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }

        public Window Window { get; set; }

        public SelectorWindowViewModel() 
        {
            OpenBrowserCommand = new RelayCommand(((SelectorWindow)Window).OpenBrowser);
            CopyToClipboardCommand = new RelayCommand(CopyToClipboard);
            CloseCommand = new RelayCommand(Close);
            EditCommentCommand = new RelayCommand(EditComment);
            SettingsCommand = new RelayCommand(OpenSettings);

            PrepareBrowsersList();
        }

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

        private void OpenSettings(object param)
        {
            BrowserSelectorCommon.Common.OpenSettings();
        }

        private void PrepareBrowsersList()
        {
            int index = ((SelectorWindow)Window).lbBrowsers.SelectedIndex;
            browsers = BrowserSelectorCommon.Common.GetBrowsers(browsers);
            ocbrowsers.Clear();
            browsers.ForEach(x => {
                ocbrowsers.Add(x);
            });
            if (index < ((SelectorWindow)Window).lbBrowsers.Items.Count)
            {
                ((SelectorWindow)Window).lbBrowsers.SelectedIndex = index;
            }
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
            button.IconForeground = new SolidColorBrush(Colors.Lime);
        }
    }
}
