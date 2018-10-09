using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Utilities;
using Mark5.ServiceReference.Exceptions;
using SVProgressHUD;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public static class Dialogs
    {
        public static void Initialize()
        {
            ProgressHUD.DefaultStyle = Style.Light;
            ProgressHUD.DefaultMaskType = MaskType.Black;
            ProgressHUD.DefaultAnimationType = AnimationType.Flat;
            ProgressHUD.Font = Theme.DefaultFont;
            ProgressHUD.Initialize();
        }

        #region Awaitable dialogs

        public static Task ShowConfirmAlertAsync(UIViewController vc, string title, string content)
        {
            return ShowConfirmAlertAsync(vc, title, content, Localization.GetString("ok"));
        }

        public static Task ShowConfirmAlertAsync(UIViewController vc, string title, string content, string confirmationText)
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(confirmationText, UIAlertActionStyle.Default, a => tcs.SetResult(true)));
            vc.PresentViewController(alert, true, null);
            return tcs.Task;
        }

        public static Task<bool> ShowYesNoAlertAsync(UIViewController vc, string title, string content)
        {
            return ShowYesNoAlertAsync(vc, title, content, Localization.GetString("yes"), Localization.GetString("no"));
        }

        public static Task<bool> ShowYesNoAlertAsync(UIViewController vc, string title, string content, string positiveText, string negativeText)
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(positiveText, UIAlertActionStyle.Default, a => tcs.SetResult(true)));
            alert.AddAction(UIAlertAction.Create(negativeText, UIAlertActionStyle.Cancel, a => tcs.SetResult(false)));
            vc.PresentViewController(alert, true, null);
            return tcs.Task;
        }

        public static Task<int> ShowYesNoCancelAlertAsync(UIViewController vc, string title, string content, string yesText, string noText, string cancelText)
        {
            var tcs = new TaskCompletionSource<int>();
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(yesText, UIAlertActionStyle.Default, a => tcs.SetResult(1)));
            alert.AddAction(UIAlertAction.Create(noText, UIAlertActionStyle.Default, a => tcs.SetResult(0)));
            alert.AddAction(UIAlertAction.Create(cancelText, UIAlertActionStyle.Cancel, a => tcs.SetResult(-1)));
            vc.PresentViewController(alert, true, null);
            return tcs.Task;
        }

        public static Task<bool> ShowDestructiveActionSheetAsync(UIViewController vc, string destructiveText, UIView anchorView)
        {
            return ShowDestructiveActionSheetAsync(vc, destructiveText, new PopoverPresentationControllerDelegate(anchorView));
        }

        public static Task<bool> ShowDestructiveActionSheetAsync(UIViewController vc, string destructiveText, UIBarButtonItem anchorBarButtonItem)
        {
            return ShowDestructiveActionSheetAsync(vc, destructiveText, new PopoverPresentationControllerDelegate(anchorBarButtonItem));
        }

        public static Task<bool> ShowDestructiveActionSheetAsync(UIViewController vc, string destructiveText, UITableView tableView, UITableViewCell anchorCell)
        {
            return ShowDestructiveActionSheetAsync(vc, destructiveText, new PopoverPresentationControllerDelegate(tableView, anchorCell));
        }

        public static Task<bool> ShowDestructiveActionSheetAsync(UIViewController vc, string destructiveText, UIPopoverPresentationControllerDelegate d)
        {
            var tcs = new TaskCompletionSource<bool>();
            var actionSheet = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheet.AddAction(UIAlertAction.Create(destructiveText, UIAlertActionStyle.Destructive, a => tcs.SetResult(true)));
            actionSheet.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => tcs.SetResult(false)));
            if (actionSheet.PopoverPresentationController != null)
                actionSheet.PopoverPresentationController.Delegate = d;
            vc.PresentViewController(actionSheet, true, null);
            return tcs.Task;
        }

        public static Task<int> ShowListActionSheetAsync(UIViewController vc, string[] listStrings)
        {
            return ShowListActionSheetAsync(vc, listStrings, (UIPopoverPresentationControllerDelegate)null);
        }

        public static Task<int> ShowListActionSheetAsync(UIViewController vc, string[] listStrings, UIView anchorView)
        {
            return ShowListActionSheetAsync(vc, listStrings, new PopoverPresentationControllerDelegate(anchorView));
        }

        public static Task<int> ShowListActionSheetAsync(UIViewController vc, string[] listStrings, UIBarButtonItem anchorBarButtonItem)
        {
            return ShowListActionSheetAsync(vc, listStrings, new PopoverPresentationControllerDelegate(anchorBarButtonItem));
        }

        public static Task<int> ShowListActionSheetAsync(UIViewController vc, string[] listStrings, UITableView tableView, UITableViewCell anchorCell)
        {
            return ShowListActionSheetAsync(vc, listStrings, new PopoverPresentationControllerDelegate(tableView, anchorCell));
        }

        public static Task<int> ShowListActionSheetAsync(UIViewController vc, string[] listStrings, UIPopoverPresentationControllerDelegate d)
        {
            var tcs = new TaskCompletionSource<int>();
            var actionSheet = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            for (var i = 0; i < listStrings.Length; i++)
            {
                var ii = i;
                actionSheet.AddAction(UIAlertAction.Create(listStrings[ii], UIAlertActionStyle.Default, a => tcs.SetResult(ii)));
            }
            actionSheet.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => tcs.SetResult(-1)));
            if (d != null && actionSheet.PopoverPresentationController != null) //If the PopoverController property is even just read, it will try to present the action sheet as a popover
                actionSheet.PopoverPresentationController.Delegate = d;
            vc.PresentViewController(actionSheet, true, null);
            return tcs.Task;
        }

        public static Task<T[]> ShowMultiSelectViewControllerAsync<T>(UIViewController vc, string title, T[] data, T[] preselected, Func<T, string> description, IEqualityComparer<T> equalityComparer, bool requireSelection)
        {
            var msvc = new MultiSelectViewController<T>(title, data, preselected, description, equalityComparer, requireSelection);
            vc.PresentViewController(new NavigationController(msvc, UIModalPresentationStyle.FormSheet), true, null);
            return msvc.Result;
        }

        public static void ShowBlockingAlert(UIViewController vc, string content)
        {
            var alert = UIAlertController.Create(null, content, UIAlertControllerStyle.Alert);
            vc.PresentViewController(alert, true, null);
        }

        #endregion

        #region Non-awaitable dialogs

        public static Action ShowInfiniteProgressDialog(string content, Action onCancel = null)
        {
            ProgressHUD.Instance.ShowProgress(Localization.GetString(content), onCancel: onCancel);
            return ProgressHUD.Instance.Dismiss;
        }

        #endregion

        #region Error dialogs

        public static Task ShowErrorAlertAsync(UIViewController vc, Exception ex)
        {
            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

            var tcs = new TaskCompletionSource<bool>();
            var alert = UIAlertController.Create(GetErrorTitle(ex), GetErrorContent(ex), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(Localization.GetString("ok"), UIAlertActionStyle.Default, a => tcs.SetResult(true)));
            if (ShouldShowCreateReport(ex))
            {
                alert.AddAction(UIAlertAction.Create(Localization.GetString("report"),
                                                     UIAlertActionStyle.Cancel,
                                                     a =>
                {
                    var dismissAction = ShowInfiniteProgressDialog(Localization.GetString("creating_system_report___"));
                    Task.Run(() => SystemReportCollector.CreateFullReport()).ContinueWith(async t =>
                    {
                        dismissAction();

                        if (!t.IsFaulted)
                        {

                            var sendWithMark5 = await ShowYesNoAlertAsync(vc, Localization.GetString("send_with_mark5_title"), Localization.GetString("send_report_with_mark5_content"));

                            if (sendWithMark5)
                            {
                                var cvc = new ComposeDocumentViewController
                                {
                                    PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>() { { DocumentAddressType.To, new string[] { "appfeedback@nordic-it.com" } } },
                                    PreConfiguredSubject = Localization.GetString("mark5_ios_feedback"),
                                    PreconfiguredContent = t.Result
                                };

                                vc.PresentViewController(new NavigationController(cvc, UIModalPresentationStyle.PageSheet), true, null);
                            }
                            else 
                                vc.PresentViewController(SystemReportCollector.CreateShareReportController(t.Result), true, () => { tcs.SetResult(true); });
                        }
                        else
                            tcs.SetResult(true);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }));
            }
            vc.PresentViewController(alert, true, () => hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Error));
            return tcs.Task;
        }

        public static void ShowErrorAlert(UIViewController vc, Exception ex)
        {
            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

            var alert = UIAlertController.Create(GetErrorTitle(ex), GetErrorContent(ex), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(Localization.GetString("ok"), UIAlertActionStyle.Default, null));
            if (ShouldShowCreateReport(ex))
            {
                alert.AddAction(UIAlertAction.Create(Localization.GetString("report"),
                                                     UIAlertActionStyle.Cancel,
                                                     a =>
                {
                    var dismissAction = ShowInfiniteProgressDialog(Localization.GetString("creating_system_report___"));
                    Task.Run(() => SystemReportCollector.CreateFullReport()).ContinueWith(async t =>
                    {
                        dismissAction();

                        if (!t.IsFaulted)
                        {
                            var sendWithMark5 = await ShowYesNoAlertAsync(vc, Localization.GetString("send_with_mark5_title"), Localization.GetString("send_report_with_mark5_content"));

                            if (sendWithMark5)
                            {
                                var cvc = new ComposeDocumentViewController
                                {
                                    PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>() { { DocumentAddressType.To, new string[] { "appfeedback@nordic-it.com" } } },
                                    PreConfiguredSubject = Localization.GetString("mark5_ios_feedback"),
                                    PreconfiguredContent = t.Result
                                };

                                vc.PresentViewController(new NavigationController(cvc, UIModalPresentationStyle.PageSheet), true, null);
                            }
                            else
                                vc.PresentViewController(SystemReportCollector.CreateShareReportController(t.Result), true, null);
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }));
            }
            vc.PresentViewController(alert, true, () => hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Error));
        }

        static string GetErrorTitle(Exception ex)
        {
            if (ex is WcfAppServiceException)
                return Localization.GetString("error_appserviceexception_title");
            if (ex is HttpAppServiceException)
                return Localization.GetString("error_appserviceexception_title");
            if (ex is FileTransferServiceException)
                return Localization.GetString("error_filetransferserviceexception_title");
            if (ex is DataNotFoundException)
                return Localization.GetString("error_datanotfoundexception_title");
            if (ex is DataAccessException)
                return Localization.GetString("error_dataaccessexception_title");
            if (ex is InvalidSourceTypeException)
                return Localization.GetString("error_invalidsourcetypeexception_title");
            if (ex is ArgumentException)
                return Localization.GetString("error_argumentexception_title");

            return Localization.GetString("error_generalexception_title");
        }

        static string GetErrorContent(Exception ex)
        {
            if (ex is WcfAppServiceException)
                return ex.Message;
            if (ex is HttpAppServiceException)
                return ex.Message;
            if (ex is FileTransferServiceException)
                return ex.Message;
            if (ex is DataNotFoundException)
                return Localization.GetString("error_datanotfoundexception_message");
            if (ex is DataAccessException)
                return ex.Message;
            if (ex is InvalidSourceTypeException)
                return Localization.GetString("error_invalidsourcetypeexception_message");
            if (ex is ArgumentException)
                return ex.Message;

            return ex.Message;
        }

        static bool ShouldShowCreateReport(Exception ex)
        {
            if (ex is WcfAppServiceException)
                return true;
            if (ex is HttpAppServiceException)
                return true;
            if (ex is FileTransferServiceException)
                return true;
            if (ex is DataNotFoundException)
                return false;
            if (ex is DataAccessException)
                return true;
            if (ex is InvalidSourceTypeException)
                return false;
            if (ex is ArgumentException)
                return false;

            return true;
        }

        #endregion
    }
}