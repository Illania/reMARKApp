using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Common.Utilities;
using Android.Support.V4.Content;
using Android.Graphics.Drawables;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public abstract class AbstractAddEditAppointmentView : LinearLayoutCompat
    {
        protected static int DistanceLarge = Conversion.ConvertDpToPixels(16f);
        protected static int DistanceNormal = Conversion.ConvertDpToPixels(8f);
        protected static int DistanceSmall = Conversion.ConvertDpToPixels(4f);
        protected static int DistanceVerySmall = Conversion.ConvertDpToPixels(4f);

        protected AbstractAddEditAppointmentView(Context context)
            : base(context)
        {
            Orientation = Horizontal;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            LayoutTransition = new LayoutTransition();
        }

        abstract public void RefreshView();

        public void LeadingIcon()
        {

        }
    }

    abstract class AddEditAppointmentView : AbstractAddEditAppointmentView
    {
        public AddEditAppointmentViewModel ViewModel;
        public ContactCreationModeFlag CreationMode;
        readonly AppCompatImageView icon;

        protected AddEditAppointmentView(Context context)
            : base(context)
        {
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

            SetBackgroundColor(Color.Azure);

            AddView(icon);
        }

        public void SetIcon(int resourceId)
        {
            icon.Visibility = ViewStates.Visible;
            icon.SetImageResource(resourceId);
        }
    }

    class NameView : AddEditAppointmentView
    {
        AppCompatEditText TextField;

        public NameView(Context context) : base(context)
        {
            TextField = LayoutInflater.From(context).Inflate(Resource.Layout.search_edit_text_layout, null).FindViewById<AppCompatEditText>(Resource.Id.search_edit_text);
            TextField.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {

            };
            TextField.SetPadding(0, 0, 0, 0);
            TextField.SetBackgroundColor(Color.Transparent);
            TextField.SetTextAppearanceCompat(context, Resource.Style.searchViewBottomLine);
            TextField.SetHintTextColor(ViewUtilities.GetColorStateList(context, Resource.Drawable.search_edit_text_selector));
            TextField.Hint = context.GetString(Resource.String.add_tite);
            TextField.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                    TextField.ClearFocus();
            };

            TextField.TextChanged += TextField_TextChanged;

            AddView(TextField);

            SetIcon(Resource.Drawable.action_search_server);
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
        AppCompatEditText TextField;
        public LocationView(Context context) : base(context)
        {
            TextField = LayoutInflater.From(context).Inflate(Resource.Layout.search_edit_text_layout, null).FindViewById<AppCompatEditText>(Resource.Id.search_edit_text);
            TextField.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {

            };
            TextField.SetPadding(0, 0, 0, 0);
            TextField.SetBackgroundColor(Color.Transparent);
            TextField.SetTextAppearanceCompat(context, Resource.Style.searchViewBottomLine);
            TextField.SetHintTextColor(ViewUtilities.GetColorStateList(context, Resource.Drawable.search_edit_text_selector));
            TextField.Hint = context.GetString(Resource.String.add_location);
            TextField.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                    TextField.ClearFocus();
            };

            TextField.TextChanged += TextField_TextChanged;

            AddView(TextField);

            SetIcon(Resource.Drawable.action_search_server);
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

        public AllDayToggleView(Context context, Action toggleChanged) : base(context)
        {
            AppCompatTextView TextField = new AppCompatTextView(context);

            this.toggleChanged = toggleChanged;

            TextField.SetPadding(0, 0, 0, 0);
            TextField.SetBackgroundColor(Color.Transparent);
            TextField.SetTextAppearanceCompat(context, Resource.Style.searchViewBottomLine);
            TextField.Text = "All day";

            AddView(TextField);

            SetIcon(Resource.Drawable.action_search_server);

            ToggleButton = new SwitchCompat(context);

            ToggleButton.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.End,
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
        public AppCompatTextView DateTextView;
        public AppCompatTextView TimeTextView;

        public DateView(Context context) : base(context)
        {
            DateTextView = new AppCompatTextView(context);
            DateTextView.SetPadding(0, 0, 0, 0);
            DateTextView.SetBackgroundColor(Color.Transparent);
            DateTextView.SetTextAppearanceCompat(context, Resource.Style.searchViewBottomLine);
            DateTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Weight = 1,
                Gravity = (int)GravityFlags.Start | (int)GravityFlags.Left
            };

            DateTextView.Click += DateClicked;

            AddView(DateTextView);

            TimeTextView = new AppCompatTextView(context);
            TimeTextView.SetPadding(0, 0, 0, 0);
            TimeTextView.SetTextAppearanceCompat(context, Resource.Style.searchViewBottomLine);
            TimeTextView.TextAlignment = TextAlignment.ViewEnd;
            TimeTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Weight = 1,
                Gravity = (int)GravityFlags.Right | (int)GravityFlags.End
            };

            TimeTextView.Click += TimeClicked;

            AddView(TimeTextView);
        }

        protected virtual async void DateClicked(object sender, EventArgs e) { }

        protected virtual void TimeClicked(object sender, EventArgs e) { }

        public override void RefreshView() { }
    }

    class CalendarView : AddEditAppointmentView
    {
        readonly AppCompatTextView label;
        readonly View colorCircle;

        Action viewClicked;

        public CalendarView(Context context, Action action)
            : base(context)
        {
            Orientation = Horizontal;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            viewClicked = action;
            AppCompatTextView title = new AppCompatTextView(context)
            {
                Text = "Calendar",
                Gravity = GravityFlags.Left,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.8f)
            };

            title.SetTextAppearanceCompat(context, Resource.Style.searchViewBottomLine);

            title.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkgray)));
            AddView(title);

            colorCircle = new View(context)
            {
                LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(10), Conversion.ConvertDpToPixels(10))
                {
                    Gravity = (int)GravityFlags.CenterVertical
                }
            };

            AddView(colorCircle);

            label = new AppCompatTextView(context)
            {
                Gravity = GravityFlags.Right,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.2f),
                Text = ""
            };

            label.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.black)));
            AddView(label);

            SetIcon(Resource.Drawable.action_search_server);

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
            if (ViewModel != null && ViewModel.Calendar != null)
            {
                CalendarViewModel calendarViewModel = ViewModel.Calendar;
                HexColor = calendarViewModel?.HexColor;
                label.Text = calendarViewModel?.Name;
            }
        }
    }

    class ParticipantsView : AddEditAppointmentView
    {
        readonly AppCompatTextView countLabel;
        readonly View colorCircle;

        Action viewClicked;

        public ParticipantsView(Context context, Action action)
            : base(context)
        {
            Orientation = Horizontal;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            viewClicked = action;
            AppCompatTextView title = new AppCompatTextView(context)
            {
                Text = "Participants",
                Gravity = GravityFlags.Left,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.8f)
            };

            title.SetTextAppearanceCompat(context, Resource.Style.searchViewBottomLine);

            title.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkgray)));
            AddView(title);

            countLabel = new AppCompatTextView(context)
            {
                Gravity = GravityFlags.Right,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.2f),
                Text = ""
            };

            countLabel.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.black)));
            AddView(countLabel);

            SetIcon(Resource.Drawable.action_search_server);

            Click += ParticipantsView_Click;
        }

        private void ParticipantsView_Click(object sender, EventArgs e)
        {
            viewClicked?.Invoke();
        }

        public override void RefreshView()
        {
            if (ViewModel != null && ViewModel.Participants != null)
            {
                if (ViewModel.Participants != null && ViewModel.Participants.Count > 0)
                    countLabel.Text = $"{ViewModel.Participants.Count}";
                else
                    countLabel.Text = "None"; //TODO : string
            }
        }
    }

    public class AddEditAppointmentFragment : BaseFragment, IAddEditAppointmentView, IMenuItemOnMenuItemClickListener
    {
        const string CreationModeFlagBundleKey = "CreationModeFlag_ab9071da-34f6-45fc-9a03-a0b348814dcd";
        const string AppointmentIdBundleKey = "AppointmentId_d09e0cb6-e224-4327-8d09-43ce921f53c6";
        const string CalendarIdBundleKey = "CalendarId_d09e0cb6-e224-4327-8d09-43ce921f53c6";

        AddEditAppointmentPresenter presenter;

        ContactCreationModeFlag creationModeFlag;
        bool loaded;
        static int calendarId;
        static int appointmentId;

        LinearLayoutCompat linearLayout;
        NestedScrollView scrollView;
        ProgressBar progressBar;

        StartDateView startDateView;
        EndDateView endDateView;
        CalendarView calendarView;
        ParticipantsView participantsView;

        AddEditAppointmentViewModel viewModel;

        List<AddEditAppointmentView> subviews = new List<AddEditAppointmentView>();

        public static (AddEditAppointmentFragment fragment, string tag) NewInstance()
        {
            Bundle args = new Bundle();

            var fragment = new AddEditAppointmentFragment
            {
                Arguments = args
            };

            args.PutInt(CreationModeFlagBundleKey, (int)ContactCreationModeFlag.New);

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

        #region Fragment Lifecycle
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(CreationModeFlagBundleKey))
                creationModeFlag = (ContactCreationModeFlag)Arguments.GetInt(CreationModeFlagBundleKey);

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
                    await presenter.LoadEmptyAppointment();
                else
                    await presenter.LoadAppointment(calendarId, appointmentId);

                loaded = true;
            }
            else
            {
                StopLoading();
                if (viewModel != null)
                    ShowAppointment(viewModel);
            }
        }

        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {

        }

        #endregion

        #region Helpers

        void PrepareViews()
        {
            subviews.Add(new NameView(Context));
            subviews.Add(new LocationView(Context));
            subviews.Add(new AllDayToggleView(Context, AllDayToggleChanged));
            subviews.Add(startDateView = new StartDateView(Context));
            subviews.Add(endDateView = new EndDateView(Context));
            subviews.Add(calendarView = new CalendarView(Context, CalendarClicked));
            subviews.Add(participantsView = new ParticipantsView(Context, ParticipantsClicked));
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
            throw new NotImplementedException();
        }

        public Task ShowAddingEditingError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void ShowAppointment(AddEditAppointmentViewModel viewModel)
        {
            this.viewModel = viewModel;

            foreach (var subview in subviews)
            {
                subview.ViewModel = viewModel;
                subview.CreationMode = creationModeFlag;
                subview.RefreshView();
            }
        }

        public Task ShowLoadError()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        #endregion
    }
}
