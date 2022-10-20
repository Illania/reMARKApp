using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{

    public class Transmit
    {
        public TransmitStatus Status { get; set; }
        public Guid DocGuid { get; set; }
        public Priority Priority { get; set; }
        public List<TransmitDestination> Destinations { get; set; }
    }

    public class TransmitDestination
    {
        public string Address { get; set; }
        public ComAddressLinkType LinkType { get; set; }
        public DestinationStatus Status { get; set; }
    }


    public class DestinationStatus
    {
        public int Attempts { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastConnectAttempt { get; set; }
        public DestinationStatusDetail StatusDetail { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public Guid WasSentByLine { get; set; }
        public string WasSentByLineName { get; set; }

    }

    public enum DestinationStatusDetail
    {
        None = -1,

        /// <summary>First status when document is sent to destination and nobody proccess it</summary>
        InTransmit = 0,

        /// <summary>Task controller processed destination</summary>
        InTaskController = 1,

        /// <summary>Send controller processed destination</summary>
        InSendController = 2,

        /// <summary>Destination is in driver queue to send out</summary>
        InDriver = 3,

        /// <summary>Destination is sending by driver</summary>
        InDriverSend = 4,

        /// <summary>Unexpected error happened</summary>
        SystemError = 5,

        /// <summary>Destination was canceled</summary>
        Cancelled = 6,

        /// <summary>Destination requested to cancel</summary>
        CancelRequested = 7,

        /// <summary>The destination transmit was sent and received approval was received</summary>
        DeliveryAccepted = 8,

        /// <summary>Destination was sent, but later non delivery report received.  Final status. </summary>
        FailedBounced = 9,

        /// <summary>Delivery confirmation was received</summary>
        Delivered = 10
    }

    public enum ComAddressLinkType
    {
        None = -1,
        To = 0,
        From = 1,
        Cc = 2,
        Bcc = 3
    }

}
