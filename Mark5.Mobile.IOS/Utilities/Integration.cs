using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using ObjCRuntime;
using PCLStorage;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class Integration
    {

        #region General

        public static bool IsRunningAtLeast(int major) => UIDevice.CurrentDevice.CheckSystemVersion(major, 0);

        public static void ClearNotificationBadge()
        {
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 1;
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        #endregion

        #region iPhone/iPad recognition

        public static bool IsIPhone() => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone;
        public static bool IsIPad() => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;

        #endregion

        #region Model recognition

        const string HardwareProperty = "hw.machine";

        [DllImport(Constants.SystemLibrary)]
        static extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        public static string GetModelNumber()
        {
            try
            {
                var pLen = Marshal.AllocHGlobal(sizeof(int));
                sysctlbyname(HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);

                var length = Marshal.ReadInt32(pLen);

                if (length == 0)
                {
                    Marshal.FreeHGlobal(pLen);
                    return "Unknown";
                }

                var pStr = Marshal.AllocHGlobal(length);
                sysctlbyname(HardwareProperty, pStr, pLen, IntPtr.Zero, 0);

                var hardwareStr = Marshal.PtrToStringAnsi(pStr);

                Marshal.FreeHGlobal(pLen);
                Marshal.FreeHGlobal(pStr);

                return hardwareStr;
            }
            catch
            {
                return "Unknown";
            }
        }

        #endregion

        #region Screen size

        public static readonly CGSize IPhoneRetina40Resolution = new CGSize(640f, 1136f);
        public static readonly CGSize IPhoneRetina47Resolution = new CGSize(750f, 1334f);
        public static readonly CGSize IPhoneRetina47ZoomResolution = new CGSize(640f, 1136f);
        public static readonly CGSize IPhoneRetina55Resolution = new CGSize(1242f, 2208f);
        public static readonly CGSize IPhoneRetina55ZoomResolution = new CGSize(1125f, 2001f);
        public static readonly CGSize IPhoneRetina58Resolution = new CGSize(1125f, 2436f);
        public static readonly CGSize IPadRetina79ScreenSize = new CGSize(1536f, 2048f);
        public static readonly CGSize IPadRetina97ScreenSize = new CGSize(1536f, 2048f);
        public static readonly CGSize IPadProRetina105ProScreenSize = new CGSize(1668f, 2224f);
        public static readonly CGSize IPadProRetina129ProScreenSize = new CGSize(2048f, 2732f);

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
            var localStorage = FileSystem.Current.LocalStorage;
            var dataFolder = PortablePath.Combine(localStorage.Path, "v2");
            var cacheFolder = PortablePath.Combine(localStorage.Path, "Caches", "v2");

            NSFileManager.DefaultManager.Remove(dataFolder, out NSError _error);
            NSFileManager.DefaultManager.Remove(cacheFolder, out _error);

            var domain = NSBundle.MainBundle.BundleIdentifier;
            NSUserDefaults.StandardUserDefaults.RemovePersistentDomain(domain);
        }

        #endregion

        #region Apple apps

        public static void OpenLink(NSUrl url, Action failureCompletionHandler = null)
        {
            var options = new UIApplicationOpenUrlOptions();
            UIApplication.SharedApplication.OpenUrl(url, options, (result) =>
            {
                if (!result)
                    failureCompletionHandler?.Invoke();
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

                Dialogs.ShowErrorAlert(viewController, ex);
            }
        }

        public static void Call(UIViewController viewController, UIView view, string number)
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
                    callChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(view);

                viewController.PresentViewController(callChooser, true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorAlert(viewController, ex);
            }
        }

        public static void Text(UIViewController viewController, UITableView tableView, UITableViewCell cell, string number)
        {
            try
            {
                var processedNumber = new string(number.Where(c => char.IsDigit(c)).ToArray());

                if (number.Split('|').FirstOrDefault()?.Length > 0)
                    processedNumber = "+" + processedNumber;

                var callUrl = new NSUrl("sms://" + processedNumber);

                var callChooser = UIAlertController.Create(null, processedNumber, UIAlertControllerStyle.ActionSheet);
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("send_text"), UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(callUrl, new NSDictionary(), null)));
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

                if (callChooser.PopoverPresentationController != null)
                    callChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, cell);

                viewController.PresentViewController(callChooser, true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorAlert(viewController, ex);
            }
        }

        public static void Text(UIViewController viewController, UIView view, string number)
        {
            try
            {
                var processedNumber = new string(number.Where(c => char.IsDigit(c)).ToArray());

                if (number.Split('|').FirstOrDefault()?.Length > 0)
                    processedNumber = "+" + processedNumber;

                var callUrl = new NSUrl("sms://" + processedNumber);

                var callChooser = UIAlertController.Create(null, processedNumber, UIAlertControllerStyle.ActionSheet);
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("send_text"), UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(callUrl, new NSDictionary(), null)));
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

                if (callChooser.PopoverPresentationController != null)
                    callChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(view);

                viewController.PresentViewController(callChooser, true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorAlert(viewController, ex);
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
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("call"), UIAlertActionStyle.Default, a =>
                {
                    UIApplication.SharedApplication.OpenUrl(callUrl, new NSDictionary(), null);
                }));
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("send_text"), UIAlertActionStyle.Default, a =>
                {
                    UIApplication.SharedApplication.OpenUrl(textUrl, new NSDictionary(), null);
                }));
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

                if (callChooser.PopoverPresentationController != null)
                    callChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, cell);

                viewController.PresentViewController(callChooser, true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorAlert(viewController, ex);
            }
        }

        public static void CallOrText(UIViewController viewController, UIView view, string number)
        {
            try
            {
                var processedNumber = new string(number.Where(c => char.IsDigit(c)).ToArray());

                if (number.Split('|').FirstOrDefault()?.Length > 0)
                    processedNumber = "+" + processedNumber;

                var callUrl = new NSUrl("tel://" + processedNumber);
                var textUrl = new NSUrl("sms://" + processedNumber);

                var callChooser = UIAlertController.Create(null, processedNumber, UIAlertControllerStyle.ActionSheet);
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("call"), UIAlertActionStyle.Default, a =>
                {
                    UIApplication.SharedApplication.OpenUrl(callUrl, new NSDictionary(), null);
                }));
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("send_text"), UIAlertActionStyle.Default, a =>
                {
                    UIApplication.SharedApplication.OpenUrl(textUrl, new NSDictionary(), null);
                }));
                callChooser.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

                if (callChooser.PopoverPresentationController != null)
                    callChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(view);

                viewController.PresentViewController(callChooser, true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorAlert(viewController, ex);
            }
        }

        public static void ShowOnMap(UIViewController viewController, UITableView tableView, UITableViewCell cell, PhysicalAddress physicalAddress)
        {
            try
            {
                var qb = new StringBuilder();
                if (!string.IsNullOrEmpty(physicalAddress.Street))
                    qb.Append(physicalAddress.Street).Append(", ");
                if (!string.IsNullOrEmpty(physicalAddress.ZipCode))
                {
                    qb.Append(physicalAddress.ZipCode);
                    if (string.IsNullOrEmpty(physicalAddress.City))
                        qb.Append(", ");
                }
                if (!string.IsNullOrEmpty(physicalAddress.City))
                    qb.Append(" ").Append(physicalAddress.City).Append(", ");
                if (!string.IsNullOrEmpty(physicalAddress.Country?.Name))
                    qb.Append(physicalAddress.Country.Name);

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

                Dialogs.ShowErrorAlert(viewController, ex);
            }
        }

        public static void ShowOnMap(UIViewController viewController, UIView view, PhysicalAddress physicalAddress)
        {
            try
            {
                var qb = new StringBuilder();
                if (!string.IsNullOrEmpty(physicalAddress.Street))
                    qb.Append(physicalAddress.Street).Append(", ");
                if (!string.IsNullOrEmpty(physicalAddress.ZipCode))
                {
                    qb.Append(physicalAddress.ZipCode);
                    if (string.IsNullOrEmpty(physicalAddress.City))
                        qb.Append(", ");
                }
                if (!string.IsNullOrEmpty(physicalAddress.City))
                    qb.Append(" ").Append(physicalAddress.City).Append(", ");
                if (!string.IsNullOrEmpty(physicalAddress.Country?.Name))
                    qb.Append(physicalAddress.Country.Name);

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
                        browserChooser.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(view);

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

                Dialogs.ShowErrorAlert(viewController, ex);
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
                    var browserChooser = UIAlertController.Create(null, url, UIAlertControllerStyle.ActionSheet);
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

                Dialogs.ShowErrorAlert(viewController, ex);
            }
        }

        public static void CopyToClipboard(UIViewController viewController, UITableView tableView, UITableViewCell cell, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

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