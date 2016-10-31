//
// Project: Mark5.Mobile.Droid
// File: ContactShortIdSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactShortIdSearchView : AbstractEditTextSearchView<SearchContactsCriteria>
    {

        public ContactShortIdSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_contact_shortid);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            EditText.Text = criteria.ShortId;
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.ShortId = EditText.Text;
        }
    }
}
