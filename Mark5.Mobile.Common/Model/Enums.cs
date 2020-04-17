using System;

namespace Mark5.Mobile.Common.Model
{
    #region Managers

    public enum SourceType
    {
        Auto = 0,
        Remote = 1,
        Local = 2,
    }

    #endregion

    #region Folders

    public enum FolderType
    {
        None = 0,
        Inbox = 1,
        Outbox = 2,
        Draft = 3,
        Cabinet = 4,
        Spam = 5,
        External = 6,
        DeliveryReports = 7,
        Companies = 8,
        Persons = 9,
        Personal = 10,
    }

    public enum FolderInternalType
    {
        None = 0,
        Static = 1,
        Dynamic = 2,
        FilterView = 3,
        Worktray = 4,
        Custom = 5,
    }

    #endregion

    #region Documents

    public enum DocumentCreationModeFlag
    {
        None = 0,
        New = 1,
        Reply = 2,
        ReplyAll = 4,
        Forward = 8,
        Edit = 16,
        Resend = 32,
        Redirect = 64,
    }

    public enum Priority
    {
        None = 0,
        Ignore = 1,
        Low = 2,
        Normal = 3,
        Urgent = 4,
        System = 5,
    }

    public enum DocumentDirection
    {
        None = 0,
        Outgoing = 1,
        Incoming = 2,
        Draft = 3,
        External = 4
    }

    public enum DocumentAddressType
    {
        None = 0,
        To = 1,
        From = 2,
        Cc = 3,
        Bcc = 4,
        ReplyTo = 5
    }

    public enum ContentType
    {
        None = 0,
        PlainText = 1,
        Html = 2,
    }

    public enum DocumentBodyTypeRequest
    {
        None = 0,
        HtmlOnly = 1,
        PlainTextOnly = 2,
        HtmlAndPlainText = 3,
    }

    public enum TransmitStatus
    {
        None = 0,
        InTransmit = 1,
        Fail = 2,
        Sent = 3,
        InCancel = 4,
        PartialSent = 5,
        Delivered = 6,
        FailedBounced = 7,
        Delayed = 8,
        CheckedOut = 9,
        Locked = 10
    }

    public enum UseForFrom
    {
        LicenseName = 0,
        UserName = 1,
        UserLogin = 2,
        LineName = 3
    }

    #endregion

    #region Contacts

    public enum ContactCreationModeFlag
    {
        None = 0,
        New = 1,
        Edit = 2,
    }

    public enum CommunicationAddressType
    {
        None = 0,
        Email = 1,
        Fax = 2,
        Phone = 3,
        Telex = 4,
        Mobile = 5,
        IM = 6,
        Internal = 7,
        System = 8,
        Skype = 9,
    }

    public enum ContactType
    {
        None = 0,
        Person = 1,
        Department = 2,
        Company = 3,
    }

    #endregion

    #region Shortcodes

    public enum ShortcodeCreationModeFlag
    {
        None = 0,
        New = 1,
        Edit = 2,
    }

    #endregion

    #region Calendar

    public enum CalendarCategoryType
    {
        None = 0,
        Appointment = 1,
        Task = 2,
        Event = 3,
        PhoneCall = 4,
    }

    public enum CalendarCategorySubType
    {
        None = 0,
        Event = 1,
        Custom = 2,
        MeetingInternal = 3,
        MeetingExternal = 4,
        PhoneCallIn = 5,
        PhoneCallOut = 6,
        FollowUp = 7,
        Planning = 8,
        Visit = 9,
        Lunch = 10,
        Proposal = 11,
        Service = 12,
        Birthday = 13,
        Anniversary = 14,
        Private = 15,
        Holiday = 16,
        Vacation = 17,
    }

    public enum WeekOfMonth
    {
        First = 0,
        Second = 1,
        Third = 2,
        Fourth = 3,
        Last = 4,
        None = 5
    }

    public enum RecurrenceType
    {
        Daily = 0,
        Hourly = 1,
        Minutely = 2,
        Monthly = 3,
        Weekly = 4,
        Yearly = 5
    }

    public enum RecurrenceRange
    {
        EndByDate = 0,
        NoEndDate = 1,
        OccurrenceCount = 2
    }

    [Flags]
    public enum WeekDays
    {
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64,
        WeekendDays = Sunday | Saturday,
        WorkDays = Monday | Tuesday | Wednesday | Thursday | Friday,
        EveryDay = WeekendDays | WorkDays
    }

    public enum CalendarOccurenceType
    {
        None = 0,
        Normal = 1,
        Pattern = 2,
        Occurrence = 3,
        ChangedOccurrence = 4,
        DeletedOccurrence = 5
    }

    public enum ParticipantPresenence
    {
        None = 0,
        Mandatory = 1,
        Optional = 2,
    }

    public enum ParticipantType
    {
        None = 0,
        User = 1,
        Client = 2,
        Other = 3,
        ComAddress = 4,
    }

    public enum ParticipantStatus
    {
        NeedAction = 0,
        Accepted = 1,
        Declined = 2,
        Tentative = 3,
        Inviting = 4,
        Invited = 5
    }

    #endregion

    #region Search

    public enum SearchCalendarEventsType
    {
        None = 0,
        Appointments = 1,
        Tasks = 2,
    }

    public enum FiledInFolderType
    {
        None = 0,
        Filed = 1,
        Unfiled = 2,
    }

    public enum FiledInFolderFolderType
    {
        None = 0,
        Any = 1,
        Cabinet = 2,
        FilterView = 3,
        Personal = 4,
    }

    public enum SubjectMessageClause
    {
        SubjectOrMessage = 0,
        SubjectOnly = 1,
        MessageOnly = 2,
    }

    public enum FromToClause
    {
        FromOrTo = 0,
        FromOnly = 1,
        ToOnly = 2,
    }

    #endregion

    #region Suggestions

    public enum RecipientType
    {
        Unknown,
        RecentAddress,
        Phonebook,
        Contact,
        Internal
    }

    #endregion

    #region Notifications

    public enum EventType
    {
        None = 0,
        NewObjectCreated = 1,
        NewObjectInFolder = 2,
        NewObjectInWorktray = 3,
        CreateOrUpdateReminder = 4,
        DeleteReminderIfExists = 5,
        Invited = 6,
        TransmitFailed = 7
    }

    public enum DeviceType
    {
        Unknown = 0,
        IOS = 1,
        Android = 2,
        UWP = 3,
    }

    #endregion

    #region Common

    public enum OnSendToSystemUser
    {
        None = 0,
        CopyToWorktray = 1,
        SendEmail = 2,
    }

    public enum ObjectType
    {
        None = 0,
        Document = 1,
        Contact = 2,
        Shortcode = 3,
        CalendarAppointment = 4,
        CalendarTask = 5,
    }

    public enum ModuleType
    {
        None = 0,
        Documents = 1,
        Contacts = 2,
        Shortcodes = 3,
        Calendar = 4,
    }

    #endregion

    #region Other

    public enum SslMode
    {
        On,
        AllowSelfSigned,
        Off
    }

    #endregion

    #region ICalendar

    public enum MethodType
    {
        Request = 0,
        Reply = 1,
        Cancelled = 2,
        Publish = 3
    }

    #endregion
}