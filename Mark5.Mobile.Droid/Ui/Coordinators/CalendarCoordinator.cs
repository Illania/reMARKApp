using System;
using Android.Content;
using Mark5.Mobile.Droid.Ui.Common;
using Android.Support.V4.App;
using Mark5.Mobile.Droid.Ui.Fragments.Calendar;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Coordinators
{
    public class CalendarCoordinator : Activities.ICalendarActivity
    {
        Context context;
        FragmentManager fragmentManager;
        MonthCalendarFragment monthFragment;
        YearCalendarFragment yearFragment;
        WeekCalendarFragment weekFragment;
        CalendarListFragment calendarListFragment;
        CreateAppointmentFragment createAppointmentFragment;

        public CalendarCoordinator(FragmentManager fm)
        {
            fragmentManager = fm;
        }

        public (BaseFragment, string) GetMainFragment()
        {
            string tag;
            (monthFragment, tag) = MonthCalendarFragment.NewInstance();
            return (monthFragment, tag);
        }

        public bool CellDoubleTapped()
        {
            var (fragment, tag) = WeekCalendarFragment.NewInstance();

            fragmentManager.BeginTransaction()
               .SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out)
               .Replace(Resource.Id.fragment_container, fragment, tag)
               .AddToBackStack(tag)
               .Commit();

            return true;
        }

        public void OnClick(View v)
        {
            var (fragment, tag) = YearCalendarFragment.NewInstance();
            fragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
            //SupportActionBar.Hide();
        }

        public void ShowToolBar()
        {
            //SupportActionBar.Show();
        }

        public void ShowCalendarSelection()
        {
            var (fragment, tag) = CalendarListFragment.NewInstance();
            fragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
            //SupportActionBar.Hide();
        }

        public void ShowCreateAppointment()
        {
            var (fragment, tag) = CreateAppointmentFragment.NewInstance();
            fragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
            //SupportActionBar.Hide();
        }
    }
}
