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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Resources;

namespace BrowserSelectorCommon.Services
{
    internal class TrackersService
    {
        internal static string RemoveTrackers(string url, out List<string> result)
        {
            result = new List<string>();
            var knowTrackers = GetKnownTrackers();

            try
            {
                Uri uri = new Uri(url);
                var newQuery = string.Empty;
                var query = HttpUtility.ParseQueryString(uri.Query);
                bool fine = true;
                foreach(var key in query.AllKeys)
                {
                    try
                    {
                        var tracker = knowTrackers.Find(x => x.Item1 == key);
                        if(tracker != null)
                        {
                            result.Add($"{tracker.Item2} ({tracker.Item1})");
                        } else
                        {
                            newQuery += $"&{key}={query.Get(key)}";
                        }
                    }
                    catch { fine = false; }
                }
                if(fine)
                {
                    return new Uri($"{uri.Scheme}://{uri.Host}{((uri.Port == 80 && uri.Scheme.ToLower() == "http") || (uri.Port == 443 && uri.Scheme.ToLower() == "https") ? "" : ":" + uri.Port.ToString())}{uri.AbsolutePath}{(newQuery.Length > 0 ? "?" + newQuery.Substring(1) : "")}").ToString();
                }
            }
            catch { }
            return null;
        }

        private static List<Tuple<string, string>> GetKnownTrackers()
        {
            List<Tuple<string, string>> knownTrackers = new List<Tuple<string, string>>();

            /*
             * source: https://raw.githubusercontent.com/mpchadwick/tracking-query-params-registry/master/_data/params.csv
name,platform,confirmed_in,unique_per_visitor
fbclid,Facebook,,
gclid, Google AdWords / Google Analytics,https://support.google.com/searchads/answer/6292795,
gclsrc, Google DoubleClick,https://support.google.com/searchads/answer/6292795,
utm_content, Google Analytics,,
utm_term, Google Analytics,,
utm_campaign, Google Analytics,,
utm_medium, Google Analytics,,
utm_source, Google Analytics,,
utm_id, Google Analytics, https://support.google.com/urchin/answer/2633697,
_ga, Google Analytics,,
            ... etc.
            */

            //var auxList = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            // "BrowserSelectorCommon.Services.TrackersRepository.trackers20230330.csv"
            try
            {
                using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("BrowserSelectorCommon.Services.TrackersRepository.trackers.csv"))
                {
                    TextReader reader = new StreamReader(s);
                    string line = null;
                    int index = -1;
                    do
                    {
                        line = reader.ReadLine();
                        index++;
                        if (string.IsNullOrEmpty(line) || index == 0) { continue; }

                        string[] trackerInfo = line.Split(',');
                        knownTrackers.Add(new Tuple<string, string>(trackerInfo[0].Trim(), trackerInfo[1].Trim()));
                    } while (line != null);
                }
            } catch { }
            return knownTrackers;
        }
    }
}
