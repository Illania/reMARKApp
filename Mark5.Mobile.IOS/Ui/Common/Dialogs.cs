using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Model.Exceptions;
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

        public static Task<bool> ShowYesNoDialogAsync(UIViewController vc, string title, string content)
        {
            return ShowYesNoDialogAsync(vc, title, content, Localization.GetString("yes"), Localization.GetString("no"));
        }

        public static Task<bool> ShowYesNoDialogAsync(UIViewController vc, string title, string content, string positiveText, string negativeText)
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(positiveText, UIAlertActionStyle.Default, a => tcs.SetResult(true)));
            alert.AddAction(UIAlertAction.Create(negativeText, UIAlertActionStyle.Cancel, a => tcs.SetResult(false)));
            vc.PresentViewController(alert, true, null);
            return tcs.Task;
        }

        public static Task ShowConfirmDialogAsync(UIViewController vc, string title, string content)
        {
            return ShowConfirmDialogAsync(vc, title, content, Localization.GetString("ok"));
        }

        public static Task ShowConfirmDialogAsync(UIViewController vc, string title, string content, string confirmationText)
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(confirmationText, UIAlertActionStyle.Default, a => tcs.SetResult(true)));
            vc.PresentViewController(alert, true, null);
            return tcs.Task;
        }

        public static Task<int> ShowListDialogAsync(UIViewController vc, string message, string[] listStrings, UIView anchorView)
        {
            var tcs = new TaskCompletionSource<int>();
            var actionSheet = PrepareLisDialogActionSheet(tcs, message, listStrings);
            if (actionSheet.PopoverPresentationController != null)
                actionSheet.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(anchorView);
            vc.PresentViewController(actionSheet, true, null);
            return tcs.Task;
        }

        public static Task<int> ShowListDialogAsync(UIViewController vc, string message, string[] listStrings, UIBarButtonItem anchorBarButtonItem)
        {
            var tcs = new TaskCompletionSource<int>();
            var actionSheet = PrepareLisDialogActionSheet(tcs, message, listStrings);
            if (actionSheet.PopoverPresentationController != null)
                actionSheet.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(anchorBarButtonItem);
            vc.PresentViewController(actionSheet, true, null);
            return tcs.Task;
        }

        public static Task<int> ShowListDialogAsync(UIViewController vc, string message, string[] listStrings, UITableView tableView, UITableViewCell anchorCell)
        {
            var tcs = new TaskCompletionSource<int>();
            var actionSheet = PrepareLisDialogActionSheet(tcs, message, listStrings);
            if (actionSheet.PopoverPresentationController != null)
                actionSheet.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, anchorCell);
            vc.PresentViewController(actionSheet, true, null);
            return tcs.Task;
        }

        public static Task<T[]> ShowMultiSelectDialogAsync<T>(UIViewController vc, string title, T[] data, T[] preselected, Func<T, string> description, IEqualityComparer<T> equalityComparer)
        {
            var msvc = new MultiSelectViewController<T>(title, data, preselected, description, equalityComparer);
            vc.PresentViewController(new NavigationController(msvc, UIModalPresentationStyle.FormSheet), true, null);
            return msvc.Task;
        }

        static UIAlertController PrepareLisDialogActionSheet(TaskCompletionSource<int> tcs, string message, string[] listStrings)
        {
            var actionSheet = UIAlertController.Create(null, message, UIAlertControllerStyle.ActionSheet);

            for (var i = 0; i < listStrings.Length; i++)
            {
                var ab = i; //Can't use i, because it's the variable, not the value, that's captured in the lambda)
                actionSheet.AddAction(UIAlertAction.Create(listStrings[i], UIAlertActionStyle.Default, a => tcs.SetResult(ab)));
            }

            actionSheet.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => tcs.SetResult(-1)));
            return actionSheet;
        }

        public static void ShowBlockingDialog(UIViewController vc, string content)
        {
            var alert = UIAlertController.Create(null, content, UIAlertControllerStyle.Alert);
            vc.PresentViewController(alert, true, null);
        }

        #endregion

        #region Non-awaitable dialogs

        public static Action ShowInfiniteProgressDialog(string content)
        {
            ProgressHUD.Instance.ShowProgress(Localization.GetString(content));
            return ProgressHUD.Instance.Dismiss;
        }

        #endregion

        #region Error dialogs

        public static Task ShowErrorDialogAsync(UIViewController vc, Exception ex)
        {
            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

            var tcs = new TaskCompletionSource<bool>();
            var alert = UIAlertController.Create(GetErrorTitle(ex), GetErrorContent(ex), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(Localization.GetString("ok"), UIAlertActionStyle.Default, a => tcs.SetResult(true)));
            if (ShouldShowCreateReport(ex))
                alert.AddAction(UIAlertAction.Create(Localization.GetString("report"),
                    UIAlertActionStyle.Cancel,
                    a =>
                    {
                        var dismissAction = ShowInfiniteProgressDialog(Localization.GetString("creating_system_report___"));
                        Task.Run(() => { return SystemReportCollector.CreateFullReport(); })
                            .ContinueWith(t =>
                                {
                                    dismissAction();

                                    if (!t.IsFaulted)
                                        vc.PresentViewController(SystemReportCollector.CreateShareReportController(t.Result), true, () => { tcs.SetResult(true); });
                                    else
                                        tcs.SetResult(true);
                                },
                                TaskScheduler.FromCurrentSynchronizationContext());
                    }));
            vc.PresentViewController(alert, true, () => hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Error));
            return tcs.Task;
        }

        public static void ShowErrorDialog(UIViewController vc, Exception ex)
        {
            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

            var alert = UIAlertController.Create(GetErrorTitle(ex), GetErrorContent(ex), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(Localization.GetString("ok"), UIAlertActionStyle.Default, null));
            if (ShouldShowCreateReport(ex))
                alert.AddAction(UIAlertAction.Create(Localization.GetString("report"),
                    UIAlertActionStyle.Cancel,
                    a =>
                    {
                        var dismissAction = ShowInfiniteProgressDialog(Localization.GetString("creating_system_report___"));
                        Task.Run(() => { return SystemReportCollector.CreateFullReport(); })
                            .ContinueWith(t =>
                                {
                                    dismissAction();

                                    if (!t.IsFaulted)
                                        vc.PresentViewController(SystemReportCollector.CreateShareReportController(t.Result), true, null);
                                },
                                TaskScheduler.FromCurrentSynchronizationContext());
                    }));
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

            return true;
        }

        #endregion
    }
}