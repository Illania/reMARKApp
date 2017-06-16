using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Contacts;
using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.IOS.Utilities
{
    public class PhonebookUtilities : IPhonebookUtils
    {
        #region IPhonebookUtilities implementation

        public List<Recipient> GetPhonebookContacts()
        {
            return GetiOSContacts();
        }

        public List<Recipient> GetFilteredPhonebookContacts(string phrase)
        {
            return GetiOSContacts(phrase);
        }

        #endregion

        #region Helper methods

        List<Recipient> GetiOSContacts(string phrase = null)
        {
            var authorizationSemaphore = new SemaphoreSlim(0, 1);

            var contacts = new List<Recipient>();

            var status = CNContactStore.GetAuthorizationStatus(CNEntityType.Contacts);

            if (status == CNAuthorizationStatus.Denied || status == CNAuthorizationStatus.Restricted)
                return null;

            using (var store = new CNContactStore())
            {
                if (status == CNAuthorizationStatus.NotDetermined)
                {
                    store.RequestAccess(CNEntityType.Contacts,
                        (granted, error) =>
                        {
                            if (granted)
                                contacts = GetContactsFromContactStore(store, phrase);
                            authorizationSemaphore.Release();
                        });
                }
                else //Authorized
                {
                    contacts = GetContactsFromContactStore(store, phrase);
                    authorizationSemaphore.Release();
                }

                authorizationSemaphore.WaitAsync().Wait();
            }

            return contacts;
        }


        List<Recipient> GetContactsFromContactStore(CNContactStore store, string phrase)
        {
            var cnContacts = new List<CNContact>();

            var keys = new[]
            {
                CNContactKey.GivenName,
                CNContactKey.FamilyName,
                CNContactKey.EmailAddresses
            };

            var containers = store.GetContainers(null, out NSError error);

            if (error != null)
                return null;

            foreach (var container in containers)
            {
                var fetchPredicate = CNContact.GetPredicateForContactsInContainer(container.Identifier);
                var cnContactsTemp = store.GetUnifiedContacts(fetchPredicate, keys, out error);

                if (error != null)
                    return null;

                cnContacts.AddRange(cnContactsTemp);
            }

            var contacts = cnContacts.Select(ConvertToContact).SelectMany(i => i);

            if (!string.IsNullOrEmpty(phrase))
                contacts = contacts.Where(c => c.Name.ContainsCaseInsensitive(phrase) || c.Address.ContainsCaseInsensitive(phrase));
            return contacts.ToList();
        }

        List<Recipient> ConvertToContact(CNContact cnContact)
        {
            var phonebookContacts = new List<Recipient>();
            var addresses = cnContact.EmailAddresses.Where(el => Validator.IsEmailValid(el.Value)).Select(el => el.Value).ToList();
            foreach (var address in addresses)
            {
                phonebookContacts.Add(new Recipient()
                {
                    Address = address,
                    Type = RecipientType.Phonebook,
                    Name = string.Join(" ", new[] { cnContact.GivenName, cnContact.FamilyName }.Where(v => !string.IsNullOrEmpty(v))),
                });
            }

            return phonebookContacts;
        }

        #endregion
    }
}