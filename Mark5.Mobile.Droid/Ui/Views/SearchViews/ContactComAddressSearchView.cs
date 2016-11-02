//
// Project: Mark5.Mobile.Droid
// File: ContactComAddressSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactComAddressSearchView : AbstractEditTextSearchView<SearchContactsCriteria>
    {

        public ContactComAddressSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_contact_com_address);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            EditText.Text = criteria.ComAddress;
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.ComAddress = EditText.Text;
        }
    }
}
