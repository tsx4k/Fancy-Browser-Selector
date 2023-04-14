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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BrowserSelectorCommon.Services
{
    internal class RememberChoiceService
    {
        public static bool SaveChoice(string appId, IBrowser browser, string url)
        {
            try
            {
                var appPath = Registry.CurrentUser.CreateSubKey(string.Format("SOFTWARE\\{0}\\Choices", appId));
                Uri uri = new Uri(url);
                appPath?.SetValue($"{uri.Host}", browser.ProgId);
                return true;
            }
            catch { }
            return false;
        }

        public static bool RemoveChoiceByUrl(string appId, string url)
        {
            try
            {
                var appPath = Registry.CurrentUser.CreateSubKey(string.Format("SOFTWARE\\{0}\\Choices", appId));
                Uri uri = new Uri(url);
                appPath?.DeleteValue($"{uri.Host}");
                return true;
            }
            catch { }
            return false;
        }

        public static bool RemoveChoice(string appId, string host)
        {
            try
            {
                var appPath = Registry.CurrentUser.CreateSubKey(string.Format("SOFTWARE\\{0}\\Choices", appId));
                appPath?.DeleteValue($"{host}");
                return true;
            }
            catch { }
            return false;
        }

        internal static string GetChoice(string appId, string url)
        {
            try
            {
                var appPath = Registry.CurrentUser.CreateSubKey(string.Format("SOFTWARE\\{0}\\Choices", appId));
                Uri uri = new Uri(url);
                return (string)appPath?.GetValue($"{uri.Host}", null);
            }
            catch { }
            return null;
        }

        internal static List<Tuple<string, string>> GetAllChoices(string appId)
        {
            try
            {
                List<Tuple<string, string>> result = new List<Tuple<string, string>>();
                var appPath = Registry.CurrentUser.CreateSubKey(string.Format("SOFTWARE\\{0}\\Choices", appId));
                foreach(var name in appPath?.GetValueNames())
                {
                    result.Add(new Tuple<string, string>(name, (string)appPath?.GetValue(name, null)));
                }
                return result;
            }
            catch { }
            return null;
        }

    }
}
