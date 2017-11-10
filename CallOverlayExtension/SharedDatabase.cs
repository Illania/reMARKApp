using System;
using System.Collections.Generic;
using CallKit;
using Foundation;
using SQLite;

namespace CallOverlayExtension
{
    public static class SharedDatabase
    {
        //Retrieves all contacts stored in the database in the shared container by a deferred query.
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

                        var commandString = $"select {nameof(ExtensionContact.Name)},{nameof(ExtensionContact.Number)} "
                            + $"from {nameof(ExtensionContact)} group by {nameof(ExtensionContact.Number)} order by {nameof(ExtensionContact.Number)} asc";
                        
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
                    throw new Exception("The database is locked, unable to get lock.");

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
                        throw new Exception("Error database lock file: " + error.LocalizedFailureReason);
                    }
                }
                else
                    throw new Exception("The database is locked, unable to get lock.");
            }
        }
    }

    [Table("ExtensionContact")]
    class ExtensionContact
    {
        [Column("Id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("FolderId")]
        [NotNull]
        public int FolderId { get; set; }

        [Column("Name")]
        [NotNull]
        public string Name { get; set; }

        [Column("Number")]
        [NotNull]
        public long Number { get; set; }
    }
}
