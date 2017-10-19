using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class FoldersSearchResultsTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString("FoldersSearchResultsTableViewCell");

        readonly UILabel folderNameLabel;
        readonly UILabel folderPathLabel;
        readonly UIImageView folderIconImage;

        public FoldersSearchResultsTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            folderNameLabel = new UILabel
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.Black,
                TextAlignment = UITextAlignment.Left,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(folderNameLabel);

            folderPathLabel = new UILabel
            {
                Font = Theme.DefaultLightFont,
                TextColor = Theme.DarkGray,
                TextAlignment = UITextAlignment.Left,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(folderPathLabel);

            folderIconImage = new UIImageView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            ContentView.Add(folderIconImage);

            ContentView.AddConstraints(new[]
            {
                folderIconImage.HeightAnchor.ConstraintEqualTo(20f),
                folderIconImage.WidthAnchor.ConstraintEqualTo(20f),
                folderIconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                folderIconImage.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),

                folderNameLabel.LeadingAnchor.ConstraintEqualTo(folderIconImage.TrailingAnchor, 8f),
                folderNameLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                folderNameLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),

                folderPathLabel.TopAnchor.ConstraintEqualTo(folderNameLabel.BottomAnchor, 4f),
                folderPathLabel.LeadingAnchor.ConstraintEqualTo(folderNameLabel.LeadingAnchor),
                folderPathLabel.TrailingAnchor.ConstraintEqualTo(folderNameLabel.TrailingAnchor),
                folderPathLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
            });
        }

        public void Initialize(Folder folder)
        {
            folderNameLabel.Text = folder.Name;
            folderPathLabel.Text = folder.Path;
            folderIconImage.Image = GetIcon(folder);

            UserInteractionEnabled = true;
            SelectionStyle = UITableViewCellSelectionStyle.Default;
        }

        public void Disable()
        {
            folderNameLabel.TextColor = Theme.DarkGray;
            folderPathLabel.TextColor = Theme.DarkGray;
            folderIconImage.TintColor = Theme.DarkGray;

            UserInteractionEnabled = false;
            SelectionStyle = UITableViewCellSelectionStyle.None;
        }

        static UIImage GetIcon(Folder folder)
        {
            if (folder.InternalType == FolderInternalType.Worktray)
                return UIImage.FromBundle(Path.Combine("icons", "folderslist", "worktray.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

            if (folder.Type == FolderType.Draft)
                return UIImage.FromBundle(Path.Combine("icons", "folderslist", "draft.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

            return UIImage.FromBundle(Path.Combine("icons", "folderslist", "folder.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
        }
    }
}