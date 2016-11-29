//
// Project: Mark5.Mobile.Common
// File: ISystemManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Managers
{

    public interface ISystemManager
    {

        Task<SystemSettings> GetSystemSettingsAsync(SourceType sourceType = SourceType.Auto);

        Task<SystemUsersDepartments> GetSystemUsersDepartmentsAsync(SourceType sourceType = SourceType.Auto);
    }
}

