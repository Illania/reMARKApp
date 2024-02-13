using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Extensions;
using Android.Content;
using System.Threading.Tasks;

namespace reMark.Mobile.Droid.Service
{
    public interface IPushNotificationsRegistrator
    {
        public Task RegisterToken(Context context);

        public void UpdateToken();

        public void DeleteToken();
        
        public void Listen(Context context);
    }
}
