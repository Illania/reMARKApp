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
    public class DocumentCategoriesSearchView : AbstractDropdownSearchView<SearchDocumentsCriteria>
    {
        readonly List<int> selectedCategoryIds = new List<int>();

        public DocumentCategoriesSearchView(Context context, DocumentSearchCriteriaFragment documentSearchCriteriaFragment)
            : base(context, Resource.String.search_categories, Resource.String.search_categories_none, documentSearchCriteriaFragment)
        {
        }

        protected override void ClickAction()
        {
            var pclf = new PickCategoriesListFragment
            {
                ObjectType = ObjectType.Document,
                PreselectedCategoryIds = selectedCategoryIds.ToArray(),
                CloseRequest = UpdateCategories
            };

            ParentFragment.ReplaceFragment(pclf, pclf.GenerateTag());
        }

        void UpdateCategories(List<Category> categories) => UpdateCategories(categories.Select(c => c.Id).ToList());

        void UpdateCategories(List<int> categoriesId)
        {
            selectedCategoryIds.Clear();
            selectedCategoryIds.AddRange(categoriesId);
            UpdateBottomTextView(categoriesId.Count);
            UpdateCriteria();
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
