using Mark5.Mobile.Common.Model;
using Mark5.ServiceReference.AppService;

namespace Mark5.Mobile.Common.Manager
{
    abstract class AbstractManager
    {
        protected string Token => ConnectionInfo.Token;

        protected readonly ConnectionInfo ConnectionInfo;
        protected readonly IAppServiceProxy AppServiceProxy;

        protected AbstractManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy)
        {
            ConnectionInfo = connectionInfo;
            AppServiceProxy = appServiceProxy;
        }
    }
}