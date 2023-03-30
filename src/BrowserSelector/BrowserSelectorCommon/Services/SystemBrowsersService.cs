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
using BrowserSelectorCommon.Services.BrowserExtensions;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace BrowserSelectorCommon.Services
{
    internal class SystemBrowsersService
    {
        const string REG_PATH = "SOFTWARE\\Clients\\StartMenuInternet\\";
        const string REG_EDGE_PATH = "Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\PackageRepository\\Packages\\";

        public static List<IBrowser> GetBrowsers(List<IBrowser> browsersList = null)
        {
            if (browsersList == null)
            {
                browsersList = new List<IBrowser>();
            }

            // grab CurrentUser browsers
            try
            {
                var regKey = Registry.CurrentUser.OpenSubKey(REG_PATH);
                if (regKey != null) 
                {
                    regKey.GetSubKeyNames().ToList().ForEach(name => {
                            var URLassoc = Registry.CurrentUser.OpenSubKey(REG_PATH + name + "\\Capabilities\\URLAssociations")?.GetValue("http") as string;
                            var browser = new Browser() 
                            {
                                Name = Registry.CurrentUser.OpenSubKey(REG_PATH + name)?.GetValue(string.Empty) as string,
                                ProgId = name,
                                IconPath  = Registry.CurrentUser.OpenSubKey(REG_PATH + name + "\\DefaultIcon")?.GetValue(string.Empty) as string,
                                ExecutablePath = Registry.CurrentUser.OpenSubKey(REG_PATH + name + "\\shell\\open\\command")?.GetValue(string.Empty) as string,
                                RegistryPath = REG_PATH + name,
                            };
                            //browser.ExecutablePath = FixExecutablePath(URLassoc, browser.ExecutablePath);
                            List<IBrowser> browserProfiles = GrabProfiles(browser);
                            foreach (var browserProfile in browserProfiles)
                            {
                                if (!browsersList.Exists(x => x.Name == browserProfile.Name) && !browserProfile.RegistryPath.Contains(BrowserSelectorCommon.Common.AppId))
                                {
                                    browsersList.Add(browserProfile);
                                }
                            }
                    });
                }
            } catch (Exception ex)
            { 
                // TODO: log
            }

            // grab LocalMachine browsers
            try
            {
                var regKey = Registry.LocalMachine.OpenSubKey(REG_PATH);
                if (regKey != null)
                {
                    regKey.GetSubKeyNames().ToList().ForEach(name => {
                        var URLassoc = Registry.LocalMachine.OpenSubKey(REG_PATH + name + "\\Capabilities\\URLAssociations")?.GetValue("http") as string;
                        var browser = new Browser()
                        {
                            Name = Registry.LocalMachine.OpenSubKey(REG_PATH + name)?.GetValue(string.Empty) as string,
                            ProgId = name,
                            IconPath = Registry.LocalMachine.OpenSubKey(REG_PATH + name + "\\DefaultIcon")?.GetValue(string.Empty) as string,
                            ExecutablePath = Registry.LocalMachine.OpenSubKey(REG_PATH + name + "\\shell\\open\\command")?.GetValue(string.Empty) as string,
                            RegistryPath = REG_PATH + name,
                        };
                        //browser.ExecutablePath = FixExecutablePath(URLassoc, browser.ExecutablePath);
                        List<IBrowser> browserProfiles = GrabProfiles(browser);
                        foreach (var browserProfile in browserProfiles)
                        {
                            if (!browsersList.Exists(x => x.Name == browserProfile.Name) && !browserProfile.RegistryPath.Contains(BrowserSelectorCommon.Common.AppId))
                            {
                                browsersList.Add(browserProfile);
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // TODO: log
            }

            // Detect Microsoft Edge (sometimes it is installed but not listed in above)
            DetectMicrosoftEdge(ref browsersList);

            // Load browsers Icons and Comments
            browsersList.ForEach(x => {
                x.Icon = LoadIcon(x);
                x.Comment = LoadComment(x);
            });

            return browsersList;
        }

        private static List<IBrowser> GrabProfiles(IBrowser browser)
        {
            if (bool.Parse(BrowserSelectorCommon.Common.GetSetting(BrowserSelectorCommon.Constants.Settings.SETTING_LOAD_BROWSER_PROFILES) ?? "false"))
            {
                if (BraveService.IsBrave(browser))
                {
                    var profiles = BraveService.GetProfiles(browser);
                    if (profiles == null || profiles.Count == 0)
                    {
                        return new List<IBrowser>() { browser };
                    }
                    else
                    {
                        return profiles;
                    }
                }
                else
                if (ChromeService.IsChrome(browser))
                {
                    var profiles = ChromeService.GetProfiles(browser);
                    if (profiles == null || profiles.Count == 0)
                    {
                        return new List<IBrowser>() { browser };
                    }
                    else
                    {
                        return profiles;
                    }
                }
            }
            return new List<IBrowser>() { browser };
        }

        private static string FixExecutablePath(string appId, string executablePath)
        {
            // Firefoxes cannot handle an extra long URL links and they have special registry for command
            // HKEY_CLASSES_ROOT\FirefoxURL-6F193CCC56814779\shell\open\command => "C:\Program Files\Firefox Nightly\firefox.exe" -osint -url "%1"
            try
            {
                var appPath = Registry.ClassesRoot.OpenSubKey(string.Format("{0}\\shell\\open\\command", appId));
                var newpath = (string)appPath?.GetValue(string.Empty, string.Empty);
                if(!string.IsNullOrEmpty(newpath))
                {
                    return newpath;
                }
            }
            catch { }
            return executablePath;
        }

        private static string LoadComment(IBrowser x)
        {
            return BrowserSelectorCommon.Common.GetComment(x);
        }

        public static BitmapImage LoadIcon(IBrowser browser)
        {
            if (browser.Icon != null) return browser.Icon;
            if (!string.IsNullOrEmpty(browser.IconPath))
            {
                return FileIconService.GetIconImage(browser.IconPath);
            }
            return null;
        }

        internal static string GetLastSelectedBrowser(string appId)
        {
            try
            {
                var appPath = Registry.CurrentUser.CreateSubKey(string.Format("SOFTWARE\\{0}", appId));
                return (string)appPath?.GetValue("LastSelectedBrowser", string.Empty);
            }
            catch { }
            return null;
        }

        internal static bool SetLastSelectedBrowser(string appId, string progId)
        {
            try
            {
                var appPath = Registry.CurrentUser.CreateSubKey(string.Format("SOFTWARE\\{0}", appId));
                appPath?.SetValue("LastSelectedBrowser", progId);
                return true;
            }
            catch { }
            return false;
        }


        private static void DetectMicrosoftEdge(ref List<IBrowser> browsersList)
        {
            string EdgeVersion = string.Empty;
            RegistryKey reg = Registry.ClassesRoot.OpenSubKey(REG_EDGE_PATH);
            if (reg != null)
            {
                foreach (string subkey in reg.GetSubKeyNames())
                {
                    if (subkey.StartsWith("Microsoft.MicrosoftEdge_"))
                    {
                        var browser = new Browser()
                        {
                            Name = "Microsoft Edge",
                            ProgId = subkey,
                            ExecutablePath = Registry.ClassesRoot.OpenSubKey(REG_EDGE_PATH + subkey)?.GetValue("Path") as string,
                            RegistryPath = REG_EDGE_PATH,
                        };
                        browser.ExecutablePath = string.IsNullOrEmpty(browser.ExecutablePath) ? browser.ExecutablePath : browser.ExecutablePath + "\\MicrosoftEdge.exe";
                        browser.IconPath = string.IsNullOrEmpty(browser.ExecutablePath) ? browser.ExecutablePath : browser.ExecutablePath + ",0";
                        List<IBrowser> browserProfiles = GrabProfiles(browser);
                        foreach (var browserProfile in browserProfiles)
                        {
                            if (!browsersList.Exists(x => x.Name == browserProfile.Name) && !browserProfile.RegistryPath.Contains(BrowserSelectorCommon.Common.AppId))
                            {
                                browsersList.Add(browserProfile);
                            }
                        }
                    }
                }
            }
        }
    }
}
