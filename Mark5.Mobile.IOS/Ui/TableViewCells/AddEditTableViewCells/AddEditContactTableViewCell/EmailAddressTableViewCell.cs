using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCell
{
    public class EmailAddressTableViewCell : MultiRowContentTableViewCell
    {
        public static readonly NSString Key = new NSString("EmailAddressTableViewCell");

        protected CommunicationAddress address;

        readonly UITextField addressTextField;
        readonly UITextField descriptionTextField;
        readonly UISwitch preferrableSwitch;
        readonly UILabel preferrableLabel;

        public Action SelectedAsPrimaryAction;
        public Action AddressChangedAction;

        public EmailAddressTableViewCell() : base(UITableViewCellStyle.Default, Key)
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

            descriptionTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("description"),
            };
            descriptionTextField.EditingChanged += DescriptionTextField_EditingChanged;
            ContentView.Add(descriptionTextField);

            var horizontalSeparator2 = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator2);

            preferrableLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Text = Localization.GetString("preferrable"),
            };
            ContentView.Add(preferrableLabel);

            preferrableSwitch = new UISwitch
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            preferrableSwitch.SetContentHuggingPriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(preferrableSwitch);

            preferrableSwitch.ValueChanged += PreferrableSwitch_ValueChanged;

            //ContentView.AddConstraints(new[]
            //{
            //    NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
            //    NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
            //    NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
            //    NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),

            //    NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
            //    NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
            //    NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Top, NSLayoutRelation.Equal, addressTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),

            //    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
            //    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
            //    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
            //    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),

            //    NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
            //    NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
            //    NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Top, NSLayoutRelation.Equal, descriptionTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),

            //    NSLayoutConstraint.Create(preferrableLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator2, NSLayoutAttribute.TopMargin, 1f, InnerVerticalMargin),
            //    NSLayoutConstraint.Create(preferrableLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
            //    NSLayoutConstraint.Create(preferrableLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),

            //    NSLayoutConstraint.Create(preferrableSwitch, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator2, NSLayoutAttribute.TopMargin, 1f, InnerVerticalMargin),
            //    NSLayoutConstraint.Create(preferrableSwitch, NSLayoutAttribute.Left, NSLayoutRelation.Equal, preferrableLabel, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
            //    NSLayoutConstraint.Create(preferrableSwitch, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
            //    NSLayoutConstraint.Create(preferrableSwitch, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
            //});

            ContentView.AddConstraints(new[]
            {
                addressTextField.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                addressTextField.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                addressTextField.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                addressTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                horizontalSeparator.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                horizontalSeparator.TrailingAnchor.ConstraintEqualTo(addressTextField.TrailingAnchor),
                horizontalSeparator.TopAnchor.ConstraintEqualTo(addressTextField.BottomAnchor, InnerVerticalMargin),

                descriptionTextField.TopAnchor.ConstraintEqualTo(horizontalSeparator.BottomAnchor, InnerVerticalMargin),
                descriptionTextField.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                descriptionTextField.TrailingAnchor.ConstraintEqualTo(addressTextField.TrailingAnchor),
                descriptionTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                horizontalSeparator2.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                horizontalSeparator2.TrailingAnchor.ConstraintEqualTo(addressTextField.TrailingAnchor),
                horizontalSeparator2.TopAnchor.ConstraintEqualTo(descriptionTextField.BottomAnchor, InnerVerticalMargin),

                preferrableLabel.TopAnchor.ConstraintEqualTo(horizontalSeparator2.BottomAnchor, InnerVerticalMargin),
                preferrableLabel.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                preferrableLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor),

                preferrableSwitch.TopAnchor.ConstraintEqualTo(horizontalSeparator2.BottomAnchor, InnerVerticalMargin),
                preferrableSwitch.LeadingAnchor.ConstraintEqualTo(preferrableLabel.TrailingAnchor, InnerHorizontalMargin),
                preferrableSwitch.TrailingAnchor.ConstraintEqualTo(addressTextField.TrailingAnchor),
                preferrableSwitch.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor),
            });

        }

        public override void Reset()
        {
            SelectedAsPrimaryAction = null;
            AddressChangedAction = null;
        }

        public void BindContent(CommunicationAddress ca, bool errorState = false)
        {
            SetErrorState(errorState, false);
            address = ca;

            preferrableSwitch.SetState(ca.IsPrimary, true);
            descriptionTextField.Text = ca.Description ?? string.Empty;
            addressTextField.Text = ca.Address ?? string.Empty;
        }

        #region EventHandlers

        void PreferrableSwitch_ValueChanged(object sender, EventArgs e)
        {
            address.IsPrimary = preferrableSwitch.On;
            SelectedAsPrimaryAction?.Invoke();
        }

        void AddressTextField_EditingChanged(object sender, EventArgs e)
        {
            address.Address = addressTextField.Text;
            AddressChangedAction?.Invoke();
        }

        void DescriptionTextField_EditingChanged(object sender, EventArgs e)
        {
            address.Description = descriptionTextField.Text;
        }

        #endregion

    }
}
