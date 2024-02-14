using Foundation;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.TableViewCells
{
    public class FoldersSearchResultsTableViewCell : UITableViewCell
    {
        #region Properties
        public static readonly NSString DefaultId = new("FoldersSearchResultsTableViewCell");

        readonly UILabelScalable folderNameLabel;
        readonly UILabelScalable folderPathLabel;
        readonly UIImageView folderIconImage;
        #endregion

        public FoldersSearchResultsTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            folderIconImage = new UIImageView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            ContentView.Add(folderIconImage);

            folderNameLabel = new UILabelScalable
            {
                Font = Theme.DefaultFont.CustomFont(),
                TextColor = Theme.Black,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(folderNameLabel);

            folderPathLabel = new UILabelScalable
            {
                Font = Theme.DefaultLightFont.CustomFont(),
                TextColor = Theme.DarkGray,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(folderPathLabel);

            ContentView.AddConstraints(new[]
            {
                folderIconImage.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                folderIconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                folderIconImage.HeightAnchor.ConstraintEqualTo(20f),
                folderIconImage.WidthAnchor.ConstraintEqualTo(20f),

                folderNameLabel.LeadingAnchor.ConstraintEqualTo(folderIconImage.TrailingAnchor, 8f),
                folderNameLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                folderNameLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),

                folderPathLabel.LeadingAnchor.ConstraintEqualTo(folderNameLabel.LeadingAnchor),
                folderPathLabel.TrailingAnchor.ConstraintEqualTo(folderNameLabel.TrailingAnchor),
                folderPathLabel.TopAnchor.ConstraintEqualTo(folderNameLabel.BottomAnchor, 4f),
                folderPathLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
            });
        }

        public void Initialize(Folder folder)
        {
            UserInteractionEnabled = true;
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            folderIconImage.TintColor = Theme.TintColor;
            folderNameLabel.TextColor = Theme.Black;
            folderPathLabel.TextColor = Theme.DarkGray;

            folderIconImage.Image = GetIcon(folder);
            folderNameLabel.Text = folder.Name;
            folderPathLabel.Text = folder.Path;
        }

        public void Disable()
        {
            UserInteractionEnabled = false;
            SelectionStyle = UITableViewCellSelectionStyle.None;

            folderIconImage.TintColor = Theme.DarkGray;
            folderNameLabel.TextColor = Theme.DarkGray;
            folderPathLabel.TextColor = Theme.DarkGray;
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