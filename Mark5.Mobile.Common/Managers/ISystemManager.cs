//
// Project: Mark5.Mobile.Common
// File: ISystemManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Managers
{

    public interface ISystemManager
    {

        Task<SystemSettings> GetSystemSettingsAsync(SourceType sourceType = SourceType.Local);

        Task<SystemUsersDepartments> GetSystemUsersDepartmentsAsync(SourceType sourceType = SourceType.Local);
    }
}

