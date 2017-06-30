using System;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class AbstractPhoneNumberView : MultipleRowsView<CommunicationAddress>
    {
        CommunicationAddressType type;

        protected AbstractPhoneNumberView(Context context, CommunicationAddressType type)
            : base(context, GetResourceIdForType(type), false)
        {
            this.type = type;
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

        public override void RefreshView()
        {
            var addresses = Contact.CommunicationAddresses.Where(a => a.Type == type);
            foreach (var address in addresses)
            {
                AddRow(address);
            }
        }

        public override void UpdateContact()
        {
            throw new NotImplementedException();
        }

        protected override Row GetNewRow(CommunicationAddress content)
        {
            return new PhoneNumberRow(Context, content);
        }

        protected class PhoneNumberRow : Row
        {
            readonly AppCompatEditText countryEditText;
            readonly AppCompatEditText phoneEditText;
            readonly AppCompatEditText descriptionEditText;

            Context context;

            CountryInfo selectedCountry;

            CommunicationAddress address;

            public PhoneNumberRow(Context context, CommunicationAddress address)
                : base(context, address)
            {
                this.context = context;
                this.address = address;

                var container = new LinearLayoutCompat(context)
                {
                    Orientation = Vertical,
                    LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
                };

                Layout.AddView(container, 0);

                countryEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                };
                countryEditText.SetHint(Resource.String.edit_contact_country);
                countryEditText.KeyListener = null;
                countryEditText.Focusable = false;
                countryEditText.Click += CountryEditText_Click;
                container.AddView(countryEditText);

                phoneEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                phoneEditText.SetHint(Resource.String.edit_contact_phone);
                container.AddView(phoneEditText);

                descriptionEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                descriptionEditText.SetHint(Resource.String.edit_contact_description);
                container.AddView(descriptionEditText);

                UpdateText();
            }

            async void CountryEditText_Click(object sender, EventArgs e)
            {
                var countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries;
                var index = await Dialogs.ShowListDialog(context, Resource.String.edit_contact_country, countries.Select(c => c.Name).ToArray(), true);
                if (index >= 0)
                {
                    selectedCountry = countries[index];
                    countryEditText.Text = selectedCountry.Name;
                }
            }

            void UpdateText()
            {
                if (address != null)
                {
                    //TODO to complete
                    descriptionEditText.Text = address.Description;
                }
            }

            public override CommunicationAddress GetContent()
            {
                return null; //TODO correct
            }

            public override bool ContainsValidContent()
            {
                throw new NotImplementedException();
            }
        }
    }
}
