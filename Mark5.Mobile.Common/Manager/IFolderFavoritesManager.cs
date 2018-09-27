using System;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.ServiceReference.AppService;
using Mark5.ServiceReference.FileTransferService;

namespace Mark5.Mobile.Common.Manager
{
    public class FolderFavoritesManager : AbstractManager
    {
        readonly IFileTransferServiceProxy fileTransferServiceProxy;
        readonly IDocumentsDataAccess documentsDataAccess;

        public FolderFavoritesManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IFileTransferServiceProxy fileTransferServiceProxy, IDocumentsDataAccess documentsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.fileTransferServiceProxy = fileTransferServiceProxy;
            this.documentsDataAccess = documentsDataAccess;
        }
    }
}
