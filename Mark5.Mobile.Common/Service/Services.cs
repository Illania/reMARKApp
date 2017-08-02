namespace Mark5.Mobile.Common.Service
{
    public static class Services
    {
        public static IDocumentsUploadService DocumentsUploadService { get; set; } = new DocumentsUploadService();
        public static IDocumentPreviewsDownloadService DocumentPreviewsDownloadService { get; set; } = new DocumentPreviewsDownloadService();
        public static IDocumentsDownloadService DocumentsDownloadService { get; set; } = new DocumentsDownloadService();
    }
}
