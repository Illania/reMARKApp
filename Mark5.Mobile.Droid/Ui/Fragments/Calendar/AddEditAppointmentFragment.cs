using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.CalendarViews.AddEditAppointmentViews;
using CalendarView = Mark5.Mobile.Droid.Ui.Views.CalendarViews.AddEditAppointmentViews.CalendarView;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class AddEditAppointmentFragment : BaseFragment, IAddEditAppointmentView, IMenuItemOnMenuItemClickListener
    {
        const string CreationModeFlagBundleKey = "CreationModeFlag_ab9071da-34f6-45fc-9a03-a0b348814dcd";
        const string AppointmentIdBundleKey = "AppointmentId_d09e0cb6-e224-4327-8d09-43ce921f53c6";
        const string CalendarIdBundleKey = "CalendarId_d09e0cb6-e224-4327-8d09-43ce921f53c6";
        const string RecurrenceIndexBundleKey = "RecurrenceIndex_7D5D4F43-FA17-4E51-944B-F33D06111515";
        const string AppointmentChangeTypeBundleKey = "AppointmentChangeType_2FC09FB5-3244-403B-B676-C820AE93429B";
        const string StartDateIdBundleKey = "StartDate_3b43a244-6a24-496f-9d33-1eeb1c277005";

        AddEditAppointmentPresenter presenter;

        ContactCreationModeFlag creationModeFlag;
        bool loaded;
        int calendarId;
        int appointmentId;
        int appointmentChangeType;
        int recurrenceIndex;
        DateTime startDate;

        LinearLayoutCompat linearLayout;
        NestedScrollView scrollView;
        ProgressBar progressBar;

        StartDateView startDateView;
        EndDateView endDateView;
        CalendarView calendarView;
        ParticipantsView participantsView;
        ReocurrenceView recurrenceView;

        AddEditAppointmentViewModel viewModel;
        List<CalendarViewModel> calendarList;

        List<View> subviews = new List<View>();

        Action dismissAction;

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

        public static (AddEditAppointmentFragment fragment, string tag) NewInstance(int calendarId, int appointmentId,
            AppointmentChangeType appointmentChangeType, int recurrenceIndex)
        {
            Bundle args = new Bundle();

            var fragment = new AddEditAppointmentFragment
            {
                Arguments = args
            };

            args.PutInt(CreationModeFlagBundleKey, (int)ContactCreationModeFlag.Edit);
            args.PutInt(AppointmentIdBundleKey, appointmentId);
            args.PutInt(CalendarIdBundleKey, calendarId);
            args.PutInt(AppointmentChangeTypeBundleKey, (int)appointmentChangeType);
            args.PutInt(RecurrenceIndexBundleKey, recurrenceIndex);

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

            if (Arguments.ContainsKey(AppointmentChangeTypeBundleKey))
                appointmentChangeType= Arguments.GetInt(AppointmentChangeTypeBundleKey);

            if (Arguments.ContainsKey(StartDateIdBundleKey))
                startDate = new DateTime(Arguments.GetLong(StartDateIdBundleKey));

            presenter = new AddEditAppointmentPresenter();
            presenter.AttachView(this);
            presenter.LoadCalendarsList();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditAppointmentFragment)}, " + $"mode={creationModeFlag}]...");

            HasOptionsMenu = true;

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.SetBackgroundColor(Color.White);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.Alpha = 0;
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
                    await presenter.LoadAppointment(calendarId, appointmentId, recurrenceIndex);
                loaded = true;

                StopLoading();
            }
            else
            {
                StopLoading();
                if (viewModel != null)
                    ShowAppointment(viewModel);
            }

            linearLayout.Animate().Alpha(1f).SetDuration(500);
        }

        #endregion

        #region IMenu

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var addAppointment = menu.Add(Menu.None, MenuItemActions.SaveAppointment, MenuItemActions.SaveAppointment, Resource.String.save);
            addAppointment.SetShowAsAction(ShowAsAction.Always);
            addAppointment.SetOnMenuItemClickListener(this);
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.SaveAppointment)
            {
                AddOrEditAppointment();
                return true;
            }

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
            subviews.Add(recurrenceView = new ReocurrenceView(Context, ReocurrenceClicked));
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

        async void AddOrEditAppointment()
        {
            await presenter.AddOrEditAppointment(viewModel, (AppointmentChangeType)appointmentChangeType);
        }

        async void ReocurrenceClicked()
        {
            var strings = new string[] { "Never", "Custom" };
            var result = await Dialogs.ShowListDialog(Context, string.Empty, strings, true);

            if (result < 0)
                return;

            if (result == 0)
            {
                viewModel.RecurrenceInfo = null;
                recurrenceView.RefreshView();
                return;
            }

            var recInfo = viewModel.RecurrenceInfo ?? viewModel.GetEmptyRecurrenceInfo();

            var (rf, tag) = ReoccurrenceFragment.NewInstance(recInfo);
            ((AppCompatActivity)Activity).SupportFragmentManager.BeginTransaction()
                          .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                          .Replace(Resource.Id.fragment_container, rf, tag)
                          .AddToBackStack(tag).Commit();

            var rec = await rf.Task;
            if (rec != null)
            {
                viewModel.RecurrenceInfo = rec;
                recurrenceView?.RefreshView();
            }
        }

        async void ParticipantsClicked()
        {
            var (aepf, tag) = AddEditParticipantsFragment.NewInstance(viewModel.Participants);

            ((AppCompatActivity)Activity).SupportFragmentManager.BeginTransaction()
                           .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                           .Replace(Resource.Id.fragment_container, aepf, tag)
                           .AddToBackStack(tag).Commit();

            var result = await aepf.TaskResult;
            if (result != null)
            {
                viewModel.Participants = result;
                participantsView?.RefreshView();
            }
        }

        async void CalendarClicked()
        {
            var (clf, tag) = AddEditAppointmentCalendarListFragment.NewInstance(calendarList, viewModel.Calendar);

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
            calendarList = calendars;
        }

        public Task ShowLoadError(Exception ex)
        {
            return Dialogs.ShowErrorDialogAsync(Context, ex);
        }

        public void ShowEditingLoading()
        {
            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity,
                creationModeFlag == ContactCreationModeFlag.New ? Resource.String.adding_appointment : Resource.String.editing_appointment,
                Resource.String.please_wait);
        }

        public void StopEditingLoading()
        {
            dismissAction?.Invoke();
        }

        public Task ShowEditingError(Exception ex)
        {
            return Dialogs.ShowErrorDialogAsync(Context, ex);
        }

        #endregion
    }
}
