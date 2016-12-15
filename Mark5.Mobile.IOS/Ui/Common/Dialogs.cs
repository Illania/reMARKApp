//
// Project: Mark5.Mobile.IOS
// File: Dialogs.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Threading.Tasks;
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

        #endregion

        #region Non-awaitable dialogs

        public static Func<Task> ShowInfiniteProgressDialog(UIViewController vc, string title, string content)
        {
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            vc.PresentViewController(alert, true, null);

            return () =>
            {
                var tcs = new TaskCompletionSource<bool>();
                alert.DismissViewController(true, () => tcs.SetResult(true));
                return tcs.Task;
            };
        }

        #endregion
    }
}
