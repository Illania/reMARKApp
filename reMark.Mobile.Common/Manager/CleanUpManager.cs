using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.DataAccess;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Storage;

namespace reMark.Mobile.Common
{
    class CleanUpManager : ICleanUpManager
    {
        readonly IDocumentsDataAccess documentsDataAccess;
        readonly IContactsDataAccess contactsDataAccess;
        readonly IShortcodesDataAccess shortcodesDataAccess;

        public CleanUpManager(IDocumentsDataAccess documentsDataAccess, IContactsDataAccess contactsDataAccess, IShortcodesDataAccess shortcodesDataAccess)
        {
            this.documentsDataAccess = documentsDataAccess;
            this.contactsDataAccess = contactsDataAccess;
            this.shortcodesDataAccess = shortcodesDataAccess;
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
            }
            else
            {
                foreach (var module in modules)
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
                        default:
                            throw new ArgumentException("Module not supported");
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