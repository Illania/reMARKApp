using CallKit;
using Foundation;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class OverlayExtensionStatus
    {
        static readonly string extensionId = "com.nordic-it.mark5.mobile.ios.extensions.callid";

        public static void SetCallerIdPreference()
        {
            CXCallDirectoryManager.SharedInstance.GetEnabledStatusForExtension(extensionId,
                                                                               (CXCallDirectoryEnabledStatus status, NSError statuserror) =>
            {
                if (statuserror == null)
                {
                    PlatformConfig.Preferences.CallerIdentificationEnabled = (status == CXCallDirectoryEnabledStatus.Enabled);
                } 
                else 
                {
                    throw new NSErrorException(statuserror);
                }
            });
        }

        public static void RefreshExtension()
        {
            CXCallDirectoryManager.SharedInstance.GetEnabledStatusForExtension(extensionId,
                                                                               (CXCallDirectoryEnabledStatus status, NSError statuserror) => 
            {
                if(statuserror == null)
                {
                    if (status == CXCallDirectoryEnabledStatus.Enabled)
                    {
                        CXCallDirectoryManager.SharedInstance.ReloadExtension(extensionId,
                        error =>
                        {
                            if (error == null)
                            {
                                CommonConfig.Logger.Info("CallOverlayExtension refreshed succesfully.");
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