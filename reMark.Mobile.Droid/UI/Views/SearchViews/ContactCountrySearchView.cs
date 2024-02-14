using System;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Ui.Fragments;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
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
            var (pllf, tag) = PickCountryFragment.NewInstance();
            ParentFragment.ReplaceFragment(pllf, tag);
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