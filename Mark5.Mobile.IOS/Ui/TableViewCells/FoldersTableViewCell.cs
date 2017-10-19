using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class FoldersTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString("FoldersTableViewCell");

        public event EventHandler<Folder> ExpandCollapseClicked;

        Folder folder;

        readonly UILabel folderNameLabel;
        readonly UIImageView folderCheckedIndicatorImage;
        readonly UIImageView folderIconImage;
        readonly UIImageView offlineIndicatorImage;
        readonly UIButton expandButton;

        NSLayoutConstraint offlineIndicatorWidthConstraint;
        NSLayoutConstraint offlineIndicatorLeadingConstraint;

        public FoldersTableViewCell()
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
            folderNameLabel.SetContentHuggingPriority((float)UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(folderNameLabel);

            folderCheckedIndicatorImage = new UIImageView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Image = UIImage.FromBundle(Path.Combine("icons", "checkmark.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TintColor = Theme.Brown,
            };
            ContentView.Add(folderCheckedIndicatorImage);

            folderIconImage = new UIImageView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            ContentView.Add(folderIconImage);

            offlineIndicatorImage = new UIImageView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Image = UIImage.FromBundle(Path.Combine("icons", "offline.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TintColor = Theme.DarkBlue,
            };
            ContentView.Add(offlineIndicatorImage);

            expandButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            expandButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "expand.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            ContentView.Add(expandButton);

            offlineIndicatorWidthConstraint = offlineIndicatorImage.WidthAnchor.ConstraintEqualTo(15f);
            offlineIndicatorLeadingConstraint = offlineIndicatorImage.LeadingAnchor.ConstraintEqualTo(folderIconImage.TrailingAnchor, 10f);

            ContentView.AddConstraints(new[]
            {
                folderIconImage.HeightAnchor.ConstraintEqualTo(20f),
                folderIconImage.WidthAnchor.ConstraintEqualTo(20f),
                folderIconImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                folderIconImage.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),

                offlineIndicatorImage.HeightAnchor.ConstraintEqualTo(15f),
                offlineIndicatorWidthConstraint,
                offlineIndicatorImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                offlineIndicatorLeadingConstraint,

                expandButton.HeightAnchor.ConstraintEqualTo(44f),
                expandButton.WidthAnchor.ConstraintEqualTo(44f),
                expandButton.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                expandButton.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                expandButton.LeadingAnchor.ConstraintEqualTo(folderNameLabel.TrailingAnchor, 8f),

                folderCheckedIndicatorImage.HeightAnchor.ConstraintEqualTo(15f),
                folderCheckedIndicatorImage.WidthAnchor.ConstraintEqualTo(15f),
                folderCheckedIndicatorImage.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor, 10f),
                folderCheckedIndicatorImage.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor, 5f),

                folderNameLabel.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                folderNameLabel.LeadingAnchor.ConstraintEqualTo(offlineIndicatorImage.TrailingAnchor, 8f),
                folderNameLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),
                folderNameLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
            });
        }

        public void Initialize(Folder folder, bool folderIsOffline)
        {
            this.folder = folder;

            folderNameLabel.Text = folder.Name;
            folderIconImage.Image = GetIcon(folder);

            if (folder.Subscribed)
            {
                folderIconImage.TintColor = Theme.Brown;
                folderCheckedIndicatorImage.TintColor = Theme.Brown;
                folderCheckedIndicatorImage.Alpha = 1f;
            }
            else
            {
                folderIconImage.TintColor = Theme.TintColor;
                folderCheckedIndicatorImage.TintColor = Theme.TintColor;
                folderCheckedIndicatorImage.Alpha = 0f;
            }

            if (folderIsOffline)
            {
                offlineIndicatorLeadingConstraint.Constant = 10f;
                offlineIndicatorWidthConstraint.Constant = 15f;
            }
            else
            {
                offlineIndicatorLeadingConstraint.Constant = 0f;
                offlineIndicatorWidthConstraint.Constant = 0f;
            }

            if (folder.HasSubFolders)
            {
                expandButton.Alpha = 1f;
                expandButton.Hidden = false;
            }
            else
            {
                expandButton.Alpha = 0f;
                expandButton.Hidden = true;
            }

            UserInteractionEnabled = true;
            SelectionStyle = UITableViewCellSelectionStyle.Default;
        }

        public void Disable()
        {
            folderNameLabel.TextColor = Theme.DarkGray;
            folderIconImage.TintColor = Theme.DarkGray;
            folderCheckedIndicatorImage.TintColor = Theme.DarkGray;

            UserInteractionEnabled = false;
            SelectionStyle = UITableViewCellSelectionStyle.None;
        }

        void ExpandButtonTouchUpInside(NSObject sender) => ExpandCollapseClicked?.Invoke(this, folder);

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