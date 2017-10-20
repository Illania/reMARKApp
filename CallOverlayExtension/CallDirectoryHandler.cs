using System;
using Foundation;
using CallKit;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;

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

            System.Collections.Generic.List<Contact> contacts = new System.Collections.Generic.List<Contact>();
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    var url = NSFileManager.DefaultManager.GetContainerUrl("group.com.nordic-it.mark5.mobile.ios");
                    var scdp = new Mark5.Mobile.Common.Database.SharedContactsDatabaseProvider(url.Path);
                    await scdp.RunInConnectionAsync(c =>
                    {
                        var commandString = $"select * from {nameof(Contact)}";

                        var cmd = c.CreateCommand(commandString);
                        contacts = cmd.ExecuteQuery<Contact>();
                    });
                }
                catch (Exception ex)
                {

                }
            }).Wait();

            //cxContext.AddIdentificationEntry(004560443773, contacts.Find(c => c.LastName.Equals("Thomsen")).LastName);
                

            //var groupPath = NSFileManager.DefaultManager.GetContainerUrl("com.nordic-it.mark5.mobile.ios.extensions.callid");

            //            //initialization
            //            AsyncHelpers.RunSync(async () =>
            //            {
            //                var mainFolder = FileSystem.Current.LocalStorage;

            //                CommonConfig.PathSeparator = Path.DirectorySeparatorChar;
            //                CommonConfig.DataFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "data"), CreationCollisionOption.OpenIfExists);
            //                CommonConfig.DatabaseFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "db"), CreationCollisionOption.OpenIfExists);
            //                CommonConfig.AttachmentsFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("Caches", "v2", "att"), CreationCollisionOption.OpenIfExists);
            //                CommonConfig.DocumentsToUploadFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "documents_upload"), CreationCollisionOption.OpenIfExists);
            //                CommonConfig.DocumentWorkingCopyFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "document_work"), CreationCollisionOption.OpenIfExists);
            //                CommonConfig.Logger = new ConsoleLogger();

            //#if !DEBUG
            //                CommonConfig.Logger.Level = LogLevel.INFO;
            //#else
            //                CommonConfig.Logger.Level = LogLevel.DEBUG;
            //#endif
            //    var authenticator = AuthenticatorFactory.Create();
            //    var ci = await authenticator.GetConnectionInfoAsync();
            //    Managers.Initialize(ci);
            //    await DatabaseUtils.InitializeDatabases();
            //});

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
            // Numbers must be provided in numerically ascending order
            /*long[] phoneNumbers = { 004560443773};
            string[] labels = { "test1234"};
            
            for (var i = 0; i < phoneNumbers.Length; i++)
            {
                long phoneNumber = phoneNumbers[i];
                string label = labels[i];
                context.AddIdentificationEntry(phoneNumber, label);
            }
            var nbrs = new List<ContactPhoneNumber>();
            CommonConfig.Logger.Info("-_____________________________________________-");

            AsyncHelpers.RunSync(async () =>
            {

                    nbrs = await Managers.ContactsManager.GetContactPhoneNumbers();

            });*/

            /*for (var i = 004559500000; i < 004560500000; i++) 
            {
                string name;
                if(i == 4560443773)
                    name = "the real mathias heyoo";
                 else 
                    name = "not mathias";
                
                context.AddIdentificationEntry(i, name);
            }*/

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

            CommonConfig.Logger.Info("Error occurred: " + error.LocalizedFailureReason);
        }
    }
}
