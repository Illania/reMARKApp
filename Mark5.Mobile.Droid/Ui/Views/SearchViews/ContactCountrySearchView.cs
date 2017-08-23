using System;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactCountrySearchView : AbstractDropdownSearchView<SearchContactsCriteria>
    {
        int prefix = -1;

        public ContactCountrySearchView(Android.Content.Context context, ISearchCriteriaFragment f)
            : base(context, Resource.String.search_contact_country, Resource.String.search_contact_country_none_selected, f)
        {
        }

        protected override async void ClickAction()
        {
            var pllf = new PickCountryFragment();
            ParentFragment.ReplaceFragment(pllf, pllf.GenerateTag());
            UpdateCountryCode(await pllf.Task);
        }

        void UpdateCountryCode(int p)
        {
            prefix = p;
            var index = ServerConfig.SystemSettings.ContactsModuleInfo.Countries.FindIndex(c => c.FaxPrefix == prefix);
            var text = index >= 0 ? ServerConfig.SystemSettings.ContactsModuleInfo.Countries[index].Name : string.Empty;

            UpdateBottomTextView(text);
            UpdateCriteria();
        }

        public override void Refresh()
        {
            UpdateCountryCode(Criteria.CountryPrefix);
        }

        public override void UpdateCriteria()
        {
            Criteria.CountryPrefix = prefix;
        }
    }
}