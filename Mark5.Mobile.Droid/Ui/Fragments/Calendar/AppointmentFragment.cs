using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.CalendarViews.AppointmentViews;
using Mark5.Mobile.Droid.Utilities;
using CalendarView = Mark5.Mobile.Droid.Ui.Views.CalendarViews.AppointmentViews.CalendarView;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class AppointmentFragment : BaseFragment, IAppointmentView
    {
        const string CalendarBundleKey = "CalendarKey";
        const string AppointmentBundleKey = "AppointmentKey";
        const string ReocurrenceBundleKey = "ReocurrenceKey";
        const string ShowActionsKey = "ShowActionsKey";

        AppointmentPresenter presenter;

        int calendarId;
        int appointmentId;
        int recurrenceIndex;
        bool showActions;

        Action dismissLoadingAction;
        List<LineViewModel> lineViewModels;

        LinearLayoutCompat linearLayout;
        NestedScrollView scrollView;
        ProgressBar progressBar;

        ParticipantsView participantsView;

        AppointmentViewModel viewModel;

        List<View> subviews = new List<View>();

        public static (AppointmentFragment fragment, string tag) NewInstance(int calendarId, int appointmentId, int recurrenceIndex, bool showActions = true)
        {
            var fragment = new AppointmentFragment();
            var tag = $"{nameof(AppointmentFragment)} [calendarId={calendarId}, appointmentId={appointmentId}, recurrenceIndex={recurrenceIndex}]";

            var args = new Bundle();

            args.PutInt(CalendarBundleKey, calendarId);
            args.PutInt(AppointmentBundleKey, appointmentId);
            args.PutInt(ReocurrenceBundleKey, recurrenceIndex);
            args.PutBoolean(ShowActionsKey, showActions);

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

            if (Arguments.ContainsKey(ShowActionsKey))
                showActions = Arguments.GetBoolean(ShowActionsKey);

            presenter = new AppointmentPresenter();
            presenter.AttachView(this);
            presenter.Start();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AppointmentFragment)}");

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

            HasOptionsMenu = showActions;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            CommonConfig.Logger.Info($"Created {nameof(AppointmentFragment)}...");

            ((AppCompatActivity)Activity).SupportActionBar.Title = null;
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
            subviews.Add(new SubjectView(Context));
            subviews.Add(new CalendarView(Context));
            subviews.Add(new DateView(Context));
            subviews.Add(new LocationView(Context));
            subviews.Add(new MessageView(Context));
            subviews.Add(new ParticipantsView(Context));
            subviews.Add(new SendInvitationView(Context, SendInvitations_Click));
            subviews.ForEach(linearLayout.AddView);
        }

        async void SendInvitations_Click()
        {
            var lineNames = lineViewModels.Select(l => l.Name).ToArray();
            var result = await Dialogs.ShowListDialog(Context, Resource.String.select_a_line, lineNames, true);
            if (result >= 0)
                await presenter.SendInvitationsClicked(lineViewModels[result]);
        }

        #endregion

        #region Options menu related

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var deleteItem = menu.Add(Menu.None, MenuItemActions.DeleteAppointment, MenuItemActions.DeleteAppointment, Resource.String.delete);
            deleteItem.SetIcon(Resource.Drawable.delete);
            deleteItem.SetShowAsAction(ShowAsAction.Always);

            var editItem = menu.Add(Menu.None, MenuItemActions.EditAppointment, MenuItemActions.EditAppointment, Resource.String.edit);
            editItem.SetIcon(Resource.Drawable.create);
            editItem.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.DeleteAppointment)
            {
                AsyncHelpers.RunOnUiThreadAsync((Activity)Context, async () =>
                {
                    var confirm = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
                    if (confirm)
                        await presenter.DeleteAppointmentClicked();
                });
            }
            else
            {
                presenter.EditAppointmentClicked();
            }

            return true;
        }

        static class MenuItemActions
        {
            public const int DeleteAppointment = 10;
            public const int EditAppointment = 20;
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
            participantsView?.RefreshView();
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
