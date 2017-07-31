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

        public DisclosureIndicatorTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            EditingAccessory = UITableViewCellAccessory.DisclosureIndicator;

            TitleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultBoldFont,
            };
            TitleLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

            ContentView.AddSubview(TitleLabel);

            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
            });

            ContentLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
            };
            ContentView.AddSubview(ContentLabel);

            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, TitleLabel, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
            });
        }

        public void SetTitle(string title)
        {
            TitleLabel.Text = title;
        }

        public void SetContent(string content)
        {
            ContentLabel.Text = content;
        }
    }
}
