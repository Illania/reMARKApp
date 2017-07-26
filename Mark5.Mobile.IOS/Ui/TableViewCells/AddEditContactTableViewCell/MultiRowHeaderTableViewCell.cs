using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class MultiRowHeaderTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("MultiRowHeaderTableViewCell");

        UILabel TitleLabel { get; set; }

        public MultiRowHeaderTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            TitleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultBoldFont,
            };

            ContentView.AddSubview(TitleLabel);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, 0f),
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeadingMargin, 1f, 8f),
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TrailingMargin, 1f, 0f),
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, 0f),
            });

        }

        public void SetTitle(string title)
        {
            TitleLabel.Text = title;
        }
    }
}
