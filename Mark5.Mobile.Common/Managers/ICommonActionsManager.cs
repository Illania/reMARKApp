//
// File: ICommonActionsManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Managers
{
    public interface ICommonActionsManager
    {
        Task<List<ObjectAction>> GetObjectActionsAsync(IBusinessEntity businessEntity, SourceType sourceType = SourceType.Auto);

        Task<List<ObjectLink>> GetObjectLinksAsync(IBusinessEntity businessEntity, SourceType sourceType = SourceType.Auto);

        Task CopyToFolder(List<IBusinessEntity> businessEntities, Folder folder, SourceType sourceType = SourceType.Auto);

        Task MoveToFolder(List<IBusinessEntity> businessEntities, Folder fromFolder, Folder toFolder, SourceType sourceType = SourceType.Auto);

        Task CopyToWorktray(List<IBusinessEntity> businessEntities, SourceType sourceType = SourceType.Auto);

        Task CopyToUserWorktray(List<IBusinessEntity> businessEntities, List<SystemUser> systemUsers, string comment = null, SourceType sourceType = SourceType.Auto);

        Task RemoveFromFolder(List<IBusinessEntity> businessEntities, Folder folder, SourceType sourceType = SourceType.Auto);

        Task Delete(List<IBusinessEntity> businessEntities, SourceType sourceType = SourceType.Auto);
    }
}