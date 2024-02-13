using System;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
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