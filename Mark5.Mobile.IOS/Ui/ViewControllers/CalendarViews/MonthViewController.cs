using System;
using Mark5.Mobile.Common.Model;
using Syncfusion.SfSchedule.iOS;
using UIKit;
using Mark5.Mobile.IOS.Ui.Common;
using System.Collections.Generic;
using Foundation;
using CoreGraphics;
using System.Collections.ObjectModel;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class MonthViewController : AbstractViewController
    {
        SFSchedule schedule;


        List<UIColor> colorCollection;
        List<String> subjectCollection;

        readonly ModuleType moduleType;

        public MonthViewController(ModuleType moduleType)
        {
            this.moduleType = moduleType;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            schedule = new SFSchedule()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            AppointmentMapping mapping = new AppointmentMapping();
            mapping.Subject = "EventName";
            mapping.StartTime = "From";
            mapping.EndTime = "To";
            mapping.AppointmentBackground = "Color";
            schedule.AppointmentMapping = mapping;
            var dayViewSettings = new DayViewSettings();

            DayLabelSettings dayLabel = new DayLabelSettings();

            var monthViewSettings = new MonthViewSettings();
            monthViewSettings.SelectionIndicatorColor = Theme.DarkBlue;
            monthViewSettings.TodayBackgroundColor = Theme.DarkerBlue;
            monthViewSettings.ShowAgendaView = true;
            monthViewSettings.ShowAppointmentsInline = false;
            schedule.MonthViewSettings = monthViewSettings;


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
            schedule.ItemsSource = Meetings;

            schedule.ScheduleView = SFScheduleView.SFScheduleViewMonth;

            //schedule.ViewHeaderTapped += Schedule_ViewHeaderTapped;
            schedule.HeaderTapped += Handle_HeaderTapped;
            schedule.EnableNavigation = true;

            schedule.CellDoubleTapped += Schedule_CellDoubleTapped;

            View.AddSubview(schedule);

            View.BackgroundColor = UIColor.White;

            View.AddConstraints(new NSLayoutConstraint[] {
                schedule.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                schedule.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                schedule.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor),
                schedule.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor)
            });

            //InitializeNavigationBar();
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
                Title = "BACK"
            };

            backBtn.Clicked += (sender, e) =>
            {
                var yearSelection = new YearViewController();
                NavigationController.PushViewController(yearSelection, true);
            };

            NavigationItem.SetLeftBarButtonItem(backBtn, true);
        }


        public class Meeting
        {
            public NSString EventName { get; set; }
            public NSDate From { get; set; }
            public NSDate To { get; set; }
            public UIColor Color { get; set; }
        }



    }
}
