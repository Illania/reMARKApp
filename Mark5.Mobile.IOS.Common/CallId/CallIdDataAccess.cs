using System;
using System.Threading.Tasks;
using CallKit;
using Foundation;
using PhoneNumbers;
using SQLite;
using static Mark5.Mobile.IOS.Common.CallId.CallIdDatabaseLock;

namespace Mark5.Mobile.IOS.Common.CallId
{
    /// <summary>
    /// This class is used to insert and retrieve contacts from the database in the shared container. 
    /// The database contains the contacts that should be recognised when receivng calls.
    /// 
    /// If only one number is associated with a name it is simply stored as the number casted to a string.
    /// If there are more numbers asociated with a number they are all stored in the same string seperated by commas (',').
    /// </summary>
    /// 
    public static class CallIdDataAccess
    {
        public static void GetContactsFromSharedDatabase(CXCallDirectoryExtensionContext cxContext)
        {
            try
            {
                LockDatabase();
                using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(CallIdContainerUtilities.AppGroupId))
                {
                    if (containerUrl == null)
                        return;

                    var fullDatabaseUrl = containerUrl.Append(CallIdContainerUtilities.DatabaseName, false);

                    using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                    {
                        var commandString = $"select {nameof(ExtensionContact.Name)},{nameof(ExtensionContact.Number)} "
                            + $"from {nameof(ExtensionContact)} "
                            + $"group by {nameof(ExtensionContact.Number)} "
                            + $"order by {nameof(ExtensionContact.Number)} asc";

                        var row = connection.DeferredQuery<ExtensionContact>(commandString);
                        var enumerator = row.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            cxContext.AddIdentificationEntry(enumerator.Current.Number, enumerator.Current.Name);
                        }
                    }
                }
            }
            finally
            {
                UnlockDatabase();
            }
        }

        public static async Task AddContactToExtensionContactsTable(int folderId, string name, string number)
        {
            await Task.Run(() =>
            {
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
                try
                {
                    //Get region code for parsing the number. Parsing will remove any char's like '(' or '-'.
                    var countryString = phoneNumberUtil.GetRegionCodeForCountryCode(Convert.ToInt32(countryNumber));
                    PhoneNumber phoneNumber = phoneNumberUtil.Parse(baseNumber, countryString);
                    number = phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
                }
                catch (NumberParseException)
                {
                    return; //Number has been stored incorrectly, e.g. it contains letters, so it is ignored.
                }

                //'+' will be in the number, since it's part of the E164 format.
                number = number.Replace("+", String.Empty);

                using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(CallIdContainerUtilities.AppGroupId))
                {
                    if (containerUrl == null)
                        return;

                    var fullDatabaseUrl = containerUrl.Append(CallIdContainerUtilities.DatabaseName, false);

                    using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                    {
                        try
                        {
                            LockDatabase();

                            var contactName = new ExtensionContact
                            {
                                FolderId = folderId,
                                Name = name,
                                Number = Convert.ToInt64(number)
                            };
                            connection.Insert(contactName);
                        }
                        finally
                        {
                            UnlockDatabase();
                        }
                    }
                }

            });
        }

        public static async Task CleanExtensionContactsTable(int folderId)
        {
            await Task.Run(() =>
            {
                using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(CallIdContainerUtilities.AppGroupId))
                {
                    if (containerUrl == null)
                        return;

                    var fullDatabaseUrl = containerUrl.Append(CallIdContainerUtilities.DatabaseName, false);

                    using (var connection = new SQLiteConnection(fullDatabaseUrl.Path, true))
                    {
                        try
                        {
                            LockDatabase();

                            connection.RunInTransaction(() =>
                            {
                                var cmd = connection.CreateCommand($"delete from {nameof(ExtensionContact)} where {nameof(ExtensionContact.FolderId)} = @folderId");
                                cmd.Bind("@folderId", folderId);
                                cmd.ExecuteNonQuery();
                            });
                        }
                        finally
                        {
                            UnlockDatabase();
                        }
                    }
                }
            });
        }
    }
}