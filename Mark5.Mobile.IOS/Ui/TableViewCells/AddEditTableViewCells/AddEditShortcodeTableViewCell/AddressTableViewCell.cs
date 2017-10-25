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

            var horizontalSeparator = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator);

            nameTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("name"),
            };
            nameTextField.EditingChanged += NameTextField_EditingChanged;
            ContentView.Add(nameTextField);

            var horizontalSeparator2 = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator2);

            attentionTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("attention"),
            };
            attentionTextField.EditingChanged += AttentionTextField_EditingChanged; ;
            ContentView.Add(attentionTextField);

            typeLabel = new UILabel
            {
                Font = Theme.DefaultFont.WithSize(11f),
                TextColor = Theme.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            typeLabel.Layer.BackgroundColor = Theme.DarkGray.CGColor;
            typeLabel.Layer.CornerRadius = 3f;
            typeLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            typeLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            ContentView.Add(typeLabel);

            ContentView.AddConstraints(new[]
            {
                addressTextField.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, VerticalMargin),
                addressTextField.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                addressTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                horizontalSeparator.TopAnchor.ConstraintEqualTo(addressTextField.BottomAnchor, InnerVerticalMargin),
                horizontalSeparator.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                horizontalSeparator.WidthAnchor.ConstraintEqualTo(addressTextField.WidthAnchor),

                nameTextField.TopAnchor.ConstraintEqualTo(horizontalSeparator.BottomAnchor, InnerVerticalMargin),
                nameTextField.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                nameTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),
                nameTextField.WidthAnchor.ConstraintEqualTo(addressTextField.WidthAnchor),

                horizontalSeparator2.TopAnchor.ConstraintEqualTo(nameTextField.BottomAnchor, InnerVerticalMargin),
                horizontalSeparator2.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                horizontalSeparator2.WidthAnchor.ConstraintEqualTo(addressTextField.WidthAnchor),

                attentionTextField.TopAnchor.ConstraintEqualTo(horizontalSeparator2.BottomAnchor, InnerVerticalMargin),
                attentionTextField.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                attentionTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),
                attentionTextField.WidthAnchor.ConstraintEqualTo(addressTextField.WidthAnchor),
                attentionTextField.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor,  -VerticalMargin),

                typeLabel.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                typeLabel.WidthAnchor.ConstraintEqualTo(27f),
                typeLabel.LeadingAnchor.ConstraintEqualTo(addressTextField.TrailingAnchor, InnerHorizontalMargin),
                typeLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),

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
