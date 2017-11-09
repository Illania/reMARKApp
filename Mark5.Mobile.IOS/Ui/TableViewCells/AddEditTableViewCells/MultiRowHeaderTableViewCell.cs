using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCell
{
    public class MultiRowHeaderTableViewCell : AddEditTableViewCell
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
                titleLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, VerticalMargin),
                titleLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -VerticalMargin),
                titleLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                titleLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
            });

        }

        public void SetTitle(string title)
        {
            titleLabel.Text = title;
        }
    }
}
