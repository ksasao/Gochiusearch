using System;
using AppKit;
using Foundation;
using CoreGraphics;
using CoreAnimation;

namespace Gochiusearch.Mac
{
    public class ResultTouchBarDelegate : NSTouchBarDelegate
    {
        private readonly string result;

        private readonly NSUrl url;

        private readonly string filePath;

        public ResultTouchBarDelegate(string result, NSUrl url, string file)
        {
            this.result = result;
            this.url = url;
            this.filePath = file;
        }

        public override NSTouchBarItem MakeItem(NSTouchBar touchBar, string identifier)
        {
            var item = new NSCustomTouchBarItem(identifier);
            switch (Array.IndexOf(DefaultIdentifiers, identifier))
            {
                case 0:
                    item.View = new NSImageView { Image = GetImage(), ImageScaling = NSImageScale.ProportionallyDown };
                    break;
                case 1:
                    item.View = NSButton.CreateButton(NSImage.ImageNamed(NSImageName.TouchBarPlayTemplate), () => NSWorkspace.SharedWorkspace.OpenUrl(url));
                    break;
                case 2:
                    // trim title
                    var start = result.IndexOf('「');
                    var end = result.IndexOf('」');
                    var summary = result.Substring(0, start) + " " + result.Substring(end + 1);
                    item.View = NSTextField.CreateLabel(summary);
                    break;
                default:
                    break;
            }
            return item;
        }

        public static string[] DefaultIdentifiers = new[]
        {
            "jp.ailen0ada.gochiusearch.image",
            "jp.ailen0ada.gochiusearch.navigate",
            "jp.ailen0ada.gochiusearch.result",
        };

        const int BarHeight = 30;

        private NSImage GetImage()
        {
            var src = NSImage.FromStream(System.IO.File.OpenRead(filePath));
            var scaleFactor = BarHeight / src.Size.Height;

            var width = (nint)(src.Size.Width * scaleFactor);
            var height = (nint)(src.Size.Height * scaleFactor);
            var bpc = 8;
            var stride = 4 * width;
            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var info = CGImageAlphaInfo.PremultipliedLast;

            var context = new CGBitmapContext(null, width, height, bpc, stride, colorSpace, info);
            var rect = new CGRect(0f, 0f, width, height);
            context.DrawImage(rect, src.CGImage);
            var ni = context.ToImage();
            return new NSImage(ni, new CGSize(width, height));
        }
    }
}
