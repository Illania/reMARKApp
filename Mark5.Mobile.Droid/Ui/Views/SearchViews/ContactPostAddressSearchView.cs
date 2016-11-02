//
// Project: Mark5.Mobile.Droid
// File: ContactPostAddressSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactPostAddressSearchView : AbstractEditTextSearchView<SearchContactsCriteria>
    {

        public ContactPostAddressSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_contact_post_address);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            EditText.Text = criteria.PostAddress;
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.PostAddress = EditText.Text;
        }
    }
}
