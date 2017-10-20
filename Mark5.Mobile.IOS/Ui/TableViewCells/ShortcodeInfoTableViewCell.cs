using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class ShortcodeInfoTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(ShortcodeInfoTableViewCell));

        readonly UILabel topLabel;
        readonly UITextView bottomTextView;

        public ShortcodeInfoTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
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

            ContentView.Add(topLabel);
            ContentView.Add(bottomTextView);

            ContentView.AddConstraints(new[]
            {
                topLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                topLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                topLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 4f),

                bottomTextView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                bottomTextView.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                bottomTextView.TopAnchor.ConstraintEqualTo(topLabel.BottomAnchor, 4f),
                bottomTextView.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f),
            });
        }

        public void Initialize(string type, string info, bool enableDataDetection = false)
        {
            if (string.IsNullOrEmpty(type))
                topLabel.Text = " ";
            else
                topLabel.Text = type.ToUpper();

            if (string.IsNullOrEmpty(info))
                bottomTextView.Text = " ";
            else
                bottomTextView.Text = info;

            bottomTextView.DataDetectorTypes = enableDataDetection ? UIDataDetectorType.All : UIDataDetectorType.None;
        }
    }
}