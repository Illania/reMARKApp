//
// Project: Mark5.Mobile.Common
// File: DocumentLinesSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentLinesSearchView : AbstractDropdownSearchView<SearchDocumentsCriteria>
    {
        List<Guid> SelectedLineGuids = new List<Guid>();

        public DocumentLinesSearchView(Android.Content.Context context, DocumentSearchCriteriaFragment f)
            : base(context, Resource.String.search_document_lines, Resource.String.search_document_lines_none_selected, f)
        {
        }

        protected override void ClickAction()
        {
            var pllf = new PickLinesListFragment
            {
                SelectedLinesGuid = SelectedLineGuids,
                CloseRequest = UpdateLines,
            };

            ParentFragment.PushDropdownViewFragment(pllf, pllf.GenerateTag());
        }

        void UpdateLines(List<Guid> lineGuids)
        {
            SelectedLineGuids = lineGuids;
            UpdateSpinnerText(lineGuids.Count);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            UpdateLines(criteria.LineGuids);
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.LineGuids = SelectedLineGuids;
        }
    }
}
