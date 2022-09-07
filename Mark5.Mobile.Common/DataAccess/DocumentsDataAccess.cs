using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Links;
using Mark5.Mobile.Common.Model.Containers;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.DataAccess.Interfaces;

namespace Mark5.Mobile.Common.DataAccess
{
    class DocumentsDataAccess : IDocumentsDataAccess, ICommonActionsDataAccess
    {
        readonly DatabaseConnectionProvider documentsDatabase;
        readonly IRestorationDataAccess restorationDataAccess;

        public DocumentsDataAccess(DatabaseConnectionProvider documentsDatabase, 
            IRestorationDataAccess restorationDataAccess)
        {
            this.documentsDatabase = documentsDatabase;
            this.restorationDataAccess = restorationDataAccess;
        }

        public async Task SaveDocumentPreviewsAsync(int folderId, List<DocumentPreview> documentPreviews, bool clean)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    if (clean)
                        c.Table<FolderDocumentLink>().Delete(fdl => fdl.FolderId == folderId);
                    c.InsertOrReplaceAll(documentPreviews.Select(dp => new FolderDocumentLink
                    {
                        FolderId = folderId,
                        DocumentId = dp.Id
                    }));
                    c.InsertOrReplaceAll(documentPreviews);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving document previews with folder.", ex);
            }
        }

        public async Task SaveDocumentPreviewsAsync(List<DocumentPreview> documentPreviews)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplaceAll(documentPreviews);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving document previews.", ex);
            }
        }

        public async Task<List<DocumentPreview>> GetDocumentPreviewsAsync(int folderId, int startId = -1, int endId = -1, int maxItems = 500)
        {
            try
            {
                List<DocumentPreview> documentPreviews = null;

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select * " + $"from {nameof(DocumentPreview)}, {nameof(FolderDocumentLink)} " + $"where {nameof(FolderDocumentLink.FolderId)} = {folderId} " +
                        $" and {nameof(DocumentPreview)}.{nameof(DocumentPreview.Id)} = {nameof(FolderDocumentLink)}.{nameof(FolderDocumentLink.DocumentId)} ";

                    if (startId > 0)
                        query += $"    and {nameof(DocumentPreview)}.{nameof(DocumentPreview.Id)} < \"{startId}\" ";
                    if (endId > 0)
                        query += $"    and {nameof(DocumentPreview)}.{nameof(DocumentPreview.Id)} > \"{endId}\" ";
                    query += $"order by {nameof(DocumentPreview.Id)} desc ";

                    if (maxItems > 0)
                        query += $"limit {maxItems - 1} ";
                    var result = c.Query<DocumentPreview>(query);

                    if (result == null || result.Count < 1)
                        throw new DataNotFoundException("Document previews could not be found.");

                    documentPreviews = result;
                });

                return documentPreviews;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting documents.", ex);
            }
        }

        public async Task<List<int>> GetNeighbourDocumentsIdAsync(int folderId, int documentId, bool getPrevious, bool getNext, int maxItems)
        {
            try
            {
                var documentIds = new List<int>(maxItems * 2 + 1);

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select {nameof(FolderDocumentLink.DocumentId)} as '{nameof(IdValue.Id)}' " + $"from {nameof(FolderDocumentLink)} " +
                        $"where {nameof(FolderDocumentLink.FolderId)} = {folderId} ";

                    if (getPrevious)
                    {
                        var getPreviousQuery = query + $" and  {nameof(FolderDocumentLink.DocumentId)} > \"{documentId}\" ";
                        getPreviousQuery += $"order by {nameof(FolderDocumentLink.DocumentId)} asc ";
                        getPreviousQuery += $"limit {maxItems} ";
                        var previous = c.Query<IdValue>(getPreviousQuery).Select(v => v.Id).Reverse();
                        documentIds.AddRange(previous);
                        if (getNext)
                            documentIds.Add(documentId);
                    }

                    if (!getNext) 
                        return;
                    
                    var getNextQuery = query + $" and  {nameof(FolderDocumentLink.DocumentId)} < \"{documentId}\" ";
                    getNextQuery += $"order by {nameof(FolderDocumentLink.DocumentId)} desc ";
                    getNextQuery += $"limit {maxItems} ";
                    var next = c.Query<IdValue>(getNextQuery).Select(v => v.Id);
                    documentIds.AddRange(next);
                });

                return documentIds;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting neighbour document ids.", ex);
            }
        }

        public async Task SaveDocumentAsync(Document document)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c => { c.InsertOrReplace(document); });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving document.", ex);
            }
        }

        public async Task<Document> GetDocumentAsync(int documentId)
        {
            try
            {
                Document document = null;

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var result = c.Find<Document>(documentId);
                    document = result ?? throw new DataNotFoundException("Document could not be found.");
                });

                return document;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting document.", ex);
            }
        }

        public async Task<DocumentPreview> GetDocumentPreviewAsync(int documentId)
        {
            try
            {
                DocumentPreview documentPreview = null;

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var result = c.Find<DocumentPreview>(documentId);

                    documentPreview = result ?? throw new DataNotFoundException("Document Preview could not be found.");
                });

                return documentPreview;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting document preview.", ex);
            }
        }

        public async Task SaveDocumentWithPreviewAsync(DocumentContainer container)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(container.DocumentPreview);
                    c.InsertOrReplace(container.Document);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving document with preview.", ex);
            }
        }

        public async Task<DocumentContainer> GetDocumentWithPreviewAsync(int documentId)
        {
            try
            {
                DocumentContainer container = null;

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var documentPreview = c.Find<DocumentPreview>(documentId);
                    if (documentPreview == null)
                        throw new DataNotFoundException("DocumentPreview could not be found.");

                    var document = c.Find<Document>(documentId);
                    if (document == null)
                        throw new DataNotFoundException("Document could not be found.");

                    container = new DocumentContainer(documentPreview, document);
                });

                return container;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting document with preview.", ex);
            }
        }

        public async Task SetDocumentReadStatusAsync(DocumentPreview documentPreview, Document document)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " + $"set \"{nameof(DocumentPreview.IsReadByCurrent)}\" = @isReadByCurrent, " +
                        $"    \"{nameof(DocumentPreview.IsReadByAnyone)}\" = @isReadByAnyone " + $"where \"{nameof(DocumentPreview.Id)}\" = @documentPreviewId");
                    cmd.Bind("@isReadByCurrent", documentPreview.IsReadByCurrent);
                    cmd.Bind("@isReadByAnyone", documentPreview.IsReadByAnyone);
                    cmd.Bind("@documentPreviewId", documentPreview.Id);

                    cmd.ExecuteNonQuery();

                    cmd = c.CreateCommand($"update \"{nameof(Document)}\" " + $"set \"{nameof(Document.ReadByUserIdsString)}\" = @readByUserIdsString, " +
                        $"    \"{nameof(Document.ReadByUserNamesString)}\" = @readByUserNamesString " + $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@readByUserIdsString", document.ReadByUserIdsString);
                    cmd.Bind("@readByUserNamesString", document.ReadByUserNamesString);
                    cmd.Bind("@documentId", documentPreview.Id);

                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error setting documents read status.", ex);
            }
        }

        public async Task SetDocumentPreviewsReadStatusAsync(List<DocumentPreview> documentPreviews)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var documentPreview in documentPreviews)
                    {
                        var cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " + $"set \"{nameof(DocumentPreview.IsReadByCurrent)}\" = @isReadByCurrent " +
                            $"   and \"{nameof(DocumentPreview.IsReadByAnyone)}\" = @isReadByAnyone " + $"where \"{nameof(DocumentPreview.Id)}\" = @documentPreviewId");
                        cmd.Bind("@isReadByCurrent", documentPreview.IsReadByCurrent);
                        cmd.Bind("@isReadByAnyone", documentPreview.IsReadByAnyone);
                        cmd.Bind("@documentPreviewId", documentPreview.Id);

                        cmd.ExecuteNonQuery();
                    }
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error setting documents read status.", ex);
            }
        }

        public async Task SetReadStatusAsync(List<int> documentIds, bool readStatus)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var documentId in documentIds)
                    {

                        var documentPreview = c.Find<DocumentPreview>(documentId);
                        if (documentPreview != null)
                        {
                            documentPreview.SetReadStatus(readStatus);

                            var cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " + $"set \"{nameof(DocumentPreview.IsReadByCurrent)}\" = @isReadByCurrent " +
                            $"   and \"{nameof(DocumentPreview.IsReadByAnyone)}\" = @isReadByAnyone " + $"where \"{nameof(DocumentPreview.Id)}\" = @documentPreviewId");
                            cmd.Bind("@isReadByCurrent", documentPreview.IsReadByCurrent);
                            cmd.Bind("@isReadByAnyone", documentPreview.IsReadByAnyone);
                            cmd.Bind("@documentPreviewId", documentPreview.Id);

                            cmd.ExecuteNonQuery();
                        }

                        var document = c.Find<Document>(documentId);
                        if (document != null)
                        {
                            document.SetReadStatus(readStatus);

                            var cmd = c.CreateCommand($"update \"{nameof(Document)}\" " + $"set \"{nameof(Document.ReadByUserIdsString)}\" = @readByUserIdsString, " +
                            $"    \"{nameof(Document.ReadByUserNamesString)}\" = @readByUserNamesString " + $"where \"{nameof(Document.Id)}\" = @documentId");
                            cmd.Bind("@readByUserIdsString", document.ReadByUserIdsString);
                            cmd.Bind("@readByUserNamesString", document.ReadByUserNamesString);
                            cmd.Bind("@documentId", documentPreview.Id);

                            cmd.ExecuteNonQuery();
                        }
                    }
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error setting documents", ex);
            }
        }

        public async Task SetDocumentPreviewsPriorityAsync(List<DocumentPreview> documentPreviews, Priority priority)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var documentPreview in documentPreviews)
                    {
                        var cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " + $"set \"{nameof(DocumentPreview.Priority)}\" = @priority " + $"where \"{nameof(DocumentPreview.Id)}\" = @documentPreviewId");
                        cmd.Bind("@priority", priority);
                        cmd.Bind("@documentPreviewId", documentPreview.Id);

                        cmd.ExecuteNonQuery();
                    }
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error setting documents priority.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<DocumentPreview> documentPreviews, 
            int folderId, bool saveBeforeDeletion = false)
        {
            try
            {
                var deletedPreviews = new List<DocumentPreview>();
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var dp in documentPreviews)
                    {
                        var id = dp.Id;
                        var linksCount = c.Table<FolderDocumentLink>()
                            .Count(fdl => fdl.DocumentId == id);
                        
                        if (linksCount == 1)
                        {
                            if (saveBeforeDeletion)
                                deletedPreviews.Add(dp);
                            
                            c.Table<DocumentPreview>().Delete(dp => dp.Id == id);
                            c.Table<Document>().Delete(d => d.Id == id);
                        }
                        c.Table<FolderDocumentLink>().Delete(fdl => 
                            fdl.DocumentId == id && fdl.FolderId == folderId);
                    }
                });

                if (deletedPreviews.Any())
                    await SaveDeletedObjectsAsync(deletedPreviews);
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing documents from folder.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<Document> documents, 
            int folderId, bool saveBeforeDeletion = false)
        {
            try
            {
                var deletedDocumentsToSave = new List<Document>();
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var document in documents)
                    {
                        var id = document.Id;
                        var linksCount = c.Table<FolderDocumentLink>().Count(fdl => fdl.DocumentId == id);
                        if (linksCount == 1)
                        {
                            if (saveBeforeDeletion)
                                deletedDocumentsToSave.Add(document);
                            
                            c.Table<DocumentPreview>().Delete(dp => dp.Id == id);
                            c.Table<Document>().Delete(d => d.Id == id);
                        }
                        c.Table<FolderDocumentLink>().Delete(fdl => 
                            fdl.DocumentId == id && fdl.FolderId == folderId);
                    }
                });

                if (deletedDocumentsToSave.Any())
                    await SaveDeletedObjectsAsync(deletedDocumentsToSave);
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing documents from folder.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<int> ids, int folderId)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var id in ids)
                    {
                        var linksCount = c.Table<FolderDocumentLink>()
                            .Count(fdl => fdl.DocumentId == id);
                        if (linksCount == 1)
                        {
                            c.Table<DocumentPreview>().Delete(dp => dp.Id == id);
                            c.Table<Document>().Delete(d => d.Id == id);
                        }
                        c.Table<FolderDocumentLink>().Delete(fdl => 
                            fdl.DocumentId == id && fdl.FolderId == folderId);
                    }
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing documents from folder.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<IBusinessEntity> businessEntities,
            int folderId, bool saveBeforeDeletion = false)
        {
            switch (businessEntities.FirstOrDefault())
            {
                case Document:
                    await RemoveFromFolderAsync(businessEntities.Select(be => 
                        (Document)be).ToList(), folderId, saveBeforeDeletion);
                    break;
                case DocumentPreview:
                    await RemoveFromFolderAsync(businessEntities.Select(be => 
                        (DocumentPreview)be).ToList(), folderId, saveBeforeDeletion);
                    break;
            }
        }

        public async Task DeleteAsync(List<IBusinessEntity> businessEntities, bool saveBeforeDeletion = false)
        {
            switch (businessEntities.FirstOrDefault())
            {
                case Document:
                    await DeleteAsync(businessEntities.Select(be => 
                        (Document)be).ToList(), saveBeforeDeletion);
                    break;
                case DocumentPreview:
                    await DeleteAsync(businessEntities.Select(be => 
                        (DocumentPreview)be).ToList(), saveBeforeDeletion);
                    break;
            }
        }

        public async Task DeleteAsync(List<DocumentPreview> documentPreviews, bool saveBeforeDeletion = false)
        {
            if (saveBeforeDeletion)
                await SaveDeletedObjectsAsync(documentPreviews);
            
            var ids = documentPreviews.Select(dp => dp.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        public async Task DeleteAsync(List<Document> documents, bool saveBeforeDeletion = false)
        {
            if (saveBeforeDeletion)
                await SaveDeletedObjectsAsync(documents);
            
            var ids = documents.Select(d => d.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        public async Task DeleteAsync(List<int> documentIds)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {         
                    c.Table<FolderDocumentLink>().Delete(fdl => documentIds.Contains(fdl.DocumentId));
                    c.Table<DocumentPreview>().Delete(dp => documentIds.Contains(dp.Id));
                    c.Table<Document>().Delete(d => documentIds.Contains(d.Id));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting documents.", ex);
            }
        }

        public async Task CopyToFolderAsync(int folderId, List<int> documentIds)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplaceAll(documentIds.Select(dp => new FolderDocumentLink
                    {
                        FolderId = folderId,
                        DocumentId = dp
                    }));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException($"Error filing document previews to folder with Id={folderId}.", ex);
            }
        }

        public async Task SaveTemplatePreviewsAsync(List<TemplatePreview> templatePreviews)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.DeleteAll<TemplatePreview>();
                    c.InsertAll(templatePreviews);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving templates.", ex);
            }
        }

        public async Task<List<TemplatePreview>> GetTemplatePreviewsAsync()
        {
            try
            {
                List<TemplatePreview> templatePreviews = null;

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var result = c.Table<TemplatePreview>().ToList();

                    if (result == null || result.Count < 1)
                        throw new DataNotFoundException("Template previews could not be found.");

                    templatePreviews = result;
                });

                return templatePreviews;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting templates.", ex);
            }
        }

        public async Task SaveTemplateAsync(Template template)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c => { c.InsertOrReplace(template); });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving templates.", ex);
            }
        }

        public async Task<Template> GetTemplateAsync(int templateId)
        {
            try
            {
                Template template = null;

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var result = c.Find<Template>(templateId);

                    if (result == null)
                        throw new DataNotFoundException("Template could not be found.");

                    template = result;
                });

                return template;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting template.", ex);
            }
        }

        public async Task SaveDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag, Template template)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(new DefaultTemplateInfo
                    {
                        CreationModeFlag = creationModeFlag,
                        Available = template != null,
                        TemplateId = template?.Id ?? -1
                    });

                    if (template != null)
                        c.InsertOrReplace(template);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving default template.", ex);
            }
        }

        public async Task<Template> GetDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag)
        {
            try
            {
                Template template = null;

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var info = c.Find<DefaultTemplateInfo>(creationModeFlag);

                    if (info == null)
                        throw new DataNotFoundException("Default template info could not be found.");

                    if (info.Available)
                    {
                        var result = c.Find<Template>(info.TemplateId);

                        if (result == null)
                            throw new DataNotFoundException("Default template could not be found.");

                        template = result;
                    }
                });

                return template;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting default template.", ex);
            }
        }

        public async Task SaveRecentAddressesAsync(List<RecentAddress> recentAddresses)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.DeleteAll<RecentAddress>();
                    c.InsertAll(recentAddresses);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving recent addresses.", ex);
            }
        }

        public async Task<List<RecentAddress>> GetRecentAddressesAsync()
        {
            try
            {
                List<RecentAddress> recentAddresses = null;

                await documentsDatabase.RunInConnectionAsync(c => { recentAddresses = c.Table<RecentAddress>().ToList(); });

                return recentAddresses;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting recent addresses.", ex);
            }
        }

        public async Task SaveAllCategories(List<Category> categories)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.DeleteAll<Category>();
                    c.InsertAll(categories);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving categories.", ex);
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                List<Category> categories = null;

                await documentsDatabase.RunInConnectionAsync(c => { categories = c.Table<Category>().ToList(); });

                return categories;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting categories.", ex);
            }
        }

        public async Task SetCategoriesAsync(int documentId, List<Category> categories)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " + $"set \"{nameof(DocumentPreview.CategoriesString)}\" = @categoriesString " + $"where \"{nameof(DocumentPreview.Id)}\" = @documentPreviewId");
                    cmd.Bind("@categoriesString",
                        new CategoriesValue
                        {
                            Categories = categories
                        }.CategoriesString);
                    cmd.Bind("@documentPreviewId", documentId);
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error setting categories.", ex);
            }
        }

        public async Task AddCommentAsync(Document document, Comment comment)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"select \"{nameof(Document.CommentsString)}\" " + $"from \"{nameof(Document)}\" " + $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@documentId", document.Id);
                    var result = cmd.ExecuteQuery<CommentsValue>();

                    if (result == null || result.Count < 1)
                        return;

                    var comments = result.First().Comments;

                    comments.Add(comment);
                    comments = comments.OrderBy(cm => cm.DateAddedTimestamp).ToList();

                    cmd = c.CreateCommand($"update \"{nameof(Document)}\" " + $"set \"{nameof(Document.CommentsString)}\" = @commentsString " + $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@commentsString",
                        new CommentsValue
                        {
                            Comments = comments
                        }.CommentsString);
                    cmd.Bind("@documentId", document.Id);
                    cmd.ExecuteNonQuery();

                    cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " + $"set \"{nameof(DocumentPreview.CommentsCount)}\" = @commentsCount " + $"where \"{nameof(DocumentPreview.Id)}\" = @documentId");
                    cmd.Bind("@commentsCount", comments.Count);
                    cmd.Bind("@documentId", document.Id);
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error adding comment.", ex);
            }
        }

        public async Task EditCommentAsync(Document document, Comment comment)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"select \"{nameof(Document.CommentsString)}\" " + $"from \"{nameof(Document)}\" " + $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@documentId", document.Id);
                    var result = cmd.ExecuteQuery<CommentsValue>();

                    if (result == null || result.Count < 1)
                        return;

                    var comments = result.First().Comments;

                    comments.RemoveAll(cm => cm.Id == comment.Id);
                    comments.Add(comment);
                    comments = comments.OrderBy(cm => cm.DateAddedTimestamp).ToList();

                    cmd = c.CreateCommand($"update \"{nameof(Document)}\" " + $"set \"{nameof(Document.CommentsString)}\" = @commentsString " + $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@commentsString",
                        new CommentsValue
                        {
                            Comments = comments
                        }.CommentsString);
                    cmd.Bind("@documentId", document.Id);
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error editing comment.", ex);
            }
        }

        public async Task DeleteCommentAsync(Document document, Comment comment)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"select \"{nameof(Document.CommentsString)}\" " + $"from \"{nameof(Document)}\" " + $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@documentId", document.Id);
                    var result = cmd.ExecuteQuery<CommentsValue>();

                    if (result == null || result.Count < 1)
                        return;

                    var comments = result.First().Comments;

                    comments.RemoveAll(cm => cm.Id == comment.Id);

                    cmd = c.CreateCommand($"update \"{nameof(Document)}\" " + $"set \"{nameof(Document.CommentsString)}\" = @commentsString " + $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@commentsString",
                        new CommentsValue
                        {
                            Comments = comments
                        }.CommentsString);
                    cmd.Bind("@documentId", document.Id);
                    cmd.ExecuteNonQuery();

                    cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " + $"set \"{nameof(DocumentPreview.CommentsCount)}\" = @commentsCount " + $"where \"{nameof(DocumentPreview.Id)}\" = @documentId");
                    cmd.Bind("@commentsCount", comments.Count);
                    cmd.Bind("@documentId", document.Id);
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting comment.", ex);
            }
        }

        public async Task<int[]> GetNonCachedDocumentIdsAsync(int[] folderIds, int limit = -1)
        {
            try
            {
                var ids = new int[0];

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select distinct DP.{nameof(DocumentPreview.Id)} " +
                                $"from {nameof(DocumentPreview)} as DP " +
                                $"where DP.{nameof(DocumentPreview.Id)} not in (select {nameof(Document.Id)} from {nameof(Document)}) " +
                                $" and DP.{nameof(DocumentPreview.Id)} in (select distinct {nameof(FolderDocumentLink.DocumentId)} from {nameof(FolderDocumentLink)} where {nameof(FolderDocumentLink.FolderId)} in ({string.Join(",", folderIds)})) " +
                                $"order by DP.{nameof(DocumentPreview.Id)} desc ";

                    if (limit > 0)
                        query += $"limit {limit}";

                    var result = c.Query<IdValue>(query);
                    ids = result.Select(v => v.Id).ToArray();
                });
                return ids;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting document preview IDs of non-cached documents.", ex);
            }
        }

        public async Task RemoveOrphans()
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var innerSelectQueryText = $"select {nameof(FolderDocumentLink.DocumentId)} from {nameof(FolderDocumentLink)}";

                    var outerDeleteQueryDocumentPreview = $"delete from {nameof(DocumentPreview)} where {nameof(DocumentPreview.Id)} not in ({innerSelectQueryText}) ";
                    var cmd = c.CreateCommand(outerDeleteQueryDocumentPreview);
                    cmd.ExecuteNonQuery();

                    var outerDeleteQueryDocument = $"delete from {nameof(Document)} where {nameof(Document.Id)} not in ({innerSelectQueryText}) ";
                    var cmd2 = c.CreateCommand(outerDeleteQueryDocument);
                    cmd2.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing orphan documents and document previews.", ex);
            }
        }

        private async Task<List<int>> GetLinkedFoldersIds(int documentId)
        {
            try
            {
                List<int> linkedFoldersId = null;

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select {nameof(FolderDocumentLink.FolderId)} " + $"from {nameof(FolderDocumentLink)} "
                    + $"where {nameof(FolderDocumentLink.DocumentId)} = {documentId} ";

                    var result = c.Query<int>(query);

                    if (result == null || result.Count < 1)
                        throw new DataNotFoundException($"Linked folders for document {documentId} could not be found.");

                    linkedFoldersId = result;
                });

                return linkedFoldersId;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException($"Error getting linked folders for document {documentId}.", ex);
            }
        }

        #region IRestorable

        public async Task RestoreDeletedObjectsAsync(List<int> ids)
        {
            try
            {
                var deletedDocumentPreviews = await restorationDataAccess.GetDeletedObjectsAsync(ids, DeletedObjectType.DocumentPreview);
                var documentPreviews = deletedDocumentPreviews.Select(dd => Serializer.Deserialize<DocumentPreview>(dd.SerializedObject)).ToList();

                var deletedDocuments = await restorationDataAccess.GetDeletedObjectsAsync(ids, DeletedObjectType.Document);
                var documents = deletedDocuments.Select(dd => Serializer.Deserialize<Document>(dd.SerializedObject)).ToList();

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplaceAll(documentPreviews);
                    c.InsertOrReplaceAll(documents);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while restoring deleted documents.", ex);
            }
        }

        public async Task SaveDeletedObjectsAsync<T>(List<T> businessEntities) where T : IBusinessEntity
        {
            try
            {
                await restorationDataAccess.SaveDeletedObjects(businessEntities);

                foreach (var be in businessEntities)
                {
                    var linkedFoldersIds = await GetLinkedFoldersIds(be.Id);
                    await restorationDataAccess.SaveDeletedObjectLinkedFolders(be.Id, linkedFoldersIds);
                }
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while saving deleted documents.", ex);
            }
        }

        #endregion
    }
}