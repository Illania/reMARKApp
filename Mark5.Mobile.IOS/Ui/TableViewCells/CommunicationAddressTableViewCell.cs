using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class CommunicationAddressTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(CommunicationAddressTableViewCell));
        public static readonly NSString CompactId = new NSString(nameof(CommunicationAddressTableViewCell) + "_Compact");

        readonly UILabel topLabel;
        readonly UILabel middleLabel;
        readonly UILabel bottomLabel;
        readonly UIImageView iconImage;

        public CommunicationAddressTableViewCell(NSString reuseIdentifier)
            : base(UITableViewCellStyle.Default, reuseIdentifier)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            topLabel = new UILabel
            {
                Font = Theme.DefaultFont.WithRelativeSize(-2f),
                TextColor = Theme.DarkGray,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            middleLabel = new UILabel
            {
                Font = Theme.DefaultFont,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            iconImage = new UIImageView
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "email.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            ContentView.Add(topLabel);
            ContentView.Add(middleLabel);
            ContentView.Add(iconImage);

            if (ReuseIdentifier == DefaultId)
            {
                bottomLabel = new UILabel
                {
                    Font = Theme.DefaultFont.WithRelativeSize(-1f),
                    TextColor = Theme.DarkGray,
                    Lines = 1,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                ContentView.Add(bottomLabel);

                ContentView.AddConstraints(new[]
                {
                    topLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                    topLabel.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                    topLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 8f),
                    topLabel.BottomAnchor.ConstraintEqualTo(middleLabel.TopAnchor, -4f),

                    middleLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                    middleLabel.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                    middleLabel.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),

                    bottomLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                    bottomLabel.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                    bottomLabel.TopAnchor.ConstraintEqualTo(middleLabel.BottomAnchor, 2f),
                    bottomLabel.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f),

                    iconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                    iconImage.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                    iconImage.WidthAnchor.ConstraintEqualTo(20f),
                    iconImage.HeightAnchor.ConstraintEqualTo(20f),
                });
            }
            else
            {
                ContentView.AddConstraints(new[]
                {
                    topLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                    topLabel.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                    topLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 8f),
                    topLabel.BottomAnchor.ConstraintEqualTo(middleLabel.TopAnchor, -4f),

                    middleLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                    middleLabel.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                    middleLabel.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f),

                    iconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                    iconImage.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                    iconImage.WidthAnchor.ConstraintEqualTo(20f),
                    iconImage.HeightAnchor.ConstraintEqualTo(20f),
                });
            }
        }

        public void Initialize(CommunicationAddress ca)
        {
            topLabel.Text = GetTypeText(ca.Type).ToUpper();
            middleLabel.Text = GetAddressFormatted(ca);

            switch (ca.Type)
            {
                case CommunicationAddressType.Email:
                    iconImage.Image = UIImage.FromBundle(Path.Combine("icons", "email.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                case CommunicationAddressType.Mobile:
                    iconImage.Image = UIImage.FromBundle(Path.Combine("icons", "phone.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                case CommunicationAddressType.Phone:
                    iconImage.Image = UIImage.FromBundle(Path.Combine("icons", "phone.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                default:
                    iconImage.Image = null;
                    break;
            }

            if (ReuseIdentifier == DefaultId)
                bottomLabel.Text = ca.Description;
        }

        static string GetTypeText(CommunicationAddressType type)
        {
            if (type == CommunicationAddressType.Email)
                return Localization.GetString("email");
            if (type == CommunicationAddressType.Fax)
                return Localization.GetString("fax");
            if (type == CommunicationAddressType.IM)
                return Localization.GetString("im");
            if (type == CommunicationAddressType.Internal)
                return Localization.GetString("internal");
            if (type == CommunicationAddressType.Mobile)
                return Localization.GetString("mobile");
            if (type == CommunicationAddressType.Phone)
                return Localization.GetString("phone");
            if (type == CommunicationAddressType.Skype)
                return Localization.GetString("skype");
            if (type == CommunicationAddressType.System)
                return Localization.GetString("system");
            if (type == CommunicationAddressType.Telex)
                return Localization.GetString("telex");

            return string.Empty;
        }

        static string GetAddressFormatted(CommunicationAddress ca)
        {
            if (ca.Address.Contains("|"))
                if (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone || ca.Type == CommunicationAddressType.Fax)
                {
                    var addressParts = ca.Address.Split('|');
                    if (addressParts[0].Length > 0)
                        addressParts[0] = "+" + addressParts[0];

                    return string.Join(" ", addressParts.Where(s => !string.IsNullOrWhiteSpace(s)));
                }

            return ca.Address;
        }
    }
}