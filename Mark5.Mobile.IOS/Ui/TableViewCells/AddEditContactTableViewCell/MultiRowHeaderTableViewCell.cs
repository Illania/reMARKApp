using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class MultiRowHeaderTableViewCell : AddEditContactTableViewCell
    {
        public static readonly NSString Key = new NSString("MultiRowHeaderTableViewCell");

        readonly UILabel titleLabel;

        public MultiRowHeaderTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            titleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkBlue,
            };

            ContentView.AddSubview(titleLabel);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeadingMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TrailingMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
            });

        }

        public void SetTitle(string title)
        {
            titleLabel.Text = title;
        }
    }
}
