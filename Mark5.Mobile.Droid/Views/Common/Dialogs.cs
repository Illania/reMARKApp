//
// Project: Mark5.Mobile.Droid
// File: Dialogs.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.App;

namespace Mark5.Mobile.Droid.Views.Common
{

    public static class Dialogs
    {

        public static Task<bool> ShowYesNoDialogAsync(Context context, int titleId, int messageId)
        {
            var tcs = new TaskCompletionSource<bool>();
            var b = new AlertDialog.Builder(context);
            b.SetTitle(titleId);
            b.SetMessage(messageId);
            b.SetPositiveButton(Resource.String.yes, (sender, e) => tcs.SetResult(true));
            b.SetNegativeButton(Resource.String.no, (sender, e) => tcs.SetResult(false));
            b.Show();
            return tcs.Task;
        }

        public static Task ShowConfirmDialogAsync(Context context, int titleId, int messageId)
        {
            var tcs = new TaskCompletionSource<bool>();
            var b = new AlertDialog.Builder(context);
            b.SetTitle(titleId);
            b.SetMessage(messageId);
            b.SetPositiveButton(Resource.String.ok, (sender, e) => tcs.SetResult(true));
            b.Show();
            return tcs.Task;
        }

        public static Task ShowErrorDialogAsync(Context context, Exception ex)
        {
            var tcs = new TaskCompletionSource<bool>();
            var b = new AlertDialog.Builder(context);
            b.SetTitle(Resource.String.error);
            b.SetMessage(ex.Message);
            b.SetPositiveButton(Resource.String.ok, (sender, e) => tcs.SetResult(true));
            b.Show();
            return tcs.Task;
        }
    }
}

