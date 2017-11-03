using System;
using System.Collections.Generic;
using Foundation;
using SQLite;

namespace CallOverlayExtension
{
    public static class SharedDatabase
    {
        //Retrieves all contacts stored in the database in the shared container.

        const string databaseFileName = "sharedcontacts.sqlite3";
        const string databaseLockName = "sharedcontacts.lock";
        const string appGroupId = "group.com.nordic-it.mark5.mobile.ios";

        public static List<(string name, long number)> GetContactsFromSharedDatabase()
        {
            List<ExtensionContact> dbContacts = null;

            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                var fullDatabaseUrl = containerUrl.Append(databaseFileName, false);
               
                using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                {
                    try
                    {
                        LockDatabase();

                        connection.RunInTransaction(() => {
                            var commandString = $"select * "
                                + $"from {nameof(ExtensionContact)} ";

                            var cmd = connection.CreateCommand(commandString);
                            var result = cmd.ExecuteQuery<ExtensionContact>();
                            dbContacts = result;
                        });
                    }
                    finally { UnlockDatabase(); }
                }
            }

            return ProcessResult(dbContacts);
        }

        static List<(string name, long number)> ProcessResult(List<ExtensionContact> dbResult)
        {
            List<(string name, long number)> result = new List<(string name, long number)>();

            foreach(ExtensionContact ec in dbResult)
            {
                var name = ec.Name;
                var numbers = ec.Numbers.Split(',');
                for (int i = 0; i < numbers.Length; i++)
                {
                    result.Add((name,Convert.ToInt64(numbers[i])));
                }
            }

            result.Sort((ec1,ec2) => ec1.number.CompareTo(ec2.number));
            return result;
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
    public class ExtensionContact
    {
        [Column("Id")]
        [PrimaryKey]
        public int Id { get; set; }

        [Column("Name")]
        [NotNull]
        public string Name { get; set; }

        [Column("Number")]
        [NotNull]
        public string Numbers { get; set; }
    }
}
