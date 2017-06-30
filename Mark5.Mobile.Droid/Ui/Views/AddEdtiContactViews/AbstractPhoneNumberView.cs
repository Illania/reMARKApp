using System;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class AbstractPhoneNumberView : AbstractMultipleRowsView<CommunicationAddress>
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
            return new PhoneNumberRow(Context, content, type);
        }

        protected class PhoneNumberRow : Row
        {
            readonly AppCompatEditText countryEditText;
            readonly AppCompatEditText phoneEditText;
            readonly AppCompatEditText descriptionEditText;

            CountryInfo selectedCountry;

            CommunicationAddressType type;

            public PhoneNumberRow(Context context, CommunicationAddress address, CommunicationAddressType type)
                : base(context, address)
            {
                this.type = type;

                var container = new LinearLayoutCompat(context)
                {
                    Orientation = Vertical,
                    LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
                };

                Layout.AddView(container, 0);


                var firstLine = new LinearLayoutCompat(context)
                {
                    Orientation = Horizontal,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                container.AddView(firstLine);


                countryEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                };
                countryEditText.SetHint(Resource.String.edit_contact_country);
                countryEditText.KeyListener = null;
                countryEditText.Focusable = false;
                countryEditText.Click += CountryEditText_Click;
                firstLine.AddView(countryEditText);

                phoneEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
                };
                phoneEditText.SetHint(GetResourceIdForType(type));
                firstLine.AddView(phoneEditText);

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
                var index = await Dialogs.ShowListDialog(Context, Resource.String.edit_contact_country, countries.Select(c => c.Name).ToArray(), true);
                if (index >= 0)
                {
                    selectedCountry = countries[index];
                    countryEditText.Text = selectedCountry.Name;
                }
            }

            void UpdateText()
            {
                if (Content != null)
                {
                    //TODO to complete
                    descriptionEditText.Text = Content.Description;
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
