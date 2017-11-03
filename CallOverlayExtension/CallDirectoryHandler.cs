using System;
using Foundation;
using CallKit;
using System.Linq;
using SQLite;

namespace CallOverlayExtension
{
    [Register("CallDirectoryHandler")]
    public class CallDirectoryHandler : CXCallDirectoryProvider, ICXCallDirectoryExtensionContextDelegate
    {
        
        protected CallDirectoryHandler(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void BeginRequestWithExtensionContext(NSExtensionContext context)
        {
            var cxContext = (CXCallDirectoryExtensionContext)context;
            cxContext.Delegate = this;

            AddIdentificationPhoneNumbers(cxContext);

            cxContext.CompleteRequest(null);
        }

        void AddIdentificationPhoneNumbers(CXCallDirectoryExtensionContext context)
        {
            var contacts = SharedDatabase.GetContactsFromSharedDatabase();
            foreach ((string name, long number) in contacts) 
            {
                context.AddIdentificationEntry(number,name);
            }
        }

        public void RequestFailed(CXCallDirectoryExtensionContext extensionContext, NSError error)
        {
            // An error occurred while adding blocking or identification entries, check the NSError for details.
            // For Call Directory error codes, see the CXErrorCodeCallDirectoryManagerError enum.
            //
            // This may be used to store the error details in a location accessible by the extension's containing app, so that the
            // app may be notified about errors which occured while loading data even if the request to load data was initiated by
            // the user in Settings instead of via the app itself.

            //CommonConfig.Logger.Info("Error occurred: " + error.LocalizedFailureReason);
        }
    }
}
