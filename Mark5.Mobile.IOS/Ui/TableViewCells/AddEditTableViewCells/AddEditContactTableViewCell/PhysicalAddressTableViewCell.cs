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
            ContentView.Add(addressTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(addressTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            var horizontalSeparator = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator, NSLayoutAttribute.Top, NSLayoutRelation.Equal, addressTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
            });

            zipTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("zip") + " ",
            };
            zipTextField.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            zipTextField.EditingDidEnd += ZipTextField_EditingDidEnd;
            ContentView.Add(zipTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(zipTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 95f),
                NSLayoutConstraint.Create(zipTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(zipTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(zipTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            var verticalSeparator = GetVerticalSeparator();
            ContentView.AddSubview(verticalSeparator);
            ContentView.AddConstraints(new[]
            {
               NSLayoutConstraint.Create(verticalSeparator, NSLayoutAttribute.Left, NSLayoutRelation.Equal, zipTextField, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(verticalSeparator, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
               NSLayoutConstraint.Create(verticalSeparator, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, zipTextField, NSLayoutAttribute.Bottom, 1f, 0f),
            });

            areaTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("area"),
                AutocorrectionType = UITextAutocorrectionType.No,
            };
            areaTextField.EditingDidEnd += AreaTextField_EditingDidEnd;
            ContentView.Add(areaTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(areaTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(areaTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, verticalSeparator, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(areaTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(areaTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            var horizontalSeparator2 = GetHorizontalSeparator();
            ContentView.AddSubview(horizontalSeparator2);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(horizontalSeparator2, NSLayoutAttribute.Top, NSLayoutRelation.Equal, areaTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
            });

            countryTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TintColor = UIColor.Clear,
                Text = Localization.GetString("country"),
                InputView = countryPicker,
                InputAccessoryView = countryPickerToolbar,
            };
            countryTextField.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(countryTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(countryTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator2, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(countryTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(countryTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
                NSLayoutConstraint.Create(countryTextField, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, 65.0f),
            });

            chevronButton = GetChevron();
            chevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            chevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(chevronButton);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, countryTextField, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, countryTextField, NSLayoutAttribute.CenterY, 1f, 0f),
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            var verticalSeparator2 = GetVerticalSeparator();
            ContentView.AddSubview(verticalSeparator2);
            ContentView.AddConstraints(new[]
            {
               NSLayoutConstraint.Create(verticalSeparator2, NSLayoutAttribute.Left, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(verticalSeparator2, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator2, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
               NSLayoutConstraint.Create(verticalSeparator2, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, countryTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
            });

            cityTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("city"),
                AutocorrectionType = UITextAutocorrectionType.No,
            };
            cityTextField.EditingDidEnd += CityTextField_EditingDidEnd;
            ContentView.Add(cityTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(cityTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, horizontalSeparator2, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
                NSLayoutConstraint.Create(cityTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, verticalSeparator2, NSLayoutAttribute.Right, 1f, InnerHorizontalMargin),
                NSLayoutConstraint.Create(cityTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(cityTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
                NSLayoutConstraint.Create(cityTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
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
