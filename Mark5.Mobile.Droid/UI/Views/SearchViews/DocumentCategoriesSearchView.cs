//
// Project: Mark5.Mobile.Droid
// File: DocumentCategoriesSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentCategoriesSearchView : AbstractCategoriesSearchView<SearchDocumentsCriteria>
    {
        public DocumentCategoriesSearchView(Context context, ISearchCriteriaFragment fragment)
            : base(context, fragment, ObjectType.Document)
        {
        }

        public override void Refresh()
        {
            UpdateCategories(Criteria.CategoryIds);
        }

        public override void UpdateCriteria()
        {
            Criteria.CategoryIds.Clear();
            Criteria.CategoryIds.AddRange(selectedCategoryIds);
        }
    }

}
