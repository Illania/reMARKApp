namespace Mark5.Mobile.Common.Model
{
    public class OutgoingDocumentAttachmentDescription : IAttachmentDescription
    {
        public string Name { get; set; }
        public long SizeInBytes { get; set; }
        public string Path { get; set; }
    }
}