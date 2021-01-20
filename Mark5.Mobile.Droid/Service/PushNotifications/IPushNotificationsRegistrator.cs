using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Extensions;
using Android.Content;
using System.Threading.Tasks;

namespace Mark5.Mobile.Droid.Service
{
    public interface IPushNotificationsRegistrator
    {
        public Task RegisterToken(Context context);

        public void UpdateToken();

        public void DeleteToken();
        
        public void Listen(Context context);
    }
}
