using System;
using Foundation;
using SQLite;
using System.Text;
using System.Linq;

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

        public static void AddContactToExtensionContactsTable(int id, string name, string number)
        {
            if (number[0] == '|') //If the first char is '|', then the country code isn't specified. The country code must be specified for the number to work with the caller extension.
                return;

            //Format number. Only '|' and "+" needs to be removed for the number to be identified by the phone.
            number = number.Replace("|", String.Empty).Replace("+", "").Replace(" ","");

            if (!number.All(n => Char.IsDigit(n))) //If the number contains letters, then it should not be added.
                return;

            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                var fullDatabaseUrl = containerUrl.Append(databaseFileName, false);

                using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                {
                    try
                    {
                        LockDatabase(containerUrl);

                        var dbExtContact = connection.Find<ExtensionContact>(id);

                        if (dbExtContact != null) //If database already contains row with given id, then @number should be appended to the string of numbers stored for that row.
                        {
                            if (dbExtContact.Numbers.Contains(number))
                                return;
                                   
                            StringBuilder sb = new StringBuilder();
                            sb.Append(dbExtContact.Numbers).Append(",").Append(number);
                            dbExtContact.Numbers = sb.ToString();

                            connection.InsertOrReplace(dbExtContact);
                        }
                        else //Otherwise insert contact with just the @number stored in the string of numbers.
                        {
                            var newExtContact = new ExtensionContact();
                            newExtContact.Id = id;
                            newExtContact.Name = name;
                            newExtContact.Numbers = number;

                            connection.Insert(newExtContact);
                        }
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
                    if(error != null)
                    {
                        throw new Exception("Error database lock file: " + error.LocalizedFailureReason);
                    }
                }
                else
                    throw new Exception("The database is not locked, unable unlock.");
            
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
