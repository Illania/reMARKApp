using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Analytics;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Manager
{
    class CommonActionsManager : AbstractManager, ICommonActionsManager
    {
        readonly IDocumentsDataAccess documentsDataAccess;
        readonly IContactsDataAccess contactsDataAccess;
        readonly IShortcodesDataAccess shortcodesDataAccess;
        readonly ICalendarDataAccess calendarDataAccess;

        public CommonActionsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IDocumentsDataAccess documentsDataAccess, IContactsDataAccess contactsDataAccess, IShortcodesDataAccess shortcodesDataAccess, ICalendarDataAccess calendarDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.documentsDataAccess = documentsDataAccess;
            this.contactsDataAccess = contactsDataAccess;
            this.shortcodesDataAccess = shortcodesDataAccess;
            this.calendarDataAccess = calendarDataAccess;
        }

        public async Task<List<ObjectAction>> GetObjectActionsAsync(IBusinessEntity businessEntity, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

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
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<ObjectLink>> GetObjectLinksAsync(IBusinessEntity businessEntity, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

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
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task CopyToFolder(List<IBusinessEntity> businessEntities, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            AnalyticsManager.LogEvent(new CopyToFolderEvent(folder.Module, businessEntities.Count));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.FileToFolderAsync(new DataContract.FileToFolderParameters
                {
                    Token = Token,
                    ObjectIds = businessEntities.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntities.First().ObjectType.ConvertEnum<DataContract.ObjectType>(),
                    ToFolderId = folder.Id,
                    Move = false
                });

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task MoveToFolder(List<IBusinessEntity> businessEntities, Folder fromFolder, Folder toFolder, SourceType sourceType = SourceType.Auto)
        {
            AnalyticsManager.LogEvent(new MoveToFolderEvent(fromFolder.Module, businessEntities.Count));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.FileToFolderAsync(new DataContract.FileToFolderParameters
                {
                    Token = Token,
                    ObjectIds = businessEntities.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntities.First().ObjectType.ConvertEnum<DataContract.ObjectType>(),
                    FromFolderId = fromFolder.Id,
                    ToFolderId = toFolder.Id,
                    Move = true
                });

                var documentPreviews = businessEntities.OfType<DocumentPreview>();
                if (documentPreviews.Any())
                    await documentsDataAccess.RemoveFromFolderAsync(documentPreviews.ToList(), fromFolder);
                var documents = businessEntities.OfType<Document>();
                if (documents.Any())
                    await documentsDataAccess.RemoveFromFolderAsync(documents.ToList(), fromFolder);
                var contactPreviews = businessEntities.OfType<ContactPreview>();
                if (contactPreviews.Any())
                    await contactsDataAccess.RemoveFromFolderAsync(contactPreviews.ToList(), fromFolder);
                var contacts = businessEntities.OfType<Contact>();
                if (contacts.Any())
                    await contactsDataAccess.RemoveFromFolderAsync(contacts.ToList(), fromFolder);
                var shortcodePreviews = businessEntities.OfType<ShortcodePreview>();
                if (shortcodePreviews.Any())
                    await shortcodesDataAccess.RemoveFromFolderAsync(shortcodePreviews.ToList(), fromFolder);
                var shortcodes = businessEntities.OfType<Shortcode>();
                if (shortcodes.Any())
                    await shortcodesDataAccess.RemoveFromFolderAsync(shortcodes.ToList(), fromFolder);
                var appointments = businessEntities.OfType<CalendarAppointment>();
                if (appointments.Any())
                    await calendarDataAccess.RemoveFromFolderAsync(appointments.ToList(), fromFolder);
                var tasks = businessEntities.OfType<CalendarTask>();
                if (tasks.Any())
                    await calendarDataAccess.RemoveFromFolderAsync(tasks.ToList(), fromFolder);

                CommonConfig.MessengerHub.Publish(new EntityMovedFromFolderMessage(this, businessEntities.First().ObjectType, fromFolder.Id, businessEntities.Select(b => b.Id).ToList()));

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task CopyToWorktray(List<IBusinessEntity> businessEntities, SourceType sourceType = SourceType.Auto)
        {
            AnalyticsManager.LogEvent(new CopyToWorktrayEvent(businessEntities.First().ModuleType, businessEntities.Count));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.CopyToWorktrayAsync(new DataContract.CopyToWorktrayParameters
                {
                    Token = Token,
                    ObjectIds = businessEntities.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntities.First().ObjectType.ConvertEnum<DataContract.ObjectType>()
                });

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task CopyToUserWorktray(List<IBusinessEntity> businessEntities, List<SystemUser> systemUsers, string comment = null, SourceType sourceType = SourceType.Auto)
        {
            AnalyticsManager.LogEvent(new CopyToUserWorktrayEvent(businessEntities.First().ModuleType, businessEntities.Count));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.CopyToWorktrayAsync(new DataContract.CopyToWorktrayParameters
                {
                    Token = Token,
                    ObjectIds = businessEntities.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntities.First().ObjectType.ConvertEnum<DataContract.ObjectType>(),
                    UserIds = systemUsers.Select(su => su.Id).ToArray(),
                    Comment = comment
                });

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task RemoveFromFolder(List<IBusinessEntity> businessEntities, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            AnalyticsManager.LogEvent(new DeleteFromFolderEvent(folder.Module, businessEntities.Count));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.RemoveFromFolderAsync(new DataContract.RemoveFromFolderParameters
                {
                    Token = Token,
                    ObjectIds = businessEntities.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntities.First().ObjectType.ConvertEnum<DataContract.ObjectType>(),
                    FolderId = folder.Id
                });

                var documentPreviews = businessEntities.OfType<DocumentPreview>();
                if (documentPreviews.Any())
                    await documentsDataAccess.RemoveFromFolderAsync(documentPreviews.ToList(), folder);
                var documents = businessEntities.OfType<Document>();
                if (documents.Any())
                    await documentsDataAccess.RemoveFromFolderAsync(documents.ToList(), folder);
                var contactPreviews = businessEntities.OfType<ContactPreview>();
                if (contactPreviews.Any())
                    await contactsDataAccess.RemoveFromFolderAsync(contactPreviews.ToList(), folder);
                var contacts = businessEntities.OfType<Contact>();
                if (contacts.Any())
                    await contactsDataAccess.RemoveFromFolderAsync(contacts.ToList(), folder);
                var shortcodePreviews = businessEntities.OfType<ShortcodePreview>();
                if (shortcodePreviews.Any())
                    await shortcodesDataAccess.RemoveFromFolderAsync(shortcodePreviews.ToList(), folder);
                var shortcodes = businessEntities.OfType<Shortcode>();
                if (shortcodes.Any())
                    await shortcodesDataAccess.RemoveFromFolderAsync(shortcodes.ToList(), folder);
                var appointments = businessEntities.OfType<CalendarAppointment>();
                if (appointments.Any())
                    await calendarDataAccess.RemoveFromFolderAsync(appointments.ToList(), folder);
                var tasks = businessEntities.OfType<CalendarTask>();
                if (tasks.Any())
                    await calendarDataAccess.RemoveFromFolderAsync(tasks.ToList(), folder);

                CommonConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this, businessEntities.First().ObjectType, folder.Id,
                                                                                     businessEntities.Select(b => b.Id).ToList()));

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task Delete(List<IBusinessEntity> businessEntities, SourceType sourceType = SourceType.Auto)
        {
            AnalyticsManager.LogEvent(new DeleteEvent(businessEntities.First().ModuleType, businessEntities.Count));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.DeleteAsync(new DataContract.DeleteParameters
                {
                    Token = Token,
                    ObjectIds = businessEntities.Select(be => be.Id).ToArray(),
                    ObjectType = businessEntities.First().ObjectType.ConvertEnum<DataContract.ObjectType>()
                });

                var documentPreviews = businessEntities.OfType<DocumentPreview>();
                if (documentPreviews.Any())
                    await documentsDataAccess.DeleteAsync(documentPreviews.ToList());
                var documents = businessEntities.OfType<Document>();
                if (documents.Any())
                    await documentsDataAccess.DeleteAsync(documents.ToList());
                var contactPreviews = businessEntities.OfType<ContactPreview>();
                if (contactPreviews.Any())
                    await contactsDataAccess.DeleteAsync(contactPreviews.ToList());
                var contacts = businessEntities.OfType<Contact>();
                if (contacts.Any())
                    await contactsDataAccess.DeleteAsync(contacts.ToList());
                var shortcodePreviews = businessEntities.OfType<ShortcodePreview>();
                if (shortcodePreviews.Any())
                    await shortcodesDataAccess.DeleteAsync(shortcodePreviews.ToList());
                var shortcodes = businessEntities.OfType<Shortcode>();
                if (shortcodes.Any())
                    await shortcodesDataAccess.DeleteAsync(shortcodes.ToList());
                var appointments = businessEntities.OfType<CalendarAppointment>();
                if (appointments.Any())
                    await calendarDataAccess.DeleteAsync(appointments.ToList());
                var tasks = businessEntities.OfType<CalendarTask>();
                if (tasks.Any())
                    await calendarDataAccess.DeleteAsync(tasks.ToList());

                CommonConfig.MessengerHub.Publish(new EntityRemovedMessage(this,
                                                                           businessEntities.First().ObjectType, businessEntities.Select(b => b.Id).ToList()));

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}