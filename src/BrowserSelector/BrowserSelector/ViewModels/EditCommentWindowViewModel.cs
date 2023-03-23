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
using BrowserSelector.Relay;
using BrowserSelector.Views;
using BrowserSelectorCommon.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BrowserSelector.ViewModels
{
    internal class EditCommentWindowViewModel : BaseViewModel, INotifyPropertyChanged
    {
        IBrowser browser;

        Window window;

        public string Title => $"{browser.Name} comment";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ICommand ButtonCommand { get; private set; }

        public IBrowser Browser { get { return browser; } set { browser = value; OnPropertyChanged("Browser"); } }

        public EditCommentWindowViewModel(IBrowser browser, Window window)
        {
            this.window = window;
            this.browser = browser;
            ButtonCommand = new RelayCommand(ButtonHandler);
        }

        private void ButtonHandler(object param)
        {
            var button = param as Button;
            if (button != null)
            {
                switch((string)button.Tag)
                {
                    case "OK":
                        window.DialogResult = true;
                        break;
                    default:
                        window.DialogResult = false;
                        break;
                }
            }
            window.Close();
        }
    }
}
