//
// Project: Mark5.Mobile.Common
// File: IContactsManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT

using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Managers
{

    public interface IContactsManager
    {

        Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId = -1, int maxItems = 500, SourceType sourceType = SourceType.Auto);

        Task<Contact> GetContactAsync(Folder folder, int contactId, SourceType sourceType = SourceType.Auto);

        Task<List<Category>> GetAllCategoriesAsync(SourceType sourceType = SourceType.Auto);

        Task SetCategoriesAsync(ContactPreview documentPreview, List<Category> categories, SourceType sourceType = SourceType.Auto);

        Task<Comment> AddComment(Contact contact, string content, SourceType sourceType = SourceType.Auto);

        Task<bool> EditComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto);

        Task DeleteComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto);
    }
}

