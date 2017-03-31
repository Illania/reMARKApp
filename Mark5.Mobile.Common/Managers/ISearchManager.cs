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

        int MaxDocumentsToFetch { get; set; }

        int MaxContactsToFetch { get; set; }

        int MaxShortcodesToFetch { get; set; }

        Task<List<SavedSearch>> GetSavedSearches(SourceType sourceType = SourceType.Auto);

        Task<List<DocumentPreview>> SearchDocumentsAsync(SearchDocumentsCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<List<ContactPreview>> SearchContactsAsync(SearchContactsCriteria critera, SourceType sourceType = SourceType.Auto);

        Task<List<ShortcodePreview>> SearchShortcodesAsync(SearchShortcodesCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<List<CalendarTask>> SearchCalendarTasksAsync(SearchCalendarEventsCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<List<CalendarAppointment>> SearchCalendarAppointmentsAsync(SearchCalendarEventsCriteria criteria, SourceType sourceType = SourceType.Auto);
    }
}

