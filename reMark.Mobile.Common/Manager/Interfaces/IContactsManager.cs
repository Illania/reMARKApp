using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.Containers;
using reMark.Mobile.Classes.Enum;
using Contact = reMark.Mobile.Common.Model.Contact;

namespace reMark.Mobile.Common.Manager
{
    public interface IContactsManager
    {
        int MaxToFetch { get; set; }

        Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId = -1, SourceType sourceType = SourceType.Auto);

        void GetAllContactPreviews(Folder folder, Action<List<ContactPreview>> callback, Action finishedCallback, Action<Exception> errorCallback, int startId = -1, CancellationToken ct = default(CancellationToken), SourceType sourceType = SourceType.Auto);

        Task<Contact> GetContactAsync(Folder folder, int contactId, SourceType sourceType = SourceType.Auto);

        Task<Contact> GetContactAsync(int? folderId, int contactId, SourceType sourceType = SourceType.Auto);

        Task<ContactContainer> GetContactWithPreviewAsync(Folder folder, int contactId, SourceType sourceType = SourceType.Auto);

        Task<ContactContainer> GetContactWithPreviewAsync(int? folderId, int contactId, SourceType sourceType = SourceType.Auto);

        Task<bool> CreateOrUpdateContactAsync(Contact contact, ContactPreview contactPreview, int parentObjectId, SourceType sourceType = SourceType.Auto);

        Task<List<Category>> GetAllCategoriesAsync(SourceType sourceType = SourceType.Auto);

        Task<Comment> AddComment(Contact contact, string content, SourceType sourceType = SourceType.Auto);

        Task<bool> EditComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto);

        Task DeleteComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto);

        Task<List<Recipient>> GetSuggestions(string phrase);
    }
}