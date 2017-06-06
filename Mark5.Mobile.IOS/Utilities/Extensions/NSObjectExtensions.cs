using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Mark5.Mobile.IOS.Utilities.Extensions
{
    public static class NSObjectExtensions
    {
        public static int PerformSelectorCustom(this NSObject obj, Selector sel, NSObject arg1, int arg2)
        {
            return int_objc_msgSend_IntPtr_int(obj.Handle, sel.Handle, arg1.Handle, arg2);
        }

        #region P/Invoke

        const string LIBOBJC_DYLIB = "/usr/lib/libobjc.dylib";

        [DllImport(LIBOBJC_DYLIB, EntryPoint = "objc_msgSend")]
        internal static extern int int_objc_msgSend_IntPtr_int(IntPtr receiver, IntPtr selector, IntPtr arg1, int arg2);

        #endregion
    }
}