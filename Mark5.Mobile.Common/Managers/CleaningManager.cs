using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;

namespace Mark5.Mobile.Common
{
    class CleaningManager : ICleaningManager
    {
        IDocumentsDataAccess documentsDataAccess;
        IContactsDataAccess contactsDataAccess;
        IShortcodesDataAccess shortcodesDataAccess;
        ICalendarDataAccess calendarDataAccess;

        public CleaningManager(IDocumentsDataAccess documentsDataAccess, IContactsDataAccess contactsDataAccess, IShortcodesDataAccess shortcodesDataAccess, ICalendarDataAccess calendarDataAccess)
        {
            this.documentsDataAccess = documentsDataAccess;
            this.contactsDataAccess = contactsDataAccess;
            this.shortcodesDataAccess = shortcodesDataAccess;
            this.calendarDataAccess = calendarDataAccess;
        }

        public async Task RemoveOrphans()
        {
            await documentsDataAccess.RemoveOrphans();
            await contactsDataAccess.RemoveOrphans();
            await shortcodesDataAccess.RemoveOrphans();
            await calendarDataAccess.RemoveOrphans();
        }
    }
}

