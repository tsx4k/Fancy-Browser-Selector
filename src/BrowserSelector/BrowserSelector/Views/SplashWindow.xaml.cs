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
using BrowserSelector.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;
using Wpf.Ui.Controls.Window;

namespace BrowserSelector.Views
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        bool canClose = false, isStarted = false;
        SplashWindowViewModel model;

        public SplashWindow(TimeSpan duration)
        {
            DataContext = model = new SplashWindowViewModel(this, duration);
            InitializeComponent();
        }

        public void AllowClose(bool value)
        {
            canClose = value;
        }

        private void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !canClose;
        }

        private void UiWindow_Activated(object sender, EventArgs e)
        {
            if(!isStarted)
            {
                isStarted = true;
            }
        }

        internal async Task ShowAsync(TimeSpan duration)
        {
            Show();
            while(!isStarted)
            {
                await Task.Delay(100);
            }
            model.LogoVisibility = Visibility.Visible;
            await Task.Delay(duration);
            canClose = true;
            Close();
        }
    }
}
