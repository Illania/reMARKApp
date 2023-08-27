using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;
using Mark5.ServiceReference.Exceptions;
using Mark5.Mobile.Common.Authenticator;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Android.App;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public static class Dialogs
    {
        class MaterialDialog
        {
            public class Builder
            {
                private readonly AlertDialog.Builder builder;

                private int positiveTextId;
                private int negativeTextId;
                private int neutralTextId;

                private int itemsId;
                private string[] items;

                public Builder(Context context)
                {
                    builder = new AlertDialog.Builder(context);
                }

                internal void Title(int titleId)
                {
                    builder.SetTitle(titleId);
                }

                internal void TitleGravity()
                {
                    
                }

                internal void Title(string title)
                {
                    builder.SetTitle(title);
                }

                internal void CustomView(View customView)
                {
                    builder.SetView(customView);
                }

                internal void PositiveText(int positiveTextId)
                {
                    this.positiveTextId = positiveTextId;
                }

                internal void NegativeText(int negativeTextId)
                {
                    this.negativeTextId = negativeTextId;
                }

                internal void NeutralText(int neutralTextId)
                {
                    this.neutralTextId = neutralTextId;
                }

                internal void OnPositive(SingleButtonCallback callback)
                {
                    builder.SetPositiveButton(positiveTextId, callback);
                }

                internal void OnNegative(SingleButtonCallback callback)
                {
                    builder.SetNegativeButton(negativeTextId, callback);
                }

                internal void OnNeutral(SingleButtonCallback callback)
                {
                    builder.SetNeutralButton(neutralTextId, callback);
                }

                internal void Cancelable(bool isCancelable)
                {
                    builder.SetCancelable(isCancelable);
                }

                internal IDialogInterface Show()
                {
                    return builder.Show();
                }

                internal void AutoDismiss(bool v)
                {
                    //TODO To implement. This is not so easy, because normally alert dialogs are dismissed automatically
                }

                internal void Content(int contentId)
                {
                    builder.SetMessage(contentId);
                }

                internal void Content(string content)
                {
                    builder.SetMessage(content);
                }

                internal void Items(string[] items)
                {
                    this.items = items;
                }

                internal void Items(int itemsId)
                {
                    this.itemsId = itemsId;
                }

                //List, single choice, dismiss when clicking an item of the list
                internal void ItemsCallback(ListCallback listCallback)
                {
                    if (itemsId > 0)
                        builder.SetItems(itemsId, listCallback);
                    else
                        builder.SetItems(items, listCallback);
                }

                //List, single choice, dismiss when clicking positive or negative button
                internal void ItemsCallbackSingleChoice(int preselectedItemIndex, SingleChoiceCallback singleChoiceCallback)
                {
                    if (itemsId > 0)
                        builder.SetSingleChoiceItems(itemsId, preselectedItemIndex, singleChoiceCallback);
                    else
                        builder.SetSingleChoiceItems(items, preselectedItemIndex, singleChoiceCallback);
                }

                //List, multiple choice, dismiss on button click
                internal void ItemsCallbackMultiChoice(bool[] checkedItemIndexes, MultiChoiceCallback multiChoiceCallback)
                {
                    if (itemsId > 0)
                        builder.SetMultiChoiceItems(itemsId, checkedItemIndexes, multiChoiceCallback);
                    else
                        builder.SetMultiChoiceItems(items, checkedItemIndexes, multiChoiceCallback);
                }

                internal IDialogInterface Build()
                {
                    return builder.Create();
                }
            }
        }

        #region Awaitable dialogs

        public static Task<bool> ShowCustomViewDialogAsync(Context context, int titleId, View customView)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.CustomView(customView);
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
            builder.CustomView(customView);
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

        public static Task<bool> ShowYesNoDialogAsync(Context context, string title, string content, int positiveTextId = Resource.String.yes,
          int negativeTextId = Resource.String.no, bool centerTitle = false, bool centerContent = false)
        {
            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(title);
            builder.Content(content);
            builder.PositiveText(positiveTextId);
            builder.NegativeText(negativeTextId);
            //// TODO
            //if (centerTitle)
            //    builder.TitleGravity(GravityEnum.Center);
            //if (centerContent)
            //    builder.ContentGravity(GravityEnum.Center);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(false)));
            builder.Cancelable(false);
            var dialog = builder.Show();
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

        //TODO need to test if this works properly...
        public static Task<T> ShowSingleSelectDialogAsync<T>(Context context, int titleId, List<T> values, T preselected = default(T), IEqualityComparer<T> equalityComparer = null, Func<T, string> displayText = null)
        {
            T newlyselected = default(T);
            var tcs = new TaskCompletionSource<T>();
            var builder = new MaterialDialog.Builder(context);

            var selectedIndex = -1;
            if (!EqualityComparer<T>.Default.Equals(preselected, default(T)))
            {
                for (var i = 0; i < values.Count; i++)
                    if (equalityComparer == null ? preselected.Equals(values[i]) : equalityComparer.Equals(preselected, values[i]))
                        selectedIndex = i;
            }

            builder.Title(titleId);
            builder.Items(values.Select(t => { return displayText == null ? t.ToString() : displayText(t); }).ToArray());
            builder.ItemsCallbackSingleChoice(selectedIndex,
                new SingleChoiceCallback(si =>
                {
                    if (si >= 0)
                        newlyselected = values[si];
                }));
            builder.PositiveText(Resource.String.ok);
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(preselected)));
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(newlyselected)));


            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        //TODO need to test if this works properly...
        public static Task<List<T>> ShowMultiSelectDialogAsync<T>(Context context, int titleId, List<T> values, List<T> preselected = null, IEqualityComparer<T> equalityComparer = null, Func<T, string> displayText = null)
        {
            bool[] isCheckedArray = new bool[values.Count];
            var tcs = new TaskCompletionSource<List<T>>();
            var result = new List<T>();
            var builder = new MaterialDialog.Builder(context);

            if (preselected != null)
            {
                var selectedIndexes = new List<int>();
                for (var i = 0; i < values.Count; i++)
                    if (equalityComparer == null ? preselected.Contains(values[i]) : preselected.Contains(values[i], equalityComparer))
                        isCheckedArray[i] = true;
            }

            builder.Title(titleId);
            builder.Items(values.Select(t => { return displayText == null ? t.ToString() : displayText(t); }).ToArray());
            builder.ItemsCallbackMultiChoice(isCheckedArray, new MultiChoiceCallback((index, isChecked) =>
            {
                isCheckedArray[index] = isChecked;
            }));
            builder.PositiveText(Resource.String.ok);
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(null)));
            builder.OnPositive(new SingleButtonCallback(() =>
            {
                for (int i = 0; i < isCheckedArray.Length; i++)
                {
                    if (isCheckedArray[i])
                        result.Add(values[i]);
                }

                tcs.SetResult(result);
            }));

            builder.Cancelable(false);
            builder.Show();
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

        public static Task<int> ShowListDialog(Context context, string title, string[] items, bool includeCancel)
        {
            var tcs = new TaskCompletionSource<int>();
            var builder = new MaterialDialog.Builder(context);
            if (!string.IsNullOrEmpty(title))
                builder.Title(title);
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


        public static Task<int> ShowListDialog(Context context, int titleId, string description, string[] items, bool includeCancel)
        {
            var tcs = new TaskCompletionSource<int>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Content(description);
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

        public static void ShowListDialog(Context context, int titleId, int itemsId, bool includeCancel, Action<int> itemAction, Action negativeAction)
        {
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Items(itemsId);
            builder.ItemsCallback(new ListCallback(i => itemAction(i)));
            if (includeCancel)
            {
                builder.NegativeText(Resource.String.cancel);
                builder.OnNegative(new SingleButtonCallback(negativeAction));
            }
            builder.Cancelable(false);
            builder.Show();
        }

        public static Task<DateTime> ShowDatePicker(Context context)
        {
            var tcs = new TaskCompletionSource<DateTime>();
            var datePicker = new DatePicker(context);
            datePicker.DateTime = DateTime.Now;
            datePicker.MinDate = datePicker.DateTime.ConvertDateTimeToTimestampMilliseconds();
            var builder = new MaterialDialog.Builder(context);
            builder.CustomView(datePicker);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => { tcs.SetResult(datePicker.DateTime); }));
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(tcs.SetCanceled));
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<long> ShowDatePicker(Context context, long initialTimestamp = -1, long minTimestamp = -1, long maxTimestamp = -1,
        bool addRemoveDateChoice = false)
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
            builder.CustomView(datePicker);
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

        public static Task<TimeSpan> ShowTimePicker(Context context, int initialHour = -1, int initialMinute = -1)
        {
            var tcs = new TaskCompletionSource<TimeSpan>();
            var timePicker = new TimePicker(context);
            if (initialHour >= 0 && initialMinute >= 0)
            {
                timePicker.Hour = initialHour;
                timePicker.Minute = initialMinute;
            }

            timePicker.SetIs24HourView((Java.Lang.Boolean)false);

            var builder = new MaterialDialog.Builder(context);
            builder.CustomView(timePicker);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => { tcs.SetResult(new TimeSpan(timePicker.Hour, timePicker.Minute, 0)); }));
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(() => tcs.SetResult(new TimeSpan(initialHour, initialMinute, 0))));
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<(int, int)> ShowTimePicker(Context context)
        {
            var tcs = new TaskCompletionSource<(int, int)>();
            var timePicker = new TimePicker(context);
            if (Android.Text.Format.DateFormat.Is24HourFormat(Android.App.Application.Context))
                timePicker.SetIs24HourView((Java.Lang.Boolean)true);
            var builder = new MaterialDialog.Builder(context);
            builder.CustomView(timePicker);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => { tcs.SetResult((timePicker.Hour, timePicker.Minute)); }));
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(tcs.SetCanceled));
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }

        public static Task<(int, int)> ShowHourMinutePicker(Context context, (int hours, int minutes) defaultTime)
        {
            var pickerContainer = new LinearLayoutCompat(context)
            {
                Orientation = LinearLayoutCompat.Horizontal,
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1f)
            };

            var hourPicker = new NumberPicker(context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f)
            };

            var displayedHourValues = new List<String>();

            for (int i = 0; i < 24; i++)
                displayedHourValues.Add(String.Format("{0:D2}", i));

            hourPicker.MinValue = 0;
            hourPicker.MaxValue = displayedHourValues.Count - 1;
            hourPicker.SetDisplayedValues(displayedHourValues.ToArray());
            hourPicker.Value = defaultTime.hours;

            var minutePicker = new NumberPicker(context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f)
            };

            var displayedMinuteValues = new List<String>();

            var step = 5;

            for (int i = 0; i < 60; i += step)
                displayedMinuteValues.Add(String.Format("{0:D2}", i));

            minutePicker.MinValue = 0;
            minutePicker.MaxValue = displayedMinuteValues.Count - 1;
            minutePicker.SetDisplayedValues(displayedMinuteValues.ToArray());
            minutePicker.Value = defaultTime.minutes / step;

            pickerContainer.AddView(hourPicker);
            pickerContainer.AddView(minutePicker);

            var tcs = new TaskCompletionSource<(int, int)>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title("Select hours and minutes:");
            builder.CustomView(pickerContainer);
            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult((hourPicker.Value, minutePicker.Value * step))));
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(tcs.SetCanceled));
            builder.Cancelable(false);
            builder.Show();
            return tcs.Task;
        }


        #endregion

        #region Non-awaitable dialogs

        public static void ShowYesNoDialog(Context context, int titleId, int contentId, Action positiveAction, Action negativeAction = null, int positiveTextId = Resource.String.yes, int negativeTextId = Resource.String.no, bool cancelable = false)
        {
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Content(contentId);
            builder.PositiveText(positiveTextId);
            builder.NegativeText(negativeTextId);
            builder.OnPositive(new SingleButtonCallback(positiveAction));
            if (negativeAction != null)
                builder.OnNegative(new SingleButtonCallback(negativeAction));
            builder.Cancelable(cancelable);
            builder.Show();
        }

        public static void ShowEditTextDialog(Context context, int titleId, string startText, Action<string> positiveAction, Action negativeAction = null, int positiveTextId = Resource.String.yes, int negativeTextId = Resource.String.no)
        {
            var editTextView = new AppCompatEditText(context);
            editTextView.Text = startText;

            var tcs = new TaskCompletionSource<(int, int)>();
            var builder = new MaterialDialog.Builder(context);
            builder.CustomView(editTextView);
            builder.Title(titleId);
            builder.PositiveText(positiveTextId);
            builder.NegativeText(negativeTextId);
            builder.OnPositive(new SingleButtonCallback(() => positiveAction(editTextView.Text)));
            if (negativeAction != null)
                builder.OnNegative(new SingleButtonCallback(negativeAction));
            else
                builder.OnNegative(new SingleButtonCallback(tcs.SetCanceled));
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
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.progress_dialog, null, false);

            var title = view.FindViewById<AppCompatTextView>(Resource.Id.title);
            var content = view.FindViewById<AppCompatTextView>(Resource.Id.content);
            var progress = view.FindViewById<ProgressBar>(Resource.Id.progress);

            title.SetText(titleId);
            content.SetText(contentId);

            var builder = new MaterialDialog.Builder(context);
            builder.CustomView(view);
            if (cts != null)
            {
                builder.PositiveText(Resource.String.cancel);
                builder.OnPositive(new SingleButtonCallback(cts.Cancel));
            }
            builder.Cancelable(false);
            var dialog = builder.Show();
            return dialog.Dismiss;
        }

        public static void ShowMultiSelectDialog<T>(Context context, int titleId, List<T> values, Action<List<T>> positiveAction, Action negativeAction = null, List<T> preselected = null, IEqualityComparer<T> equalityComparer = null, Func<T, string> displayText = null)
        {
            bool[] isCheckedArray = new bool[values.Count];
            var result = new List<T>();
            var builder = new MaterialDialog.Builder(context);

            if (preselected != null)
            {
                var selectedIndexes = new List<int>();
                for (var i = 0; i < values.Count; i++)
                    if (equalityComparer == null ? preselected.Contains(values[i]) : preselected.Contains(values[i], equalityComparer))
                        isCheckedArray[i] = true;
            }

            builder.Title(titleId);
            builder.Items(values.Select(t => { return displayText == null ? t.ToString() : displayText(t); }).ToArray());
            builder.ItemsCallbackMultiChoice(isCheckedArray, new MultiChoiceCallback((index, isChecked) =>
            {
                isCheckedArray[index] = isChecked;
            }));
            builder.PositiveText(Resource.String.ok);
            builder.NegativeText(Resource.String.cancel);
            builder.OnNegative(new SingleButtonCallback(negativeAction));
            builder.OnPositive(new SingleButtonCallback(() =>
            {
                for (int i = 0; i < isCheckedArray.Length; i++)
                {
                    if (isCheckedArray[i])
                        result.Add(values[i]);
                }

                positiveAction(result);
            }));

            builder.Cancelable(false);
            builder.Show();
        }

        public static void ShowBlockingAlert(Context context, int titleId)
        {
            var builder = new MaterialDialog.Builder(context);
            builder.Title(titleId);
            builder.Cancelable(false);
            builder.Show();
            return;
        }

        #endregion

        #region Error dialogs

        public static async Task ShowErrorDialogAsync(Context context, Exception ex)
        {
            if (context == null)
                return;

            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);
            builder.Title(GetErrorTitle(context, ex));
            builder.Content(GetErrorMessage(context, ex));

            if (IsAccessDisabled(ex))
            {
                await AuthenticatorFactory.Create().RetainConnectionInfoAsync();
                await Integration.ClearData(context);

                ShowBlockingAlert(context, Resource.String.access_disabled);
                return;
            }

            builder.PositiveText(Resource.String.ok);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            if (ShouldShowCreateReport(ex))
            {
                builder.NeutralText(Resource.String.report);
                builder.OnNeutral(new SingleButtonCallback(() =>
                {
                    var dismissAction = ShowInfiniteProgressDialog(context, Resource.String.dialog_creating_report, Resource.String.please_wait);
                    Task.Run(() => { return SystemReportCollector.CreateFullReport(); })
                        .ContinueWith(async t =>
                        {
                            dismissAction();

                            if (!t.IsFaulted)
                            {
                                var sendWithReMARK = await ShowYesNoDialogAsync(context, Resource.String.send_with_mark5_title, Resource.String.send_report_with_mark5_content);

                                if (sendWithReMARK)
                                    context.StartActivity(SystemReportCollector.CreateShareReportComposeDocumentActivityIntent(context, t.Result));
                                else
                                    context.StartActivity(SystemReportCollector.CreateShareReportIntent(context, t.Result));
                            }

                            tcs.SetResult(true);
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }));
            }
            builder.Cancelable(false);
            builder.Show();
            await tcs.Task;
            return;
        }

        public static bool IsAccessDisabled(Exception ex)
        {
            if (ex is HttpAppServiceException httpEx)
            {
                var code = httpEx?.Detail?.Code;

                return code == AppServiceFaultCode.InvalidToken
                    || code == AppServiceFaultCode.MobileLicenseError
                    || code == AppServiceFaultCode.AccessDisabled;
            }

            return false;
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
                        .ContinueWith(async t =>
                        {
                            dismissAction.Invoke();

                            if (!t.IsFaulted)
                            {
                                var sendWithMark5 = await ShowYesNoDialogAsync(context, Resource.String.send_with_mark5_title, Resource.String.send_report_with_mark5_content);

                                if (sendWithMark5)
                                    context.StartActivity(SystemReportCollector.CreateShareReportComposeDocumentActivityIntent(context, t.Result));
                                else
                                    context.StartActivity(SystemReportCollector.CreateShareReportIntent(context, t.Result));

                                action?.Invoke();
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }));
            }
            builder.Cancelable(false);
            builder.Show();
        }

        public static Task SendCriticalReport(Context context, Exception ex)
        {
            if (context == null)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();
            var builder = new MaterialDialog.Builder(context);

            builder.Title(Resource.String.critical_exception_title);
            builder.Content(Resource.String.critical_exception_message);

            builder.PositiveText(Resource.String.report);
            builder.OnPositive(new SingleButtonCallback(() => tcs.SetResult(true)));
            builder.NeutralText(Resource.String.cancel);

            builder.OnPositive(new SingleButtonCallback(() =>
            {
                var dismissAction = ShowInfiniteProgressDialog(context, Resource.String.dialog_creating_report, Resource.String.please_wait);
                Task.Run(() => { return SystemReportCollector.CreateFullReport(); })
                    .ContinueWith(t =>
                    {
                        context.StartActivity(SystemReportCollector.CreateShareReportIntent(context, t.Result));
                        dismissAction.Invoke();
                        tcs.SetResult(true);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }));

            builder.Cancelable(false);
            builder.Show();

            return tcs.Task;
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
            if (ex is ReMarkException && ((ReMarkException)ex).ErrorCode.Equals(ErrorConstants.Codes.InvalidSourceType))
                return context.GetString(Resource.String.invalidsourcetypeexception_title);
            if (ex is ReMarkException && ((ReMarkException)ex).ErrorCode.Equals(ErrorConstants.Codes.FileTooLarge))
                return context.GetString(Resource.String.file_too_large);
            if (ex is ReMarkException && ((ReMarkException)ex).ErrorCode.Equals(ErrorConstants.Codes.FileCouldNotBeLoaded))
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
            if (ex is ReMarkException)
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
            if (ex is ReMarkException && ((ReMarkException)ex).ErrorCode.Equals(ErrorConstants.Codes.InvalidSourceType))
                return false;
            if (ex is ReMarkException && ((ReMarkException)ex).ErrorCode.Equals(ErrorConstants.Codes.FileCouldNotBeLoaded))
                return true;

            return true;
        }

        #endregion

        #region Interface implementations

        internal class SingleButtonCallback : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            readonly Action action;
            readonly Action<IDialogInterface> actionWithDialog;

            public SingleButtonCallback(Action action)
            {
                this.action = action;
            }

            public SingleButtonCallback(Action<IDialogInterface> action)
            {
                this.actionWithDialog = action;
            }

            public void OnClick(IDialogInterface dialog, int which)
            {
                action?.Invoke();
                actionWithDialog?.Invoke(dialog);
            }
        }

        internal class SingleChoiceCallback : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            readonly Action<int> action;

            public SingleChoiceCallback(Action<int> action)
            {
                this.action = action;
            }

            public void OnClick(IDialogInterface dialog, int which)
            {
                action?.Invoke(which);
            }
        }

        internal class ListCallback : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            readonly Action<int> action;

            public ListCallback(Action<int> action)
            {
                this.action = action;
            }

            public void OnClick(IDialogInterface dialog, int which)
            {
                action?.Invoke(which);
            }
        }

        //This one only selcts the right one, doesn't dismiss....
        internal class MultiChoiceCallback : Java.Lang.Object, IDialogInterfaceOnMultiChoiceClickListener
        {
            readonly Action<int, bool> action;

            public MultiChoiceCallback(Action<int, bool> action)
            {
                this.action = action;
            }

            //TODO this works differently than the previous version...
            public void OnClick(IDialogInterface dialog, int which, bool isChecked)
            {
                action?.Invoke(which, isChecked);
            }
        }

        #endregion
    }
}
