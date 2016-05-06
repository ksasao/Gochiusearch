using System;
using Mpga.ImageSearchEngine;
using AppKit;
using System.IO;
using CoreGraphics;
using System.Runtime.InteropServices;
using ModelIO;

namespace Gochiusearch.Mac
{
    internal class XMImageSearch : ImageSearch
    {
        protected override Argb32DataWithSize GetImageData(string targetFile)
        {
            using (var imgRef = NSImageRep.ImageRepFromFile(targetFile)?.CGImage)
            {
                if (imgRef == null)
                    throw new InvalidOperationException();

                using (var dataProvider = imgRef.DataProvider)
                using (var data = dataProvider.CopyData())
                {
                    var source = data.Bytes;
                    var bmp32Data = new byte[data.Length];
                    Marshal.Copy(source, bmp32Data, 0, bmp32Data.Length);
                    return new Argb32DataWithSize((int)imgRef.Width, (int)imgRef.Height, bmp32Data);
                }
            }
        }
    }
}

