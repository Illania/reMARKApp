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
    }
}

