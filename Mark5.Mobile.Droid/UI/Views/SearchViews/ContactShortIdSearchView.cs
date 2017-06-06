using System;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactShortIdSearchView : AbstractEditableTextSearchView<SearchContactsCriteria>
    {
        public ContactShortIdSearchView(Android.Content.Context context, LinearLayoutCompat containerLayout)
            : base(context, Resource.String.search_contact_shortid, containerLayout)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.ShortId);
        }

        public override void UpdateCriteria()
        {
            Criteria.ShortId = GetText();
        }
    }
}