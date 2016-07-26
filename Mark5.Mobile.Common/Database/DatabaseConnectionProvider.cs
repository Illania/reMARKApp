//
// Project: Mark5.Mobile.Common
// File: DatabaseConnectionProvider.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using PCLStorage;
using SQLite;

namespace Mark5.Mobile.Common.Database
{

    class DatabaseConnectionProvider
    {

        public static DatabaseConnectionProvider DocumentsDatabase
        {
            get
            {
                if (CommonConfig.DataFolder == null)
                {
                    throw new InvalidOperationException("Data and/or cache folder is not configured.");
                }

                return documentsDatabase.Value;
            }
        }

        public static DatabaseConnectionProvider ContactsDatabase
        {
            get
            {
                if (CommonConfig.DataFolder == null)
                {
                    throw new InvalidOperationException("Data and/or cache folder is not configured.");
                }

                return contactsDatabase.Value;
            }
        }

        public static DatabaseConnectionProvider ShortcodesDatabase
        {
            get
            {
                if (CommonConfig.DataFolder == null)
                {
                    throw new InvalidOperationException("Data and/or cache folder is not configured.");
                }

                return shortcodesDatabase.Value;
            }
        }

        public static DatabaseConnectionProvider CalendarDatabase
        {
            get
            {
                if (CommonConfig.DataFolder == null)
                {
                    throw new InvalidOperationException("Data and/or cache folder is not configured.");
                }

                return calendarDatabase.Value;
            }
        }

        public static DatabaseConnectionProvider CommonDatabase
        {
            get
            {
                if (CommonConfig.DataFolder == null)
                {
                    throw new InvalidOperationException("Data and/or cache folder is not configured.");
                }

                return commonDatabase.Value;
            }
        }

        static readonly Lazy<DatabaseConnectionProvider> documentsDatabase = new Lazy<DatabaseConnectionProvider>(() => new DatabaseConnectionProvider("documents.sqlite3"), LazyThreadSafetyMode.ExecutionAndPublication);
        static readonly Lazy<DatabaseConnectionProvider> contactsDatabase = new Lazy<DatabaseConnectionProvider>(() => new DatabaseConnectionProvider("contacts.sqlite3"), LazyThreadSafetyMode.ExecutionAndPublication);
        static readonly Lazy<DatabaseConnectionProvider> shortcodesDatabase = new Lazy<DatabaseConnectionProvider>(() => new DatabaseConnectionProvider("shortcodes.sqlite3"), LazyThreadSafetyMode.ExecutionAndPublication);
        static readonly Lazy<DatabaseConnectionProvider> calendarDatabase = new Lazy<DatabaseConnectionProvider>(() => new DatabaseConnectionProvider("calendar.sqlite3"), LazyThreadSafetyMode.ExecutionAndPublication);
        static readonly Lazy<DatabaseConnectionProvider> commonDatabase = new Lazy<DatabaseConnectionProvider>(() => new DatabaseConnectionProvider("common.sqlite3"), LazyThreadSafetyMode.ExecutionAndPublication);

        public static DatabaseConnectionProvider DatabaseForModuleType(ModuleType moduleType)
        {
            switch (moduleType)
            {
                case ModuleType.Documents:
                    return DocumentsDatabase;
                case ModuleType.Contacts:
                    return ContactsDatabase;
                case ModuleType.Shortcodes:
                    return ShortcodesDatabase;
                case ModuleType.Calendar:
                    return CalendarDatabase;
                default:
                    return CommonDatabase;
            }
        }

        readonly SQLiteConnection connection;
        readonly SemaphoreSlim connectionSemaphore = new SemaphoreSlim(1);

        DatabaseConnectionProvider(string dbFileName)
        {
            connection = new SQLiteConnection(PortablePath.Combine(CommonConfig.DatabaseFolder.Path, dbFileName), true);
#if DEBUG
            connection.Trace = true;
#endif
        }

        ~DatabaseConnectionProvider()
        {
            try
            {
                connection?.Close();
            }
            catch
            {
                // Nothing to do here
            }
        }

        public async Task RunInConnectionAsync(Action<SQLiteConnection> action)
        {
            await Task.Run(() =>
            {
                try
                {
                    connectionSemaphore.Wait();

                    connection.RunInTransaction(() =>
                    {
                        action(connection);
                    });
                }
                finally
                {
                    connectionSemaphore.Release();
                }
            });
        }
    }
}

