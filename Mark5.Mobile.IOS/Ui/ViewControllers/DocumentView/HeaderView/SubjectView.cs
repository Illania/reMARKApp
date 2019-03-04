using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class SubjectView : DocumentSubView
    {
        UITextView subjectTextView;

        public SubjectView()
        {
            subjectTextView = new UITextView
            {
                BackgroundColor = Theme.Clear,
                TextColor = Theme.DarkerBlue,
                Font = Theme.DefaultLightBoldFont.WithRelativeSize(4f),
                Opaque = false,
                ClipsToBounds = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextContainerInset = UIEdgeInsets.Zero,
                ScrollEnabled = false,
                Editable = false
            };
            subjectTextView.TextContainer.LineFragmentPadding = 0f;
            subjectTextView.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;

            ContainerView.AddSubview(subjectTextView);
            ContainerView.AddConstraints(new[]
            {
                subjectTextView.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                subjectTextView.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin),
                subjectTextView.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                subjectTextView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -5f)
            });
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                subjectTextView?.RemoveFromSuperview();
                subjectTextView = null;
            }
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
                subjectTextView.Text = DocumentPreview.Subject;
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
