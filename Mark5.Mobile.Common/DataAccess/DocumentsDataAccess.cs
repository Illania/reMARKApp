//
// Project: Mark5.Mobile.Common
// File: DocumentsDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Links;

namespace Mark5.Mobile.Common.DataAccess
{

    class DocumentsDataAccess : IDocumentsDataAccess
    {

        readonly DatabaseConnectionProvider documentsDatabase;

        public DocumentsDataAccess(DatabaseConnectionProvider documentsDatabase)
        {
            this.documentsDatabase = documentsDatabase;
        }

        public async Task SaveDocumentPreviewsAsync(Folder folder, List<DocumentPreview> documentPreviews, bool clean)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    if (clean)
                    {
                        c.Table<FolderDocumentLink>()
                         .Delete(fdl => fdl.FolderId == folder.Id);
                    }

                    c.InsertOrReplaceAll(documentPreviews.Select(dp => new FolderDocumentLink { FolderId = folder.Id, DocumentId = dp.Id }));
                    c.InsertOrReplaceAll(documentPreviews);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving document previews.", ex);
            }
        }

        public async Task<List<DocumentPreview>> GetDocumentPreviewsAsync(Folder folder, int startId = -1, int endId = -1, int maxItems = 500)
        {
            try
            {
                List<DocumentPreview> documentPreviews = null;

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var query = c.Table<FolderDocumentLink>()
                                 .Where(fdl => fdl.FolderId == folder.Id)
                                 .Join(c.Table<DocumentPreview>(), fdl => fdl.DocumentId, dp => dp.Id, (fdl, dp) => dp)
                                 .OrderByDescending(dp => dp.Id);

                    if (startId > 0)
                    {
                        query = query.Where(dp => dp.Id < startId);
                    }

                    if (endId > 0)
                    {
                        query = query.Where(dp => dp.Id > endId);
                    }

                    if (maxItems > 0)
                    {
                        query = query.Take(maxItems);
                    }

                    var result = query.ToList();

                    if (result == null || result.Count < 1)
                    {
                        throw new DataNotFoundException("Document previews could not be found.");
                    }

                    documentPreviews = result;
                });

                return documentPreviews;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting documents.", ex);
            }
        }

        public async Task SaveDocumentAsync(Document document)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(document);
                });
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

                    if (result == null)
                    {
                        throw new DataNotFoundException("Document could not be found.");
                    }

                    document = result;
                });

                return document;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting document.", ex);
            }
        }

        public async Task SetDocumentPreviewsReadStatusAsync(List<DocumentPreview> documentPreviews, bool isRead)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var documentPreview in documentPreviews)
                    {
                        var cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " +
                                                  $"set \"{nameof(DocumentPreview.IsReadByCurrent)}\" = @isReadByCurrent " +
                                                  $"   and \"{nameof(DocumentPreview.IsReadByAnyone)}\" = @isReadByAnyone " +
                                                  $"where \"{nameof(DocumentPreview.Id)}\" = @documentPreviewId");
                        cmd.Bind("@isReadByCurrent", isRead);
                        cmd.Bind("@isReadByAnyone", documentPreview.IsReadByAnyone || isRead);
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

        public async Task SetDocumentPreviewsPriorityAsync(List<DocumentPreview> documentPreviews, Priority priority)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var documentPreview in documentPreviews)
                    {
                        var cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " +
                                                  $"set \"{nameof(DocumentPreview.Priority)}\" = @priority " +
                                                  $"where \"{nameof(DocumentPreview.Id)}\" = @documentPreviewId");
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

        public async Task RemoveFromFolderAsync(List<DocumentPreview> documentPreviews, Folder folder)
        {
            var ids = documentPreviews.Select(dp => dp.Id).Distinct().ToList();
            await RemoveFromFolderAsync(ids, folder.Id);
        }

        public async Task RemoveFromFolderAsync(List<Document> documents, Folder folder)
        {
            var ids = documents.Select(d => d.Id).Distinct().ToList();
            await RemoveFromFolderAsync(ids, folder.Id);
        }

        async Task RemoveFromFolderAsync(List<int> ids, int folderId)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var id in ids)
                    {
                        var linksCount = c.Table<FolderDocumentLink>().Count(fdl => fdl.DocumentId == id);
                        if (linksCount == 1)
                        {
                            c.Table<DocumentPreview>().Delete(dp => dp.Id == id);
                            c.Table<Document>().Delete(d => d.Id == id);
                        }

                        c.Table<FolderDocumentLink>().Delete(fdl => fdl.DocumentId == id && fdl.FolderId == folderId);
                    }
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing documents from folder.", ex);
            }
        }

        public async Task DeleteAsync(List<DocumentPreview> documentPreviews)
        {
            var ids = documentPreviews.Select(dp => dp.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        public async Task DeleteAsync(List<Document> documents)
        {
            var ids = documents.Select(d => d.Id).Distinct().ToList();
            await DeleteAsync(ids);
        }

        async Task DeleteAsync(List<int> ids)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.Table<FolderDocumentLink>().Delete(fdl => ids.Contains(fdl.DocumentId));
                    c.Table<DocumentPreview>().Delete(dp => ids.Contains(dp.Id));
                    c.Table<Document>().Delete(d => ids.Contains(d.Id));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting documents.", ex);
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
                    {
                        throw new DataNotFoundException("Template previews could not be found.");
                    }

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
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(template);
                });
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
                    {
                        throw new DataNotFoundException("Template could not be found.");
                    }

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
                    c.InsertOrReplace(new DefaultTemplateInfo { CreationModeFlag = creationModeFlag, Available = template != null, TemplateId = template?.Id ?? -1 });

                    if (template != null)
                    {
                        c.InsertOrReplace(template);
                    }
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
                    {
                        throw new DataNotFoundException("Default template info could not be found.");
                    }

                    if (info.Available)
                    {
                        var result = c.Find<Template>(info.TemplateId);

                        if (result == null)
                        {
                            throw new DataNotFoundException("Default template could not be found.");
                        }

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

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    recentAddresses = c.Table<RecentAddress>().ToList();
                });

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

                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    categories = c.Table<Category>().ToList();
                });

                return categories;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting categories.", ex);
            }
        }

        public async Task SetCategoriesAsync(DocumentPreview documentPreview, List<Category> categories)
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " +
                                              $"set \"{nameof(DocumentPreview.CategoriesBytes)}\" = @categoriesBytes " +
                                              $"where \"{nameof(DocumentPreview.Id)}\" = @documentPreviewId");
                    cmd.Bind("@categoriesBytes", new CategoriesValue { Categories = categories }.CategoriesBytes);
                    cmd.Bind("@documentPreviewId", documentPreview.Id);
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
                    var cmd = c.CreateCommand($"select \"{nameof(Document.CommentsBytes)}\" " +
                                    $"from \"{nameof(Document)}\" " +
                                    $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@documentId", document.Id);
                    var result = cmd.ExecuteQuery<CommentsValue>();

                    if (result == null || result.Count < 1)
                    {
                        return;
                    }

                    var comments = result.First().Comments;

                    comments.Add(comment);
                    comments = comments.OrderBy(cm => cm.DateAdded).ToList();

                    cmd = c.CreateCommand($"update \"{nameof(Document)}\" " +
                                          $"set \"{nameof(Document.CommentsBytes)}\" = @commentsBytes " +
                                          $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
                    cmd.Bind("@documentId", document.Id);
                    cmd.ExecuteNonQuery();

                    cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " +
                                          $"set \"{nameof(DocumentPreview.CommentsCount)}\" = @commentsCount " +
                                          $"where \"{nameof(DocumentPreview.Id)}\" = @documentId");
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
                    var cmd = c.CreateCommand($"select \"{nameof(Document.CommentsBytes)}\" " +
                                    $"from \"{nameof(Document)}\" " +
                                    $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@documentId", document.Id);
                    var result = cmd.ExecuteQuery<CommentsValue>();

                    if (result == null || result.Count < 1)
                    {
                        return;
                    }

                    var comments = result.First().Comments;

                    comments.RemoveAll(cm => cm.Id == comment.Id);
                    comments.Add(comment);
                    comments = comments.OrderBy(cm => cm.DateAdded).ToList();

                    cmd = c.CreateCommand($"update \"{nameof(Document)}\" " +
                                          $"set \"{nameof(Document.CommentsBytes)}\" = @commentsBytes " +
                                          $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
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
                    var cmd = c.CreateCommand($"select \"{nameof(Document.CommentsBytes)}\" " +
                                              $"from \"{nameof(Document)}\" " +
                                              $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@documentId", document.Id);
                    var result = cmd.ExecuteQuery<CommentsValue>();

                    if (result == null || result.Count < 1)
                    {
                        return;
                    }

                    var comments = result.First().Comments;

                    comments.RemoveAll(cm => cm.Id == comment.Id);

                    cmd = c.CreateCommand($"update \"{nameof(Document)}\" " +
                                          $"set \"{nameof(Document.CommentsBytes)}\" = @commentsBytes " +
                                          $"where \"{nameof(Document.Id)}\" = @documentId");
                    cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
                    cmd.Bind("@documentId", document.Id);
                    cmd.ExecuteNonQuery();

                    cmd = c.CreateCommand($"update \"{nameof(DocumentPreview)}\" " +
                                          $"set \"{nameof(DocumentPreview.CommentsCount)}\" = @commentsCount " +
                                          $"where \"{nameof(DocumentPreview.Id)}\" = @documentId");
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

        public async Task RemoveOrphans()
        {
            try
            {
                await documentsDatabase.RunInConnectionAsync(c =>
                {
                    var innerSelectQueryText = $"select {nameof(FolderDocumentLink.DocumentId)} from {nameof(FolderDocumentLink)}";

                    var outerDeleteQueryPreview = $"delete from {nameof(DocumentPreview)} where {nameof(DocumentPreview.Id)} not in ({innerSelectQueryText}) ";
                    var cmd = c.CreateCommand(outerDeleteQueryPreview);
                    cmd.ExecuteNonQuery();

                    var outerDeleteQueryContact = $"delete from {nameof(Document)} where {nameof(Document.Id)} not in ({innerSelectQueryText}) ";
                    var cmd2 = c.CreateCommand(outerDeleteQueryContact);
                    cmd2.ExecuteNonQuery();
                });

            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing orphan documents and document previews.", ex);
            }
        }
    }
}

