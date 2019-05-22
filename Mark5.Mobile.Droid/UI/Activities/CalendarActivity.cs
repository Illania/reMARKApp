using System;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Content.PM;
using Android.Support.V4.Content;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments.Calendar;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class CalendarActivity : BaseAppCompatActivity, ICalendarActivity, View.IOnClickListener
    {
        private Android.Support.V7.Widget.Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.base_layout);
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            SetupLeftToolbarButton();

            var (monthCalendarFragment, tag) = MonthCalendarFragment.NewInstance();
            OverridePendingTransition(Resource.Animation.fade_in, Resource.Animation.fade_out);

            SupportFragmentManager.BeginTransaction()
                .SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out)
                .Replace(Resource.Id.fragment_container, monthCalendarFragment, tag)
                .Commit();
        }

        public override void OnBackPressed()
        {
            if (SupportFragmentManager.BackStackEntryCount > 0)
            {
                SupportActionBar.Show();
                SupportFragmentManager.PopBackStack();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public bool CellDoubleTapped()
        {
            var (fragment, tag) = WeekCalendarFragment.NewInstance();

            SupportFragmentManager.BeginTransaction()
               .SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out)
               .Replace(Resource.Id.fragment_container, fragment, tag)
               .AddToBackStack(tag)
               .Commit();

            return true;
        }

        public void OnClick(View v)
        {
            var (fragment, tag) = YearCalendarFragment.NewInstance();
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
            SupportActionBar.Hide();
        }

        public void ShowToolBar()
        {
            SupportActionBar.Show();
        }

        public void ShowCalendarSelection()
        {
            var (fragment, tag) = CalendarListFragment.NewInstance();
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
            SupportActionBar.Hide();
        }

        public void ShowCreateAppointment()
        {
            var (fragment, tag) = CreateAppointmentFragment.NewInstance();
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
            SupportActionBar.Hide();
        }

        private void SetupLeftToolbarButton()
        {
            TextView yearTextView = new TextView(this)
            {
                Gravity = GravityFlags.Left,
                TextSize = 18,
                Text = "Year"
            };

            yearTextView.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkerblue)));

            yearTextView.SetTextColor(Color.White);
            yearTextView.LayoutParameters = new Toolbar.LayoutParams(Toolbar.LayoutParams.WrapContent, Toolbar.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.Left
            };
            yearTextView.Text = "Year";
            yearTextView.TextSize = 18;
            yearTextView.Clickable = true;
            yearTextView.SetOnClickListener(this);

            toolbar.AddView(yearTextView);
        }
    }

    public interface ICalendarActivity
    {
        bool CellDoubleTapped();

        void ShowToolBar();

        void ShowCalendarSelection();

        void ShowCreateAppointment();
    }
}
