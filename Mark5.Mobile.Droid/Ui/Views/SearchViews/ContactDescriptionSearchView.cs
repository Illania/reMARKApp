//
// Project: Mark5.Mobile.Droid
// File: ContactDescriptionSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactDescriptionSearchView : AbstractEditTextSearchView<SearchContactsCriteria>
    {

        public ContactDescriptionSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_contact_description);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            EditText.Text = criteria.Description;
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.Description = EditText.Text;
        }
    }
}
