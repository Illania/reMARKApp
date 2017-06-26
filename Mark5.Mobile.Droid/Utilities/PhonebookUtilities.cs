using System.Collections.Generic;
using Android;
using Android.Net;
using Android.Provider;
using Android.Support.V4.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities
{
    public class PhonebookUtilities : Java.Lang.Object, IPhonebook
    {
        readonly Uri contactsUri = ContactsContract.CommonDataKinds.Email.ContentUri;

        string ContactIdColumn = ContactsContract.RawContacts.InterfaceConsts.ContactId;
        string ContactsNameColumn = ContactsContract.Contacts.InterfaceConsts.DisplayName;
        string ContactEmailColumn = ContactsContract.CommonDataKinds.Email.Address;

        #region IPhonebookUtilities implementation

        public List<Recipient> GetPhonebookContacts()
        {
            return GetAndroidContacts(null);
        }

        public List<Recipient> GetFilteredPhonebookContacts(string phrase)
        {
            var selection = string.Format("{0} like \"%{2}%\" OR {1} like \"%{2}%\" COLLATE NOCASE", ContactsNameColumn, ContactEmailColumn, phrase);
            return GetAndroidContacts(selection);
        }

        #endregion

        #region Helper methods

        List<Recipient> GetAndroidContacts(string selection = null)
        {
            var phonebookContacts = new List<Recipient>();
            var permissionCheck = ContextCompat.CheckSelfPermission(Android.App.Application.Context, Manifest.Permission.ReadContacts);

            if (permissionCheck != Android.Content.PM.Permission.Granted)
                return null;

            string[] projection =
            {
                    ContactIdColumn,
                    ContactsNameColumn,
                    ContactEmailColumn,
                };

            var cursor = Android.App.Application.Context.ContentResolver.Query(contactsUri, projection, selection, null, null);

            if (cursor.MoveToFirst())
            {
                var contactIdIndex = cursor.GetColumnIndex(ContactIdColumn);
                var contactsNameIndex = cursor.GetColumnIndex(ContactsNameColumn);
                var contactsEmailIndex = cursor.GetColumnIndex(ContactEmailColumn);

                do
                {
                    var contactId = cursor.GetLong(contactIdIndex);
                    var contactName = cursor.GetString(contactsNameIndex);
                    var contactEmail = cursor.GetString(contactsEmailIndex);

                    var phonebookContact = new Recipient
                    {
                        Type = RecipientType.Phonebook,
                        Address = contactEmail,
                        Name = contactName,
                    };

                    phonebookContacts.Add(phonebookContact);

                } while (cursor.MoveToNext());

                cursor.Close();
            }

            return phonebookContacts;
        }

        #endregion
    }
}