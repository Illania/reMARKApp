using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentLinesSearchView : AbstractDropdownSearchView<SearchDocumentsCriteria>
    {
        readonly List<Guid> selectedLineGuids = new List<Guid>();

        public DocumentLinesSearchView(Android.Content.Context context, DocumentSearchCriteriaFragment f)
            : base(context, Resource.String.search_document_lines, Resource.String.search_document_lines_none_selected, f)
        {
        }

        protected override async void ClickAction()
        {
            var pllf = new PickLinesListFragment(selectedLineGuids);
            ParentFragment.ReplaceFragment(pllf, pllf.GenerateTag());
            UpdateLines(await pllf.Task);
        }

        void UpdateLines(List<Guid> lineGuids)
        {
            selectedLineGuids.Clear();
            selectedLineGuids.AddRange(lineGuids);
            UpdateBottomTextView(lineGuids.Count);
            UpdateCriteria();
        }

        public override void Refresh()
        {
            UpdateLines(Criteria.LineGuids);
        }

        public override void UpdateCriteria()
        {
            Criteria.LineGuids.Clear();
            Criteria.LineGuids.AddRange(selectedLineGuids);
        }
    }
}