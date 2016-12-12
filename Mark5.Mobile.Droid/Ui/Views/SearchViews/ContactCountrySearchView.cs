//
// Project: Mark5.Mobile.Droid
// File: ContactCountrySearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    
    public class ContactCountrySearchView : AbstractSingleChoiceSearchView<SearchContactsCriteria, CountryInfo>
    {

        public ContactCountrySearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_contact_country);

            var countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries.OrderBy(ci => ci.Name).ToList();
            countries.Insert(0, new CountryInfo { Id = -1, Name = context.GetString(Resource.String.search_contact_country_none_selected), FaxPrefix = -1 });

            DialogTitle = Resource.String.search_contact_country;
            Values = countries;
            SelectedValue = countries[0];
            DisplayText = ci =>
            {
                return ci.Name;
            };
            EqualityComparer = LambdaEqualityComparer<CountryInfo>.Create(ci => ci.Id);

            UpdateSubtitle();
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            SelectedValue = ServerConfig.SystemSettings.ContactsModuleInfo.Countries.FirstOrDefault(ci => ci.FaxPrefix == criteria.CountryPrefix);
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.CountryPrefix = SelectedValue.FaxPrefix;
        }
    }
}
