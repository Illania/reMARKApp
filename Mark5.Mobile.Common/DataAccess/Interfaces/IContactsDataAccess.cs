using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;

namespace Mark5.Mobile.Common.DataAccess
{
    interface IContactsDataAccess
    {
        Task SaveContactPreviewsAsync(Folder folder, List<ContactPreview> contactPreviews, bool clean);

        Task SaveContactPreviewsAsync(List<ContactPreview> contactPreviews);

        Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId, int maxItems);

        Task SaveContactAsync(Contact contact);

        Task SaveContactWithPreviewAsync(ContactContainer container);

        Task<Contact> GetContactAsync(int contactId);

        Task<ContactContainer> GetContactWithPreviewAsync(int contactId);

        Task RemoveFromFolderAsync(List<ContactPreview> contactPreviews, int folderId, bool saveBeforeDeletion = false);

        Task RemoveFromFolderAsync(List<Contact> contacts, int folderId, bool saveBeforeDeletion = false);

        Task DeleteAsync(List<ContactPreview> contactPreviews, bool saveBeforeDeletion = false);

        Task DeleteAsync(List<Contact> contacts, bool saveBeforeDeletion = false);

        Task DeleteAsync(List<int> contactsIds);

        Task SaveAllCategories(List<Category> categories);

        Task<List<Category>> GetAllCategoriesAsync();

        Task SetCategoriesAsync(ContactPreview contactPreview, List<Category> categories);

        Task AddCommentAsync(Contact contact, Comment comment);

        Task EditCommentAsync(Contact contact, Comment comment);

        Task DeleteCommentAsync(Contact contact, Comment comment);

        Task RemoveOrphans();

        Task<List<Recipient>> GetSuggestions(string phrase);

        Task DeleteAllAsync();
    }
}