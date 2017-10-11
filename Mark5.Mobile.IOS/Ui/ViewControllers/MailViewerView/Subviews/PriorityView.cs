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
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
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
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1f, InnerMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
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