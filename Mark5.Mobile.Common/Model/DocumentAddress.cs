namespace Mark5.Mobile.Common.Model
{
    public class DocumentAddress
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CommunicationAddressType Type { get; set; }
        public DocumentAddressType AddressType { get; set; }
        public string Address { get; set; }
        public string FullAddress { get; set; }
        public string Attention { get; set; }
        public string FullAttention { get; set; }
        public ObjectType ObjectType { get; set; }
        public int ObjectId { get; set; }
    }
}