//
// Project: Mark5.Mobile.Common
// File: Managers.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.ServiceReference;
using Mark5.Mobile.Common.Database;

namespace Mark5.Mobile.Common.Managers
{

    public static class Managers
    {

        public static IFoldersManager FoldersManager
        {
            get;
            private set;
        }

        public static IDocumentsManager DocumentsManager
        {
            get;
            private set;
        }

        public static IContactsManager ContactsManager
        {
            get;
            private set;
        }

        public static IShortcodesManager ShortcodesManager
        {
            get;
            private set;
        }

        public static ICalendarManager CalendarManager
        {
            get;
            private set;
        }

        public static ISearchManager SearchManager
        {
            get;
            private set;
        }

        public static INotificationsManager NotificationsManager
        {
            get;
            private set;
        }

        public static ISystemManager SystemManager
        {
            get;
            private set;
        }

        public static ICommonActionsManager CommonActionsManager
        {
            get;
            private set;
        }

        public static IOutgoingDocumentsManager OutgoingDocumentsManager
        {
            get;
            private set;
        }

        public static void Initialize(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException(nameof(connectionInfo));
            }

            if (!connectionInfo.Authenticated)
            {
                throw new ArgumentException("Connection info is not authenticated.");
            }

            var appServiceProxy = AppServiceProxyFactory.Create(connectionInfo.Ssl, connectionInfo.Hostname, connectionInfo.Port);
            var fileTransferServiceProxy = FileTransferServiceProxyFactory.Create(connectionInfo.Ssl, connectionInfo.Hostname, connectionInfo.Port);

            var foldersDataAccess = new FoldersDataAccess(DatabaseConnectionProvider.DatabaseForModuleType);
            var documentsDataAccess = new DocumentsDataAccess(DatabaseConnectionProvider.DocumentsDatabase);
            var contactsDataAccess = new ContactsDataAccess(DatabaseConnectionProvider.ContactsDatabase);
            var shortcodesDataAccess = new ShortcodesDataAccess(DatabaseConnectionProvider.ShortcodesDatabase);
            var calendarDataAccess = new CalendarDataAccess(DatabaseConnectionProvider.CalendarDatabase);
            var notificationsDataAccess = new NotificationsDataAccess(DatabaseConnectionProvider.SystemDatabase);

            OutgoingDocumentsManager = new OutgoingDocumentsManager();

            FoldersManager = new FoldersManager(connectionInfo, appServiceProxy, foldersDataAccess);
            DocumentsManager = new DocumentsManager(connectionInfo, appServiceProxy, fileTransferServiceProxy, documentsDataAccess, OutgoingDocumentsManager);
            ContactsManager = new ContactsManager(connectionInfo, appServiceProxy, contactsDataAccess);
            ShortcodesManager = new ShortcodesManager(connectionInfo, appServiceProxy, shortcodesDataAccess);
            CalendarManager = new CalendarManager(connectionInfo, appServiceProxy, calendarDataAccess);
            SearchManager = new SearchManager(connectionInfo, appServiceProxy);
            NotificationsManager = new NotificationsManager(connectionInfo, appServiceProxy, foldersDataAccess, notificationsDataAccess);
            SystemManager = new SystemManager(connectionInfo, appServiceProxy);
            CommonActionsManager = new CommonActionsManager(connectionInfo, appServiceProxy, documentsDataAccess, contactsDataAccess, shortcodesDataAccess, calendarDataAccess);
        }
    }
}

