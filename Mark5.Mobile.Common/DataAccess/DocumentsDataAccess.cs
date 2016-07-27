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
            await documentsDatabase.RunInConnectionAsync(c =>
            {
                var documentPreviewIds = documentPreviews.Select(dp => dp.Id).ToList();

                if (clean)
                {
                    c.Table<FolderDocumentLink>()
                     .Delete(fdl => fdl.FolderId == folder.Id && documentPreviewIds.Contains(fdl.DocumentId));
                }

                c.InsertOrReplace(documentPreviews.Select(dp => new FolderDocumentLink { FolderId = folder.Id, DocumentId = dp.Id }));
                c.InsertOrReplace(documentPreviews);
            });
        }

        public async Task<List<DocumentPreview>> GetDocumentPreviewsAsync(Folder folder, int startId = -1, int endId = -1, int maxItems = 500)
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

        public async Task SaveDocumentAsync(Document document)
        {
            await documentsDatabase.RunInConnectionAsync(c =>
            {
                c.InsertOrReplace(document);
            });
        }

        public async Task<Document> GetDocumentAsync(int documentId)
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

        public async Task SetDocumentPreviewsReadStatusAsync(DocumentPreview[] documentPreviews, bool isRead)
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

        public async Task SetDocumentPreviewsPriorityAsync(DocumentPreview[] documentPreviews, Priority priority)
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

        public async Task DeleteDocumentPreviewsAndDocumentsAsync(int[] ids)
        {
            await documentsDatabase.RunInConnectionAsync(c =>
            {
                c.Table<FolderDocumentLink>().Delete(fdl => ids.Contains(fdl.DocumentId));
                c.Table<DocumentPreview>().Delete(dp => ids.Contains(dp.Id));
                c.Table<Document>().Delete(d => ids.Contains(d.Id));
            });
        }

        public async Task SaveTemplatePreviewsAsync(List<TemplatePreview> templatePreviews)
        {
            await documentsDatabase.RunInConnectionAsync(c =>
            {
                c.DeleteAll<TemplatePreview>();
                c.InsertAll(templatePreviews);
            });
        }

        public async Task<List<TemplatePreview>> GetTemplatePreviewsAsync()
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

        public async Task SaveTemplateAsync(Template template)
        {
            await documentsDatabase.RunInConnectionAsync(c =>
            {
                c.InsertOrReplace(template);
            });
        }

        public async Task<Template> GetTemplateAsync(int templateId)
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

        public async Task SaveDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag, Template template)
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

        public async Task<Template> GetDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag)
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

        public async Task SaveRecentAddressesAsync(List<RecentAddress> recentAddresses)
        {
            await documentsDatabase.RunInConnectionAsync(c =>
            {
                c.DeleteAll<RecentAddress>();
                c.InsertAll(recentAddresses);
            });
        }

        public async Task<List<RecentAddress>> GetRecentAddressesAsync()
        {
            List<RecentAddress> recentAddresses = null;

            await documentsDatabase.RunInConnectionAsync(c =>
            {
                recentAddresses = c.Table<RecentAddress>().ToList();
            });

            return recentAddresses;
        }

        public async Task AddCommentAsync(Document document, Comment comment)
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
                    throw new DataNotFoundException("Document could not be found.");
                }

                var comments = result.First().Comments;

                comments.Add(comment);
                comments = comments.OrderBy(cm => cm.DateAdded).ToList();

                cmd = c.CreateCommand($"update \"{nameof(Document)}\" " +
                                      $"set \"{nameof(Document.CommentsBytes)}\" = @commentsBytes " +
                                      $"where \"{nameof(Document.Id)}\" = documentId");
                cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
                cmd.Bind("@documentId", document.Id);
                cmd.ExecuteNonQuery();
            });
        }

        public async Task EditCommentAsync(Document document, Comment comment)
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
                    throw new DataNotFoundException("Document could not be found.");
                }

                var comments = result.First().Comments;

                comments.RemoveAll(cm => cm.Id == comment.Id);
                comments.Add(comment);
                comments = comments.OrderBy(cm => cm.DateAdded).ToList();

                cmd = c.CreateCommand($"update \"{nameof(Document)}\" " +
                                      $"set \"{nameof(Document.CommentsBytes)}\" = @commentsBytes " +
                                      $"where \"{nameof(Document.Id)}\" = documentId");
                cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
                cmd.Bind("@documentId", document.Id);
                cmd.ExecuteNonQuery();
            });
        }

        public async Task DeleteCommentAsync(Document document, Comment comment)
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
                    throw new DataNotFoundException("Document could not be found.");
                }

                var comments = result.First().Comments;

                comments.RemoveAll(cm => cm.Id == comment.Id);

                cmd = c.CreateCommand($"update \"{nameof(Document)}\" " +
                                      $"set \"{nameof(Document.CommentsBytes)}\" = @commentsBytes " +
                                      $"where \"{nameof(Document.Id)}\" = documentId");
                cmd.Bind("@commentsBytes", new CommentsValue { Comments = comments }.CommentsBytes);
                cmd.Bind("@documentId", document.Id);
                cmd.ExecuteNonQuery();
            });
        }
    }
}

