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
    }
}

