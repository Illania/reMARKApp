//
// Project: Mark5.Mobile.IOS
// File: Integration.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using System.Text;
using AudioToolbox;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers;
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

        public static void OpenLink(NSUrl url, Action failureCompletionHandler)
        {
            var options = new UIApplicationOpenUrlOptions();
            UIApplication.SharedApplication.OpenUrl(url, options, (result) =>
            {
                if (!result)
                {
                    failureCompletionHandler();
                }
            });
        }

        #endregion

        #region Sharing

        public static void Call(UIViewController viewController, UITableView tableView, UITableViewCell cell, string number)
        {
            try
            {
                var processedNumber = new string(number.Where(c => char.IsDigit(c)).ToArray());

                if (number.Split('|').FirstOrDefault()?.Length > 0)
                    processedNumber = "+" + processedNumber;

                var callUrl = new NSUrl("tel://" + processedNumber);

                var callChooser = UIAlertController.Create(null, processedNumber, UIAlertControllerStyle.ActionSheet);
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("call"), UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(callUrl, new NSDictionary(), null)));
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

                if (callChooser.PopoverPresentationController != null)
                    callChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, cell);

                viewController.PresentViewController(callChooser, true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorDialog(viewController, ex);
            }
        }

        public static void CallOrText(UIViewController viewController, UITableView tableView, UITableViewCell cell, string number)
        {
            try
            {
                var processedNumber = new string(number.Where(c => char.IsDigit(c)).ToArray());

                if (number.Split('|').FirstOrDefault()?.Length > 0)
                    processedNumber = "+" + processedNumber;

                var callUrl = new NSUrl("tel://" + processedNumber);
                var textUrl = new NSUrl("sms://" + processedNumber);

                var callChooser = UIAlertController.Create(null, processedNumber, UIAlertControllerStyle.ActionSheet);
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("call"), UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(callUrl, new NSDictionary(), null)));
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("text"), UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(textUrl, new NSDictionary(), null)));
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

                if (callChooser.PopoverPresentationController != null)
                    callChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, cell);

                viewController.PresentViewController(callChooser, true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorDialog(viewController, ex);
            }
        }

        public static void ShowOnMap(UIViewController viewController, UITableView tableView, UITableViewCell cell, PhysicalAddress physicalAddress)
        {
            try
            {
                var qb = new StringBuilder();
                if (!string.IsNullOrEmpty(physicalAddress.Street))
                {
                    qb.Append(physicalAddress.Street).Append(", ");
                }
                if (!string.IsNullOrEmpty(physicalAddress.ZipCode))
                {
                    qb.Append(physicalAddress.ZipCode);
                    if (string.IsNullOrEmpty(physicalAddress.City))
                    {
                        qb.Append(", ");
                    }
                }
                if (!string.IsNullOrEmpty(physicalAddress.City))
                {
                    qb.Append(" ").Append(physicalAddress.City).Append(", ");
                }
                if (!string.IsNullOrEmpty(physicalAddress.Country?.Name))
                {
                    qb.Append(physicalAddress.Country.Name);
                }

                var address = Uri.EscapeUriString(qb.ToString());

                var appleMapsUrl = new NSUrl($"http://maps.apple.com/maps?q={address}");
                var googleMapsUrl = new NSUrl($"comgooglemapsurl://maps.google.com/?q={address}");

                if (UIApplication.SharedApplication.CanOpenUrl(googleMapsUrl))
                {
                    var browserChooser = UIAlertController.Create(null, qb.ToString(), UIAlertControllerStyle.ActionSheet);
                    browserChooser.AddAction(UIAlertAction.Create(Localization.GetString("open_in_apple_maps"), UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(appleMapsUrl, new NSDictionary(), null)));
                    browserChooser.AddAction(UIAlertAction.Create(Localization.GetString("open_in_google_maps"), UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(googleMapsUrl, new NSDictionary(), null)));
                    browserChooser.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

                    if (browserChooser.PopoverPresentationController != null)
                        browserChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, cell);

                    viewController.PresentViewController(browserChooser, true, null);
                }
                else
                {
                    UIApplication.SharedApplication.OpenUrl(appleMapsUrl, new NSDictionary(), null);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorDialog(viewController, ex);
            }
        }

        public static void OpenUrl(UIViewController viewController, UITableView tableView, UITableViewCell cell, string url)
        {
            try
            {
                var safariUrl = new NSUrl(url.Trim().StartsWith("http", StringComparison.CurrentCultureIgnoreCase) ? url : "http://" + url);
                var chromeUrl = new NSUrl(url.Trim().StartsWith("http", StringComparison.CurrentCultureIgnoreCase) ? url.Replace("http", "googlechrome") : "googlechrome://" + url);

                if (UIApplication.SharedApplication.CanOpenUrl(chromeUrl))
                {
                    var browserChooser = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
                    browserChooser.AddAction(UIAlertAction.Create(Localization.GetString("safari"), UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(safariUrl, new NSDictionary(), null)));
                    browserChooser.AddAction(UIAlertAction.Create(Localization.GetString("chrome"), UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(chromeUrl, new NSDictionary(), null)));
                    browserChooser.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

                    if (browserChooser.PopoverPresentationController != null)
                        browserChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, cell);

                    viewController.PresentViewController(browserChooser, true, null);
                }
                else
                {
                    UIApplication.SharedApplication.OpenUrl(safariUrl, new NSDictionary(), null);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorDialog(viewController, ex);
            }
        }

        public static void CopyToClipboard(UIViewController viewController, UITableView tableView, UITableViewCell cell, string text)
        {
            var browserChooser = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            browserChooser.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_clipboard"), UIAlertActionStyle.Default, a => UIPasteboard.General.String = text));
            browserChooser.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (browserChooser.PopoverPresentationController != null)
                browserChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, cell);

            viewController.PresentViewController(browserChooser, true, null);
        }

        #endregion

    }
}
