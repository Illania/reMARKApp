using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class DisclosureIndicatorTableViewCell : AddEditContactTableViewCell
    {
        public static readonly NSString Key = new NSString("DisclosureIndicatorTableViewCell");

        protected UILabel TitleLabel { get; set; }
        protected UILabel ContentLabel { get; set; }

        NSLayoutConstraint leftContentConstraint;
        NSLayoutConstraint leftTitleConstraint;

        public DisclosureIndicatorTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            TitleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultBoldFont,
            };
            TitleLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            TitleLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(TitleLabel);
            leftTitleConstraint = NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
                leftTitleConstraint,
            });

            ContentLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Lines = 0,
            };
            ContentView.AddSubview(ContentLabel);
            leftContentConstraint = NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, TitleLabel, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.Height, 1f, 20f),
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
                leftContentConstraint,
            });

            var chevronButton = GetChevron();
            chevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            chevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(chevronButton);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentLabel, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, ContentLabel, NSLayoutAttribute.CenterY, 1f, 0f),
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
            });
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                leftTitleConstraint.Constant = 0;
                leftContentConstraint.Constant = HorizontalMargin;
            }
            else
            {
                leftTitleConstraint.Constant = HorizontalMargin;
                leftContentConstraint.Constant = InnerHorizontalMargin;
                TitleLabel.Text = title;
            }
        }

        public void SetContent(string content)
        {
            ContentLabel.Text = content;
        }

        public void Reset()
        {
            SetErrorState(false);

            SetTitle(string.Empty);
            SetContent(string.Empty);
        }
    }
}
