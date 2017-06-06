//
// File: SearchDocumentsCriteria.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class SearchDocumentsCriteria
    {
        public string SavedSearchFilterHash { get; set; }
        public int MaxToFetch { get; set; } = -1;

        public string SubjectMessageField { get; set; }
        public SubjectMessageClause SubjectMessageClause { get; set; }
        public string FromToField { get; set; }
        public FromToClause FromToClause { get; set; }
        public bool SearchInAttachments { get; set; }
        public bool UnreadOnly { get; set; }
        public bool PartialWordSearch { get; set; }
        public bool? Handled { get; set; }
        public string Reference { get; set; }
        List<Priority> priorities;

        public List<Priority> Priorities
        {
            get
            {
                if (priorities == null)
                {
                    priorities = new List<Priority>();
                }
                return priorities;
            }
            set { priorities = value; }
        }

        List<DocumentDirection> directions;

        public List<DocumentDirection> Directions
        {
            get
            {
                if (directions == null)
                {
                    directions = new List<DocumentDirection>();
                }
                return directions;
            }
            set { directions = value; }
        }

        List<int> categoryIds;

        public List<int> CategoryIds
        {
            get
            {
                if (categoryIds == null)
                {
                    categoryIds = new List<int>();
                }
                return categoryIds;
            }
            set { categoryIds = value; }
        }

        List<int> mustHaveCategoryIds;

        public List<int> MustHaveCategoryIds
        {
            get
            {
                if (mustHaveCategoryIds == null)
                {
                    mustHaveCategoryIds = new List<int>();
                }
                return mustHaveCategoryIds;
            }
            set { mustHaveCategoryIds = value; }
        }

        List<Guid> lineGuids;

        public List<Guid> LineGuids
        {
            get
            {
                if (lineGuids == null)
                {
                    lineGuids = new List<Guid>();
                }
                return lineGuids;
            }
            set { lineGuids = value; }
        }

        List<Guid> creatorGuids;

        public List<Guid> CreatorGuids
        {
            get
            {
                if (creatorGuids == null)
                {
                    creatorGuids = new List<Guid>();
                }
                return creatorGuids;
            }
            set { creatorGuids = value; }
        }

        public DateRange DateRange { get; set; }
        public string Comment { get; set; }
        public string AttachmentName { get; set; }
        public bool HavingAttachmentsOnly { get; set; }
        public FiledInFolderType FiledInFolderType { get; set; }
        public FiledInFolderFolderType FiledInFolderFolderType { get; set; }
        List<int> filedInFolderIds;

        public List<int> FiledInFolderIds
        {
            get
            {
                if (filedInFolderIds == null)
                {
                    filedInFolderIds = new List<int>();
                }
                return filedInFolderIds;
            }
            set { filedInFolderIds = value; }
        }

        public string ExtraFields { get; set; }
    }
}