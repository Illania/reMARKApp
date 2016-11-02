//
// Project: Mark5.Mobile.Droid
// File: ContactFirstNameSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactFirstNameSearchView : AbstractEditTextSearchView<SearchContactsCriteria>
    {

        public ContactFirstNameSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_contact_first_name);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            EditText.Text = criteria.FirstName;
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.FirstName = EditText.Text;
        }
    }
}
