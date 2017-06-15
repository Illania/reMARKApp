using System;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentCommentsSearchView : AbstractEditableTextSearchView<SearchDocumentsCriteria>
    {
        public DocumentCommentsSearchView(Android.Content.Context context, LinearLayoutCompat containerLayout)
            : base(context, Resource.String.search_document_comments, containerLayout)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.Comment);
        }

        public override void UpdateCriteria()
        {
            Criteria.Comment = GetText();
        }
    }
}