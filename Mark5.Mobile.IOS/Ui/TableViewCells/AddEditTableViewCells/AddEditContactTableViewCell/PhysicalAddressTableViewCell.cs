using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCell
{
    public class PhysicalAddressTableViewCell : AddEditTableViewCell
    {
        public static readonly NSString Key = new NSString("PhysicalAddressTableViewCell");

        protected PhysicalAddress address;

        readonly UITextField cityTextField;
        readonly UITextField addressTextField;
        readonly UITextField zipTextField;
        readonly UITextField areaTextField;
        readonly UIButton chevronButton;
        readonly UITextField countryTextField;

        readonly UIToolbar countryPickerToolbar;
        readonly UIPickerView countryPicker;
        readonly Source countrySource;

        public PhysicalAddressTableViewCell()
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

            addressTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("address"),
                AutocorrectionType = UITextAutocorrectionType.No,
            };
            addressTextField.EditingDidEnd += AddressTextField_EditingDidEnd;
            ContentView.AddSubview(addressTextField);

            var horizontalSeparator = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator);

            zipTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("zip") + " ",
            };
            zipTextField.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            zipTextField.EditingDidEnd += ZipTextField_EditingDidEnd;
            ContentView.AddSubview(zipTextField);

            var verticalSeparator = GetVerticalSeparator();
            ContentView.AddSubview(verticalSeparator);

            areaTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("area"),
                AutocorrectionType = UITextAutocorrectionType.No,
            };
            areaTextField.EditingDidEnd += AreaTextField_EditingDidEnd;
            ContentView.AddSubview(areaTextField);

            var horizontalSeparator2 = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator2);

            countryTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TintColor = Theme.Clear,
                Text = Localization.GetString("country"),
                InputView = countryPicker,
                InputAccessoryView = countryPickerToolbar,
            };
            countryTextField.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(countryTextField);

            chevronButton = GetChevron();
            chevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            chevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(chevronButton);

            var verticalSeparator2 = GetVerticalSeparator();
            ContentView.AddSubview(verticalSeparator2);

            cityTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("city"),
                AutocorrectionType = UITextAutocorrectionType.No,
            };
            cityTextField.EditingDidEnd += CityTextField_EditingDidEnd;
            ContentView.AddSubview(cityTextField);

            ContentView.AddConstraints(new[]
            {
                addressTextField.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, VerticalMargin),
                addressTextField.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                addressTextField.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                addressTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                horizontalSeparator.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                horizontalSeparator.TrailingAnchor.ConstraintEqualTo(addressTextField.TrailingAnchor),
                horizontalSeparator.TopAnchor.ConstraintEqualTo(addressTextField.BottomAnchor, InnerVerticalMargin),

                zipTextField.WidthAnchor.ConstraintEqualTo(95f),
                zipTextField.TopAnchor.ConstraintEqualTo(horizontalSeparator.BottomAnchor, InnerVerticalMargin),
                zipTextField.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                zipTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                verticalSeparator.LeadingAnchor.ConstraintEqualTo(zipTextField.TrailingAnchor, InnerHorizontalMargin),
                verticalSeparator.TopAnchor.ConstraintEqualTo(horizontalSeparator.BottomAnchor, InnerVerticalMargin),
                verticalSeparator.BottomAnchor.ConstraintEqualTo(zipTextField.BottomAnchor),

                areaTextField.TopAnchor.ConstraintEqualTo(horizontalSeparator.BottomAnchor, InnerVerticalMargin),
                areaTextField.LeadingAnchor.ConstraintEqualTo(verticalSeparator.TrailingAnchor, InnerHorizontalMargin),
                areaTextField.TrailingAnchor.ConstraintEqualTo(addressTextField.TrailingAnchor),
                areaTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                horizontalSeparator2.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                horizontalSeparator2.TrailingAnchor.ConstraintEqualTo(addressTextField.TrailingAnchor),
                horizontalSeparator2.TopAnchor.ConstraintEqualTo(areaTextField.BottomAnchor, InnerVerticalMargin),

                countryTextField.TopAnchor.ConstraintEqualTo(horizontalSeparator2.BottomAnchor, InnerVerticalMargin),
                countryTextField.LeadingAnchor.ConstraintEqualTo(addressTextField.LeadingAnchor),
                countryTextField.WidthAnchor.ConstraintEqualTo(65f),
                countryTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                chevronButton.LeadingAnchor.ConstraintEqualTo(countryTextField.TrailingAnchor, InnerHorizontalMargin),
                chevronButton.CenterYAnchor.ConstraintEqualTo(countryTextField.CenterYAnchor),
                chevronButton.HeightAnchor.ConstraintEqualTo(InnerRowHeight),

                verticalSeparator2.LeadingAnchor.ConstraintEqualTo(chevronButton.TrailingAnchor, InnerHorizontalMargin),
                verticalSeparator2.TopAnchor.ConstraintEqualTo(horizontalSeparator2.BottomAnchor, InnerVerticalMargin),
                verticalSeparator2.BottomAnchor.ConstraintEqualTo(countryTextField.BottomAnchor, InnerVerticalMargin),

                cityTextField.TopAnchor.ConstraintEqualTo(horizontalSeparator2.BottomAnchor, InnerVerticalMargin),
                cityTextField.LeadingAnchor.ConstraintEqualTo(verticalSeparator2.TrailingAnchor, InnerHorizontalMargin),
                cityTextField.TrailingAnchor.ConstraintEqualTo(addressTextField.TrailingAnchor),
                cityTextField.HeightAnchor.ConstraintEqualTo(InnerRowHeight),
                cityTextField.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -VerticalMargin),
            });

        }

        public void BindContent(PhysicalAddress pa, bool errorState = false)
        {
            SetErrorState(errorState, false);

            address = pa;

            addressTextField.Text = pa.Street ?? string.Empty;
            zipTextField.Text = pa.ZipCode ?? string.Empty;
            areaTextField.Text = pa.Area ?? string.Empty;
            cityTextField.Text = pa.City ?? string.Empty;

            address.Country = address.Country ?? countrySource.CountryByPrefix(0);

            countrySource.SelectCountryByFaxPrefix(countryPicker, address.Country.FaxPrefix);

            UpdatePrefix();
        }

        #region Event handlers

        void ZipTextField_EditingDidEnd(object sender, EventArgs e)
        {
            address.ZipCode = zipTextField.Text;
        }

        void AddressTextField_EditingDidEnd(object sender, EventArgs e)
        {
            address.Street = addressTextField.Text;
        }

        void AreaTextField_EditingDidEnd(object sender, EventArgs e)
        {
            address.Area = areaTextField.Text;
        }

        void CityTextField_EditingDidEnd(object sender, EventArgs e)
        {
            address.City = cityTextField.Text;
        }

        void UpdatePrefix()
        {
            countryTextField.Text = address.Country.Name.StartsWith("[", StringComparison.Ordinal) ? Localization.GetString("country") : address.Country.CCode;
        }

        #endregion

        #region Country Picker

        class Source : UIPickerViewDataSource, IUIPickerViewDelegate
        {
            readonly CountryInfo[] countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries.OrderBy(ci => ci.Name.TrimStart()).ToArray();

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
                var title = ci.Name;
                if (!string.IsNullOrEmpty(ci.CCode3))
                {
                    title += $" ({ci.CCode})";
                }
                return title;
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
            address.Country = countrySource.SelectedCountry(countryPicker);
            UpdatePrefix();
            countryTextField.ResignFirstResponder();
        }

        [Export("cancelTapped:")]
        void CancelTapped(UIBarButtonItem sender)
        {
            countryTextField.ResignFirstResponder();
        }

        #endregion
    }
}
