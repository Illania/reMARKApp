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
using CoreAnimation;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class MonthViewController : AbstractViewController
    {
        ReMarkMonthView schedule;

        public bool transitioning;

        readonly ModuleType moduleType;

        public MonthViewController(ModuleType moduleType)
        {
            this.moduleType = moduleType;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBarHidden = false;
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

            View.BackgroundColor = UIColor.White;

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

        void InitializeNavigationBar()
        {
            var backBtn = new UIBarButtonItem
            {
                Title = "Year"
            };

            backBtn.Clicked += (sender, e) =>
            {
                if (transitioning)
                    return;

                YearViewController yearSelection = new YearViewController();
                CATransition transition = new CATransition();
                transition.Duration = 0.35;
                transition.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
                transition.Type = CAAnimation.TransitionPush;
                transition.Subtype = CAAnimation.TransitionFromLeft;
                transition.Delegate = new AnimationDelegate(this);
                transitioning = true;

                NavigationController.View.Layer.AddAnimation(transition, null);
                NavigationController.PushViewController(yearSelection, false);
            };

            NavigationItem.SetLeftBarButtonItem(backBtn, true);

            var addBtn = new UIBarButtonItem
            {
                Title = "",
                Image = UIImage.FromBundle("Create")
            };

            addBtn.Clicked += (sender, e) =>
            {
                var newAppointmentVC = new CreateAppointmentViewController();
                NavigationController.PushViewController(newAppointmentVC, true);
            };

            NavigationItem.SetRightBarButtonItem(addBtn, true);
        }

        class AnimationDelegate : CAAnimationDelegate
        {
            MonthViewController ctrl;

            public AnimationDelegate(MonthViewController ctrl)
            {
                this.ctrl = ctrl;
            }

            public override void AnimationStopped(CAAnimation anim, bool finished)
            {
                ctrl.transitioning = false;
            }
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
            TextColor = UIColor.White
        };

        readonly MonthViewSettings monthViewSettings = new MonthViewSettings()
        {
            SelectionIndicatorColor = Theme.LightBlue,
            TodayBackgroundColor = Theme.LightBlue,
            SelectionTextColor = Theme.White,
            ShowAgendaView = true,
            ShowAppointmentsInline = false,
            AgendaViewStyle = new AgendaViewStyle
            {
                SubjectTextColor = Theme.DarkerBlue,
                SubjectTextStyle = Theme.DefaultLightFont,
                TimeTextStyle = Theme.CalendarTimeLightFont,
                TimeTextColor = Theme.DarkGray,
                DateTextColor = Theme.DarkGray,
                DateTextStyle = Theme.DefaultLightFont,
                HeaderHeight = 50f
            }
        };

        public ReMarkMonthView()
        {
            ScheduleView = SFScheduleView.SFScheduleViewMonth;

            AppointmentMapping = CalendarUtils.GetAppointmentMapping();

            ItemsSource = CalendarUtils.GetMeetings();

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
    }
}
