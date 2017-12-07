using System;
using Foundation;
using CallKit;

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
                CallerIdSharedDatabase.GetContactsFromSharedDatabase(cxContext);
            }
            catch (Exception ex)
            {
                new ErrorLogger().WriteToLog(ex);
                throw; //This will prompt the user with an error dialog, instead of hiding the fact that an exception occured.
            }
            cxContext.CompleteRequest(null);
        }

        public void RequestFailed(CXCallDirectoryExtensionContext extensionContext, NSError error)
        {
            new ErrorLogger().WriteToLog(new NSErrorException(error));
        }
    }
}