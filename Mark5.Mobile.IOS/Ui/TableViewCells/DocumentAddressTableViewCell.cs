using System.IO;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class DocumentAddressTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(DocumentAddressTableViewCell));
        public static readonly NSString CompactId = new NSString(nameof(DocumentAddressTableViewCell) + "_Compact");

        readonly UILabel typeLabel;
        readonly UILabel topLabel;
        readonly UILabel bottomLabel;
        readonly UIImageView iconImage;

        public DocumentAddressTableViewCell(NSString reuseIdentifier)
            : base(UITableViewCellStyle.Default, reuseIdentifier)
        {
            typeLabel = new UILabel
            {
                Font = Theme.DefaultFont.WithSize(11f),
                TextColor = Theme.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            typeLabel.Layer.BackgroundColor = Theme.DarkGray.CGColor;
            typeLabel.Layer.CornerRadius = 3f;

            topLabel = new UILabel
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

            ContentView.Add(typeLabel);
            ContentView.Add(topLabel);
            ContentView.Add(iconImage);

            if (ReuseIdentifier == DefaultId)
            {
                bottomLabel = new UILabel
                {
                    Font = Theme.DefaultFont.WithRelativeSize(-2f),
                    TextColor = Theme.DarkGray,
                    Lines = 1,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                ContentView.Add(bottomLabel);

                ContentView.AddConstraints(new[]
                {
                    typeLabel.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                    typeLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                    typeLabel.WidthAnchor.ConstraintEqualTo(27f),
                    typeLabel.HeightAnchor.ConstraintEqualTo(17f),

                    topLabel.LeadingAnchor.ConstraintEqualTo(typeLabel.TrailingAnchor, 8f),
                    topLabel.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                    topLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 8f),
                    topLabel.HeightAnchor.ConstraintEqualTo(22f),

                    bottomLabel.LeadingAnchor.ConstraintEqualTo(typeLabel.TrailingAnchor, 8f),
                    bottomLabel.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                    bottomLabel.TopAnchor.ConstraintEqualTo(topLabel.BottomAnchor, 4f),
                    bottomLabel.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f),
                    bottomLabel.HeightAnchor.ConstraintEqualTo(18f),

                    iconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                    iconImage.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                    iconImage.WidthAnchor.ConstraintEqualTo(20f),
                    iconImage.HeightAnchor.ConstraintEqualTo(20f)
                });
            }
            else
            {
                ContentView.AddConstraints(new[]
                {
                    typeLabel.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                    typeLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                    typeLabel.WidthAnchor.ConstraintEqualTo(27f),
                    typeLabel.HeightAnchor.ConstraintEqualTo(17f),

                    topLabel.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                    topLabel.LeadingAnchor.ConstraintEqualTo(typeLabel.TrailingAnchor, 8f),
                    topLabel.TrailingAnchor.ConstraintEqualTo(iconImage.LeadingAnchor, -8f),
                    topLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 11f),
                    topLabel.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -11f),
                    topLabel.HeightAnchor.ConstraintEqualTo(22f),

                    iconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                    iconImage.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                    iconImage.WidthAnchor.ConstraintEqualTo(20f),
                    iconImage.HeightAnchor.ConstraintEqualTo(20f)
                });
            }
        }

        public void Initialize(DocumentAddress da)
        {
            typeLabel.Text = da.AddressType.ToString().ToUpper();

            if (ReuseIdentifier == DefaultId)
            {
                topLabel.Text = string.IsNullOrWhiteSpace(da.Name) ? da.Address : $"{da.Name} <{da.Address}>";
                bottomLabel.Text = da.FullAttention;
            }
            else
                topLabel.Text = string.IsNullOrWhiteSpace(da.Name) ? da.Address : $"{da.Name} <{da.Address}>";
        }
    }
}