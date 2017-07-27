using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class CommunicationAddressTableViewCell : AddEditContactTableViewCell
    {
        public static readonly NSString Key = new NSString("CommunicationAddressTableViewCell");

        readonly UITextField numberTextField;
        readonly UITextField descriptionTextField;
        readonly UIButton chevronButton;
        readonly UILabel prefixLabel;
        readonly UISwitch preferrableSwitch;
        readonly UILabel preferrableLabel;

        public CommunicationAddressTableViewCell()
          : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            prefixLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
            };
            prefixLabel.Text = Localization.GetString("prefix");
            prefixLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(prefixLabel);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(prefixLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(prefixLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(prefixLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            chevronButton = new UIButton();
            chevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            chevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(chevronButton);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, prefixLabel, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, prefixLabel, NSLayoutAttribute.CenterY, 1f, 0f),
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            var disclosureCell = new UITableViewCell();
            disclosureCell.TranslatesAutoresizingMaskIntoConstraints = false;
            disclosureCell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            disclosureCell.UserInteractionEnabled = false;
            chevronButton.AddSubview(disclosureCell);
            chevronButton.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(disclosureCell, NSLayoutAttribute.Left, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(disclosureCell, NSLayoutAttribute.Right, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(disclosureCell, NSLayoutAttribute.Top, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(disclosureCell, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Bottom, 1f, 0f),
            });

            var verticalSeparator = GetVerticalSeparator();
            ContentView.AddSubview(verticalSeparator);
            ContentView.AddConstraints(new[]
            {
               NSLayoutConstraint.Create(verticalSeparator, NSLayoutAttribute.Left, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
               NSLayoutConstraint.Create(verticalSeparator, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
               NSLayoutConstraint.Create(verticalSeparator, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, prefixLabel, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
            });

            numberTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("number"),
            };
            ContentView.Add(numberTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(numberTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(numberTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, verticalSeparator, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(numberTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(numberTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            var horizontalSeparator = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Top, NSLayoutRelation.Equal, numberTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
            });

            descriptionTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("description"),
            };
            ContentView.Add(descriptionTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            var horizontalSeparator2 = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator2);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Top, NSLayoutRelation.Equal, descriptionTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
            });

            preferrableLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
            };
            preferrableLabel.Text = Localization.GetString("preferrable");
            ContentView.Add(preferrableLabel);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(preferrableLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator2, NSLayoutAttribute.TopMargin, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(preferrableLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(preferrableLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
            });

            preferrableSwitch = new UISwitch
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            preferrableSwitch.SetContentHuggingPriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(preferrableSwitch);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(preferrableSwitch, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator2, NSLayoutAttribute.TopMargin, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(preferrableSwitch, NSLayoutAttribute.Left, NSLayoutRelation.Equal, preferrableLabel, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(preferrableSwitch, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(preferrableSwitch, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
            });

            preferrableSwitch.ValueChanged += PreferrableSwitch_ValueChanged;
        }

        void PreferrableSwitch_ValueChanged(object sender, System.EventArgs e)
        {

        }
    }
}
