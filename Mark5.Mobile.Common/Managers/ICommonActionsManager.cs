//
// Project: Mark5.Mobile.Common
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

        Task CopyToFolder(List<IBusinessEntity> businessEntity, Folder folder, SourceType sourceType = SourceType.Auto);

        Task MoveToFolder(List<IBusinessEntity> businessEntity, Folder fromFolder, Folder toFolder, SourceType sourceType = SourceType.Auto);

        Task CopyToWorktray(List<BusinessEntity> businessEntity, SourceType sourceType = SourceType.Auto);

        Task CopyToUserWorktray(List<BusinessEntity> businessEntity, List<SystemUser> systemUsers, string comment = null, SourceType sourceType = SourceType.Auto);

        Task RemoveFromFolder(List<BusinessEntity> businessEntity, Folder folder, SourceType sourceType = SourceType.Auto);

        Task Delete(List<BusinessEntity> businessEntity, SourceType sourceType = SourceType.Auto);
    }
}

