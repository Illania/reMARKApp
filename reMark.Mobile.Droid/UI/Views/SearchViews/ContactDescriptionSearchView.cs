using System;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactDescriptionSearchView : AbstractEditableTextSearchView<SearchContactsCriteria>
    {
        public ContactDescriptionSearchView(Android.Content.Context context, LinearLayoutCompat containerLayout)
            : base(context, Resource.String.search_contact_description, containerLayout)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.Description);
        }

        public override void UpdateCriteria()
        {
            Criteria.Description = GetText();
        }
    }
}