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
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.CalendarViews.AppointmentViews;
using CalendarView = Mark5.Mobile.Droid.Ui.Views.CalendarViews.AppointmentViews.CalendarView;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class BaseAppointmentFragment : BaseFragment, IAppointmentView
    {
        const string CalendarBundleKey = "Calendar_Id";
        const string AppointmentBundleKey = "Appointment_Id";
        const string ReocurrenceBundleKey = "Reocurrence_Id";

        AppointmentPresenter presenter;

        int calendarId;
        int appointmentId;
        int recurrenceIndex;

        Action dismissLoadingAction;

        LinearLayoutCompat linearLayout;
        NestedScrollView scrollView;
        ProgressBar progressBar;

        ParticipantsView participantsView;

        AppointmentViewModel viewModel;

        List<View> subviews = new List<View>();

        public static (BaseAppointmentFragment fragment, string tag) NewInstance(int calendarId, int appointmentId, int recurrenceIndex)
        {
            var fragment = new BaseAppointmentFragment();
            var tag = $"{nameof(BaseAppointmentFragment)} [calendarId={calendarId}, appointmentId={appointmentId}, recurrenceIndex={recurrenceIndex}]";

            var args = new Bundle();

            args.PutInt(CalendarBundleKey, calendarId);
            args.PutInt(AppointmentBundleKey, appointmentId);
            args.PutInt(ReocurrenceBundleKey, recurrenceIndex);

            fragment.Arguments = args;
            return (fragment, tag);
        }

        #region Fragment Lifecycle

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(CalendarBundleKey))
                calendarId = Arguments.GetInt(CalendarBundleKey);

            if (Arguments.ContainsKey(AppointmentBundleKey))
                appointmentId = Arguments.GetInt(AppointmentBundleKey);

            if (Arguments.ContainsKey(ReocurrenceBundleKey))
                recurrenceIndex = Arguments.GetInt(ReocurrenceBundleKey);

            presenter = new AppointmentPresenter();
            presenter.AttachView(this);
            presenter.Start();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(BaseAppointmentFragment)}");

            HasOptionsMenu = false;

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

        public override async void OnResume()
        {
            base.OnResume();
            await presenter.LoadAppointment(appointmentId, recurrenceIndex, calendarId);
        }

        public override void OnDestroy()
        {
            presenter?.Stop();
            base.OnDestroy();
        }

        #endregion

        #region Helpers

        void PrepareViews()
        {
            subviews.Add(new NameView(Context));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(new CalendarView(Context, null));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(new DateView(Context));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(participantsView = new ParticipantsView(Context, null));
            subviews.Add(new SeparatorSubview(Context));
            subviews.Add(new LocationView(Context));
            subviews.Add(new SeparatorSubview(Context));
            //subviews.Add(new ReminderView(Context));
            //subviews.Add(new SeparatorSubview(Context));
            subviews.Add(new MessageView(Context));
            subviews.Add(new SeparatorSubview(Context));
            subviews.ForEach(linearLayout.AddView);
        }

        #endregion

        #region IAddEditAppointmentView implementation

        public void CloseView()
        {
            FragmentManager?.PopBackStack();
        }

        public void OpenEditAppointment(int calendarId, int appointmentId)
        {
            var (aeaf, tag) = AddEditAppointmentFragment.NewInstance(calendarId, appointmentId);
            ((AppCompatActivity)Activity).SupportFragmentManager.BeginTransaction()
                          .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                          .Replace(Resource.Id.fragment_container, aeaf, tag)
                          .AddToBackStack(tag).Commit();
        }

        public void SetLines(IEnumerable<LineViewModel> lines)
        {
            lineViewModels = lines.ToList();
        }

        public void ShowAppointment(AppointmentViewModel viewModel)
        {
            if (viewModel != null)
            {
                this.viewModel = viewModel;

                foreach (var subview in subviews.OfType<AppointmentView>())
                {
                    subview.ViewModel = viewModel;
                    subview.RefreshView();
                }
            }

            linearLayout.Animate().Alpha(1f).SetDuration(500);
        }

        public void ShowAppointmentLoadingDialog()
        {
            dismissLoadingAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_appointments, Resource.String.please_wait);
            progressBar.Visibility = ViewStates.Visible;
            scrollView.Visibility = ViewStates.Gone;
        }

        public void CloseDialog()
        {
            dismissLoadingAction?.Invoke();
            progressBar.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;
        }

        public void UpdateParticipants(List<ParticipantsViewModel> participants)
        {
            // participantsView?.Refresh(participants);  //TODO
        }

        public void ShowSendInvitationsDialog()
        {
            dismissLoadingAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.sending_invitations, Resource.String.please_wait);
        }

        public void ShowDeletingDialog()
        {
            dismissLoadingAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.deleting, Resource.String.please_wait);
        }

        public async Task ShowLoadError(Exception ex)
        {
            await Dialogs.ShowErrorDialogAsync(Context, ex);
        }

        public async Task ShowDeleteError(Exception ex)
        {
            await Dialogs.ShowErrorDialogAsync(Context, ex);
        }

        public async Task ShowSendInvitationError(Exception ex)
        {
            await Dialogs.ShowErrorDialogAsync(Context, ex);
        }

        #endregion
    }
}
