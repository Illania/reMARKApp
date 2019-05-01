using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class MonthViewController : AbstractViewController, ICalendarView
    {
        ReMarkMonthSchedule monthSchedule;
        UIBarButtonItem backButtonItem;
        UIBarButtonItem calendarsButtonItem;
        UIBarButtonItem createAppointmentButtonItem;

        private bool transitioning;
        CalendarPresenter presenter;

        ObservableCollection<Meeting> Items = new ObservableCollection<Meeting>();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            monthSchedule = new ReMarkMonthSchedule
            {
                AppointmentMapping = GetAppointmentMapping(),
                ItemsSource = Items  //TODO If we're lucky, we can use the same items, in all the views, so we don't need to do strange stuff
            };

            View.BackgroundColor = UIColor.White;
            View.AddSubview(monthSchedule);
            View.AddConstraints(new NSLayoutConstraint[] {
                monthSchedule.TopAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TopAnchor : View.TopAnchor),
                monthSchedule.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : View.BottomAnchor),
                monthSchedule.RightAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.RightAnchor : View.RightAnchor),
                monthSchedule.LeftAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.LeftAnchor : View.LeftAnchor)
            });

            InitializeNavigationBar();

            MoveToDate(NSDate.Now);

            presenter = new CalendarPresenter();
            presenter.AttachView(this);
            presenter.Start();
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
            if (monthSchedule != null)
            {
                monthSchedule.HeaderTapped += Handle_HeaderTapped;
                monthSchedule.CellDoubleTapped += Schedule_CellDoubleTapped;
                monthSchedule.MonthInlineAppointmentTapped += Schedule_MonthInlineAppointmentTapped;
                monthSchedule.VisibleDatesChanged += MonthSchedule_VisibleDatesChanged;
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
            if (monthSchedule != null)
            {
                monthSchedule.HeaderTapped -= Handle_HeaderTapped;
                monthSchedule.CellDoubleTapped -= Schedule_CellDoubleTapped;
                monthSchedule.MonthInlineAppointmentTapped -= Schedule_MonthInlineAppointmentTapped;
                monthSchedule.VisibleDatesChanged -= MonthSchedule_VisibleDatesChanged;
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

        void MoveToDate(NSDate date)
        {
            if (monthSchedule != null)
            {
                monthSchedule.MoveToDate(date);
                monthSchedule.SelectedDate = date;
            }
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

        //void ICalendarView.OpenAppointment() //TODO
        //{
        //    NavigationController.PushViewController(new AppointmentViewController(), true);
        //}

        void ICalendarView.SetCalendars(List<CalendarViewModel> calendars)
        {
            //TODO
        }

        void ICalendarView.UpdateAppointments(IEnumerable<SimpleCalendarAppointmentViewModel> caViewModels)
        {
            foreach (var caViewModel in caViewModels)
            {
                Items.Add(Convert(caViewModel));
            }
        }

        Meeting Convert(SimpleCalendarAppointmentViewModel cavm)
        {
            return new Meeting
            {
                Subject = new NSString(cavm.Subject),
                Start = (NSDate)DateTime.SpecifyKind(cavm.Start, DateTimeKind.Local),
                End = (NSDate)DateTime.SpecifyKind(cavm.End, DateTimeKind.Local),
                Color = UI.UIColorFromHexString(cavm.HexColor),
            };
        }

        Action loadingDialogDismissal;

        void ICalendarView.ShowLoading()
        {
            loadingDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_appointments___"));
        }

        void ICalendarView.StopLoading()
        {
            loadingDialogDismissal?.Invoke();
        }

        async Task ICalendarView.ShowError(Exception ex)
        {
            await Dialogs.ShowErrorAlertAsync(this, ex);
        }

        #endregion

        #region Event handlers

        void Schedule_MonthInlineAppointmentTapped(object sender, MonthInlineAppointmentTappedEventArgs e)
        {
            //presenter.AppointmentClicked(); //TODO
        }

        async void MonthSchedule_VisibleDatesChanged(object sender, VisibleDatesChangedEventArgs e)
        {
            var startDate = monthSchedule.VisibleDates.GetItem<NSDate>(0);
            var endDate = monthSchedule.VisibleDates.GetItem<NSDate>(monthSchedule.VisibleDates.Count - 1);

            var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year, startDate);
            var start = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, 0, 0, 0, DateTimeKind.Local);

            selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year, endDate);
            var end = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, 23, 59, 59, DateTimeKind.Local).AddDays(1);

            await presenter.LoadAppointments(start, end);
        }

        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            NSDateComponents components = new NSDateComponents
            {
                Hour = 8
            };

            NSDate date = NSCalendar.CurrentCalendar.DateByAddingComponents(components, e.Date, NSCalendarOptions.None);

            NavigationController.PushViewController(new DayViewController(date), true);
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

        public class Meeting
        {
            public int Id { get; set; }
            public NSString Subject { get; set; }
            public NSDate Start { get; set; }
            public NSDate End { get; set; }
            public UIColor Color { get; set; }
        }

        public static AppointmentMapping GetAppointmentMapping()
        {
            AppointmentMapping mapping = new AppointmentMapping
            {
                Subject = "Subject",
                StartTime = "Start",
                EndTime = "End",
                AppointmentBackground = "Color",
            };
            return mapping;
        }
    }

    class ReMarkMonthSchedule : SFSchedule
    {
        public ReMarkMonthSchedule()
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
