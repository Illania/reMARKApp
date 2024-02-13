using System;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentReferenceNumberSearchView : AbstractEditableTextSearchView<SearchDocumentsCriteria>
    {
        public DocumentReferenceNumberSearchView(Android.Content.Context context, LinearLayoutCompat containerLayout)
            : base(context, Resource.String.search_document_reference_number, containerLayout)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.Reference);
        }

        public override void UpdateCriteria()
        {
            Criteria.Reference = GetText();
        }
    }
}