using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
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
        readonly int largeSpacing = Conversion.ConvertDpToPixels(24f);
        readonly int normalSpacing = Conversion.ConvertDpToPixels(8f);

        AppointmentSubjectView subjectView;
        AppointmentDateView dateView;
        AppointmentReocurrenceView reocurrenceView;
        AppointmentLocationView locationView;
        AppointmentDescriptionView descriptionView;
        AppointmentOrganizerView organizerView;
        AppointmentCalendarView calendarView;
        AppointmentPresenter appointmentPresenter;
        AppointmentParticipantsView participantsView;

        int calendarId;
        int appointmentId;
        int recurrenceIndex;

        const string CalendarBundleKey = "Calendar_Id";
        const string AppointmentBundleKey = "Appointment_Id";
        const string ReocurrenceBundleKey = "Reocurrence_Id";

        public static (AppointmentFragment fragment, string tag) NewInstance(int calendarId, int appointmentId, int recurrenceIndex)
        {
            var fragment = new AppointmentFragment();
            var tag = $"{nameof(AppointmentFragment)} [calendarId={calendarId}, fappointmentId={appointmentId}, recurrenceIndex={recurrenceIndex}]";

            var args = new Bundle();

            args.PutInt(CalendarBundleKey, calendarId);
            args.PutInt(AppointmentBundleKey, appointmentId);
            args.PutInt(ReocurrenceBundleKey, recurrenceIndex);

            fragment.Arguments = args;
            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(CalendarBundleKey))
                calendarId = Arguments.GetInt(CalendarBundleKey);

            if (Arguments.ContainsKey(AppointmentBundleKey))
                appointmentId = Arguments.GetInt(AppointmentBundleKey);

            if (Arguments.ContainsKey(ReocurrenceBundleKey))
                recurrenceIndex = Arguments.GetInt(ReocurrenceBundleKey);

            appointmentPresenter = new AppointmentPresenter();
            appointmentPresenter.AttachView(this);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AppointmentFragment)}");
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            LinearLayoutCompat linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            subjectView = new AppointmentSubjectView(Context);
            subjectView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(subjectView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            dateView = new AppointmentDateView(Context);
            dateView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(dateView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            reocurrenceView = new AppointmentReocurrenceView(Context);
            reocurrenceView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(reocurrenceView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            locationView = new AppointmentLocationView(Context);
            locationView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(locationView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            descriptionView = new AppointmentDescriptionView(Context);
            descriptionView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(descriptionView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            organizerView = new AppointmentOrganizerView(Context);
            organizerView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(organizerView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            calendarView = new AppointmentCalendarView(Context);
            calendarView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(calendarView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            participantsView = new AppointmentParticipantsView(Context);
            participantsView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(participantsView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            HasOptionsMenu = true;

            return rootView;
        }

        bool loaded;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (!loaded)
            {
                await appointmentPresenter.LoadAppointment(appointmentId, recurrenceIndex, calendarId);
                loaded = true;
            }
        }

        public void CloseView()
        {
            //TODO:
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

        #region Options menu related

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var deleteItem = menu.Add(Menu.None, MenuItemActions.DeleteAppointment, MenuItemActions.DeleteAppointment, Resource.String.delete);
            deleteItem.SetIcon(Resource.Drawable.action_bin);
            deleteItem.SetShowAsAction(ShowAsAction.Always);

            var editItem = menu.Add(Menu.None, MenuItemActions.EditAppointment, MenuItemActions.EditAppointment, Resource.String.edit);
            editItem.SetIcon(Resource.Drawable.action_new);
            editItem.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return true;
        }

        static class MenuItemActions
        {
            public const int DeleteAppointment = 10;
            public const int EditAppointment = 20;
        }

        #endregion

        #region Custome Appointment Views
        interface IAppointmentView
        {
            void Refresh(AppointmentViewModel viewModel);
            void SetViewPadding(int left, int top, int right, int bottom);
        }

        class AppointmentSubjectView : AppCompatTextView, IAppointmentView
        {

            public AppointmentSubjectView(Context context)
                : base(context)
            {
                Text = context.GetString(Resource.String.related_contacts);
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                SetTextSize(Android.Util.ComplexUnitType.Sp, 22);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Subject;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentDateView : AppCompatTextView, IAppointmentView
        {

            public AppointmentDateView(Context context)
                : base(context)
            {
                Text = context.GetString(Resource.String.related_contacts);
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (viewModel.Start.Date.CompareTo(viewModel.End.Date) == 0)
                {
                    Text += viewModel.Start.ToString("dddd, d MMMM yyyy", CultureInfo.CurrentCulture);
                    if (viewModel.AllDay)
                        Text += "\r\nAll Day";
                    else
                        Text += $"\r\nfrom { viewModel.Start.ToString("hh:mm", CultureInfo.CurrentCulture) } to { viewModel.End.ToString("hh:mm", CultureInfo.CurrentCulture) }";
                }
                else
                {
                    if (viewModel.AllDay)
                    {
                        Text = $"All day from { viewModel.Start.ToString("ddd, d MMMM yyyy", CultureInfo.CurrentCulture) } ";
                        Text += $"\r\nto { viewModel.End.ToString("ddd, d MMMM yyyy", CultureInfo.CurrentCulture) }";
                    }
                    else
                    {
                        Text = $"from { viewModel.Start.ToString("hh:mm ddd, d MMMM yyyy", CultureInfo.CurrentCulture) } ";
                        Text += $"\r\nto { viewModel.End.ToString("hh:mm ddd, d MMMM yyyy", CultureInfo.CurrentCulture) }";
                    }
                }
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentReocurrenceView : AppCompatTextView, IAppointmentView
        {

            public AppointmentReocurrenceView(Context context)
                : base(context)
            {
                Text = context.GetString(Resource.String.related_contacts);
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.RecurrenceInfo;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentLocationView : AppCompatTextView, IAppointmentView
        {

            public AppointmentLocationView(Context context)
                : base(context)
            {
                Text = "Location";
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.blue)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Location;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentDescriptionView : AppCompatTextView, IAppointmentView
        {
            public AppointmentDescriptionView(Context context)
                : base(context)
            {
                Text = "Description";
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Description;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentOrganizerView : LinearLayoutCompat, IAppointmentView
        {
            public AppointmentOrganizerView(Context context)
                : base(context)
            {
                int largeSpacing = Conversion.ConvertDpToPixels(24f);
                int normalSpacing = Conversion.ConvertDpToPixels(8f);

                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                AppCompatTextView title = new AppCompatTextView(Context);
                title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                title.Text = "Organizer";
                title.Gravity = GravityFlags.Left;
                title.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f);
                AddView(title);

                AppCompatTextView label = new AppCompatTextView(Context);
                label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                label.Gravity = GravityFlags.Right;
                label.Text = "Label";

                label.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f);
                AddView(label);

                SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                //TODO:
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentCalendarView : LinearLayoutCompat, IAppointmentView
        {
            public AppointmentCalendarView(Context context)
                : base(context)
            {
                int largeSpacing = Conversion.ConvertDpToPixels(24f);
                int normalSpacing = Conversion.ConvertDpToPixels(8f);

                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                AppCompatTextView title = new AppCompatTextView(Context);
                title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                title.Text = "Calendar";
                title.Gravity = GravityFlags.Left;
                title.LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f);
                AddView(title);

                AppCompatTextView label = new AppCompatTextView(Context);
                label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                label.Gravity = GravityFlags.Right;
                label.Text = "Label";

                label.LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f);
                AddView(label);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                //TODO:
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentParticipantsView : LinearLayoutCompat, IAppointmentView
        {
            HeaderView headerView;

            public AppointmentParticipantsView(Context context)
                : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                headerView = new HeaderView(Context);
                headerView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                AddView(headerView);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                foreach (var participant in viewModel.Participants)
                {
                    ParticiapntView partView = new ParticiapntView(Context);
                    partView.LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
                    AddView(partView);
                }

                headerView.Refresh(viewModel);
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }

            private class HeaderView : LinearLayoutCompat, IAppointmentView
            {
                public HeaderView(Context context) : base(context)
                {
                    Orientation = Horizontal;
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                    AppCompatTextView title = new AppCompatTextView(Context);
                    title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                    title.Text = "Participants";
                    title.Gravity = GravityFlags.Left;
                    title.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f);
                    AddView(title);

                    AppCompatTextView label = new AppCompatTextView(Context);
                    label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                    label.Gravity = GravityFlags.Right;
                    label.Text = "3";

                    label.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.9f);
                    AddView(label);

                    AppCompatImageButton appCompatImageButton = new AppCompatImageButton(Context);

                    appCompatImageButton.SetImageResource(Resource.Drawable.arrow_right);

                    var layoutParams = new LayoutParams(Conversion.ConvertDpToPixels(10), Conversion.ConvertDpToPixels(16), 0.1f)
                    {
                        Gravity = (int)GravityFlags.CenterVertical | (int)GravityFlags.Right
                    };

                    appCompatImageButton.LayoutParameters = layoutParams;
                    appCompatImageButton.Clickable = false;

                    AddView(appCompatImageButton);
                    SetPadding(0, 0, 0, Conversion.ConvertDpToPixels(8f));
                }

                public void Refresh(AppointmentViewModel viewModel)
                {
                    //TODO:
                }

                public void SetViewPadding(int left, int top, int right, int bottom)
                {
                    // we dont need this..
                }
            }

            private class ParticiapntView : LinearLayoutCompat, IAppointmentView
            {
                public ParticiapntView(Context context) : base(context)
                {
                    Orientation = Horizontal;
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                    AppCompatImageButton appCompatImageButton = new AppCompatImageButton(Context);

                    appCompatImageButton.SetImageResource(Resource.Drawable.arrow_right);

                    var layoutParams = new LayoutParams(Conversion.ConvertDpToPixels(16), Conversion.ConvertDpToPixels(16), 0.2f)
                    {
                        Gravity = (int)GravityFlags.CenterVertical | (int)GravityFlags.Left
                    };

                    appCompatImageButton.LayoutParameters = layoutParams;
                    appCompatImageButton.Clickable = false;

                    AddView(appCompatImageButton);

                    AppCompatTextView title = new AppCompatTextView(Context);
                    title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                    title.Text = "Title";
                    title.TextSize = 11f;
                    title.Gravity = GravityFlags.Left;
                    title.LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.8f);
                    AddView(title);
                    SetPadding(0, Conversion.ConvertDpToPixels(8f), 0, Conversion.ConvertDpToPixels(8f));
                }

                public void Refresh(AppointmentViewModel viewModel)
                {
                    //TODO:
                }

                public void SetViewPadding(int left, int top, int right, int bottom)
                {
                    // we dont need this..
                }
            }
        }
        #endregion
    }
}