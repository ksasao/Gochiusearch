// WARNING
//
// This file has been generated automatically by Xamarin Studio Community to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Gochiusearch.Mac
{
    [Register("MainWindowController")]
    partial class MainWindowController
    {
        [Outlet]
        AppKit.NSTextView LogField { get; set; }

        [Outlet]
        AppKit.NSPopUpButton SearchLevelSelector { get; set; }

        [Outlet]
        AcceptDropImageView TargetImageView { get; set; }

        [Action("OnOpenNiconicoChanged:")]
        partial void OnOpenNiconicoChanged(Foundation.NSObject sender);

        void ReleaseDesignerOutlets()
        {
            if (SearchLevelSelector != null)
            {
                SearchLevelSelector.Dispose();
                SearchLevelSelector = null;
            }

            if (TargetImageView != null)
            {
                TargetImageView.Dispose();
                TargetImageView = null;
            }

            if (LogField != null)
            {
                LogField.Dispose();
                LogField = null;
            }
        }
    }
}
