using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model.Exceptions;
using Mark5.Mobile.Droid.Utilities;
using Mark5.ServiceReference.Exceptions;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public static class Dialogs
    {
        #region Awaitable dialogs

        public static Task<bool> ShowCustomViewDialogAsync(Context context, int titleId, View customView)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.CustomView(customView, true);
            builder.PositiveText(Resource.String.ok);
            builder.NegativeText(Resource.String.cancel);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(false)));
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<bool> ShowCustomViewDialogWithValidityAsync(Context context, int titleId, View customView, Func<bool> isContentValid)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.CustomView(customView, true);
            builder.PositiveText(Resource.String.ok);
            builder.NegativeText(Resource.String.cancel);
            builder.AutoDismiss(false);
            builder.OnPositive(new SingleButtonCallback(md =>
            {
                if (isContentValid())
                {
                    tcs.SetResult(true);
                    md.Dismiss();
                }
            }));

            builder.OnNegative(new SingleButtonCallback(md =>
            {
                tcs.SetResult(false);
                md.Dismiss();
            }));

            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<bool> ShowYesNoDialogAsync(Context context, int titleId, int contentId, int positiveTextId = Resource.String.yes, int negativeTextId = Resource.String.no)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(positiveTextId);
            builder.NegativeText(negativeTextId);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(false)));
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<bool> ShowYesNoDialogAsync(Context context, string title, string content, int positiveTextId = Resource.String.yes, int negativeTextId = Resource.String.no)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(title);
            builder.Content(content);
            builder.PositiveText(positiveTextId);
            builder.NegativeText(negativeTextId);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(false)));
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<int> ShowYesNoCancelDialogAsync(Context context, int titleId, int contentId = -1, int positiveTextId = Resource.String.yes, int negativeTextId = Resource.String.no, int cancelTextId = Resource.String.cancel)
        {
            var tcs = new TaskCompletionSource<int>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);

            if (contentId > 0)
                builder.Content(contentId);

            builder.PositiveText(positiveTextId);
            builder.NegativeText(negativeTextId);
            builder.NeutralText(cancelTextId);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(1)));
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(0)));
            builder.OnNeutral(new SingleButtonCallback(() => tcs.SetResult(-1)));
            builder.Cancelable(false);
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
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task ShowConfirmDialogAsync(Context context, string title, string content)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(title);
            builder.Content(content);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<T> ShowSingleSelectDialogAsync<T>(Context context, int titleId, List<T> values, T selected = default(T), IEqualityComparer<T> equalityComparer = null, Func<T, string> displayText = null)
        {
            var tcs = new TaskCompletionSource<T>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Items(values.Select(t => { return displayText == null ? t.ToString() : displayText(t); }).ToArray());
            builder.ItemsCallbackSingleChoice(-1,
                new SingleChoiceCallback(si =>
                {
                    if (si >= 0)
                        tcs.SetResult(values[si]);
                }));
            builder.PositiveText(Resource.String.ok);
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(selected)));
            var md = builder.Build();
            var selectedIndex = -1;
            if (!EqualityComparer<T>.Default.Equals(selected, default(T)))
            {
                for (var i = 0; i < values.Count; i++)
                    if (equalityComparer == null ? selected.Equals(values[i]) : equalityComparer.Equals(selected, values[i]))
                        selectedIndex = i;

                md.SetSelectedIndex(selectedIndex);
            }

            builder.Cancelable(false);
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
            builder.ItemsCallbackMultiChoice(null,
                new MultiChoiceCallback(si =>
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

                md.SetSelectedIndices(selectedIndexes.ToArray());
            }

            builder.Cancelable(false);
            md.Show();
            return tcs.Task;
        }

        public static Task<int> ShowListDialog(Context context, int titleId, int itemsId, bool includeCancel)
        {
            var tcs = new TaskCompletionSource<int>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Items(itemsId);
            builder.ItemsCallback(new ListCallback(i => tcs.SetResult(i)));
            if (includeCancel)
            {
                builder.NegativeText(Resource.String.cancel);
                builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(-1)));
            }
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<int> ShowListDialog(Context context, string title, int itemsId, bool includeCancel)
        {
            var tcs = new TaskCompletionSource<int>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(title);
            builder.Items(itemsId);
            builder.ItemsCallback(new ListCallback(i => tcs.SetResult(i)));
            if (includeCancel)
            {
                builder.NegativeText(Resource.String.cancel);
                builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(-1)));
            }
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<int> ShowListDialog(Context context, int titleId, string[] items, bool includeCancel)
        {
            var tcs = new TaskCompletionSource<int>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Items(items);
            builder.ItemsCallback(new ListCallback(i => tcs.SetResult(i)));
            if (includeCancel)
            {
                builder.NegativeText(Resource.String.cancel);
                builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(-1)));
            }
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<long> ShowDatePicker(Context context, long initialTimestamp = -1, long minTimestamp = -1, long maxTimestamp = -1, bool addRemoveDateChoice = false)
        {
            var tcs = new TaskCompletionSource<long>();
            var datePicker = new DatePicker(context);
            if (initialTimestamp >= 0)
                datePicker.DateTime = initialTimestamp.ConvertTimestampMillisecondsToDateTime();
            if (minTimestamp >= 0)
                datePicker.MinDate = minTimestamp;
            if (maxTimestamp >= 0)
                datePicker.MaxDate = maxTimestamp;
            var builder = new MaterialDialog.Builder(context);
            builder.CustomView(datePicker, false);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => { tcs.SetResult(datePicker.DateTime.ConvertDateTimeToTimestampMilliseconds()); }));
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(initialTimestamp)));
            if (addRemoveDateChoice)
            {
                builder.NeutralText(Resource.String.remove);
                builder.OnNeutral(new SingleButtonCallback(() => tcs.SetResult(0)));
            }
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        #endregion

        #region Non-awaitable dialogs

        public static void ShowYesNoDialog(Context context, int titleId, int contentId, Action positiveAction, Action negativeAction = null, int positiveTextId = Resource.String.yes, int negativeTextId = Resource.String.no)
        {
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(positiveTextId);
            builder.NegativeText(negativeTextId);
            builder.OnPositive(new SingleButtonCallback(positiveAction));
            if (negativeAction != null)
                builder.OnNegative(new SingleButtonCallback(negativeAction));
            builder.Cancelable(false);
            builder.Show();
        }

        public static void ShowEditTextDialog(Context context, int titleId, string startText, Action<string> positiveAction, Action negativeAction = null, int positiveTextId = Resource.String.yes, int negativeTextId = Resource.String.no)
        {
            var editTextView = new AppCompatEditText(context);
            editTextView.Text = startText;

            var builder = new MaterialDialog.Builder(context);
            builder.CustomView(editTextView, true);
            builder.Title(titleId);
            builder.PositiveText(positiveTextId);
            builder.NegativeText(negativeTextId);
            builder.OnPositive(new SingleButtonCallback(() => positiveAction(editTextView.Text)));
            if (negativeAction != null)
                builder.OnNegative(new SingleButtonCallback(negativeAction));
            builder.Cancelable(false);
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
            builder.Cancelable(false);
            builder.Show();
        }

        public static Action ShowInfiniteProgressDialog(Context context, int titleId, int contentId, CancellationTokenSource cts = null)
        {
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Content(contentId);
            builder.Progress(true, -1);
            if (cts != null)
            {
                builder.PositiveText(Resource.String.cancel);
                builder.OnPositive(new SingleButtonCallback(cts.Cancel));
            }
            builder.Cancelable(false);
            var dialog = builder.Show();
            return dialog.Dismiss;
        }

        public static void ShowMultiSelectDialog<T>(Context context, int titleId, List<T> values, Action<List<T>> positiveAction, Action negativeAction = null, List<T> selected = null, IEqualityComparer<T> equalityComparer = null, Func<T, string> displayText = null)
        {
            var result = new List<T>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Items(values.Select(t => { return displayText == null ? t.ToString() : displayText(t); }).ToArray());
            builder.ItemsCallbackMultiChoice(null,
                new MultiChoiceCallback(si =>
                {
                    foreach (var i in si)
                        result.Add(values[i]);

                    positiveAction(result);
                }));
            builder.PositiveText(Resource.String.ok);
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(negativeAction));
            var md = builder.Build();
            if (selected != null)
            {
                var selectedIndexes = new List<int>();
                for (var i = 0; i < values.Count; i++)
                    if (equalityComparer == null ? selected.Contains(values[i]) : selected.Contains(values[i], equalityComparer))
                        selectedIndexes.Add(i);

                md.SetSelectedIndices(selectedIndexes.ToArray());
            }

            builder.Cancelable(false);
            md.Show();
        }

        #endregion

        #region Error dialogs

        public static Task ShowErrorDialogAsync(Context context, Exception ex)
        {
            if (context == null)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(GetErrorTitle(context, ex));
            builder.Content(GetErrorMessage(context, ex));
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            if (ShouldShowCreateReport(ex))
            {
                builder.NeutralText(Resource.String.report);
                builder.OnNeutral(new SingleButtonCallback(() =>
                {
                    var dismissAction = ShowInfiniteProgressDialog(context, Resource.String.dialog_creating_report, Resource.String.please_wait);
                    Task.Run(() => { return SystemReportCollector.CreateFullReport(); })
                        .ContinueWith(t =>
                            {
                                dismissAction();

                                if (!t.IsFaulted)
                                    context.StartActivity(SystemReportCollector.CreateShareReportIntent(context, t.Result));

                                tcs.SetResult(true);
                            },
                            TaskScheduler.FromCurrentSynchronizationContext());
                }));
            }
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static void ShowErrorDialog(Context context, Exception ex, Action action = null)
        {
            if (context == null)
                return;

            var builder = new MaterialDialog.Builder(context);
            builder.Title(GetErrorTitle(context, ex));
            builder.Content(GetErrorMessage(context, ex));
            builder.PositiveText(Resource.String.ok);
            if (action != null)
                builder.OnPositive(new SingleButtonCallback(action));
            if (ShouldShowCreateReport(ex))
            {
                builder.NeutralText(Resource.String.report);
                builder.OnNeutral(new SingleButtonCallback(() =>
                {
                    var dismissAction = ShowInfiniteProgressDialog(context, Resource.String.dialog_creating_report, Resource.String.please_wait);
                    Task.Run(() => { return SystemReportCollector.CreateFullReport(); })
                        .ContinueWith(t =>
                            {
                                dismissAction();

                                if (!t.IsFaulted)
                                    context.StartActivity(SystemReportCollector.CreateShareReportIntent(context, t.Result));

                                action?.Invoke();
                            },
                            TaskScheduler.FromCurrentSynchronizationContext());
                }));
            }
            builder.Cancelable(false);
            builder.Show();
        }

        static string GetErrorTitle(Context context, Exception ex)
        {
            if (ex is WcfAppServiceException)
                return context.GetString(Resource.String.appserviceexception_title);
            if (ex is HttpAppServiceException)
                return context.GetString(Resource.String.appserviceexception_title);
            if (ex is FileTransferServiceException)
                return context.GetString(Resource.String.filetransferserviceexception_title);
            if (ex is DataNotFoundException)
                return context.GetString(Resource.String.datanotfoundexception_title);
            if (ex is DataAccessException)
                return context.GetString(Resource.String.dataaccessexception_title);
            if (ex is InvalidSourceTypeException)
                return context.GetString(Resource.String.invalidsourcetypeexception_title);
            if (ex is MailViewerException)
                return context.GetString(Resource.String.couldnotopenemlmsg_title);

            return context.GetString(Resource.String.generalexception_title);
        }

        static string GetErrorMessage(Context context, Exception ex)
        {
            if (ex is WcfAppServiceException)
                return ex.Message;
            if (ex is HttpAppServiceException)
                return ex.Message;
            if (ex is FileTransferServiceException)
                return ex.Message;
            if (ex is DataNotFoundException)
                return context.GetString(Resource.String.datanotfoundexception_message);
            if (ex is DataAccessException)
                return ex.Message;
            if (ex is InvalidSourceTypeException)
                return context.GetString(Resource.String.invalidsourcetypeexception_message);
            if (ex is MailViewerException)
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
            if (ex is MailViewerException)
                return true;

            return true;
        }

        #endregion

        #region Interface implementations

        class SingleButtonCallback : Java.Lang.Object, MaterialDialog.ISingleButtonCallback
        {
            readonly Action action;
            readonly Action<MaterialDialog> actionWithDialog;

            public SingleButtonCallback(Action action)
            {
                this.action = action;
            }

            public SingleButtonCallback(Action<MaterialDialog> action)
            {
                this.actionWithDialog = action;
            }

            public void OnClick(MaterialDialog p0, DialogAction p1)
            {
                action?.Invoke();
                actionWithDialog?.Invoke(p0);
            }
        }

        class SingleChoiceCallback : Java.Lang.Object, MaterialDialog.IListCallbackSingleChoice
        {
            readonly Action<int> action;

            public SingleChoiceCallback(Action<int> action)
            {
                this.action = action;
            }

            public bool OnSelection(MaterialDialog p0, View p1, int p2, string p3)
            {
                action?.Invoke(p2);
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

            public bool OnSelection(MaterialDialog p0, int[] p1, string[] p2)
            {
                action?.Invoke(p1);
                return true;
            }
        }

        class ListCallback : Java.Lang.Object, MaterialDialog.IListCallback
        {
            readonly Action<int> action;

            public ListCallback(Action<int> action)
            {
                this.action = action;
            }

            public void OnSelection(MaterialDialog p0, View p1, int p2, string p3)
            {
                action?.Invoke(p2);
            }
        }

        #endregion
    }
}