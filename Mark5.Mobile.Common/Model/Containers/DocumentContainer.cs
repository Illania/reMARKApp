using System;

namespace Mark5.Mobile.Common.Model.Containers
{
    public class DocumentContainer
    {
        public DocumentPreview DocumentPreview { get; }
        public Document Document { get; }

        public DocumentContainer(DocumentPreview documentPreview, Document document)
        {
            if (documentPreview == null)
                throw new ArgumentNullException(nameof(documentPreview));
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (documentPreview.Id != document.Id)
                throw new ArgumentException("DocumentPreview and Document do not match.");

            DocumentPreview = documentPreview;
            Document = document;
        }
    }
}