//
// Project: Mark5.Mobile.Droid
// File: ContactCategoriesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactCategoriesSearchView : AbstractCategoriesSearchView<SearchContactsCriteria>
    {

        public ContactCategoriesSearchView(Context context)
            : base(context)
        {
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            // TODO
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            // TODO
        }
    }
}
