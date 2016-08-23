//
// Project: Mark5.Mobile.Common
// File: IContactsDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{

    interface IContactsDataAccess
    {

        Task SaveContactPreviewsAsync(Folder folder, List<ContactPreview> contactPreviews, bool clean);

        Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId, int maxItems);

        Task SaveContactAsync(Contact contact);

        Task<Contact> GetContactAsync(int contactId);

        Task RemoveFromFolderAsync(List<ContactPreview> contactPreviews, Folder folder);

        Task RemoveFromFolderAsync(List<Contact> contacts, Folder folder);

        Task DeleteAsync(List<ContactPreview> contactPreviews);

        Task DeleteAsync(List<Contact> contacts);

        Task SaveAllCategories(List<Category> categories);

        Task<List<Category>> GetAllCategoriesAsync();

        Task SetCategoriesAsync(ContactPreview contactPreview, List<Category> categories);

        Task AddCommentAsync(Contact contact, Comment comment);

        Task EditCommentAsync(Contact contact, Comment comment);

        Task DeleteCommentAsync(Contact contact, Comment comment);

        Task<IEnumerable<ContactDownloadInfo>> GetUnsavedContactIds(int? folderId);

        Task<bool> IsContactCached(int id);
    }
}

