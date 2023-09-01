using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentSavedSearchView : AbstractDropdownSearchView<SearchDocumentsCriteria>
    {
        SavedDocumentsSearch selectedSavedSearch = new SavedDocumentsSearch();
        readonly DocumentSearchCriteriaFragment parent;

        public DocumentSavedSearchView(Android.Content.Context context, DocumentSearchCriteriaFragment f)
            : base(context, Resource.String.saved_searches, Resource.String.saved_searches_none_selected, f)
        {
            parent = f;
        }

        protected override async void ClickAction()
        {
            var (pplf, tag) = PickDocumentSavedSearchesFragment.NewInstance();
            ParentFragment.ReplaceFragment(pplf, tag);
            var selectedSavedSearch = await pplf.Task;
            UpdateSavedSearch(selectedSavedSearch);
            parent.CurrentSavedSearch = selectedSavedSearch;
            parent.ReloadCriteria(selectedSavedSearch.Criteria);
        }

        public void UpdateSavedSearch(SavedDocumentsSearch savedSearch)
        {
            if (savedSearch == null)
                return;
            selectedSavedSearch = savedSearch;
            if(!string.IsNullOrEmpty(selectedSavedSearch?.Name))
                UpdateBottomTextView(selectedSavedSearch?.Name);
        }

        public override void Refresh()
        {
        }

        public override void UpdateCriteria()
        {
        }
    }
}