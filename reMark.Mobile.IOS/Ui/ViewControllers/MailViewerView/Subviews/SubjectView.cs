using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class SubjectView : MailViewerSubview
    {
        UILabelScalable subjectLabel;

        public SubjectView()
        {
            subjectLabel = new UILabelScalable
            {
                BackgroundColor = Theme.Clear,
                TextColor = Theme.DarkerBlue,
                Font = Theme.DefaultFont.CustomFont().WithRelativeSize(4f),
                Opaque = false,
                ClipsToBounds = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            subjectLabel.Lines = 0;
            subjectLabel.LineBreakMode = UILineBreakMode.WordWrap;
            ContainerView.AddSubview(subjectLabel);
            ContainerView.AddConstraints(new[]
            {
                subjectLabel.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                subjectLabel.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin),
                subjectLabel.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                subjectLabel.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -5f)
            });
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                subjectLabel?.RemoveFromSuperview();
                subjectLabel = null;
            }
        }

        public override void RefreshView()
        {
            if (MailMessage != null)
                subjectLabel.Text = MailMessage.Subject;
        }

        public override void UpdateVisibility()
        {
            if (MailMessage == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(MailMessage.Subject);
        }
    }
}