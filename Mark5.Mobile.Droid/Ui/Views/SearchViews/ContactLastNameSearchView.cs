//
// Project: Mark5.Mobile.Droid
// File: ContactLastNameSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactLastNameSearchView : AbstractEditTextSearchView<SearchContactsCriteria>
    {

        public ContactLastNameSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_contact_last_name);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            EditText.Text = criteria.LastName;
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.LastName = EditText.Text;
        }
    }
}
