using System;
using Foundation;
using CallKit;

namespace CallOverlayExtension
{
    [Register("CallIdDirectoryHandler")]
    public class CallIdDirectoryHandler : CXCallDirectoryProvider, ICXCallDirectoryExtensionContextDelegate
    {
        protected CallIdDirectoryHandler(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
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
            }
            cxContext.CompleteRequest(null);
        }

        public void RequestFailed(CXCallDirectoryExtensionContext extensionContext, NSError error)
        {
            // An error occurred while adding blocking or identification entries, check the NSError for details.
            // For Call Directory error codes, see the CXErrorCodeCallDirectoryManagerError enum.
            //
            // This may be used to store the error details in a location accessible by the extension's containing app, so that the
            // app may be notified about errors which occured while loading data even if the request to load data was initiated by
            // the user in Settings instead of via the app itself.
            new ErrorLogger().WriteToLog(new NSErrorException(error));
        }
    }
}
