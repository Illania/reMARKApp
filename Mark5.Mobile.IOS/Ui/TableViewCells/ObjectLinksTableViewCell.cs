using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class ObjectLinksTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(ObjectLinksTableViewCell));

        readonly UILabel descriptionLabel;
        readonly UITextView typeDescription;

        public ObjectLinksTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            descriptionLabel = new UILabel
            {
                Font = Theme.DefaultBoldFont,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            typeDescription = new UITextView
            {
                Selectable = false,
                Editable = false,
                ScrollEnabled = false,
                ClipsToBounds = false,
                TextContainerInset = UIEdgeInsets.Zero,
                UserInteractionEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            typeDescription.ApplyTheme();
            typeDescription.TextContainer.LineFragmentPadding = 0f;

            ContentView.Add(typeDescription);
            ContentView.Add(descriptionLabel);

            ContentView.AddConstraints(new[]
            {
                descriptionLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                descriptionLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                descriptionLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),

                typeDescription.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                typeDescription.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                typeDescription.TopAnchor.ConstraintEqualTo(descriptionLabel.BottomAnchor, 4f),
                typeDescription.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
            });
        }

        public void Initialize(ObjectLink link)
        {
            descriptionLabel.Text = link.Description;
            typeDescription.Text = link.TypeInfo.DescriptionSimple;

            var clickable = false;
            if (link.IsReverse)
                clickable = link.FromObjectType == ObjectType.Document || link.FromObjectType == ObjectType.Contact || link.FromObjectType == ObjectType.Shortcode;
            else
                clickable = link.ToObjectType == ObjectType.Document || link.ToObjectType == ObjectType.Contact || link.ToObjectType == ObjectType.Shortcode;
            SelectionStyle = clickable ? UITableViewCellSelectionStyle.Default : UITableViewCellSelectionStyle.None;
        }
    }
}