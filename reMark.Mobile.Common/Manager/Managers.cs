using System;
using reMark.Mobile.Classes.Azure;
using reMark.Mobile.Common.DataAccess;
using reMark.Mobile.Common.Database;
using reMark.Mobile.Common.Model;
using reMark.ServiceReference;
using reMark.Mobile.Common.Manager.Interfaces;
using reMark.Mobile.Classes.AuthService;

namespace reMark.Mobile.Common.Manager
{
    public static class Managers
    {
        public static ConnectionInfo ActiveConnectionInfo { get; private set; }

        public static IFoldersManager FoldersManager { get; private set; }
        public static IDocumentsManager DocumentsManager { get; private set; }
        public static IContactsManager ContactsManager { get; private set; }
        public static IShortcodesManager ShortcodesManager { get; private set; }
        public static ISearchManager SearchManager { get; private set; }
        public static INotificationsManager NotificationsManager { get; private set; }
        public static ISystemManager SystemManager { get; private set; }
        public static ICommonActionsManager CommonActionsManager { get; private set; }
        public static ICleanUpManager CleanUpManager { get; private set; }
        internal static IActionsManager ActionsManager { get; private set; }
        public static IFavoriteFoldersManager FavoriteFoldersManager { get; set; }
        public static IFavoriteFoldersManager FavoriteFoldersDesktopSyncManager { get; private set; }
        public static IFavoriteFoldersManager FavoriteFoldersDeviceSyncManager { get; private set; }
        public static IMicrosoftGraphClient MicrosoftGraphClient { get; set; }

        public static void Initialize(ConnectionInfo connectionInfo, string appToken = "", AzureApplicationProxyInfo azureAppProxyInfo =  null)
        {
            ActiveConnectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));

            var appServiceProxy = AppServiceProxyFactory.Create(connectionInfo.SslMode != SslMode.Off,
                                                                connectionInfo.Hostname,
                                                                connectionInfo.Port,
                                                                CommonConfig.HttpClientHandler,
                                                                CommonConfig.OnStartTransmission,
                                                                CommonConfig.OnStopTransmission,
                                                                CommonConfig.Reachability,
                                                                appToken,
                                                                azureAppProxyInfo);

            var fileTransferServiceProxy = FileTransferServiceProxyFactory.Create(connectionInfo.SslMode != SslMode.Off,
                                                                                  connectionInfo.Hostname,
                                                                                  connectionInfo.Port,
                                                                                  CommonConfig.HttpClientHandler,
                                                                                  CommonConfig.OnStartTransmission,
                                                                                  CommonConfig.OnStopTransmission,
                                                                                  appToken,
                                                                                  azureAppProxyInfo);

            var foldersDataAccess = new FoldersDataAccess(DatabaseConnectionProvider.DatabaseForModuleType);
            var restorationDataAccess = new RestorationDataAccess(DatabaseConnectionProvider.SystemDatabase);
            var documentsDataAccess = new DocumentsDataAccess(DatabaseConnectionProvider.DocumentsDatabase, restorationDataAccess);
            var contactsDataAccess = new ContactsDataAccess(DatabaseConnectionProvider.ContactsDatabase, restorationDataAccess);
            var shortcodesDataAccess = new ShortcodesDataAccess(DatabaseConnectionProvider.ShortcodesDatabase, restorationDataAccess);
            var notificationsDataAccess = new NotificationsDataAccess(DatabaseConnectionProvider.SystemDatabase);
            var actionsDataAccess = new ActionsDataAccess(DatabaseConnectionProvider.ActionsDatabase);

            FoldersManager = new FoldersManager(connectionInfo, appServiceProxy, foldersDataAccess);
            DocumentsManager = new DocumentsManager(connectionInfo, appServiceProxy, fileTransferServiceProxy, documentsDataAccess);
            ContactsManager = new ContactsManager(connectionInfo, appServiceProxy, contactsDataAccess);
            ShortcodesManager = new ShortcodesManager(connectionInfo, appServiceProxy, shortcodesDataAccess);
            SearchManager = new SearchManager(connectionInfo, appServiceProxy, documentsDataAccess, contactsDataAccess, shortcodesDataAccess);
            NotificationsManager = new NotificationsManager(connectionInfo, appServiceProxy, foldersDataAccess, notificationsDataAccess);
            SystemManager = new SystemManager(connectionInfo, appServiceProxy);
            CommonActionsManager = new CommonActionsManager(connectionInfo, appServiceProxy, documentsDataAccess, contactsDataAccess, shortcodesDataAccess);
            CleanUpManager = new CleanUpManager(documentsDataAccess, contactsDataAccess, shortcodesDataAccess);
            ActionsManager = new ActionsManager(connectionInfo, appServiceProxy, actionsDataAccess);
            FavoriteFoldersDesktopSyncManager = new FavoriteFoldersDesktopSyncManager(connectionInfo, appServiceProxy);
            FavoriteFoldersDeviceSyncManager = new FavoriteFoldersDeviceSyncManager(connectionInfo, appServiceProxy);
        }

    }
}