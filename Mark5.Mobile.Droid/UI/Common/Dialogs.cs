//
// Project: Mark5.Mobile.Droid
// File: Dialogs.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.Content;
using System.Threading;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public static class Dialogs
    {

        #region Awaitable dialogs

        public static Task<bool> ShowYesNoDialogAsync(Context context, int titleId, int contentId)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            //builder.Typeface("Avenir-Heavy.ttf", "Avenir-Book.ttf");
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(Resource.String.yes);
            builder.NegativeText(Resource.String.no);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(false)));
            builder.Show();
            return tcs.Task;
        }

        public static Task ShowConfirmDialogAsync(Context context, int titleId, int contentId)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            //builder.Typeface("Avenir-Heavy.ttf", "Avenir-Book.ttf");
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.Show();
            return tcs.Task;
        }

        public static Task ShowErrorDialogAsync(Context context, Exception ex)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            //builder.Typeface("Avenir-Heavy.ttf", "Avenir-Book.ttf");
            builder.Title(Resource.String.error);
            builder.Content(ex.Message);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.Show();
            return tcs.Task;
        }

        #endregion

        #region Non-awaitable dialogs

        public static void ShowYesNoDialog(Context context, int titleId, int contentId, Action positiveAction, Action negativeAction = null)
        {
            var builder = new MaterialDialog.Builder(context);
            //builder.Typeface("Avenir-Heavy.ttf", "Avenir-Book.ttf");
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(Resource.String.yes);
            builder.NegativeText(Resource.String.no);
            builder.OnPositive(new SingleButtonCallback(positiveAction));
            if (negativeAction != null)
            {
                builder.OnNegative(new SingleButtonCallback(positiveAction));
            }
            builder.Show();
        }

        public static void ShowConfirmDialog(Context context, int titleId, int contentId, Action action = null)
        {
            var builder = new MaterialDialog.Builder(context);
            //builder.Typeface("Avenir-Heavy.ttf", "Avenir-Book.ttf");
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(Resource.String.ok);
            if (action != null)
            {
                builder.OnPositive(new SingleButtonCallback(action));
            }
            builder.Show();
        }

        public static void ShowErrorDialog(Context context, Exception ex, Action action = null)
        {
            var builder = new MaterialDialog.Builder(context);
            //builder.Typeface("Avenir-Heavy.ttf", "Avenir-Book.ttf");
            builder.Title(Resource.String.error);
            builder.Content(ex.Message);
            builder.PositiveText(Resource.String.ok);
            if (action != null)
            {
                builder.OnPositive(new SingleButtonCallback(action));
            }
            builder.Show();
        }

        public static Action ShowInfiniteProgressDialog(Context context, int titleId, int contentId, CancellationTokenSource cts = null)
        {
            var builder = new MaterialDialog.Builder(context);
            //builder.Typeface("Avenir-Heavy.ttf", "Avenir-Book.ttf");
            builder.Title(titleId);
            builder.Content(contentId);
            builder.Progress(true, -1);
            builder.Cancelable(false);
            if (cts != null)
            {
                builder.PositiveText(Resource.String.cancel);
                builder.OnPositive(new SingleButtonCallback(cts.Cancel));
            }
            var dialog = builder.Show();
            return dialog.Dismiss;
        }

        #endregion

        #region Interface implementations

        class SingleButtonCallback : Java.Lang.Object, MaterialDialog.ISingleButtonCallback
        {

            readonly Action action;

            public SingleButtonCallback(Action action)
            {
                this.action = action;
            }

            public void OnClick(MaterialDialog p0, DialogAction p1)
            {
                if (action != null)
                {
                    action();
                }
            }
        }

        #endregion
    }
}

