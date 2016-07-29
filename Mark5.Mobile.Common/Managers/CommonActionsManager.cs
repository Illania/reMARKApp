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

        Task CopyToFolder(List<IBusinessEntity> businessEntity, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {

            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        Task MoveToFolder(List<IBusinessEntity> businessEntity, Folder fromFolder, Folder toFolder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {

            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        Task CopyToWorktray(List<BusinessEntity> businessEntity, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {

            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        Task CopyToUserWorktray(List<BusinessEntity> businessEntity, SystemUser systemUser, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {

            }

            throw new ArgumentException("Invalid sourceType provided.");
        }


        Task RemoveFromFolder(List<BusinessEntity> businessEntity, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {

            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        Task Delete(List<BusinessEntity> businessEntity, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {

            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}

