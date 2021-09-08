using System;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.ServiceReference;

namespace Mark5.Mobile.Common.Manager
{
    public static class Managers
    {
        public static ConnectionInfo ActiveConnectionInfo { get; private set; }

        public static IFoldersManager FoldersManager { get; private set; }
        public static IDocumentsManager DocumentsManager { get; private set; }
        public static IContactsManager ContactsManager { get; private set; }
        public static IShortcodesManager ShortcodesManager { get; private set; }
        public static ICalendarManager CalendarManager { get; private set; }
        public static ISearchManager SearchManager { get; private set; }
        public static INotificationsManager NotificationsManager { get; private set; }
        public static ISystemManager SystemManager { get; private set; }
        public static ICommonActionsManager CommonActionsManager { get; private set; }
        public static ICleanUpManager CleanUpManager { get; private set; }
        internal static IActionsManager ActionsManager { get; private set; }

        public static void Initialize(ConnectionInfo connectionInfo, string appToken = "")
        {
            ActiveConnectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));

            var appServiceProxy = AppServiceProxyFactory.Create(connectionInfo.SslMode != SslMode.Off,
                                                                connectionInfo.Hostname,
                                                                connectionInfo.Port,
                                                                CommonConfig.HttpClientHandler,
                                                                CommonConfig.OnStartTransmission,
                                                                CommonConfig.OnStopTransmission,
                                                                appToken);
            var fileTransferServiceProxy = FileTransferServiceProxyFactory.Create(connectionInfo.SslMode != SslMode.Off,
                                                                                  connectionInfo.Hostname,
                                                                                  connectionInfo.Port,
                                                                                  CommonConfig.HttpClientHandler,
                                                                                  CommonConfig.OnStartTransmission,
                                                                                  CommonConfig.OnStopTransmission);

            var foldersDataAccess = new FoldersDataAccess(DatabaseConnectionProvider.DatabaseForModuleType);
            var documentsDataAccess = new DocumentsDataAccess(DatabaseConnectionProvider.DocumentsDatabase);
            var contactsDataAccess = new ContactsDataAccess(DatabaseConnectionProvider.ContactsDatabase);
            var shortcodesDataAccess = new ShortcodesDataAccess(DatabaseConnectionProvider.ShortcodesDatabase);
            var calendarDataAccess = new CalendarDataAccess(DatabaseConnectionProvider.CalendarDatabase);
            var notificationsDataAccess = new NotificationsDataAccess(DatabaseConnectionProvider.SystemDatabase);
            var actionsDataAccess = new ActionsDataAccess(DatabaseConnectionProvider.ActionsDatabase);

            FoldersManager = new FoldersManager(connectionInfo, appServiceProxy, foldersDataAccess);
            DocumentsManager = new DocumentsManager(connectionInfo, appServiceProxy, fileTransferServiceProxy, documentsDataAccess);
            ContactsManager = new ContactsManager(connectionInfo, appServiceProxy, contactsDataAccess);
            ShortcodesManager = new ShortcodesManager(connectionInfo, appServiceProxy, shortcodesDataAccess);
            CalendarManager = new CalendarManager(connectionInfo, appServiceProxy, calendarDataAccess);
            SearchManager = new SearchManager(connectionInfo, appServiceProxy, documentsDataAccess, contactsDataAccess, shortcodesDataAccess);
            NotificationsManager = new NotificationsManager(connectionInfo, appServiceProxy, foldersDataAccess, notificationsDataAccess);
            SystemManager = new SystemManager(connectionInfo, appServiceProxy);
            CommonActionsManager = new CommonActionsManager(connectionInfo, appServiceProxy, documentsDataAccess, contactsDataAccess, shortcodesDataAccess, calendarDataAccess);
            CleanUpManager = new CleanUpManager(documentsDataAccess, contactsDataAccess, shortcodesDataAccess, calendarDataAccess);
            ActionsManager = new ActionsManager(connectionInfo, appServiceProxy, actionsDataAccess);
        }
    }
}