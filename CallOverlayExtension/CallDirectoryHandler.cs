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
        const string databaseFileName = "sharedcontacts.sqlite3";

        protected CallDirectoryHandler(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void BeginRequestWithExtensionContext(NSExtensionContext context)
        {
            var cxContext = (CXCallDirectoryExtensionContext)context;
            cxContext.Delegate = this;

            /*try
            {
                using (var url = NSFileManager.DefaultManager.GetContainerUrl("group.com.nordic-it.mark5.mobile.ios"))
                {
                    var fullurl = url.Append(databaseFileName, false);

                    var exists = NSFileManager.DefaultManager.FileExists(fullurl.Path);

                    using (var connection = new SQLiteConnection(fullurl.Path,SQLiteOpenFlags.ReadOnly, true))
                    {
                        connection.CreateTable<ExtensionContact>();
                    }

                    using (var connection = new SQLiteConnection(fullurl.Path, true))
                    {
                        var contacts = connection.Table<ExtensionContact>();
                        foreach (var contact in contacts)
                        {
                            cxContext.AddIdentificationEntry(contact.Number,contact.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }*/

            #region stuff

            /*CommonConfig.Logger = new Utilities.ConsoleLogger();
            var url = NSFileManager.DefaultManager.GetContainerUrl("group.com.nordic-it.mark5.mobile.ios");
            var scdp = new Mark5.Mobile.Common.Database.SharedContactsDatabaseProvider(url.Path);
            try
            {
                Utilities.AsyncHelpers.InvokeOnMainThreadAsync(this, async () =>
                {
                    await scdp.RunInConnectionAsync(c =>
                    {
                        try
                        {
                            var commandString = $"select * from {nameof(Contact)}";
                            var cmd = c.CreateCommand(commandString);
                            contacts = cmd.ExecuteQuery<Contact>();
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    });


                }).Wait();
            }
            catch (Exception ex)
            {
                
            }*/
            /*
            try
            {
                scdp.RunInConnectionSynchronous(c =>
                {
                    try
                    {
                        CommonConfig.Logger.Info("-------------------Creating query for db-------------------");
                        //var commandString = $"select * from {nameof(Contact)}";
                        //var cmd = c.CreateCommand(commandString);
                        //CommonConfig.Logger.Info("-------------------Executing query for db------------------");
                        //var contacts = cmd.ExecuteQuery<Contact>();
                        var contacts = c.Table<Contact>().ToList();
                        CommonConfig.Logger.Info("-----------------------Query completed---------------------");
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);
            }*/

            //cxContext.AddIdentificationEntry(004560443773, contacts.Find(c => c.LastName.Equals("Thomsen")).LastName);


            //if (!AddIdentificationPhoneNumbers(cxContext))
            //{
            //    Console.WriteLine("Unable to add identification phone numbers");
            //    var error = new NSError(new NSString("CallDirectoryHandler"), 2, null);
            //    cxContext.CancelRequest(error);
            //    return;
            //}

            #endregion

            cxContext.AddIdentificationEntry(4560443773, "its a me");

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

            //CommonConfig.Logger.Info("Error occurred: " + error.LocalizedFailureReason);
        }
    }
}
