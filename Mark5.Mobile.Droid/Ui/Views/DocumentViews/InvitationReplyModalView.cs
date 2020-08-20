using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{
    public class InvitationReplyModalView : LinearLayoutCompat
    {
        readonly int verticalMargine;
        readonly int horizontalMargine;
        readonly ParticipantStatus userStatus;

        AppCompatTextView lineLabel;
        ReplyButton acceptButton;
        ReplyButton tentativeButton;
        ReplyButton declineButton;

        public InvitationReplyDetailViewModel DetailsModel;
        public EventHandler<InvitationReplyDetailViewModel> ResponseSelected = delegate { };

        public InvitationReplyModalView(Context context, ParticipantStatus userStatus, Line predefinedLine)
            : base(context)
        {
            this.userStatus = userStatus;

            DetailsModel = new InvitationReplyDetailViewModel();
            DetailsModel = new InvitationReplyDetailViewModel
            {
                Line = predefinedLine
            };

            verticalMargine = Conversion.ConvertDpToPixels(20f);
            horizontalMargine = Conversion.ConvertDpToPixels(20f);

            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Orientation = Vertical;

            AddView(InitializeLineView());
            AddView(InitializeMessageView());
            AddView(InitializeButtonsGrid());

            RefreshView();
        }

        RelativeLayout InitializeLineView()
        {
            RelativeLayout lineContainer = new RelativeLayout(Context)
            {
                Clickable = true,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = horizontalMargine,
                    RightMargin = horizontalMargine,
                    TopMargin = verticalMargine
                }
            };

            lineContainer.Click += LineContainer_Click;

            RelativeLayout.LayoutParams lineLabelLayoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            lineLabelLayoutParams.AddRule(LayoutRules.AlignParentLeft);

            lineLabel = new AppCompatTextView(Context)
            {
                LayoutParameters = lineLabelLayoutParams
            };

            lineLabel.SetText(Resource.String.tap_select_line);
            lineLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            lineLabel.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            lineContainer.AddView(lineLabel);

            RelativeLayout.LayoutParams rightArrowImglLayoutParams = new RelativeLayout.LayoutParams(Conversion.ConvertDpToPixels(16), Conversion.ConvertDpToPixels(16));
            rightArrowImglLayoutParams.AddRule(LayoutRules.AlignParentRight);
            AppCompatImageView rightArrowImg = new AppCompatImageView(Context)
            {
                LayoutParameters = rightArrowImglLayoutParams
            };

            rightArrowImg.SetImageResource(Resource.Drawable.arrow_right);
            lineContainer.AddView(rightArrowImg);

            return lineContainer;
        }

        AppCompatEditText InitializeMessageView()
        {
            AppCompatEditText messageEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = horizontalMargine,
                    RightMargin = horizontalMargine,
                    TopMargin = verticalMargine,
                    BottomMargin = verticalMargine
                }
            };

            messageEditText.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            messageEditText.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
            messageEditText.SetHint(Resource.String.add_message_optional);

            messageEditText.TextChanged += MessageEditText_TextChanged;

            return messageEditText;
        }

        private void MessageEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (e.Text != null)
                DetailsModel.Message = e.Text.ToString();
        }

        void RefreshView()
        {
            if (DetailsModel.Line != null)
                lineLabel.Text = DetailsModel.Line.Name;

            AppCompatButton buttonToDisable = null;
            switch (userStatus)
            {
                case ParticipantStatus.Accepted:
                    buttonToDisable = acceptButton;
                    break;
                case ParticipantStatus.Tentative:
                    buttonToDisable = tentativeButton;
                    break;
                case ParticipantStatus.Declined:
                    buttonToDisable = declineButton;
                    break;
            }

            if (buttonToDisable != null)
            {
                buttonToDisable.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.mediumGray2)));
                buttonToDisable.Enabled = false;
            }
        }

        LinearLayout InitializeButtonsGrid()
        {
            LinearLayout container = new LinearLayout(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = horizontalMargine,
                    RightMargin = horizontalMargine,
                    BottomMargin = verticalMargine
                }
            };

            acceptButton = new ReplyButton(Context, ParticipantStatus.Accepted)
            {
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1)
            };
            acceptButton.ButtonTapped += ButtonClicked;
            container.AddView(acceptButton);

            tentativeButton = new ReplyButton(Context, ParticipantStatus.Tentative)
            {
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1)
            };
            tentativeButton.ButtonTapped += ButtonClicked;
            container.AddView(tentativeButton);

            declineButton = new ReplyButton(Context, ParticipantStatus.Declined)
            {
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1)
            };
            declineButton.ButtonTapped += ButtonClicked;
            container.AddView(declineButton);

            return container;
        }

        private void ButtonClicked(object sender, ParticipantStatus status)
        {
            if (IsValid())
            {
                DetailsModel.Status = status;
                ResponseSelected(this, DetailsModel);
            }
            else
                Dialogs.ShowConfirmDialog(Context, Resource.String.line_missing_title, Resource.String.line_missing_content);
        }

        bool IsValid()
        {
            return DetailsModel.Line != null;
        }

        private void LineContainer_Click(object sender, EventArgs e)
        {
            var availableOutgoingLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines;
            var choices = availableOutgoingLines.Select(x => x.Name).ToArray();

            var dialog = new AlertDialog.Builder(Context);

            dialog.SetSingleChoiceItems(choices, -1, (obj, selected) =>
            {
                var dial = obj as AlertDialog;
                DetailsModel.Line = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines[selected.Which];
                RefreshView();
                dial.Dismiss();
            });

            dialog.SetCancelable(true);
            dialog.Show();
        }

        public class ReplyButton : AppCompatButton
        {
            ParticipantStatus participantStatus;
            public EventHandler<ParticipantStatus> ButtonTapped = delegate { };

            public ReplyButton(Context context, ParticipantStatus status) : base(context)
            {
                participantStatus = status;
                Text = GetTitle();
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
                SetBackgroundColor(Color.Transparent);
                SetMaxLines(1);
                Click += (sender, e) => ButtonTapped(sender, participantStatus);
            }

            string GetTitle()
            {
                switch (participantStatus)
                {
                    case ParticipantStatus.Accepted:
                        return Resources.GetString(Resource.String.accept);
                    case ParticipantStatus.Declined:
                        return Resources.GetString(Resource.String.decline);
                    case ParticipantStatus.Tentative:
                        return Resources.GetString(Resource.String.tentative);
                    default:
                        return "";
                }
            }
        }
    }
}