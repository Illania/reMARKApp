using System;
using CoreAnimation;
using CoreGraphics;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using Mark5.Mobile.Common.Extensions;
using Foundation;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class NewRecipientsView : MailViewerSubview
    {
        public enum Type
        {
            To,
            Cc,
            Bcc,
            From,
            ReplyTo
        }

        enum State
        {
            Compressed,
            PartiallyExpanded,
            FullyExpanded,
        }

        public event EventHandler BeginAnimating = delegate { };
        public event EventHandler Animating = delegate { };
        public event EventHandler EndAnimating = delegate { };

        const string EmailSeparator = ", ";
        const string RecipentRegex = @"[^,]*";

        readonly Type addressType;
        readonly float buttonSize = 20f;
        readonly UIFont addressesFont = Theme.DefaultFont;
        readonly uint partiallyExpandedLines = 3;

        UILabel titleLabel;
        UITextView textView;
        UIButton expandButton;


        State currentState;
        CADisplayLink displayLink;

        NSLayoutConstraint expandButtonWidthConstraint;

        public NewRecipientsView(Type addressType)
        {
            this.addressType = addressType;

            titleLabel = new UILabel
            {
                Text = GetTitle() + ":",
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            titleLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            titleLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(titleLabel);
            ContainerView.AddConstraints(new[]
            {
                titleLabel.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                titleLabel.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin),
                titleLabel.WidthAnchor.ConstraintEqualTo(45f),
            });

            var textStorage = new NSTextStorage();
            var layoutManager = new NSLayoutManager();
            textStorage.AddLayoutManager(layoutManager);
            var textContainer = new NSTextContainer();
            layoutManager.AddTextContainer(textContainer);

            textView = new UITextView(CGRect.Empty, textContainer)
            {
                BackgroundColor = Theme.Clear,
                Editable = false,
                Opaque = false,
                TextContainerInset = UIEdgeInsets.Zero,
                ClipsToBounds = false,
                ScrollEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            textView.TextContainer.LineFragmentPadding = 0f;
            textView.TextContainer.MaximumNumberOfLines = 1;
            textView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            textView.TextAlignment = UITextAlignment.Justified;
            ContainerView.AddSubview(textView);
            ContainerView.AddConstraints(new[]
            {
                textView.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                textView.LeftAnchor.ConstraintEqualTo(titleLabel.RightAnchor, InnerMargin),
                textView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin),
            });

            expandButton = new LargeHitAreaButton
            {
                TintColor = Theme.Blue,
                BackgroundColor = Theme.Clear,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                HitAreaMargin = 10,
            };
            expandButton.SetImage(UIImage.FromBundle("Arrow-Expand").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            ContainerView.AddSubview(expandButton);
            ContainerView.AddConstraints(new[]
            {
                expandButtonWidthConstraint = expandButton.WidthAnchor.ConstraintEqualTo(0f),
                expandButton.HeightAnchor.ConstraintEqualTo(buttonSize),
                expandButton.TrailingAnchor.ConstraintEqualTo(ContainerView.TrailingAnchor, -HorizontalMargin),
                expandButton.LeadingAnchor.ConstraintEqualTo(textView.TrailingAnchor, 0),
                expandButton.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
            });
            expandButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            expandButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            expandButton.TouchUpInside += ExpandButton_TouchUpInside;
        }

        void TransitionToState(State state)
        {
            if (currentState == state || Superview == null)
                return;

            currentState = state;

            textView.TextStorage.BeginEditing();
            textView.TextStorage.Insert(" ".ToNSAttributedString(), 0);
            textView.TextStorage.DeleteRange(new NSRange(0, 1));
            textView.TextStorage.EndEditing();

            AnimateNotify(0.3d, () =>
            {
                BeginAnimating(this, EventArgs.Empty);
                displayLink = CADisplayLink.Create(() => Animating(this, EventArgs.Empty));
                displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);

                switch (state)
                {
                    case State.Compressed:
                        textView.TextContainer.MaximumNumberOfLines = 1;
                        textView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
                        expandButtonWidthConstraint.Constant = 0f;
                        expandButton.Transform = CGAffineTransform.MakeRotation(0f);
                        break;
                    case State.PartiallyExpanded:
                        textView.TextContainer.MaximumNumberOfLines = addressType == Type.From ? 0 : partiallyExpandedLines;
                        textView.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
                        expandButtonWidthConstraint.Constant = (addressType == Type.From || partiallyExpandedLines >= GetNumberLines())
                            ? 0f : buttonSize;
                        expandButton.Transform = CGAffineTransform.MakeRotation(0f);
                        break;
                    case State.FullyExpanded:
                        textView.TextContainer.MaximumNumberOfLines = 0;
                        textView.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
                        expandButtonWidthConstraint.Constant = buttonSize;
                        expandButton.Transform = CGAffineTransform.MakeRotation((nfloat)(Math.PI / 2.0f));
                        break;
                }

                Superview?.Superview?.Superview?.Superview?.LayoutIfNeeded();
            }, (finished) =>
            {
                displayLink.Invalidate();
                displayLink = null;
                EndAnimating(this, EventArgs.Empty);
            });
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                titleLabel?.RemoveFromSuperview();
                titleLabel = null;

                textView?.RemoveFromSuperview();
                textView.GestureRecognizers.ForEach(textView.RemoveGestureRecognizer);
                textView = null;

                expandButton.TouchUpInside -= ExpandButton_TouchUpInside;
            }
        }

        string GetTitle()
        {
            switch (addressType)
            {
                case Type.To:
                    return Localization.GetString("to");
                case Type.Cc:
                    return Localization.GetString("cc");
                case Type.Bcc:
                    return Localization.GetString("bcc");
                case Type.From:
                    return Localization.GetString("from");
                case Type.ReplyTo:
                    return Localization.GetString("reply_to");
                default:
                    throw new ArgumentException(string.Format("Unknown type. [addressType={0}]", addressType));
            }
        }

        string GetValue()
        {
            switch (addressType)
            {
                case Type.To:
                    return MailMessage?.To?.AsString;
                case Type.Cc:
                    return MailMessage?.Cc?.AsString;
                case Type.Bcc:
                    return MailMessage?.Bcc?.AsString;
                case Type.From:
                    return MailMessage?.From?.AsString;
                case Type.ReplyTo:
                    return MailMessage?.ReplyTo?.AsString;
                default:
                    throw new ArgumentException(string.Format("Unknown type. [addressType={0}]", addressType));
            }
        }

        public override void RefreshView()
        {
            if (MailMessage != null)
            {
                textView.TextStorage.BeginEditing();
                textView.TextStorage.SetString(GetValue().ToNSAttributedString());
                textView.TextStorage.AddAttribute(UIStringAttributeKey.Font, Theme.DefaultFont, new NSRange(0, textView.Text.Length));
                textView.TextStorage.EndEditing();
            }
        }

        public override void UpdateVisibility()
        {
            if (MailMessage == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(GetValue());
        }

        int GetNumberLines()
        {
            var rect = new NSString(textView.Text).GetBoundingRect(new CGSize(textView.Bounds.Width, nfloat.MaxValue),
                                                    NSStringDrawingOptions.UsesLineFragmentOrigin,
                                                    new UIStringAttributes { Font = addressesFont },
                                                    null);
            return (int)Math.Ceiling(rect.Height / addressesFont.LineHeight - 0.001);
        }

        void ExpandButton_TouchUpInside(object sender, EventArgs e)
        {
            if (currentState == State.PartiallyExpanded)
                TransitionToState(State.FullyExpanded);
            else if (currentState == State.FullyExpanded)
                TransitionToState(State.PartiallyExpanded);
        }

        public void ExpandCompressView()
        {
            if (currentState == State.Compressed)
                TransitionToState(State.PartiallyExpanded);
            else
                TransitionToState(State.Compressed);
        }
    }
}
