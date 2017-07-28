using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class PhoneNumberTableViewCell : AddEditContactTableViewCell
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

        CountryInfo selectedCountry;

        public event EventHandler SelectedAsPrimary = delegate { };

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
                TintColor = UIColor.Clear,
                Text = Localization.GetString("prefix"),
                InputView = countryPicker,
                InputAccessoryView = countryPickerToolbar,
            };
            //TODO need to fix the pushed to the left problem
            prefixTextField.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(prefixTextField);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(prefixTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(prefixTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(prefixTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            chevronButton = new UIButton();
            chevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            chevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(chevronButton);
            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, prefixTextField, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, prefixTextField, NSLayoutAttribute.CenterY, 1f, 0f),
                NSLayoutConstraint.Create(chevronButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, InnerRowHeight),
            });

            var disclosureCell = new UITableViewCell()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Accessory = UITableViewCellAccessory.DisclosureIndicator,
                UserInteractionEnabled = false
            };
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
               NSLayoutConstraint.Create(verticalSeparator, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, prefixTextField, NSLayoutAttribute.Bottom, 1f, InnerVerticalMargin),
            });

            numberTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Placeholder = Localization.GetString("number"),
            };
            numberTextField.EditingDidEnd += NumberTextField_EditingDidEnd;
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
            descriptionTextField.EditingDidEnd += DescriptionTextField_EditingDidEnd;
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
                Text = Localization.GetString("preferrable"),
            };
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

        public void BindContent(CommunicationAddress ca)
        {
            address = ca;

            preferrableSwitch.SetState(ca.IsPrimary, true);
            descriptionTextField.Text = ca.Description ?? string.Empty;

            if (ca.Address != null)
            {
                var parts = AddressUtils.CommunicationAddressParts(ca);

                numberTextField.Text = parts.Number;
                prefixTextField.Text = $"+{parts.CountryPrefix}";
            }
        }

        #region Event handlers

        void PreferrableSwitch_ValueChanged(object sender, EventArgs e)
        {
            address.IsPrimary = preferrableSwitch.On;
            SelectedAsPrimary(this, EventArgs.Empty);
        }

        void DescriptionTextField_EditingDidEnd(object sender, EventArgs e)
        {
            address.Description = descriptionTextField.Text;
        }

        void NumberTextField_EditingDidEnd(object sender, EventArgs e)
        {
            UpdateAddress();
        }

        void UpdatePrefix()
        {
            if (selectedCountry != null)
                prefixTextField.Text = $"+{selectedCountry.FaxPrefix}";

            UpdateAddress();
        }

        void UpdateAddress()
        {
            var prefixString = selectedCountry != null ? selectedCountry.FaxPrefix.ToString() : "0";
            address.Address = string.Join("|", prefixString, "", numberTextField.Text);
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
