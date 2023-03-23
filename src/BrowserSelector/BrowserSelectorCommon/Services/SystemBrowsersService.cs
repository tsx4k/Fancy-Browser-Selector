﻿/*
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
                            var browser = new Browser() 
                            {
                                Name = Registry.CurrentUser.OpenSubKey(REG_PATH + name)?.GetValue(string.Empty) as string,
                                ProgId = name,
                                IconPath  = Registry.CurrentUser.OpenSubKey(REG_PATH + name + "\\DefaultIcon")?.GetValue(string.Empty) as string,
                                ExecutablePath = Registry.CurrentUser.OpenSubKey(REG_PATH + name + "\\shell\\open\\command")?.GetValue(string.Empty) as string,
                                RegistryPath = REG_PATH + name,
                            };
                            if (!browsersList.Exists(x => x.Name == browser.Name) && !browser.RegistryPath.Contains(BrowserSelectorCommon.Common.AppId))
                            {
                                browsersList.Add(browser);
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
                        var browser = new Browser()
                        {
                            Name = Registry.LocalMachine.OpenSubKey(REG_PATH + name)?.GetValue(string.Empty) as string,
                            ProgId = name,
                            IconPath = Registry.LocalMachine.OpenSubKey(REG_PATH + name + "\\DefaultIcon")?.GetValue(string.Empty) as string,
                            ExecutablePath = Registry.LocalMachine.OpenSubKey(REG_PATH + name + "\\shell\\open\\command")?.GetValue(string.Empty) as string,
                            RegistryPath = REG_PATH + name,
                        };
                        if (!browsersList.Exists(x => x.Name == browser.Name))
                        {
                            browsersList.Add(browser);
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
                        if (!browsersList.Exists(x => x.Name == browser.Name))
                        {
                            browsersList.Add(browser);
                        }
                    }
                }
            }
        }
    }
}