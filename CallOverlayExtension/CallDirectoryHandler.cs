using System;
using Foundation;
using CallKit;
using Mark5.Mobile.Common;
using CallOverlayExtension.Utilities;
using System.Threading.Tasks;
using UIKit;
using System.Runtime.InteropServices;
using System.Text;
using PCLStorage;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Extensions;
using System.IO;

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

            /*//initialization
            Task.Run(async () =>
            {
                var mainFolder = FileSystem.Current.LocalStorage;

                CommonConfig.PathSeparator = Path.DirectorySeparatorChar;
                CommonConfig.DataFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "data"), CreationCollisionOption.OpenIfExists);
                CommonConfig.DatabaseFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "db"), CreationCollisionOption.OpenIfExists);
                CommonConfig.AttachmentsFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("Caches", "v2", "att"), CreationCollisionOption.OpenIfExists);
                CommonConfig.DocumentsToUploadFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "documents_upload"), CreationCollisionOption.OpenIfExists);
                CommonConfig.DocumentWorkingCopyFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "document_work"), CreationCollisionOption.OpenIfExists);
                CommonConfig.Logger = new ConsoleLogger();


                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 3))
                    CommonConfig.Utf8Normalizer = filename =>
                    {
                        var url = NSUrl.FromFilename(filename);
                        var fsPtr = url.GetFileSystemRepresentationAsUtf8Ptr;
                        var numBytes = 0;
                        while (Marshal.ReadByte(fsPtr, numBytes) != 0)
                            numBytes++;

                        var utf8Bytes = new byte[numBytes];
                        Marshal.Copy(fsPtr, utf8Bytes, 0, numBytes);
                        return Encoding.UTF8.GetString(utf8Bytes).SafeSubstringAfterLast(Path.DirectorySeparatorChar);
                    };
                else
                    CommonConfig.Utf8Normalizer = filename => filename;
                
#if !DEBUG
                CommonConfig.Logger.Level = LogLevel.INFO;
#else
                CommonConfig.Logger.Level = LogLevel.DEBUG;
#endif



                //((ConsoleAndFileLogger)CommonConfig.Logger).CleanUpOldLogFiles();
                await DatabaseUtils.InitializeDatabases();



            })
            .Wait();*/

            if (!AddIdentificationPhoneNumbers(cxContext))
            {
                Console.WriteLine("Unable to add identification phone numbers");
                var error = new NSError(new NSString("CallDirectoryHandler"), 2, null);
                cxContext.CancelRequest(error);
                return;
            }

            cxContext.CompleteRequest(null);
        }

        bool AddIdentificationPhoneNumbers(CXCallDirectoryExtensionContext context)
        {
            // Numbers must be provided in numerically ascending order.

            long[] phoneNumbers = { 004560443773};
            string[] labels = { "Bob"};

            for (var i = 0; i < phoneNumbers.Length; i++)
            {
                long phoneNumber = phoneNumbers[i];
                string label = labels[i];
                context.AddIdentificationEntry(phoneNumber, label);
            }

            return true;
        }

        public void RequestFailed(CXCallDirectoryExtensionContext extensionContext, NSError error)
        {
            // An error occurred while adding blocking or identification entries, check the NSError for details.
            // For Call Directory error codes, see the CXErrorCodeCallDirectoryManagerError enum.
            //
            // This may be used to store the error details in a location accessible by the extension's containing app, so that the
            // app may be notified about errors which occured while loading data even if the request to load data was initiated by
            // the user in Settings instead of via the app itself.
        }
    }
}
