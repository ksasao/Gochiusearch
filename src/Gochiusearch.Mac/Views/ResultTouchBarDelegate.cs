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
                    if (url == null)
                    {
                        item.View = new NSImageView { Image = NSImage.ImageNamed(NSImageName.Caution) };
                    }
                    else
                    {
                        item.View = NSButton.CreateButton(NSImage.ImageNamed(NSImageName.TouchBarPlayTemplate), () => NSWorkspace.SharedWorkspace.OpenUrl(url));
                    }
                    break;
                case 2:
                    var view = new NSScrollView
                    {
                        HasVerticalScroller = false,
                        HasHorizontalScroller = true,
                        BorderType = NSBorderType.NoBorder
                    };
                    item.View = view;

                    // trim title
                    var label = NSTextField.CreateLabel(result);
                    var size = label.AttributedStringValue.Size;
                    label.SetBoundsOrigin(new CGPoint(0, (BarHeight - size.Height) / 2));
                    label.SetFrameSize(new CGSize(size.Width + 8, BarHeight));
                    view.DocumentView = label;
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
