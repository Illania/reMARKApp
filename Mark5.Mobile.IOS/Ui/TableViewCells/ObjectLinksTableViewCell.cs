using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class ObjectLinksTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString("ObjectLinksTableViewCell");

        readonly UILabel titleLabel;
        readonly UILabel subtitleLabel;

        public ObjectLinksTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            titleLabel = new UILabel
            {
                Font = Theme.DefaultBoldFont,
                TextColor = Theme.Black,
                TextAlignment = UITextAlignment.Left,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(titleLabel);

            subtitleLabel = new UILabel
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.Black,
                TextAlignment = UITextAlignment.Left,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(subtitleLabel);

            ContentView.AddConstraints(new[]
            {
                subtitleLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                subtitleLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                subtitleLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),

                titleLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                titleLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                titleLabel.TopAnchor.ConstraintEqualTo(subtitleLabel.BottomAnchor, 4f),
                titleLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
            });
        }

        public void Initialize(ObjectLink link)
        {
            titleLabel.Text = link.IsReverse ? link.TypeInfo.DescriptionComplexReverse : link.TypeInfo.DescriptionComplex;
            subtitleLabel.Text = link.Description;

            var clickable = false;
            if (link.IsReverse)
                clickable = link.FromObjectType == ObjectType.Document || link.FromObjectType == ObjectType.Contact || link.FromObjectType == ObjectType.Shortcode;
            else
                clickable = link.ToObjectType == ObjectType.Document || link.ToObjectType == ObjectType.Contact || link.ToObjectType == ObjectType.Shortcode;

            SelectionStyle = clickable ? UITableViewCellSelectionStyle.Default : UITableViewCellSelectionStyle.None;
        }
    }
}