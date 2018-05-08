using System;
using System.Linq;
using System.Text.RegularExpressions;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class RecipientsView : DocumentSubView
    {
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

        readonly DocumentAddressType addressType;
        readonly float buttonSize = 20f;
        readonly UIFont addressesFont = Theme.DefaultFont;
        readonly uint partiallyExpandedLines = 3;

        UILabel titleLabel;
        UITextView textView;
        UIButton expandButton;

        State currentState;
        CADisplayLink displayLink;

        NSLayoutConstraint expandButtonWidthConstraint;

        public event EventHandler<RecipientTappedEventArgs> RecipientTapped = delegate { };

        public RecipientsView(DocumentAddressType addressType)
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
            textView.AddGestureRecognizer(new UITapGestureRecognizer(HandleTextTapped));

            expandButton = new UIButton
            {
                TintColor = Theme.Blue,
                BackgroundColor = Theme.Clear,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(1f, 0f, 4f, 1f)
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
                        textView.TextContainer.MaximumNumberOfLines = addressType == DocumentAddressType.From ? 0 : partiallyExpandedLines;
                        textView.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
                        expandButtonWidthConstraint.Constant = (addressType == DocumentAddressType.From || partiallyExpandedLines >= GetNumberLines())
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

        void HandleTextTapped(UITapGestureRecognizer gestureRecognizer)
        {
            if (currentState == State.Compressed)
                return;

            var tapPosition = textView.GetClosestPositionToPoint(gestureRecognizer.LocationInView(textView));
            var offset = textView.GetOffsetFromPosition(textView.BeginningOfDocument, tapPosition);

            var beforeSubstring = textView.Text.SafeSubstring(0, (int)offset).SafeSubstringAfterLast(EmailSeparator, StringComparison.CurrentCultureIgnoreCase).Trim();
            var afterSubstring = textView.Text.SafeSubstring((int)offset).SafeSubstringBefore(EmailSeparator, StringComparison.CurrentCultureIgnoreCase).Trim();

            var tappedRecipent = beforeSubstring + afterSubstring;

            if (CommonConfig.Logger.IsTraceEnabled())
                CommonConfig.Logger.Trace(string.Format("Tapped recipent. [recipent={0}]", tappedRecipent));

            RecipientTapped?.Invoke(this, new RecipientTappedEventArgs(tappedRecipent));
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
            {
                Func<DocumentAddress, string> addressText = (da) =>
                {
                    if (!string.IsNullOrWhiteSpace(da.Name) && string.IsNullOrWhiteSpace(da.Address))
                        return da.Name;
                    if (!string.IsNullOrWhiteSpace(da.Name) && !string.IsNullOrWhiteSpace(da.Address))
                        return da.Name + " <" + da.Address + ">";
                    if (string.IsNullOrWhiteSpace(da.Name) && !string.IsNullOrWhiteSpace(da.Address))
                        return da.Address;

                    return string.Empty;
                };

                var prettyAddresses = DocumentPreview.Addresses.Where(da => da.AddressType == addressType).Select(addressText);
                var text = string.Join(EmailSeparator, prettyAddresses);

                textView.TextStorage.BeginEditing();
                textView.TextStorage.SetString(text.ToNSAttributedString());
                textView.TextStorage.EndEditing();

                CorrectMarkup();
            }
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = IsEmpty();
        }

        public bool IsEmpty()
        {
            return !DocumentPreview.Addresses.Any(da => da.AddressType == addressType);
        }

        string GetTitle()
        {
            switch (addressType)
            {
                case DocumentAddressType.To:
                    return Localization.GetString("to");
                case DocumentAddressType.Cc:
                    return Localization.GetString("cc");
                case DocumentAddressType.Bcc:
                    return Localization.GetString("bcc");
                case DocumentAddressType.From:
                    return Localization.GetString("from");
                default:
                    throw new ArgumentException(string.Format("Unknown type. [addressType={0}]", addressType));
            }
        }

        void CorrectMarkup()
        {
            textView.TextStorage.BeginEditing();

            textView.TextStorage.AddAttribute(UIStringAttributeKey.Font, addressesFont, new NSRange(0, textView.Text.Length));
            textView.TextStorage.RemoveAttribute(UIStringAttributeKey.ForegroundColor, new NSRange(0, textView.Text.Length));

            var matches = Regex.Matches(textView.Text, RecipentRegex, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var textInMatch = textView.Text.SafeSubstring(match.Index, match.Length);
                if (Validator.ContainsValidEmails(textInMatch))
                    textView.TextStorage.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.DarkBlue, new NSRange(match.Index, match.Length));
            }

            textView.TextStorage.EndEditing();
        }

        int GetNumberLines()
        {
            var size = new NSString(textView.Text).GetBoundingRect(new CGSize(textView.Bounds.Width, nfloat.MaxValue),
                                                    NSStringDrawingOptions.UsesLineFragmentOrigin,
                                                    new UIStringAttributes { Font = addressesFont },
                                                    null);
            return (int)Math.Ceiling(size.Height / addressesFont.LineHeight);
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

    public class RecipientTappedEventArgs : EventArgs
    {
        public string Recipent { get; }

        public RecipientTappedEventArgs(string recipent)
        {
            Recipent = recipent;
        }
    }
}
