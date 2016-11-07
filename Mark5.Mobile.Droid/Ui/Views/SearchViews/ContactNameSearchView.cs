//
// Project: Mark5.Mobile.Droid
// File: ContactNameSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactNameSearchView : AbstractEditTextSearchView<SearchContactsCriteria>
    {

        public ContactNameSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_contact_name);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            EditText.Text = criteria.Name;
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.Name = EditText.Text;
        }
    }
}
