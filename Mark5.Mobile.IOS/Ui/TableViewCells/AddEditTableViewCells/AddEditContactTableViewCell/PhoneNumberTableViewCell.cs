using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCell
{
    public class PhoneNumberTableViewCell : MultiRowContentTableViewCell
    {
        public static readonly NSString Key = new NSString("PhoneNumberTableViewCell");

        protected CommunicationAddress address;

        readonly UITextField numberTextField;
        readonly UITextField descriptionTextField;
        readonly UIButton chevronButton;
        readonly UITextField prefixTextField;
        readonly UISwitch preferrableSwitch;
        readonly UILabel preferrableLabel;

        readonly UIToolbar countryPickerToolbar;
        readonly UIPickerView countryPicker;
        readonly Source countrySource;
        readonly NSLayoutConstraint prefixWidthConstraint;

        CountryInfo selectedCountry;

        public Action SelectedAsPrimaryAction;
        public Action AddressChangedAction;

        public PhoneNumberTableViewCell()
          : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            countrySource = new Source();
            countryPicker = new UIPickerView
            {
                DataSource = countrySource,
                Delegate = countrySource
            };

            countryPickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
            {
                Items = new[]
                {
                        new UIBarButtonItem(UIBarButtonSystemItem.Cancel, this, new Selector("cancelTapped:"))
                        {
                            TintColor = Theme.DarkerBlue
                        },
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("doneTapped:"))
                        {
                            TintColor = Theme.DarkerBlue
                        }
                }
            };

            prefixTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TintColor = Theme.Clear,
                Text = Localization.GetString("prefix"),
                InputView = countryPicker,
                InputAccessoryView = countryPickerToolbar,
            };
            prefixTextField.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(prefixTextField);
            prefixWidthConstraint = NSLayoutConstraint.Create(prefixTextField, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, 0.0f);

            chevronButton = GetChevron();
            chevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            chevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(chevronButton);

            var verticalSeparator = GetVerticalSeparator();
            ContentView.AddSubview(verticalSeparator);

            numberTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("number"),
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
            };
            numberTextField.EditingChanged += NumberTextField_EditingChanged;
            ContentView.Add(numberTextField);

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

            ContentView.AddConstraints(new[]
            {
                prefixTextField.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, VerticalMargin),
                prefixTextField.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                prefixTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),
                prefixWidthConstraint = prefixTextField.WidthAnchor.ConstraintGreaterThanOrEqualTo(0f),

                chevronButton.LeadingAnchor.ConstraintEqualTo(prefixTextField.TrailingAnchor),
                chevronButton.CenterYAnchor.ConstraintEqualTo(prefixTextField.CenterYAnchor),
                chevronButton.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                verticalSeparator.LeadingAnchor.ConstraintEqualTo(chevronButton.TrailingAnchor, InnerHorizontalMargin),
                verticalSeparator.TopAnchor.ConstraintEqualTo(prefixTextField.TopAnchor),
                verticalSeparator.BottomAnchor.ConstraintEqualTo(prefixTextField.BottomAnchor),

                numberTextField.TopAnchor.ConstraintEqualTo(prefixTextField.TopAnchor),
                numberTextField.LeadingAnchor.ConstraintEqualTo(verticalSeparator.TrailingAnchor, InnerHorizontalMargin),
                numberTextField.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                numberTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                horizontalSeparator.LeadingAnchor.ConstraintEqualTo(prefixTextField.LeadingAnchor),
                horizontalSeparator.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                horizontalSeparator.TopAnchor.ConstraintEqualTo(numberTextField.BottomAnchor, InnerVerticalMargin),

                descriptionTextField.TopAnchor.ConstraintEqualTo(horizontalSeparator.BottomAnchor, InnerVerticalMargin),
                descriptionTextField.LeadingAnchor.ConstraintEqualTo(prefixTextField.LeadingAnchor),
                descriptionTextField.TrailingAnchor.ConstraintEqualTo(numberTextField.TrailingAnchor),
                descriptionTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                horizontalSeparator2.LeadingAnchor.ConstraintEqualTo(prefixTextField.LeadingAnchor),
                horizontalSeparator2.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                horizontalSeparator2.TopAnchor.ConstraintEqualTo(descriptionTextField.BottomAnchor, InnerVerticalMargin),

                preferrableLabel.TopAnchor.ConstraintEqualTo(horizontalSeparator2.BottomAnchor, InnerVerticalMargin),
                preferrableLabel.LeadingAnchor.ConstraintEqualTo(prefixTextField.LeadingAnchor),
                preferrableLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -VerticalMargin),

                preferrableSwitch.TopAnchor.ConstraintEqualTo(horizontalSeparator2.BottomAnchor, InnerVerticalMargin),
                preferrableSwitch.LeadingAnchor.ConstraintEqualTo(preferrableLabel.TrailingAnchor, InnerHorizontalMargin),
                preferrableSwitch.TrailingAnchor.ConstraintEqualTo(numberTextField.TrailingAnchor),
                preferrableSwitch.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -VerticalMargin),

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

            if (ca.Address != null)
            {
                var parts = AddressFormatter.CommunicationAddressParts(ca);

                numberTextField.Text = parts.Number;
                selectedCountry = countrySource.CountryByPrefix(parts.CountryPrefix);
            }
            else
            {
                numberTextField.Text = string.Empty;
                selectedCountry = null;
            }

            UpdatePrefix(false);
        }

        #region Event handlers

        void PreferrableSwitch_ValueChanged(object sender, EventArgs e)
        {
            address.IsPrimary = preferrableSwitch.On;
            SelectedAsPrimaryAction?.Invoke();
        }

        void DescriptionTextField_EditingChanged(object sender, EventArgs e)
        {
            address.Description = descriptionTextField.Text;
        }

        void NumberTextField_EditingChanged(object sender, EventArgs e)
        {
            UpdateAddress();
        }

        void UpdatePrefix(bool updatedAddress = true)
        {
            if (selectedCountry != null)
                prefixTextField.Text = $"+{selectedCountry.FaxPrefix}";
            else
                prefixTextField.Text = Localization.GetString("prefix");

            prefixTextField.SizeToFit();
            var width = prefixTextField.IntrinsicContentSize.Width;
            prefixWidthConstraint.Constant = width + 5.0f;

            if (updatedAddress)
                UpdateAddress();
        }

        void UpdateAddress()
        {
            var prefixString = selectedCountry != null ? selectedCountry.FaxPrefix.ToString() : "0";
            address.Address = string.Join("|", prefixString, "", numberTextField.Text);
            AddressChangedAction?.Invoke();
        }

        #endregion

        #region Country Picker

        class Source : UIPickerViewDataSource, IUIPickerViewDelegate
        {
            readonly CountryInfo[] countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries.OrderBy(ci => ci.Name.Trim()).ToArray();

            public override nint GetComponentCount(UIPickerView pickerView)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
            {
                return countries.Length;
            }

            [Export("pickerView:titleForRow:forComponent:")]
            public string GetTitle(UIPickerView picker, nint row, nint component)
            {
                var ci = countries[row];

                return $"{ci.Name} (+{ci.FaxPrefix})";
            }

            public void SelectCountryByFaxPrefix(UIPickerView picker, int faxPrefix)
            {
                var index = 0;
                for (var i = 0; i < countries.Length; i++)
                    if (countries[i].FaxPrefix == faxPrefix)
                    {
                        index = i;
                        break;
                    }

                picker.Select(index, 0, true);
            }

            public CountryInfo SelectedCountry(UIPickerView picker)
            {
                var selectedIndex = picker.SelectedRowInComponent(0);
                return countries[selectedIndex];
            }

            public CountryInfo CountryByPrefix(int faxPrefix)
            {
                for (var i = 0; i < countries.Length; i++)
                    if (countries[i].FaxPrefix == faxPrefix)
                        return countries[i];

                return null;
            }
        }

        [Export("doneTapped:")]
        void DoneTapped(UIBarButtonItem sender)
        {
            selectedCountry = countrySource.SelectedCountry(countryPicker);
            UpdatePrefix();
            prefixTextField.ResignFirstResponder();
        }

        [Export("cancelTapped:")]
        void CancelTapped(UIBarButtonItem sender)
        {
            prefixTextField.ResignFirstResponder();
        }

        #endregion
    }
}
