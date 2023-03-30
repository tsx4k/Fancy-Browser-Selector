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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserSelectorCommon.Constants
{
    public class Settings
    {
        public static readonly string SETTING_USE_SINGLE_CLICK = "UseSingleClick";
        public static readonly string SETTING_LEARN_HOSTS = "LearnHosts";
        public static readonly string SETTING_LEARN_HOSTS_SAFELINK = "LearnHostsSafeLink";
        public static readonly string SETTING_STRIP_SAFELINKS = "StripSafeLinks";
        public static readonly string SETTING_LOAD_BROWSER_PROFILES = "LoadBrowserProfiles";
        public static readonly string SETTING_REMOVE_TRACKERS = "RemoveTrackers";
        public static readonly string SETTING_VERSION = "Version";
        public static readonly string SETTING_APPPATH = "AppPath";
    }
}
