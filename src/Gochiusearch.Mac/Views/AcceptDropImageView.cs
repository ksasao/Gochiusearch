using System;
using AppKit;
using Foundation;
using CoreGraphics;
using System.Linq;
using MobileCoreServices;

namespace Gochiusearch.Mac
{
    [global::Foundation.Register("AcceptDropImageView")]
    public class AcceptDropImageView : NSImageView
    {
        public AcceptDropImageView(IntPtr handle)
            : base(handle)
        {
        }

        public AcceptDropImageView(CGRect r)
            : base(r)
        {
            WantsLayer = true;
        }

        public event EventHandler<DropEventArgs> FileDropped;

        public event EventHandler<DropEventArgs> ImageUrlDropped;

        public override void ViewDidMoveToSuperview()
        {
            RegisterForDraggedTypes(new string[] { NSPasteboard.NSFilenamesType, NSPasteboard.NSTiffType });
        }

        public override NSDragOperation DraggingEntered(NSDraggingInfo sender)
        {
            var item = sender.DraggingPasteboard.PasteboardItems.First();

            NSString url;
            if (item.Types.Any(x => x == "public.url"))
            {
                url = new NSString(item.GetStringForType("public.url"));
            }
            else if (item.Types.Any(x => x == "public.file-url"))
            {
                url = new NSString(new NSUrl(item.GetStringForType("public.file-url")).Path);
            }
            else {
                return NSDragOperation.None;
            }
            return CanConformsToImageUTI(url)
                             ? NSDragOperation.Copy
                                 : NSDragOperation.None;
        }

        public override bool PerformDragOperation(NSDraggingInfo sender)
        {
            var item = sender.DraggingPasteboard.PasteboardItems.First();
            if (item.Types.Any(x => x == "public.url"))
            {
                var url = new NSString(item.GetStringForType("public.url"));
                ImageUrlDropped?.Invoke(this, new DropEventArgs(url));
            }
            else if (item.Types.Any(x => x == "public.file-url"))
            {
                var url = new NSString(new NSUrl(item.GetStringForType("public.file-url")).Path);
                FileDropped?.Invoke(this, new DropEventArgs(url));
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool CanConformsToImageUTI(NSString url)
        {
            var uti = UTType.CreatePreferredIdentifier(UTType.TagClassFilenameExtension, url.PathExtension, null);
            return UTType.ConformsTo(uti, UTType.Image);
        }
    }

    public class DropEventArgs
    {
        public DropEventArgs(NSString url)
        {
            Payload = url;
        }

        public NSString Payload { get; }
    }
}

