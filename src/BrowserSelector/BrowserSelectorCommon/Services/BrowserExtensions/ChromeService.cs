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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrowserSelectorCommon.Services.BrowserExtensions
{
    internal class ChromeService
    {
        const string ProfileConfig = "Local State";
        const string ProfileIcon = "Google Profile.ico";

        public static bool IsChrome(IBrowser browser)
        {
            var isChrome = browser?.ProgId?.ToLower().StartsWith("google chrome");
            return isChrome ?? false;
        }

        public static List<IBrowser> GetProfiles(IBrowser mainBrowser)
        {
            var profiles = new List<IBrowser>();
            string appDataPath = System.IO.Path.Combine(BrowserSelectorCommon.Common.GetLocalAppDataPath(), "Google\\Chrome\\User Data\\");
            var configFile = System.IO.Path.Combine(appDataPath, ProfileConfig);
            if(System.IO.File.Exists(configFile))
            {
                try {
                    string json = null;
                    using(var file = new FileStream(configFile, FileMode.Open, FileAccess.Read))
                    {
                        var reader = new StreamReader(file);
                        json = reader.ReadToEnd();
                        file.Close();
                    }
                    dynamic data = JObject.Parse(json);
                    if(data != null)
                    {
                        foreach(var profile in (JObject)data.profile["info_cache"])
                        {
                            var profileId = profile.Key;
                            var profileName = (string)profile.Value["name"];
                            string profilePath = System.IO.Path.Combine(appDataPath, profileId);
                            string profileIconPath = System.IO.Path.Combine(profilePath, ProfileIcon);
                            if (File.Exists(profileIconPath))
                            {
                                var browser = new Browser()
                                {
                                    Name = $"{mainBrowser.Name} - {profileName}",
                                    ExecutablePath = $"{mainBrowser.ExecutablePath}",
                                    ExecutableArgs = $"--profile-directory=\"{profileId}\"",
                                    IconPath = profileIconPath,
                                    ProgId = $"{mainBrowser.ProgId}-{profileId}",
                                    RegistryPath = mainBrowser.RegistryPath
                                };
                                profiles.Add(browser);
                            }
                        }
                    }
                }
                catch(Exception ex) { 
                    // TODO: log
                }
            }
            return profiles;
        }
    }
}
