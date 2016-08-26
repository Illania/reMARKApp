using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{
    class CleanUpManager : ICleanUpManager
    {
        IDocumentsDataAccess documentsDataAccess;
        IContactsDataAccess contactsDataAccess;
        IShortcodesDataAccess shortcodesDataAccess;
        ICalendarDataAccess calendarDataAccess;

        public CleanUpManager(IDocumentsDataAccess documentsDataAccess, IContactsDataAccess contactsDataAccess, IShortcodesDataAccess shortcodesDataAccess, ICalendarDataAccess calendarDataAccess)
        {
            this.documentsDataAccess = documentsDataAccess;
            this.contactsDataAccess = contactsDataAccess;
            this.shortcodesDataAccess = shortcodesDataAccess;
            this.calendarDataAccess = calendarDataAccess;
        }

        public async Task RemoveOrphans(IEnumerable<ModuleType> modules = null)
        {
            if (modules == null)
            {
                await documentsDataAccess.RemoveOrphans();
                await contactsDataAccess.RemoveOrphans();
                await shortcodesDataAccess.RemoveOrphans();
                await calendarDataAccess.RemoveOrphans();

                return;
            }

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
    }
}

