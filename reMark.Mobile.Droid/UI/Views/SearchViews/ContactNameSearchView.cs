using System;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactNameSearchView : AbstractEditableLargeSearchView<SearchContactsCriteria>
    {
        public ContactNameSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_contact_name, Resource.String.search_contact_name_hint)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.Name);
        }

        public override void UpdateCriteria()
        {
            Criteria.Name = GetText();
        }
    }
}