using MailBee.Mime;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class PriorityView : MailViewerSubview
    {
        UILabel label;
        UITextView textView;

        public PriorityView()
        {
            label = new UILabel
            {
                Text = Localization.GetString("priority") + ":",
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                label.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                label.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin)
            });

            textView = new UITextView
            {
                Font = Theme.DefaultFont,
                Editable = false,
                AutocapitalizationType = UITextAutocapitalizationType.Sentences,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                SpellCheckingType = UITextSpellCheckingType.Yes,
                TextContainerInset = UIEdgeInsets.Zero,
                ClipsToBounds = false,
                ScrollEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            textView.TextContainer.LineFragmentPadding = 0f;
            ContainerView.AddSubview(textView);
            ContainerView.AddConstraints(new[]
            {
                textView.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                textView.LeftAnchor.ConstraintEqualTo(label.RightAnchor, InnerMargin),
                textView.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                textView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                label?.RemoveFromSuperview();
                label = null;

                textView?.RemoveFromSuperview();
                textView = null;
            }
        }

        public override void RefreshView()
        {
            if (MailMessage != null)
                switch (MailMessage.Priority)
                {
                    case MailPriority.Highest:
                    case MailPriority.High:
                        textView.Text = Localization.GetString("priority_urgent");
                        break;
                    case MailPriority.Lowest:
                    case MailPriority.Low:
                        textView.Text = Localization.GetString("priority_low");
                        break;
                    default:
                        textView.Text = string.Empty;
                        break;
                }
        }

        public override void UpdateVisibility()
        {
            if (MailMessage == null)
            {
                Hidden = true;
                return;
            }

            Hidden = MailMessage.Priority == MailPriority.None || MailMessage.Priority == MailPriority.Normal;
        }
    }
}