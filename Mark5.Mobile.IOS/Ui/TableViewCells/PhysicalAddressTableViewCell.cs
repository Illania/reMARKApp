using System;
using System.IO;
using System.Text;
using Contacts;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class PhysicalAddressTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(PhysicalAddressTableViewCell));

        readonly UILabel topLabel;
        readonly UITextView bottomTextView;
        readonly UIImageView iconImage;

        public PhysicalAddressTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            topLabel = new UILabel
            {
                Text = Localization.GetString("address").ToUpper(),
                Font = Theme.DefaultFont.WithRelativeSize(-2f),
                TextColor = Theme.DarkGray,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            bottomTextView = new UITextView
            {
                Selectable = false,
                Editable = false,
                ScrollEnabled = false,
                ClipsToBounds = false,
                TextContainerInset = UIEdgeInsets.Zero,
                UserInteractionEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            bottomTextView.ApplyTheme();
            bottomTextView.TextContainer.LineFragmentPadding = 0f;

            iconImage = new UIImageView
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "map.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            ContentView.Add(topLabel);
            ContentView.Add(bottomTextView);
            ContentView.Add(iconImage);

            ContentView.AddConstraints(new[]
            {
                topLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                topLabel.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                topLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 8f),
                topLabel.BottomAnchor.ConstraintEqualTo(bottomTextView.TopAnchor, -4f),
                topLabel.HeightAnchor.ConstraintEqualTo(20f),

                bottomTextView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                bottomTextView.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                bottomTextView.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f),

                iconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                iconImage.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                iconImage.WidthAnchor.ConstraintEqualTo(20f),
                iconImage.HeightAnchor.ConstraintEqualTo(20f),
            });
        }

        public void Initialize(PhysicalAddress pa)
        {
            var cnAddress = new CNMutablePostalAddress
            {
                Street = pa.Street,
                State = pa.Area,
                PostalCode = pa.ZipCode,
                City = pa.City,
                Country = pa.Country.Name,
            };

            var formattedAddress = new CNPostalAddressFormatter().GetStringFromPostalAddress(cnAddress);

            if (pa.Type != null && pa.Type.Id > 0 && !string.IsNullOrEmpty(pa.Type.Name))
            {
                var sb = new StringBuilder();
                sb.Append(pa.Type.Name).AppendLine();
                sb.Append(formattedAddress);
                formattedAddress = sb.ToString();
            }

            bottomTextView.Text = formattedAddress;
        }
    }
}