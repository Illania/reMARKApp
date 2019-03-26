using System;
using Mark5.Mobile.Common.Model;
using Syncfusion.SfSchedule.iOS;
using UIKit;
using Mark5.Mobile.IOS.Ui.Common;
using System.Collections.Generic;
using Foundation;
using CoreGraphics;
using System.Collections.ObjectModel;
using Syncfusion.SfCalendar.iOS;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class MonthViewController : AbstractViewController
    {
        ReMarkMonthView schedule;

        readonly ModuleType moduleType;

        public MonthViewController(ModuleType moduleType)
        {
            this.moduleType = moduleType;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            schedule = new ReMarkMonthView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            schedule.HeaderTapped += Handle_HeaderTapped;
            schedule.EnableNavigation = true;
            schedule.CellDoubleTapped += Schedule_CellDoubleTapped;

            View.AddSubview(schedule);

            View.AddConstraints(new NSLayoutConstraint[] {
                schedule.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                schedule.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                schedule.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor),
                schedule.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor)
            });

            InitializeNavigationBar();
        }

        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            NavigationController.PushViewController(new DayViewController(), true);
        }

        void Handle_HeaderTapped(object sender, HeaderTappedEventArgs e)
        {
            NavigationController.PushViewController(new YearViewController(), true);
        }

        void Schedule_ViewHeaderTapped(object sender, ViewHeaderTappedEventArgs e)
        {
            NavigationController.PushViewController(new YearViewController(), true);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        void InitializeNavigationBar()
        {
            var backBtn = new UIBarButtonItem
            {
                Title = "Year"
            };

            backBtn.Clicked += (sender, e) =>
            {
                var yearSelection = new YearViewController();
                NavigationController.PushViewController(yearSelection, true);
            };

            NavigationItem.SetLeftBarButtonItem(backBtn, true);

            var addBtn = new UIBarButtonItem
            {
                Title = "",
                Image = UIImage.FromBundle("Create")
            };

            addBtn.Clicked += (sender, e) =>
            {
                var yearSelection = new YearViewController();
                NavigationController.PushViewController(yearSelection, true);
            };

            NavigationItem.SetRightBarButtonItem(addBtn, true);
        }
    }

    public class ReMarkMonthView : SFSchedule
    {
        readonly SFViewHeaderStyle dayHeaderStyle = new SFViewHeaderStyle
        {
            BackgroundColor = Theme.DarkerBlue,
            DayTextColor = UIColor.White,
            DayTextStyle = Theme.DefaultLightFont
        };

        readonly HeaderStyle calendarHeaderStyle = new HeaderStyle
        {
            BackgroundColor = Theme.DarkerBlue,
            TextStyle = Theme.DefaultLightFont,
            TextColor = UIColor.White,
            TextPosition = UITextAlignment.Center
        };

        readonly MonthViewSettings monthViewSettings = new MonthViewSettings
        {
            SelectionIndicatorColor = Theme.LightBlue,
            TodayBackgroundColor = Theme.LightBlue,
            SelectionTextColor = Theme.White,
            ShowAgendaView = true,
            ShowAppointmentsInline = false
        };

        public ReMarkMonthView()
        {
            ScheduleView = SFScheduleView.SFScheduleViewMonth;

            AppointmentMapping = GetAppointmentMapping();
            AddMeetings();

            MonthCellLoaded += ReMark_MonthCellLoaded;
            MonthViewSettings = monthViewSettings;


            SelectionStyle = new SFSelectionStyle
            {
                BackgroundColor = Theme.DarkBlue,
                BorderColor = Theme.LightBlue
            };

            HeaderStyle = calendarHeaderStyle;
            DayHeaderStyle = dayHeaderStyle;
        }

        void ReMark_MonthCellLoaded(object sender, MonthCellLoadedEventArgs e)
        {
            SFCellStyle style = new SFCellStyle();

            if (e.IsToday)
            {
                style = new SFCellStyle
                {
                    TextStyle = Theme.DefaultLightFont,
                    BackgroundColor = Theme.DarkerBlue,
                    TextColor = Theme.DarkerBlue
                };
            }
            else
            {
                style = new SFCellStyle
                {
                    TextStyle = Theme.DefaultLightFont,
                    BackgroundColor = Theme.DarkerBlue,
                    TextColor = UIColor.White
                };
            }

            e.CellStyle = style;
        }

        class Meeting
        {
            public NSString EventName { get; set; }
            public NSDate From { get; set; }
            public NSDate To { get; set; }
            public UIColor Color { get; set; }
        }

        private AppointmentMapping GetAppointmentMapping()
        {
            AppointmentMapping mapping = new AppointmentMapping();
            mapping.Subject = "EventName";
            mapping.StartTime = "From";
            mapping.EndTime = "To";
            mapping.AppointmentBackground = "Color";
            return mapping;
        }

        void AddMeetings()
        {
            NSCalendar calendar = new NSCalendar(NSCalendarType.Gregorian);
            NSDate today = new NSDate();
            NSDateComponents startDateComponents = calendar.Components(NSCalendarUnit.Year |
                                                                       NSCalendarUnit.Month |
                                                                       NSCalendarUnit.Day, today);
            startDateComponents.Hour = 09;
            startDateComponents.Minute = 0;
            startDateComponents.Second = 0;
            NSDateComponents endDateComponents = calendar.Components(NSCalendarUnit.Year |
                                                                     NSCalendarUnit.Month |
                                                                     NSCalendarUnit.Day, today);

            endDateComponents.Hour = 10;
            endDateComponents.Minute = 0;
            endDateComponents.Second = 0;
            NSDate startDate = calendar.DateFromComponents(startDateComponents);
            NSDate endDate = calendar.DateFromComponents(endDateComponents);

            // Creating instance for custom appointment class
            Meeting meeting = new Meeting();
            // Setting start time of an event
            meeting.From = startDate;
            // Setting end time of an event
            meeting.To = endDate;
            // Setting start time for an event
            meeting.EventName = (NSString)"Anniversary";
            // Setting color for an event
            meeting.Color = UIColor.Green;
            // Creating instance for collection of custom appointments
            var Meetings = new ObservableCollection<Meeting>();
            // Adding a custom appointment in CustomAppointmentCollection
            Meetings.Add(meeting);

            Meeting meeting2 = new Meeting();
            // Setting start time of an event
            meeting2.From = startDate.AddSeconds(3600);
            // Setting end time of an event
            meeting2.To = endDate.AddSeconds(3600);
            // Setting start time for an event
            meeting2.EventName = (NSString)"Meetings with Tester";
            // Setting color for an event
            meeting2.Color = UIColor.Red;
            // Creating instance for collection of custom appointments

            // Adding a custom appointment in CustomAppointmentCollection
            Meetings.Add(meeting2);

            // Adding custom appointments in DataSource of SfSchedule
            ItemsSource = Meetings;
        }
    }
}
