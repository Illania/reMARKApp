using System;
namespace Mark5.Mobile.Common.Model
{
    public class IReplyInfo
    {
        public string AppId { get; set; }
        public string FromAddress { get; set; }
        public ParticipantStatus Status { get; set; }
    }
}
