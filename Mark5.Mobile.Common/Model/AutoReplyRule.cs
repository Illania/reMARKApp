using System;
using Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Model
{
    public class AutoReplyRule
    {
        public bool Active { get; set; } = false;
        public DateTime ActiveFrom { get; set; } = DateTime.MinValue;
        public DateTime ActiveTo { get; set; } = DateTime.MaxValue;
        public Guid IncomingMailboxGuid { get; set; }
        public string ReplySubject { get; set; }
        public string ReplyText { get; set; }
        public MarkBodyTypeEnum BodyType { get; set; }

        public AutoReplyRule()
        {
        }
    }
}

