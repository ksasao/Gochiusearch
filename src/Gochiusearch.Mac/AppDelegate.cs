using AppKit;
using Foundation;

namespace Gochiusearch.Mac
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        MainWindowController mainWindowController;

        NSString containerDirectory;

        public AppDelegate()
        {
            containerDirectory = NativeMethods.ContainerDirectory.AppendPathComponent(new NSString(".browser"));
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            System.IO.Directory.CreateDirectory(containerDirectory);
            mainWindowController = new MainWindowController(containerDirectory);
            mainWindowController.Window.MakeKeyAndOrderFront(this);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
            // Clear caches
            System.IO.Directory.Delete(containerDirectory, true);
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

        partial void NavigateToGithub(NSObject sender)
        {
            NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl("https://github.com/ksasao/Gochiusearch"));
        }
    }
}
