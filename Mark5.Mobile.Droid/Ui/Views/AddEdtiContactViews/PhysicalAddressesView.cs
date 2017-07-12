using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class PhysicalAddressesView : AbstractMultipleRowsView<PhysicalAddress>
    {
        List<CountryInfo> countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries;

        public PhysicalAddressesView(Context context)
            : base(context, Resource.String.edit_contact_physical)
        {
        }

        public override void RefreshView()
        {
            Clear();

            foreach (var address in Contact.PhysicalAddresses)
                AddRow(address);
        }

        protected override void AddButton_Click(object sender, EventArgs e)
        {
            CreateDialog();
        }

        protected override Row GetNewRow()
        {
            return new PhysicalAddressRow(Context, this);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            var row = (PhysicalAddressRow)sender;
            var pa = row.GetContent();
            Contact.PhysicalAddresses.Remove(pa);
            RemoveRow(row);
        }

        async void CreateDialog(PhysicalAddressRow row = null)
        {
            PhysicalAddress pa = null;

            var container = new LinearLayoutCompat(Context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            var streetEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            streetEditText.SetHint(Resource.String.edit_contact_physical_address);
            container.AddView(streetEditText);

            var secondLine = new LinearLayoutCompat(Context)
            {
                Orientation = Horizontal,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            container.AddView(secondLine);

            var zipEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            zipEditText.SetHint(Resource.String.edit_contact_zip);
            secondLine.AddView(zipEditText);

            var areaEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f)
            };
            areaEditText.SetHint(Resource.String.edit_contact_area);
            secondLine.AddView(areaEditText);

            var cityEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            cityEditText.SetHint(Resource.String.edit_contact_city);
            container.AddView(cityEditText);

            var countrySpinner = new Spinner(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    TopMargin = DistanceSmall,
                    LeftMargin = DistanceSmall,
                }
            };
            countrySpinner.Adapter = new CountryAdapter(Context, countries);
            container.AddView(countrySpinner);

            if (row != null)
            {
                pa = row.GetContent();
                streetEditText.Text = pa.Street;
                zipEditText.Text = pa.ZipCode;
                areaEditText.Text = pa.Area;
                cityEditText.Text = pa.City;
                countrySpinner.SetSelection(countries.FindIndex(c => c.FaxPrefix == pa.Country.FaxPrefix));
            }

            if (await Dialogs.ShowCustomViewDialogAsync(Context, Resource.String.edit_contact_physical, container) == true)
            {
                pa = pa ?? new PhysicalAddress();
                pa.Street = streetEditText.Text;
                pa.ZipCode = zipEditText.Text;
                pa.Area = areaEditText.Text;
                pa.City = cityEditText.Text;
                pa.Country = countries[countrySpinner.SelectedItemPosition];

                if (row == null)
                {
                    AddRow(pa);
                    Contact.PhysicalAddresses.Add(pa);
                }
                else
                {
                    row.UpdateRow();
                }
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

                (view as AppCompatTextView).Text = $"{countries[position].Name}";
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

        protected class PhysicalAddressRow : Row
        {
            readonly AppCompatEditText addressEditText;

            public PhysicalAddressRow(Context context, PhysicalAddressesView physicalAddressesView)
                : base(context, physicalAddressesView)
            {
                addressEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
                    KeyListener = null,
                    Focusable = false,
                };
                addressEditText.Click += PhoneEditText_Click;
                Layout.AddView(addressEditText, 0);
            }

            void PhoneEditText_Click(object sender, EventArgs e)
            {
                ((PhysicalAddressesView)ParentView).CreateDialog(this);
            }

            public override void UpdateRow()
            {
                addressEditText.Text = GetAddressText(Content);
            }

            string GetAddressText(PhysicalAddress address)
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(address.Street))
                    sb.Append(address.Street);
                if (!string.IsNullOrWhiteSpace(address.Area))
                    sb.AppendLine().Append(address.Area);
                if (!string.IsNullOrWhiteSpace(address.ZipCode))
                    sb.AppendLine().Append(address.ZipCode);
                if (!string.IsNullOrWhiteSpace(address.City))
                    sb.Append(" ").Append(address.City);
                if (address.Country != null && address.Country.Id != 0)
                    sb.AppendLine().Append(address.Country.Name);
                return sb.ToString();
            }

            public override bool ContainsValidContent() => true;
        }

    }
}
