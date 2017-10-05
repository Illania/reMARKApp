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
            topLabel = new UILabel
            {
                Font = Theme.DefaultFont.WithRelativeSize(-2f),
                TextColor = Theme.DarkGray,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            bottomTextView = new UITextView
            {
                Editable = false,
                ScrollEnabled = false,
                TextContainerInset = UIEdgeInsets.Zero,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            bottomTextView.TextContainer.LineFragmentPadding = 0f;

            ContentView.Add(topLabel);
            ContentView.Add(bottomTextView);

            ContentView.AddConstraints(new[]
            {
                topLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                topLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                topLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 4f),
                topLabel.HeightAnchor.ConstraintEqualTo(22f),

                bottomTextView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                bottomTextView.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                bottomTextView.TopAnchor.ConstraintEqualTo(topLabel.BottomAnchor, 4f),
                bottomTextView.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f),
            });
        }

        public void Initialize(string type, string info, bool enableDataDetection = false)
        {
            Initialize(type, new NSAttributedString(info, new UIStringAttributes { Font = Theme.DefaultFont }), enableDataDetection);
        }

        public void Initialize(string type, NSAttributedString info, bool dataDetection = false)
        {
            topLabel.Text = type.ToUpper();
            bottomTextView.AttributedText = info;
            bottomTextView.DataDetectorTypes = dataDetection ? UIDataDetectorType.All : UIDataDetectorType.None;
        }
    }
}