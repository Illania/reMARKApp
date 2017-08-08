using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class TitledTextViewTableViewCell : AddEditContactTableViewCell, IUITextViewDelegate
    {
        public static readonly NSString Key = new NSString("TitledTextFieldTableViewCell");

        public event EventHandler<string> ContentEdited = delegate { };

        readonly UITextView textView;
        readonly UILabel titleLabel;

        public TitledTextViewTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            titleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultBoldFont,
            };
            ContentView.AddSubview(titleLabel);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
            });

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
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, titleLabel, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });
        }

        [Export("textView:shouldChangeTextInRange:replacementText:")]
        public bool ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            if (textView.TextContainer.MaximumNumberOfLines == 1)
                return !text.Contains(Environment.NewLine);

            return true;
        }

        [Export("textViewDidChange:")]
        public void Changed(UITextView textView)
        {
            ContentEdited(this, textView.Text);
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

        public void Reset()
        {
            ContentEdited = delegate { };
            SetErrorState(false);

            SetTitle(string.Empty);
            SetContent(string.Empty);
        }
    }
}
