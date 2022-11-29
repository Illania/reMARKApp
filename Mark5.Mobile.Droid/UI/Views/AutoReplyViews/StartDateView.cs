using System;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.Lifecycle;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AutoReplyViews
{
    public class StartDateView: DateView
    {
        public StartDateView(Context context) : base(context) { }

        protected override async void DateClicked(object sender, EventArgs e)
        {
            var start = AutoReplyRule.ActiveFrom;
            var startDate = new DateTime(start.Year, start.Month, start.Day);

            var newTimestamp = await Dialogs.ShowDatePicker(Context, startDate.ConvertDateTimeToTimestampMilliseconds());
            var newDate = DateTime.SpecifyKind(newTimestamp.ConvertTimestampMillisecondsToDateTime(), DateTimeKind.Local);
            AutoReplyRule.ActiveFrom = newDate + new TimeSpan(start.Hour, start.Minute, start.Second);

            await RefreshView();
        }

        protected override async void TimeClicked(object sender, EventArgs e)
        {
            var start = AutoReplyRule.ActiveFrom;

            TimeSpan result = await Dialogs.ShowTimePicker(Context, start.Hour, start.Minute);
            var newDate = new DateTime(start.Year, start.Month, start.Day, result.Hours, result.Minutes, 0, DateTimeKind.Local);
            AutoReplyRule.ActiveFrom = newDate;
            await RefreshView();
        }

        public override Task RefreshView()
        {
            if (AutoReplyRule != null)
                UpdateUI(AutoReplyRule.ActiveFrom);
            return Task.CompletedTask;
        }

        public override Task UpdateAutoReply() {
            return Task.CompletedTask;
        }

    }
}

