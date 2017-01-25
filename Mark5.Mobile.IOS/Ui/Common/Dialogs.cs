//
// Project: Mark5.Mobile.IOS
// File: Dialogs.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.IOS.Utilities;
using Mark5.ServiceReference.Exceptions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{

    public static class Dialogs
    {

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

        public static void ShowBlockingDialog(UIViewController vc, string content)
        {
            var alert = UIAlertController.Create(null, content, UIAlertControllerStyle.Alert);
            vc.PresentViewController(alert, true, null);
        }

        #endregion

        #region Non-awaitable dialogs

        public static Action ShowInfiniteProgressDialog(string content)
        {
            // TODO
            //SVProgressHUD.ShowWithStatus(Localization.GetString(content));
            //return SVProgressHUD.Dismiss;
            return () => { };
        }

        #endregion

        #region Error dialogs

        public static Task ShowErrorDialogAsync(UIViewController vc, Exception ex)
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = UIAlertController.Create(GetErrorTitle(ex), GetErrorContent(ex), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(Localization.GetString("ok"), UIAlertActionStyle.Default, a => tcs.SetResult(true)));
            if (ShouldShowCreateReport(ex))
            {
                alert.AddAction(UIAlertAction.Create(Localization.GetString("report"), UIAlertActionStyle.Cancel, a =>
                {
                    var dismissAction = ShowInfiniteProgressDialog(Localization.GetString("creating_system_report___"));
                    Task.Run(() =>
                    {
                        return SystemReportCollector.CreateFullReport();
                    }).ContinueWith(t =>
                    {
                        dismissAction();

                        if (!t.IsFaulted)
                        {
                            vc.PresentViewController(SystemReportCollector.CreateShareReportController(t.Result), true, () =>
                            {
                                tcs.SetResult(true);
                            });
                        }
                        else
                        {
                            tcs.SetResult(true);
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }));
            }
            vc.PresentViewController(alert, true, null);
            return tcs.Task;
        }

        static string GetErrorTitle(Exception ex)
        {
            if (ex is AppServiceException)
            {
                return Localization.GetString("error_appserviceexception_title");
            }
            if (ex is FileTransferServiceException)
            {
                return Localization.GetString("error_filetransferserviceexception_title");
            }
            if (ex is DataNotFoundException)
            {
                return Localization.GetString("error_datanotfoundexception_title");
            }
            if (ex is DataAccessException)
            {
                return Localization.GetString("error_dataaccessexception_title");
            }
            if (ex is InvalidSourceTypeException)
            {
                return Localization.GetString("error_invalidsourcetypeexception_title");
            }

            return Localization.GetString("error_generalexception_title");
        }

        static string GetErrorContent(Exception ex)
        {
            if (ex is AppServiceException)
            {
                return ex.Message;
            }
            if (ex is FileTransferServiceException)
            {
                return ex.Message;
            }
            if (ex is DataNotFoundException)
            {
                return Localization.GetString("error_datanotfoundexception_message");
            }
            if (ex is DataAccessException)
            {
                return ex.Message;
            }
            if (ex is InvalidSourceTypeException)
            {
                return Localization.GetString("error_invalidsourcetypeexception_message");
            }

            return ex.Message;
        }

        static bool ShouldShowCreateReport(Exception ex)
        {
            if (ex is AppServiceException)
            {
                return true;
            }
            if (ex is FileTransferServiceException)
            {
                return true;
            }
            if (ex is DataNotFoundException)
            {
                return false;
            }
            if (ex is DataAccessException)
            {
                return true;
            }
            if (ex is InvalidSourceTypeException)
            {
                return false;
            }

            return true;
        }

        #endregion

    }
}
