using System;
using System.Globalization;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using AndroidX.Lifecycle;
using Mark5.Mobile.Droid.Ui.Views.CalendarViews.AddEditAppointmentViews;

namespace Mark5.Mobile.Droid.Ui.Views.AutoReplyViews
{
    public abstract class DateView: AutoReplySubView
    {
        protected BasicTextView DateTextView;
        protected BasicTextView TimeTextView;

        public DateView(Context context) : base(context)
        {
            DateTextView = new BasicTextView(context);
            DateTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1)
            {
                Gravity = (GravityFlags)((int)GravityFlags.CenterVertical | (int)GravityFlags.Left)
            };

            DateTextView.Click += DateClicked;

            AddView(DateTextView);

            TimeTextView = new BasicTextView(context)
            {
                Gravity = GravityFlags.Right,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                {
                    Gravity = (GravityFlags)((int)GravityFlags.CenterVertical | (int)GravityFlags.Right)
                },
            };

            TimeTextView.SetPadding(2 * DistanceLarge, 0, 0, 0);
            TimeTextView.TextAlignment = TextAlignment.ViewEnd;

            TimeTextView.Click += TimeClicked;

            AddView(TimeTextView);
        }

        protected void UpdateUI(DateTime date)
        {
            var culture = CultureInfo.InvariantCulture;

            DateTextView.Text = date.ToString("ddd, d MMMM yyyy", culture);
            TimeTextView.Visibility = ViewStates.Visible;
            TimeTextView.Text = date.ToString("hh:mm tt", culture);
                
        }
        protected abstract void DateClicked(object sender, EventArgs e);

        protected abstract void TimeClicked(object sender, EventArgs e);

        public override Task RefreshView() { return Task.CompletedTask; }


        public override Task UpdateAutoReply() { return Task.CompletedTask; }

    }
}

