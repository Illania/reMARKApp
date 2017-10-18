using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class ShortcodesTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(ShortcodesTableViewCell));

        readonly UILabel label;

        public ShortcodesTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.DisclosureIndicator;

            label = new UILabel
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.Black,
                TextAlignment = UITextAlignment.Left,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(label);
            ContentView.AddConstraints(new[]
            {
                label.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor, 12),
                label.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                label.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 4),
                label.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -4),
                label.HeightAnchor.ConstraintEqualTo(22f),
            });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            Hacks.CorrectFontInActions(this, Theme.DefaultActionsFont);
        }

        public void Initialize(ShortcodePreview shortcodePreview)
        {
            label.Text = shortcodePreview.Name;
        }
    }
}