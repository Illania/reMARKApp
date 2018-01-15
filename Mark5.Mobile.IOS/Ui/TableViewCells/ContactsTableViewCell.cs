using System.Linq;
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

            var leadingMarginGuide = new UILayoutGuide();
            ContentView.AddLayoutGuide(leadingMarginGuide);

            leadingMarginGuide.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor).Active = true;
            var leadingMarginWidthAnchor = leadingMarginGuide.WidthAnchor.ConstraintEqualTo(0f);
            leadingMarginWidthAnchor.SetIdentifier("leadingMarginWidth");
            leadingMarginWidthAnchor.Active = true;

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
                categoriesStackView.TrailingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor, -8f),
                categoriesStackView.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 4f),
                categoriesStackView.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -4f),
                categoriesStackView.WidthAnchor.ConstraintEqualTo(4f),

                label.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor),
                label.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                label.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),
                label.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
                label.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
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
            UIColor[] colors = null;

            if (categoriesStackView != null)
            {
                colors = new UIColor[categoriesStackView.Subviews.Length];
                for (var i = 0; i < categoriesStackView.Subviews.Length; i++)
                    colors[i] = categoriesStackView.Subviews[i].BackgroundColor;
            }

            base.SetSelected(selected, animated);

            if (colors != null)
            {
                for (var i = 0; i < categoriesStackView.Subviews.Length; i++)
                    categoriesStackView.Subviews[i].BackgroundColor = colors[i];

                colors = null;
            }
        }

        public override void SetHighlighted(bool highlighted, bool animated)
        {
            UIColor[] colors = null;

            if (categoriesStackView != null)
            {
                colors = new UIColor[categoriesStackView.Subviews.Length];
                for (var i = 0; i < categoriesStackView.Subviews.Length; i++)
                    colors[i] = categoriesStackView.Subviews[i].BackgroundColor;
            }

            base.SetHighlighted(highlighted, animated);

            if (colors != null)
            {
                for (var i = 0; i < categoriesStackView.Subviews.Length; i++)
                    categoriesStackView.Subviews[i].BackgroundColor = colors[i];

                colors = null;
            }
        }

        public override void SetEditing(bool editing, bool animated)
        {
            base.SetEditing(editing, animated);

            var c = ContentView?.Constraints?.FirstOrDefault(nslc => nslc.GetIdentifier() == "leadingMarginWidth");
            if (c != null)
                c.Constant = editing ? 16f : 0f;

            if (animated)
                AnimateNotify(.25d, ContentView.LayoutIfNeeded, null);
            else
                LayoutIfNeeded();
        }
    }
}