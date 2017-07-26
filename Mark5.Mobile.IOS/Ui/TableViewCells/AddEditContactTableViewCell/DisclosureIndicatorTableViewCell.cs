using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class DisclosureIndicatorTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("DisclosureIndicatorTableViewCell");

        UILabel TitleLabel { get; set; }
        UILabel ContentLabel { get; set; }

        public DisclosureIndicatorTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            Accessory = UITableViewCellAccessory.DisclosureIndicator;

            TitleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultBoldFont,
            };
            TitleLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

            ContentView.AddSubview(TitleLabel);

            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, 0f),
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeadingMargin, 1f, 8f),
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, 0f),
            });

            ContentLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
            };
            ContentView.AddSubview(ContentLabel);

            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, 0f),
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, TitleLabel, NSLayoutAttribute.Right, 1f, 12f),
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TrailingMargin, 1f, 0f),
                NSLayoutConstraint.Create(ContentLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, 0f),
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
