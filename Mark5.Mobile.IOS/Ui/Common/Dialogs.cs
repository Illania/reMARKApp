//
// Project: Mark5.Mobile.IOS
// File: Dialogs.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{

    public static class Dialogs
    {

        #region Awaitable dialogs

        public static Task<bool> ShowYesNoDialogAsync(UIViewController nv, string title, string content, string positiveText = "Yes", string negativeText = "No")
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(positiveText, UIAlertActionStyle.Default, a => tcs.SetResult(true)));
            alert.AddAction(UIAlertAction.Create(negativeText, UIAlertActionStyle.Cancel, a => tcs.SetResult(false)));
            nv.PresentViewController(alert, true, null);
            return tcs.Task;
        }

        public static Task ShowConfirmDialogAsync(UIViewController nv, string title, string content)
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, a => tcs.SetResult(true)));
            nv.PresentViewController(alert, true, null);
            return tcs.Task;
        }

        #endregion

        #region Non-awaitable dialogs

        public static Func<Task> ShowInfiniteProgressDialog(UIViewController nv, string title, string content, CancellationTokenSource cts = null)
        {
            var alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            nv.PresentViewController(alert, true, null);

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
