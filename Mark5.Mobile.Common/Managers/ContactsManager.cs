using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Managers
{
    class ContactsManager : AbstractManager, IContactsManager
    {
        public int MaxToFetch { get; set; } = 100;

        readonly IContactsDataAccess contactsDataAccess;

        public ContactsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IContactsDataAccess contactsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.contactsDataAccess = contactsDataAccess;
        }

        public async Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId = -1, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetContactPreviewsAsync(new DataContract.GetContactPreviewsParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    StartRowId = startRowId,
                    MaxToFetch = MaxToFetch
                });

                var contactPreviews = result.ContactPreviews.WhereNotNull().OrderBy(cp => cp.RowId).Select(cp => cp.Convert()).ToList();

                await contactsDataAccess.SaveContactPreviewsAsync(folder, contactPreviews, startRowId <= 0);

                return contactPreviews;
            }

            if (sourceType == SourceType.Local)
                return await contactsDataAccess.GetContactPreviewsAsync(folder, startRowId, MaxToFetch);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public void GetAllContactPreviews(Folder folder, Action<List<ContactPreview>> callback, Action finishedCallback, Action<Exception> errorCallback, int startRowId = -1, CancellationToken ct = default(CancellationToken), SourceType sourceType = SourceType.Auto)
        {
            Task.Run(async () =>
                {
                    var stopLoop = false;

                    while (!stopLoop && !ct.IsCancellationRequested)
                    {
                        var previews = await GetContactPreviewsAsync(folder, startRowId, sourceType);

                        if (ct.IsCancellationRequested)
                            continue;

                        callback(previews);

                        if (previews.Count > 0)
                            startRowId = previews.LastOrDefault()?.RowId + 1 ?? -1;
                        stopLoop = previews.Count < MaxToFetch;
                    }
                })
                .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            errorCallback(t.Exception.InnerException);
                        finishedCallback();
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task<Contact> GetContactAsync(Folder folder, int contactId, SourceType sourceType = SourceType.Auto)
        {
            return await GetContactAsync(folder?.Id, contactId, sourceType);
        }

        public async Task<Contact> GetContactAsync(int? folderId, int contactId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetContactAsync(new DataContract.GetContactParameters
                {
                    Token = Token,
                    FolderId = folderId ?? -1,
                    ContactId = contactId,
                    IncludePreview = false
                });

                var contact = result.Contact.Convert();

                await contactsDataAccess.SaveContactAsync(contact);

                return contact;
            }

            if (sourceType == SourceType.Local)
                return await contactsDataAccess.GetContactAsync(contactId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<ContactContainer> GetContactWithPreviewAsync(Folder folder, int contactId, SourceType sourceType = SourceType.Auto)
        {
            return await GetContactWithPreviewAsync(folder?.Id, contactId, sourceType);
        }

        public async Task<ContactContainer> GetContactWithPreviewAsync(int? folderId, int contactId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetContactAsync(new DataContract.GetContactParameters
                {
                    Token = Token,
                    FolderId = folderId ?? -1,
                    ContactId = contactId,
                    IncludePreview = true
                });

                var contact = result.Contact.Convert();
                var contactPreview = result.ContactPreview.Convert();

                var container = new ContactContainer(contactPreview, contact);

                await contactsDataAccess.SaveContactWithPreviewAsync(container);

                return container;
            }

            if (sourceType == SourceType.Local)
                return await contactsDataAccess.GetContactWithPreviewAsync(contactId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> CreteOrUpdateContactAsync(Contact contact, ContactPreview contactPreview, int parentObjectId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.CreateOrUpdateContactAsync(new DataContract.CreateOrUpdateContactParameters
                {
                    Token = Token,
                    Contact = contact.Convert(),
                    ContactPreview = contactPreview.Convert(),
                    ParentObjectId = parentObjectId
                });

                contact.Id = result.Id;
                contact.Guid = result.Guid;

                contactPreview.Id = result.Id;
                contactPreview.Guid = result.Guid;

                return result.Updated;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task<List<Category>> GetAllCategoriesAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetAllCategoriesAsync(new DataContract.GetAllCategoriesParameters
                {
                    Token = Token,
                    ObjectType = DataContract.ObjectType.Contact
                });

                var categories = result.Categories.WhereNotNull().Select(c => c.Convert()).ToList();

                await contactsDataAccess.SaveAllCategories(categories);

                return categories;
            }

            if (sourceType == SourceType.Local)
                return await contactsDataAccess.GetAllCategoriesAsync();

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetCategoriesAsync(ContactPreview contactPreview, List<Category> categories, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetCategoriesAsync(new DataContract.SetCategoriesParameters
                {
                    Token = Token,
                    ObjectId = contactPreview.Id,
                    ObjectType = DataContract.ObjectType.Contact,
                    CategoryIds = categories.Select(c => c.Id).ToArray()
                });

                contactPreview.Categories.Clear();
                contactPreview.Categories.AddRange(categories);

                await contactsDataAccess.SetCategoriesAsync(contactPreview, categories);

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Comment> AddComment(Contact contact, string content, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.AddCommentAsync(new DataContract.AddCommentParameters
                {
                    Token = Token,
                    ObjectId = contact.Id,
                    ObjectType = DataContract.ObjectType.Contact,
                    Content = content
                });

                var comment = result.Comment.Convert();
                contact.Comments.Add(comment);

                await contactsDataAccess.AddCommentAsync(contact, comment);

                return comment;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> EditComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.EditCommentAsync(new DataContract.EditCommentParameters
                {
                    Token = Token,
                    CommentId = comment.Id,
                    ObjectId = contact.Id,
                    ObjectType = DataContract.ObjectType.Contact,
                    Content = comment.Content
                });

                var editSuccess = result.EditSuccess;

                if (editSuccess)
                {
                    await contactsDataAccess.EditCommentAsync(contact, comment);
                    var index = contact.Comments.FindIndex(c => c.Id == comment.Id);
                    if (index >= 0)
                        contact.Comments[index] = comment;
                }
                return editSuccess;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task DeleteComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.DeleteCommentAsync(new DataContract.DeleteCommentParameters
                {
                    Token = Token,
                    CommentId = comment.Id,
                    ObjectId = contact.Id,
                    ObjectType = DataContract.ObjectType.Contact
                });

                contact.Comments.Remove(comment);

                await contactsDataAccess.DeleteCommentAsync(contact, comment);

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Recipient>> GetSuggestions(string phrase)
        {
            var result = await contactsDataAccess.GetSuggestions(phrase);
            return result;
        }
    }
}