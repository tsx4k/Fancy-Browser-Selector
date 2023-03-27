using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BrowserSelectorCommon.Services
{
    internal class SafeLinksService
    {
        internal static bool IsSafeLink(string url, out string originalUrl)
        {
            // https://statics.teams.cdn.office.net/evergreen-assets/safelinks/1/atp-safelinks.html?url=
            try
            {
                Uri uri = new Uri(url);
                if(uri.Host.ToLower().EndsWith(".office.net") && uri.AbsolutePath.Contains("/safelinks/") && uri.AbsolutePath.Contains("-safelinks.html"))
                {
                    var nurl = HttpUtility.ParseQueryString(uri.Query)?.Get("url");
                    if (!string.IsNullOrEmpty(nurl))
                    {
                        originalUrl = HttpUtility.UrlDecode(nurl);
                        return true;
                    }
                }
            } catch { }
            originalUrl = null;
            return false;
        }
    }
}
