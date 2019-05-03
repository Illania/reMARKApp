using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
using Foundation;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Syncfusion.SfSchedule.iOS;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class MonthViewController : CalendarViewController
    {
        UIBarButtonItem backButtonItem;
        UIBarButtonItem calendarsButtonItem;
        UIBarButtonItem createAppointmentButtonItem;

        bool transitioning;
        CalendarPresenter presenter;
        Action loadingDialogDismissal;

        public override void LoadView()
        {
            base.LoadView();

            schedule = new MonthSchedule();
            View.BackgroundColor = UIColor.White;
            View.AddSubview(schedule);
            View.AddConstraints(new NSLayoutConstraint[] {
                schedule.TopAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TopAnchor : View.TopAnchor),
                schedule.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : View.BottomAnchor),
                schedule.RightAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.RightAnchor : View.RightAnchor),
                schedule.LeftAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.LeftAnchor : View.LeftAnchor)
            });

            InitializeNavigationBar();

            MoveToDate(NSDate.Now);

            presenter = new CalendarPresenter();
            presenter.AttachView(this);
            presenter.Start();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            presenter.ViewReady();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBarHidden = false;

            InitializeHandlers();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();
        }

        void InitializeHandlers()
        {
            if (schedule != null)
            {
                schedule.HeaderTapped += Handle_HeaderTapped;
                schedule.CellDoubleTapped += Schedule_CellDoubleTapped;
                schedule.MonthInlineAppointmentTapped += Schedule_MonthInlineAppointmentTapped;
                schedule.VisibleDatesChanged += MonthSchedule_VisibleDatesChanged;
            }

            if (backButtonItem != null)
                backButtonItem.Clicked += BackButtonItem_Clicked;

            if (createAppointmentButtonItem != null)
                createAppointmentButtonItem.Clicked += CreateAppointmentButtonItem_Clicked;

            if (calendarsButtonItem != null)
                calendarsButtonItem.Clicked += CalendarsButtonItem_Clicked;
        }

        void DeInitializeHandlers()
        {
            if (schedule != null)
            {
                schedule.HeaderTapped -= Handle_HeaderTapped;
                schedule.CellDoubleTapped -= Schedule_CellDoubleTapped;
                schedule.MonthInlineAppointmentTapped -= Schedule_MonthInlineAppointmentTapped;
                schedule.VisibleDatesChanged -= MonthSchedule_VisibleDatesChanged;
            }

            if (backButtonItem != null)
                backButtonItem.Clicked -= BackButtonItem_Clicked;

            if (createAppointmentButtonItem != null)
                createAppointmentButtonItem.Clicked -= CreateAppointmentButtonItem_Clicked;

            if (calendarsButtonItem != null)
                calendarsButtonItem.Clicked -= CalendarsButtonItem_Clicked;
        }

        void InitializeNavigationBar()
        {
            backButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("year"),
            };

            NavigationItem.SetLeftBarButtonItem(backButtonItem, true);

            createAppointmentButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("create"),
                Image = UIImage.FromBundle("Create")
            };

            calendarsButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("calendars"),
            };

            NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { createAppointmentButtonItem, calendarsButtonItem }, true);
        }

        class AnimationDelegate : CAAnimationDelegate
        {
            readonly MonthViewController ctrl;

            public AnimationDelegate(MonthViewController ctrl)
            {
                this.ctrl = ctrl;
            }

            public override void AnimationStopped(CAAnimation anim, bool finished)
            {
                ctrl.transitioning = false;
            }
        }

        #region ICalendar implementation

        public override void ShowAppointment(int appointmentId) //TODO maybe I'll need to do some modification for recurring appointments
        {
            NavigationController.PushViewController(new AppointmentViewController(appointmentId), true);
        }

        public override void ShowLoading()
        {
            loadingDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_appointments___"));
        }

        public override void StopLoading()
        {
            loadingDialogDismissal?.Invoke();
        }

        public override async Task ShowError(Exception ex)
        {
            await Dialogs.ShowErrorAlertAsync(this, ex);
        }

        #endregion

        #region Event handlers

        void Schedule_MonthInlineAppointmentTapped(object sender, MonthInlineAppointmentTappedEventArgs e)
        {
            //presenter.AppointmentClicked(); //TODO
        }

        void MonthSchedule_VisibleDatesChanged(object sender, VisibleDatesChangedEventArgs e)
        {
            var startDate = schedule.VisibleDates.GetItem<NSDate>(0);
            var endDate = schedule.VisibleDates.GetItem<NSDate>(schedule.VisibleDates.Count - 1);

            var start = ((DateTime)startDate).ToLocalTime();
            var end = ((DateTime)endDate).ToLocalTime();

            presenter.LoadAppointments(start, end);
        }

        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            NSDateComponents components = new NSDateComponents
            {
                Hour = 8
            };

            NSDate date = NSCalendar.CurrentCalendar.DateByAddingComponents(components, e.Date, NSCalendarOptions.None);

            NavigationController.PushViewController(new DayWeekViewController(date), true);
        }

        void Handle_HeaderTapped(object sender, HeaderTappedEventArgs e)
        {
            NavigationController.PushViewController(new YearViewController(MoveToDate), true);
        }

        void Schedule_ViewHeaderTapped(object sender, ViewHeaderTappedEventArgs e)
        {
            NavigationController.PushViewController(new YearViewController(MoveToDate), true);
        }

        void BackButtonItem_Clicked(object sender, EventArgs e)
        {
            if (transitioning)
                return;

            YearViewController yearSelection = new YearViewController(MoveToDate);
            CATransition transition = new CATransition
            {
                Duration = 0.35,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear),
                Type = CAAnimation.TransitionPush,
                Subtype = CAAnimation.TransitionFromLeft,
                Delegate = new AnimationDelegate(this)
            };
            transitioning = true;

            NavigationController.View.Layer.AddAnimation(transition, null);
            NavigationController.PushViewController(yearSelection, false);
        }

        void CreateAppointmentButtonItem_Clicked(object sender, EventArgs e)
        {
            NavigationController.PushViewController(new CreateAppointmentViewController(), true);
        }

        void CalendarsButtonItem_Clicked(object sender, EventArgs e)
        {
            //TODO to complete
        }

        #endregion

    }

    class MonthSchedule : SFSchedule
    {
        public MonthSchedule()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;

            ScheduleView = SFScheduleView.SFScheduleViewMonth;
            EnableNavigation = true;

            MonthViewSettings = new MonthViewSettings()
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

            SelectionStyle = new SFSelectionStyle
            {
                BackgroundColor = Theme.DarkBlue,
                BorderColor = Theme.LightBlue
            };

            HeaderStyle = new HeaderStyle
            {
                BackgroundColor = Theme.DarkerBlue,
                TextStyle = Theme.DefaultLightFont,
                TextColor = UIColor.White
            };

            DayHeaderStyle = new SFViewHeaderStyle
            {
                BackgroundColor = Theme.DarkerBlue,
                DayTextColor = UIColor.White,
                DayTextStyle = Theme.DefaultLightFont
            };

            MonthCellLoaded += ReMark_MonthCellLoaded;
        }

        void ReMark_MonthCellLoaded(object sender, MonthCellLoadedEventArgs e)
        {
            SFCellStyle style;

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
