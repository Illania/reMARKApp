//
// Project: Mark5.Mobile.Droid
// File: DocumentPrioritySearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentPrioritySearchView : AbstractDropdownSearchView<SearchDocumentsCriteria>
    {
        List<Priority> SelectedPriorities = new List<Priority>();

        public DocumentPrioritySearchView(Android.Content.Context context, DocumentSearchCriteriaFragment f)
            : base(context, Resource.String.search_document_priorities, Resource.String.search_document_priorities_none_selected, f)
        {
        }

        protected override void ClickAction()
        {
            var pllf = new PickPrioritiesListFragment
            {
                SelectedPriorities = SelectedPriorities,
                CloseRequest = UpdatePriorities
            };

            ParentFragment.PushDropdownViewFragment(pllf, pllf.GenerateTag());
        }

        void UpdatePriorities(List<Priority> priorities)
        {
            SelectedPriorities = priorities;
            UpdateBottomTextView(priorities.Count);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            UpdatePriorities(criteria.Priorities);
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Priorities = SelectedPriorities;
        }
    }
}
