using UIKit;
using Foundation;
using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Ui.TableViewCells;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class AppointmentViewController : AbstractViewController, IAppointmentView
    {
        readonly int calendarId, appointmentId, recurrenceIndex;
        readonly AppointmentPresenter presenter;

        bool loaded;
        bool showActions;

        List<LineViewModel> lineViewModels;

        UIBarButtonItem editButtonItem;
        UIBarButtonItem deleteButtonItem;
        UIBarButtonItem closeButtonItem;

        UIStackView stackView;
        AppointmentSubjectView subjectView;
        AppointmentDateView dateView;
        AppointmentLocationView locationView;
        AppointmentDescriptionView descriptionView;
        AppointmentOrganizerView organizerView;
        AppointmentCalendarView calendarView;
        AppointmentParticipantsView participantsView;
        AppointmentReminderView reminderView;

        Action progressDialogDismissal;

        AppointmentViewModel appointment;

        public AppointmentViewController(int calendarId, int appointmentId, int recurrenceIndex, bool showActions = true)
        {
            this.appointmentId = appointmentId;
            this.calendarId = calendarId;
            this.recurrenceIndex = recurrenceIndex;
            this.showActions = showActions;
            presenter = new AppointmentPresenter();
            presenter.AttachView(this);
            presenter.Start();
        }

        public override void LoadView()
        {
            base.LoadView();

            InitView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;
            InitNavigationBar();
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = false;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            if (!loaded)
            {
                await presenter.LoadAppointment(appointmentId, recurrenceIndex, calendarId);
                loaded = true;
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            DeinitializeHandlers();
        }

        protected override void Recycle()
        {
            base.Recycle();
            deleteButtonItem = null;
            editButtonItem = null;

            presenter?.Stop();
        }

        private void InitializeHandlers()
        {
            if (deleteButtonItem != null)
                deleteButtonItem.Clicked += DeleteButtonItem_Clicked;

            if (editButtonItem != null)
                editButtonItem.Clicked += EditButtinItem_Clicked;

            if (closeButtonItem != null)
                closeButtonItem.Clicked += CloseButtonItem_Clicked;

            if (participantsView != null)
            {
                participantsView.SendInvitationClicked += SendInvitationsButton_TouchUpInside;
                participantsView.ShowParticipantsClicked += ParticipantsView_ShowParticipantsClicked;
            }
        }

        private void DeinitializeHandlers()
        {
            if (deleteButtonItem != null)
                deleteButtonItem.Clicked -= DeleteButtonItem_Clicked;

            if (editButtonItem != null)
                editButtonItem.Clicked -= EditButtinItem_Clicked;

            if (closeButtonItem != null)
                closeButtonItem.Clicked -= CloseButtonItem_Clicked;

            if (participantsView != null)
            {
                participantsView.SendInvitationClicked -= SendInvitationsButton_TouchUpInside;
                participantsView.ShowParticipantsClicked -= ParticipantsView_ShowParticipantsClicked;
            }
        }

        private void InitNavigationBar()
        {
            if (showActions)
            {
                deleteButtonItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle("Bin").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                };

                editButtonItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle("Edit").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                };

                NavigationItem.SetRightBarButtonItems(new[] { editButtonItem, deleteButtonItem }, false);
            }
            else
            {
                closeButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Close);
                NavigationItem.SetRightBarButtonItem(closeButtonItem, false);
            }
        }

        private void InitView()
        {
            UIScrollView scrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.GroupTableViewBackgroundColor,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
            };

            View.AddSubview(scrollView);

            View.AddConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            stackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                LayoutMargins = new UIEdgeInsets(10f, 15f, 15f, 15f),
                LayoutMarginsRelativeArrangement = true,
                Spacing = 8f,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Alpha = 0,
            };

            scrollView.AddSubview(stackView);

            scrollView.AddConstraints(new[]
            {
                    stackView.LeftAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.LeftAnchor),
                    stackView.TopAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.TopAnchor),
                    stackView.RightAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.RightAnchor),
                    stackView.WidthAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.WidthAnchor)
            });

            subjectView = new AppointmentSubjectView();
            dateView = new AppointmentDateView();
            locationView = new AppointmentLocationView();
            descriptionView = new AppointmentDescriptionView();
            organizerView = new AppointmentOrganizerView();
            calendarView = new AppointmentCalendarView();
            participantsView = new AppointmentParticipantsView();
            reminderView = new AppointmentReminderView();

            locationView.LocationClicked += LocationView_LocationClicked;

            stackView.AddArrangedSubview(subjectView);
            stackView.AddArrangedSubview(dateView);

            stackView.AddArrangedSubview(new SpacerView());
            stackView.AddArrangedSubview(locationView);
            stackView.AddArrangedSubview(descriptionView);

            stackView.AddArrangedSubview(new SpacerView());
            stackView.AddArrangedSubview(new SeparatorView());

            stackView.AddArrangedSubview(organizerView);
            stackView.AddArrangedSubview(calendarView);
            stackView.AddArrangedSubview(reminderView);
            stackView.AddArrangedSubview(participantsView);
        }

        #region IAppointmentView implementation

        public void CloseView()
        {
            NavigationController.PopViewController(true);
        }

        public void OpenEditAppointment(int calendarId, int appointmentId)
        {
            NavigationController.PushViewController(new EditAppointmentViewController(appointmentId, calendarId), true);
        }

        public void SetLines(IEnumerable<LineViewModel> lines)
        {
            lineViewModels = lines.ToList();
        }

        public void ShowAppointment(AppointmentViewModel appointment)
        {
            subjectView.Refresh(appointment);
            dateView.Refresh(appointment);
            locationView.Refresh(appointment);
            descriptionView.Refresh(appointment);
            organizerView.Refresh(appointment);
            calendarView.Refresh(appointment);
            participantsView.Refresh(appointment);
            reminderView.Refresh(appointment);

            UIView.Animate(0.05, () => { stackView.Alpha = 1; });

            this.appointment = appointment;
        }

        public void UpdateParticipants(List<ParticipantsViewModel> participants)
        {
            participantsView.Update(participants);
        }

        public async Task ShowDeleteError(Exception ex)
        {
            await Dialogs.ShowErrorAlertAsync(this, ex);
        }

        public async Task ShowLoadError(Exception ex)
        {
            await Dialogs.ShowErrorAlertAsync(this, ex);
        }

        public async Task ShowSendInvitationError(Exception ex)
        {
            await Dialogs.ShowErrorAlertAsync(this, ex);
        }

        public void ShowLoading()
        {
            progressDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_appointments___"));
        }

        public void CloseDialog()
        {
            progressDialogDismissal?.Invoke();
        }

        public void ShowAppointmentLoadingDialog()
        {
            progressDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_appointments___"));
        }

        public void ShowDeletingDialog()
        {
            progressDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));
        }

        public void ShowSendInvitationsDialog()
        {
            progressDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("sending_inviations__"));
        }

        #endregion

        #region Event handlers

        private void EditButtinItem_Clicked(object sender, EventArgs e)
        {
            presenter.EditAppointmentClicked();
        }

        private void CloseButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        private async void DeleteButtonItem_Clicked(object sender, EventArgs e)
        {
            var d = new PopoverPresentationControllerDelegate(deleteButtonItem);

            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete"), d);
            if (result)
                await presenter.DeleteAppointmentClicked();
        }

        async void SendInvitationsButton_TouchUpInside(object sender, EventArgs e)
        {
            var lineNames = lineViewModels.Select(l => l.Name).ToArray();

            var result = await Dialogs.ShowListActionSheetWithTitleAsync(this, lineNames, (UIView)sender, Localization.GetString("select_line"));

            if (result >= 0)
                await presenter.SendInvitationsClicked(lineViewModels[result]);
        }

        private void ParticipantsView_ShowParticipantsClicked(object sender, EventArgs e)
        {
            PresentViewController(new NavigationController(new ParticipantsViewController(appointment.Participants), UIModalPresentationStyle.PageSheet), true, null);
        }

        private void LocationView_LocationClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(appointment.Location))
                Integration.ShowOnMap(this, (UIView)sender, appointment.Location);
        }

        #endregion

        #region Subviews

        private interface IAppointmentView
        {
            void Refresh(AppointmentViewModel viewModel);
        }

        private class AppointmentOrganizerView : UIStackView, IAppointmentView
        {
            UILabel label;

            public AppointmentOrganizerView()
            {
                InitView();
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                label.Text = viewModel.Creator;
            }

            void InitView()
            {
                Opaque = false;
                TranslatesAutoresizingMaskIntoConstraints = false;
                Spacing = 10f;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Axis = UILayoutConstraintAxis.Vertical;

                var internalStackView = new UIStackView()
                {
                    Axis = UILayoutConstraintAxis.Horizontal,
                    Alignment = UIStackViewAlignment.Fill,
                    Distribution = UIStackViewDistribution.Fill,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                UILabel title = new UILabel()
                {
                    Text = "Organizer",
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    LineBreakMode = UILineBreakMode.WordWrap,
                    Lines = 0,
                    Font = Theme.AppointmentDefaultFont,
                    TextColor = Theme.DarkGray,
                };

                label = new UILabel()
                {
                    Text = "",
                    TextAlignment = UITextAlignment.Right,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    LineBreakMode = UILineBreakMode.WordWrap,
                    Lines = 0,
                    Font = Theme.AppointmentDefaultFont,
                    TextColor = Theme.Black
                };

                internalStackView.AddArrangedSubview(title);
                internalStackView.AddArrangedSubview(label);

                AddArrangedSubview(internalStackView);
                AddArrangedSubview(new SeparatorView());
            }
        }

        private class AppointmentReminderView : UIStackView, IAppointmentView
        {
            UILabel label;

            public AppointmentReminderView()
            {
                InitView();
            }

            void InitView()
            {
                Opaque = false;
                TranslatesAutoresizingMaskIntoConstraints = false;
                Spacing = 10f;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Axis = UILayoutConstraintAxis.Vertical;

                var internalStackView = new UIStackView()
                {
                    Axis = UILayoutConstraintAxis.Horizontal,
                    Alignment = UIStackViewAlignment.Fill,
                    Distribution = UIStackViewDistribution.Fill,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                UILabel title = new UILabel()
                {
                    Text = "Reminder",
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    LineBreakMode = UILineBreakMode.WordWrap,
                    Font = Theme.AppointmentDefaultFont,
                    TextColor = Theme.DarkGray,
                };

                label = new UILabel()
                {
                    TextAlignment = UITextAlignment.Right,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    LineBreakMode = UILineBreakMode.WordWrap,
                    Font = Theme.AppointmentDefaultFont,
                    TextColor = Theme.Black
                };

                internalStackView.AddArrangedSubview(title);
                internalStackView.AddArrangedSubview(label);

                AddArrangedSubview(internalStackView);
                AddArrangedSubview(new SeparatorView());
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (viewModel.ReminderTimeBefore < 0)
                {
                    Hidden = true;
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

        private class AppointmentSubjectView : UILabel, IAppointmentView
        {
            public AppointmentSubjectView()
            {
                Font = Theme.AppointmentTitleFont;
                TextColor = Theme.DarkerBlue;
                LineBreakMode = UILineBreakMode.WordWrap;
                Lines = 0;
                BackgroundColor = Theme.Clear;
                TextAlignment = UITextAlignment.Left;
                TranslatesAutoresizingMaskIntoConstraints = false;
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Subject;
            }
        }

        private class AppointmentDateView : UILabel, IAppointmentView
        {
            public AppointmentDateView()
            {
                Font = Theme.AppointmentDefaultFont;
                TextColor = Theme.DarkGray;
                LineBreakMode = UILineBreakMode.WordWrap;
                Lines = 0;
                BackgroundColor = Theme.Clear;
                TextAlignment = UITextAlignment.Left;
                TranslatesAutoresizingMaskIntoConstraints = false;
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                var culture = CultureInfo.InvariantCulture;

                if (viewModel.Start.Date.CompareTo(viewModel.End.Date) == 0)
                {
                    Text += viewModel.Start.ToString("dddd, d MMMM yyyy", culture);
                    if (viewModel.AllDay)
                        Text += "\r\nAll Day";
                    else
                        Text += $"\r\nfrom { viewModel.Start.ToString("hh:mm tt", culture) } to { viewModel.End.ToString("hh:mm tt", culture) }";
                }
                else
                {
                    if (viewModel.AllDay)
                    {
                        Text = $"All day from { viewModel.Start.ToString("ddd, d MMMM yyyy", culture) } ";
                        Text += $"\r\nto { viewModel.End.ToString("ddd, d MMMM yyyy", culture) }";
                    }
                    else
                    {
                        Text = $"from { viewModel.Start.ToString("hh:mm tt ddd, d MMMM yyyy", culture) } ";
                        Text += $"\r\nto { viewModel.End.ToString("hh:mm tt ddd, d MMMM yyyy", culture) }";
                    }
                }

                if (viewModel.RecurrenceInfo != null)
                {
                    Text += $"\n{viewModel.RecurrenceInfo}";
                }
            }
        }

        private class AppointmentLocationView : UILabel, IAppointmentView
        {
            public event EventHandler LocationClicked = delegate { };

            public AppointmentLocationView()
            {
                Font = Theme.AppointmentDefaultFont;
                TextColor = Theme.DarkBlue;
                LineBreakMode = UILineBreakMode.WordWrap;
                TextAlignment = UITextAlignment.Left;
                Lines = 0;
                TranslatesAutoresizingMaskIntoConstraints = false;
                UserInteractionEnabled = true;

                AddGestureRecognizer(new UITapGestureRecognizer(OpenAddress));
            }

            public void OpenAddress()
            {
                LocationClicked(this, EventArgs.Empty);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (string.IsNullOrEmpty(viewModel.Location))
                {
                    Hidden = true;
                    return;
                }

                Text = viewModel.Location;
            }
        }

        private class AppointmentDescriptionView : UITextView, IAppointmentView
        {
            public AppointmentDescriptionView()
            {
                Font = Theme.AppointmentDefaultFont;
                TextColor = Theme.Black;
                DataDetectorTypes = UIDataDetectorType.Link | UIDataDetectorType.Address | UIDataDetectorType.PhoneNumber;
                TextAlignment = UITextAlignment.Left;
                ScrollEnabled = false;
                Editable = false;
                BackgroundColor = UIColor.Clear;
                TextContainerInset = UIEdgeInsets.Zero;
                TextContainer.LineFragmentPadding = 0;
                TranslatesAutoresizingMaskIntoConstraints = false;
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (string.IsNullOrEmpty(viewModel.Description))
                {
                    Hidden = true;
                    return;
                }

                Text = viewModel.Description;
            }
        }

        class AppointmentCalendarView : UIStackView, IAppointmentView
        {
            UIView colorView;
            UILabel label;
            UILabel title;

            public AppointmentCalendarView()
            {
                InitView();
            }

            void InitView()
            {
                Opaque = false;
                TranslatesAutoresizingMaskIntoConstraints = false;
                Spacing = 10f;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Axis = UILayoutConstraintAxis.Vertical;

                var internalView = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                colorView = new UIView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                colorView.BackgroundColor = Theme.Black;
                colorView.Layer.CornerRadius = 5;

                title = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = "Calendar",
                    TextAlignment = UITextAlignment.Left,
                    Lines = 1,
                    Font = Theme.AppointmentDefaultFont,
                    TextColor = Theme.DarkGray,
                };

                label = new UILabel
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = Theme.AppointmentDefaultFont,
                    Lines = 1,
                    Text = ""
                };

                internalView.AddSubview(title);
                internalView.AddSubview(colorView);
                internalView.AddSubview(label);

                internalView.AddConstraints(new[]
                {
                    title.LeadingAnchor.ConstraintEqualTo(internalView.LeadingAnchor),
                    title.TopAnchor.ConstraintEqualTo(internalView.TopAnchor),
                    title.BottomAnchor.ConstraintEqualTo(internalView.BottomAnchor),

                    label.LeadingAnchor.ConstraintEqualTo(colorView.TrailingAnchor, 8f),
                    label.TrailingAnchor.ConstraintEqualTo(internalView.TrailingAnchor),
                    label.TopAnchor.ConstraintEqualTo(internalView.TopAnchor),
                    label.BottomAnchor.ConstraintEqualTo(internalView.BottomAnchor),

                    colorView.CenterYAnchor.ConstraintEqualTo(label.CenterYAnchor),
                    colorView.WidthAnchor.ConstraintEqualTo(10f),
                    colorView.HeightAnchor.ConstraintEqualTo(10f),
                });

                AddArrangedSubview(internalView);
                AddArrangedSubview(new SeparatorView());
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (viewModel.Calendar != null)
                {
                    label.Text = viewModel.Calendar.Name;
                    colorView.BackgroundColor = UI.UIColorFromHexString(viewModel.Calendar.HexColor);
                }
            }
        }

        private class AppointmentParticipantsView : UIStackView, IAppointmentView
        {
            readonly Header participantHeader;
            readonly SendInvitationsButton sendInvitationsButton;

            public EventHandler SendInvitationClicked = delegate { };
            public EventHandler ShowParticipantsClicked = delegate { };

            public AppointmentParticipantsView()
            {
                Opaque = false;
                Axis = UILayoutConstraintAxis.Vertical;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Spacing = 10f;
                TranslatesAutoresizingMaskIntoConstraints = false;

                sendInvitationsButton = new SendInvitationsButton();
                sendInvitationsButton.TouchUpInside += SendInvitationsButton_TouchUp;

                participantHeader = new Header();
                participantHeader.AddGestureRecognizer(new UITapGestureRecognizer(ShowParticipantsTapped));
                AddArrangedSubview(participantHeader);
            }

            private void SendInvitationsButton_TouchUp(object sender, EventArgs e)
            {
                SendInvitationClicked.Invoke(sender, e);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Update(viewModel.Participants);
            }

            public void Update(List<ParticipantsViewModel> participants)
            {
                foreach (var v in ArrangedSubviews.OfType<ParticipantView>())
                    v.RemoveFromSuperview();

                foreach (var participant in participants.Take(4))
                {
                    ParticipantView participantView = new ParticipantView();
                    participantView.Refresh(participant);
                    AddArrangedSubview(participantView);
                }

                participantHeader.Update(participants);

                if (participants.Any())
                    AddArrangedSubview(sendInvitationsButton);
            }

            void ShowParticipantsTapped(UITapGestureRecognizer a)
            {
                ShowParticipantsClicked(this, EventArgs.Empty);
            }

            class SendInvitationsButton : UIButton
            {
                public SendInvitationsButton()
                {
                    SetTitle("Send Invitations", UIControlState.Normal);
                    SetTitleColor(Theme.DarkerBlue, UIControlState.Normal);
                    SetTitleColor(Theme.DarkGray, UIControlState.Highlighted);
                }
            }

            class Header : UIView, IAppointmentView
            {
                UIImageView arrowImage;
                UILabel countLabel;
                UILabel title;

                public Header()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false;
                    InitView();
                }

                public void Refresh(AppointmentViewModel viewModel)
                {
                    Update(viewModel.Participants);
                }

                public void Update(List<ParticipantsViewModel> participants)
                {
                    int count = participants.Count;

                    if (count == 0)
                    {
                        Hidden = true;
                        return;
                    }
                    countLabel.Text = $"{count}";
                }

                void InitView()
                {
                    arrowImage = new UIImageView
                    {
                        Image = UIImage.FromBundle("Arrow-Expand").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                        TintColor = Theme.DarkBlue,
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = false
                    };

                    AddSubview(arrowImage);

                    title = new UILabel()
                    {
                        Text = "Participants",
                        TextAlignment = UITextAlignment.Left,
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        LineBreakMode = UILineBreakMode.WordWrap,
                        Lines = 0,
                        Font = Theme.AppointmentDefaultFont,
                        TextColor = Theme.DarkGray,
                    };

                    AddSubview(title);

                    countLabel = new UILabel
                    {
                        Font = Theme.AppointmentDefaultFont,
                        TextColor = Theme.Black,
                        Lines = 1,
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        TextAlignment = UITextAlignment.Right,
                        Text = ""
                    };

                    AddSubview(countLabel);

                    AddConstraints(new[]
                    {
                        title.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                        title.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                        title.BottomAnchor.ConstraintEqualTo(BottomAnchor),

                        arrowImage.TrailingAnchor.ConstraintEqualTo(TrailingAnchor),
                        arrowImage.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                        arrowImage.BottomAnchor.ConstraintEqualTo(BottomAnchor),

                        countLabel.TrailingAnchor.ConstraintEqualTo(arrowImage.LeadingAnchor, -10f),
                        countLabel.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
                        countLabel.CenterYAnchor.ConstraintEqualTo(CenterYAnchor)
                    });
                }
            }

            class ParticipantView : UIView
            {
                UILabel label;
                UIImageView statusImage;

                public ParticipantView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false;
                    InitView();
                }

                public void Refresh(ParticipantsViewModel viewModel)
                {
                    if (string.IsNullOrEmpty(viewModel.Name) || string.IsNullOrEmpty(viewModel.Email))
                        label.Text = viewModel.Name + viewModel.Email;
                    else
                        label.Text = $"{viewModel.Name} <{viewModel.Email}>";

                    if (viewModel.Status == Mobile.Common.Model.ParticipantStatus.Accepted)
                        statusImage.Image = UIImage.FromBundle("Participant-Accepted").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    else if (viewModel.Status == Mobile.Common.Model.ParticipantStatus.Declined)
                        statusImage.Image = UIImage.FromBundle("Participant-Declined").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    else
                        statusImage.Image = UIImage.FromBundle("Participant-Unknown").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                }

                void InitView()
                {
                    Opaque = false;
                    TranslatesAutoresizingMaskIntoConstraints = false;

                    statusImage = new UIImageView
                    {
                        TintColor = Theme.DarkGray,
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = false,
                    };

                    AddSubview(statusImage);

                    label = new UILabel()
                    {
                        Text = "",
                        TextAlignment = UITextAlignment.Left,
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        LineBreakMode = UILineBreakMode.WordWrap,
                        Lines = 0,
                        Font = Theme.CalendarTimeLightFont,
                        TextColor = Theme.DarkGray,
                    };

                    AddSubview(label);

                    AddConstraints(new[]
                    {
                        statusImage.LeadingAnchor.ConstraintEqualTo(LeadingAnchor, 5f),
                        statusImage.WidthAnchor.ConstraintEqualTo(18f),
                        statusImage.HeightAnchor.ConstraintEqualTo(statusImage.WidthAnchor),
                        statusImage.CenterYAnchor.ConstraintEqualTo(label.CenterYAnchor),

                        label.LeadingAnchor.ConstraintEqualTo(statusImage.TrailingAnchor, 8f),
                        label.TopAnchor.ConstraintEqualTo(TopAnchor, 2f),
                        label.BottomAnchor.ConstraintEqualTo(BottomAnchor, -2f),
                        label.HeightAnchor.ConstraintGreaterThanOrEqualTo(16f)
                    });
                }
            }
        }

        private class SeparatorView : UIView
        {
            public SeparatorView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;
                BackgroundColor = new UITableView().SeparatorColor;
                HeightAnchor.ConstraintEqualTo(0.5f).Active = true;
            }
        }

        private class SpacerView : UIView
        {
            public SpacerView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;
                BackgroundColor = Theme.Clear;
                HeightAnchor.ConstraintEqualTo(10f).Active = true;
            }
        }

        #endregion

    }

    public class ParticipantsViewController : AbstractTableViewController
    {
        readonly List<ParticipantsViewModel> participants;

        UIBarButtonItem doneItem;

        public ParticipantsViewController(List<ParticipantsViewModel> participants)
        {
            this.participants = participants;
        }

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)TableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            doneItem = null;

            ((DataSource)TableView.Source)?.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitializeNavigationBar()
        {
            Title = Localization.GetString("participants");

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(doneItem, false);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(TableView);
            TableView.AllowsSelection = true;
            TableView.AllowsMultipleSelection = true;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
        }

        void InitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked += DoneItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked -= DoneItem_Clicked;
        }

        void DoneItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        void RefreshData()
        {
            ((DataSource)TableView.Source).SetItems(participants);
        }

        class DataSource : UITableViewSource
        {
            List<ParticipantsViewModel> items = new List<ParticipantsViewModel>();
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool Empty => (items == null || items.Count == 0);

            public DataSource(UITableView tableView)
            {
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var c = items[indexPath.Row];

                var cell = tableView.DequeueReusableCell(ParticipantsTableViewCell.DefaultId) as ParticipantsTableViewCell ?? new ParticipantsTableViewCell();
                cell.Initialize(c);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (Empty)
                    return 0;

                return items.Count;
            }

            public void SetItems(List<ParticipantsViewModel> participants)
            {
                items.Clear();
                items.AddRange(participants.OrderBy(c => c.Name));
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }
    }
}