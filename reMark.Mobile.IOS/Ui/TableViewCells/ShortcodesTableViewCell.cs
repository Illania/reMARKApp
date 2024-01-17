using Foundation;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.TableViewCells
{
    public class ShortcodesTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(ShortcodesTableViewCell));

        readonly UILabelScalable label;

        public ShortcodesTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.DisclosureIndicator;

            label = new UILabelScalable
            {
                Font = Theme.DefaultFont.CustomFont(),
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(label);
            ContentView.AddConstraints(new[]
            {
                label.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                label.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                label.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),
                label.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
                label.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
            });
        }

        public void Initialize(ShortcodePreview sp)
        {
            label.Text = sp.Name;
        }
    }
}