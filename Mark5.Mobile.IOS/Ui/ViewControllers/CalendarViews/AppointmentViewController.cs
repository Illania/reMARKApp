using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class AppointmentViewController : AbstractViewController, IAppointmentView
    {
        readonly int calendarId, appointmentId, recurrenceIndex;
        readonly AppointmentPresenter presenter;

        bool loaded;

        List<LineViewModel> lineViewModels;

        UIBarButtonItem editButtinItem;
        UIBarButtonItem deleteButtonItem;

        AppointmentSubjectView subjectView;
        AppointmentDateView dateView;
        AppointmentLocationView locationView;
        AppointmentDescriptionView descriptionView;
        AppointmentOrganizerView organizerView;
        AppointmentCalendarView calendarView;
        AppointmentParticipantsView participantsView;
        AppointmentReminderView reminderView;
        SendInvitationsButton sendInvitationsButton;

        Action progressDialogDismissal;

        public AppointmentViewController(int calendarId, int appointmentId, int recurrenceIndex)
        {
            this.appointmentId = appointmentId;
            this.calendarId = calendarId;
            this.recurrenceIndex = recurrenceIndex;
            presenter = new AppointmentPresenter();
            presenter.AttachView(this);
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
            editButtinItem = null;
        }

        private void InitializeHandlers()
        {
            if (deleteButtonItem != null)
                deleteButtonItem.Clicked += DeleteButtonItem_Clicked;

            if (editButtinItem != null)
                editButtinItem.Clicked += EditButtinItem_Clicked;

            if (sendInvitationsButton != null)
                sendInvitationsButton.TouchUpInside += SendInvitationsButton_TouchUpInside;
        }

        private void DeinitializeHandlers()
        {
            if (deleteButtonItem != null)
                deleteButtonItem.Clicked -= DeleteButtonItem_Clicked;

            if (editButtinItem != null)
                editButtinItem.Clicked -= EditButtinItem_Clicked;

            if (sendInvitationsButton != null)
                sendInvitationsButton.TouchUpInside -= SendInvitationsButton_TouchUpInside;
        }

        private void InitNavigationBar()
        {
            deleteButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Bin").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
            };

            editButtinItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Edit").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
            };

            NavigationItem.SetRightBarButtonItems(new[] { editButtinItem, deleteButtonItem }, false);
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

            UIStackView stackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                LayoutMargins = new UIEdgeInsets(10f, 15f, 15f, 15f),
                LayoutMarginsRelativeArrangement = true,
                Spacing = 10f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            scrollView.AddSubview(stackView);

            if (Integration.IsIPad())
            {
                scrollView.AddConstraints(new[]
                {
                    stackView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor),
                    stackView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor),
                    stackView.CenterXAnchor.ConstraintEqualTo(scrollView.CenterXAnchor),
                    stackView.WidthAnchor.ConstraintEqualTo(500f)
                });
            }
            else
            {
                scrollView.AddConstraints(new[]
                {
                    stackView.LeftAnchor.ConstraintEqualTo(scrollView.LeftAnchor),
                    stackView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor),
                    stackView.RightAnchor.ConstraintEqualTo(scrollView.RightAnchor),
                    stackView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor),
                    stackView.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor)
               });
            }

            subjectView = new AppointmentSubjectView();
            dateView = new AppointmentDateView();
            locationView = new AppointmentLocationView();
            descriptionView = new AppointmentDescriptionView();
            organizerView = new AppointmentOrganizerView();
            calendarView = new AppointmentCalendarView();
            participantsView = new AppointmentParticipantsView();
            sendInvitationsButton = new SendInvitationsButton();
            reminderView = new AppointmentReminderView();

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
            stackView.AddArrangedSubview(sendInvitationsButton);
        }

        #region IAppointmentView implementation

        public void CloseView()
        {
            NavigationController.PopViewController(true);
        }

        public void OpenEditAppointment(int calendarId, int appointmentId)
        {
            presenter.EditAppointmentClicked();
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
            reminderView.Refresh(appointment); //TODO I think we should just add all those view to a collection to simplyify

            if (appointment.Participants.Count == 0)
                sendInvitationsButton.Hidden = true;
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
            progressDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting__"));
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

            var result = await Dialogs.ShowListActionSheetWithTitleAsync(this, lineNames, sendInvitationsButton, Localization.GetString("select_line"));

            if (result >= 0)
                await presenter.SendInvitationsClicked(lineViewModels[result]);
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
                    label.Text = "At time of event"; //TODO localization..?
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

                if (viewModel.RecurrenceInfo != null)
                {
                    Text += $"\n{viewModel.RecurrenceInfo}";
                }
            }
        }

        private class AppointmentLocationView : UILabel, IAppointmentView
        {
            public AppointmentLocationView()
            {
                Font = Theme.AppointmentDefaultFont;
                TextColor = Theme.DarkBlue;
                LineBreakMode = UILineBreakMode.WordWrap;
                TextAlignment = UITextAlignment.Left;
                TranslatesAutoresizingMaskIntoConstraints = false;
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

        private class AppointmentDescriptionView : UILabel, IAppointmentView
        {
            public AppointmentDescriptionView()
            {
                Font = Theme.AppointmentDefaultFont;
                TextColor = Theme.Black;
                LineBreakMode = UILineBreakMode.WordWrap;
                Lines = 0;
                TextAlignment = UITextAlignment.Left;
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
            Header participantHeader;

            public AppointmentParticipantsView()
            {
                Opaque = false;
                Axis = UILayoutConstraintAxis.Vertical;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Spacing = 10f;
                TranslatesAutoresizingMaskIntoConstraints = false;

                participantHeader = new Header();
                AddArrangedSubview(participantHeader);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                foreach (var participant in viewModel.Participants)
                {
                    ParticipantView participantView = new ParticipantView();
                    participantView.Refresh(participant);
                    AddArrangedSubview(participantView);
                }

                participantHeader.Refresh(viewModel);
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
                    int count = viewModel.Participants.Count;

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
                        //arrowImage.WidthAnchor.ConstraintEqualTo(10f),

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
                    label.Text = viewModel.Name + " " + viewModel.Email;
                }

                void InitView()
                {
                    Opaque = false;
                    TranslatesAutoresizingMaskIntoConstraints = false;

                    statusImage = new UIImageView
                    {
                        Image = UIImage.FromBundle("Add").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                        TintColor = Theme.DarkGray,
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = false
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
                        statusImage.TopAnchor.ConstraintEqualTo(TopAnchor, 2f),
                        statusImage.BottomAnchor.ConstraintEqualTo(BottomAnchor, -2f),

                        label.LeadingAnchor.ConstraintEqualTo(statusImage.TrailingAnchor, 8f),
                        label.TopAnchor.ConstraintEqualTo(TopAnchor, 2f),
                        label.BottomAnchor.ConstraintEqualTo(BottomAnchor, -2f),
                        label.HeightAnchor.ConstraintGreaterThanOrEqualTo(16f)
                    });
                }
            }
        }

        private class SendInvitationsButton : UIButton
        {
            public SendInvitationsButton()
            {
                SetTitle("Send Invitations", UIControlState.Normal);
                SetTitleColor(Theme.DarkerBlue, UIControlState.Normal);
                SetTitleColor(Theme.DarkGray, UIControlState.Highlighted);
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
}
