//
// Project: Mark5.Mobile.Common
// File: ContactsDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;
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
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    if (clean)
                    {
                        c.Table<FolderContactLink>()
                         .Delete(fdl => fdl.FolderId == folder.Id);
                    }

                    c.InsertOrReplaceAll(contactPreviews.Select(cp => new FolderContactLink { FolderId = folder.Id, ContactId = cp.Id }));
                    c.InsertOrReplaceAll(contactPreviews);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error inserting contacts.", ex);
            }
        }

        public async Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId, int maxItems)
        {
            try
            {
                List<ContactPreview> contactPreviews = null;

                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select * " +
                                $"from {nameof(ContactPreview)}, {nameof(FolderContactLink)} " +
                                $"where {nameof(FolderContactLink.FolderId)} = {folder.Id} " +
                                $"     and {nameof(ContactPreview)}.{nameof(ContactPreview.Id)} = {nameof(FolderContactLink)}.{nameof(FolderContactLink.ContactId)} ";

                    query += $"order by {nameof(ContactPreview.Name)} ";

                    if (maxItems > 0)
                    {
                        query += $"limit {maxItems - 1} ";
                    }

                    if (startRowId > 0)
                    {
                        query += $"offset {startRowId} ";
                    }

                    var result = c.Query<ContactPreview>(query);

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
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting contacts.", ex);
            }
        }

        public async Task SaveContactAsync(Contact contact)
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(contact);

                    c.Table<ContactCommunicationAddress>().Delete(ca => ca.ContactId == contact.Id);

                    var contactCommunicationAddresses = contact.CommunicationAddresses
                                                               .Select(ca => new ContactCommunicationAddress(contact.Id, ca.Address, ca.Type, ca.Description, ca.IsPrimary));
                    c.InsertAll(contactCommunicationAddresses);

                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving contact.", ex);
            }
        }

        public async Task<Contact> GetContactAsync(int contactId)
        {
            try
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

                    var cmd = c.CreateCommand($"select \"{nameof(ContactCommunicationAddress.Address)}\", " +
                                              $"\"{nameof(ContactCommunicationAddress.Description)}\"," +
                                              $" \"{nameof(ContactCommunicationAddress.IsPrimary)}\", " +
                                              $"\"{nameof(ContactCommunicationAddress.Type)}\" " +
                                              $"from \"{nameof(ContactCommunicationAddress)}\" " +
                                              $"where \"{nameof(ContactCommunicationAddress.ContactId)}\" = @contactId");
                    cmd.Bind("@contactId", contactId);
                    var addresses = cmd.ExecuteQuery<CommunicationAddress>();

                    contact.CommunicationAddresses.AddRange(addresses);

                });

                return contact;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting contact.", ex);
            }
        }

        public async Task SaveContactWithPreviewAsync(ContactContainer container)
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(container.Contact);
                    c.Table<ContactCommunicationAddress>().Delete(ca => ca.ContactId == container.Contact.Id);

                    var contactCommunicationAddresses = container.Contact.CommunicationAddresses
                                                               .Select(ca => new ContactCommunicationAddress(container.Contact.Id, ca.Address, ca.Type, ca.Description, ca.IsPrimary));
                    c.InsertAll(contactCommunicationAddresses);


                    c.InsertOrReplace(container.ContactPreview);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving contact with preview.", ex);
            }
        }

        public async Task<ContactContainer> GetContactWithPreviewAsync(int contactId)
        {
            try
            {
                ContactContainer container = null;

                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var contact = c.Find<Contact>(contactId);
                    if (contact == null)
                    {
                        throw new DataNotFoundException("Contact could not be found.");
                    }

                    var cmd = c.CreateCommand($"select \"{nameof(ContactCommunicationAddress.Address)}\", " +
                          $"\"{nameof(ContactCommunicationAddress.Description)}\"," +
                          $" \"{nameof(ContactCommunicationAddress.IsPrimary)}\", " +
                          $"\"{nameof(ContactCommunicationAddress.Type)}\" " +
                          $"from \"{nameof(ContactCommunicationAddress)}\" " +
                          $"where \"{nameof(ContactCommunicationAddress.ContactId)}\" = @contactId");
                    cmd.Bind("@contactId", contactId);
                    var addresses = cmd.ExecuteQuery<CommunicationAddress>();

                    contact.CommunicationAddresses.AddRange(addresses);

                    var contactPreview = c.Find<ContactPreview>(contactId);
                    if (contactPreview == null)
                    {
                        throw new DataNotFoundException("Contact preview could not be found.");
                    }

                    container = new ContactContainer(contactPreview, contact);
                });

                return container;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting contact with preview.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<ContactPreview> contactPreviews, Folder folder)
        {
            var ids = contactPreviews.Select(cp => cp.Id).Distinct().ToList();
            await RemoveFromFolderAsync(ids, folder.Id);
        }

        public async Task RemoveFromFolderAsync(List<Contact> contacts, Folder folder) //TODO remember to delete also addresses
        {
            var ids = contacts.Select(c => c.Id).Distinct().ToList();
            await RemoveFromFolderAsync(ids, folder.Id);
        }

        async Task RemoveFromFolderAsync(List<int> ids, int folderId)
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var id in ids)
                    {
                        var linksCount = c.Table<FolderContactLink>().Count(fdl => fdl.ContactId == id);
                        if (linksCount == 1)
                        {
                            c.Table<ContactPreview>().Delete(cp => cp.Id == id);
                            c.Table<Contact>().Delete(ct => ct.Id == id);
                            c.Table<ContactCommunicationAddress>().Delete(ca => ca.ContactId == id);
                        }

                        c.Table<FolderContactLink>().Delete(fcl => fcl.ContactId == id && fcl.FolderId == folderId);
                    }
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing contacts from folders.", ex);
            }
        }

        public async Task DeleteAsync(List<ContactPreview> contactPreviews)
        {
            var ids = contactPreviews.Select(cp => cp.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        public async Task DeleteAsync(List<Contact> contacts)
        {
            var ids = contacts.Select(c => c.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        async Task DeleteAsync(List<int> ids)
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    c.Table<FolderContactLink>().Delete(fcl => ids.Contains(fcl.ContactId));
                    c.Table<ContactPreview>().Delete(cp => ids.Contains(cp.Id));
                    c.Table<Contact>().Delete(ct => ids.Contains(ct.Id));
                    c.Table<ContactCommunicationAddress>().Delete(ca => ids.Contains(ca.ContactId));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting contacts.", ex);
            }
        }

        public async Task SaveAllCategories(List<Category> categories)
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    c.DeleteAll<Category>();
                    c.InsertAll(categories);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving categories.", ex);
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                List<Category> categories = null;

                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    categories = c.Table<Category>().ToList();
                });

                return categories;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting categories.", ex);
            }
        }

        public async Task SetCategoriesAsync(ContactPreview contactPreview, List<Category> categories)
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"update \"{nameof(ContactPreview)}\" " +
                                              $"set \"{nameof(ContactPreview.CategoriesBytes)}\" = @categoriesBytes " +
                                              $"where \"{nameof(ContactPreview.Id)}\" = @contactPreviewId");
                    cmd.Bind("@categoriesBytes", new CategoriesValue { Categories = categories }.CategoriesBytes);
                    cmd.Bind("@contactPreviewId", contactPreview.Id);
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error setting categories.", ex);
            }
        }

        public async Task AddCommentAsync(Contact contact, Comment comment)
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"select \"{nameof(Contact.CommentsBytes)}\" " +
                                              $"from \"{nameof(Contact)}\" " +
                                              $"where \"{nameof(Contact.Id)}\" = @contactId");
                    cmd.Bind("@contactId", contact.Id);
                    var result = cmd.ExecuteQuery<CommentsValue>();

                    if (result == null || result.Count < 1)
                    {
                        return;
                    }

                    var comments = result.First().Comments;

                    comments.Add(comment);

                    cmd = c.CreateCommand($"update \"{nameof(Contact)}\" " +
                                          $"set \"{nameof(Contact.CommentsBytes)}\" = @commentsBytes " +
                                          $"where \"{nameof(Contact.Id)}\" = @contactId");
                    cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
                    cmd.Bind("@contactId", contact.Id);
                    cmd.ExecuteNonQuery();

                    cmd = c.CreateCommand($"update \"{nameof(ContactPreview)}\" " +
                                          $"set \"{nameof(ContactPreview.CommentsCount)}\" = @commentsCount " +
                                          $"where \"{nameof(ContactPreview.Id)}\" = @contactId");
                    cmd.Bind("@commentsCount", comments.Count);
                    cmd.Bind("@contactId", contact.Id);
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error adding comment.", ex);
            }
        }

        public async Task EditCommentAsync(Contact contact, Comment comment)
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"select \"{nameof(Contact.CommentsBytes)}\" " +
                                    $"from \"{nameof(Contact)}\" " +
                                    $"where \"{nameof(Contact.Id)}\" = @contactId");
                    cmd.Bind("@contactId", contact.Id);
                    var result = cmd.ExecuteQuery<CommentsValue>();

                    if (result == null || result.Count < 1)
                    {
                        return;
                    }

                    var comments = result.First().Comments;

                    comments.RemoveAll(cm => cm.Id == comment.Id);
                    comments.Add(comment);
                    comments = comments.OrderBy(cm => cm.DateAddedTimestamp).ToList();

                    cmd = c.CreateCommand($"update \"{nameof(Contact)}\" " +
                                          $"set \"{nameof(Contact.CommentsBytes)}\" = @commentsBytes " +
                                          $"where \"{nameof(Contact.Id)}\" = @contactId");
                    cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
                    cmd.Bind("@contactId", contact.Id);
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error editting comment.", ex);
            }
        }

        public async Task DeleteCommentAsync(Contact contact, Comment comment)
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"select \"{nameof(Contact.CommentsBytes)}\" " +
                                    $"from \"{nameof(Contact)}\" " +
                                    $"where \"{nameof(Contact.Id)}\" = @contactId");
                    cmd.Bind("@contactId", contact.Id);
                    var result = cmd.ExecuteQuery<CommentsValue>();

                    if (result == null || result.Count < 1)
                    {
                        return;
                    }

                    var comments = result.First().Comments;

                    comments.RemoveAll(cm => cm.Id == comment.Id);

                    cmd = c.CreateCommand($"update \"{nameof(Contact)}\" " +
                                          $"set \"{nameof(Contact.CommentsBytes)}\" = @commentsBytes " +
                                          $"where \"{nameof(Contact.Id)}\" = @contactId");
                    cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
                    cmd.Bind("@contactId", contact.Id);
                    cmd.ExecuteNonQuery();

                    cmd = c.CreateCommand($"update \"{nameof(ContactPreview)}\" " +
                                          $"set \"{nameof(ContactPreview.CommentsCount)}\" = @commentsCount " +
                                          $"where \"{nameof(ContactPreview.Id)}\" = @contactId");
                    cmd.Bind("@commentsCount", comments.Count);
                    cmd.Bind("@contactId", contact.Id);
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting comment.", ex);
            }
        }

        public async Task<IEnumerable<int>> GetPendingFolders()
        {
            try
            {
                var fIds = new List<int>();

                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var queryString = $"select {nameof(FolderContactLink.FolderId)} " +
                        $"from {nameof(FolderContactLink)} " +
                        $"where {nameof(FolderContactLink.ContactId)} not in (select {nameof(Contact.Id)} from {nameof(Contact)})";

                    var result = c.Query<int>(queryString);

                    fIds = result.ToList();
                });

                return fIds;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while getting pending folders id for contacts.", ex);
            }
        }

        public async Task<bool> IsContactCached(int contactId)
        {
            try
            {
                bool found = false;

                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var result = c.Table<Contact>().Count(co => co.Id == contactId);
                    found = result >= 1;
                });

                return found;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while checking contact existence.", ex);
            }
        }

        public async Task<IEnumerable<int>> GetPendingContactsId(int folderId)
        {
            try
            {
                var contactsId = new List<int>();

                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var folderCondition = $"{ nameof(FolderContactLink.FolderId)} = ? ";
                    var inCondition = $"{ nameof(FolderContactLink.ContactId) }   not in (select { nameof(Contact.Id)}   from { nameof(Contact)})";
                    var queryString = $"select {nameof(FolderContactLink.ContactId)} from {nameof(FolderContactLink)} " +
                        $"where {folderCondition} and {inCondition}";

                    var result = c.Query<int>(queryString, folderId);
                    contactsId = result.ToList();
                });

                return contactsId;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while getting pending contacts id.", ex);
            }
        }

        public async Task RemoveOrphans()
        {
            try
            {
                await contactsDatabase.RunInConnectionAsync(c =>
                {
                    var innerSelectQueryText = $"select {nameof(FolderContactLink.ContactId)} from {nameof(FolderContactLink)}";

                    var outerDeleteQueryContactPreview = $"delete from {nameof(ContactPreview)} where {nameof(ContactPreview.Id)} not in ({innerSelectQueryText}) ";
                    var cmd = c.CreateCommand(outerDeleteQueryContactPreview);
                    cmd.ExecuteNonQuery();

                    var outerDeleteQueryContact = $"delete from {nameof(Contact)} where {nameof(Contact.Id)} not in ({innerSelectQueryText}) ";
                    var cmd2 = c.CreateCommand(outerDeleteQueryContact);
                    cmd2.ExecuteNonQuery();

                    var outerDeleteQueryContactCommunicationAddresses = $"delete from {nameof(ContactCommunicationAddress)} where {nameof(ContactCommunicationAddress.ContactId)} not in ({innerSelectQueryText}) ";
                    var cmd3 = c.CreateCommand(outerDeleteQueryContactCommunicationAddresses);
                    cmd3.ExecuteNonQuery();
                });

            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing orphan contacts and contact previews.", ex);
            }
        }
    }
}

