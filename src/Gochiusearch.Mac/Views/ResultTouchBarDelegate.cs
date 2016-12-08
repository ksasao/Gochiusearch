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
            using (var src = NSImage.FromStream(System.IO.File.OpenRead(filePath)))
            {
                var imageSize = src.Size;
                var thumbnailSize = new CGSize(Math.Ceiling(BarHeight * imageSize.Width / imageSize.Height), BarHeight);

                var thumbnail = new NSImage(thumbnailSize);
                thumbnail.LockFocus();
                src.Draw(new CGRect(CGPoint.Empty, thumbnailSize), new CGRect(CGPoint.Empty, imageSize), NSCompositingOperation.SourceOver, 1f);
                thumbnail.UnlockFocus();
                return thumbnail;
            }
        }
    }
}
