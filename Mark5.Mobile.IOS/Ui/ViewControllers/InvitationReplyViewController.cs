using UIKit;
using System;
using Foundation;
using System.Linq;
using ObjCRuntime;
using System.Threading.Tasks;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class InvitationReplyDetailViewModel
    {
        public Line Line;
        public string Message;
        public ParticipantStatus Status;
    }

    public class InvitationReplyViewController : UIViewController
    {
        readonly TaskCompletionSource<InvitationReplyDetailViewModel> tcs = new TaskCompletionSource<InvitationReplyDetailViewModel>();
        readonly float InnerHorizontalMargine = 25f;
        readonly float InnerVerticalMargine = 20f;
        readonly InvitationReplyDetailViewModel model;
        readonly ParticipantStatus userStatus;

        UIView viewsContainer;
        UITextView messageView;
        UILabel lineLabel;

        ReplyButton acceptButton;
        ReplyButton tentativeButton;
        ReplyButton declineButton;

        public Task<InvitationReplyDetailViewModel> Result => tcs.Task;

        public InvitationReplyViewController(ParticipantStatus userStatus, Line predefinedLine)
        {
            this.userStatus = userStatus;
            model = new InvitationReplyDetailViewModel
            {
                Line = predefinedLine
            };
        }

        #region VC Lifecycle

        public override void LoadView()
        {
            base.LoadView();
            View.UserInteractionEnabled = true;
            View.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
            InitializeViews();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UpdateLine();
            UpdateButtons();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            messageView.Ended += MessageView_StartedEnded;
            messageView.Started += MessageView_StartedEnded;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            messageView.Ended -= MessageView_StartedEnded;
            messageView.Started -= MessageView_StartedEnded;
        }

        #endregion

        #region UI Initialization

        private void InitializeViews()
        {
            View.BackgroundColor = UIColor.Black.ColorWithAlpha(0.7f); //TODO check on ipad

            viewsContainer = InitializeContainer();

            View.AddSubview(viewsContainer);
            View.AddConstraints(new NSLayoutConstraint[]
            {
                viewsContainer.HeightAnchor.ConstraintGreaterThanOrEqualTo(200),
                viewsContainer.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                viewsContainer.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
                viewsContainer.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 20f),
                viewsContainer.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -20f)
            });

            UIView lineView = InitializeLineView();

            viewsContainer.AddSubview(lineView);
            viewsContainer.AddConstraints(new NSLayoutConstraint[]
            {
                lineView.LeadingAnchor.ConstraintEqualTo(viewsContainer.LeadingAnchor, InnerHorizontalMargine),
                lineView.TrailingAnchor.ConstraintEqualTo(viewsContainer.TrailingAnchor, -InnerHorizontalMargine),
                lineView.TopAnchor.ConstraintEqualTo(viewsContainer.TopAnchor, InnerVerticalMargine),
            });

            UIView buttonGridView = InitializeButtonGrid();

            viewsContainer.AddSubview(buttonGridView);
            viewsContainer.AddConstraints(new NSLayoutConstraint[]
            {
                buttonGridView.LeadingAnchor.ConstraintEqualTo(viewsContainer.LeadingAnchor),
                buttonGridView.TrailingAnchor.ConstraintEqualTo(viewsContainer.TrailingAnchor),
                buttonGridView.BottomAnchor.ConstraintEqualTo(viewsContainer.BottomAnchor, -InnerVerticalMargine),
                buttonGridView.HeightAnchor.ConstraintGreaterThanOrEqualTo(40f)
            });

            messageView = InitializeMessageView();

            viewsContainer.AddSubview(messageView);
            viewsContainer.AddConstraints(new NSLayoutConstraint[]
            {
                messageView.LeadingAnchor.ConstraintEqualTo(viewsContainer.LeadingAnchor, InnerHorizontalMargine),
                messageView.TrailingAnchor.ConstraintEqualTo(viewsContainer.TrailingAnchor, -InnerHorizontalMargine),
                messageView.TopAnchor.ConstraintEqualTo(lineView.BottomAnchor, InnerVerticalMargine),
                messageView.BottomAnchor.ConstraintEqualTo(buttonGridView.TopAnchor, -InnerVerticalMargine)
            });
        }

        private UIView InitializeContainer()
        {
            UIView container = new UIView()
            {
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            container.Layer.ShadowColor = UIColor.Black.CGColor;
            container.Layer.ShadowOpacity = 0.5f;
            container.Layer.ShadowRadius = 24;
            container.Layer.ShadowOffset = new CoreGraphics.CGSize(0f, 4f);
            container.Layer.CornerRadius = 8;

            return container;
        }

        private UITextView InitializeMessageView()
        {
            UITextView textView = new UITextView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultLightFont,
                TextColor = Theme.DarkGray,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                TextAlignment = UITextAlignment.Left,
                Text = Localization.GetString("add_message"),
                ClipsToBounds = true,
                TextContainerInset = UIEdgeInsets.Zero
            };

            textView.TextContainer.LineFragmentPadding = 0;

            return textView;
        }

        private UIView InitializeLineView()
        {
            UIView container = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            lineLabel = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 1,
                Font = Theme.DefaultLightFont,
                TextColor = Theme.DarkerBlue,
                TextAlignment = UITextAlignment.Left,
                Text = "",
                MinimumScaleFactor = .8f,
                AdjustsFontSizeToFitWidth = true
            };

            container.AddSubview(lineLabel);

            UIImageView arrowImg = new UIImageView()
            {
                Image = UIImage.FromBundle("Arrow-Expand").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            container.AddSubview(arrowImg);

            container.AddConstraints(new NSLayoutConstraint[]
            {
                lineLabel.CenterYAnchor.ConstraintEqualTo(container.CenterYAnchor),
                lineLabel.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor),
                lineLabel.TopAnchor.ConstraintEqualTo(container.TopAnchor),
                lineLabel.BottomAnchor.ConstraintEqualTo(container.BottomAnchor),
                arrowImg.LeadingAnchor.ConstraintGreaterThanOrEqualTo(lineLabel.TrailingAnchor,10),
                arrowImg.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor),
                arrowImg.HeightAnchor.ConstraintEqualTo(20f),
                arrowImg.WidthAnchor.ConstraintEqualTo(20f),
                arrowImg.CenterYAnchor.ConstraintEqualTo(container.CenterYAnchor)
            });

            container.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("lineTapped:")));

            return container;
        }

        private UIView InitializeButtonGrid()
        {
            UIView container = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            acceptButton = new ReplyButton(ParticipantStatus.Accepted);
            declineButton = new ReplyButton(ParticipantStatus.Declined);
            tentativeButton = new ReplyButton(ParticipantStatus.Tentative);

            acceptButton.ButtonTapped += ButtonTapped;
            declineButton.ButtonTapped += ButtonTapped;
            tentativeButton.ButtonTapped += ButtonTapped;

            container.AddSubviews(new UIView[]
            {
                acceptButton,
                tentativeButton,
                declineButton
            });

            container.AddConstraints(new NSLayoutConstraint[]
            {
                acceptButton.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor),
                acceptButton.TopAnchor.ConstraintEqualTo(container.TopAnchor),
                acceptButton.BottomAnchor.ConstraintEqualTo(container.BottomAnchor),
                declineButton.LeadingAnchor.ConstraintEqualTo(acceptButton.TrailingAnchor),
                declineButton.WidthAnchor.ConstraintEqualTo(acceptButton.WidthAnchor),
                declineButton.HeightAnchor.ConstraintEqualTo(acceptButton.HeightAnchor),
                tentativeButton.LeadingAnchor.ConstraintEqualTo(declineButton.TrailingAnchor),
                tentativeButton.WidthAnchor.ConstraintEqualTo(acceptButton.WidthAnchor),
                tentativeButton.HeightAnchor.ConstraintEqualTo(acceptButton.HeightAnchor),
                tentativeButton.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor)
            });

            return container;
        }
        #endregion

        #region Listeners/Delegates/Actions

        private void MessageView_StartedEnded(object sender, EventArgs e)
        {
            var field = (UITextView)sender;

            if (field.Text.Equals(Localization.GetString("add_message")))
            {
                field.Text = string.Empty;
                field.TextColor = Theme.DarkerBlue;
            }
            else if (string.IsNullOrWhiteSpace(field.Text))
            {
                field.TextColor = Theme.DarkGray;
                field.Text = Localization.GetString("add_message");
            }
        }

        [Export("tapped:")]
        void Tapped(UITapGestureRecognizer recognizer)
        {
            if (!viewsContainer.Frame.Contains(recognizer.LocationInView(this.View)))
            {
                tcs.TrySetResult(null);
                DismissModalViewController(true);
            }
        }

        [Export("lineTapped:")]
        async void LineTapped(UITapGestureRecognizer recognizer)
        {
            var availableOutgoingLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines;
            var choices = availableOutgoingLines.Select(x => x.Name).ToArray();

            var result = await Dialogs.ShowListActionSheetWithTitleAsync(this, choices, recognizer.View, Localization.GetString("select_line"));

            if (result < 0)
                return;

            model.Line = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines[result];
            UpdateLine();
        }

        void ButtonTapped(object sender, ParticipantStatus status)
        {
            if (IsValid())
            {
                model.Status = status;
                model.Message = messageView.Text;
                tcs.TrySetResult(model);
                DismissModalViewController(true);
            }
            else
                Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("line_missing"), Localization.GetString("line_missing_content"), "Ok");
        }
        #endregion

        #region Helpers

        void UpdateLine()
        {
            lineLabel.Text = model?.Line?.Name ?? Localization.GetString("tap_select_line");
        }

        void UpdateButtons()
        {
            switch (userStatus)
            {
                case ParticipantStatus.Accepted:
                    acceptButton.Enabled = false;
                    break;
                case ParticipantStatus.Tentative:
                    tentativeButton.Enabled = false;
                    break;
                case ParticipantStatus.Declined:
                    declineButton.Enabled = false;
                    break;
            }

        }

        bool IsValid() => model.Line != null;

        #endregion
    }

    public class ReplyButton : UIButton
    {
        readonly ParticipantStatus participantStatus;

        public EventHandler<ParticipantStatus> ButtonTapped = delegate { };

        public ReplyButton(ParticipantStatus status)
        {
            participantStatus = status;
            TranslatesAutoresizingMaskIntoConstraints = false;
            SetTitle(GetTitle(), UIControlState.Normal);
            SetTitleColor(Theme.DarkerBlue, UIControlState.Normal);
            SetTitleColor(Theme.DarkGray, UIControlState.Disabled);
            Font = Theme.DefaultBoldFont;
            BackgroundColor = Theme.White;
            TouchUpInside += (sender, e) => ButtonTapped(sender, participantStatus);
        }

        string GetTitle()
        {
            switch (participantStatus)
            {
                case ParticipantStatus.Accepted:
                    return Localization.GetString("accept");
                case ParticipantStatus.Declined:
                    return Localization.GetString("decline");
                case ParticipantStatus.Tentative:
                    return Localization.GetString("tentative");
                default:
                    return "";
            }
        }
    }
}