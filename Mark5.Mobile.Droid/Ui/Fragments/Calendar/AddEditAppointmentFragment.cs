using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Animation;
using Android.Graphics.Drawables;
using Android.Support.V4.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views.InputMethods;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using System.Linq;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class AddEditAppointmentFragment : BaseFragment, IAddEditAppointmentView, IMenuItemOnMenuItemClickListener
    {
        const string CreationModeFlagBundleKey = "CreationModeFlag_ab9071da-34f6-45fc-9a03-a0b348814dcd";
        const string AppointmentIdBundleKey = "AppointmentId_d09e0cb6-e224-4327-8d09-43ce921f53c6";
        const string CalendarIdBundleKey = "CalendarId_d09e0cb6-e224-4327-8d09-43ce921f53c6";
        const string StartDateIdBundleKey = "StartDate_3b43a244-6a24-496f-9d33-1eeb1c277005";

        AddEditAppointmentPresenter presenter;

        ContactCreationModeFlag creationModeFlag;
        bool loaded;
        int calendarId;
        int appointmentId;
        DateTime startDate;

        LinearLayoutCompat linearLayout;
        NestedScrollView scrollView;
        ProgressBar progressBar;

        StartDateView startDateView;
        EndDateView endDateView;
        CalendarView calendarView;
        ParticipantsView participantsView;

        AddEditAppointmentViewModel viewModel;

        List<View> subviews = new List<View>();

        public static (AddEditAppointmentFragment fragment, string tag) NewInstance(DateTime startDate = default)
        {
            Bundle args = new Bundle();

            var fragment = new AddEditAppointmentFragment
            {
                Arguments = args
            };

            args.PutInt(CreationModeFlagBundleKey, (int)ContactCreationModeFlag.New);
            args.PutLong(StartDateIdBundleKey, startDate.Ticks);

            var tag = $"{nameof(AddEditAppointmentFragment)}";

            return (fragment, tag);
        }

        public static (AddEditAppointmentFragment fragment, string tag) NewInstance(int calendarId, int appointmentId)
        {
            Bundle args = new Bundle();

            var fragment = new AddEditAppointmentFragment
            {
                Arguments = args
            };

            args.PutInt(CreationModeFlagBundleKey, (int)ContactCreationModeFlag.Edit);
            args.PutInt(AppointmentIdBundleKey, appointmentId);
            args.PutInt(CalendarIdBundleKey, calendarId);

            var tag = $"{nameof(AddEditAppointmentFragment)}";

            return (fragment, tag);
        }

        #region Fragment Lifecycle
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(CreationModeFlagBundleKey))
                creationModeFlag = (ContactCreationModeFlag)Arguments.GetInt(CreationModeFlagBundleKey);

            if (Arguments.ContainsKey(AppointmentIdBundleKey))
                appointmentId = Arguments.GetInt(AppointmentIdBundleKey);

            if (Arguments.ContainsKey(CalendarIdBundleKey))
                calendarId = Arguments.GetInt(CalendarIdBundleKey);

            if (Arguments.ContainsKey(StartDateIdBundleKey))
                startDate = new DateTime(Arguments.GetLong(StartDateIdBundleKey));  //TODO check if the datetime kind is there

            presenter = new AddEditAppointmentPresenter();
            presenter.AttachView(this);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditAppointmentFragment)}, " + $"mode={creationModeFlag}]...");

            HasOptionsMenu = true;

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            linearLayout.FocusableInTouchMode = true;

            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress);

            subviews.Clear();

            linearLayout.SetPadding(linearLayout.PaddingLeft, linearLayout.PaddingTop, linearLayout.PaddingRight, linearLayout.PaddingBottom);

            PrepareViews();

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            CommonConfig.Logger.Info($"Created {nameof(AddEditAppointmentFragment)} mode={creationModeFlag}]...");
            if (creationModeFlag == ContactCreationModeFlag.New)
                ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.create_appointment);
            else
                ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.edit_appointment);
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (!loaded)
            {
                if (creationModeFlag == ContactCreationModeFlag.New)
                    await presenter.LoadEmptyAppointment(startDate);
                else
                    await presenter.LoadAppointment(calendarId, appointmentId);
                loaded = true;

                StopLoading();
            }
            else
            {
                StopLoading();
                if (viewModel != null)
                    ShowAppointment(viewModel);
            }
        }

        #endregion

        #region IMenu

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var addAppointment = menu.Add(Menu.None, MenuItemActions.SaveAppointment, MenuItemActions.SaveAppointment, Resource.String.save);  //TODO change
            addAppointment.SetIcon(Resource.Drawable.action_save);
            addAppointment.SetShowAsAction(ShowAsAction.Always);
            addAppointment.SetOnMenuItemClickListener(this);
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.SaveAppointment)
                return false;

            return true;
        }

        static class MenuItemActions
        {
            public const int SaveAppointment = 10;
        }

        #endregion

        #region Helpers

        void PrepareViews()
        {
            subviews.Add(new NameView(Context));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(calendarView = new CalendarView(Context, CalendarClicked));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(new AllDayToggleView(Context, AllDayToggleChanged));
            subviews.Add(startDateView = new StartDateView(Context));
            subviews.Add(endDateView = new EndDateView(Context));
            subviews.Add(new ReocurrenceView(Context, ReocurrenceClicked));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(participantsView = new ParticipantsView(Context, ParticipantsClicked));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(new LocationView(Context));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(new ReminderView(Context));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(new MessageView(Context));
            subviews.Add(new SeparatorSubview(Context));
            subviews.ForEach(linearLayout.AddView);
        }

        void AllDayToggleChanged()
        {
            startDateView.RefreshView();
            endDateView.RefreshView();
        }

        void CalendarClicked()
        {
            _ = CalendarClickedAsync();
        }

        void ReocurrenceClicked()
        {
            var (rf, tag) = ReoccurrenceFragment.NewInstance(viewModel.RecurrenceInfo);
            ((AppCompatActivity)Activity).SupportFragmentManager.BeginTransaction()
                          .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                          .Replace(Resource.Id.fragment_container, rf, tag)
                          .AddToBackStack(tag).Commit();
        }

        async void ParticipantsClicked()
        {
            var (aepf, tag) = AddEditParticipantsFragment.NewInstance(viewModel.Participants);

            ((AppCompatActivity)Activity).SupportFragmentManager.BeginTransaction()
                           .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                           .Replace(Resource.Id.fragment_container, aepf, tag)
                           .AddToBackStack(tag).Commit();

            var result = await aepf.TaskResult;
            if (result != null && viewModel != null)
            {
                viewModel.Participants = result;
                participantsView?.RefreshView();
            }
        }

        async Task CalendarClickedAsync()
        {
            List<Mobile.Common.Model.Calendar> calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;
            Dictionary<int, bool> calendarsSelectedState = new Dictionary<int, bool>();
            Dictionary<int, string> calendarsColor = new Dictionary<int, string>();

            calendarsList.ForEach(c => calendarsColor.Add(c.Id, c.ColorHex));

            var calendarsWithSelection = new Dictionary<CalendarViewModel, bool>();
            foreach (var cal in calendarsList)
                calendarsWithSelection.Add(CalendarViewModel.ConvertToViewModel(cal), viewModel.Calendar != null && viewModel.Calendar.Id == cal.Id);

            var (clf, tag) = CalendarListFragment.NewInstance(calendarsWithSelection);

            ((AppCompatActivity)Activity).SupportFragmentManager.BeginTransaction()
                           .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                           .Replace(Resource.Id.fragment_container, clf, tag)
                           .AddToBackStack(tag)
                           .Commit();

            var result = await clf.Result;
            if (result != null)
            {
                ((AppCompatActivity)Activity).SupportFragmentManager.PopBackStack();
                viewModel.Calendar = result;
                calendarView.RefreshView();
            }
        }

        void CalendarSelected()
        {

        }

        #endregion

        #region IAddEditAppointmentView implementation

        public void CloseView()
        {
            ((AppCompatActivity)Activity).SupportFragmentManager.PopBackStack();
        }

        public Task ShowAddingEditingError(Exception ex)
        {
            return Dialogs.ShowErrorDialogAsync(Context, ex);
        }

        public void ShowAppointment(AddEditAppointmentViewModel viewModel)
        {
            if (viewModel != null)
            {
                this.viewModel = viewModel;

                foreach (var subview in subviews.OfType<AddEditAppointmentView>())
                {
                    subview.ViewModel = viewModel;
                    subview.RefreshView();
                }
            }
        }

        public async Task ShowLoadError()
        {
            await Dialogs.ShowErrorDialogAsync(Context, new Exception("Failed to load appointment data"));
        }

        public void ShowLoading()
        {
            progressBar.Visibility = ViewStates.Visible;
            scrollView.Visibility = ViewStates.Gone;
        }

        public void StopLoading()
        {
            progressBar.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;
        }

        public void UpdateCalendarsList(List<CalendarViewModel> calendars)
        {
            // TODO :
        }

        public Task ShowLoadError(Exception ex)
        {
            return Dialogs.ShowErrorDialogAsync(Context, ex);
        }

        public void ShowEditingLoading()
        {
            //TODO : throw new NotImplementedException();
        }

        public void StopEditingLoading()
        {
            //TODO : 
        }

        public Task ShowEditingError(Exception ex)
        {
            return Dialogs.ShowErrorDialogAsync(Context, ex);
        }

        #endregion
    }

    class SeparatorSubview : View
    {
        public SeparatorSubview(Context c) : base(c)
        {
            SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, Conversion.ConvertDpToPixels(1.5f));
        }
    }

    class BasicTextView : AppCompatTextView
    {
        public BasicTextView(Context context) : base(context)
        {
            SetPadding(0, 0, 0, 0);
            SetBackgroundColor(Color.Transparent);
            this.SetTextAppearanceCompat(context, Resource.Style.editAppointmentText);
        }
    }

    class BasicTextField : AppCompatEditText
    {
        public BasicTextField(Context context) : base(context)
        {
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(0, 0, 0, 0);
            SetBackgroundColor(Color.Transparent);
            SetHintTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
            this.SetTextAppearanceCompat(context, Resource.Style.editAppointmentField);
        }
    }

    class TitleTextField : AppCompatEditText
    {
        public TitleTextField(Context context) : base(context)
        {
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(0, 0, 0, 0);
            SetBackgroundColor(Color.Transparent);
            SetHintTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
            this.SetTextAppearanceCompat(context, Resource.Style.editAppointmentTitle);
        }
    }


    abstract class AddEditAppointmentView : LinearLayoutCompat
    {
        protected static int DistanceLarge = Conversion.ConvertDpToPixels(16f);
        protected static int DistanceNormal = Conversion.ConvertDpToPixels(8f);
        protected static int DistanceSmall = Conversion.ConvertDpToPixels(4f);
        protected static int DistanceVerySmall = Conversion.ConvertDpToPixels(4f);

        public AddEditAppointmentViewModel ViewModel;
        readonly AppCompatImageView icon;

        protected AddEditAppointmentView(Context context, int resourceId = -1)
            : base(context)
        {
            Orientation = Horizontal;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceLarge);

            var iconSize = Conversion.ConvertDpToPixels(20f);
            icon = new AppCompatImageView(context)
            {
                LayoutParameters = new LayoutParams(iconSize, iconSize)
                {
                    Gravity = (int)GravityFlags.Start,
                    RightMargin = DistanceLarge,
                },
                Visibility = ViewStates.Invisible
            };

            AddView(icon);

            if (resourceId > 0)
            {
                icon.Visibility = ViewStates.Visible;
                icon.SetImageResource(resourceId);
            }

            LayoutTransition = new LayoutTransition();
        }

        abstract public void RefreshView();
    }

    class NameView : AddEditAppointmentView
    {
        TitleTextField TextField;

        public NameView(Context context) : base(context)
        {
            TextField = new TitleTextField(context);
            TextField.Hint = context.GetString(Resource.String.add_tite);
            TextField.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                    TextField.ClearFocus();
            };

            TextField.TextChanged += TextField_TextChanged;

            AddView(TextField);
        }

        private void TextField_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var editText = (AppCompatEditText)sender;
            if (ViewModel != null)
                ViewModel.Subject = editText.Text;
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel.Subject))
            {
                TextField.Text = ViewModel.Subject;
            }
        }
    }

    class LocationView : AddEditAppointmentView
    {
        BasicTextField TextField;

        public LocationView(Context context)
            : base(context, Resource.Drawable.location)
        {
            TextField = new BasicTextField(context);
            TextField.Hint = context.GetString(Resource.String.add_location);
            TextField.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                    TextField.ClearFocus();
            };

            TextField.TextChanged += TextField_TextChanged;

            AddView(TextField);
        }

        private void TextField_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var editText = (AppCompatEditText)sender;
            if (ViewModel != null)
                ViewModel.Location = editText.Text;
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel.Location))
            {
                TextField.Text = ViewModel.Location;
            }
        }
    }

    class AllDayToggleView : AddEditAppointmentView
    {
        SwitchCompat ToggleButton;
        Action toggleChanged = delegate { };

        public AllDayToggleView(Context context, Action toggleChanged)
            : base(context, Resource.Drawable.time)
        {
            this.toggleChanged = toggleChanged;

            var allDayText = new BasicTextView(context);
            allDayText.Text = "All day";
            allDayText.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.Left | (int)GravityFlags.CenterVertical,
            };

            AddView(allDayText);

            ToggleButton = new SwitchCompat(context)
            {
                Gravity = GravityFlags.Right,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)GravityFlags.Right | (int)GravityFlags.CenterVertical,
                }
            };

            ToggleButton.CheckedChange += ToggleButton_CheckedChange;

            AddView(ToggleButton);
        }

        private void ToggleButton_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            ViewModel.AllDay = !ViewModel.AllDay;
            toggleChanged.Invoke();
        }

        public override void RefreshView()
        {
            ToggleButton.Checked = ViewModel.AllDay;
        }
    }

    class StartDateView : DateView
    {
        public StartDateView(Context context) : base(context) { }

        public override void RefreshView()
        {
            if (ViewModel != null)
            {
                DateTextView.Text = ViewModel.Start.ToString("ddd, d MMMM yyyy", CultureInfo.CurrentCulture);

                if (ViewModel.AllDay)
                {
                    TimeTextView.Visibility = ViewStates.Gone;
                }
                else
                {
                    TimeTextView.Visibility = ViewStates.Visible;
                    TimeTextView.Text = ViewModel.Start.ToString("hh:mm", CultureInfo.CurrentCulture);
                }
            }
        }

        protected override async void TimeClicked(object sender, EventArgs e)
        {
            TimeSpan result = await Dialogs.ShowTimePicker(Context, ViewModel.Start.Hour, ViewModel.Start.Minute);
            var newDate = new DateTime(ViewModel.Start.Year, ViewModel.Start.Month, ViewModel.Start.Day, result.Hours, result.Minutes, 0);
            ViewModel.Start = newDate;
            RefreshView();
        }

        protected override async void DateClicked(object sender, EventArgs e)
        {
            long startTiemStamp = ViewModel.Start.ConvertDateTimeToTimestampMilliseconds();
            var newTimestamp = await Dialogs.ShowDatePicker(Context, startTiemStamp, addRemoveDateChoice: true);

            if (newTimestamp == 0)
            {
                return;
            }

            DateTime newDate = newTimestamp.ConvertTimestampMillisecondsToDateTime();
            ViewModel.Start = newDate + new TimeSpan(ViewModel.Start.Hour, ViewModel.Start.Minute, ViewModel.Start.Second);

            RefreshView();
            return;
        }
    }

    class EndDateView : DateView
    {
        public EndDateView(Context context) : base(context) { }

        public override void RefreshView()
        {
            if (ViewModel != null)
            {
                DateTextView.Text = ViewModel.End.ToString("ddd, d MMMM yyyy", CultureInfo.CurrentCulture);

                if (ViewModel.AllDay)
                {
                    TimeTextView.Visibility = ViewStates.Gone;
                }
                else
                {
                    TimeTextView.Visibility = ViewStates.Visible;
                    TimeTextView.Text = ViewModel.End.ToString("hh:mm", CultureInfo.CurrentCulture);
                }
            }
        }

        protected override async void TimeClicked(object sender, EventArgs e)
        {
            TimeSpan result = await Dialogs.ShowTimePicker(Context, ViewModel.Start.Hour, ViewModel.Start.Minute);
            var newDate = new DateTime(ViewModel.End.Year, ViewModel.End.Month, ViewModel.End.Day, result.Hours, result.Minutes, 0);
            ViewModel.Start = newDate;
            RefreshView();
        }

        protected override async void DateClicked(object sender, EventArgs e)
        {
            long endTiemStamp = ViewModel.End.ConvertDateTimeToTimestampMilliseconds();
            var newTimestamp = await Dialogs.ShowDatePicker(Context, endTiemStamp, addRemoveDateChoice: true);
            if (newTimestamp == 0)
            {
                return;
            }

            DateTime newDate = newTimestamp.ConvertTimestampMillisecondsToDateTime();
            ViewModel.End = newDate + new TimeSpan(ViewModel.End.Hour, ViewModel.End.Minute, ViewModel.End.Second);

            RefreshView();
            return;
        }
    }

    class DateView : AddEditAppointmentView
    {
        public BasicTextView DateTextView;
        public BasicTextView TimeTextView;

        public DateView(Context context) : base(context)
        {
            DateTextView = new BasicTextView(context);
            DateTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                Weight = 1,
                Gravity = (int)GravityFlags.Start | (int)GravityFlags.Left
            };

            DateTextView.Click += DateClicked;

            AddView(DateTextView);

            TimeTextView = new BasicTextView(context)
            {
                Gravity = GravityFlags.Right,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent),
                Text = ""
            };
            TimeTextView.TextAlignment = TextAlignment.ViewEnd;

            TimeTextView.Click += TimeClicked;

            AddView(TimeTextView);
        }

        protected virtual async void DateClicked(object sender, EventArgs e) { }

        protected virtual void TimeClicked(object sender, EventArgs e) { }

        public override void RefreshView() { }
    }

    class CalendarView : AddEditAppointmentView
    {
        readonly BasicTextView label;
        readonly View colorCircle;

        Action viewClicked;

        public CalendarView(Context context, Action viewClicked)
            : base(context, Resource.Drawable.calendar_black)
        {
            this.viewClicked = viewClicked;

            colorCircle = new View(context)
            {
                LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(10), Conversion.ConvertDpToPixels(10))
                {
                    Gravity = (int)GravityFlags.CenterVertical
                }
            };

            AddView(colorCircle);

            label = new BasicTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                {
                    LeftMargin = DistanceNormal
                },
                Text = "",
            };

            label.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.black)));
            AddView(label);

            Click += CalendarView_Click;
        }

        private void CalendarView_Click(object sender, EventArgs e)
        {
            viewClicked?.Invoke();
        }

        public string HexColor
        {
            set
            {
                var gd = new GradientDrawable();
                gd.SetShape(ShapeType.Oval);
                gd.SetStroke(Conversion.ConvertDpToPixels(1), Color.Black);
                gd.SetColor(Color.ParseColor(value));
                colorCircle.Background = gd;
            }
        }

        public override void RefreshView()
        {
            if (ViewModel?.Calendar != null)
            {
                HexColor = ViewModel.Calendar.HexColor;
                label.Text = ViewModel.Calendar.Name;
            }
        }
    }

    class ParticipantsView : AddEditAppointmentView
    {
        readonly BasicTextView title;

        Action viewClicked;

        public ParticipantsView(Context context, Action action)
            : base(context, Resource.Drawable.participants)
        {
            Orientation = Horizontal;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            viewClicked = action;
            title = new BasicTextView(context)
            {
                Text = "Participants",
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
            };

            AddView(title);
            Click += ParticipantsView_Click;
        }

        private void ParticipantsView_Click(object sender, EventArgs e)
        {
            viewClicked?.Invoke();
        }

        public override void RefreshView()
        {
            if (ViewModel != null && ViewModel.Participants != null && ViewModel.Participants.Count > 0)
                title.Text = $"{ViewModel.Participants.Count}";
            else
                title.Text = "None"; //TODO : string
        }
    }

    class MessageView : AddEditAppointmentView
    {
        BasicTextField TextField;
        public MessageView(Context context)
            : base(context, Resource.Drawable.description)
        {
            TextField = new BasicTextField(context);
            TextField.Hint = context.GetString(Resource.String.add_message);
            TextField.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                    TextField.ClearFocus();
            };

            TextField.TextChanged += TextField_TextChanged;

            AddView(TextField);
        }

        private void TextField_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var editText = (AppCompatEditText)sender;
            if (ViewModel != null)
                ViewModel.Description = editText.Text;
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel.Location))
            {
                TextField.Text = ViewModel.Description;
            }
        }
    }

    class ReocurrenceView : AddEditAppointmentView
    {
        readonly BasicTextView title;

        Action ViewClicked = delegate { };

        public ReocurrenceView(Context context, Action viewClicked)
            : base(context, Resource.Drawable.refresh_black)
        {
            Click += RecurrencView_Click;
            ViewClicked = viewClicked;

            title = new BasicTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    Weight = 1
                },
                Text = "Repeats"
            };

            AddView(title);

        }

        private void RecurrencView_Click(object sender, EventArgs e)
        {
            ViewClicked?.Invoke();
        }

        public override void RefreshView()
        {
            if (ViewModel?.RecurrenceInfo != null)
            {
                //TODO : 
            }
            else
            {
                title.Text = "Does not repeat";
            }
        }
    }

    class ReminderView : AddEditAppointmentView
    {
        readonly BasicTextView title;

        public ReminderView(Context context)
            : base(context, Resource.Drawable.alarm)
        {
            title = new BasicTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            AddView(title);

            Click += ReminderView_Click;
        }

        private async void ReminderView_Click(object sender, EventArgs e)
        {
            List<ReminderInfo> reminders = new List<ReminderInfo> {
                new ReminderInfo(ReminderInfo.ReminderType.None),
                new ReminderInfo(ReminderInfo.ReminderType.AtTheTime),
                new ReminderInfo(ReminderInfo.ReminderType.FiveMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.FifteenMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.ThirtyMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.OneHour),
                new ReminderInfo(ReminderInfo.ReminderType.TwoHours),
                new ReminderInfo(ReminderInfo.ReminderType.OneDay)
            };

            ReminderInfo selectedReminder = ReminderInfo.ConvertFromSeconds((int)ViewModel.ReminderTimeBeforeStart);

            var reminder = await Dialogs.ShowSingleSelectDialogAsync(Context, Resource.String.set_reminder, reminders, selectedReminder);
            //if (priority == default(Priority) || priority == DocumentPreview.Priority)
            //return;
        }

        public override void RefreshView()
        {
            if (ViewModel != null && ViewModel.ReminderTimeBeforeStart > -1)
            {
                ReminderInfo reminder = ReminderInfo.ConvertFromSeconds((int)ViewModel.ReminderTimeBeforeStart);  //TOODO this needs to be changed
                title.Text = reminder.Title;
            }
            else
            {
                title.Text = "Add reminder";
            }
        }
    }
}
