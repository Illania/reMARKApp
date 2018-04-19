using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class SubjectView : DocumentSubView
    {
        UILabel subjectLabel;

        public SubjectView()
        {
            subjectLabel = new UILabel
            {
                BackgroundColor = Theme.Clear,
                TextColor = Theme.DarkerBlue,
                Font = Theme.DefaultLightBoldFont.WithRelativeSize(4f),
                Opaque = false,
                ClipsToBounds = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            subjectLabel.Lines = 0;
            subjectLabel.LineBreakMode = UILineBreakMode.WordWrap;
            ContainerView.AddSubview(subjectLabel);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(subjectLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(subjectLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(subjectLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(subjectLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -5f)
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
            if (DocumentPreview != null)
                subjectLabel.Text = DocumentPreview.Subject;
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(DocumentPreview.Subject);
        }
    }
}
