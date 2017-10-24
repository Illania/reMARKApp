using System;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCell
{
    public class TitledTextViewTableViewCell : AddEditTableViewCell, IUITextViewDelegate
    {
        public static readonly NSString Key = new NSString("TitledTextFieldTableViewCell");

        public Action<string> ContentEditedAction;
        public Action NumbersOfLineChangedAction;

        readonly UITextView textView;
        readonly UILabel titleLabel;

        public TitledTextViewTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            titleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont.WithRelativeSize(1),
                UserInteractionEnabled = true,
            };
            titleLabel.AddGestureRecognizer(new UITapGestureRecognizer(HandlTitleTap));
            ContentView.AddSubview(titleLabel);

            textView = new UITextView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Editable = true,
                ScrollEnabled = false,
            };
            textView.TextContainer.LineFragmentPadding = 0f;
            textView.TextContainerInset = UIEdgeInsets.Zero;
            textView.Delegate = this;
            textView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            textView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            ContentView.AddSubview(textView);

            ContentView.AddConstraints(new[]
            {
                titleLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                titleLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                titleLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),

                textView.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor, InnerVerticalMargin),
                textView.LeadingAnchor.ConstraintEqualTo(titleLabel.LeadingAnchor),
                textView.TrailingAnchor.ConstraintEqualTo(titleLabel.TrailingAnchor),
                textView.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor),
                textView.HeightAnchor.ConstraintGreaterThanOrEqualTo(InnerRowHeight),
            });
        }

        CGRect previous = CGRect.Empty;

        [Export("textView:shouldChangeTextInRange:replacementText:")]
        public bool ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            if (textView.TextContainer.MaximumNumberOfLines == 1)
                return !text.Contains(Environment.NewLine);

            previous = textView.GetCaretRectForPosition(textView.EndOfDocument);

            return true;
        }

        [Export("textViewDidChange:")]
        public void Changed(UITextView textView)
        {
            ContentEditedAction?.Invoke(textView.Text);

            if (textView.GetCaretRectForPosition(textView.EndOfDocument).Y != previous.Y)
            {
                NumbersOfLineChangedAction?.Invoke();
            }
        }

        void HandlTitleTap()
        {
            textView.BecomeFirstResponder();
        }

        public void SetTitle(string title)
        {
            titleLabel.Text = title;
        }

        public void SetContent(string content)
        {
            textView.Text = content;
        }

        public void SetMultiline(bool isMultiline)
        {
            textView.TextContainer.MaximumNumberOfLines = isMultiline ? 0 : (nuint)1;
            textView.ScrollEnabled = !isMultiline;
        }

        public void SetAutocapitalizationType(UITextAutocapitalizationType type)
        {
            textView.AutocapitalizationType = type;
        }

        public void SetAutocorrectionType(UITextAutocorrectionType type)
        {
            textView.AutocorrectionType = type;
        }

        public override void Reset()
        {
            ContentEditedAction = null;
            NumbersOfLineChangedAction = null;

            textView.AutocorrectionType = UITextAutocorrectionType.Default;
            textView.AutocapitalizationType = UITextAutocapitalizationType.Sentences;

            SetErrorState(false);

            titleLabel.Text = string.Empty;
            textView.Text = string.Empty;
        }
    }
}
