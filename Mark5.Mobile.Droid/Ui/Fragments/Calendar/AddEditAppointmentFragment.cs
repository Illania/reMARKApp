using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

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
            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            LayoutTransition = new LayoutTransition();
        }

        abstract public void RefreshView();

        public void LeadingIcon()
        {

        }
    }

    public abstract class AddEditAppointmentView : AbstractAddEditAppointmentView
    {
        public AddEditAppointmentViewModel ViewModel;
        public ContactCreationModeFlag CreationMode;
        public LinearLayoutCompat LayoutContainer;

        ImageView icon;

        protected AddEditAppointmentView(Context context)
            : base(context)
        {
            LayoutContainer = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
            };

            AddView(LayoutContainer);

            icon = new ImageView(context);
            LayoutContainer.AddView(icon);
        }
    }

    public class NameView : AddEditAppointmentView
    {
        public NameView(Context context) : base(context)
        {

        }

        public override void RefreshView()
        {

        }
    }

    public class AddEditAppointmentFragment : BaseFragment, IAddEditAppointmentView
    {
        const string CreationModeFlagBundleKey = "CreationModeFlag_ab9071da-34f6-45fc-9a03-a0b348814dcd";
        const string AppointmentIdBundleKey = "AppointmentId_d09e0cb6-e224-4327-8d09-43ce921f53c6";
        const string CalendarIdBundleKey = "CalendarId_d09e0cb6-e224-4327-8d09-43ce921f53c6";

        AddEditAppointmentPresenter presenter;

        //TODO:
        ContactCreationModeFlag creationModeFlag;
        bool loaded;
        static int calendarId;
        static int appointmentId;

        LinearLayoutCompat linearLayout;
        NestedScrollView scrollView;
        FloatingActionButton fab;
        ProgressBar progressBar;

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
            CommonConfig.Logger.Info($"Creating {nameof(AddEditAppointmentFragment)}, " +
                                    $"mode={creationModeFlag}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            linearLayout.FocusableInTouchMode = true;

            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress);

            fab = ((BaseAppCompatActivity)Activity).Fab;
            fab.SetImageResource(Resource.Drawable.action_save);
            fab.Enabled = true;
            fab.Size = FloatingActionButton.SizeNormal;
            fab.Visibility = ViewStates.Visible;

            subviews.Clear();

            //TODO set title for edit
            if (creationModeFlag == ContactCreationModeFlag.New)
                ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.create_appointment);

            var bottomMargin = ((CoordinatorLayout.LayoutParams)fab.LayoutParameters).BottomMargin;
            var fabHeight = Conversion.ConvertDpToPixels(56);

            linearLayout.SetPadding(linearLayout.PaddingLeft, linearLayout.PaddingTop, linearLayout.PaddingRight, fabHeight + bottomMargin * 2);

            PrepareViews();

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CommonConfig.Logger.Info($"Created {nameof(AddEditAppointmentFragment)} mode={creationModeFlag}]...");
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
        }

        #endregion

        #region Helpers

        void PrepareViews()
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
