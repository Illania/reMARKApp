//
// Project: Mark5.Mobile.Droid
// File: Dialogs.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using Android.Support.V7.App;
using Android.Content;

namespace Mark5.Mobile.Droid.Views.Common
{

    public static class Dialogs
    {

        public static Task<bool> ShowYesNoDialog(Context context, int titleId, int messageId)
        {
            var tcs = new TaskCompletionSource<bool>();
            var b = new AlertDialog.Builder(context);
            b.SetTitle(titleId);
            b.SetMessage(messageId);
            b.SetPositiveButton("Yes", (sender, e) => tcs.SetResult(true));
            b.SetNegativeButton("No", (sender, e) => tcs.SetResult(false));
            b.Show();
            return tcs.Task;
        }

        public static Task<bool> ShowYesNoDialog(Context context, string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            var b = new AlertDialog.Builder(context);
            b.SetTitle(title);
            b.SetMessage(message);
            b.SetPositiveButton("Yes", (sender, e) => tcs.SetResult(true));
            b.SetNegativeButton("No", (sender, e) => tcs.SetResult(false));
            b.Show();
            return tcs.Task;
        }

        public static Task ShowConfirmDialog(Context context, int titleId, int messageId)
        {
            var tcs = new TaskCompletionSource<bool>();
            var b = new AlertDialog.Builder(context);
            b.SetTitle(titleId);
            b.SetMessage(messageId);
            b.SetPositiveButton("OK", (sender, e) => tcs.SetResult(true));
            b.Show();
            return tcs.Task;
        }

        public static Task ShowConfirmDialog(Context context, string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            var b = new AlertDialog.Builder(context);
            b.SetTitle(title);
            b.SetMessage(message);
            b.SetPositiveButton("OK", (sender, e) => tcs.SetResult(true));
            b.Show();
            return tcs.Task;
        }
    }
}

