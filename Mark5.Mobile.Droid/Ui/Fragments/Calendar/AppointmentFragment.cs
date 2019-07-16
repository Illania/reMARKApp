using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;

using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class AppointmentFragment : BaseFragment, IAppointmentView
    {
        SubjectView subjectView;


        AppointmentPresenter appointmentPresenter;

        int largeSpacing = Conversion.ConvertDpToPixels(24f);
        int normalSpacing = Conversion.ConvertDpToPixels(8f);

        public static (AppointmentFragment fragment, string tag) NewInstance(int calendarId, int appointmentId, int recurrenceIndex)
        {
            var fragment = new AppointmentFragment();
            var tag = $"{nameof(AppointmentFragment)} [calendarId={calendarId}, fappointmentId={appointmentId}, recurrenceIndex={recurrenceIndex}]";
            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            appointmentPresenter = new AppointmentPresenter();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AppointmentFragment)}");
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            LinearLayoutCompat linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            subjectView = new SubjectView(Context);

            linearLayout.AddView(subjectView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            appointmentPresenter.AttachView(this);
        }

        public void CloseView()
        {
            throw new NotImplementedException();
        }

        public void OpenEditAppointment(int calendarId, int appointmentId)
        {
            throw new NotImplementedException();
        }

        public void SetLines(IEnumerable<LineViewModel> lines)
        {
            throw new NotImplementedException();
        }

        public void ShowAppointment(AppointmentViewModel appointment)
        {
            throw new NotImplementedException();
        }

        public Task ShowDeleteError()
        {
            throw new NotImplementedException();
        }

        public Task ShowLoadError()
        {
            throw new NotImplementedException();
        }

        public void ShowLoading()
        {
            throw new NotImplementedException();
        }

        public Task ShowSendInvitationError()
        {
            throw new NotImplementedException();
        }

        public void StopLoading()
        {
            throw new NotImplementedException();
        }

        interface IAppointmentView
        {
            void Refresh();
        }

        class SubjectView : AppCompatTextView, IAppointmentView
        {
            int largeSpacing = Conversion.ConvertDpToPixels(24f);
            int normalSpacing = Conversion.ConvertDpToPixels(8f);

            public SubjectView(Context context)
                : base(context)
            {
                Text = context.GetString(Resource.String.related_contacts);

                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                SetPadding(largeSpacing, 0, largeSpacing, normalSpacing);
            }

            public void Refresh()
            {
                //TODO:
            }
        }


    }
}
