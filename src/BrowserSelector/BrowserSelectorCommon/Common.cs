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
using BrowserSelectorCommon.Interfaces;
using BrowserSelectorCommon.Models;
using BrowserSelectorCommon.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BrowserSelectorCommon
{
    public class Common
    {
        public const string AppName = "Fancy Browser Selector";
        public const string AppDesc = "Fancy Browser Selector is a lightweight UI tool for selecting browser before navigating to any web address.";
        public const string AppId = "FancyBrowserSelector";
        private static readonly string AppPath = System.Reflection.Assembly.GetEntryAssembly().Location;
        private static readonly string AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

        public static void RegisterAsBrowser()
        {
#if !DEBUG
            // handle new versions in different path
            var apppath = (string)GetSetting(Constants.Settings.SETTING_APPPATH);
            var version = (string)GetSetting(Constants.Settings.SETTING_VERSION);
            if (string.IsNullOrEmpty(version) || (version != AppVersion && apppath != AppPath) || apppath != AppPath)
            {
                SystemRegisteringService.UnregisterBrowser(AppId);
            }
#endif
            SystemRegisteringService.RegisterBrowser(AppId, AppName, $"{AppPath},0", AppDesc, AppPath);
            SystemRegisteringService.SetAsDefault(AppId);
            SetSetting(Constants.Settings.SETTING_VERSION, AppVersion);
            SetSetting(Constants.Settings.SETTING_APPPATH, AppPath);
        }

        public static void UnRegisterAsBrowser()
        {
            SystemRegisteringService.UnregisterBrowser(AppId);
        }

        public static List<IBrowser> GetBrowsers()
        {
            return SystemBrowsersService.GetBrowsers();
        }
        public static List<IBrowser> GetBrowsers(List<IBrowser> browsers)
        {
            return SystemBrowsersService.GetBrowsers(browsers);
        }

        public static void OpenBrowser(IBrowser browser, string url)
        {
            if(bool.Parse(GetSetting(Constants.Settings.SETTING_LEARN_HOSTS) ?? "false"))
            {
                if(bool.Parse(GetSetting(Constants.Settings.SETTING_LEARN_HOSTS_SAFELINK) ?? "false") && IsSafeLink(url, out string originalUrl))
                {
                    LearnService.LearnHost(AppId, browser, originalUrl);
                }
                else
                {
                    LearnService.LearnHost(AppId, browser, url);
                }
            }
            OpenBrowserService.Open(browser, url);
        }

        public static void OpenUrl(string url)
        {
            ProcessService.StartProcess(url, string.Empty);
        }

        public static void OpenSystemSettings()
        {
            ProcessService.StartProcess("ms-settings:defaultapps", string.Empty);
        }
        public static void OpenSettings()
        {
            ProcessService.StartProcess(System.Reflection.Assembly.GetEntryAssembly().Location, string.Empty);
        }

        public static bool IsRegistered()
        {
            return SystemRegisteringService.IsRegistered(AppId);
        }

        public static bool IsDefaultBrowser()
        {
            return SystemRegisteringService.IsDefaultBrowser(AppId);
        }

        public static bool SetComment(IBrowser browser, string newComment)
        {
            return CommentsService.SetComment(browser, AppId, newComment);
        }

        public static string GetComment(IBrowser browser)
        {
            return CommentsService.GetComment(browser, AppId);
        }

        public static string GetLastSelectedBrowser()
        {
            return SystemBrowsersService.GetLastSelectedBrowser(AppId);
        }

        public static bool SetLastSelectedBrowser(string progId)
        {
            return SystemBrowsersService.SetLastSelectedBrowser(AppId, progId);
        }

        public static bool IsSafeLink(string url, out string originalUrl)
        {
            return SafeLinksService.IsSafeLink(url, out originalUrl);
        }

        public static bool SetTheme(string newTheme)
        {
            return SettingsService.SetTheme(AppId, newTheme);
        }

        public static string GetTheme()
        {
            return SettingsService.GetTheme(AppId);
        }

        public static string GetSetting(string settingId)
        {
            return SettingsService.GetSetting(AppId, settingId);
        }

        public static bool SetSetting(string settingId, string newValue)
        {
            return SettingsService.SetSetting(AppId, settingId, newValue);
        }

        public static string GetLearning(string url)
        {
            return LearnService.MatchHost(AppId, url);
        }

    }
}
