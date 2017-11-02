using System;
using Foundation;
using SQLite;
using PhoneNumbers;



namespace Mark5.Mobile.IOS.Utilities
{
    public class SharedDatabase
    {
        //This class is used by the app for storing contact information to be shown to the user when receiving calls.
        //The information is stored in a shared container that the CallOverlayExtension can access. 
        //In the class called (Insert name) in the CallOverlayExtension solution, the contacts from the shared container are retrieved.
        const string databaseFileName = "sharedcontacts.sqlite3";
        const string appGroupId = "group.com.nordic-it.mark5.mobile.ios";

        public void CreateExtensionContactsTable()
        {
            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                var fullDatabaseUrl = containerUrl.Append(databaseFileName, false);

                using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                {
                    connection.CreateTable<ExtensionContact>();
                }
            }
        }

        public void AddContactToExtensionContactsTable(int id, string name, string number)
        {
            //Handle the number formatting
            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(appGroupId))
            {
                var extContact = new ExtensionContact();
                extContact.Id = id;
                extContact.Name = name;

                var fullDatabaseUrl = containerUrl.Append(databaseFileName, false);


                using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                {
                    try
                    {
                        LockDatabase();

                        var dbExtContact = connection.Get<ExtensionContact>(extContact);
                        if (dbExtContact != null) //If database already contains row with given id, then @number should be appended to the string of numbers stored for that row.
                        {

                        }
                        else //Otherwise insert contact with just the @number stored in the string of numbers.
                        {
                            var formatedNumber = number.Replace("|",String.Empty);
                            formatedNumber = PhoneNumberUtil.ExtractPossibleNumber(formatedNumber);
                            connection.InsertOrReplace(extContact);
                        }
                    }
                    finally { UnlockDatabase(); }
                }
            }

        }


        void FormatNumberForDatabase()
        {

        }

        void LockDatabase()
        {
            //Create log file, if already exists throw exception.
        }

        void UnlockDatabase()
        {
            //Delete log file.
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
