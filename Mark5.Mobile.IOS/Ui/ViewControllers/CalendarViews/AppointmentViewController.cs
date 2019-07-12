using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class AppointmentViewController : AbstractViewController, IAppointmentView
    {
        int calendarId, appointmentId, recurrenceIndex;
        bool loaded;

        UIBarButtonItem editButtinItem;
        UIBarButtonItem deleteButtonItem;

        AppointmentSubjectView subjectView;
        AppointmentDateView dateView;
        AppointmentReocurrenceView reocurrenceView;
        AppointmentLocationView locationView;
        AppointmentDescriptionView descriptionView;
        AppointmentOrganizerView organizerView;
        AppointmentCalendarView calendarView;
        AppointmentParticipantsView participantsView;
        SendInvitationsButton sendInvitationsButton;

        Action loadingDialogDismissal;
        AppointmentPresenter presenter;

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

        public void CloseView()
        {
            NavigationController.PopViewController(true);
        }

        public void OpenEditAppointment(int calendarId, int appointmentId)
        {
            // TODO
            //presenter.EditAppointmentClicked();
        }

        public void SetLines(IEnumerable<LineViewModel> lines)
        {
            //TODO
        }

        public void ShowAppointment(AppointmentViewModel appointment)
        {
            subjectView.Refresh(appointment);
            dateView.Refresh(appointment);
            reocurrenceView.Refresh(appointment);
            locationView.Refresh(appointment);
            descriptionView.Refresh(appointment);
            organizerView.Refresh(appointment);
            calendarView.Refresh(appointment);
            participantsView.Refresh(appointment);
        }

        public async Task ShowDeleteError()
        {
            loadingDialogDismissal?.Invoke();
            await Dialogs.ShowErrorAlertAsync(this, new Exception("ShowDeleteError"));
        }

        public async Task ShowLoadError()
        {
            loadingDialogDismissal?.Invoke();
            await Dialogs.ShowErrorAlertAsync(this, new Exception("ShowLoadError"));
        }

        public async Task ShowSendInvitationError()
        {
            loadingDialogDismissal?.Invoke();
            await Dialogs.ShowErrorAlertAsync(this, new Exception("ShowLoadError"));
        }

        public void ShowLoading()
        {
            loadingDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_appointments___"));
        }

        public void StopLoading()
        {
            loadingDialogDismissal?.Invoke();
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

        private void EditButtinItem_Clicked(object sender, EventArgs e)
        {
            presenter.EditAppointmentClicked();
        }

        private void DeleteButtonItem_Clicked(object sender, EventArgs e)
        {
            // TODO
            //_ = presenter.DeleteAppointmentClicked();
        }

        async void SendInvitationsButton_TouchUpInside(object sender, EventArgs e)
        {
            //TODO : come back
            //await presenter.SendInvitationClicked(new Guid());
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
                Distribution = UIStackViewDistribution.EqualSpacing,
                LayoutMargins = new UIEdgeInsets(10f, 10f, 10f, 10f),
                LayoutMarginsRelativeArrangement = true,
                Spacing = 15f,
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
            stackView.AddArrangedSubview(subjectView);

            dateView = new AppointmentDateView();
            stackView.AddArrangedSubview(dateView);

            reocurrenceView = new AppointmentReocurrenceView();
            stackView.AddArrangedSubview(reocurrenceView);

            locationView = new AppointmentLocationView();
            stackView.AddArrangedSubview(locationView);

            descriptionView = new AppointmentDescriptionView();
            stackView.AddArrangedSubview(descriptionView);

            organizerView = new AppointmentOrganizerView();
            stackView.AddArrangedSubview(organizerView);

            calendarView = new AppointmentCalendarView();
            stackView.AddArrangedSubview(calendarView);

            participantsView = new AppointmentParticipantsView();
            stackView.AddArrangedSubview(participantsView);

            sendInvitationsButton = new SendInvitationsButton();
            stackView.AddArrangedSubview(sendInvitationsButton);
        }

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
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Axis = UILayoutConstraintAxis.Horizontal;

                UILabel title = new UILabel()
                {
                    Text = "Organizer",
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    LineBreakMode = UILineBreakMode.WordWrap,
                    Lines = 0,
                    Font = Theme.DefaultFont,
                    TextColor = Theme.DarkGray,
                };

                AddArrangedSubview(title);

                label = new UILabel()
                {
                    Text = "",
                    TextAlignment = UITextAlignment.Right,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    LineBreakMode = UILineBreakMode.WordWrap,
                    Lines = 0,
                    Font = Theme.DefaultFont,
                    TextColor = Theme.Black
                };

                AddArrangedSubview(label);
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
                Text = "";
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
                Font = Theme.DefaultFont;
                TextColor = Theme.Black;
                LineBreakMode = UILineBreakMode.WordWrap;
                Lines = 0;
                BackgroundColor = Theme.Clear;
                TextAlignment = UITextAlignment.Left;
                TranslatesAutoresizingMaskIntoConstraints = false;
                Text = "";
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
        }

        private class AppointmentReocurrenceView : UILabel, IAppointmentView
        {
            public AppointmentReocurrenceView()
            {

                Font = Theme.DefaultLightFont;
                TextColor = Theme.DarkGray;
                LineBreakMode = UILineBreakMode.WordWrap;
                Lines = 0;
                Text = "";
                TextAlignment = UITextAlignment.Left;
                TranslatesAutoresizingMaskIntoConstraints = false;
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.RecurrenceInfo;
            }
        }

        private class AppointmentLocationView : UILabel, IAppointmentView
        {
            public AppointmentLocationView()
            {
                Font = Theme.DefaultFont;
                TextColor = Theme.DarkBlue;
                LineBreakMode = UILineBreakMode.WordWrap;
                Lines = 0;
                Text = "";
                TextAlignment = UITextAlignment.Left;
                TranslatesAutoresizingMaskIntoConstraints = false;
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Location;
            }
        }

        private class AppointmentDescriptionView : UILabel, IAppointmentView
        {
            public AppointmentDescriptionView()
            {

                Font = Theme.DefaultFont;
                TextColor = Theme.Black;
                LineBreakMode = UILineBreakMode.WordWrap;
                Lines = 0;
                Text = "";
                TextAlignment = UITextAlignment.Left;
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Description;
            }
        }

        private class AppointmentCalendarView : UIView, IAppointmentView
        {
            UIView colorView;
            UILabel lable;
            UILabel title;

            public AppointmentCalendarView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;
                InitView();
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (viewModel.Calendar != null)
                {
                    lable.Text = viewModel.Calendar.Name;
                    colorView.BackgroundColor = UI.UIColorFromHexString(viewModel.Calendar.HexColor);
                }
            }

            void InitView()
            {
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
                    Font = Theme.DefaultFont,
                    TextColor = Theme.DarkGray,
                };

                lable = new UILabel
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = Theme.DefaultFont,
                    Lines = 1,
                    Text = ""
                };

                AddSubview(title);
                AddSubview(colorView);
                AddSubview(lable);

                AddConstraints(new[]
                {
                    title.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                    title.TopAnchor.ConstraintEqualTo(TopAnchor),
                    title.BottomAnchor.ConstraintEqualTo(BottomAnchor),
                    title.HeightAnchor.ConstraintEqualTo(Theme.MinimumLabelSize),

                    lable.LeadingAnchor.ConstraintEqualTo(colorView.TrailingAnchor, 8f),
                    lable.TrailingAnchor.ConstraintEqualTo(TrailingAnchor),
                    lable.TopAnchor.ConstraintEqualTo(TopAnchor),
                    lable.BottomAnchor.ConstraintEqualTo(BottomAnchor),

                    colorView.CenterYAnchor.ConstraintEqualTo(lable.CenterYAnchor),
                    colorView.WidthAnchor.ConstraintEqualTo(10f),
                    colorView.HeightAnchor.ConstraintEqualTo(10f),
                });
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
                        Font = Theme.DefaultFont,
                        TextColor = Theme.DarkGray,
                    };

                    AddSubview(title);

                    countLabel = new UILabel
                    {
                        Font = Theme.DefaultFont,
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
                        arrowImage.WidthAnchor.ConstraintEqualTo(10f),

                        countLabel.TrailingAnchor.ConstraintEqualTo(arrowImage.LeadingAnchor, -10f),
                        countLabel.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
                        countLabel.CenterYAnchor.ConstraintEqualTo(CenterYAnchor)
                    });
                }
            }

            class ParticipantView : UIView
            {
                UILabel labe;
                UIImageView statusImage;

                public ParticipantView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false;
                    InitView();
                }

                public void Refresh(ParticipantsViewModel viewModel)
                {
                    labe.Text = viewModel.Name + " " + viewModel.Email;
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

                    labe = new UILabel()
                    {
                        Text = "",
                        TextAlignment = UITextAlignment.Left,
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        LineBreakMode = UILineBreakMode.WordWrap,
                        Lines = 0,
                        Font = Theme.CalendarTimeLightFont,
                        TextColor = Theme.DarkGray,
                    };

                    AddSubview(labe);

                    AddConstraints(new[]
                    {
                        statusImage.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                        statusImage.TopAnchor.ConstraintEqualTo(TopAnchor, 2f),
                        statusImage.BottomAnchor.ConstraintEqualTo(BottomAnchor, -2f),

                        labe.LeadingAnchor.ConstraintEqualTo(statusImage.TrailingAnchor, 8f),
                        labe.TopAnchor.ConstraintEqualTo(TopAnchor, 2f),
                        labe.BottomAnchor.ConstraintEqualTo(BottomAnchor, -2f),
                        labe.HeightAnchor.ConstraintGreaterThanOrEqualTo(16f)
                    });
                }
            }
        }

        private class SendInvitationsButton : UIButton
        {
            public SendInvitationsButton()
            {
                Font = Theme.DefaultLightFont;
                SetTitle("Send Invitations", UIControlState.Normal);
                SetTitleColor(Theme.DarkerBlue, UIControlState.Normal);
                SetTitleColor(Theme.DarkGray, UIControlState.Highlighted);
            }
        }

        private class SeperatorView : UIView
        {
            public SeperatorView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;
                BackgroundColor = Theme.DarkGray;
                HeightAnchor.ConstraintEqualTo(2f);
                AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            }
        }
    }
}
