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
using Mark5.Mobile.Common.Model.Containers;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.DataAccess
{

    interface IContactsDataAccess
    {

        Task SaveContactPreviewsAsync(Folder folder, List<ContactPreview> contactPreviews, bool clean);

        Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId, int maxItems);

        Task SaveContactAsync(Contact contact);

        Task SaveContactWithPreviewAsync(ContactContainer container);

        Task<Contact> GetContactAsync(int contactId);

        Task<ContactContainer> GetContactWithPreviewAsync(int contactId);

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

        Task<IEnumerable<int>> GetPendingFolders();

        Task<IEnumerable<int>> GetPendingContactsId(int folderId);

        Task<bool> IsContactCached(int contactId);

        Task RemoveOrphans();

        Task<List<PrintableSuggestion>> GetSuggestions(string phrase);
    }
}

