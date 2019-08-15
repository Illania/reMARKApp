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

        Task RemoveFromFolderAsync(List<ContactPreview> contactPreviews, Folder folder);

        Task RemoveFromFolderAsync(List<Contact> contacts, Folder folder);

        Task RemoveFromFolderAsync(List<int> conIds, int folderId);

        Task DeleteAsync(List<ContactPreview> contactPreviews);

        Task DeleteAsync(List<Contact> contacts);

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