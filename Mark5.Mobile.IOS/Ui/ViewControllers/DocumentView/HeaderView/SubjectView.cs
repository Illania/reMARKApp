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
                Editable = false,
                Opaque = false,
                AutocapitalizationType = UITextAutocapitalizationType.Sentences,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                SpellCheckingType = UITextSpellCheckingType.Yes,
                TextContainerInset = UIEdgeInsets.Zero,
                ClipsToBounds = false,
                ScrollEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            subjectTextView.TextContainer.LineFragmentPadding = 0f;
            ContainerView.AddSubview(subjectTextView);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(subjectTextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(subjectTextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(subjectTextView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(subjectTextView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -5f)
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
