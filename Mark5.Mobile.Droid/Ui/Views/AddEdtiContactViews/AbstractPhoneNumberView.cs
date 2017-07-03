using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
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

        protected override Row GetNewRow()
        {
            return new PhoneNumberRow(Context, this, type);
        }


        protected override void AddButton_Click(object sender, EventArgs e)
        {
            CreateDialog();
        }

        async void CreateDialog(CommunicationAddress ca = null, PhoneNumberRow row = null)
        {
            var container = new LinearLayoutCompat(Context)
            {
                Orientation = Horizontal,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
            };

            var firstLine = new LinearLayoutCompat(Context)
            {
                Orientation = Horizontal,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            container.AddView(firstLine);

            var countryEditText = new Spinner(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
            };
            countryEditText.Adapter = new CountryAdapter(Context); // TODO SELECT NONE
            firstLine.AddView(countryEditText);

            var phoneEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
            };
            phoneEditText.SetHint(GetResourceIdForType(type));
            firstLine.AddView(phoneEditText);

            var descriptionEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            descriptionEditText.SetHint(Resource.String.edit_contact_description);
            container.AddView(descriptionEditText);

            if (ca != null)
            {
                var parts = AddressUtils.CommunicationAddressParts(ca);
                //countryEditText.Text = $"+{parts.Item1}";
                phoneEditText.Text = parts.Item2;
            }

            if (await Dialogs.ShowCustomViewDialogAsync(Context, Resource.String.edit_contact_email, container) == true)
            {
                ca = ca ?? new CommunicationAddress(); //TODO check if correct
                //ca.Address = string.Join("|", countryEditText.Text.Skip(1), phoneEditText);
                ca.Description = descriptionEditText.Text;

                if (row == null)
                {
                    AddRow(ca);
                }
                else
                {
                    row.SetContent(ca);
                }
            }
        }

        class CountryAdapter : ArrayAdapter
        {
            List<CountryInfo> countries;
            Context context;

            public CountryAdapter(Context context)
                : base(context, Android.Resource.Layout.SimpleSpinnerItem)
            {
                this.context = context;
                countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries;
            }

            public override int Count => countries.Count;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var view = convertView ?? new AppCompatTextView(Context);

                (view as AppCompatTextView).Text = $"+{countries[position].FaxPrefix}";
                return convertView;
            }

            public override View GetDropDownView(int position, View convertView, ViewGroup parent)
            {

                var view = convertView ?? new AppCompatTextView(Context);

                (view as AppCompatTextView).Text = $"+{countries[position].FaxPrefix}";
                return convertView;

                //var view = convertView ?? LayoutInflater.From(parent.Context).Inflate(Resource.Layout.countries_dropdown, parent, false);
                //var countryTextView = view.FindViewById<AppCompatTextView>(Resource.Id.countryText);
                //countryTextView.Text = $"{countries[position].Name} (+{countries[position].FaxPrefix})";
                //return view;
            }
        }

        protected class PhoneNumberRow : Row
        {
            readonly AppCompatEditText phoneEditText;

            CountryInfo selectedCountry;

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
                phoneEditText.SetHint(GetResourceIdForType(type));
                Layout.AddView(phoneEditText, 0);
            }

            //async void CountryEditText_Click(object sender, EventArgs e)
            //{
            //    var countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries;
            //    var index = await Dialogs.ShowListDialog(Context, Resource.String.edit_contact_country, countries.Select(c => c.Name).ToArray(), true);
            //    if (index >= 0)
            //    {
            //        selectedCountry = countries[index];
            //        countryEditText.Text = selectedCountry.Name;
            //    }
            //}

            protected override void UpdateRow()
            {
                if (Content != null)
                {
                    //TODO to complete
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
