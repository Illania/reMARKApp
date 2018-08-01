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
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(number))
                return;

            var splitNumber = number.Split('|');

            if (splitNumber.Length != 3)
                return;

            var countryNumber = splitNumber[0];
            var regCode = splitNumber[1];
            var baseNumber = splitNumber[2];

            if (countryNumber == String.Empty) //No country code is defined, so we try to use the regional as country
            {
                countryNumber = regCode;
            }
            else
            {
                if (regCode != String.Empty) //If there actually is a regional code, then this will be appended with the base number.
                    baseNumber = regCode + baseNumber;
            }

            if (countryNumber == String.Empty) //We need to have a country number to continue
                return;

            var phoneNumberUtil = PhoneNumberUtil.GetInstance();

            //Get region code for parsing the number. Parsing will remove any char's like '(' or '-'.
            try
            {
                var countryString = phoneNumberUtil.GetRegionCodeForCountryCode(Convert.ToInt32(countryNumber));
                PhoneNumber phoneNumber = phoneNumberUtil.Parse(baseNumber, countryString);
                number = phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
            }
            catch (Exception)
            {
                return; //Number has been stored incorrectly, e.g. it contains letters, so it is ignored.
            }

            await Task.Run(() =>
            {
                try
                {
                    connectionSemaphore.Wait();

                    var contactName = new ContactIdentification
                    {
                        FolderId = folderId,
                        Name = name,
                        Number = number
                    };

                    connection.Insert(contactName);
                }
                finally
                {
                    connectionSemaphore.Release();
                }
            });
        }

        public async Task<ContactIdentification> GetMatchingContactsFromCallerIdDatabase(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return null;

            return await Task.Run(() =>
            {
                ContactIdentification c = null;
                try
                {
                    connectionSemaphore.Wait();
                    connection.RunInTransaction(() =>
                    {
                        var commandString = $"select distinct {nameof(ContactIdentification.Name)},{nameof(ContactIdentification.Number)} "
                            + $"from {nameof(ContactIdentification)} where {nameof(ContactIdentification.Number)} = ?";

                        c = connection.FindWithQuery<ContactIdentification>(commandString, number);
                    });
                }
                finally
                {
                    connectionSemaphore.Release();
                }
                return c;
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
        public string Number { get; set; }
    }
}