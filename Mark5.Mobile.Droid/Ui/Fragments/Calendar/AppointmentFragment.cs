using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Android.Support.V7.App;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class AppointmentFragment : BaseFragment, IAppointmentView
    {
        const string CalendarBundleKey = "Calendar_Id";
        const string AppointmentBundleKey = "Appointment_Id";
        const string ReocurrenceBundleKey = "Reocurrence_Id";

        readonly int largeSpacing = Conversion.ConvertDpToPixels(24f);
        readonly int normalSpacing = Conversion.ConvertDpToPixels(8f);

        int calendarId;
        int appointmentId;
        int recurrenceIndex;

        Action dismissLoadingAction;
        List<LineViewModel> lineViewModels;

        AppointmentSubjectView subjectView;
        AppointmentDateView dateView;
        AppointmentLocationView locationView;
        AppointmentDescriptionView descriptionView;
        AppointmentOrganizerView organizerView;
        AppointmentCalendarView calendarView;
        AppointmentReminderView reminderView;
        AppointmentPresenter presenter;
        AppointmentParticipantsView participantsView;

        LinearLayoutCompat containerLayout;
        List<View> subviews = new List<View>();

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

            presenter = new AppointmentPresenter();
            presenter.AttachView(this);
            presenter.Start();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AppointmentFragment)}");
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);
            rootView.SetBackgroundColor(Color.White);

            containerLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            subjectView = new AppointmentSubjectView(Context);
            dateView = new AppointmentDateView(Context);
            locationView = new AppointmentLocationView(Context);
            descriptionView = new AppointmentDescriptionView(Context);
            organizerView = new AppointmentOrganizerView(Context);
            calendarView = new AppointmentCalendarView(Context);
            reminderView = new AppointmentReminderView(Context);
            participantsView = new AppointmentParticipantsView(Context);

            participantsView.ShowParticipantsClicked += ParticipantsView_ShowParticipantsClicked;
            participantsView.SendInvitationClicked += SendInvitationsButton_TouchUpInside;

            foreach (var subview in subviews)
            {
                if (subview.Parent != null)
                    ((ViewGroup)subview.Parent).RemoveView(subview);
            }

            subviews = new List<View>();

            subviews.Add(subjectView);
            subviews.Add(dateView);
            subviews.Add(locationView);
            subviews.Add(descriptionView);
            subviews.Add(new SeparatorSubView(Context));
            subviews.Add(organizerView);
            subviews.Add(new SeparatorSubView(Context));
            subviews.Add(calendarView);
            subviews.Add(new SeparatorSubView(Context));
            //subviews.Add(reminderView);
            //subviews.Add(new SeparatorSubView(Context));
            subviews.Add(participantsView);

            foreach (var subview in subviews)
            {
                if (subview is SeparatorSubView)
                {
                    subview.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, Conversion.ConvertDpToPixels(1));
                }
                else
                {
                    subview.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
                    subview.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                }

                containerLayout.AddView(subview);
            }

            containerLayout.Alpha = 0;

            HasOptionsMenu = true;
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

        private void ParticipantsView_ShowParticipantsClicked(object sender, EventArgs e)
        {
            //TODO: implement
        }

        private async void SendInvitationsButton_TouchUpInside(object sender, EventArgs e)
        {
            var lineNames = lineViewModels.Select(l => l.Name).ToArray();
            var result = await Dialogs.ShowListDialog(Context, Resource.String.select_a_line, lineNames, true);
            if (result >= 0)
                await presenter.SendInvitationsClicked(lineViewModels[result]);
        }

        #region IAppointmentView implementation

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

        public void ShowAppointment(AppointmentViewModel appointment)
        {
            subviews.OfType<IAppointmentView>().ToList().ForEach(v => v.Refresh(appointment));

            //Fix separator
            for (int i = 0; i < subviews.Count - 1; i++)
            {
                var current = subviews[i];
                if (current is SeparatorSubView && subviews[i + 1].Visibility == ViewStates.Gone)
                    current.Visibility = ViewStates.Gone;
            }

            containerLayout.Animate().Alpha(1f).SetDuration(500);
        }

        public void ShowAppointmentLoadingDialog()
        {
            dismissLoadingAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_appointments, Resource.String.please_wait);
        }

        public void CloseDialog()
        {
            dismissLoadingAction?.Invoke();
        }

        public void UpdateParticipants(List<ParticipantsViewModel> participants)
        {
            participantsView?.Refresh(participants);
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

        #region Custom views

        private interface IAppointmentView
        {
            void Refresh(AppointmentViewModel viewModel);
        }

        class SeparatorSubView : View
        {
            public SeparatorSubView(Context c) : base(c)
            {
                SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
            }
        }

        class AppointmentSubjectView : AppCompatTextView, IAppointmentView
        {

            public AppointmentSubjectView(Context context)
                : base(context)
            {
                Text = "";
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                SetTextSize(Android.Util.ComplexUnitType.Sp, 22);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Subject;
            }
        }

        class AppointmentDateView : AppCompatTextView, IAppointmentView
        {
            public AppointmentDateView(Context context)
                : base(context)
            {
                Text = "";
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (viewModel.Start.Date.CompareTo(viewModel.End.Date) == 0)
                {
                    Text = viewModel.Start.ToString("dddd, d MMMM yyyy", CultureInfo.CurrentCulture);
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

                Text += $"\r\n{viewModel.RecurrenceInfo}";
            }
        }

        class AppointmentReocurrenceView : AppCompatTextView, IAppointmentView
        {

            public AppointmentReocurrenceView(Context context)
                : base(context)
            {
                Text = "";
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.RecurrenceInfo;
            }
        }

        class AppointmentLocationView : AppCompatTextView, IAppointmentView
        {

            public AppointmentLocationView(Context context)
                : base(context)
            {
                Text = "";
                Click += AppointmentLocationView_Click;
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.blue)));
            }

            private void AppointmentLocationView_Click(object sender, EventArgs e)
            {
                if (!string.IsNullOrEmpty(Text))
                    Integration.OpenMap(Context, Text);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (string.IsNullOrEmpty(viewModel.Location))
                    Visibility = ViewStates.Gone;
                Text = viewModel.Location;
            }
        }

        class AppointmentDescriptionView : AppCompatTextView, IAppointmentView
        {
            public AppointmentDescriptionView(Context context)
                : base(context)
            {
                Text = "";
                LinksClickable = true;
                AutoLinkMask = Android.Text.Util.MatchOptions.All;
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (string.IsNullOrEmpty(viewModel.Description))
                    Visibility = ViewStates.Gone;
                Text = viewModel.Description;
            }
        }

        class AppointmentOrganizerView : LinearLayoutCompat, IAppointmentView
        {
            readonly AppCompatTextView label;

            public AppointmentOrganizerView(Context context)
                : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                AppCompatTextView title = new AppCompatTextView(Context)
                {
                    Text = "Organizer",
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1)
                };
                title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                AddView(title);

                label = new AppCompatTextView(Context)
                {
                    Gravity = GravityFlags.Right,
                    Text = "",
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                };
                label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                AddView(label);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (string.IsNullOrEmpty(viewModel.Creator))
                    Visibility = ViewStates.Gone;
                label.Text = viewModel.Creator;
            }
        }

        class AppointmentReminderView : LinearLayoutCompat, IAppointmentView
        {
            readonly AppCompatTextView label;

            public AppointmentReminderView(Context context)
                : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                AppCompatTextView title = new AppCompatTextView(Context)
                {
                    Text = "Reminder",
                    Gravity = GravityFlags.Left,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f)
                };
                title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                AddView(title);

                label = new AppCompatTextView(Context)
                {
                    Gravity = GravityFlags.Right,
                    Text = "",
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f)
                };
                label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                AddView(label);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (viewModel.ReminderTimeBefore < 0)
                {
                    Visibility = ViewStates.Gone;
                    return;
                }

                if (viewModel.ReminderTimeBefore == 0)
                {
                    label.Text = "At time of event";
                    return;
                }

                var timeSpan = TimeSpan.FromSeconds(viewModel.ReminderTimeBefore);

                int weeks = (int)timeSpan.TotalDays / 7;
                var days = timeSpan.TotalDays;
                var hours = timeSpan.TotalHours;
                var minutes = timeSpan.TotalMinutes;

                if (weeks == 1)
                    label.Text = "1 week";
                else if (days >= 1)
                    label.Text = $"{days} day(s)";
                else if (hours >= 1)
                    label.Text = $"{hours} hour(s)";
                else if (minutes >= 1)
                    label.Text = $"{minutes} minute(s)";

                label.Text += " before";
            }
        }

        class AppointmentCalendarView : LinearLayoutCompat, IAppointmentView
        {
            readonly AppCompatTextView label;
            readonly View colorCircle;

            public AppointmentCalendarView(Context context)
                : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                AppCompatTextView title = new AppCompatTextView(Context)
                {
                    Text = "Calendar",
                    LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1)
                    {
                        Gravity = (int)GravityFlags.Left,
                    },
                };
                title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                AddView(title);

                colorCircle = new View(Context)
                {
                    LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(12), Conversion.ConvertDpToPixels(12))
                    {
                        Gravity = (int)GravityFlags.Right | (int)GravityFlags.CenterVertical,
                        RightMargin = Conversion.ConvertDpToPixels(5),
                    }
                };
                AddView(colorCircle);

                label = new AppCompatTextView(Context)
                {
                    Gravity = GravityFlags.Right,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (int)GravityFlags.Right,
                    },
                    Text = ""
                };
                label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                AddView(label);
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

            public void Refresh(AppointmentViewModel viewModel)
            {
                CalendarViewModel calendarViewModel = viewModel.Calendar;
                HexColor = calendarViewModel?.HexColor;
                label.Text = calendarViewModel?.Name;
            }
        }

        class AppointmentParticipantsView : LinearLayoutCompat, IAppointmentView
        {
            HeaderView headerView;
            SendInvitationsButton sendInvitationsButton;

            public EventHandler SendInvitationClicked = delegate { };
            public EventHandler ShowParticipantsClicked = delegate { };

            public AppointmentParticipantsView(Context context)
                : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Refresh(viewModel.Participants);
            }

            public void Refresh(List<ParticipantsViewModel> participants)
            {
                RemoveAllViews();

                headerView = new HeaderView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
                };
                AddView(headerView);

                headerView.Refresh(participants.Count);

                foreach (var participant in participants)
                {
                    ParticipantView partView = new ParticipantView(Context)
                    {
                        LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                    };
                    partView.Refresh(participant);
                    AddView(partView);
                }

                if (participants.Any())
                {
                    sendInvitationsButton = new SendInvitationsButton(Context);
                    sendInvitationsButton.Click += (sender, e) =>
                    {
                        SendInvitationClicked(sender, e);
                    };
                    AddView(sendInvitationsButton);
                }
            }

            private void HeaderView_Touch(object sender, TouchEventArgs e)
            {
                ShowParticipantsClicked(sender, e);
            }

            private class SendInvitationsButton : AppCompatButton
            {
                public SendInvitationsButton(Context context) : base(context)
                {
                    Text = "Send Invitations";
                    SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)));
                }
            }

            private class HeaderView : LinearLayoutCompat
            {
                readonly AppCompatTextView label;

                public HeaderView(Context context) : base(context)
                {
                    Orientation = Horizontal;
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                    AppCompatTextView title = new AppCompatTextView(Context)
                    {
                        Gravity = GravityFlags.Left | GravityFlags.CenterVertical,
                        LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f),
                        Text = "Participants"
                    };

                    title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                    AddView(title);

                    label = new AppCompatTextView(Context)
                    {
                        Gravity = GravityFlags.Right | GravityFlags.CenterVertical,
                        LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.9f),
                        Text = ""
                    };
                    label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                    AddView(label);

                    //AppCompatImageButton appCompatImageButton = new AppCompatImageButton(Context)  //TODO for now we do not show it, as it is not clickable
                    //{
                    //    LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(10), Conversion.ConvertDpToPixels(16), 0.1f)
                    //    {
                    //        Gravity = (int)GravityFlags.CenterVertical | (int)GravityFlags.Right,
                    //        BottomMargin = 5,
                    //        TopMargin = 5
                    //    },
                    //    Clickable = false
                    //};

                    //appCompatImageButton.SetImageResource(Resource.Drawable.arrow_right);
                    //AddView(appCompatImageButton);

                    SetPadding(0, 0, 0, Conversion.ConvertDpToPixels(8f));
                }

                public void Refresh(int count)
                {
                    label.Text = $"{count}";
                }
            }

            private class ParticipantView : LinearLayoutCompat
            {
                readonly AppCompatTextView label;
                readonly AppCompatImageView appCompatImageButton;

                public ParticipantView(Context context) : base(context)
                {
                    Orientation = Horizontal;
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                    appCompatImageButton = new AppCompatImageView(Context)
                    {
                        Clickable = false,
                        LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(16), Conversion.ConvertDpToPixels(16), 0.2f)
                        {
                            Gravity = (int)GravityFlags.CenterVertical | (int)GravityFlags.Left
                        }
                    };

                    appCompatImageButton.SetImageResource(Resource.Drawable.arrow_right);
                    appCompatImageButton.SetColorFilter(Color.Gray);
                    appCompatImageButton.SetPadding(0, 0, Conversion.ConvertDpToPixels(4f), 0);

                    AddView(appCompatImageButton);

                    label = new AppCompatTextView(Context)
                    {
                        Text = "",
                        TextSize = 11f,
                        Gravity = GravityFlags.Left,
                        LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.8f)
                    };

                    label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                    AddView(label);

                    SetPadding(0, Conversion.ConvertDpToPixels(8f), 0, Conversion.ConvertDpToPixels(8f));
                }

                public void Refresh(ParticipantsViewModel participant)
                {
                    if (string.IsNullOrEmpty(participant.Name) || string.IsNullOrEmpty(participant.Email))
                        label.Text = participant.Name + participant.Email;
                    else
                        label.Text = $"{participant.Name} <{participant.Email}>";

                    label.Text = $"{participant.Name} {participant.Email}";

                    switch (participant.Status)
                    {
                        case Mobile.Common.Model.ParticipantStatus.Accepted:
                            appCompatImageButton.SetImageResource(Resource.Drawable.icon_check);
                            break;
                        case Mobile.Common.Model.ParticipantStatus.Invited:
                        case Mobile.Common.Model.ParticipantStatus.NeedAction:
                            appCompatImageButton.SetImageResource(Resource.Drawable.icon_question);
                            break;
                        case Mobile.Common.Model.ParticipantStatus.Tentative:
                        case Mobile.Common.Model.ParticipantStatus.Declined:
                            appCompatImageButton.SetImageResource(Resource.Drawable.icon_cross);
                            break;
                    }
                }
            }
        }
        #endregion
    }
}