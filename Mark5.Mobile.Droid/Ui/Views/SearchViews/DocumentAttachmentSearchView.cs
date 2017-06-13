using System;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentAttachmentSearchView : AbstractEditableTextSearchView<SearchDocumentsCriteria>
    {
        public DocumentAttachmentSearchView(Android.Content.Context context, LinearLayoutCompat containerLayout)
            : base(context, Resource.String.search_document_attachment, containerLayout)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.AttachmentName);
        }

        public override void UpdateCriteria()
        {
            Criteria.AttachmentName = GetText();
        }
    }
}