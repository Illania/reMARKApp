//
// Project: Mark5.Mobile.Common
// File: SystemManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Storage;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Managers
{

    class SystemManager : AbstractManager, ISystemManager
    {

        public SystemManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy)
            : base(connectionInfo, appServiceProxy)
        {
        }

        public async Task<SystemSettings> GetSystemSettingsAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Local)
            {
                return await FileSystemStorage.GetSystemSettingsAsync();
            }

            if (sourceType == SourceType.Remote)
            {
                var systemSettingsResult = await AppServiceProxy.GetSystemSettingsAsync(new DataContract.GetSystemSettingsParameters
                {
                    Token = ConnectionInfo.Token
                });

                var result = new SystemSettings
                {
                    SystemInfo = systemSettingsResult.SystemInfo.Convert(),
                    DocumentsModuleInfo = systemSettingsResult.DocumentsModuleInfo.Convert(),
                    ContactsModuleInfo = systemSettingsResult.ContactsModuleInfo.Convert(),
                    ShortcodesModuleInfo = systemSettingsResult.ShortcodesModuleInfo.Convert(),
                    CalendarModuleInfo = systemSettingsResult.CalendarModuleInfo.Convert(),
                    UserInfo = systemSettingsResult.UserInfo.Convert()
                };

                await FileSystemStorage.SaveSystemSettingsAsync(result);

                return result;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<SystemUsersDepartments> GetSystemUsersDepartmentsAsync(SourceType sourceType = SourceType.Local)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Local)
            {
                return await FileSystemStorage.GetSystemUsersDepartmentsAsync();
            }

            if (sourceType == SourceType.Remote)
            {
                var systemSettingsResult = await AppServiceProxy.GetSystemUsersAsync(new DataContract.GetSystemUsersParameters
                {
                    Token = ConnectionInfo.Token
                });

                var result = new SystemUsersDepartments();
                result.Users.AddRange(systemSettingsResult.Users.WhereNotNull().Select(u => u.Convert()));
                result.Departments.AddRange(systemSettingsResult.Departments.WhereNotNull().Select(d => d.Convert()));

                await FileSystemStorage.SaveSystemUsersDepartmentsAsync(result);

                return result;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}

