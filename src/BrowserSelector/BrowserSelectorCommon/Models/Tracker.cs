using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserSelectorCommon.Models
{
    public class Tracker
    {
        public string Key { get; set; }
        public string Source { get; set; }

        public Tracker(string Key, string Source) 
        { 
            this.Key = Key;
            this.Source = Source;
        }
    }
}
