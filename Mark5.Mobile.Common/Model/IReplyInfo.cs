using System;
namespace Mark5.Mobile.Common.Model
{
    public class IReplyInfo
    {
        public string AppId { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public ParticipantStatus Status { get; set; } = ParticipantStatus.Accepted;
    }
}
