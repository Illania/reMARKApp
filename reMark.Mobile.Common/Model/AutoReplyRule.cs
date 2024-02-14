using System;
using System.Runtime.Serialization;
using reMark.ServiceReference.DataContract;

namespace reMark.Mobile.Common.Model
{
    public class AutoReplyRule
    {
        public int Id { get; set; } = 0;
        public bool Active { get; set; } = false;
        public DateTime ActiveFrom { get; set; } = DateTime.MinValue;
        public DateTime ActiveTo { get; set; } = DateTime.MaxValue;
        public Guid IncomingMailboxGuid { get; set; } = Guid.Empty;
        public string ReplySubject { get; set; } = string.Empty;
        public string ReplyText { get; set; } = string.Empty;

        public AutoReplyRule()
        {
        }
    }
}

