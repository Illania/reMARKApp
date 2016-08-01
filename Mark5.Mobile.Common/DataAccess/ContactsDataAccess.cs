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

        public async Task RemoveFromFolderAsync(List<ContactPreview> contactPreviews, Folder folder)
        {
            var ids = contactPreviews.Select(cp => cp.Id).Distinct().ToList();
            await RemoveFromFolderAsync(ids, folder.Id);
        }

        public async Task RemoveFromFolderAsync(List<Contact> contacts, Folder folder)
        {
            var ids = contacts.Select(c => c.Id).Distinct().ToList();
            await RemoveFromFolderAsync(ids, folder.Id);
        }

        async Task RemoveFromFolderAsync(List<int> ids, int folderId)
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
                    }

                    c.Table<FolderContactLink>().Delete(fcl => fcl.ContactId == id && fcl.FolderId == folderId);
                }
            });
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
            await contactsDatabase.RunInConnectionAsync(c =>
            {
                c.Table<FolderContactLink>().Delete(fcl => ids.Contains(fcl.ContactId));
                c.Table<ContactPreview>().Delete(cp => ids.Contains(cp.Id));
                c.Table<Contact>().Delete(ct => ids.Contains(ct.Id));
            });
        }

        public async Task SaveAllCategories(List<Category> categories)
        {
            await contactsDatabase.RunInConnectionAsync(c =>
            {
                c.DeleteAll<Category>();
                c.InsertAll(categories);
            });
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            List<Category> categories = null;

            await contactsDatabase.RunInConnectionAsync(c =>
            {
                categories = c.Table<Category>().ToList();
            });

            return categories;
        }

        public async Task SetCategoriesAsync(ContactPreview contactPreview, List<Category> categories)
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

        public async Task AddCommentAsync(Contact contact, Comment comment)
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
                    throw new DataNotFoundException("Contact could not be found.");
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

        public async Task EditCommentAsync(Contact contact, Comment comment)
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
                    throw new DataNotFoundException("Contact could not be found.");
                }

                var comments = result.First().Comments;

                comments.RemoveAll(cm => cm.Id == comment.Id);
                comments.Add(comment);
                comments = comments.OrderBy(cm => cm.DateAdded).ToList();

                cmd = c.CreateCommand($"update \"{nameof(Contact)}\" " +
                                      $"set \"{nameof(Contact.CommentsBytes)}\" = @commentsBytes " +
                                      $"where \"{nameof(Contact.Id)}\" = @contactId");
                cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
                cmd.Bind("@contactId", contact.Id);
                cmd.ExecuteNonQuery();
            });
        }

        public async Task DeleteCommentAsync(Contact contact, Comment comment)
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
                    throw new DataNotFoundException("Contact could not be found.");
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
    }
}

