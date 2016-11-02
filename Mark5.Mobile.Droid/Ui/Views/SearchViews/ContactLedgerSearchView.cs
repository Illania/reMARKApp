//
// Project: Mark5.Mobile.Droid
// File: ContactLedgerSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactLedgerSearchView : AbstractEditTextSearchView<SearchContactsCriteria>
    {

        public ContactLedgerSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_contact_ledger);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            EditText.Text = criteria.Ledger;
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.Ledger = EditText.Text;
        }
    }
}
