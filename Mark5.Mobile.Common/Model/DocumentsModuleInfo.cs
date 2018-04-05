using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class DocumentsModuleInfo
    {
        public OnSendToSystemUser OnSendToSystemUser { get; set; }
        public Line DefaultOutgoingLine { get; set; }

        List<Line> outgoingLines;

        public List<Line> OutgoingLines
        {
            get
            {
                if (outgoingLines == null)
                    outgoingLines = new List<Line>();
                return outgoingLines;
            }
            set => outgoingLines = value;
        }

        List<Line> incomingLines;

        public List<Line> IncomingLines
        {
            get
            {
                if (incomingLines == null)
                    incomingLines = new List<Line>();
                return incomingLines;
            }
            set => incomingLines = value;
        }

        public bool IsMissingAttachmentWarningEnabled { get; set; }
        List<string> forwardAbbreviations;

        public List<string> ForwardAbbreviations
        {
            get
            {
                if (forwardAbbreviations == null)
                    forwardAbbreviations = new List<string>();
                return forwardAbbreviations;
            }
            set => forwardAbbreviations = value;
        }

        List<string> replyAbbreviations;

        public List<string> ReplyAbbreviations
        {
            get
            {
                if (replyAbbreviations == null)
                    replyAbbreviations = new List<string>();
                return replyAbbreviations;
            }
            set => replyAbbreviations = value;
        }

        List<string> attachmentKeywords;

        public List<string> AttachmentKeywords
        {
            get
            {
                if (attachmentKeywords == null)
                    attachmentKeywords = new List<string>();
                return attachmentKeywords;
            }
            set => attachmentKeywords = value;
        }

        public long MaximumAttachmentSizeBytes { get; set; }
        List<DocumentExtraFieldInfo> extraFieldInfos;

        public List<DocumentExtraFieldInfo> ExtraFieldInfos
        {
            get
            {
                if (extraFieldInfos == null)
                    extraFieldInfos = new List<DocumentExtraFieldInfo>();
                return extraFieldInfos;
            }
            set => extraFieldInfos = value;
        }

        public bool AttachmentSearchEnabled { get; set; }
        public bool HandledFieldEnabled { get; set; }
        public DocumentsModulePermissions Permissions { get; set; }
    }
}