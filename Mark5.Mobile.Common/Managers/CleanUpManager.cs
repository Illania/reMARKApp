//
// Project: Mark5.Mobile.Common
// File: CleanUpManager.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common
{

    class CleanUpManager : ICleanUpManager
    {

        readonly IDocumentsDataAccess documentsDataAccess;
        readonly IContactsDataAccess contactsDataAccess;
        readonly IShortcodesDataAccess shortcodesDataAccess;
        readonly ICalendarDataAccess calendarDataAccess;

        public CleanUpManager(IDocumentsDataAccess documentsDataAccess, IContactsDataAccess contactsDataAccess, IShortcodesDataAccess shortcodesDataAccess, ICalendarDataAccess calendarDataAccess)
        {
            this.documentsDataAccess = documentsDataAccess;
            this.contactsDataAccess = contactsDataAccess;
            this.shortcodesDataAccess = shortcodesDataAccess;
            this.calendarDataAccess = calendarDataAccess;
        }

        public async Task<bool> IsCleanUpNecessary(int intervalDays)
        {
            var lastCacheCleanUp = await FileSystemStorage.GetLastCacheCleanUpAsync();
            if (lastCacheCleanUp == DateTime.SpecifyKind(default(DateTime), DateTimeKind.Utc))
            {
                await FileSystemStorage.SaveLastCacheCleanUpAsync(DateTime.UtcNow);
                return false;
            }

            return lastCacheCleanUp.AddDays(intervalDays) <= DateTime.UtcNow;
        }

        public async Task CleanUp(IEnumerable<ModuleType> modules = null)
        {
            if (modules == null)
            {
                await documentsDataAccess.RemoveOrphans();
                await contactsDataAccess.RemoveOrphans();
                await shortcodesDataAccess.RemoveOrphans();
                await calendarDataAccess.RemoveOrphans();
            }
            else
            {
                foreach (var module in modules)
                {
                    switch (module)
                    {
                        case ModuleType.Documents:
                            await documentsDataAccess.RemoveOrphans();
                            break;
                        case ModuleType.Contacts:
                            await contactsDataAccess.RemoveOrphans();
                            break;
                        case ModuleType.Shortcodes:
                            await shortcodesDataAccess.RemoveOrphans();
                            break;
                        case ModuleType.Calendar:
                            await calendarDataAccess.RemoveOrphans();
                            break;
                        default:
                            throw new ArgumentException("Module not supported");
                    }
                }
            }

            await FileSystemStorage.SaveLastCacheCleanUpAsync(DateTime.UtcNow);
        }

        public async Task ClearContactsCache()
        {
            await contactsDataAccess.DeleteAllAsync();
        }

        public async Task ClearShortcodeCache()
        {
            await shortcodesDataAccess.DeleteAllAsync();
        }
    }
}

