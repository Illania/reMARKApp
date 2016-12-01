//
// Project: Mark5.Mobile.Droid
// File: DocumentCategoriesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V4.App;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class DocumentCategoriesSearchView : AbstractCategoriesSearchView<SearchDocumentsCriteria>
    {

        public DocumentCategoriesSearchView(Context context, Fragment fragment)
            : base(context, fragment)
        {
            ObjectType = ObjectType.Document;
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedCategories.Clear();
            SelectedCategories.AddRange(criteria.CategoryIds);

            UpdateSubtitle();
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.CategoryIds = SelectedCategories;
        }
    }
}
