using System;
using AppKit;
using Foundation;
using System.Diagnostics;
using CoreGraphics;
using System.Linq;
using CoreAnimation;
using System.Security.Policy;

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
            RegisterForDraggedTypes(new string[]{ NSPasteboard.NSFilenamesType, NSPasteboard.NSTiffType });
        }

        public override NSDragOperation DraggingEntered(NSDraggingInfo sender)
        {
            return NSDragOperation.Copy;
        }

        public override void DraggingEnded(NSDraggingInfo sender)
        {
            var item = sender.DraggingPasteboard.PasteboardItems.First();
            if (item.Types.Any(x => x == "public.url"))
            {
                var url = new NSUrl(item.GetStringForType("public.url"));
                ImageUrlDropped?.Invoke(this, new DropEventArgs(url));
            }
            else if (item.Types.Any(x => x == "public.file-url"))
            {
                var url = new NSUrl(item.GetStringForType("public.file-url"));
                FileDropped?.Invoke(this, new DropEventArgs(url));
            }
        }
    }

    public class DropEventArgs
    {
        public DropEventArgs(NSUrl url)
        {
            Payload = url;
        }

        public NSUrl Payload{ get; }
    }
}

