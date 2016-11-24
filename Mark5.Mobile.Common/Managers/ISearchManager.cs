//
// Project: Mark5.Mobile.Common
// File: ISearchManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using System.Collections.Generic;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Managers
{

    public interface ISearchManager
    {

        DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; }

        Task<List<SavedSearch>> GetSavedSearches(SourceType sourceType = SourceType.Auto);

        Task<SearchDocumentsResult> SearchDocumentsAsync(SearchDocumentsCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<SearchContactsResult> SearchContactsAsync(SearchContactsCriteria critera, SourceType sourceType = SourceType.Auto);

        Task<SearchShortcodesResult> SearchShortcodesAsync(SearchShortcodesCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<SearchCalendarTasksResult> SearchCalendarTasksAsync(SearchCalendarEventsCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<SearchCalendarAppointmentsResult> SearchCalendarAppointmentsAsync(SearchCalendarEventsCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<Document> GetDocumentAsync(int searchId, DocumentPreview documentPreview, SourceType sourceType = SourceType.Auto);

        Task<Contact> GetContactAsync(int searchId, ContactPreview contactPreview, SourceType sourceType = SourceType.Auto);

        Task<Shortcode> GetShortcodeAsync(int searchId, ShortcodePreview shortcodePreview, SourceType sourceType = SourceType.Auto);

        Task<CalendarTask> GetCalendarTaskAsync(int searchId, int taskId, SourceType sourceType = SourceType.Auto);

        Task<CalendarAppointment> GetCalendarAppointmentAsync(int searchId, int appointmentId, SourceType sourceType = SourceType.Auto);

    }
}

