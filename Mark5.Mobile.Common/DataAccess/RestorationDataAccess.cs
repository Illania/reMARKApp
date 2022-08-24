using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.DataAccess.Interfaces;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.DataAccess
{
    class RestorationDataAccess : IRestorationDataAccess
    {
        readonly DatabaseConnectionProvider systemDatabase;

        public RestorationDataAccess(DatabaseConnectionProvider systemDatabase)
        {
            this.systemDatabase = systemDatabase;
        }

        public async Task<List<DeletedObject>> GetDeletedObjectsAsync(List<int> ids, DeletedObjectType type, int maxItems = 500)
        {
            try
            {
                var deletedObjects = new List<DeletedObject>();

                await systemDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select * " + $"from {nameof(DeletedObject)} " +
                    $@"where {nameof(DeletedObject.DeletedObjectId)} in ({string.Join(",", ids).TrimEnd(',')}) " +
                    $"and {nameof(DeletedObject.ObjectType)} = {(int)type} " + 
                    $"order by {nameof(DeletedObject.Id)} desc ";

                    if (maxItems > 0)
                        query += $"limit {maxItems - 1} ";
                    var result = c.Query<DeletedObject>(query);

                    deletedObjects = result;
                });

                return deletedObjects;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting deleted objects.", ex);
            }
        }

        public async Task SaveDeletedObjects<T>(List<T> businessEntities) where T : IBusinessEntity
        {
            var be = businessEntities.FirstOrDefault();
            try
            {
                await DatabaseConnectionProvider.SystemDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplaceAll(businessEntities.Select(dp => new DeletedObject
                    {
                        ObjectType = GetDeletedObjectType(be),
                        DeletedObjectId = dp.Id,
                        DateDeletedTimestamp = DateTime.UtcNow.ConvertDateTimeToTimestampMilliseconds(),
                        SerializedObject = Serializer.Serialize(dp)
                    }));

                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while saving deleted objects.", ex);
            }
        }

        public async Task SaveDeletedObjectLinkedFolders(int documentId, List<int> linkedFoldersIds)
        {
            try
            {
                await systemDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplaceAll(linkedFoldersIds.Select(lf => new DeletedObjectLink
                    {
                        FolderId = lf,
                        DeletedObjectId = documentId,
                        ObjectType = ObjectType.Document
                    }));

                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException($"Error while saving linked folders for document {documentId}.", ex);
            }
        }

        DeletedObjectType GetDeletedObjectType(IBusinessEntity businessEntity)
        {
            switch (businessEntity)
            {
                case Document:
                    return DeletedObjectType.Document;
                case DocumentPreview:
                    return DeletedObjectType.DocumentPreview;
                case Contact:
                    return DeletedObjectType.Contact;
                case ContactPreview:
                    return DeletedObjectType.ContactPreview;
                case Shortcode:
                    return DeletedObjectType.Shortcode;
                case ShortcodePreview:
                    return DeletedObjectType.ShortcodePreview;
                default:
                    throw new ArgumentException("Object type is not supported");
            }
        }

        DeletedObjectType GetDeletedObjectType(ModuleType moduleType, bool isPreview)
        {
            switch (moduleType)
            {
                case ModuleType.Documents:
                    return isPreview ? DeletedObjectType.DocumentPreview : DeletedObjectType.Document;
                case ModuleType.Contacts:
                    return isPreview ? DeletedObjectType.ContactPreview : DeletedObjectType.Contact;
                case ModuleType.Shortcodes:
                    return isPreview ? DeletedObjectType.ShortcodePreview : DeletedObjectType.Shortcode;
                default:
                    throw new ArgumentException("Object type is not supported");
            }
        }


    }
}
