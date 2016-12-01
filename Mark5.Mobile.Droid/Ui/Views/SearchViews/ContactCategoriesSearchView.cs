//
// Project: Mark5.Mobile.Droid
// File: ContactCategoriesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V4.App;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactCategoriesSearchView : AbstractCategoriesSearchView<SearchContactsCriteria>
    {

        public ContactCategoriesSearchView(Context context, Fragment fragment)
            : base(context, fragment)
        {
            ObjectType = ObjectType.Contact;
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            SelectedCategories.Clear();
            SelectedCategories.AddRange(criteria.CategoryIds);

            UpdateSubtitle();
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.CategoryIds = SelectedCategories;
        }
    }
}
