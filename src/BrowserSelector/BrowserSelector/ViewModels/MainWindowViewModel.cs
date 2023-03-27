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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Appearance;

namespace BrowserSelector.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel, INotifyPropertyChanged
    {
        DispatcherTimer timer = new DispatcherTimer();

        public string Title => $"{BrowserSelectorCommon.Common.AppName} Settings";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ICommand RegisterAsBrowserCommand { get; private set; }
        public ICommand UnRegisterAsBrowserCommand { get; private set; }
        public ICommand OpenSettingsCommand { get; private set; }

        private int percent = 0;
        public int Percent { get { return percent; } set { if (percent != value) { percent = value; OnPropertyChanged("Percent"); } } }

        private bool isRegistered = false;
        public bool IsRegistered { get { return isRegistered; } set { if (isRegistered != value) { isRegistered = value; OnPropertyChanged("IsRegistered"); } } }
        
        private bool isDefault = false;
        public bool IsDefault { get { return isDefault; } set { if (isDefault != value) { isDefault = value; OnPropertyChanged("IsDefault"); } } }

        private bool isSetupCompleted = false;
        public bool IsSetupCompleted { get { return isSetupCompleted; } set { if (isSetupCompleted != value) { isSetupCompleted = value; OnPropertyChanged("IsSetupCompleted"); } } }

        private bool isAutoRegisterEnabled = true;

        public List<Wpf.Ui.Appearance.ThemeType> Themes => new List<Wpf.Ui.Appearance.ThemeType>() { Wpf.Ui.Appearance.ThemeType.Light, Wpf.Ui.Appearance.ThemeType.Dark };

        private Wpf.Ui.Appearance.ThemeType currentTheme;
        public Wpf.Ui.Appearance.ThemeType CurrentTheme { get { return currentTheme; } set { currentTheme = value; OnPropertyChanged("CurrentTheme"); ApplyTheme(value); } }


        private bool settingUseSingleClick = bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_USE_SINGLE_CLICK) ?? "false");
        public bool SettingUseSingleClick { get { return settingUseSingleClick; } set { settingUseSingleClick = value; SetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_USE_SINGLE_CLICK, value.ToString()); OnPropertyChanged("SettingUseSingleClick"); } }

        private bool settingLearnHosts = bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_LEARN_HOSTS) ?? "false");
        public bool SettingLearnHosts { get { return settingLearnHosts; } set { settingLearnHosts = value; SetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_LEARN_HOSTS, value.ToString()); OnPropertyChanged("SettingLearnHosts"); } }

        private bool settingLearnHostsSafeLink = bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_LEARN_HOSTS_SAFELINK) ?? "false");
        public bool SettingLearnHostsSafeLink { get { return settingLearnHostsSafeLink; } set { settingLearnHostsSafeLink = value; SetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_LEARN_HOSTS_SAFELINK, value.ToString()); OnPropertyChanged("SettingLearnHostsSafeLink"); } }

        private bool settingStripSafeLinks = bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_STRIP_SAFELINKS) ?? "false");
        public bool SettingStripSafeLinks { get { return settingStripSafeLinks; } set { settingStripSafeLinks = value; SetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_STRIP_SAFELINKS, value.ToString()); OnPropertyChanged("SettingStripSafeLinks"); } }


        private void SetSetting(string key, string value)
        {
            BrowserSelectorCommon.Common.SetSetting(key, value);
        }

        private void ApplyTheme(Wpf.Ui.Appearance.ThemeType newTheme)
        {
            Wpf.Ui.Appearance.Theme.Apply(newTheme);
            BrowserSelectorCommon.Common.SetTheme(newTheme.ToString());
        }

        public void ApplyTheme()
        {
            // apply theme from settings (if any)
            string theme = BrowserSelectorCommon.Common.GetTheme();
            if (!string.IsNullOrEmpty(theme))
            {
                if (Enum.TryParse(theme, out Wpf.Ui.Appearance.ThemeType appTheme))
                {
                    CurrentTheme = appTheme;
                }
            }
        }


        public MainWindowViewModel() 
        {
            RegisterAsBrowserCommand = new RelayCommand(RegisterAsBrowser);
            UnRegisterAsBrowserCommand = new RelayCommand(UnRegisterAsBrowser);
            OpenSettingsCommand = new RelayCommand(OpenSettings);

            Initialize();

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Initialize()
        {
            // autoregister
            if (isAutoRegisterEnabled)
            {
                RegisterAsBrowser(null);
            }

            // grab initial status
            Timer_Tick(null, null);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int perc = 0;

            var isReg = BrowserSelectorCommon.Common.IsRegistered();
            var isDef = BrowserSelectorCommon.Common.IsDefaultBrowser();
            var isDone = isReg && isDef;

            IsRegistered = isReg;
            if (isReg)
            {
                perc = 50;
            }

            IsDefault = isDef;
            if (isDef)
            {
                perc = 100;
            }

            IsSetupCompleted = isDone;
            if (isDone)
            {
                perc = 100;
            }

            Percent = perc;
        }

        private void OpenSettings(object obj)
        {
            BrowserSelectorCommon.Common.OpenSystemSettings();
        }

        private void RegisterAsBrowser(object obj)
        {
            BrowserSelectorCommon.Common.RegisterAsBrowser();
        }

        private void UnRegisterAsBrowser(object obj)
        {
            BrowserSelectorCommon.Common.UnRegisterAsBrowser();
        }
    }
}
