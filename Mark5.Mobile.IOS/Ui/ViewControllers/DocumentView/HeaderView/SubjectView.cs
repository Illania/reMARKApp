using System;
using CoreAnimation;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class SubjectView : DocumentSubView, IAnimating
    {
        bool expanded;
        UITextView subjectTextView;

        public event EventHandler BeginAnimating = delegate { };
        public event EventHandler Animating = delegate { };
        public event EventHandler EndAnimating = delegate { };

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
                Editable = false,
            };
            subjectTextView.TextContainer.LineFragmentPadding = 0f;
            subjectTextView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            subjectTextView.TextContainer.MaximumNumberOfLines = 3;

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

        public void ExpandCompressView()
        {
            subjectTextView.TextStorage.BeginEditing();
            subjectTextView.TextStorage.Insert(" ".ToNSAttributedString(), 0);
            subjectTextView.TextStorage.DeleteRange(new NSRange(0, 1));
            subjectTextView.TextStorage.EndEditing();

            CADisplayLink displayLink = null;

            AnimateNotify(0.3d, () =>
            {
                BeginAnimating(this, EventArgs.Empty);
                displayLink = CADisplayLink.Create(() => Animating(this, EventArgs.Empty));
                displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);

                if (expanded)
                {
                    subjectTextView.TextContainer.MaximumNumberOfLines = 3;
                    subjectTextView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
                }
                else
                {
                    subjectTextView.TextContainer.MaximumNumberOfLines = 0;
                    subjectTextView.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
                }

                expanded = !expanded;
                Superview?.Superview?.Superview?.Superview?.LayoutIfNeeded();

            }, (finished) =>
            {
                displayLink.Invalidate();
                displayLink = null;
                EndAnimating(this, EventArgs.Empty);
            });
        }
    }
}
