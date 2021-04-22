using System;
using Foundation;
using CallKit;
using Mark5.Mobile.IOS.Common.CallId;

namespace Mark5.Mobile.IOS.Extensions.CallId
{
    [Register("CallDirectoryHandler")]
    public class CallIdDirectoryHandler : CXCallDirectoryProvider, ICXCallDirectoryExtensionContextDelegate
    {
        protected CallIdDirectoryHandler(IntPtr handle)
            : base(handle) 
        {
        }

        public override void BeginRequestWithExtensionContext(NSExtensionContext context)
        {
            var cxContext = (CXCallDirectoryExtensionContext)context;
            cxContext.Delegate = this;

            try
            {
                CallIdDataAccess.GetContactsFromSharedDatabase(cxContext);
            }
            catch (Exception ex)
            {
                CallIdErrorLogger.WriteToLog("Error when trying to get contacts from shared db", ex);
            }
            cxContext.CompleteRequest(null);
        }

        public void RequestFailed(CXCallDirectoryExtensionContext extensionContext, NSError error)
        {
            CallIdErrorLogger.WriteToLog("CallId request failed", new NSErrorException(error));
        }
    }
}