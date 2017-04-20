//
// Project: Mark5.ServiceReference.DataContract
// File: AppServiceExceptions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

#pragma warning disable CS1701
namespace Mark5.ServiceReference.Exceptions
{

    #region Local exceptions

    public class AppServiceException : Exception
    {

        public AppServiceFaultDetail Detail
        {
            get;
            private set;
        }

        public AppServiceException(Exception ex)
            : base(GetMessage(ex), ex)
        {
            Detail = (ex as FaultException<AppServiceFaultDetail>)?.Detail;
        }

        static string GetMessage(Exception ex)
        {
            var fe = ex as FaultException<AppServiceFaultDetail>;
            if (fe != null)
            {
                return $"{fe.Message} ({fe.Detail.Code}).";
            }

            var gfe = ex as FaultException;
            if (gfe != null)
            {
                return "Unknown service fault.";
            }

            var te = ex as TimeoutException;
            if (te != null)
            {
                return "Service operation timed out.";
            }

            var ce = ex as CommunicationException;
            if (ce != null)
            {
                return "There was a problem communicating with service.";
            }

            return "Unexpected exception of type: \"" + ex?.GetType()?.Name + "\", message: \"" + ex?.Message + "\" occured.";
        }

        public override string ToString()
        {
            if (Detail != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(base.ToString());
                sb.AppendLine("Detail.Code: " + Detail.Code);
                sb.AppendLine("Detail.DiagnosticInformation: " + Detail.DiagnosticInformation);
                return sb.ToString();
            }

            return base.ToString();
        }
    }

    #endregion

    #region Exception handling

    [DataContract(Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
    public class AppServiceFaultDetail
    {

        [DataMember(Name = "Code", Order = 0)]
        public AppServiceFaultCode Code { get; set; }

        [DataMember(Name = "DiagnosticInformation", Order = 0)]
        public string DiagnosticInformation { get; set; }
    }

    [DataContract(Name = "AppServiceFaultCode", Namespace = "com.nordic-it.appservice.v3")]
    public enum AppServiceFaultCode
    {

        [EnumMember(Value = "Unknown")]
        Unknown = 0,

        [EnumMember(Value = "InternalError")]
        InternalError = 500,
        [EnumMember(Value = "InvalidParameters")]
        InvalidParameters = 501,

        [EnumMember(Value = "AuthenticationError")]
        AuthenticationError = 1000,
        [EnumMember(Value = "AuthorisationError")]
        AuthorisationError = 1001,

        [EnumMember(Value = "GetFoldersError")]
        GetFoldersError = 1100,

        [EnumMember(Value = "GetDocumentsError")]
        GetDocumentsError = 1200,
        [EnumMember(Value = "GetDocumentError")]
        GetDocumentError = 1201,
        [EnumMember(Value = "SendDocumentError")]
        SendDocumentError = 1202,
        [EnumMember(Value = "SetDocumentsReadStatus")]
        SetDocumentsReadStatus = 1210,
        [EnumMember(Value = "SetDocumentsPriorityError")]
        SetDocumentsPriorityError = 1211,
        [EnumMember(Value = "MoveToSpamError")]
        MoveToSpamError = 1212,
        [EnumMember(Value = "GetTemplatesError")]
        GetTemplatesError = 1220,
        [EnumMember(Value = "GetTemplateError")]
        GetTemplateError = 1221,
        [EnumMember(Value = "GetDefaultTemplateError")]
        GetDefaultTemplateError = 1222,

        [EnumMember(Value = "GetContactsError")]
        GetContactsError = 1300,
        [EnumMember(Value = "GetContactError")]
        GetContactError = 1301,
        [EnumMember(Value = "CreateOrUpdateContactError")]
        CreateOrUpdateContactError = 1302,

        [EnumMember(Value = "GetShortcodesError")]
        GetShortcodesError = 1400,
        [EnumMember(Value = "GetShortcodeError")]
        GetShortcodeError = 1401,

        [EnumMember(Value = "GetCalendarEventsError")]
        GetCalendarEventsError = 1500,
        [EnumMember(Value = "GetCalendarAppointmentError")]
        GetCalendarAppointmentError = 1501,
        [EnumMember(Value = "GetCalendarTaskError")]
        GetCalendarTaskError = 1502,
        [EnumMember(Value = "CreateOrUpdateCalendarAppointmentError")]
        CreateOrUpdateCalendarAppointmentError = 1503,
        [EnumMember(Value = "CreateOrUpdateCalendarTaskError")]
        CreateOrUpdateCalendarTaskError = 1504,

        [EnumMember(Value = "GetSavedSearchesError")]
        GetSavedSearchesError = 1600,
        [EnumMember(Value = "SearchDocumentsError")]
        SearchDocumentsError = 1601,
        [EnumMember(Value = "SearchContactsError")]
        SearchContactsError = 1602,
        [EnumMember(Value = "SearchShortcodesError")]
        SearchShortcodesError = 1603,
        [EnumMember(Value = "SearchCalendarEventsError")]
        SearchCalendarEventsError = 1604,

        [EnumMember(Value = "GetNotificationsError")]
        GetNotificationsError = 1700,
        [EnumMember(Value = "SetFoldersNotificationsError")]
        SetFoldersNotificationsError = 1701,
        [EnumMember(Value = "GetFoldersNotificationsError")]
        GetFoldersNotificationsError = 1702,
        [EnumMember(Value = "GetCalendarNotificationsError")]
        GetCalendarNotificationsError = 1703,
        [EnumMember(Value = "SetCalendarNotificationsError")]
        SetCalendarNotificationsError = 1704,
        [EnumMember(Value = "GetNotificationsSoundError")]
        GetNotificationsSoundError = 1705,
        [EnumMember(Value = "SetNotificationsSoundError")]
        SetNotificationsSoundError = 1706,
        [EnumMember(Value = "ClearAllNotificationsError")]
        ClearAllNotificationsError = 1707,

        [EnumMember(Value = "AddCommentError")]
        AddCommentError = 1800,
        [EnumMember(Value = "EditCommentError")]
        EditCommentError = 1801,
        [EnumMember(Value = "DeleteCommentError")]
        DeleteCommentError = 1802,
        [EnumMember(Value = "GetAllCategories")]
        GetAllCategories = 1810,
        [EnumMember(Value = "SetCategoriesError")]
        SetCategoriesError = 1811,
        [EnumMember(Value = "GetObjectActions")]
        GetObjectActions = 1820,
        [EnumMember(Value = "GetObjectLinks")]
        GetObjectLinks = 1821,
        [EnumMember(Value = "GetRecentAddresses")]
        GetRecentAddresses = 1830,
        [EnumMember(Value = "FileToFolderError")]
        FileToFolderError = 1840,
        [EnumMember(Value = "CopyToWorktrayError")]
        CopyToWorktrayError = 1841,
        [EnumMember(Value = "DeleteError")]
        DeleteError = 1850,
        [EnumMember(Value = "RemoveFromFolderError")]
        RemoveFromFolderError = 1851,

        [EnumMember(Value = "GetSystemSettingsError")]
        GetSystemSettingsError = 1900,
        [EnumMember(Value = "GetSystemUsersError")]
        GetSystemUsersError = 1901,
    }

    #endregion

}

