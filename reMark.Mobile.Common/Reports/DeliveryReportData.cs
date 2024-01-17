using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.Reports
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
