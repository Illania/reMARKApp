namespace Mark5.Mobile.Common.Model
{
    public class DocumentAddress
    {
        public string Name { get; set; }
        public CommunicationAddressType Type { get; set; }
        public DocumentAddressType AddressType { get; set; }
        public string Address { get; set; }
        public string FullAddress { get; set; }
    }
}