using System;
using Foundation;
using SQLite;
using System.Text;
using System.Linq;
using PhoneNumbers;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class SharedDatabase
    {
        /*This class is used by the app for storing contact information to be shown to the user when receiving calls.
        The information is stored in a shared container that the CallOverlayExtension can access. 
        In the class called CallOverlayExtension.SharedDatabase in the CallOverlayExtension solution, the contacts from the shared container are retrieved.

        If only one number is associated with a name it is simply stored as the number casted to a string.
        If there are more numbers asociated with a number they are all stored in the same string seperated by commas (',').*/

        const string databaseFileName = "sharedcontacts.sqlite3";
        const string databaseLockName = "sharedcontacts.lock";
        const string appGroupId = "group.com.nordic-it.mark5.mobile.ios";

        public static void CreateExtensionContactsTable()
        {
            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                try
                {
                    LockDatabase(containerUrl);

                    var fullDatabaseUrl = containerUrl.Append(databaseFileName, false);

                    using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                    {
                        connection.CreateTable<ExtensionContact>();
                    }
                }
                finally
                {
                    UnlockDatabase(containerUrl);
                }
            }
        }

        public static void DropExtensionContactsTable()
        {
            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                try
                {
                    LockDatabase(containerUrl);

                    var fullDatabaseUrl = containerUrl.Append(databaseFileName, false);

                    using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                    {
                        connection.DropTable<ExtensionContact>();
                    }
                }
                finally
                {
                    UnlockDatabase(containerUrl);
                }
            }
        }

        public static void CleanExtensionContactsTable(int folderId)
        {
            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                var fullDatabaseUrl = containerUrl.Append(databaseFileName, false);

                using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                {
                    try
                    {
                        LockDatabase(containerUrl);

                        connection.RunInTransaction(() => {
                            var cmd = connection.CreateCommand($"delete from {nameof(ExtensionContact)} where {nameof(ExtensionContact.FolderId)} = @folderId");
                            cmd.Bind("@folderId", folderId);
                            cmd.ExecuteNonQuery();
                        });
                    }
                    finally
                    {
                        UnlockDatabase(containerUrl);
                    }
                }
            }
            
        }

        public static void AddContactToExtensionContactsTable(int folderId, string name, string number)
        {
            var splitNumber = number.Split('|');

            var countryNumber = splitNumber[0];

            if (countryNumber == "") //No country code is defined, so the number will not be handled.
                return;

            var regCode = splitNumber[1];
            var baseNumber = splitNumber[2];

            if (regCode != String.Empty) //If there actually is a regional code, then this will be appended with the base number.
                baseNumber = regCode + baseNumber;

            var phoneNumberUtil = PhoneNumberUtil.GetInstance();

            //Get  for parsing the number. Parsing will remove any char's like '(' or '-'.
            var countryString = phoneNumberUtil.GetRegionCodeForCountryCode(Convert.ToInt32(countryNumber));
            try
            {
                PhoneNumber phoneNumber = phoneNumberUtil.Parse(baseNumber, countryString);
                number = phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
            }
            catch (NumberParseException ex)
            {
                return; //Number has been stored incorrectly, e.g. contains letters, so it is ignored.
            }

            //'+' will be in the number, since it's part of the E164 format.
            number = number.Replace("+", String.Empty);

            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                var fullDatabaseUrl = containerUrl.Append(databaseFileName, false);

                using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                {
                    try
                    {
                        LockDatabase(containerUrl);
                     
                        var contactName = new ExtensionContact();
                        contactName.FolderId = folderId;
                        contactName.Name = name;
                        contactName.Number = Convert.ToInt64(number);

                        connection.Insert(contactName);
                    }
                    finally
                    {
                        UnlockDatabase(containerUrl);
                    }
                }
            }
        }

        static void LockDatabase(NSUrl containerUrl)
        {

            var fm = NSFileManager.DefaultManager;
            var lockPath = containerUrl.Append(databaseLockName, false).Path;

            if (fm.FileExists(lockPath))
                throw new Exception("The database is locked, unable to get lock.");

            fm.CreateFile(lockPath, new NSData(), new NSFileAttributes());

        }

        static void UnlockDatabase(NSUrl containerUrl)
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
                throw new Exception("The database is not locked, unable unlock.");
        }
    }

    [Table("ExtensionContact")]
    class ExtensionContact
    {
        [Column("Id")]
        [PrimaryKey,AutoIncrement]
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
