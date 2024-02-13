using reMark.Mobile.Common.Model;
using reMark.ServiceReference.AppService;

namespace reMark.Mobile.Common.Manager
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