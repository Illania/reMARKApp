using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class ContactsTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(ContactsTableViewCell));

        readonly UIStackView categoriesStackView;
        readonly UILabel label;

        public ContactsTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.DisclosureIndicator;

            categoriesStackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.FillEqually,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            label = new UILabel
            {
                Font = Theme.DefaultFont,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            ContentView.AddSubview(categoriesStackView);
            ContentView.AddSubview(label);

            ContentView.AddConstraints(new[]
            {
                categoriesStackView.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor, -8f),
                categoriesStackView.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 4f),
                categoriesStackView.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -4f),
                categoriesStackView.WidthAnchor.ConstraintEqualTo(4f),

                label.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                label.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                label.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 8f),
                label.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f)
            });
        }

        public void Initialize(ContactPreview cp)
        {
            label.Text = cp.Name;

            categoriesStackView.Subviews.ForEach(v => v.RemoveFromSuperview());
            foreach (var c in cp.Categories)
            {
                var v = new UIView
                {
                    BackgroundColor = UI.UIColorFromHexString(c.HexColor),
                    UserInteractionEnabled = false
                };
                categoriesStackView.AddArrangedSubview(v);
            }
        }

        public override void SetSelected(bool selected, bool animated)
        {
            base.SetSelected(selected, animated);

            categoriesStackView.Subviews.ForEach(v =>
            {
                if (v.BackgroundColor.CGColor.Alpha < 1f)
                    v.BackgroundColor = v.BackgroundColor.ColorWithAlpha(1f);
            });
        }

        public override void SetHighlighted(bool highlighted, bool animated)
        {
            base.SetHighlighted(highlighted, animated);

            categoriesStackView.Subviews.ForEach(v =>
            {
                if (v.BackgroundColor.CGColor.Alpha < 1f)
                    v.BackgroundColor = v.BackgroundColor.ColorWithAlpha(1f);
            });
        }
    }
}