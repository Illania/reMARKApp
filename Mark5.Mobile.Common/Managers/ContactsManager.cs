//
// Project: Mark5.Mobile.Common
// File: ContactsManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Managers
{

    class ContactsManager : AbstractManager, IContactsManager
    {

        readonly IContactsDataAccess contactsDataAccess;

        public ContactsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IContactsDataAccess contactsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.contactsDataAccess = contactsDataAccess;
        }

        public async Task<List<ContactPreview>> GetContactPreviewsAsync(Folder folder, int startRowId = -1, int maxItems = 500, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetContactPreviewsAsync(new DataContract.GetContactPreviewsParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    StartRowId = startRowId,
                    MaxToFetch = maxItems
                });

                var contactPreviews = result.ContactPreviews.WhereNotNull().OrderBy(cp => cp.RowId).Select(cp => cp.Convert()).ToList();

                await contactsDataAccess.SaveContactPreviewsAsync(folder, contactPreviews, startRowId == -1);

                return contactPreviews;
            }

            if (sourceType == SourceType.Local)
            {
                return await contactsDataAccess.GetContactPreviewsAsync(folder, startRowId, maxItems);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Contact> GetContactAsync(Folder folder, int contactId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetContactAsync(new DataContract.GetContactParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    ContactId = contactId,
                    IncludePreview = false
                });

                var contact = result.Contact.Convert();

                await contactsDataAccess.SaveContactAsync(contact);

                return contact;
            }

            if (sourceType == SourceType.Local)
            {
                return await contactsDataAccess.GetContactAsync(contactId);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Comment> AddComment(Contact contact, string content, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.AddCommentAsync(new DataContract.AddCommentParameters
                {
                    Token = Token,
                    ObjectId = contact.Id,
                    ObjectType = DataContract.ObjectType.Contact,
                    Content = content
                });

                var comment = result.Comment.Convert();

                await contactsDataAccess.AddCommentAsync(contact, comment);

                return comment;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> EditComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
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
                }

                return editSuccess;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task DeleteComment(Contact contact, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.DeleteCommentAsync(new DataContract.DeleteCommentParameters
                {
                    Token = Token,
                    CommentId = comment.Id,
                    ObjectId = contact.Id,
                    ObjectType = DataContract.ObjectType.Contact,
                });

                await contactsDataAccess.DeleteCommentAsync(contact, comment);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}

