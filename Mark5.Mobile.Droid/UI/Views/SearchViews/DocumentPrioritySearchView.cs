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
        readonly List<Priority> selectedPriorities = new List<Priority>();

        public DocumentPrioritySearchView(Android.Content.Context context, DocumentSearchCriteriaFragment f)
            : base(context, Resource.String.search_document_priorities, Resource.String.search_document_priorities_none_selected, f)
        {
        }

        protected override void ClickAction()
        {
            var pllf = new PickPrioritiesListFragment
            {
                SelectedPriorities = selectedPriorities,
                CloseRequest = UpdatePriorities
            };

            ParentFragment.ReplaceFragment(pllf, pllf.GenerateTag());
        }

        void UpdatePriorities(List<Priority> priorities)
        {
            selectedPriorities.Clear();
            selectedPriorities.AddRange(priorities);
            UpdateBottomTextView(priorities.Count);
            UpdateCriteria();
        }

        public override void Refresh()
        {
            UpdatePriorities(Criteria.Priorities);
        }

        public override void UpdateCriteria()
        {
            Criteria.Priorities.Clear();
            Criteria.Priorities.AddRange(selectedPriorities);
        }
    }
}