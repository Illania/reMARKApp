using System;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.Lifecycle;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AutoReplyViews
{
    public class  EndDateView: DateView
    {
        public EndDateView(Context context) : base(context) { }

        protected override async void DateClicked(object sender, EventArgs e)
        {
            var end = AutoReplyRule.ActiveFrom;
            var endDate = new DateTime(end.Year, end.Month, end.Day);

            var newTimestamp = await Dialogs.ShowDatePicker(Context, endDate.ConvertDateTimeToTimestampMilliseconds());
            var newDate = DateTime.SpecifyKind(newTimestamp.ConvertTimestampMillisecondsToDateTime(), DateTimeKind.Local);
            AutoReplyRule.ActiveTo = newDate + new TimeSpan(end.Hour,end.Minute, end.Second);

            await RefreshView();
        }

        protected override async void TimeClicked(object sender, EventArgs e)
        {
            var start = AutoReplyRule.ActiveTo;

            TimeSpan result = await Dialogs.ShowTimePicker(Context, start.Hour, start.Minute);
            var newDate = new DateTime(start.Year, start.Month, start.Day, result.Hours, result.Minutes, 0, DateTimeKind.Local);
            AutoReplyRule.ActiveTo = newDate;
            await RefreshView();
        }

        public override Task RefreshView()
        {
            if (AutoReplyRule != null)
                UpdateUI(AutoReplyRule.ActiveTo);
            return Task.CompletedTask;
        }

        public override Task UpdateAutoReply()
        {
            return Task.CompletedTask;
        }
    }
}

