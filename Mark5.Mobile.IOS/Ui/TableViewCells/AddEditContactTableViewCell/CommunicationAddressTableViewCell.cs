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

        public CommunicationAddressTableViewCell()
          : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            prefixLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
            };
            prefixLabel.Text = "Prefix";
            prefixLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(prefixLabel);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(prefixLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(prefixLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
            });

            chevronButton = new UIButton(UIButtonType.ContactAdd); //TODO someone was suggesting to put a cell inside the button and to get the chevron
            chevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            chevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(chevronButton);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, prefixLabel, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
            });

            numberTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = "Number",
            };
            ContentView.Add(numberTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(numberTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(numberTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(numberTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
            });

            descriptionTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = "Description",
            };
            ContentView.Add(descriptionTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, numberTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
            });
        }
    }
}
