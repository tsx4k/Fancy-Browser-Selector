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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BrowserSelectorCommon.Services
{
    internal class FileIconService
    {
        [DllImport("shell32.dll")]
        static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        internal static Bitmap GetExecutableIcon(string executablePathWithIconIndex, int defaultIndex = 0)
        {
            Bitmap bitmap = null;
            try
            {
                string pathExe = executablePathWithIconIndex;
                int index = defaultIndex;
                string[] pathIcon = executablePathWithIconIndex.Split(',');
                if (pathIcon.Length == 2)
                {
                    pathExe = pathIcon[0];
                    if (!int.TryParse(pathIcon[1], out index))
                    {
                        pathExe = executablePathWithIconIndex.Substring(0, executablePathWithIconIndex.LastIndexOf(','));
                        index = defaultIndex;
                    }
                }
                IntPtr hIcon = ExtractIcon(IntPtr.Zero, pathExe, index);
                Icon icon = Icon.FromHandle(hIcon);
                bitmap = icon?.ToBitmap();
                icon.Dispose();
            } catch (Exception ex)
            {
                // TODO: log
            }
            return bitmap;
        }

        internal static BitmapImage GetIconImage(string path)
        {
            var bitmap = GetExecutableIcon(path);
            if (bitmap != null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    memory.Seek(0, SeekOrigin.Begin);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
            }
            return null;
        }
    }
}
