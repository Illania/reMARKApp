using CallKit;
using Foundation;
using System.Threading.Tasks;

namespace Mark5.Mobile.IOS.Common.CallId
{
    public static class CallIdUtilities
    {
        static readonly string extensionId = "com.nordic-it.mark5.mobile.ios.extensions.callid";

        public static Task<bool> IsCallIdExtensionEnabled()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

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
                            if (error == null) 
                            {
                                   
                            }
                            else
                            {
                                // Extension failed, see error.Code 
                                // and error.Description for more 
                                // information 
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
    }
}
