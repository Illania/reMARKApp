//
// Project: Mark5.Mobile.Common
// File: SearchContactsCriteria.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{

    public class SearchContactsCriteria
    {

        public string SavedSearchFilterHash { get; set; }

        public int MaxToFetch { get; set; } = -1;

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ShortId { get; set; }

        public string Description { get; set; }

        HashSet<ContactType> contactTypes;

        public HashSet<ContactType> ContactTypes
        {
            get
            {
                if (contactTypes == null)
                {
                    contactTypes = new HashSet<ContactType>();
                }

                return contactTypes;
            }
            set
            {
                contactTypes = value;
            }
        }

        public string ComAddress { get; set; }

        public string PostAddress { get; set; }

        public string Comment { get; set; }

        public string Vat { get; set; }

        public string Ledger { get; set; }

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
            set
            {
                categoryIds = value;
            }
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
            set
            {
                mustHaveCategoryIds = value;
            }
        }

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
            set
            {
                filedInFolderIds = value;
            }
        }

        public int CountryPrefix { get; set; } = -1;

        List<int> responsibleIds;

        public List<int> ResponsibleIds
        {
            get
            {
                if (responsibleIds == null)
                {
                    responsibleIds = new List<int>();
                }

                return responsibleIds;
            }
            set
            {
                responsibleIds = value;
            }
        }
    }
}

