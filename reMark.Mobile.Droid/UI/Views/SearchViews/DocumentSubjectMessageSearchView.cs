using System;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentSubjectMessageSearchView : AbstractMultiSearchView<SearchDocumentsCriteria>
    {
        public DocumentSubjectMessageSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_document_where, Resource.String.search_document_where_hint, GetSubjectMessageCriteriaValuesArray())
        {
        }

        private static int GetSubjectMessageCriteriaValuesArray()
        {
            if(ServerConfig.SystemSettings?.SystemInfo?.SubjectAndMessageSearchAvailable == true)
                return Resource.Array.search_document_subject_message_v2;
            else
                return Resource.Array.search_document_subject_message;
        }
        public override void Refresh()
        {
            if (ServerConfig.SystemSettings?.SystemInfo?.SubjectAndMessageSearchAvailable == false && (int)Criteria.SubjectMessageClause > 2)
                Spinner.SetSelection((int)SubjectMessageClause.SubjectOrMessage);
            else
                Spinner.SetSelection((int)Criteria.SubjectMessageClause);
            
            BottomEditText.Text = Criteria.SubjectMessageField;
        }

        public override void UpdateCriteria()
        {
            Criteria.SubjectMessageClause = (SubjectMessageClause) Spinner.SelectedItemPosition;
            Criteria.SubjectMessageField = BottomEditText.Text;
        }
    }
}