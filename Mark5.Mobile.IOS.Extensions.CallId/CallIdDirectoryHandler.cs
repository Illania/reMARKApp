using System;
using Foundation;
using CallKit;

namespace Mark5.Mobile.IOS.Extensions.CallId
{
    [Register("CallIdDirectoryHandler")]
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
                var lel = new NSString("ele");
                var er = new NSError(lel,0,null);
                cxContext.CancelRequest(er);
            } 
            catch (Exception ex)
            {
                new ErrorLogger().WriteToLog(ex);


            }
            cxContext.CompleteRequest(null);
        }

        public void RequestFailed(CXCallDirectoryExtensionContext extensionContext, NSError error)
        {
            new ErrorLogger().WriteToLog(new NSErrorException(error));
        }
    }
}