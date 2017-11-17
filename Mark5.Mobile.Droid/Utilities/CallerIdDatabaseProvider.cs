using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using PhoneNumbers;
using SQLite;

namespace Mark5.Mobile.Droid.Utilities
{
    class CallerIdDatabaseProvider
    {
        public static CallerIdDatabaseProvider CallerIdDatabase
        {
            get { return callerIdDatabaseConnection.Value; }
        }

        readonly SemaphoreSlim connectionSemaphore = new SemaphoreSlim(1);
        readonly SQLiteConnection connection;
        const string dbFileName = "CallerIdContacts.sqlite3";
        static readonly Lazy<CallerIdDatabaseProvider> callerIdDatabaseConnection = new Lazy<CallerIdDatabaseProvider>(() => new CallerIdDatabaseProvider(), LazyThreadSafetyMode.ExecutionAndPublication);

        CallerIdDatabaseProvider()
        {
            connection = new SQLiteConnection(CommonConfig.DatabaseFolder.Path + CommonConfig.PathSeparator + dbFileName, true);

#if DEBUG
            connection.Trace = true;
#endif
        }

        ~CallerIdDatabaseProvider()
        {
            try
            {
                connection?.Close();
            }
            catch
            {
                // Nothing to do here
            }
        }

        public async Task CreateTable()
        {
            await Task.Run(() =>
            {
                try
                {
                    connectionSemaphore.Wait();

                    connection.CreateTable<ContactIdentification>();
                }
                finally
                {
                    connectionSemaphore.Release();
                }
            });
        }

        public async Task DropTable()
        {
            await Task.Run(() =>
            {
                try
                {
                    connectionSemaphore.Wait();

                    connection.DropTable<ContactIdentification>();
                }
                finally
                {
                    connectionSemaphore.Release();
                }
            });
        }

        public async Task CleanTable(int folderId)
        {
            await Task.Run(() =>
            {
                try
                {
                    connectionSemaphore.Wait();

                    connection.RunInTransaction(() =>
                    {
                        var cmd = connection.CreateCommand($"delete from {nameof(ContactIdentification)} where {nameof(ContactIdentification.FolderId)} = @folderId");
                        cmd.Bind("@folderId", folderId);
                        cmd.ExecuteNonQuery();
                    });
                }
                finally
                {
                    connectionSemaphore.Release();
                }
            });
        }

        public async Task AddContact(int folderId, string name, string number)
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

            //Get region code for parsing the number. Parsing will remove any char's like '(' or '-'.
            var countryString = phoneNumberUtil.GetRegionCodeForCountryCode(Convert.ToInt32(countryNumber));
            try
            {
                PhoneNumber phoneNumber = phoneNumberUtil.Parse(baseNumber, countryString);
                number = phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
            }
            catch (NumberParseException ex)
            {
                return; //Number has been stored incorrectly, e.g. it contains letters, so it is ignored.
            }

            //'+' will be in the number, since it's part of the E164 format.
            number = number.Replace("+", String.Empty);

            await Task.Run(() =>
            {
                try
                {
                    connectionSemaphore.Wait();

                    var contactName = new ContactIdentification();
                    contactName.FolderId = folderId;
                    contactName.Name = name;
                    contactName.Number = Convert.ToInt64(number);

                    connection.Insert(contactName);
                }
                finally
                {
                    connectionSemaphore.Release();
                }
            });
        }

        public async Task<ContactIdentification> GetContactsFromSharedDatabase(string number)
        {
            await Task.Run(() =>
            {
                ContactIdentification contact = null;
                try
                {
                    
                    connectionSemaphore.Wait();
                    connection.RunInTransaction(() => {
                        var commandString = $"select distinct {nameof(ContactIdentification.Name)},{nameof(ContactIdentification.Number)} "
                            + $"from {nameof(ContactIdentification)} where {nameof(ContactIdentification.Number)} = ?";

                        contact = connection.FindWithQuery<ContactIdentification>(commandString, number);
                        /*var cmd = connection.CreateCommand(commandString);
                        cmd.Bind("@number", number);
                        var result = cmd.ExecuteQuery<ContactIdentification>();*/
                    });
                }
                finally
                {
                    connectionSemaphore.Release();
                }
                return contact;
            });
        }

    }

    [Table("ContactIdentification")]
    class ContactIdentification
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



