using System;
using CallKit;
using Foundation;
using SQLite;
using Mark5.Mobile.IOS.Extensions.CallId.Exceptions;

namespace Mark5.Mobile.IOS.Extensions.CallId
{
    public static class CallerIdSharedDatabase
    {
        /// <summary>
        /// This class is used to access the database in the shared container. 
        /// The database contains the contacts that should be recognised when receing calls.
        /// </summary>

        const string databaseFileName = "sharedcontacts.sqlite3";
        const string databaseLockName = "sharedcontacts.lock";
        const string appGroupId = "group.com.nordic-it.mark5.mobile.ios";

        public static void GetContactsFromSharedDatabase(CXCallDirectoryExtensionContext cxContext)
        {
            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                var fullDatabaseUrl = containerUrl.Append(databaseFileName, false);
               
                using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                {
                    try
                    {
                        LockDatabase();

                        //throw new Exception("ding ding");

                        var commandString = $"select {nameof(ExtensionContact.Name)},{nameof(ExtensionContact.Number)} "
                            + $"from {nameof(ExtensionContact)} "
                            + $"group by {nameof(ExtensionContact.Number)} " 
                            + $"order by {nameof(ExtensionContact.Number)} asc";
                        
                        var row = connection.DeferredQuery<ExtensionContact>(commandString);
                        var enumerator = row.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            cxContext.AddIdentificationEntry(enumerator.Current.Number,enumerator.Current.Name);
                        }
                    }
                    finally { UnlockDatabase(); }
                }
            }
        }

        static void LockDatabase()
        {
            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                var fm = NSFileManager.DefaultManager;
                var lockPath = containerUrl.Append(databaseLockName, false).Path;

                if (fm.FileExists(lockPath))
                    throw new DatabaseLockException("The database is locked, unable to get lock.");

                fm.CreateFile(lockPath, new NSData(), new NSFileAttributes());
            }
        }

        static void UnlockDatabase()
        {
            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                var fm = NSFileManager.DefaultManager;
                var lockPath = containerUrl.Append(databaseLockName, false).Path;

                if (fm.FileExists(lockPath))
                {
                    NSError error = new NSError();
                    fm.Remove(lockPath, out error);
                    if (error != null)
                    {
                        throw new NSErrorException(error);
                    }
                }
                else
                    throw new DatabaseLockException("The database is locked, unable to get lock.");
            }
        }
    }
}