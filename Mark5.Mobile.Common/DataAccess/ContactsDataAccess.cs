//
// Project: Mark5.Mobile.Common
// File: ContactsDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Links;

namespace Mark5.Mobile.Common.DataAccess
{

    class ContactsDataAccess : IContactsDataAccess
    {

        readonly DatabaseConnectionProvider contactsDatabase;

        public ContactsDataAccess(DatabaseConnectionProvider contactsDatabase)
        {
            this.contactsDatabase = contactsDatabase;
        }

        public async Task SaveContactPreviewsAsync(Folder folder, List<ContactPreview> contactPreviews, bool clean)
        {
            await contactsDatabase.RunInConnectionAsync(c =>
            {
                var contactPreviewIds = contactPreviews.Select(cp => cp.Id).ToList();

                if (clean)
                {
                    c.Table<FolderContactLink>()
                     .Delete(fdl => fdl.FolderId == folder.Id && contactPreviewIds.Contains(fdl.ContactId));
                }

                c.InsertOrReplace(contactPreviews.Select(cp => new FolderContactLink { FolderId = folder.Id, ContactId = cp.Id }));
                c.InsertOrReplace(contactPreviews);
            });
        }
        public async Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId, int maxItems)
        {
            List<ContactPreview> contactPreviews = null;

            await contactsDatabase.RunInConnectionAsync(c =>
            {
                var query = c.Table<FolderContactLink>()
                             .Where(fdl => fdl.FolderId == folder.Id)
                             .Join(c.Table<ContactPreview>(), fdl => fdl.ContactId, cp => cp.Id, (fdl, cp) => cp)
                             .OrderBy(cp => cp.Name);

                if (startRowId > 0)
                {
                    query = query.Skip(startRowId);
                }

                if (maxItems > 0)
                {
                    query = query.Take(maxItems);
                }

                var result = query.ToList();

                if (result == null || result.Count < 1)
                {
                    throw new DataNotFoundException("Contact previews could not be found.");
                }

                startRowId = startRowId < 1 ? 1 : startRowId;
                foreach (var contactPreview in contactPreviews)
                {
                    contactPreview.RowId = startRowId++;
                }

                contactPreviews = result;
            });

            return contactPreviews;
        }

        public async Task SaveContactAsync(Contact contact)
        {
            await contactsDatabase.RunInConnectionAsync(c =>
            {
                c.InsertOrReplace(contact);
            });
        }

        public async Task<Contact> GetContactAsync(int contactId)
        {
            Contact contact = null;

            await contactsDatabase.RunInConnectionAsync(c =>
            {
                var result = c.Find<Contact>(contactId);

                if (result == null)
                {
                    throw new DataNotFoundException("Contact could not be found.");
                }

                contact = result;
            });

            return contact;
        }
    }
}

