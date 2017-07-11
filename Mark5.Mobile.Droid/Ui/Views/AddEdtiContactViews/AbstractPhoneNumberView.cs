using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class AbstractPhoneNumberView : AbstractMultipleRowsView<CommunicationAddress>
    {
        CommunicationAddressType type;
        List<CountryInfo> countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries;

        protected AbstractPhoneNumberView(Context context, CommunicationAddressType type)
            : base(context, GetResourceIdForType(type), false)
        {
            this.type = type;
        }

        public override void RefreshView()
        {
            Clear();

            var addresses = Contact.CommunicationAddresses.Where(a => a.Type == type);
            foreach (var address in addresses)
            {
                AddRow(address);
            }
        }

        protected override Row GetNewRow()
        {
            return new PhoneNumberRow(Context, this, type);
        }

        protected override void AddButton_Click(object sender, EventArgs e)
        {
            CreateDialog();
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            var row = sender as PhoneNumberRow;
            var ca = row.GetContent();
            Contact.CommunicationAddresses.Remove(ca);
            RemoveRow(row);

            OnContentChanged();
        }

        async void CreateDialog(PhoneNumberRow row = null)
        {
            CommunicationAddress ca = null;

            var container = new LinearLayoutCompat(Context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
            };

            var firstLine = new LinearLayoutCompat(Context)
            {
                Orientation = Horizontal,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            container.AddView(firstLine);

            var countrySpinner = new Spinner(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
            };
            countrySpinner.Adapter = new CountryAdapter(Context, countries);
            firstLine.AddView(countrySpinner);

            var phoneEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
            };
            phoneEditText.SetHint(GetResourceIdForType(type));
            phoneEditText.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            phoneEditText.TextChanged += (sender, e) =>
            {
                phoneEditText.Error = string.IsNullOrWhiteSpace(phoneEditText.Text) ? Context.GetString(Resource.String.edit_contact_empty_number) : null;
            };
            firstLine.AddView(phoneEditText);

            var descriptionEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            descriptionEditText.SetHint(Resource.String.edit_contact_description);
            descriptionEditText.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            container.AddView(descriptionEditText);

            var thirdLine = new LinearLayoutCompat(Context)
            {
                Orientation = Horizontal,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    TopMargin = DistanceVerySmall,
                }
            };
            container.AddView(thirdLine);

            var preferableTextView = new AppCompatTextView(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = DistanceVerySmall
                }
            };
            preferableTextView.SetText(Resource.String.edit_contact_mark_as_preferable);
            preferableTextView.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            thirdLine.AddView(preferableTextView);

            var preferableCheckBox = new AppCompatCheckBox(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            thirdLine.AddView(preferableCheckBox);

            if (row != null)
            {
                ca = row.GetContent();
                var parts = AddressUtils.CommunicationAddressParts(ca);
                if (parts.CountryPrefix >= 0)
                {
                    countrySpinner.SetSelection(countries.FindIndex(c => c.FaxPrefix == parts.CountryPrefix));
                }
                phoneEditText.Text = parts.Number;
                descriptionEditText.Text = ca.Description;
                preferableCheckBox.Checked = ca.IsPrimary;
            }

            var titleResource = GetResourceIdForDialogForType(type, row != null);
            if (await Dialogs.ShowCustomViewDialogAsync(Context, titleResource, container) == true)
            {
                ca = ca ?? new CommunicationAddress();
                ca.Type = type;
                ca.Address = string.Join("|", countries[countrySpinner.SelectedItemPosition].FaxPrefix.ToString(), "", phoneEditText.Text);
                ca.Description = descriptionEditText.Text;
                ca.IsPrimary = preferableCheckBox.Checked;

                if (ca.IsPrimary)
                {
                    DisablePrimaryOnOtherRows(row);
                }

                if (row == null)
                {
                    AddRow(ca);
                    Contact.CommunicationAddresses.Add(ca);
                }
                else
                {
                    row.UpdateRow();
                }

                OnContentChanged();
            }
        }

        void DisablePrimaryOnOtherRows(PhoneNumberRow primaryAddressRow)
        {
            foreach (var row in Rows)
            {
                if (row != primaryAddressRow)
                {
                    var emailRow = row as PhoneNumberRow;
                    emailRow.GetContent().IsPrimary = false;
                    emailRow.UpdateRow();
                }
            }
        }

        static int GetResourceIdForType(CommunicationAddressType type)
        {
            switch (type)
            {
                case CommunicationAddressType.Mobile:
                    return Resource.String.edit_contact_mobile;
                case CommunicationAddressType.Phone:
                    return Resource.String.edit_contact_phone;
                case CommunicationAddressType.Fax:
                    return Resource.String.edit_contact_fax;
                case CommunicationAddressType.Telex:
                    return Resource.String.edit_contact_telex;
                default:
                    throw new ArgumentException("This view does not support the input communication address type");
            }
        }

        static int GetResourceIdForDialogForType(CommunicationAddressType type, bool edit)
        {
            switch (type)
            {
                case CommunicationAddressType.Mobile:
                    return edit ? Resource.String.edit_contact_edit_mobile : Resource.String.edit_contact_add_mobile;
                case CommunicationAddressType.Phone:
                    return edit ? Resource.String.edit_contact_edit_phone : Resource.String.edit_contact_add_phone;
                case CommunicationAddressType.Fax:
                    return edit ? Resource.String.edit_contact_edit_fax : Resource.String.edit_contact_add_fax;
                case CommunicationAddressType.Telex:
                    return edit ? Resource.String.edit_contact_edit_telex : Resource.String.edit_contact_add_telex;
                default:
                    throw new ArgumentException("This view does not support the input communication address type");
            }
        }

        class CountryAdapter : ArrayAdapter
        {
            readonly List<CountryInfo> countries;
            Context context;

            public CountryAdapter(Context context, List<CountryInfo> countries)
                : base(context, Android.Resource.Layout.SimpleSpinnerItem)
            {
                this.context = context;
                this.countries = countries;
            }

            public override int Count => countries.Count;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var view = convertView ?? new AppCompatTextView(Context);

                (view as AppCompatTextView).Text = $"+{countries[position].FaxPrefix}";
                return view;
            }

            public override View GetDropDownView(int position, View convertView, ViewGroup parent)
            {
                var view = convertView ?? LayoutInflater.From(parent.Context).Inflate(Resource.Layout.countries_dropdown, parent, false);
                var countryTextView = view.FindViewById<AppCompatTextView>(Resource.Id.countryText);
                countryTextView.Text = $"{countries[position].Name} (+{countries[position].FaxPrefix})";
                return view;
            }
        }

        public override bool ContainsValidContent()
        {
            return Rows.All(r => r.ContainsValidContent());
        }

        protected class PhoneNumberRow : Row
        {
            readonly AppCompatEditText phoneEditText;

            CommunicationAddressType type;

            public PhoneNumberRow(Context context, AbstractPhoneNumberView phoneNumberView, CommunicationAddressType type)
                : base(context, phoneNumberView)
            {
                this.type = type;

                phoneEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
                    KeyListener = null,
                    Focusable = false,
                };
                phoneEditText.Click += PhoneEditText_Click;
                phoneEditText.SetHint(GetResourceIdForType(type));
                Layout.AddView(phoneEditText, 0);
            }

            void PhoneEditText_Click(object sender, EventArgs e)
            {
                ((AbstractPhoneNumberView)ParentView).CreateDialog(this);
            }

            public override void UpdateRow()
            {
                if (Content != null)
                {
                    var parts = AddressUtils.CommunicationAddressParts(Content);
                    var country = parts.CountryPrefix > 0 ? $"+{parts.CountryPrefix}" : string.Empty;
                    phoneEditText.Text = string.Join(" ", country, parts.Number);
                    phoneEditText.SetTextAppearanceCompat(Context, Content.IsPrimary ? Resource.Style.fontPrimaryBold : Resource.Style.fontPrimary);
                    phoneEditText.Error = string.IsNullOrWhiteSpace(parts.Number) ? Context.GetString(Resource.String.edit_contact_empty_number) : null;
                }
            }

            public override bool ContainsValidContent() => true;
        }
    }
}
