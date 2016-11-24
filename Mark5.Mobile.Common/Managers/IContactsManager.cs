//
// Project: Mark5.Mobile.Common
// File: IContactsManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Managers
{

    public interface IContactsManager
    {

        int MaxToFetch { get; set; }

        Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId = -1, SourceType sourceType = SourceType.Auto);

        void GetAllContactPreviews(Folder folder, Action<List<ContactPreview>> callback, Action finishedCallback, Action<Exception> errorCallback, int startId = -1, CancellationToken ct = default(CancellationToken), SourceType sourceType = SourceType.Auto);

        Task<Contact> GetContactAsync(Folder folder, int contactId, SourceType sourceType = SourceType.Auto);

        Task<Contact> GetContactAsync(int folderId, int contactId, SourceType sourceType = SourceType.Auto);

        Task<ContactContainer> GetContactWithPreviewAsync(Folder folder, int contactId, SourceType sourceType = SourceType.Auto);

        Task<ContactContainer> GetContactWithPreviewAsync(int folderId, int contactId, SourceType sourceType = SourceType.Auto);

        Task<bool> CreteOrUpdateContactAsync(Contact contact, ContactPreview contactPreview, int parentObjectId, SourceType sourceType = SourceType.Auto);

        Task<List<Category>> GetAllCategoriesAsync(SourceType sourceType = SourceType.Auto);

        Task SetCategoriesAsync(ContactPreview documentPreview, List<Category> categories, SourceType sourceType = SourceType.Auto);

        Task<Comment> AddComment(Contact contact, string content, SourceType sourceType = SourceType.Auto);

        Task<bool> EditComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto);

        Task DeleteComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto);
    }
}

