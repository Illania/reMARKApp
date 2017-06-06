using MailBee.Mime;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class PriorityView : MailViewerSubview
    {
        readonly UITextView textView;

        public PriorityView()
        {
            var label = new UILabel();
            label.Text = Localization.GetString("priority") + ":";
            label.Font = Theme.DefaultFont;
            label.TextColor = UIColor.LightGray;
            label.Opaque = false;
            label.TranslatesAutoresizingMaskIntoConstraints = false;
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
            });

            textView = new UITextView();
            textView.Font = Theme.DefaultFont;
            textView.Editable = false;
            textView.Opaque = false;
            textView.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            textView.AutocorrectionType = UITextAutocorrectionType.Yes;
            textView.SpellCheckingType = UITextSpellCheckingType.Yes;
            textView.TextContainer.LineFragmentPadding = 0f;
            textView.TextContainerInset = UIEdgeInsets.Zero;
            textView.ClipsToBounds = false;
            textView.ScrollEnabled = false;
            textView.TranslatesAutoresizingMaskIntoConstraints = false;
            ContainerView.AddSubview(textView);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1f, InnerMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });
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