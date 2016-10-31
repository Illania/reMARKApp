//
// Project: Mark5.Mobile.Droid
// File: ContactVatSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactVatSearchView : AbstractEditTextSearchView<SearchContactsCriteria>
    {

        public ContactVatSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_contact_vat);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            EditText.Text = criteria.Vat;
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.Vat = EditText.Text;
        }
    }
}
