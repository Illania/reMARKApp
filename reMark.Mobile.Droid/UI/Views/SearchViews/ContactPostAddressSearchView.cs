using System;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactPostAddressSearchView : AbstractEditableTextSearchView<SearchContactsCriteria>
    {
        public ContactPostAddressSearchView(Android.Content.Context context, LinearLayoutCompat containerLayout)
            : base(context, Resource.String.search_contact_post_address, containerLayout)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.PostAddress);
        }

        public override void UpdateCriteria()
        {
            Criteria.PostAddress = GetText();
        }
    }
}