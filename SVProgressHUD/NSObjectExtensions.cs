//
// Project: SVProgressHUD
// File: NSObjectExtensions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace SVProgressHUD
{
    static class NSObjectExtensions
    {
        
        public static void PerformSelector(this NSObject obj, Selector sel)
        {
            void_objc_msgSend(obj.Handle, sel.Handle);
        }

        #region P/Invoke

        const string LIBOBJC_DYLIB = "/usr/lib/libobjc.dylib";

        [DllImport(LIBOBJC_DYLIB, EntryPoint = "objc_msgSend")]
        internal extern static void void_objc_msgSend(IntPtr receiver, IntPtr selector);

        #endregion
    }
}
