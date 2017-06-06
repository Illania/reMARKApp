using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentSubjectMessageSearchView : AbstractMultiSearchView<SearchDocumentsCriteria>
    {
        public DocumentSubjectMessageSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_document_where, Resource.String.search_document_where_hint, Resource.Array.search_document_subject_message)
        {
        }

        public override void Refresh()
        {
            Spinner.SetSelection((int) Criteria.SubjectMessageClause);
            BottomEditText.Text = Criteria.SubjectMessageField;
        }

        public override void UpdateCriteria()
        {
            Criteria.SubjectMessageClause = (SubjectMessageClause) Spinner.SelectedItemPosition;
            Criteria.SubjectMessageField = BottomEditText.Text;
        }
    }
}