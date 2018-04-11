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
        readonly UIImageView favoriteIndicatorImage;
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

            favoriteIndicatorImage = new UIImageView
            {
                Image = UIImage.FromBundle("Checkmark).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(favoriteIndicatorImage);

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

            expandButton = new UIButton
            {
                ImageEdgeInsets = new UIEdgeInsets(-10f, -10f, -10f, -10f),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            expandButton.SetImage(UIImage.FromBundle("Expand").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            ContentView.Add(expandButton);

            ContentView.AddConstraints(new[]
            {
                folderIconImage.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                folderIconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                folderIconImage.HeightAnchor.ConstraintEqualTo(20f),
                folderIconImage.WidthAnchor.ConstraintEqualTo(20f),

                favoriteIndicatorImage.CenterXAnchor.ConstraintEqualTo(folderIconImage.TrailingAnchor, -5f),
                favoriteIndicatorImage.CenterYAnchor.ConstraintEqualTo(folderIconImage.BottomAnchor, -5f),
                favoriteIndicatorImage.HeightAnchor.ConstraintEqualTo(15f),
                favoriteIndicatorImage.WidthAnchor.ConstraintEqualTo(15f),

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
            favoriteIndicatorImage.TintColor = Theme.TintColor;

            folderIconImage.Image = GetIcon(folder);
            favoriteIndicatorImage.Hidden = !folder.Subscribed;
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
            favoriteIndicatorImage.TintColor = Theme.DarkGray;
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