//
// Project: Mark5.Mobile.Common
// File: CommonActionsManager.cs
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

    class CommonActionsManager : AbstractManager, ICommonActionsManager
    {

        readonly IDocumentsDataAccess documentsDataAccess;
        readonly IContactsDataAccess contactsDataAccess;
        readonly IShortcodesDataAccess shortcodesDataAccess;
        readonly ICalendarDataAccess calendarDataAccess;

        public CommonActionsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IDocumentsDataAccess documentsDataAccess, IContactsDataAccess contactsDataAccess, IShortcodesDataAccess shortcodesDataAccess, ICalendarDataAccess calendarDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.documentsDataAccess = documentsDataAccess;
            this.contactsDataAccess = contactsDataAccess;
            this.shortcodesDataAccess = shortcodesDataAccess;
            this.calendarDataAccess = calendarDataAccess;
        }

        public async Task<List<ObjectAction>> GetObjectActionsAsync(IBusinessEntity businessEntity, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetObjectActionsAsync(new DataContract.GetObjectActionsParameters
                {
                    Token = Token,
                    ObjectId = businessEntity.Id,
                    ObjectType = businessEntity.ObjectType.ConvertEnum<DataContract.ObjectType>()
                });

                return result.ObjectActions.WhereNotNull().Select(oa => oa.Convert()).ToList();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<ObjectLink>> GetObjectLinksAsync(IBusinessEntity businessEntity, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetObjectLinksAsync(new DataContract.GetObjectLinksParameters
                {
                    Token = Token,
                    ObjectId = businessEntity.Id,
                    ObjectType = businessEntity.ObjectType.ConvertEnum<DataContract.ObjectType>()
                });

                return result.ObjectLinks.WhereNotNull().Select(ol => ol.Convert()).ToList();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task CopyToFolder(List<IBusinessEntity> businessEntity, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.FileToFolderAsync(new DataContract.FileToFolderParameters
                {
                    Token = Token,
                    ObjectIds = businessEntity.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntity.First().ObjectType.ConvertEnum<DataContract.ObjectType>(),
                    ToFolderId = folder.Id,
                    Move = false
                });
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task MoveToFolder(List<IBusinessEntity> businessEntity, Folder fromFolder, Folder toFolder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.FileToFolderAsync(new DataContract.FileToFolderParameters
                {
                    Token = Token,
                    ObjectIds = businessEntity.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntity.First().ObjectType.ConvertEnum<DataContract.ObjectType>(),
                    FromFolderId = fromFolder.Id,
                    ToFolderId = toFolder.Id,
                    Move = true
                });
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task CopyToWorktray(List<BusinessEntity> businessEntity, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.CopyToWorktrayAsync(new DataContract.CopyToWorktrayParameters
                {
                    Token = Token,
                    ObjectIds = businessEntity.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntity.First().ObjectType.ConvertEnum<DataContract.ObjectType>()
                });
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task CopyToUserWorktray(List<BusinessEntity> businessEntity, List<SystemUser> systemUsers, string comment = null, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.CopyToWorktrayAsync(new DataContract.CopyToWorktrayParameters
                {
                    Token = Token,
                    ObjectIds = businessEntity.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntity.First().ObjectType.ConvertEnum<DataContract.ObjectType>(),
                    UserIds = systemUsers.Select(su => su.Id).ToArray(),
                    Comment = comment
                });
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task RemoveFromFolder(List<BusinessEntity> businessEntity, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.RemoveFromFolderAsync(new DataContract.RemoveFromFolderParameters
                {
                    Token = Token,
                    ObjectIds = businessEntity.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntity.First().ObjectType.ConvertEnum<DataContract.ObjectType>(),
                    FolderId = folder.Id
                });
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task Delete(List<BusinessEntity> businessEntity, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.DeleteAsync(new DataContract.DeleteParameters
                {
                    Token = Token,
                    ObjectIds = businessEntity.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntity.First().ObjectType.ConvertEnum<DataContract.ObjectType>()
                });
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}

