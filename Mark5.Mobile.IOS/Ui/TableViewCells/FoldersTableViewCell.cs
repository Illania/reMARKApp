using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class FoldersTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(FoldersTableViewCell));

        public UITapGestureRecognizer ExpandGestureRecognizer
        {
            get => expandButton.GestureRecognizers.OfType<UITapGestureRecognizer>().FirstOrDefault();
            set
            {
                expandButton.GestureRecognizers?.ForEach(expandButton.RemoveGestureRecognizer);
                expandButton.AddGestureRecognizer(value);
            }
        }

        readonly UIImageView folderIconImage;
        readonly UIImageView subscribedIndicatorImage;
        readonly UIImageView offlineIndicatorImage;
        readonly UILabel nameLabel;
        readonly UIButton expandButton;

        readonly NSLayoutConstraint offlineIndicatorWidthConstraint;
        readonly NSLayoutConstraint offlineIndicatorLeadingConstraint;

        public FoldersTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            folderIconImage = new UIImageView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            ContentView.Add(folderIconImage);

            subscribedIndicatorImage = new UIImageView
            {
                Image = UIImage.FromBundle("Checkmark").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(subscribedIndicatorImage);

            offlineIndicatorImage = new UIImageView
            {
                Image = UIImage.FromBundle("Offline").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(offlineIndicatorImage);

            nameLabel = new UILabel
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.Black,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(nameLabel);

            expandButton = new LargeHitAreaButton
            {
                ImageEdgeInsets = new UIEdgeInsets(-10f, -10f, -10f, -10f),
                TranslatesAutoresizingMaskIntoConstraints = false,
                HitAreaMargin = 15
            };

            expandButton.SetImage(UIImage.FromBundle("Expand").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            ContentView.Add(expandButton);

            ContentView.AddConstraints(new[]
            {
                folderIconImage.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                folderIconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                folderIconImage.HeightAnchor.ConstraintEqualTo(20f),
                folderIconImage.WidthAnchor.ConstraintEqualTo(20f),

                subscribedIndicatorImage.CenterXAnchor.ConstraintEqualTo(folderIconImage.TrailingAnchor, -5f),
                subscribedIndicatorImage.CenterYAnchor.ConstraintEqualTo(folderIconImage.BottomAnchor, -5f),
                subscribedIndicatorImage.HeightAnchor.ConstraintEqualTo(15f),
                subscribedIndicatorImage.WidthAnchor.ConstraintEqualTo(15f),

                offlineIndicatorLeadingConstraint = offlineIndicatorImage.LeadingAnchor.ConstraintEqualTo(folderIconImage.TrailingAnchor, 12f),
                offlineIndicatorImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                offlineIndicatorImage.HeightAnchor.ConstraintEqualTo(15f),
                offlineIndicatorWidthConstraint = offlineIndicatorImage.WidthAnchor.ConstraintEqualTo(15f),

                nameLabel.LeadingAnchor.ConstraintEqualTo(offlineIndicatorImage.TrailingAnchor, 8f),
                nameLabel.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                nameLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 4f),
                nameLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -4f),

                expandButton.LeadingAnchor.ConstraintEqualTo(nameLabel.TrailingAnchor, 8f),
                expandButton.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                expandButton.HeightAnchor.ConstraintEqualTo(40f),
                expandButton.WidthAnchor.ConstraintEqualTo(40f),
                expandButton.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor)
            });
        }

        public void Initialize(Folder folder, bool folderIsOffline)
        {
            UserInteractionEnabled = true;
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            nameLabel.TextColor = Theme.Black;
            folderIconImage.TintColor = Theme.TintColor;
            subscribedIndicatorImage.TintColor = Theme.TintColor;

            folderIconImage.Image = GetIcon(folder);
            subscribedIndicatorImage.Hidden = !folder.Subscribed;
            offlineIndicatorLeadingConstraint.Constant = folderIsOffline ? 10f : 0f;
            offlineIndicatorWidthConstraint.Constant = folderIsOffline ? 15f : 0f;
            nameLabel.Text = folder.Name;
            expandButton.Hidden = !folder.HasSubFolders;
        }

        public void Disable()
        {
            UserInteractionEnabled = false;
            SelectionStyle = UITableViewCellSelectionStyle.None;

            nameLabel.TextColor = Theme.DarkGray;
            folderIconImage.TintColor = Theme.DarkGray;
            subscribedIndicatorImage.TintColor = Theme.DarkGray;
        }

        static UIImage GetIcon(Folder folder)
        {
            if (folder.InternalType == FolderInternalType.Worktray)
                return UIImage.FromBundle("Folderslist-Worktray").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

            if (folder.Type == FolderType.Draft)
                return UIImage.FromBundle("Folderslist-Draft").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

            return UIImage.FromBundle("Folderslist-Folder").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
        }
    }
}