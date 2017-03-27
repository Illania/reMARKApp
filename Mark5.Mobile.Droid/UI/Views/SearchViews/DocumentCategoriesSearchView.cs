//
// Project: Mark5.Mobile.Droid
// File: DocumentCategoriesSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentCategoriesSearchView : AbstractDropdownSearchView<SearchDocumentsCriteria, Category>
    {
        DocumentSearchCriteriaFragment documentSearchCriteriaFragment;

        DocumentCategoriesSearchView(Context context)
            : base(context, Resource.String.search_categories, Resource.String.search_categories_none)
        {
        }

        public DocumentCategoriesSearchView(Context context, DocumentSearchCriteriaFragment documentSearchCriteriaFragment) : this(context)
        {
            this.documentSearchCriteriaFragment = documentSearchCriteriaFragment;
        }

        protected override void ClickAction()
        {
            var pclf = new PickCategoriesListFragment
            {
                ObjectType = ObjectType.Document,
                PreselectedCategoryIds = new int[] { },
                CloseRequest = categories =>
                {
                    //TODO
                }
            };

            documentSearchCriteriaFragment.PushDropdownViewFragment(pclf, pclf.GenerateTag());
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO
        }
    }


}
