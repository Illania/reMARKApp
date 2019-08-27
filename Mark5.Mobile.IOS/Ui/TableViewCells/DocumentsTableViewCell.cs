using System.Linq;
using System.Text.RegularExpressions;
using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class DocumentsTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(DocumentsTableViewCell));
        public static readonly NSString CompactId = new NSString(nameof(DocumentsTableViewCell) + "_Compact");
        public static readonly NSString ExternalId = new NSString(nameof(DocumentsTableViewCell) + "_External");
        public static readonly NSString UploadId = new NSString(nameof(DocumentsTableViewCell) + "_Upload");

        readonly UIStackView categoriesStackView;
        readonly UIImageView directionIndicatorImageView;
        readonly UIImageView unreadIndicatorImageView;
        readonly UIImageView attachmentsIndicatorImageView;
        readonly UIImageView commentsIndicatorImageView;
        readonly UILabel topLabel;
        readonly UILabel dateLabel;
        readonly UILabel middleLabel;
        readonly UITextView bottomLabel;

        readonly bool unreadIndicatorMe = PlatformConfig.Preferences.UnreadIndicatorMe;
        readonly bool showCreatorOutgoing = PlatformConfig.Preferences.ShowCreatorOutgoing;

        public DocumentsTableViewCell(NSString reuseIdentifier)
            : base(UITableViewCellStyle.Default, reuseIdentifier)
        {
            if (reuseIdentifier == DefaultId || reuseIdentifier == CompactId || reuseIdentifier == ExternalId)
            {
                SelectionStyle = UITableViewCellSelectionStyle.Default;
                Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }
            else
            {
                SelectionStyle = UITableViewCellSelectionStyle.None;
                Accessory = UITableViewCellAccessory.None;
            }

            var leadingMarginGuide = new UILayoutGuide();
            ContentView.AddLayoutGuide(leadingMarginGuide);

            leadingMarginGuide.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor).Active = true;
            var leadingMarginWidthAnchor = leadingMarginGuide.WidthAnchor.ConstraintEqualTo(0f);
            leadingMarginWidthAnchor.SetIdentifier("leadingMarginWidth");
            leadingMarginWidthAnchor.Active = true;

            if (reuseIdentifier == DefaultId || reuseIdentifier == CompactId || reuseIdentifier == ExternalId)
            {
                categoriesStackView = new UIStackView
                {
                    Axis = UILayoutConstraintAxis.Vertical,
                    Alignment = UIStackViewAlignment.Fill,
                    Distribution = UIStackViewDistribution.FillEqually,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                ContentView.AddSubview(categoriesStackView);
                ContentView.AddConstraints(new[]
                {
                    categoriesStackView.TrailingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor, -8f),
                    categoriesStackView.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 4f),
                    categoriesStackView.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -4f),
                    categoriesStackView.WidthAnchor.ConstraintEqualTo(4f),
                });
            }

            topLabel = new UILabel
            {
                Font = Theme.DefaultBoldFont,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            ContentView.AddSubview(topLabel);
            ContentView.AddConstraints(new[]
            {
                topLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 8f),
                topLabel.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor, 15f + 8f),
                topLabel.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
            });

            if (reuseIdentifier == DefaultId || reuseIdentifier == CompactId || reuseIdentifier == ExternalId)
            {
                dateLabel = new UILabel
                {
                    Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                    TextColor = Theme.DarkGray,
                    Lines = 1,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                dateLabel.SetContentHuggingPriority(1000f, UILayoutConstraintAxis.Horizontal);
                dateLabel.SetContentCompressionResistancePriority(1000f, UILayoutConstraintAxis.Horizontal);

                ContentView.AddSubview(dateLabel);
                ContentView.AddConstraints(new[]
                {
                    dateLabel.LeadingAnchor.ConstraintEqualTo(topLabel.TrailingAnchor, 8f),
                    dateLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                    dateLabel.CenterYAnchor.ConstraintEqualTo(topLabel.CenterYAnchor)
                });
            }
            else
            {
                ContentView.AddConstraints(new[]
                {
                    topLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor)
                });
            }

            middleLabel = new UILabel
            {
                Font = Theme.DefaultFont,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            ContentView.AddSubview(middleLabel);
            ContentView.AddConstraints(new[]
            {
                middleLabel.LeadingAnchor.ConstraintEqualTo(topLabel.LeadingAnchor),
                middleLabel.TopAnchor.ConstraintEqualTo(topLabel.BottomAnchor, 4f),
                middleLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
            });

            if (reuseIdentifier == DefaultId)
            {
                bottomLabel = new UITextView
                {
                    Font = Theme.DefaultFont.WithRelativeSize(-2f),
                    TextColor = Theme.DarkGray,
                    Selectable = false,
                    Editable = false,
                    ScrollEnabled = false,
                    ClipsToBounds = false,
                    TextContainerInset = UIEdgeInsets.Zero,
                    UserInteractionEnabled = false,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = UIColor.Clear,
                };
                bottomLabel.TextContainer.MaximumNumberOfLines = 3;
                bottomLabel.TextContainer.LineFragmentPadding = 0f;

                ContentView.AddSubview(bottomLabel);
                ContentView.AddConstraints(new[]
                {
                    bottomLabel.LeadingAnchor.ConstraintEqualTo(topLabel.LeadingAnchor),
                    bottomLabel.TopAnchor.ConstraintEqualTo(middleLabel.BottomAnchor, 4f),
                    bottomLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                    bottomLabel.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f),
                });
            }
            else
            {
                ContentView.AddConstraints(new[]
                {
                    middleLabel.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f),
                });
            }

            if (reuseIdentifier == DefaultId || reuseIdentifier == CompactId || reuseIdentifier == UploadId)
            {
                directionIndicatorImageView = new UIImageView
                {
                    ContentMode = UIViewContentMode.ScaleToFill,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                ContentView.AddSubview(directionIndicatorImageView);
                ContentView.AddConstraints(new[]
                {
                    directionIndicatorImageView.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor),
                    directionIndicatorImageView.CenterYAnchor.ConstraintEqualTo(topLabel.CenterYAnchor),
                    directionIndicatorImageView.WidthAnchor.ConstraintEqualTo(15f),
                    directionIndicatorImageView.HeightAnchor.ConstraintEqualTo(15f),
                });
            }

            if (reuseIdentifier == DefaultId || reuseIdentifier == CompactId)
            {
                unreadIndicatorImageView = new UIImageView
                {
                    ContentMode = UIViewContentMode.ScaleToFill,
                    Image = UIImage.FromBundle("Full-Dot").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                ContentView.AddSubview(unreadIndicatorImageView);
                ContentView.AddConstraints(new[]
                {
                    unreadIndicatorImageView.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor),
                    unreadIndicatorImageView.TopAnchor.ConstraintEqualTo(directionIndicatorImageView.BottomAnchor, 4f),
                    unreadIndicatorImageView.WidthAnchor.ConstraintEqualTo(15f),
                    unreadIndicatorImageView.HeightAnchor.ConstraintEqualTo(15f),
                });
            }

            if (reuseIdentifier == DefaultId)
            {
                attachmentsIndicatorImageView = new UIImageView
                {
                    ContentMode = UIViewContentMode.ScaleToFill,
                    Image = UIImage.FromBundle("Attachment-Small").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                ContentView.AddSubview(attachmentsIndicatorImageView);
                ContentView.AddConstraints(new[]
                {
                    attachmentsIndicatorImageView.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor),
                    attachmentsIndicatorImageView.TopAnchor.ConstraintEqualTo(unreadIndicatorImageView.BottomAnchor, 4f),
                    attachmentsIndicatorImageView.WidthAnchor.ConstraintEqualTo(15f),
                    attachmentsIndicatorImageView.HeightAnchor.ConstraintEqualTo(15f),
                });

                commentsIndicatorImageView = new UIImageView
                {
                    ContentMode = UIViewContentMode.ScaleToFill,
                    Image = UIImage.FromBundle("Message-Small").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                ContentView.AddSubview(commentsIndicatorImageView);
                ContentView.AddConstraints(new[]
                {
                    commentsIndicatorImageView.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor),
                    commentsIndicatorImageView.TopAnchor.ConstraintEqualTo(attachmentsIndicatorImageView.BottomAnchor, 4f),
                    commentsIndicatorImageView.WidthAnchor.ConstraintEqualTo(15f),
                    commentsIndicatorImageView.HeightAnchor.ConstraintEqualTo(15f)
                });
            }
        }

        public void Initialize(DocumentPreview dp)
        {
            if (ReuseIdentifier == DefaultId || ReuseIdentifier == CompactId || ReuseIdentifier == ExternalId)
                InitializeCategories(dp);
            InitializeTopLabel(dp);
            if (ReuseIdentifier == DefaultId || ReuseIdentifier == CompactId || ReuseIdentifier == ExternalId)
                InitializeDate(dp);
            InitializeMiddleLabel(dp);
            if (ReuseIdentifier == DefaultId)
                InitializeBottomLabel(dp);
            if (ReuseIdentifier == DefaultId || ReuseIdentifier == CompactId || ReuseIdentifier == UploadId)
                InitializeDirectionIndicator(dp);
            if (ReuseIdentifier == DefaultId || ReuseIdentifier == CompactId)
                InitializeUnreadIndicator(dp);
            if (ReuseIdentifier == DefaultId)
            {
                InitializeAttachmentIndicator(dp);
                InitializeCommentsIndicator(dp);
            }
        }

        void InitializeCategories(DocumentPreview dp)
        {
            if (categoriesStackView == null)
                return;

            categoriesStackView.Subviews.ForEach(v => v.RemoveFromSuperview());
            foreach (var c in dp.Categories)
            {
                var v = new UIView
                {
                    BackgroundColor = UI.UIColorFromHexString(c.HexColor),
                    UserInteractionEnabled = false
                };
                categoriesStackView.AddArrangedSubview(v);
            }
        }

        void InitializeTopLabel(DocumentPreview dp)
        {
            if (topLabel == null)
                return;

            string text = null;
            if (dp.Direction == DocumentDirection.Incoming)
            {
                var address = dp.Addresses.FirstOrDefault(da => da.AddressType == DocumentAddressType.From);
                if (address != null)
                    text = string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
            }
            else
            {
                if (showCreatorOutgoing)
                    text = dp.Creator;
                else
                {
                    var address = dp.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
                    if (address != null)
                        text = string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
                }
            }

            topLabel.Text = text ?? string.Empty;
        }

        void InitializeDate(DocumentPreview dp)
        {
            if (dateLabel == null)
                return;

            var timestampMs = dp.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime()
                                .ConvertUtcToUserTime()
                                .ConvertDateTimeToTimestampMilliseconds();
            dateLabel.Text = PlatformConfig.Preferences.ShowTimeForOldEmails ?
                timestampMs.FormatUserTimestampAsCompactMediumDateTimeString() :
                timestampMs.FormatUserTimestampAsCompactShortDateTimeString();
        }

        void InitializeMiddleLabel(DocumentPreview dp)
        {
            if (middleLabel == null)
                return;

            middleLabel.Text = dp.Subject;
        }

        void InitializeBottomLabel(DocumentPreview dp)
        {
            if (bottomLabel == null)
                return;

            bottomLabel.Text = !string.IsNullOrWhiteSpace(dp.Preview) ? Regex.Replace(dp.Preview, @"\r\n?|\n", "  ", RegexOptions.Multiline) : Localization.GetString("no_content");
        }

        void InitializeDirectionIndicator(DocumentPreview dp)
        {
            if (directionIndicatorImageView == null)
                return;

            UIImage i = null;
            switch (dp.Direction)
            {
                case DocumentDirection.Incoming:
                    i = UIImage.FromBundle("Incoming").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                case DocumentDirection.Outgoing:
                    i = UIImage.FromBundle("Outgoing").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                case DocumentDirection.Draft:
                    i = UIImage.FromBundle("Edit-Small").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
            }
            directionIndicatorImageView.Image = i;
        }

        void InitializeUnreadIndicator(DocumentPreview dp)
        {
            if (unreadIndicatorImageView == null)
                return;

            var show = unreadIndicatorMe ? !dp.IsReadByCurrent : !dp.IsReadByAnyone;
            unreadIndicatorImageView.Alpha = show ? 1f : 0f;
        }

        void InitializeAttachmentIndicator(DocumentPreview dp)
        {
            if (attachmentsIndicatorImageView == null)
                return;

            attachmentsIndicatorImageView.Alpha = dp.AttachmentsCount > 0 ? 1f : 0f;
        }

        void InitializeCommentsIndicator(DocumentPreview dp)
        {
            if (commentsIndicatorImageView == null)
                return;

            commentsIndicatorImageView.Alpha = dp.CommentsCount > 0 ? 1f : 0f;
        }

        public override void SetSelected(bool selected, bool animated)
        {
            UIColor[] colors = null;

            if (categoriesStackView != null)
            {
                colors = new UIColor[categoriesStackView.Subviews.Length];
                for (var i = 0; i < categoriesStackView.Subviews.Length; i++)
                    colors[i] = categoriesStackView.Subviews[i].BackgroundColor;
            }

            base.SetSelected(selected, animated);

            if (colors != null)
            {
                for (var i = 0; i < categoriesStackView.Subviews.Length; i++)
                    categoriesStackView.Subviews[i].BackgroundColor = colors[i];

                colors = null;
            }
        }

        public override void SetHighlighted(bool highlighted, bool animated)
        {
            UIColor[] colors = null;

            if (categoriesStackView != null)
            {
                colors = new UIColor[categoriesStackView.Subviews.Length];
                for (var i = 0; i < categoriesStackView.Subviews.Length; i++)
                    colors[i] = categoriesStackView.Subviews[i].BackgroundColor;
            }

            base.SetHighlighted(highlighted, animated);

            if (colors != null)
            {
                for (var i = 0; i < categoriesStackView.Subviews.Length; i++)
                    categoriesStackView.Subviews[i].BackgroundColor = colors[i];

                colors = null;
            }
        }

        public override void SetEditing(bool editing, bool animated)
        {
            base.SetEditing(editing, animated);

            var c = ContentView?.Constraints?.FirstOrDefault(nslc => nslc.GetIdentifier() == "leadingMarginWidth");
            if (c != null)
                c.Constant = editing ? 16f : 0f;

            if (animated)
                AnimateNotify(.25d, ContentView.LayoutIfNeeded, null);
            else
                LayoutIfNeeded();
        }
    }
}