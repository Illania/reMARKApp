using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class EmptyTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(EmptyTableViewCell));

        readonly UITextView label;

        public EmptyTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            TranslatesAutoresizingMaskIntoConstraints = false;

            UserInteractionEnabled = false;
            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;

            label = new UITextView
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextContainerInset = new UIEdgeInsets(12, 0, 12, 0),
            };
            ContentView.Add(label);
            ContentView.AddConstraints(new[]
            {
                label.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor),
                label.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor),
                label.LeftAnchor.ConstraintEqualTo(ContentView.LeftAnchor),
                label.RightAnchor.ConstraintEqualTo(ContentView.RightAnchor),
            });
        }

        public void Initialize(string text) => label.Text = text;
    }
}