using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCell
{
    public class AddressTableViewCell : MultiRowContentTableViewCell
    {
        public static readonly NSString Key = new NSString("AddressTableViewCell");

        protected DocumentAddress address;

        readonly UITextField addressTextField;
        readonly UITextField nameTextField;
        readonly UITextField attentionTextField;
        readonly UILabel typeLabel;

        public Action AddressChangedAction;

        public AddressTableViewCell() : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            addressTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("address"),
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
            };
            addressTextField.EditingChanged += AddressTextField_EditingChanged;
            ContentView.Add(addressTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, 0f),
                NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            var horizontalSeparator = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Top, NSLayoutRelation.Equal, addressTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Width, NSLayoutRelation.Equal, addressTextField, NSLayoutAttribute.Width, 1f, 0f),
            });

            nameTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("name"),
            };
            nameTextField.EditingChanged += NameTextField_EditingChanged;
            ContentView.Add(nameTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(nameTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(nameTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(nameTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
                NSLayoutConstraint.Create(nameTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, addressTextField, NSLayoutAttribute.Width, 1f, 0f),
            });

            var horizontalSeparator2 = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator2);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Top, NSLayoutRelation.Equal, nameTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Width, NSLayoutRelation.Equal, addressTextField, NSLayoutAttribute.Width, 1f, 0f),
            });

            attentionTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("attention"),
            };
            attentionTextField.EditingChanged += AttentionTextField_EditingChanged; ;
            ContentView.Add(attentionTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(attentionTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator2, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(attentionTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(attentionTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
                NSLayoutConstraint.Create(attentionTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, addressTextField, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(attentionTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, 0f),
            });

            typeLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultLightFont.WithRelativeSize(-2),
                TextColor = Theme.White,
                BackgroundColor = Theme.DarkGray,
                TextAlignment = UITextAlignment.Center,
            };
            typeLabel.Layer.CornerRadius = 10;
            typeLabel.Layer.MasksToBounds = true;
            typeLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            typeLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            ContentView.Add(typeLabel);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(typeLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, nameTextField, NSLayoutAttribute.CenterY, 1f, 0f),
                NSLayoutConstraint.Create(typeLabel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 38f),
                NSLayoutConstraint.Create(typeLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -2f),
                NSLayoutConstraint.Create(typeLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, horizontalSeparator, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
            });
        }

        public override void Reset()
        {
            AddressChangedAction = null;
        }

        public void BindContent(DocumentAddress ca, bool errorState = false)
        {
            SetErrorState(errorState, false);
            address = ca;

            addressTextField.Text = ca.Address ?? string.Empty;
            nameTextField.Text = ca.Name ?? string.Empty;
            attentionTextField.Text = ca.FullAttention ?? string.Empty;

            switch (address.AddressType)
            {
                case DocumentAddressType.To:
                    typeLabel.Text = Localization.GetString("to").ToUpper();
                    break;
                case DocumentAddressType.Cc:
                    typeLabel.Text = Localization.GetString("cc").ToUpper();
                    break;
                case DocumentAddressType.Bcc:
                    typeLabel.Text = Localization.GetString("bcc").ToUpper();
                    break;
            }
            typeLabel.SizeToFit();
        }

        #region EventHandlers

        void AddressTextField_EditingChanged(object sender, EventArgs e)
        {
            address.Address = addressTextField.Text;
            AddressChangedAction?.Invoke();
        }

        void AttentionTextField_EditingChanged(object sender, EventArgs e)
        {
            //No it is not error to get the attention value from the FullAttention property
            //and save it in the Attention one. It actually makes a difference for linked addresses
            address.Attention = attentionTextField.Text;
        }

        void NameTextField_EditingChanged(object sender, EventArgs e)
        {
            address.Name = nameTextField.Text;
        }

        #endregion
    }
}
