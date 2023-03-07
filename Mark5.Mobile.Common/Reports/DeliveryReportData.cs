using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Reports
{
    public class DeliveryReportData
    {
        public string ReferenceName { get; set; }
        public string Destination { get; set; }
        public DestinationStatus Status { get; set; }

        public DeliveryReportData(string referenceName, string destination, DestinationStatus status)
        {
            ReferenceName = referenceName;
            Destination = destination;
            Status = status;
        }
    }
}
