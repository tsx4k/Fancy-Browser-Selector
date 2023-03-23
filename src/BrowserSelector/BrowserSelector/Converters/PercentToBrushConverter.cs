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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace BrowserSelector
{
    internal class PercentToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var percent = (int)value;
            var param = (string)parameter;

            if (percent < 1) percent = 1;
            if (percent > 100) percent = 100;
            percent = 100 - percent;


            string[] vals = param.Split(',');

            var colorFrom = (Color)ColorConverter.ConvertFromString(vals[1]);
            var colorTo = (Color)ColorConverter.ConvertFromString(vals[0]);
            double newA = Math.Abs(colorTo.A - ((double)(colorTo.A - colorFrom.A) * ((double)percent / 100)));
            double newR = Math.Abs(colorTo.R - ((double)(colorTo.R - colorFrom.R) * ((double)percent/100)));
            double newG = Math.Abs(colorTo.G - ((double)(colorTo.G - colorFrom.G) * ((double)percent / 100)));
            double newB = Math.Abs(colorTo.B - ((double)(colorTo.B - colorFrom.B) * ((double)percent / 100)));

            return new SolidColorBrush(Color.FromArgb((byte)newA, (byte)newR, (byte)newG, (byte)newB));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
