using System;
using Foundation;

namespace Gochiusearch.Mac
{
    public static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        public static extern IntPtr NSHomeDirectory();

        public static NSString ContainerDirectory
        {
            get
            {
                return (NSString)ObjCRuntime.Runtime.GetNSObject(NSHomeDirectory());
            }
        }
    }
}

