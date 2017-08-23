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

        protected override async void ClickAction()
        {
            var pllf = new PickPrioritiesListFragment(selectedPriorities);
            ParentFragment.ReplaceFragment(pllf, pllf.GenerateTag());
            UpdatePriorities(await pllf.Task);
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