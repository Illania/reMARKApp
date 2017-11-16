using System;
using CallKit;
using Foundation;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class OverlayExtensionStatus
    {
        public static bool IsEnabled()
        {
            bool isEnabled = false;

            CXCallDirectoryManager.SharedInstance.GetEnabledStatusForExtension("com.nordic-it.mark5.mobile.ios.extensions.callid",
                                                                               (CXCallDirectoryEnabledStatus status, NSError statuserror) =>
            {
                if (statuserror == null)
                {
                    isEnabled = status == CXCallDirectoryEnabledStatus.Enabled;
                } 
                else 
                {
                    throw new NSErrorException(statuserror);
                }
            });
            return isEnabled;
        }

        public static void RefreshExtension()
        {
            CXCallDirectoryManager.SharedInstance.GetEnabledStatusForExtension("com.nordic-it.mark5.mobile.ios.extensions.callid",
                                                                               (CXCallDirectoryEnabledStatus status, NSError statuserror) => 
            {
                if(statuserror == null)
                {
                    if (status == CXCallDirectoryEnabledStatus.Enabled)
                    {
                        CXCallDirectoryManager.SharedInstance.ReloadExtension("com.nordic-it.mark5.mobile.ios.extensions.callid",
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
