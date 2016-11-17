//
// Project: Mark5.Mobile.Common
// File: DatabaseUtils.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Links;

namespace Mark5.Mobile.Common.Database
{

    public static class DatabaseUtils
    {

        public static async Task InitializeDatabases()
        {
            await DatabaseConnectionProvider.DocumentsDatabase.RunInConnectionAsync(c =>
            {
                c.CreateTable<Folder>();
                c.CreateTable<FolderDocumentLink>();
                c.CreateTable<DocumentPreview>();
                c.CreateTable<Document>();
                c.CreateTable<Category>();
                c.CreateTable<TemplatePreview>();
                c.CreateTable<Template>();
                c.CreateTable<DefaultTemplateInfo>();
                c.CreateTable<RecentAddress>();
            });
            await DatabaseConnectionProvider.ContactsDatabase.RunInConnectionAsync(c =>
            {
                c.CreateTable<Folder>();
                c.CreateTable<FolderContactLink>();
                c.CreateTable<ContactPreview>();
                c.CreateTable<Contact>();
                c.CreateTable<Category>();
                c.CreateTable<ContactCommunicationAddress>();
            });
            await DatabaseConnectionProvider.ShortcodesDatabase.RunInConnectionAsync(c =>
            {
                c.CreateTable<Folder>();
                c.CreateTable<FolderShortcodeLink>();
                c.CreateTable<ShortcodePreview>();
                c.CreateTable<Shortcode>();
                c.CreateTable<Category>();
            });
            await DatabaseConnectionProvider.CalendarDatabase.RunInConnectionAsync(c =>
            {
                c.CreateTable<Folder>();
                c.CreateTable<FolderCalendarAppointmentLink>();
                c.CreateTable<FolderCalendarTaskLink>();
                c.CreateTable<CalendarTask>();
                c.CreateTable<CalendarAppointment>();
                c.CreateTable<Category>();
            });
        }

        public static async Task ClearDatabases()
        {
            await DatabaseConnectionProvider.DocumentsDatabase.RunInConnectionAsync(c =>
            {
                c.DeleteAll<Folder>();
                c.DeleteAll<FolderDocumentLink>();
                c.DeleteAll<DocumentPreview>();
                c.DeleteAll<Document>();
                c.DeleteAll<Category>();
                c.DeleteAll<TemplatePreview>();
                c.DeleteAll<Template>();
                c.DeleteAll<DefaultTemplateInfo>();
            });
            await DatabaseConnectionProvider.ContactsDatabase.RunInConnectionAsync(c =>
            {
                c.DeleteAll<Folder>();
                c.DeleteAll<FolderContactLink>();
                c.DeleteAll<ContactPreview>();
                c.DeleteAll<Contact>();
                c.DeleteAll<Category>();
                c.DeleteAll<ContactCommunicationAddress>();
            });
            await DatabaseConnectionProvider.ShortcodesDatabase.RunInConnectionAsync(c =>
            {
                c.DeleteAll<Folder>();
                c.DeleteAll<FolderShortcodeLink>();
                c.DeleteAll<ShortcodePreview>();
                c.DeleteAll<Shortcode>();
                c.DeleteAll<Category>();
            });
            await DatabaseConnectionProvider.CalendarDatabase.RunInConnectionAsync(c =>
            {
                c.DeleteAll<Folder>();
                c.DeleteAll<FolderCalendarAppointmentLink>();
                c.DeleteAll<FolderCalendarTaskLink>();
                c.DeleteAll<CalendarTask>();
                c.DeleteAll<CalendarAppointment>();
                c.DeleteAll<Category>();
            });
        }

        public static async Task CompactDatabases()
        {
            await DatabaseConnectionProvider.DocumentsDatabase.RunInConnectionAsync(c =>
            {
                c.CreateCommand("VACUUM;").ExecuteNonQuery();
            });
            await DatabaseConnectionProvider.ContactsDatabase.RunInConnectionAsync(c =>
            {
                c.CreateCommand("VACUUM;").ExecuteNonQuery();
            });
            await DatabaseConnectionProvider.ShortcodesDatabase.RunInConnectionAsync(c =>
            {
                c.CreateCommand("VACUUM;").ExecuteNonQuery();
            });
            await DatabaseConnectionProvider.CalendarDatabase.RunInConnectionAsync(c =>
            {
                c.CreateCommand("VACUUM;").ExecuteNonQuery();
            });
            await DatabaseConnectionProvider.SystemDatabase.RunInConnectionAsync(c =>
            {
                c.CreateCommand("VACUUM;").ExecuteNonQuery();
            });
        }

        public static async Task ResetDatabases()
        {
            await ClearDatabases();
            await CompactDatabases();
        }
    }
}

