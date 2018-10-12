using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class EmptyTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(EmptyTableViewCell));

        readonly UILabel label;

        public EmptyTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            UserInteractionEnabled = false;
            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;

            label = new UILabel
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                TextAlignment = UITextAlignment.Center,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(label);
            ContentView.AddConstraints(new[]
            {
                label.CenterXAnchor.ConstraintEqualTo(ContentView.CenterXAnchor),
                label.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                label.HeightAnchor.ConstraintEqualTo(22f),
                label.WidthAnchor.ConstraintEqualTo(ContentView.WidthAnchor, 1f, -16f),
            });
        }

        public void Initialize(string text) => label.Text = text;
    }
}