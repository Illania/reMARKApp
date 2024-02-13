using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reMark.Mobile.Classes.Enum;
using reMark.Mobile.Common.DataAccess;
using reMark.Mobile.Common.DataAccess.Interfaces;
using reMark.Mobile.Common.Extensions;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.Actions;
using reMark.Mobile.Common.Model.Converters;
using reMark.Mobile.Common.Model.Exceptions;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.ServiceReference.AppService;
using DataContract = reMark.ServiceReference.DataContract;

namespace reMark.Mobile.Common.Manager
{
    class CommonActionsManager : AbstractManager, ICommonActionsManager
    {
        readonly IDocumentsDataAccess documentsDataAccess;
        readonly IContactsDataAccess contactsDataAccess;
        readonly IShortcodesDataAccess shortcodesDataAccess;

        IActionsManager ActionsManager => Managers.ActionsManager;

        public CommonActionsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy,
            IDocumentsDataAccess documentsDataAccess, IContactsDataAccess contactsDataAccess,
            IShortcodesDataAccess shortcodesDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.documentsDataAccess = documentsDataAccess;
            this.contactsDataAccess = contactsDataAccess;
            this.shortcodesDataAccess = shortcodesDataAccess;
        }

        public async Task<List<ObjectAction>> GetObjectActionsAsync(IBusinessEntity businessEntity,
            SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetObjectActionsAsync(new DataContract.GetObjectActionsParameters
                {
                    Token = Token,
                    ObjectId = businessEntity.Id,
                    ObjectType = businessEntity.ObjectType.ConvertEnum<DataContract.ObjectType>()
                });

                return result.ObjectActions.WhereNotNull().Select(oa => oa.Convert()).ToList();
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<ObjectLink>> GetObjectLinksAsync(IBusinessEntity businessEntity,
            SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetObjectLinksAsync(new DataContract.GetObjectLinksParameters
                {
                    Token = Token,
                    ObjectId = businessEntity.Id,
                    ObjectType = businessEntity.ObjectType.ConvertEnum<DataContract.ObjectType>()
                });

                return result.ObjectLinks.WhereNotNull().Select(ol => ol.Convert()).ToList();
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task CopyToFolder(List<IBusinessEntity> businessEntities,
            Folder folder, SourceType sourceType = SourceType.Auto)
        {
            await CopyToFolder(businessEntities.Select(bi => bi.Id).ToList(),
                businessEntities.First().ObjectType, folder.Id, sourceType);
        }

        public async Task CopyToFolder(List<int> ids, ObjectType objectType, 
            int folderId, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(
                new CopyToFolderEvent(objectType.ToModuleType(), ids.Count));

            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
                await CopyToFolderRemoteAsync(ids, folderId, objectType);
            else if (sourceType == SourceType.Local)
            {
                await ActionsManager.QueueActionAsync(
                      CopyToFolderAction.Create(folderId, objectType, ids.ToArray()));
            }
            else
                throw new ArgumentException("Invalid sourceType provided.");

            await CopyToFolderLocalAsync(ids, folderId, objectType);
        }

        internal async Task CopyToFolderRemoteAsync(List<int> ids,  int folderId, ObjectType objectType)
        {
            await AppServiceProxy.FileToFolderAsync(new DataContract.FileToFolderParameters
            {
                Token = Token,
                ObjectIds = ids.ToArray(),
                ObjectType = objectType.ConvertEnum<DataContract.ObjectType>(),
                ToFolderId = folderId,
                Move = false
            });
        }

        internal async Task CopyToFolderLocalAsync(List<int> ids, int folderId, ObjectType objectType)
        {
            var commonActionsDataAccess = GetCommonActionsDataAccess(objectType);
            if (commonActionsDataAccess is not ICopiable copiable)
                return;
            await copiable.CopyToFolderAsync(folderId, ids);
        }

        public async Task MoveToFolder(List<IBusinessEntity> businessEntities, Folder fromFolder,
            Folder toFolder, SourceType sourceType = SourceType.Auto)
        {
            await MoveToFolder(businessEntities.Select(bi => bi.Id).ToList(),
                businessEntities.First().ObjectType, fromFolder, toFolder, sourceType);
        }

        public async Task MoveToFolder(List<int> beIds, ObjectType ot, Folder fromFolder, 
            Folder toFolder, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(
                new MoveToFolderEvent(fromFolder.Module, beIds.Count));

            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
                await MoveToFolderRemoteAsync(beIds, fromFolder.Id, toFolder.Id, ot);
            else if (sourceType == SourceType.Local)
            {
                await ActionsManager.QueueActionAsync(
                      MoveToFolderAction.Create(fromFolder.Id, toFolder.Id, ot, beIds.ToArray()));
            }
            else
                throw new ArgumentException("Invalid sourceType provided.");

            await MoveToFolderLocalAsync(beIds, fromFolder.Id, toFolder.Id, ot);
            CommonConfig.MessengerHub.Publish(
                new EntityMovedFromFolderMessage(this, ot, fromFolder.Id, beIds.ToList()));
        }
        
        internal async Task MoveToFolderRemoteAsync(List<int> ids, int fromFolderId, 
            int toFolderId, ObjectType objectType)
        {
            await AppServiceProxy.FileToFolderAsync(new DataContract.FileToFolderParameters
            {
                Token = Token,
                ObjectIds = ids.ToArray(),
                ObjectType = objectType.ConvertEnum<DataContract.ObjectType>(),
                FromFolderId = fromFolderId,
                ToFolderId = toFolderId,
                Move = true
            });
        }

        internal async Task MoveToFolderLocalAsync(List<int> ids, int fromFolderId, 
            int toFolderId, ObjectType objectType)
        {
            var commonActionsDataAccess = GetCommonActionsDataAccess(objectType);
            await commonActionsDataAccess.CopyToFolderAsync(toFolderId, ids);
            await commonActionsDataAccess.RemoveFromFolderAsync(ids, fromFolderId);        
        }

        public async Task CopyToWorktray(List<IBusinessEntity> businessEntities, 
            SourceType sourceType = SourceType.Auto)
        {
            await CopyToWorktray(businessEntities.Select(bi => bi.Id).ToList(),
                businessEntities.First().ObjectType, sourceType);
        }

        public async Task CopyToWorktray(List<int> ids, ObjectType objectType, 
            SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(
                new CopyToWorktrayEvent(objectType.ToModuleType(), ids.Count));

            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
                await CopyToWorktrayRemoteAsync(ids, objectType);
            else if (sourceType == SourceType.Local)
            {
                await ActionsManager.QueueActionAsync(
                      CopyToWorktrayAction.Create(objectType, ids.ToArray()));
            }
            else
                throw new ArgumentException("Invalid sourceType provided.");

            await CopyToWorktrayLocalAsync(ids, objectType);
        }
    
        internal async Task CopyToWorktrayRemoteAsync(List<int> ids, ObjectType objectType)
        {
            await AppServiceProxy.CopyToWorktrayAsync(new DataContract.CopyToWorktrayParameters
            {
                Token = Token,
                ObjectIds = ids.ToArray(),
                ObjectType = objectType.ConvertEnum<DataContract.ObjectType>(),
            });
        }

        internal async Task CopyToWorktrayLocalAsync(List<int> ids, ObjectType objectType)
        {
            var worktrayId = SystemFoldersInfo.Int_WorktrayRoot;
            var commonActionsDataAccess = GetCommonActionsDataAccess(objectType);
            if (!(commonActionsDataAccess is ICopiable copiable))
                return;
            await copiable.CopyToFolderAsync(worktrayId, ids);   
        }

        public async Task CopyToUserWorktray(List<IBusinessEntity> businessEntities,
            List<SystemUser> systemUsers, string comment = null, SourceType sourceType = SourceType.Auto)
        {
            await CopyToUserWorktray(businessEntities.Select(bi => bi.Id).ToList(),
                businessEntities.First().ObjectType, systemUsers, comment, sourceType);
        }

        public async Task CopyToUserWorktray(List<IBusinessEntity> businessEntities,
            List<int> systemUsersIds, string comment = null, SourceType sourceType = SourceType.Auto)
        {
            await CopyToUserWorktray(businessEntities.Select(bi => bi.Id).ToList(),
                businessEntities.First().ObjectType, systemUsersIds, comment, sourceType);
        }

        public async Task CopyToUserWorktray(List<int> ids, ObjectType objectType, List<SystemUser> systemUsers,
            string comment = null, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(
                new CopyToUserWorktrayEvent(objectType.ToModuleType(), ids.Count));

            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                      ? SourceType.Remote
                      : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.CopyToWorktrayAsync(new DataContract.CopyToWorktrayParameters
                {
                    Token = Token,
                    ObjectIds = ids.ToArray(),
                    ObjectType = objectType.ConvertEnum<DataContract.ObjectType>(),
                    UserIds = systemUsers.Select(su => su.Id).ToArray(),
                    Comment = comment
                });

                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task CopyToUserWorktray(List<int> ids, ObjectType objectType, List<int> systemUsersIds,
            string comment = null, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(
                new CopyToUserWorktrayEvent(objectType.ToModuleType(), ids.Count));

            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.CopyToWorktrayAsync(new DataContract.CopyToWorktrayParameters
                {
                    Token = Token,
                    ObjectIds = ids.ToArray(),
                    ObjectType = objectType.ConvertEnum<DataContract.ObjectType>(),
                    UserIds = systemUsersIds.ToArray(),
                    Comment = comment
                });

                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task RemoveFromFolder(List<IBusinessEntity> businessEntities,
            Folder folder, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(
                new DeleteFromFolderEvent(folder.Module, businessEntities.Count));

            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            var ids = businessEntities.Select(b => b.Id).ToList();
            var folderId = folder.Id;
            var objectType = businessEntities.First().ObjectType;

            if (sourceType == SourceType.Remote)
                await RemoveFromFolderRemoteAsync(ids, folderId, objectType);
            else if (sourceType == SourceType.Local)
            {
                await ActionsManager.QueueActionAsync(RemoveFromFolderAction
                      .Create(folder.Id, businessEntities.First().ObjectType,
                      businessEntities.Select(dp => dp.Id).ToArray()));
            }
            else
                throw new ArgumentException("Invalid sourceType provided.");

            await RemoveFromFolderLocalAsync(businessEntities, folderId, objectType);

            CommonConfig.MessengerHub.Publish(
                new EntityRemovedFromFolderMessage(
                    this, businessEntities.First().ObjectType,
                    folder.Id, businessEntities.Select(b => b.Id).ToList()));
        }

        internal async Task RemoveFromFolderRemoteAsync(List<int> ids, int folderId, ObjectType objectType)
        {
            await AppServiceProxy.RemoveFromFolderAsync(new DataContract.RemoveFromFolderParameters
            {
                Token = Token,
                ObjectIds = ids.ToArray(),
                ObjectType = objectType.ConvertEnum<DataContract.ObjectType>(),
                FolderId = folderId
            });
        }

        internal async Task RemoveFromFolderLocalAsync(List<IBusinessEntity> businessEntities, int folderId, ObjectType objectType)
        {
            var commonActionsDataAccess = GetCommonActionsDataAccess(objectType);
            if (commonActionsDataAccess is not IRemovable removable)
                return;
            await removable.RemoveFromFolderAsync(businessEntities, folderId, true);
        }

        internal async Task RemoveFromFolderLocalAsync(List<int> ids, int folderId, ObjectType objectType)
        {
            var commonActionsDataAccess = GetCommonActionsDataAccess(objectType);
            if (commonActionsDataAccess is not IRemovable removable)
                return;
            await removable.RemoveFromFolderAsync(ids, folderId);
        }

        public async Task Delete(List<IBusinessEntity> businessEntities, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(
                new DeleteEvent(businessEntities.First().ModuleType, businessEntities.Count));

            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            var ids = businessEntities.Select(be => be.Id).ToList();
            var objectType = businessEntities.First().ObjectType;

            if (sourceType == SourceType.Remote)
                await DeleteRemoteAsync(ids, objectType);
            else if (sourceType == SourceType.Local)
            {
                await ActionsManager.QueueActionAsync(
                    DeleteAction.Create(businessEntities.First().ObjectType,
                    businessEntities.Select(dp => dp.Id).ToArray()));
            }
            else
                throw new ArgumentException("Invalid sourceType provided.");

            await DeleteLocalAsync(businessEntities, objectType);

            CommonConfig.MessengerHub.Publish(
                new EntityRemovedMessage(
                    this, businessEntities.First().ObjectType,
                    businessEntities.Select(b => b.Id).ToList()));
        }

        internal async Task DeleteRemoteAsync(List<int> ids, ObjectType objectType)
        {
            await AppServiceProxy.DeleteAsync(new DataContract.DeleteParameters
            {
                Token = Token,
                ObjectIds = ids.ToArray(),
                ObjectType = objectType.ConvertEnum<DataContract.ObjectType>()
            });
        }

        internal async Task DeleteLocalAsync(List<IBusinessEntity> businessEntities, ObjectType objectType)
        {

            var commonActionsDataAccess = GetCommonActionsDataAccess(objectType);
            if (commonActionsDataAccess is not IDeletable deletable)
                return;
            await deletable.DeleteAsync(businessEntities, true);
        }

        public async Task RestoreDeletedObjectsLocalAsync(List<int> ids, ObjectType objectType)
        {
            var commonActionsDataAccess = GetCommonActionsDataAccess(objectType);
            if (commonActionsDataAccess is not IRestorable restorable)
                return;
            await restorable.RestoreDeletedObjectsAsync(ids);            
        }

        private ICommonActionsDataAccess GetCommonActionsDataAccess(ObjectType objectType)
        {
            switch (objectType)
            {
                case ObjectType.Document:
                    return (ICommonActionsDataAccess)documentsDataAccess;
                case ObjectType.Contact:
                    return (ICommonActionsDataAccess)contactsDataAccess;
                case ObjectType.Shortcode:
                    return (ICommonActionsDataAccess)shortcodesDataAccess;
                default:
                    throw new ArgumentException($"Object type {objectType} doesn't implement ICommonActionsDataAccess");
            }
        }

        public async Task<List<int>> GetFavoriteCategories(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetFavoriteCategoriesAsync
                    (new DataContract.GetFavoriteCategoriesParameters
                    {
                        Token = Token
                    });

                return result.FavoriteCategoriesIds;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType,"Favorite categories cannot be loaded offline");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task AddFavoriteCategory(int categoryId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.AddFavoriteCategoryAsync(
                    new DataContract.AddFavoriteCategoryParameters
                    {
                        Token = Token,
                        CategoryId = categoryId
                    });

                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task RemoveFavoriteCategory(int categoryId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.RemoveFavoriteCategoryAsync(
                    new DataContract.RemoveFavoriteCategoryParameters
                    {
                        Token = Token,
                        CategoryId = categoryId
                    });

                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetCategoriesAsync(IBusinessEntity businessEntity,
            List<Category> newCategories, SourceType sourceType = SourceType.Auto)
        {
            if (businessEntity is not ICategorizable categorizableEntity)
                return;

            var oldCategories = categorizableEntity.Categories;
            CommonConfig.UsageAnalytics.LogEvent(
                new SetCategoriesEvent(businessEntity.ModuleType, 1));

            if (sourceType == SourceType.Auto)
            {
                sourceType = CommonConfig.Reachability.IsReachable
                    ? SourceType.Remote
                    : SourceType.Local;
            }

            if (sourceType == SourceType.Remote)
            {
                await SetCategoriesRemoteAsync(businessEntity.Id,
                    newCategories.Select(c => c.Id).ToArray(), businessEntity.ObjectType);
            }
            else if (sourceType == SourceType.Local)
            {
                await ActionsManager.QueueActionAsync(
                    SetCategoriesAction.Create(
                        newCategories, oldCategories, businessEntity.Id, businessEntity.ObjectType));
            }
            else
                throw new ArgumentException("Invalid sourceType provided.");

            UpdateBusinessEntityCategories(businessEntity, newCategories);

            await SetCategoriesLocalAsync(businessEntity.Id, newCategories,
                businessEntity.ObjectType);

            CommonConfig.MessengerHub.Publish(
                new EntityCategoriesChangedMessage(
                    this, businessEntity.ObjectType,
                    businessEntity.Id, categorizableEntity.Categories.ToList()));
        }

        internal async Task SetCategoriesRemoteAsync(int objectId, int[] categoryIds, ObjectType objectType)
        {
            await AppServiceProxy.SetCategoriesAsync(new DataContract.SetCategoriesParameters
            {
                Token = Token,
                ObjectId = objectId,
                ObjectType = objectType.ConvertEnum<DataContract.ObjectType>(),
                CategoryIds = categoryIds
            }); 
        }

        internal async Task SetCategoriesLocalAsync(int entityId, List<Category> categories, ObjectType objectType)
        {
            switch (objectType)
            {
                case ObjectType.Document:
                    await documentsDataAccess.SetCategoriesAsync(entityId, categories);
                    break;
                case ObjectType.Contact:
                    await contactsDataAccess.SetCategoriesAsync(entityId, categories);
                    break;
            }
        }

        internal void UpdateBusinessEntityCategories(IBusinessEntity businessEntity, List<Category> categories)
        {
            if (businessEntity is not ICategorizable categorizable)
                return;

            categorizable.Categories.Clear();
            categorizable.Categories.AddRange(categories);
        }
    }
}