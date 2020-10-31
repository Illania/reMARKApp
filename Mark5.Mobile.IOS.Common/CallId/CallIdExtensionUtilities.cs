using CallKit;
using Foundation;
using System.Threading.Tasks;

namespace Mark5.Mobile.IOS.Common.CallId
{
    public static class CallIdExtensionUtilities
    {
        static readonly string extensionId = "com.nordic-it.mark5.mobile.ios.callid";

        public static Task<bool> IsCallIdExtensionEnabled()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if(IsChinaCustomer())
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            CXCallDirectoryManager.SharedInstance.GetEnabledStatusForExtension(extensionId,
                                                                               (CXCallDirectoryEnabledStatus status, NSError statuserror) =>
            {
                if (statuserror == null)
                {
                    var enabled = status == CXCallDirectoryEnabledStatus.Enabled;
                    tcs.SetResult(enabled);
                }
                else
                {
                    tcs.SetException(new NSErrorException(statuserror));
                }
            });
            return tcs.Task;
        }

        public static void ReloadExtension()
        {
            CXCallDirectoryManager.SharedInstance.GetEnabledStatusForExtension(extensionId,
                                                                               (CXCallDirectoryEnabledStatus status, NSError statuserror) =>
            {
                if (statuserror == null)
                {
                    if (status == CXCallDirectoryEnabledStatus.Enabled)
                    {
                        CXCallDirectoryManager.SharedInstance.ReloadExtension(extensionId,
                                                                              error =>
                        {
                            if (error != null)
                            {
                                throw new NSErrorException(error);
                            }
                        });
                    }
                    else
                        return;
                }
                else
                {
                    throw new NSErrorException(statuserror);
                }
            });
        }

        public static bool IsChinaCustomer()
        {
            NSLocale userLocale = NSLocale.CurrentLocale;
            if (userLocale.CountryCode.Contains("CN") || userLocale.CountryCode.Contains("CHN"))
            {
                return true;
            }
            return false;
        }
    
    }
}
