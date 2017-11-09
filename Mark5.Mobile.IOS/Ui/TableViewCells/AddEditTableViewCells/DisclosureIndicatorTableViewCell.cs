using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCell
{
    public class DisclosureIndicatorTableViewCell : AddEditTableViewCell
    {
        public static readonly NSString Key = new NSString("DisclosureIndicatorTableViewCell");

        protected UILabel TitleLabel { get; set; }
        protected UILabel ContentLabel { get; set; }

        NSLayoutConstraint leftContentConstraint;

        readonly UIButton chevronButton;

        public DisclosureIndicatorTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            TitleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkBlue,
            };
            TitleLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            TitleLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(TitleLabel);

            ContentLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Lines = 0,
            };
            ContentView.AddSubview(ContentLabel);

            chevronButton = GetChevron();
            chevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            chevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(chevronButton);

            ContentView.AddConstraints(new[]
            {
                TitleLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, VerticalMargin),
                TitleLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -VerticalMargin),
                TitleLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),

                ContentLabel.HeightAnchor.ConstraintGreaterThanOrEqualTo(20f),
                ContentLabel.TopAnchor.ConstraintEqualTo(TitleLabel.TopAnchor),
                ContentLabel.BottomAnchor.ConstraintEqualTo(TitleLabel.BottomAnchor),
                leftContentConstraint = ContentLabel.LeadingAnchor.ConstraintEqualTo(TitleLabel.TrailingAnchor, InnerHorizontalMargin),

                chevronButton.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                chevronButton.LeadingAnchor.ConstraintEqualTo(ContentLabel.TrailingAnchor, InnerHorizontalMargin),
                chevronButton.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
            });
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                leftContentConstraint.Constant = 0;
            }
            else
            {
                leftContentConstraint.Constant = InnerHorizontalMargin;
                TitleLabel.Text = title;
            }
        }

        public void SetContent(string content)
        {
            ContentLabel.Text = content;
        }

        public void RemoveChevron()
        {
            chevronButton.RemoveFromSuperview();
        }

        public override void Reset()
        {
            SetErrorState(false);

            TitleLabel.Text = string.Empty;
            ContentLabel.Text = string.Empty;
        }
    }
}
