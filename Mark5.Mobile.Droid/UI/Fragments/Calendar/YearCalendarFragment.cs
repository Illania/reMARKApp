using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Views.Animations;
using Android.Support.V4.Content;
using Com.Syncfusion.Calendar;
using Com.Syncfusion.Calendar.Enums;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class YearCalendarFragment : BaseFragment
    {
        ICalendarActivity iCalendarActivity;

        public static (YearCalendarFragment fragment, string tag) NewInstance()
        {
            var args = new Bundle();

            var fragment = new YearCalendarFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(YearCalendarFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            iCalendarActivity = (CalendarActivity)context;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;
            var configurator = new YearCalendarConfiguration();
            configurator.MonthSelected += Configurator_MonthSelected;
            return configurator.GetContent(Context);
        }

        void Configurator_MonthSelected()
        {
            iCalendarActivity.ShowToolBar();
            Activity.SupportFragmentManager.PopBackStack();
        }
    }

    public class YearCalendarConfiguration : IDisposable
    {
        private FrameLayout mainView;
        private SfCalendar calendar;
        private Context context;
        public Action MonthSelected;

        public View GetContent(Context con)
        {
            context = con;

            MonthViewLabelSetting labelSettings = new MonthViewLabelSetting
            {
                DateLabelSize = 12
            };

            MonthViewSettings monthViewSettings = new MonthViewSettings
            {
                MonthViewLabelSetting = labelSettings,
                PreviousMonthBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                WeekEndBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                WeekDayBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                DayHeight = 0,
                CurrentMonthBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                TodayTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                WeekEndTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                InlineTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                DisabledTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                CurrentMonthTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                WeekDayTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                DateSelectionColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                SelectedDayTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                TodaySelectionTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                TodaySelectionBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                PreviousMonthTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue))
            };

            YearViewSettings yearViewSettings = new YearViewSettings
            {
                MonthHeaderBackground = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                YearHeaderBackground = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                YearLayoutBackground = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                YearHeaderTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.white)),
                MonthHeaderTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.white)),
                DateTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.white)),
            };

            calendar = new SfCalendar(con)
            {
                ShowEventsInline = false,
                HeaderHeight = 100,
                ViewMode = ViewMode.YearView,
                MonthViewSettings = monthViewSettings,
                YearViewSettings = yearViewSettings
            };

            calendar.MonthChanged += Calendar_MonthChanged;

            mainView = new FrameLayout(con);

            mainView.SetPadding(15, 15, 15, 15);
            mainView.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)));
            mainView.AddView(calendar);

            return mainView;
        }

        void Calendar_MonthChanged(object sender, MonthChangedEventArgs e)
        {
            mainView.RemoveAllViews();
            Animation animation = AnimationUtils.LoadAnimation(context, Resource.Animation.fade_out);
            mainView.StartAnimation(animation);

            if (MonthSelected != null)
            {
                MonthSelected.Invoke();
            }
        }

        public void Dispose()
        {
            //TODO : implement
        }
    }
}
