using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mark5.ServiceReference.DataContract
{
    #region Abstract public classes

    [DataContract(Name = "AbstractParameters", Namespace = "com.nordic-it.appservice.v3")]
    public abstract class AbstractParameters
    {
        [DataMember(Name = "Token", Order = 0, IsRequired = true)]
        public string Token { get; set; }
    }

    #endregion

    #region Authentication

    [DataContract(Name = "AuthenticateParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class AuthenticateParameters
    {
        [DataMember(Name = "Username", Order = 0, IsRequired = true)]
        public string Username { get; set; }

        [DataMember(Name = "Password", Order = 0, IsRequired = true)]
        public string Password { get; set; }

        [DataMember(Name = "InstallationId", Order = 0, IsRequired = true)]
        public string InstallationId { get; set; }

        [DataMember(Name = "FriendlyDeviceName", Order = 0, IsRequired = true)]
        public string FriendlyDeviceName { get; set; }

        [DataMember(Name = "DeviceType", Order = 0, IsRequired = true)]
        public DeviceType DeviceType { get; set; }
    }

    [DataContract(Name = "AuthenticateResult", Namespace = "com.nordic-it.appservice.v3")]
    public class AuthenticateResult
    {
        [DataMember(Name = "Token", Order = 0, IsRequired = true)]
        public string Token { get; set; }
    }

    #endregion

    #region Folders

    [DataContract(Name = "GetFoldersParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetFoldersParameters : AbstractParameters
    {
        [DataMember(Name = "FolderId", Order = 0)]
        public int FolderId { get; set; } = -1;

        [DataMember(Name = "ModuleType", Order = 0)]
        public ModuleType ModuleType { get; set; }

        [DataMember(Name = "Depth", Order = 0)]
        public int Depth { get; set; } = -1;
    }

    [DataContract(Name = "GetFoldersResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetFoldersResult
    {
        [DataMember(Name = "Folders", Order = 0)]
        public List<Folder> Folders { get; set; } = new List<Folder>();
    }

    [DataContract(Name = "SearchFoldersParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchFoldersParameters : AbstractParameters
    {
        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "ModuleType", Order = 0)]
        public ModuleType ModuleType { get; set; }
    }

    [DataContract(Name = "SearchFoldersResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchFoldersResult
    {
        [DataMember(Name = "Folders", Order = 0)]
        public List<Folder> Folders { get; set; } = new List<Folder>();
    }

    [DataContract(Name = "Folder", Namespace = "com.nordic-it.appservice.v3", IsReference = true)]
    public class Folder
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "ParentFolderId", Order = 0)]
        public int ParentFolderId { get; set; } = -1;

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Module", Order = 0)]
        public ModuleType Module { get; set; }

        [DataMember(Name = "Type", Order = 0)]
        public FolderType Type { get; set; }

        [DataMember(Name = "InternalType", Order = 0)]
        public FolderInternalType InternalType { get; set; }

        [DataMember(Name = "HasSubFolders", Order = 0)]
        public bool HasSubFolders { get; set; }

        [DataMember(Name = "SubFolders", Order = 0)]
        public List<Folder> SubFolders { get; set; } = new List<Folder>();

        [DataMember(Name = "Subscribed", Order = 0)] public bool Subscribed;

        [DataMember(Name = "Position", Order = 0)]
        public int Position { get; set; } = -1;

        [DataMember(Name = "OptionalParameters", Order = 0)]
        public OptionalParameters OptionalParameters { get; set; }
    }

    [DataContract(Name = "OptionalParameters", Namespace = "com.nordic-it.appservice.v3")]
    [KnownType(typeof(CalendarEventOptionalParameters))]
    public abstract class OptionalParameters
    {
    }

    [DataContract(Name = "CalendarEventOptionalParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class CalendarEventOptionalParameters : OptionalParameters
    {
        [DataMember(Name = "CanContainAppointments", Order = 0)]
        public bool CanContainAppointments { get; set; }

        [DataMember(Name = "CanContainTasks", Order = 0)]
        public bool CanContainTasks { get; set; }
    }

    [DataContract(Name = "ModuleType", Namespace = "com.nordic-it.appservice.v3")]
    public enum ModuleType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Documents")] Documents = 1,
        [EnumMember(Value = "Contacts")] Contacts = 2,
        [EnumMember(Value = "Shortcodes")] Shortcodes = 3,
        [EnumMember(Value = "Calendar")] Calendar = 4,
    }

    [DataContract(Name = "FolderType", Namespace = "com.nordic-it.appservice.v3")]
    public enum FolderType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Inbox")] Inbox = 1,
        [EnumMember(Value = "Outbox")] Outbox = 2,
        [EnumMember(Value = "Draft")] Draft = 3,
        [EnumMember(Value = "Cabinet")] Cabinet = 4,
        [EnumMember(Value = "Spam")] Spam = 5,
        [EnumMember(Value = "External")] External = 6,
        [EnumMember(Value = "DeliveryReports")] DeliveryReports = 7,
        [EnumMember(Value = "Companies")] Companies = 8,
        [EnumMember(Value = "Persons")] Persons = 9,
        [EnumMember(Value = "Personal")] Personal = 10,
    }

    [DataContract(Name = "FolderInternalType", Namespace = "com.nordic-it.appservice.v3")]
    public enum FolderInternalType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Static")] Static = 1,
        [EnumMember(Value = "Dynamic")] Dynamic = 2,
        [EnumMember(Value = "FilterView")] FilterView = 3,
        [EnumMember(Value = "Worktray")] Worktray = 4,
        [EnumMember(Value = "Custom")] Custom = 5,
    }

    #endregion

    #region Documents

    [DataContract(Name = "GetDocumentPreviewsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetDocumentPreviewsParameters : AbstractParameters
    {
        [DataMember(Name = "FolderId", Order = 0)]
        public int FolderId { get; set; } = -1;

        [DataMember(Name = "StartId", Order = 0)]
        public int StartId { get; set; } = -1;

        [DataMember(Name = "EndId", Order = 0)]
        public int EndId { get; set; } = -1;

        [DataMember(Name = "MaxToFetch", Order = 0)]
        public int MaxToFetch { get; set; } = -1;

        [DataMember(Name = "ReverseSortOrder", Order = 0)]
        public bool ReverseSortOrder { get; set; }
    }

    [DataContract(Name = "GetDocumentPreviewsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetDocumentPreviewsResult
    {
        [DataMember(Name = "DocumentPreviews", Order = 0)]
        public List<DocumentPreview> DocumentPreviews { get; set; } = new List<DocumentPreview>();
    }

    [DataContract(Name = "GetDocumentParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetDocumentParameters : AbstractParameters
    {
        [DataMember(Name = "FolderId", Order = 0)]
        public int FolderId { get; set; } = -1;

        [DataMember(Name = "DocumentId", Order = 0)]
        public int DocumentId { get; set; } = -1;

        [DataMember(Name = "IncludePreview", Order = 0)]
        public bool IncludePreview { get; set; }

        [DataMember(Name = "BodyRequest", Order = 0)]
        public DocumentBodyTypeRequest BodyRequest { get; set; }
    }

    [DataContract(Name = "GetDocumentResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetDocumentResult
    {
        [DataMember(Name = "Document", Order = 0)]
        public Document Document { get; set; }

        [DataMember(Name = "DocumentPreview", Order = 0)]
        public DocumentPreview DocumentPreview { get; set; }
    }

    [DataContract(Name = "SendDocumentParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SendDocumentParameters : AbstractParameters
    {
        [DataMember(Name = "Document", Order = 0)]
        public Document Document { get; set; }

        [DataMember(Name = "DocumentPreview", Order = 0)]
        public DocumentPreview DocumentPreview { get; set; }

        [DataMember(Name = "CreationModeFlag", Order = 0)]
        public DocumentCreationModeFlag CreationModeFlag { get; set; }

        [DataMember(Name = "PreceedingDocumentId", Order = 0)]
        public int PreceedingDocumentId { get; set; } = -1;

        [DataMember(Name = "PreceedingDocumentFolderId", Order = 0)]
        public int PreceedingDocumentFolderId { get; set; } = -1;

        [DataMember(Name = "SendOn", Order = 0)]
        public DateTime SendOn { get; set; }

        [DataMember(Name = "ConfirmRead", Order = 0)]
        public bool ConfirmRead { get; set; }

        [DataMember(Name = "ConfirmDelivery", Order = 0)]
        public bool ConfirmDelivery { get; set; }

        [DataMember(Name = "TemporaryAttachmentGuids", Order = 0)]
        public List<Guid> TemporaryAttachmentGuids { get; set; } = new List<Guid>();
    }

    [DataContract(Name = "SendDocumentResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SendDocumentResult
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "ReferenceNumber", Order = 0)]
        public string ReferenceNumber { get; set; }
    }

    [DataContract(Name = "SetDocumentsReadStatusParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SetDocumentsReadStatusParameters : AbstractParameters
    {
        [DataMember(Name = "DocumentIds", Order = 0)]
        public int[] DocumentIds { get; set; }

        [DataMember(Name = "IsRead", Order = 0)]
        public bool IsRead { get; set; }
    }

    [DataContract(Name = "SetDocumentsReadStatusResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SetDocumentsReadStatusResult
    {
    }

    [DataContract(Name = "SetDocumentPriorityParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SetDocumentPriorityParameters : AbstractParameters
    {
        [DataMember(Name = "DocumentIds", Order = 0)]
        public int[] DocumentIds { get; set; }

        [DataMember(Name = "Priority", Order = 0)]
        public Priority Priority { get; set; }
    }

    [DataContract(Name = "SetDocumentPriorityResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SetDocumentPriorityResult
    {
    }

    [DataContract(Name = "MoveToSpamParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class MoveToSpamParameters : AbstractParameters
    {
        [DataMember(Name = "DocumentIds", Order = 0)]
        public int[] DocumentIds { get; set; }
    }

    [DataContract(Name = "MoveToSpamResult", Namespace = "com.nordic-it.appservice.v3")]
    public class MoveToSpamResult
    {
    }

    [DataContract(Name = "GetTemplatePreviewsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetTemplatePreviewsParameters : AbstractParameters
    {
    }

    [DataContract(Name = "GetTemplatePreviewsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetTemplatePreviewsResult
    {
        [DataMember(Name = "TemplatePreviews", Order = 0)]
        public List<TemplatePreview> TemplatePreviews { get; set; } = new List<TemplatePreview>();
    }

    [DataContract(Name = "GetTemplateParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetTemplateParameters : AbstractParameters
    {
        [DataMember(Name = "TemplateId", Order = 0)]
        public int TemplateId { get; set; } = -1;

        [DataMember(Name = "IncludePreview", Order = 0)]
        public bool IncludePreview { get; set; }
    }

    [DataContract(Name = "GetTemplateResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetTemplateResult
    {
        [DataMember(Name = "TemplatePreview", Order = 0)]
        public TemplatePreview TemplatePreview { get; set; }

        [DataMember(Name = "Template", Order = 0)]
        public Template Template { get; set; }
    }

    [DataContract(Name = "GetDefaultTemplateParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetDefaultTemplateParameters : AbstractParameters
    {
        [DataMember(Name = "CreationModeFlag", Order = 0)]
        public DocumentCreationModeFlag CreationModeFlag { get; set; }

        [DataMember(Name = "IncludePreview", Order = 0)]
        public bool IncludePreview { get; set; }
    }

    [DataContract(Name = "GetDefaultTemplateResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetDefaultTemplateResult
    {
        [DataMember(Name = "NoDefaultTemplate", Order = 0)]
        public bool NoDefaultTemplate { get; set; }

        [DataMember(Name = "TemplatePreview", Order = 0)]
        public TemplatePreview TemplatePreview { get; set; }

        [DataMember(Name = "Template", Order = 0)]
        public Template Template { get; set; }
    }

    [DataContract(Name = "GetLinesParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetLinesParameters : AbstractParameters
    {
    }

    [DataContract(Name = "GetLinesResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetLinesResult
    {
        [DataMember(Name = "OutgoingLines", Order = 0)]
        public List<Line> OutgoingLines { get; set; } = new List<Line>();

        [DataMember(Name = "IncomingLines", Order = 0)]
        public List<Line> IncomingLines { get; set; } = new List<Line>();
    }

    [DataContract(Name = "Document", Namespace = "com.nordic-it.appservice.v3")]
    public class Document
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Lines", Order = 0)]
        public List<Line> Lines { get; set; } = new List<Line>();

        [DataMember(Name = "HtmlBody", Order = 0)]
        public string HtmlBody { get; set; }

        [DataMember(Name = "PlainTextBody", Order = 0)]
        public string PlainTextBody { get; set; }

        [DataMember(Name = "ReadByUserIds", Order = 0)]
        public List<int> ReadByUserIds { get; set; } = new List<int>();

        [DataMember(Name = "ReadByUserNames", Order = 0)]
        public Dictionary<int, string> ReadByUserNames { get; set; } = new Dictionary<int, string>();

        [DataMember(Name = "Attachments", Order = 0)]
        public List<AttachmentDescription> Attachments { get; set; } = new List<AttachmentDescription>();

        [DataMember(Name = "Comments", Order = 0)]
        public List<Comment> Comments { get; set; } = new List<Comment>();

        [DataMember(Name = "ExtraFields", Order = 0)]
        public Dictionary<DocumentExtraFieldInfo, string> ExtraFields { get; set; } = new Dictionary<DocumentExtraFieldInfo, string>();

        [DataMember(Name = "IsEncrypted", Order = 0)]
        public bool IsEncrypted { get; set; }
    }

    [DataContract(Name = "DocumentPreview", Namespace = "com.nordic-it.appservice.v3")]
    public class DocumentPreview
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "ReferenceNumber", Order = 0)]
        public string ReferenceNumber { get; set; }

        [DataMember(Name = "Addresses", Order = 0)]
        public List<DocumentAddress> Addresses { get; set; } = new List<DocumentAddress>();

        [DataMember(Name = "Subject", Order = 0)]
        public string Subject { get; set; }

        [DataMember(Name = "Preview", Order = 0)]
        public string Preview { get; set; }

        [DataMember(Name = "Direction", Order = 0)]
        public DocumentDirection Direction { get; set; }

        [DataMember(Name = "Priority", Order = 0)]
        public Priority Priority { get; set; }

        [DataMember(Name = "IsReadByAnyone", Order = 0)]
        public bool IsReadByAnyone { get; set; }

        [DataMember(Name = "IsReadByCurrent", Order = 0)]
        public bool IsReadByCurrent { get; set; }

        [DataMember(Name = "CommentsCount", Order = 0)]
        public int CommentsCount { get; set; }

        [DataMember(Name = "AttachmentsCount", Order = 0)]
        public int AttachmentsCount { get; set; }

        [DataMember(Name = "Categories", Order = 0)]
        public List<Category> Categories { get; set; } = new List<Category>();

        [DataMember(Name = "DateReceived", Order = 0)]
        public DateTime DateReceived { get; set; }

        [DataMember(Name = "CreatorId", Order = 0)]
        public int CreatorId { get; set; }

        [DataMember(Name = "Creator", Order = 0)]
        public string Creator { get; set; }
    }

    [DataContract(Name = "DocumentAddress", Namespace = "com.nordic-it.appservice.v3")]
    public class DocumentAddress
    {
        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Type", Order = 0)]
        public CommunicationAddressType Type { get; set; }

        [DataMember(Name = "AddressType", Order = 0)]
        public DocumentAddressType AddressType { get; set; }

        [DataMember(Name = "Address", Order = 0)]
        public string Address { get; set; }

        [DataMember(Name = "FullAddress", Order = 0)]
        public string FullAddress { get; set; }

        [DataMember(Name = "Attention", Order = 1)]
        public string Attention { get; set; }

        [DataMember(Name = "FullAttention", Order = 1)]
        public string FullAttention { get; set; }

        [DataMember(Name = "ObjectId", Order = 1)]
        public int ObjectId { get; set; } = -1;

        [DataMember(Name = "ObjectType", Order = 1)]
        public ObjectType ObjectType { get; set; }

        [DataMember(Name = "Id", Order = 1)]
        public int Id { get; set; } = -1;
    }

    [DataContract(Name = "AttachmentDescription", Namespace = "com.nordic-it.appservice.v3")]
    public class AttachmentDescription
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "SizeInBytes", Order = 0)]
        public long SizeInBytes { get; set; }
    }

    [DataContract(Name = "Comment", Namespace = "com.nordic-it.appservice.v3")]
    public class Comment
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "DateAdded", Order = 0)]
        public DateTime DateAdded { get; set; }

        [DataMember(Name = "UserId", Order = 0)]
        public int UserId { get; set; }

        [DataMember(Name = "UserName", Order = 0)]
        public string UserName { get; set; }

        [DataMember(Name = "Content", Order = 0)]
        public string Content { get; set; }

        [DataMember(Name = "ParentId", Order = 0)]
        public int ParentId { get; set; }

        [DataMember(Name = "ParentTypeId", Order = 0)]
        public int ParentTypeId { get; set; }
    }

    [DataContract(Name = "Line", Namespace = "com.nordic-it.appservice.v3")]
    public class Line
    {
        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "FromAddress", Order = 0)]
        public string FromAddress { get; set; }
    }

    [DataContract(Name = "TemplatePreview", Namespace = "com.nordic-it.appservice.v3")]
    public class TemplatePreview
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Private", Order = 0)]
        public bool Private { get; set; }

        [DataMember(Name = "CreationMode", Order = 0)]
        public DocumentCreationModeFlag CreationMode { get; set; }
    }

    [DataContract(Name = "Template", Namespace = "com.nordic-it.appservice.v3")]
    public class Template
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Subject", Order = 0)]
        public string Subject { get; set; }

        [DataMember(Name = "LineGuid", Order = 0)]
        public Guid LineGuid { get; set; }

        [DataMember(Name = "ContentType", Order = 0)]
        public ContentType ContentType { get; set; }

        [DataMember(Name = "Content", Order = 0)]
        public string Content { get; set; }
    }

    [DataContract(Name = "DocumentCreationModeFlag", Namespace = "com.nordic-it.appservice.v3")]
    [Flags]
    public enum DocumentCreationModeFlag
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "New")] New = 1,
        [EnumMember(Value = "Reply")] Reply = 2,
        [EnumMember(Value = "ReplyAll")] ReplyAll = 4,
        [EnumMember(Value = "Forward")] Forward = 8,
        [EnumMember(Value = "Edit")] Edit = 16,
        [EnumMember(Value = "Resend")] Resend = 32,
        [EnumMember(Value = "Redirect")] Redirect = 64,
    }

    [DataContract(Name = "Priority", Namespace = "com.nordic-it.appservice.v3")]
    public enum Priority
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Ignore")] Ignore = 1,
        [EnumMember(Value = "Low")] Low = 2,
        [EnumMember(Value = "Normal")] Normal = 3,
        [EnumMember(Value = "Urgent")] Urgent = 4,
        [EnumMember(Value = "System")] System = 5,
    }

    [DataContract(Name = "DocumentDirection", Namespace = "com.nordic-it.appservice.v3")]
    public enum DocumentDirection
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Outgoing")] Outgoing = 1,
        [EnumMember(Value = "Incoming")] Incoming = 2,
        [EnumMember(Value = "Draft")] Draft = 3,
        [EnumMember(Value = "External")] External = 4
    }

    [DataContract(Name = "DocumentAddressType", Namespace = "com.nordic-it.appservice.v3")]
    public enum DocumentAddressType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "To")] To = 1,
        [EnumMember(Value = "From")] From = 2,
        [EnumMember(Value = "Cc")] Cc = 3,
        [EnumMember(Value = "Bcc")] Bcc = 4,
        [EnumMember(Value = "ReplyTo")] ReplyTo = 5
    }

    [DataContract(Name = "ContentType", Namespace = "com.nordic-it.appservice.v3")]
    public enum ContentType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "PlainText")] PlainText = 1,
        [EnumMember(Value = "Html")] Html = 2,
    }

    [DataContract(Name = "DocumentBodyTypeRequest", Namespace = "com.nordic-it.appservice.v3")]
    public enum DocumentBodyTypeRequest
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "HtmlOnly")] HtmlOnly = 1,
        [EnumMember(Value = "PlainTextOnly")] PlainTextOnly = 2,
        [EnumMember(Value = "HtmlAndPlainText")] HtmlAndPlainText = 3,
    }

    #endregion

    #region Contact

    [DataContract(Name = "GetContactPreviewsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetContactPreviewsParameters : AbstractParameters
    {
        [DataMember(Name = "FolderId", Order = 0)]
        public int FolderId { get; set; } = -1;

        [DataMember(Name = "StartRowId", Order = 0)]
        public int StartRowId { get; set; } = -1;

        [DataMember(Name = "MaxToFetch", Order = 0)]
        public int MaxToFetch { get; set; } = -1;
    }

    [DataContract(Name = "GetContactPreviewsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetContactPreviewsResult
    {
        [DataMember(Name = "ContactPreviews", Order = 0)]
        public List<ContactPreview> ContactPreviews { get; set; } = new List<ContactPreview>();
    }

    [DataContract(Name = "GetContactParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetContactParameters : AbstractParameters
    {
        [DataMember(Name = "FolderId", Order = 0)]
        public int FolderId { get; set; } = -1;

        [DataMember(Name = "ContactId", Order = 0)]
        public int ContactId { get; set; } = -1;

        [DataMember(Name = "IncludePreview", Order = 0)]
        public bool IncludePreview { get; set; }
    }

    [DataContract(Name = "GetContactResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetContactResult
    {
        [DataMember(Name = "Contact", Order = 0)]
        public Contact Contact { get; set; }

        [DataMember(Name = "ContactPreview", Order = 0)]
        public ContactPreview ContactPreview { get; set; }
    }

    [DataContract(Name = "CreateOrUpdateContactParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class CreateOrUpdateContactParameters : AbstractParameters
    {
        [DataMember(Name = "Contact", Order = 0)]
        public Contact Contact { get; set; }

        [DataMember(Name = "ContactPreview", Order = 0)]
        public ContactPreview ContactPreview { get; set; }

        [DataMember(Name = "ParentObjectId", Order = 0)]
        public int ParentObjectId { get; set; } = -1;
    }

    [DataContract(Name = "CreateOrUpdateContactResult", Namespace = "com.nordic-it.appservice.v3")]
    public class CreateOrUpdateContactResult
    {
        [DataMember(Name = "Updated", Order = 0)]
        public bool Updated { get; set; }

        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }
    }

    [DataContract(Name = "Contact", Namespace = "com.nordic-it.appservice.v3")]
    public class Contact
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "FirstName", Order = 0)]
        public string FirstName { get; set; }

        [DataMember(Name = "Patronymic", Order = 0)]
        public string Patronymic { get; set; }

        [DataMember(Name = "LastName", Order = 0)]
        public string LastName { get; set; }

        [DataMember(Name = "Position", Order = 0)]
        public string Position { get; set; }

        [DataMember(Name = "WebPageAddress", Order = 0)]
        public string WebPageAddress { get; set; }

        [DataMember(Name = "Account", Order = 0)]
        public string Account { get; set; }

        [DataMember(Name = "Vat", Order = 0)]
        public string Vat { get; set; }

        [DataMember(Name = "BirthDate", Order = 0)]
        public DateTime BirthDate { get; set; }

        [DataMember(Name = "Ledger", Order = 0)]
        public string Ledger { get; set; }

        [DataMember(Name = "PrimaryPerson", Order = 0)]
        public ContactPreview PrimaryPerson { get; set; }

        [DataMember(Name = "Children", Order = 0)]
        public List<ContactPreview> Children { get; set; } = new List<ContactPreview>();

        [DataMember(Name = "ResponsibleUserIds", Order = 0)]
        public List<int> ResponsibleUserIds { get; set; } = new List<int>();

        [DataMember(Name = "ResponsibleUsers", Order = 0)]
        public Dictionary<int, string> ResponsibleUsers { get; set; } = new Dictionary<int, string>();

        [DataMember(Name = "PreferrableType", Order = 0)]
        public CommunicationAddressType PreferrableType { get; set; }

        [DataMember(Name = "CommunicationAddresses", Order = 0)]
        public List<CommunicationAddress> CommunicationAddresses { get; set; } = new List<CommunicationAddress>();

        [DataMember(Name = "PhysicalAddresses", Order = 0)]
        public List<PhysicalAddress> PhysicalAddresses { get; set; } = new List<PhysicalAddress>();

        [DataMember(Name = "Comments", Order = 0)]
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

    [DataContract(Name = "ContactPreview", Namespace = "com.nordic-it.appservice.v3")]
    public class ContactPreview
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "RowId", Order = 0)]
        public int RowId { get; set; } = -1;

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "CompanyName", Order = 0)]
        public string CompanyName { get; set; }

        [DataMember(Name = "ShortId", Order = 0)]
        public string ShortId { get; set; }

        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }

        [DataMember(Name = "Type", Order = 0)]
        public ContactType Type { get; set; }

        [DataMember(Name = "Categories", Order = 0)]
        public List<Category> Categories { get; set; } = new List<Category>();

        [DataMember(Name = "PrimaryAddress", Order = 0)]
        public CommunicationAddress PrimaryAddress { get; set; }

        [DataMember(Name = "CommentsCount", Order = 0)]
        public int CommentsCount { get; set; }
    }

    [DataContract(Name = "PhysicalAddress", Namespace = "com.nordic-it.appservice.v3")]
    public class PhysicalAddress
    {
        [DataMember(Name = "Type", Order = 0)]
        public PhysicalAddressType Type { get; set; }

        [DataMember(Name = "Country", Order = 0)]
        public CountryInfo Country { get; set; }

        [DataMember(Name = "Street", Order = 0)]
        public string Street { get; set; }

        [DataMember(Name = "ZipCode", Order = 0)]
        public string ZipCode { get; set; }

        [DataMember(Name = "Area", Order = 0)]
        public string Area { get; set; }

        [DataMember(Name = "City", Order = 0)]
        public string City { get; set; }
    }

    [DataContract(Name = "CountryInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class CountryInfo
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "FaxPrefix", Order = 0)]
        public int FaxPrefix { get; set; } = -1;

        [DataMember(Name = "TelexPrefix", Order = 0)]
        public int TelexPrefix { get; set; } = -1;

        [DataMember(Name = "CCode", Order = 0)]
        public string CCode { get; set; }

        [DataMember(Name = "CCode3", Order = 0)]
        public string CCode3 { get; set; }

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }
    }

    [DataContract(Name = "PhysicalAddressType", Namespace = "com.nordic-it.appservice.v3")]
    public class PhysicalAddressType
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }
    }

    [DataContract(Name = "CommunicationAddress", Namespace = "com.nordic-it.appservice.v3")]
    public class CommunicationAddress
    {
        [DataMember(Name = "CommunicationAddressType", Order = 0)]
        public CommunicationAddressType Type { get; set; }

        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }

        [DataMember(Name = "Address", Order = 0)]
        public string Address { get; set; }

        [DataMember(Name = "IsPrimary", Order = 0)]
        public bool IsPrimary { get; set; }
    }

    [DataContract(Name = "CommunicationAddressType", Namespace = "com.nordic-it.appservice.v3")]
    public enum CommunicationAddressType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Email")] Email = 1,
        [EnumMember(Value = "Fax")] Fax = 2,
        [EnumMember(Value = "Phone")] Phone = 3,
        [EnumMember(Value = "Telex")] Telex = 4,
        [EnumMember(Value = "Mobile")] Mobile = 5,
        [EnumMember(Value = "IM")] IM = 6,
        [EnumMember(Value = "Internal")] Internal = 7,
        [EnumMember(Value = "System")] System = 8,
        [EnumMember(Value = "Skype")] Skype = 9,
    }

    [DataContract(Name = "ContactType", Namespace = "com.nordic-it.appservice.v3")]
    public enum ContactType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Person")] Person = 1,
        [EnumMember(Value = "Department")] Department = 2,
        [EnumMember(Value = "Company")] Company = 3,
    }

    #endregion

    #region Shortcodes module

    [DataContract(Name = "GetShortcodePreviewsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetShortcodePreviewsParameters : AbstractParameters
    {
        [DataMember(Name = "FolderId", Order = 0)]
        public int FolderId { get; set; } = -1;

        [DataMember(Name = "StartRowId", Order = 0)]
        public int StartRowId { get; set; } = -1;

        [DataMember(Name = "MaxToFetch", Order = 0)]
        public int MaxToFetch { get; set; } = -1;
    }

    [DataContract(Name = "GetShortcodePreviewsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetShortcodePreviewsResult
    {
        [DataMember(Name = "ShortcodePreviews", Order = 0)]
        public List<ShortcodePreview> ShortcodePreviews { get; set; } = new List<ShortcodePreview>();
    }

    [DataContract(Name = "GetShortcodeParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetShortcodeParameters : AbstractParameters
    {
        [DataMember(Name = "FolderId", Order = 0)]
        public int FolderId { get; set; } = -1;

        [DataMember(Name = "ShortcodeId", Order = 0)]
        public int ShortcodeId { get; set; } = -1;

        [DataMember(Name = "IncludePreview", Order = 0)]
        public bool IncludePreview { get; set; }
    }

    [DataContract(Name = "GetShortcodeResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetShortcodeResult
    {
        [DataMember(Name = "ShortcodePreview", Order = 0)]
        public ShortcodePreview ShortcodePreview { get; set; }

        [DataMember(Name = "Shortcode", Order = 0)]
        public Shortcode Shortcode { get; set; }
    }

    [DataContract(Name = "CreateOrUpdateShortcodeParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class CreateOrUpdateShortcodeParameters : AbstractParameters
    {
        [DataMember(Name = "Shortcode", Order = 0)]
        public Shortcode Shortcode { get; set; }

        [DataMember(Name = "ShortcodePreview", Order = 0)]
        public ShortcodePreview ShortcodePreview { get; set; }
    }

    [DataContract(Name = "CreateOrUpdateShortcodeResult", Namespace = "com.nordic-it.appservice.v3")]
    public class CreateOrUpdateShortcodeResult
    {
        [DataMember(Name = "Updated", Order = 0)]
        public bool Updated { get; set; }

        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }
    }

    [DataContract(Name = "ShortcodePreview", Namespace = "com.nordic-it.appservice.v3")]
    public class ShortcodePreview
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "RowId", Order = 0)]
        public int RowId { get; set; }

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }

        [DataMember(Name = "AddressCount", Order = 0)]
        public int AddressCount { get; set; }
    }

    [DataContract(Name = "Shortcode", Namespace = "com.nordic-it.appservice.v3")]
    public class Shortcode
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Addresses", Order = 0)]
        public List<DocumentAddress> Addresses { get; set; } = new List<DocumentAddress>();
    }

    #endregion

    #region Calendar module

    [DataContract(Name = "GetCalendarEventsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetCalendarEventsParameters : AbstractParameters
    {
        [DataMember(Name = "CalendarIds", Order = 0)]
        public List<int> CalendarIds { get; set; } = new List<int>();

        [DataMember(Name = "GetAppointments", Order = 0)]
        public bool GetAppointments { get; set; }

        [DataMember(Name = "GetTasks", Order = 0)]
        public bool GetTasks { get; set; }

        [DataMember(Name = "StartDate", Order = 0)]
        public DateTime StartDate { get; set; }

        [DataMember(Name = "EndDate", Order = 0)]
        public DateTime EndDate { get; set; }
    }

    [DataContract(Name = "GetCalendarEventsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetCalendarEventsResult
    {
        [DataMember(Name = "CalendarAppointment", Order = 0)]
        public List<CalendarAppointment> CalendarAppointments { get; set; } = new List<CalendarAppointment>();

        [DataMember(Name = "CalendarTasks", Order = 0)]
        public List<CalendarTask> CalendarTasks { get; set; } = new List<CalendarTask>();
    }

    [DataContract(Name = "GetCalendarAppointmentParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetCalendarAppointmentParameters : AbstractParameters
    {
        [DataMember(Name = "CalendarId", Order = 0)]
        public int CalendarId { get; set; } = -1;

        [DataMember(Name = "CalendarAppointmentId", Order = 0)]
        public int CalendarAppointmentId { get; set; } = -1;
    }

    [DataContract(Name = "GetCalendarAppointmentResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetCalendarAppointmentResult
    {
        [DataMember(Name = "CalendarAppointment", Order = 0)]
        public CalendarAppointment CalendarAppointment { get; set; }
    }

    [DataContract(Name = "GetCalendarTaskParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetCalendarTaskParameters : AbstractParameters
    {
        [DataMember(Name = "CalendarId", Order = 0)]
        public int CalendarId { get; set; } = -1;

        [DataMember(Name = "CalendarTaskId", Order = 0)]
        public int CalendarTaskId { get; set; } = -1;
    }

    [DataContract(Name = "GetCalendarTaskResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetCalendarTaskResult
    {
        [DataMember(Name = "CalendarTask", Order = 0)]
        public CalendarTask CalendarTask { get; set; }

    }

    [DataContract(Name = "CreateOrUpdateCalendarAppointmentParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class CreateOrUpdateCalendarAppointmentParameters : AbstractParameters
    {
        [DataMember(Name = "CalendarId", Order = 0)]
        public int CalendarId { get; set; } = -1;

        [DataMember(Name = "CalendarAppointment", Order = 0)]
        public CalendarAppointment CalendarAppointment { get; set; }
    }

    [DataContract(Name = "CreateOrUpdateCalendarAppointmentResult", Namespace = "com.nordic-it.appservice.v3")]
    public class CreateOrUpdateCalendarAppointmentResult
    {
        [DataMember(Name = "Updated", Order = 0)]
        public bool Updated { get; set; }

        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }
    }

    [DataContract(Name = "CreateOrUpdateCalendarTaskParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class CreateOrUpdateCalendarTaskParameters : AbstractParameters
    {
        [DataMember(Name = "CalendarId", Order = 0)]
        public int CalendarId { get; set; } = -1;

        [DataMember(Name = "CalendarTask", Order = 0)]
        public CalendarTask CalendarTask { get; set; }
    }

    [DataContract(Name = "CreateOrUpdateCalendarTaskResult", Namespace = "com.nordic-it.appservice.v3")]
    public class CreateOrUpdateCalendarTaskResult
    {
        [DataMember(Name = "Updated", Order = 0)]
        public bool Updated { get; set; }

        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }
    }

    [DataContract(Name = "Calendar", Namespace = "com.nordic-it.appservice.v3")]
    public class Calendar
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; }

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "ColorHex", Order = 0)]
        public string ColorHex { get; set; }

        [DataMember(Name = "Shared", Order = 0)]
        public bool Shared { get; set; }
    }

    [DataContract(Name = "CalendarAppointment", Namespace = "com.nordic-it.appservice.v3")]
    public class CalendarAppointment
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Subject", Order = 0)]
        public string Subject { get; set; }

        [DataMember(Name = "Location", Order = 0)]
        public string Location { get; set; }

        [DataMember(Name = "Occurrences", Order = 0)]
        public List<CalendarAppointmentOccurrence> Occurrences { get; set; } = new List<CalendarAppointmentOccurrence>();

        [DataMember(Name = "AllDay", Order = 0)]
        public bool AllDay { get; set; }

        [DataMember(Name = "Private", Order = 0)]
        public bool Private { get; set; }

        [DataMember(Name = "Status", Order = 0)]
        public AppointmentStatus Status { get; set; }

        [DataMember(Name = "CreatorId", Order = 0)]
        public int CreatorId { get; set; } = -1;

        [DataMember(Name = "Creator", Order = 0)]
        public string Creator { get; set; }

        [DataMember(Name = "Priority", Order = 0)]
        public Priority Priority { get; set; }

        [DataMember(Name = "Type", Order = 0)]
        public CalendarOccurenceType Type { get; set; }

        [DataMember(Name = "CalendarId", Order = 0)]
        public int CalendarId { get; set; }

        [DataMember(Name = "ReminderAlertTime", Order = 0)]
        public DateTime ReminderAlertTime { get; set; }

        [DataMember(Name = "ReminderTimeBefore", Order = 0)]
        public long ReminderTimeBefore { get; set; } = -1;

        [DataMember(Name = "Participants", Order = 0)]
        public List<Participant> Participants { get; set; } = new List<Participant>();

        [DataMember(Name = "RecurrenceInfo", Order = 0)]
        public RecurrenceInfo RecurrenceInfo { get; set; }
    }

    [DataContract(Name = "CalendarAppointmentOccurrence", Namespace = "com.nordic-it.appservice.v3")]
    public class CalendarAppointmentOccurrence
    {
        [DataMember(Name = "StartDate", Order = 0)]
        public DateTime StartDate { get; set; }

        [DataMember(Name = "EndDate", Order = 0)]
        public DateTime EndDate { get; set; }

        [DataMember(Name = "RecurrenceIndex", Order = 0)]
        public int RecurrenceIndex { get; set; } = -1;
    }

    [DataContract(Name = "CalendarTask", Namespace = "com.nordic-it.appservice.v3")]
    public class CalendarTask
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Subject", Order = 0)]
        public string Subject { get; set; }

        [DataMember(Name = "StartDate", Order = 0)]
        public DateTime StartDate { get; set; }

        [DataMember(Name = "EndDate", Order = 0)]
        public DateTime EndDate { get; set; }

        [DataMember(Name = "Private", Order = 0)]
        public bool Private { get; set; }

        [DataMember(Name = "Status", Order = 0)]
        public TaskStatus Status { get; set; }

        [DataMember(Name = "CreatorId", Order = 0)]
        public int CreatorId { get; set; } = -1;

        [DataMember(Name = "Creator", Order = 0)]
        public string Creator { get; set; }

        [DataMember(Name = "Priority", Order = 0)]
        public Priority Priority { get; set; }

        [DataMember(Name = "Type", Order = 0)]
        public CalendarOccurenceType Type { get; set; }

        [DataMember(Name = "CalendarId", Order = 0)]
        public int CalendarId { get; set; }

        [DataMember(Name = "ReminderAlertTime", Order = 0)]
        public DateTime ReminderAlertTime { get; set; }

        [DataMember(Name = "ReminderTimeBefore", Order = 0)]
        public long ReminderTimeBefore { get; set; } = -1;

        [DataMember(Name = "PercentComplete", Order = 0)]
        public int PercentComplete { get; set; }

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }

        [DataMember(Name = "ObjectId", Order = 0)]
        public int ObjectId { get; set; } = -1;

        [DataMember(Name = "DelegatorId", Order = 0)]
        public int DelegatorId { get; set; } = -1;

        [DataMember(Name = "Delegator", Order = 0)]
        public string Delegator { get; set; }

        [DataMember(Name = "UserIds", Order = 0)]
        public List<int> UserIds { get; set; } = new List<int>();

        [DataMember(Name = "Users", Order = 0)]
        public Dictionary<int, string> Users { get; set; } = new Dictionary<int, string>();

        [DataMember(Name = "DepartmentIds", Order = 0)]
        public List<int> DepartmentIds { get; set; } = new List<int>();

        [DataMember(Name = "Departments", Order = 0)]
        public Dictionary<int, string> Departments { get; set; } = new Dictionary<int, string>();

        [DataMember(Name = "DelegationStatus", Order = 0)]
        public DelegationStatus DelegationStatus { get; set; }
    }

    [DataContract(Name = "RecurrenceInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class RecurrenceInfo
    {
        [DataMember(Name = "AllDay", Order = 0)]
        public bool AllDay { get; set; }

        [DataMember(Name = "DayNumber", Order = 0)]
        public int DayNumber { get; set; } = -1;

        [DataMember(Name = "Duration", Order = 0)]
        public TimeSpan Duration { get; set; }

        [DataMember(Name = "End", Order = 0)]
        public DateTime End { get; set; }

        [DataMember(Name = "FirstDayOfWeek", Order = 0)]
        public DayOfWeek FirstDayOfWeek { get; set; }

        [DataMember(Name = "Month", Order = 0)]
        public int Month { get; set; } = -1;

        [DataMember(Name = "OccurrenceCount", Order = 0)]
        public int OccurrenceCount { get; set; } = -1;

        [DataMember(Name = "Periodicity", Order = 0)]
        public int Periodicity { get; set; } = -1;

        [DataMember(Name = "Range", Order = 0)]
        public RecurrenceRange Range { get; set; }

        [DataMember(Name = "Start", Order = 0)]
        public DateTime Start { get; set; }

        [DataMember(Name = "Type", Order = 0)]
        public RecurrenceType Type { get; set; }

        [DataMember(Name = "WeekDays", Order = 0)]
        public WeekDays WeekDays { get; set; }

        [DataMember(Name = "WeekOfMonth", Order = 0)]
        public WeekOfMonth WeekOfMonth { get; set; }
    }


    [DataContract(Name = "Participant", Namespace = "com.nordic-it.appservice.v3")]
    public class Participant
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Presence", Order = 0)]
        public ParticipantPresenence Presence { get; set; }

        [DataMember(Name = "Type", Order = 0)]
        public ParticipantType Type { get; set; }

        [DataMember(Name = "Status", Order = 0)]
        public ParticipantStatus Status { get; set; }

        [DataMember(Name = "CN", Order = 0)]
        public string CN { get; set; }

        [DataMember(Name = "Email", Order = 0)]
        public string Email { get; set; }

        [DataMember(Name = "Customer", Order = 0)]
        public bool Customer { get; set; }

        [DataMember(Name = "Note", Order = 0)]
        public string Note { get; set; }
    }

    [DataContract(Name = "WeekOfMonth", Namespace = "com.nordic-it.appservice.v3")]
    public enum WeekOfMonth
    {
        [EnumMember(Value = "First")]
        First = 0,

        [EnumMember(Value = "Second")]
        Second = 1,

        [EnumMember(Value = "Third")]
        Third = 2,

        [EnumMember(Value = "Fourth")]
        Fourth = 3,

        [EnumMember(Value = "Last")]
        Last = 4,

        [EnumMember(Value = "None")]
        None = 5
    }


    [DataContract(Name = "RecurrenceType", Namespace = "com.nordic-it.appservice.v3")]
    public enum RecurrenceType
    {
        [EnumMember(Value = "Daily")]
        Daily = 0,

        [EnumMember(Value = "Hourly")]
        Hourly = 1,

        [EnumMember(Value = "Minutely")]
        Minutely = 2,

        [EnumMember(Value = "Monthly")]
        Monthly = 3,

        [EnumMember(Value = "Weekly")]
        Weekly = 4,

        [EnumMember(Value = "Yearly")]
        Yearly = 5
    }

    [DataContract(Name = "RecurrenceRange", Namespace = "com.nordic-it.appservice.v3")]
    public enum RecurrenceRange
    {
        [EnumMember(Value = "EndByDate")]
        EndByDate = 0,

        [EnumMember(Value = "NoEndDate")]
        NoEndDate = 1,

        [EnumMember(Value = "OccurrenceCount")]
        OccurrenceCount = 2
    }

    [Flags]
    [DataContract(Name = "WeekDays", Namespace = "com.nordic-it.appservice.v3")]
    public enum WeekDays
    {
        [EnumMember(Value = "Sunday")]
        Sunday = 1,

        [EnumMember(Value = "Monday")]
        Monday = 2,

        [EnumMember(Value = "Tuesday")]
        Tuesday = 4,

        [EnumMember(Value = "Wednesday")]
        Wednesday = 8,

        [EnumMember(Value = "Thursday")]
        Thursday = 16,

        [EnumMember(Value = "Friday")]
        Friday = 32,

        [EnumMember(Value = "Saturday")]
        Saturday = 64,

        [EnumMember(Value = "WeekendDays")]
        WeekendDays = Sunday | Saturday,

        [EnumMember(Value = "WorkDays")]
        WorkDays = Monday | Tuesday | Wednesday | Thursday | Friday,

        [EnumMember(Value = "EveryDay")]
        EveryDay = WeekendDays | WorkDays
    }


    [DataContract(Name = "AppointmentStatus", Namespace = "com.nordic-it.appservice.v3")]
    public enum AppointmentStatus
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Free")] Free = 1,
        [EnumMember(Value = "Tentative")] Tentative = 2,
        [EnumMember(Value = "Busy")] Busy = 3,
        [EnumMember(Value = "OutOfOffice")] OutOfOffice = 4,
        [EnumMember(Value = "WorkingElsewhere")] WorkingElsewhere = 5,
        [EnumMember(Value = "Custom")] Custom = 6,
    }

    [DataContract(Name = "CalendarOccurenceType", Namespace = "com.nordic-it.appservice.v3")]
    public enum CalendarOccurenceType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Normal")] Normal = 1,
        [EnumMember(Value = "Pattern")] Pattern = 2,
        [EnumMember(Value = "Occurrence")] Occurrence = 3,
        [EnumMember(Value = "ChangedOccurrence")] ChangedOccurrence = 4,
        [EnumMember(Value = "DeletedOccurrence")] DeletedOccurrence = 5
    }

    [DataContract(Name = "ParticipantPresenence", Namespace = "com.nordic-it.appservice.v3")]
    public enum ParticipantPresenence
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Mandatory")] Mandatory = 1,
        [EnumMember(Value = "Optional")] Optional = 2,
    }

    [DataContract(Name = "ParticipantType", Namespace = "com.nordic-it.appservice.v3")]
    public enum ParticipantType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "User")] User = 1,
        [EnumMember(Value = "Client")] Client = 2,
        [EnumMember(Value = "Other")] Other = 3,
        [EnumMember(Value = "ComAddress")] ComAddress = 4,
    }

    [DataContract(Name = "ParticipantStatus", Namespace = "com.nordic-it.appservice.v3")]
    public enum ParticipantStatus
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "NeedAction")] NeedAction = 1,
        [EnumMember(Value = "Accepted")] Accepted = 2,
        [EnumMember(Value = "Declined")] Declined = 3,
        [EnumMember(Value = "Tentative")] Tentative = 4,
    }

    [DataContract(Name = "TaskStatus", Namespace = "com.nordic-it.appservice.v3")]
    public enum TaskStatus
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "NotStarted")] NotStarted = 1,
        [EnumMember(Value = "Active")] Active = 2,
        [EnumMember(Value = "Completed")] Completed = 3,
        [EnumMember(Value = "Waiting")] Waiting = 4,
        [EnumMember(Value = "Postponed")] Postponed = 5,
    }

    [DataContract(Name = "DelegationStatus", Namespace = "com.nordic-it.appservice.v3")]
    public enum DelegationStatus
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "NoDelegation")] NoDelegation = 1,
        [EnumMember(Value = "Unknown")] Unknown = 2,
        [EnumMember(Value = "Accepted")] Accepted = 3,
        [EnumMember(Value = "Declined")] Declined = 4,
    }

    #endregion

    #region Search

    [DataContract(Name = "GetSavedSearchesParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetSavedSearchesParameters : AbstractParameters
    {
    }

    [DataContract(Name = "GetSavedSearchesResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetSavedSearchesResult
    {
        [DataMember(Name = "SavedSearches", Order = 0)]
        public List<SavedSearch> SavedSearches { get; set; } = new List<SavedSearch>();
    }

    [DataContract(Name = "SearchDocumentsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchDocumentsParameters : AbstractParameters
    {
        [DataMember(Name = "SavedSearchFilterHash", Order = 0)]
        public string SavedSearchFilterHash { get; set; }

        [DataMember(Name = "MaxToFetch", Order = 0)]
        public int MaxToFetch { get; set; } = -1;

        [DataMember(Name = "SubjectMessageField", Order = 0)]
        public string SubjectMessageField { get; set; }

        [DataMember(Name = "SubjectMessageClause", Order = 0)]
        public SubjectMessageClause SubjectMessageClause { get; set; }

        [DataMember(Name = "FromToField", Order = 0)]
        public string FromToField { get; set; }

        [DataMember(Name = "FromToClause", Order = 0)]
        public FromToClause FromToClause { get; set; }

        [DataMember(Name = "SearchInAttachments", Order = 0)]
        public bool SearchInAttachments { get; set; }

        [DataMember(Name = "Unread", Order = 0)]
        public bool Unread { get; set; }

        [DataMember(Name = "PartialWordSearch", Order = 0)]
        public bool PartialWordSearch { get; set; }

        [DataMember(Name = "Processed", Order = 0)]
        public bool? Processed { get; set; }

        [DataMember(Name = "Reference", Order = 0)]
        public string Reference { get; set; }

        [DataMember(Name = "Priorities", Order = 0)]
        public List<Priority> Priorities { get; set; } = new List<Priority>();

        [DataMember(Name = "Directions", Order = 0)]
        public List<DocumentDirection> Directions { get; set; } = new List<DocumentDirection>();

        [DataMember(Name = "CategoryIds", Order = 0)]
        public List<int> CategoryIds { get; set; } = new List<int>();

        [DataMember(Name = "MustHaveCategoryIds", Order = 0)]
        public List<int> MustHaveCategoryIds { get; set; } = new List<int>();

        [DataMember(Name = "Lines", Order = 0)]
        public List<Guid> LineGuids { get; set; } = new List<Guid>();

        [DataMember(Name = "CreatorGuids", Order = 0)]
        public List<Guid> CreatorGuids { get; set; } = new List<Guid>();

        [DataMember(Name = "DateRange", Order = 0)]
        public DateRange DateRange { get; set; } = new DateRange();

        [DataMember(Name = "Comment", Order = 0)]
        public string Comment { get; set; }

        [DataMember(Name = "AttachmentName", Order = 0)]
        public string AttachmentName { get; set; }

        [DataMember(Name = "HavingAttachmentsOnly", Order = 0)]
        public bool HavingAttachmentsOnly { get; set; }

        [DataMember(Name = "FiledInFolderType", Order = 0)]
        public FiledInFolderType FiledInFolderType { get; set; }

        [DataMember(Name = "FiledInFolderFolderType", Order = 0)]
        public FiledInFolderFolderType FiledInFolderFolderType { get; set; }

        [DataMember(Name = "FiledInFolderIds", Order = 0)]
        public List<int> FiledInFolderIds { get; set; } = new List<int>();

        [DataMember(Name = "ExtraFields", Order = 0)]
        public string ExtraFields { get; set; }
    }

    [DataContract(Name = "SearchDocumentsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchDocumentsResult
    {
        [DataMember(Name = "SearchResults", Order = 0)]
        public List<DocumentPreview> SearchResults { get; set; } = new List<DocumentPreview>();
    }

    [DataContract(Name = "SearchContactsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchContactsParameters : AbstractParameters
    {
        [DataMember(Name = "SavedSearchFilterHash", Order = 0)]
        public string SavedSearchFilterHash { get; set; }

        [DataMember(Name = "MaxToFetch", Order = 0)]
        public int MaxToFetch { get; set; } = -1;

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "FirstName", Order = 0)]
        public string FirstName { get; set; }

        [DataMember(Name = "LastName", Order = 0)]
        public string LastName { get; set; }

        [DataMember(Name = "ShortId", Order = 0)]
        public string ShortId { get; set; }

        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }

        [DataMember(Name = "ContactType", Order = 0)]
        public HashSet<ContactType> ContactTypes { get; set; } = new HashSet<ContactType>();

        [DataMember(Name = "ComAddress", Order = 0)]
        public string ComAddress { get; set; }

        [DataMember(Name = "PostAddress", Order = 0)]
        public string PostAddress { get; set; }

        [DataMember(Name = "Comment", Order = 0)]
        public string Comment { get; set; }

        [DataMember(Name = "Vat", Order = 0)]
        public string Vat { get; set; }

        [DataMember(Name = "Ledger", Order = 0)]
        public string Ledger { get; set; }

        [DataMember(Name = "CategoriesIds", Order = 0)]
        public List<int> CategoriesIds { get; set; } = new List<int>();

        [DataMember(Name = "MustHaveCategoriesIds", Order = 0)]
        public List<int> MustHaveCategoriesIds { get; set; } = new List<int>();

        [DataMember(Name = "FiledInFolderType", Order = 0)]
        public FiledInFolderType FiledInFolderType { get; set; }

        [DataMember(Name = "FiledInFolderFolderType", Order = 0)]
        public FiledInFolderFolderType FiledInFolderFolderType { get; set; }

        [DataMember(Name = "FiledInFolderIds", Order = 0)]
        public List<int> FiledInFolderIds { get; set; } = new List<int>();

        [DataMember(Name = "CountryPrefix", Order = 0)]
        public int CountryPrefix { get; set; } = -1;

        [DataMember(Name = "ResponsibleIds", Order = 0)]
        public List<int> ResponsibleIds { get; set; } = new List<int>();
    }

    [DataContract(Name = "SearchContactsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchContactsResult
    {
        [DataMember(Name = "SearchResults", Order = 0)]
        public List<ContactPreview> SearchResults { get; set; } = new List<ContactPreview>();
    }

    [DataContract(Name = "SearchShortcodesParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchShortcodesParameters : AbstractParameters
    {
        [DataMember(Name = "SavedSearchFilterHash", Order = 0)]
        public string SavedSearchFilterHash { get; set; }

        [DataMember(Name = "MaxToFetch", Order = 0)]
        public int MaxToFetch { get; set; } = -1;

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }

        [DataMember(Name = "Address", Order = 0)]
        public string Address { get; set; }

        [DataMember(Name = "FiledInFolderType", Order = 0)]
        public FiledInFolderType FiledInFolderType { get; set; }

        [DataMember(Name = "FiledInFolderFolderType", Order = 0)]
        public FiledInFolderFolderType FiledInFolderFolderType { get; set; }

        [DataMember(Name = "FiledInFolderIds", Order = 0)]
        public List<int> FiledInFolderIds { get; set; } = new List<int>();
    }

    [DataContract(Name = "SearchShortcodesResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchShortcodesResult
    {
        [DataMember(Name = "ShortcodePreviews", Order = 0)]
        public List<ShortcodePreview> ShortcodePreviews { get; set; } = new List<ShortcodePreview>();
    }

    [DataContract(Name = "SearchCalendarEventsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchCalendarEventsParameters : AbstractParameters
    {
        [DataMember(Name = "Type", Order = 0)]
        public SearchCalendarEventsType Type { get; set; }

        [DataMember(Name = "SavedSearchFilterHash", Order = 0)]
        public string SavedSearchFilterHash { get; set; }

        // Appointment, Task
        [DataMember(Name = "InCalendarOfUserIds", Order = 0)]
        public List<int> InCalendarOfUserIds { get; set; } = new List<int>();

        // Appointment, Task
        [DataMember(Name = "Priority", Order = 0)]
        public Priority Priority { get; set; }

        // Appointment, Task
        [DataMember(Name = "Subject", Order = 0)]
        public string Subject { get; set; }

        // Appointment, Task
        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }

        // Task
        [DataMember(Name = "InGroupCalendarOfUserIds", Order = 0)]
        public List<int> InGroupCalendarOfUserIds { get; set; } = new List<int>();

        // Task
        [DataMember(Name = "TaskCreatedByUserIds", Order = 0)]
        public List<int> TaskCreatedByUserIds { get; set; } = new List<int>();

        // Task
        [DataMember(Name = "DelegatedToUserIds", Order = 0)]
        public List<int> DelegatedToUserIds { get; set; } = new List<int>();

        // Task
        [DataMember(Name = "DelegatedToDepartmentIds", Order = 0)]
        public List<int> DelegatedToDepartmentIds { get; set; } = new List<int>();

        // Appointment
        [DataMember(Name = "CalendarCategoryIds", Order = 0)]
        public List<int> CalendarCategoryIds { get; set; } = new List<int>();

        // Appointment
        [DataMember(Name = "Location", Order = 0)]
        public string Location { get; set; }

        // Appointment
        [DataMember(Name = "ParticipantUserIds", Order = 0)]
        public List<int> ParticipantUserIds { get; set; } = new List<int>();

        // Appointment, Task
        [DataMember(Name = "DateRange", Order = 0)]
        public DateRange DateRange { get; set; } = new DateRange();

        // Appointment, Task
        [DataMember(Name = "FiledInFolderType", Order = 0)]
        public FiledInFolderType FiledInFolderType { get; set; }

        // Appointment, Task
        [DataMember(Name = "FiledInFolderFolderType", Order = 0)]
        public FiledInFolderFolderType FiledInFolderFolderType { get; set; }

        // Appointment, Task
        [DataMember(Name = "FiledInFolderIds", Order = 0)]
        public List<int> FiledInFolderIds { get; set; } = new List<int>();
    }

    [DataContract(Name = "SearchCalendarEventsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SearchCalendarEventsResult
    {
        [DataMember(Name = "CalendarAppointment", Order = 0)]
        public List<CalendarAppointment> CalendarAppointments { get; set; } = new List<CalendarAppointment>();

        [DataMember(Name = "CalendarTasks", Order = 0)]
        public List<CalendarTask> CalendarTasks { get; set; } = new List<CalendarTask>();
    }

    [DataContract(Name = "SavedSearch", Namespace = "com.nordic-it.appservice.v3")]
    public class SavedSearch
    {
        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }

        [DataMember(Name = "SavedSearchFilterHash", Order = 0)]
        public string SavedSearchFilterHash { get; set; }
    }

    [DataContract(Name = "SearchCalendarEventsType", Namespace = "com.nordic-it.appservice.v3")]
    public enum SearchCalendarEventsType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Appointments")] Appointments = 1,
        [EnumMember(Value = "Tasks")] Tasks = 2,
    }

    [DataContract(Name = "FiledInFolderType", Namespace = "com.nordic-it.appservice.v3")]
    public enum FiledInFolderType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Filed")] Filed = 1,
        [EnumMember(Value = "Unfiled")] Unfiled = 2,
    }

    [DataContract(Name = "FiledInFolderFolderType", Namespace = "com.nordic-it.appservice.v3")]
    public enum FiledInFolderFolderType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Any")] Any = 1,
        [EnumMember(Value = "Cabinet")] Cabinet = 2,
        [EnumMember(Value = "FilterView")] FilterView = 3,
        [EnumMember(Value = "Personal")] Personal = 4,
    }

    [DataContract(Name = "SubjectMessageClause", Namespace = "com.nordic-it.appservice.v3")]
    public enum SubjectMessageClause
    {
        [EnumMember(Value = "SubjectOrMessage")] SubjectOrMessage = 0,
        [EnumMember(Value = "SubjectOnly")] SubjectOnly = 1,
        [EnumMember(Value = "MessageOnly")] MessageOnly = 2,
    }

    [DataContract(Name = "FromToClause", Namespace = "com.nordic-it.appservice.v3")]
    public enum FromToClause
    {
        [EnumMember(Value = "FromOrTo")] FromOrTo = 0,
        [EnumMember(Value = "FromOnly")] FromOnly = 1,
        [EnumMember(Value = "ToOnly")] ToOnly = 2,
    }

    #endregion

    #region Notifications

    [DataContract(Name = "GetNotificationsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetNotificationsParameters : AbstractParameters
    {
        [DataMember(Name = "PushToken", Order = 0)]
        public string PushToken { get; set; }

        [DataMember(Name = "DeviceType", Order = 0)]
        public DeviceType DeviceType { get; set; }
    }

    [DataContract(Name = "GetNotificationsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetNotificationsResult
    {
        [DataMember(Name = "Notifications", Order = 0)] public List<Notification> Notifications = new List<Notification>();
    }

    [DataContract(Name = "SetFoldersNotificationsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SetFoldersNotificationsParameters : AbstractParameters
    {
        [DataMember(Name = "PushToken", Order = 0)]
        public string PushToken { get; set; }

        [DataMember(Name = "DeviceType", Order = 0)]
        public DeviceType DeviceType { get; set; }

        [DataMember(Name = "FolderIds", Order = 0)]
        public int[] FolderIds { get; set; }

        [DataMember(Name = "ModuleType", Order = 0)]
        public ModuleType ModuleType { get; set; }

        [DataMember(Name = "Enabled", Order = 0)]
        public bool Enabled { get; set; }
    }

    [DataContract(Name = "SetFoldersNotificationsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SetFoldersNotificationsResult
    {
    }

    [DataContract(Name = "GetFoldersNotificationsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetFoldersNotificationsParameters : AbstractParameters
    {
        [DataMember(Name = "PushToken", Order = 0)]
        public string PushToken { get; set; }

        [DataMember(Name = "DeviceType", Order = 0)]
        public DeviceType DeviceType { get; set; }
    }

    [DataContract(Name = "GetFoldersNotificationsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetFoldersNotificationsResult
    {
        [DataMember(Name = "DocumentFolders", Order = 0)] public List<Folder> DocumentFolders = new List<Folder>();

        [DataMember(Name = "ContactFolders", Order = 0)] public List<Folder> ContactFolders = new List<Folder>();

        [DataMember(Name = "ShortcodeFolders", Order = 0)] public List<Folder> ShortcodeFolders = new List<Folder>();
    }

    [DataContract(Name = "GetCalendarNotificationsEnabledParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetCalendarNotificationsEnabledParameters : AbstractParameters
    {
        [DataMember(Name = "PushToken", Order = 0)]
        public string PushToken { get; set; }

        [DataMember(Name = "DeviceType", Order = 0)]
        public DeviceType DeviceType { get; set; }
    }

    [DataContract(Name = "GetCalendarNotificationsEnabledResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetCalendarNotificationsEnabledResult
    {
        [DataMember(Name = "Enabled", Order = 0)]
        public bool Enabled { get; set; }
    }

    [DataContract(Name = "SetCalendarNotificationsEnabledParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SetCalendarNotificationsEnabledParameters : AbstractParameters
    {
        [DataMember(Name = "PushToken", Order = 0)]
        public string PushToken { get; set; }

        [DataMember(Name = "DeviceType", Order = 0)]
        public DeviceType DeviceType { get; set; }

        [DataMember(Name = "Enabled", Order = 0)]
        public bool Enabled { get; set; }
    }

    [DataContract(Name = "SetCalendarNotificationsEnabledResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SetCalendarNotificationsEnabledResult
    {
    }

    [DataContract(Name = "GetNotificationsSoundParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetNotificationsSoundParameters : AbstractParameters
    {
        [DataMember(Name = "PushToken", Order = 0)]
        public string PushToken { get; set; }

        [DataMember(Name = "DeviceType", Order = 0)]
        public DeviceType DeviceType { get; set; }
    }

    [DataContract(Name = "GetNotificationsSoundResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetNotificationsSoundResult
    {
        [DataMember(Name = "SoundName", Order = 0)]
        public string SoundName { get; set; }
    }

    [DataContract(Name = "SetNotificationsSoundParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SetNotificationsSoundParameters : AbstractParameters
    {
        [DataMember(Name = "PushToken", Order = 0)]
        public string PushToken { get; set; }

        [DataMember(Name = "DeviceType", Order = 0)]
        public DeviceType DeviceType { get; set; }

        [DataMember(Name = "SoundName", Order = 0)]
        public string SoundName { get; set; }
    }

    [DataContract(Name = "SetNotificationsSoundResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SetNotificationsSoundResult
    {
    }

    [DataContract(Name = "ClearAllNotificationsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class ClearAllNotificationsParameters : AbstractParameters
    {
        [DataMember(Name = "PushToken", Order = 0)]
        public string PushToken { get; set; }

        [DataMember(Name = "DeviceType", Order = 0)]
        public DeviceType DeviceType { get; set; }
    }

    [DataContract(Name = "ClearAllNotificationsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class ClearAllNotificationsResult
    {
    }

    [DataContract(Name = "Notification", Namespace = "com.nordic-it.appservice.v3")]
    public class Notification
    {
        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Title", Order = 0)]
        public string Title { get; set; } = string.Empty;

        [DataMember(Name = "Message", Order = 0)]
        public string Message { get; set; } = string.Empty;

        [DataMember(Name = "Type", Order = 0)]
        public EventType Type { get; set; }

        [DataMember(Name = "DateTime", Order = 0)]
        public DateTime DateTime { get; set; }

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }

        [DataMember(Name = "ObjectId", Order = 0)]
        public int ObjectId { get; set; } = -1;

        [DataMember(Name = "FolderId", Order = 0)]
        public int FolderId { get; set; } = -1;

        [DataMember(Name = "RemindOn", Order = 0)]
        public DateTime RemindOn { get; set; }

        [DataMember(Name = "IsSilent", Order = 0)]
        public bool IsSilent { get; set; }
    }

    [DataContract(Name = "EventType", Namespace = "com.nordic-it.appservice.v3")]
    public enum EventType
    {
        [EnumMember(Value = "Unknown")] None = 0,
        [EnumMember(Value = "NewObjectCreated")] NewObjectCreated = 1,
        [EnumMember(Value = "NewObjectInFolder")] NewObjectInFolder = 2,
        [EnumMember(Value = "NewObjectInWorktray")] NewObjectInWorktray = 3,
        [EnumMember(Value = "CreateOrUpdateReminder")] CreateOrUpdateReminder = 4,
        [EnumMember(Value = "DeleteReminderIfExists")] DeleteReminderIfExists = 5,
        [EnumMember(Value = "Invited")] Invited = 6,
    }

    [DataContract(Name = "DeviceType", Namespace = "com.nordic-it.appservice.v3")]
    public enum DeviceType
    {
        [EnumMember(Value = "Unknown")] Unknown = 0,
        [EnumMember(Value = "IOS")] IOS = 1,
        [EnumMember(Value = "Android")] Android = 2,
        [EnumMember(Value = "UWP")] UWP = 3,
    }

    #endregion

    #region Common

    [DataContract(Name = "AddCommentParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class AddCommentParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectId", Order = 0)]
        public int ObjectId { get; set; } = -1;

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }

        [DataMember(Name = "Content", Order = 0)]
        public string Content { get; set; }
    }

    [DataContract(Name = "AddCommentResult", Namespace = "com.nordic-it.appservice.v3")]
    public class AddCommentResult
    {
        [DataMember(Name = "Comment", Order = 0)]
        public Comment Comment { get; set; }
    }

    [DataContract(Name = "EditCommentParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class EditCommentParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectId", Order = 0)]
        public int ObjectId { get; set; } = -1;

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }

        [DataMember(Name = "CommentId", Order = 0)]
        public int CommentId { get; set; } = -1;

        [DataMember(Name = "Content", Order = 0)]
        public string Content { get; set; }
    }

    [DataContract(Name = "EditCommentResult", Namespace = "com.nordic-it.appservice.v3")]
    public class EditCommentResult
    {
        [DataMember(Name = "EditSuccess", Order = 0)]
        public bool EditSuccess { get; set; }
    }

    [DataContract(Name = "DeleteCommentParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class DeleteCommentParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectId", Order = 0)]
        public int ObjectId { get; set; } = -1;

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }

        [DataMember(Name = "CommentId", Order = 0)]
        public int CommentId { get; set; } = -1;
    }

    [DataContract(Name = "DeleteCommentResult", Namespace = "com.nordic-it.appservice.v3")]
    public class DeleteCommentResult
    {
    }

    [DataContract(Name = "GetAllCategoriesParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetAllCategoriesParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }
    }

    [DataContract(Name = "GetAllCategoriesResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetAllCategoriesResult
    {
        [DataMember(Name = "Categories", Order = 0)]
        public List<Category> Categories { get; set; } = new List<Category>();
    }

    [DataContract(Name = "SetCategoriesParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class SetCategoriesParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectId", Order = 0)]
        public int ObjectId { get; set; } = -1;

        [DataMember(Name = "CategoryIds", Order = 0)]
        public int[] CategoryIds { get; set; }

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }
    }

    [DataContract(Name = "SetCategoriesResult", Namespace = "com.nordic-it.appservice.v3")]
    public class SetCategoriesResult
    {
    }

    [DataContract(Name = "GetObjectActionsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetObjectActionsParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectId", Order = 0)]
        public int ObjectId { get; set; } = -1;

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }
    }

    [DataContract(Name = "GetObjectActionsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetObjectActionsResult
    {
        [DataMember(Name = "ObjectActions", Order = 0)]
        public List<ObjectAction> ObjectActions { get; set; } = new List<ObjectAction>();
    }

    [DataContract(Name = "GetObjectLinksParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetObjectLinksParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectId", Order = 0)]
        public int ObjectId { get; set; } = -1;

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }
    }

    [DataContract(Name = "GetObjectLinksResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetObjectLinksResult
    {
        [DataMember(Name = "ObjectLinks", Order = 0)]
        public List<ObjectLink> ObjectLinks { get; set; } = new List<ObjectLink>();
    }

    [DataContract(Name = "GetRecentAddressesParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetRecentAddressesParameters : AbstractParameters
    {
    }

    [DataContract(Name = "GetRecentAddressesResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetRecentAddressesResult
    {
        [DataMember(Name = "RecentAddresses", Order = 0)]
        public List<RecentAddress> RecentAddresses { get; set; } = new List<RecentAddress>();
    }

    [DataContract(Name = "FileToFolderParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class FileToFolderParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectIds", Order = 0)]
        public int[] ObjectIds { get; set; }

        [DataMember(Name = "ToFolderId", Order = 0)]
        public int ToFolderId { get; set; } = -1;

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }

        [DataMember(Name = "Move", Order = 0)]
        public bool Move { get; set; }

        [DataMember(Name = "FromFolderId", Order = 0)]
        public int FromFolderId { get; set; } = -1;
    }

    [DataContract(Name = "FileToFolderResult", Namespace = "com.nordic-it.appservice.v3")]
    public class FileToFolderResult
    {
    }

    [DataContract(Name = "CopyToWorktrayParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class CopyToWorktrayParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectIds", Order = 0)]
        public int[] ObjectIds { get; set; }

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }

        [DataMember(Name = "UserIds", Order = 0)]
        public int[] UserIds { get; set; }

        [DataMember(Name = "Comment", Order = 0)]
        public string Comment { get; set; }
    }

    [DataContract(Name = "CopyToWorktrayResult", Namespace = "com.nordic-it.appservice.v3")]
    public class CopyToWorktrayResult
    {
    }

    [DataContract(Name = "DeleteParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class DeleteParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectIds", Order = 0)]
        public int[] ObjectIds { get; set; }

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }
    }

    [DataContract(Name = "DeleteResult", Namespace = "com.nordic-it.appservice.v3")]
    public class DeleteResult
    {
    }

    [DataContract(Name = "RemoveFromFolderParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class RemoveFromFolderParameters : AbstractParameters
    {
        [DataMember(Name = "ObjectIds", Order = 0)]
        public int[] ObjectIds { get; set; }

        [DataMember(Name = "FolderId", Order = 0)]
        public int FolderId { get; set; } = -1;

        [DataMember(Name = "ObjectType", Order = 0)]
        public ObjectType ObjectType { get; set; }
    }

    [DataContract(Name = "RemoveFromFolderResult", Namespace = "com.nordic-it.appservice.v3")]
    public class RemoveFromFolderResult
    {
    }

    [DataContract(Name = "GetSystemSettingsParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetSystemSettingsParameters : AbstractParameters
    {
    }

    [DataContract(Name = "GetSystemSettingsResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetSystemSettingsResult
    {
        [DataMember(Name = "SystemInfo", Order = 0)]
        public SystemInfo SystemInfo { get; set; } = new SystemInfo();

        [DataMember(Name = "DocumentsModuleInfo", Order = 0)]
        public DocumentsModuleInfo DocumentsModuleInfo { get; set; } = new DocumentsModuleInfo();

        [DataMember(Name = "ContactsModuleInfo", Order = 0)]
        public ContactsModuleInfo ContactsModuleInfo { get; set; } = new ContactsModuleInfo();

        [DataMember(Name = "ShortcodesModuleInfo", Order = 0)]
        public ShortcodesModuleInfo ShortcodesModuleInfo { get; set; } = new ShortcodesModuleInfo();

        [DataMember(Name = "CalendarModuleInfo", Order = 0)]
        public CalendarModuleInfo CalendarModuleInfo { get; set; } = new CalendarModuleInfo();

        [DataMember(Name = "UserInfo", Order = 0)]
        public UserInfo UserInfo { get; set; } = new UserInfo();
    }

    [DataContract(Name = "GetSystemUsersParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class GetSystemUsersParameters : AbstractParameters
    {
    }

    [DataContract(Name = "GetSystemUsersResult", Namespace = "com.nordic-it.appservice.v3")]
    public class GetSystemUsersResult
    {
        [DataMember(Name = "Departments", Order = 0)]
        public List<SystemDepartment> Departments { get; set; } = new List<SystemDepartment>();

        [DataMember(Name = "Users", Order = 0)]
        public List<SystemUser> Users { get; set; } = new List<SystemUser>();
    }

    [DataContract(Name = "RecentAddress", Namespace = "com.nordic-it.appservice.v3")]
    public class RecentAddress
    {
        [DataMember(Name = "AddressType", Order = 0)]
        public DocumentAddressType AddressType { get; set; }

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Address", Order = 0)]
        public string Address { get; set; }
    }

    [DataContract(Name = "SystemInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class SystemInfo
    {
        [DataMember(Name = "SystemVersion", Order = 0)]
        public string SystemVersion { get; set; }

        [DataMember(Name = "ServiceVersion", Order = 0)]
        public Version ServiceVersion { get; set; }

        [DataMember(Name = "AvailableModules", Order = 0)]
        public List<ModuleType> AvailableModules { get; set; } = new List<ModuleType>();

        [DataMember(Name = "ServerUtcOffset", Order = 0)]
        public TimeSpan ServerUtcOffset { get; set; }

        [DataMember(Name = "CustomerName", Order = 1)]
        public string CustomerName { get; set; }

        [DataMember(Name = "CustomerGuid", Order = 1)]
        public Guid CustomerGuid { get; set; }

        [DataMember(Name = "ServerTimeZoneInfoSerialized", Order = 1)]
        public string ServerTimeZoneInfoSerialized { get; set; }
    }

    [DataContract(Name = "DocumentsModuleInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class DocumentsModuleInfo
    {
        [DataMember(Name = "OnSendToSystemUser", Order = 0)]
        public OnSendToSystemUser OnSendToSystemUser { get; set; }

        [DataMember(Name = "DefaultOutgoingLine", Order = 0)]
        public Line DefaultOutgoingLine { get; set; }

        [DataMember(Name = "OutgoingLines", Order = 0)]
        public List<Line> OutgoingLines { get; set; } = new List<Line>();

        [DataMember(Name = "IsMissingAttachmentWarningEnabled", Order = 0)]
        public bool IsMissingAttachmentWarningEnabled { get; set; }

        [DataMember(Name = "ForwardAbbreviations", Order = 0)]
        public List<string> ForwardAbbreviations { get; set; } = new List<string>();

        [DataMember(Name = "ReplyAbbreviations", Order = 0)]
        public List<string> ReplyAbbreviations { get; set; } = new List<string>();

        [DataMember(Name = "AttachmentKeywords", Order = 0)]
        public List<string> AttachmentKeywords { get; set; } = new List<string>();

        [DataMember(Name = "MaximumAttachmentSizeBytes", Order = 0)]
        public long MaximumAttachmentSizeBytes { get; set; }

        [DataMember(Name = "ExtraFieldInfos", Order = 0)]
        public List<DocumentExtraFieldInfo> ExtraFieldInfos { get; set; } = new List<DocumentExtraFieldInfo>();

        [DataMember(Name = "AttachmentSearchEnabled", Order = 0)]
        public bool AttachmentSearchEnabled { get; set; }

        [DataMember(Name = "HandledFieldEnabled", Order = 0)]
        public bool HandledFieldEnabled { get; set; }

        [DataMember(Name = "Permissions", Order = 0)]
        public DocumentsModulePermissions Permissions { get; set; } = new DocumentsModulePermissions();

        [DataMember(Name = "WorktrayEnabled", Order = 1)]
        public bool? WorktrayEnabled { get; set; } = null;
    }

    [DataContract(Name = "ContactsModuleInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class ContactsModuleInfo
    {
        [DataMember(Name = "PhysicalAddressTypes", Order = 0)]
        public List<PhysicalAddressType> PhysicalAddressTypes { get; set; } = new List<PhysicalAddressType>();

        [DataMember(Name = "Countries", Order = 0)]
        public List<CountryInfo> Countries { get; set; } = new List<CountryInfo>();

        [DataMember(Name = "Permissions", Order = 0)]
        public Permissions Permissions { get; set; } = new Permissions();

        [DataMember(Name = "WorktrayEnabled", Order = 1)]
        public bool? WorktrayEnabled { get; set; } = null;
    }

    [DataContract(Name = "ShortcodesModuleInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class ShortcodesModuleInfo
    {
        [DataMember(Name = "Permissions", Order = 0)]
        public Permissions Permissions { get; set; } = new Permissions();

        [DataMember(Name = "WorktrayEnabled", Order = 1)]
        public bool? WorktrayEnabled { get; set; } = null;
    }

    [DataContract(Name = "CalendarModuleInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class CalendarModuleInfo
    {
        [DataMember(Name = "Calendars", Order = 0)]
        public List<Calendar> Calendars { get; set; } = new List<Calendar>();

        [DataMember(Name = "Permissions", Order = 0)]
        public Permissions Permissions { get; set; } = new Permissions();
    }

    [DataContract(Name = "UserInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class UserInfo
    {
        [DataMember(Name = "User", Order = 0)]
        public SystemUser User { get; set; } = new SystemUser();

        [DataMember(Name = "IsSystemAdministrator", Order = 0)]
        public bool IsSystemAdministrator { get; set; }
    }

    [DataContract(Name = "Permissions", Namespace = "com.nordic-it.appservice.v3")]
    public class Permissions
    {
        [DataMember(Name = "ManageCategories", Order = 0)]
        public bool ManageCategories { get; set; }

        [DataMember(Name = "CabinetSupervisor", Order = 0)]
        public bool CabinetSupervisor { get; set; }

        [DataMember(Name = "CreateFolderAllowed", Order = 0)]
        public bool CreateFolderAllowed { get; set; }

        [DataMember(Name = "EditFolderAllowed", Order = 0)]
        public bool EditFolderAllowed { get; set; }

        [DataMember(Name = "DeleteFolderAllowed", Order = 0)]
        public bool DeleteFolderAllowed { get; set; }

        [DataMember(Name = "RemoveFromFolderAllowed", Order = 0)]
        public bool RemoveFromFolderAllowed { get; set; }

        [DataMember(Name = "ManagePublicDynamicFolderAllowed", Order = 0)]
        public bool ManagePublicDynamicFolderAllowed { get; set; }

        [DataMember(Name = "MaxPublicPersonalFoldersAllowed", Order = 0)]
        public int MaxPublicPersonalFoldersAllowed { get; set; }

        [DataMember(Name = "CreateAllowed", Order = 0)]
        public bool CreateAllowed { get; set; }

        [DataMember(Name = "EditAllowed", Order = 0)]
        public bool EditAllowed { get; set; }

        [DataMember(Name = "DeleteAllowed", Order = 0)]
        public bool DeleteAllowed { get; set; }

        [DataMember(Name = "EditAccessRightsAllowed", Order = 0)]
        public bool EditAccessRightsAllowed { get; set; }
    }

    [DataContract(Name = "DocumentsModulePermissions", Namespace = "com.nordic-it.appservice.v3")]
    public class DocumentsModulePermissions : Permissions
    {
        [DataMember(Name = "IncomingSupervisor", Order = 0)]
        public bool IncomingSupervisor { get; set; }

        [DataMember(Name = "OutgoingSupervisor", Order = 0)]
        public bool OutgoingSupervisor { get; set; }

        [DataMember(Name = "ManageFilterViewFoldersAllowed", Order = 0)]
        public bool ManageFilterViewFoldersAllowed { get; set; }

        [DataMember(Name = "SpamManager", Order = 0)]
        public bool SpamManager { get; set; }
    }

    [DataContract(Name = "DocumentExtraFieldInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class DocumentExtraFieldInfo
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }
    }

    [DataContract(Name = "SystemDepartment", Namespace = "com.nordic-it.appservice.v3")]
    public class SystemDepartment
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "UserIds", Order = 0)]
        public List<int> UserIds { get; set; } = new List<int>();
    }

    [DataContract(Name = "SystemUser", Namespace = "com.nordic-it.appservice.v3")]
    public class SystemUser
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Username", Order = 0)]
        public string Username { get; set; }

        [DataMember(Name = "FirstName", Order = 0)]
        public string FirstName { get; set; }

        [DataMember(Name = "PatronymicName", Order = 0)]
        public string PatronymicName { get; set; }

        [DataMember(Name = "LastName", Order = 0)]
        public string LastName { get; set; }

        [DataMember(Name = "Avatar", Order = 0)]
        public byte[] Avatar { get; set; }
    }

    [DataContract(Name = "Category", Namespace = "com.nordic-it.appservice.v3")]
    public class Category
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }

        [DataMember(Name = "HexColor", Order = 0)]
        public string HexColor { get; set; }
    }

    [DataContract(Name = "ObjectAction", Namespace = "com.nordic-it.appservice.v3")]
    public class ObjectAction
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "ActionType", Order = 0)]
        public string ActionType { get; set; }

        [DataMember(Name = "ActionTypeGid", Order = 0)]
        public Guid ActionTypeGid { get; set; }

        [DataMember(Name = "ActionTypeId", Order = 0)]
        public int ActionTypeId { get; set; } = -1;

        [DataMember(Name = "UserId", Order = 0)]
        public int UserId { get; set; } = -1;

        [DataMember(Name = "Username", Order = 0)]
        public string Username { get; set; }

        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }

        [DataMember(Name = "ActionTime", Order = 0)]
        public DateTime ActionTime { get; set; }
    }

    [DataContract(Name = "ObjectLink", Namespace = "com.nordic-it.appservice.v3")]
    public class ObjectLink
    {
        [DataMember(Name = "FromObjectId", Order = 0)]
        public int FromObjectId { get; set; } = -1;

        [DataMember(Name = "FromObjectType", Order = 0)]
        public ObjectType FromObjectType { get; set; }

        [DataMember(Name = "ToObjectId", Order = 0)]
        public int ToObjectId { get; set; } = -1;

        [DataMember(Name = "ToObjectType", Order = 0)]
        public ObjectType ToObjectType { get; set; }

        [DataMember(Name = "IsReverse", Order = 0)]
        public bool IsReverse { get; set; }

        [DataMember(Name = "Description", Order = 0)]
        public string Description { get; set; }

        [DataMember(Name = "TypeInfo", Order = 0)]
        public ObjectLinkTypeInfo TypeInfo { get; set; } = new ObjectLinkTypeInfo();
    }

    [DataContract(Name = "ObjectLinkTypeInfo", Namespace = "com.nordic-it.appservice.v3")]
    public class ObjectLinkTypeInfo
    {
        [DataMember(Name = "Id", Order = 0)]
        public int Id { get; set; } = -1;

        [DataMember(Name = "Guid", Order = 0)]
        public Guid Guid { get; set; }

        [DataMember(Name = "FromType", Order = 0)]
        public ObjectType FromType { get; set; }

        [DataMember(Name = "ToType", Order = 0)]
        public ObjectType ToType { get; set; }

        [DataMember(Name = "DescriptionSimple", Order = 0)]
        public string DescriptionSimple { get; set; }

        [DataMember(Name = "DescriptionComplex", Order = 0)]
        public string DescriptionComplex { get; set; }

        [DataMember(Name = "DescriptionComplexReverse", Order = 0)]
        public string DescriptionComplexReverse { get; set; }

        [DataMember(Name = "DescriptionAction", Order = 0)]
        public string DescriptionAction { get; set; }

        [DataMember(Name = "DescriptionActionReverse", Order = 0)]
        public string DescriptionActionReverse { get; set; }
    }

    [DataContract(Name = "DateRange", Namespace = "com.nordic-it.appservice.v3")]
    public class DateRange
    {
        [DataMember(Name = "Start", Order = 0)]
        public DateTime Start { get; set; }

        [DataMember(Name = "End", Order = 0)]
        public DateTime End { get; set; }

        [DataMember(Name = "Enabled", Order = 0)]
        public bool Enabled { get; set; }
    }

    [Flags]
    [DataContract(Name = "OnSendToSystemUser", Namespace = "com.nordic-it.appservice.v3")]
    public enum OnSendToSystemUser
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "CopyToWorktray")] CopyToWorktray = 1,
        [EnumMember(Value = "SendEmail")] SendEmail = 2,
    }

    [DataContract(Name = "ObjectType", Namespace = "com.nordic-it.appservice.v3")]
    public enum ObjectType
    {
        [EnumMember(Value = "None")] None = 0,
        [EnumMember(Value = "Document")] Document = 1,
        [EnumMember(Value = "Contact")] Contact = 2,
        [EnumMember(Value = "Shortcode")] Shortcode = 3,
        [EnumMember(Value = "CalendarAppointment")] CalendarAppointment = 4,
        [EnumMember(Value = "CalendarTask")] CalendarTask = 5,
    }

    #endregion

    #region Diagnostic

    [DataContract(Name = "TestParameters", Namespace = "com.nordic-it.appservice.v3")]
    public class TestParameters : AbstractParameters
    {
    }

    [DataContract(Name = "TestResult", Namespace = "com.nordic-it.appservice.v3")]
    public class TestResult
    {
    }

    #endregion
}