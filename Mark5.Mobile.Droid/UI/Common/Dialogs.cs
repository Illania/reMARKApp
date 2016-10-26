//
// Project: Mark5.Mobile.Droid
// File: Dialogs.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.Content;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public static class Dialogs
    {

        #region Awaitable dialogs

        public static Task<bool> ShowYesNoDialogAsync(Context context, int titleId, int contentId)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
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
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.Show();
            return tcs.Task;
        }

        public static Task<T> ShowSingleSelectDialogAsync<T>(Context context, int titleId, List<T> values, T selected = default(T), IEqualityComparer<T> equalityComparer = null, Func<T, string> displayText = null)
        {
            var tcs = new TaskCompletionSource<T>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Items(values.Select(t => { return displayText == null ? t.ToString() : displayText(t); }).ToArray());
            builder.ItemsCallbackSingleChoice(-1, new SingleChoiceCallback(si =>
            {
                tcs.SetResult(values[si]);
            }));
            builder.PositiveText(Resource.String.ok);
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(selected)));
            var md = builder.Build();
            var selectedIndex = -1;
            for (var i = 0; i < values.Count; i++)
                if (equalityComparer == null ? selected.Equals(values[i]) : equalityComparer.Equals(selected, values[i]))
                    selectedIndex = i;
            md.SelectedIndex = selectedIndex;
            md.Show();
            return tcs.Task;
        }

        public static Task<List<T>> ShowMultiSelectDialogAsync<T>(Context context, int titleId, List<T> values, List<T> selected = null, IEqualityComparer<T> equalityComparer = null, Func<T, string> displayText = null)
        {
            var tcs = new TaskCompletionSource<List<T>>();
            var result = new List<T>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Items(values.Select(t => { return displayText == null ? t.ToString() : displayText(t); }).ToArray());
            builder.ItemsCallbackMultiChoice(null, new MultiChoiceCallback(si =>
            {
                foreach (var i in si)
                    result.Add(values[i]);
                tcs.SetResult(result);
            }));
            builder.PositiveText(Resource.String.ok);
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(selected)));
            var md = builder.Build();
            if (selected != null)
            {
                var selectedIndexes = new List<int>();
                for (var i = 0; i < values.Count; i++)
                    if (equalityComparer == null ? selected.Contains(values[i]) : selected.Contains(values[i], equalityComparer))
                        selectedIndexes.Add(i);
                md.SetSelectedIndices(selectedIndexes.Select(i => new Java.Lang.Integer(i)).ToArray());
            }
            md.Show();
            return tcs.Task;
        }

        public static Task ShowErrorDialogAsync(Context context, Exception ex)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(Resource.String.error);
            builder.Content(ex.Message);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.Show();
            return tcs.Task;
        }

        public static Task<long> ShowDatePicker(Context context, long initialTimestamp = -1, long minTimestamp = -1, long maxTimestamp = -1)
        {
            var tcs = new TaskCompletionSource<long>();
            var datePicker = new DatePicker(context);
            if (initialTimestamp >= 0) datePicker.DateTime = initialTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToServerTime();
            if (minTimestamp >= 0) datePicker.MinDate = minTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToServerTime().ConvertDateTimeToTimestampMilliseconds();
            if (maxTimestamp >= 0) datePicker.MaxDate = maxTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToServerTime().ConvertDateTimeToTimestampMilliseconds();
            var builder = new MaterialDialog.Builder(context);
            builder.CustomView(datePicker, false);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() =>
            {
                tcs.SetResult(datePicker.DateTime.ConvertServerTimeToUtc().ConvertDateTimeToTimestampMilliseconds());
            }));
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(initialTimestamp)));
            builder.Show();
            return tcs.Task;
        }

        #endregion

        #region Non-awaitable dialogs

        public static void ShowYesNoDialog(Context context, int titleId, int contentId, Action positiveAction, Action negativeAction = null)
        {
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(Resource.String.yes);
            builder.NegativeText(Resource.String.no);
            builder.OnPositive(new SingleButtonCallback(positiveAction));
            if (negativeAction != null)
                builder.OnNegative(new SingleButtonCallback(positiveAction));
            builder.Show();
        }

        public static void ShowConfirmDialog(Context context, int titleId, int contentId, Action action = null)
        {
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(Resource.String.ok);
            if (action != null)
                builder.OnPositive(new SingleButtonCallback(action));
            builder.Show();
        }

        public static void ShowErrorDialog(Context context, Exception ex, Action action = null)
        {
            var builder = new MaterialDialog.Builder(context);
            builder.Title(Resource.String.error);
            builder.Content(ex.Message);
            builder.PositiveText(Resource.String.ok);
            if (action != null)
                builder.OnPositive(new SingleButtonCallback(action));
            builder.Show();
        }

        public static Action ShowInfiniteProgressDialog(Context context, int titleId, int contentId, CancellationTokenSource cts = null)
        {
            var builder = new MaterialDialog.Builder(context);
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
                    action();
            }
        }

        class SingleChoiceCallback : Java.Lang.Object, MaterialDialog.IListCallbackSingleChoice
        {

            readonly Action<int> action;

            public SingleChoiceCallback(Action<int> action)
            {
                this.action = action;
            }

            public bool OnSelection(MaterialDialog p0, View p1, int p2, Java.Lang.ICharSequence p3)
            {
                if (action != null)
                    action(p2);
                return true;
            }
        }

        class MultiChoiceCallback : Java.Lang.Object, MaterialDialog.IListCallbackMultiChoice
        {

            readonly Action<int[]> action;

            public MultiChoiceCallback(Action<int[]> action)
            {
                this.action = action;
            }

            public bool OnSelection(MaterialDialog p0, Java.Lang.Integer[] p1, Java.Lang.ICharSequence[] p2)
            {
                if (action != null)
                    action(p1.Select(i => i.IntValue()).ToArray());
                return true;
            }
        }

        #endregion

    }
}

