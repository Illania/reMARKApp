//
// Project: Mark5.Mobile.IOS
// File: Integration.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using PCLStorage;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{

    public static class Integration
    {

        #region iPhone/iPad recognition

        const float IPhonePlusMaxBounds = 736.0f;

        public static bool IsIPhone()
        {
            return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone;
        }

        public static bool IsIPhonePlus()
        {
            return IsIPhone() && Math.Abs(Math.Max(UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height) - IPhonePlusMaxBounds) < 0.01f;
        }

        public static bool IsIPad()
        {
            return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;
        }

        #endregion

        #region Screen size

        public static readonly CGSize IPhone4SScreenSize = new CGSize(640.0f, 960.0f);
        public static readonly CGSize IPhone5ScreenSize = new CGSize(640.0f, 1136.0f);
        public static readonly CGSize IPhone6ScreenSize = new CGSize(750.0f, 1334.0f);
        public static readonly CGSize IPhone6ZoomScreenSize = new CGSize(640.0f, 1136.0f);
        public static readonly CGSize IPhone6PlusScreenSize = new CGSize(1242.0f, 2208.0f);
        public static readonly CGSize IPhone6PlusZoomScreenSize = new CGSize(1125.0f, 2001.0f);
        public static readonly CGSize IPadScreenSize = new CGSize(768.0f, 1024.0f);
        public static readonly CGSize IPadRetinaScreenSize = new CGSize(1536.0f, 2048.0f);
        public static readonly CGSize IPadProScreenSize = new CGSize(2048.0f, 2732.0f);

        public static CGSize GetScreenSizeInPixels()
        {
            var bounds = UIScreen.MainScreen.Bounds;
            var scale = UIScreen.MainScreen.Scale;

            return new CGSize(bounds.Width * scale, bounds.Height * scale);
        }

        #endregion

        #region Disk

        public static long GetFreeDiskSpace()
        {
            var paths = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, true);
            var dict = NSFileManager.DefaultManager.GetFileSystemAttributes(paths.Last());
            return ((NSNumber)dict.FreeSize).LongValue;
        }

        public static long GetTotalDiskSpace()
        {
            var paths = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, true);
            var dict = NSFileManager.DefaultManager.GetFileSystemAttributes(paths.Last());
            return ((NSNumber)dict.Size).LongValue;
        }

        public static void ClearData()
        {
            NSError _error;

            var localStorage = FileSystem.Current.LocalStorage;
            var dataFolder = PortablePath.Combine(localStorage.Path, "v2");
            var cacheFolder = PortablePath.Combine(localStorage.Path, "Caches", "v2");

            NSFileManager.DefaultManager.Remove(dataFolder, out _error);
            NSFileManager.DefaultManager.Remove(cacheFolder, out _error);

            var domain = NSBundle.MainBundle.BundleIdentifier;
            NSUserDefaults.StandardUserDefaults.RemovePersistentDomain(domain);
        }

        #endregion

        #region Apple apps

        public static void OpenLink(NSUrl url)
        {
            UIApplication.SharedApplication.OpenUrl(url); //TODO deprecated
        }

        #endregion

    }
}
