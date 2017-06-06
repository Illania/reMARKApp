using System;
using System.Runtime.InteropServices;
using System.Security;
using Foundation;
using ObjCRuntime;

namespace SVProgressHUD
{
    static class NativeMethods
    {
        [SecuritySafeCritical]
        public static void PerformSelector(this NSObject obj, Selector sel)
        {
            void_objc_msgSend(obj.Handle, sel.Handle);
        }

        #region P/Invoke

        const string LibobjcDylib = "/usr/lib/libobjc.dylib";

        [SecurityCritical]
        [DllImport(LibobjcDylib, EntryPoint = "objc_msgSend")]
        static extern void void_objc_msgSend(IntPtr receiver, IntPtr selector);

        #endregion
    }
}