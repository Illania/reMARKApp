//
// Project: Mark5.Mobile.Common
// File: DocumentPreview.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    [Table("DocumentPreview")]
    public class DocumentPreview : BusinessEntityPreview
    {

        [Ignore]
        public override ObjectType ObjectType
        {
            get
            {
                return ObjectType.Document;
            }
        }

        [Ignore]
        public override ModuleType ModuleType
        {
            get
            {
                return ModuleType.Documents;
            }
        }

        [Column("ReferenceNumber")]
        public string ReferenceNumber { get; set; }

        List<DocumentAddress> addresses;

        [Ignore]
        public List<DocumentAddress> Addresses
        {
            get
            {
                if (addresses == null)
                {
                    addresses = new List<DocumentAddress>();
                }

                return addresses;
            }

            set
            {
                addresses = value;
            }
        }

        [Column("Subject")]
        public string Subject { get; set; }

        [Column("Preview")]
        public string Preview { get; set; }

        [Column("Direction")]
        public DocumentDirection Direction { get; set; }

        [Column("Priority")]
        public Priority Priority { get; set; }

        [Column("IsReadByAnyone")]
        public bool IsReadByAnyone { get; set; }

        [Column("IsReadByCurrent")]
        public bool IsReadByCurrent { get; set; }

        [Column("CommentsCount")]
        public int CommentsCount { get; set; }

        [Column("AttachmentsCount")]
        public int AttachmentsCount { get; set; }

        List<Category> categories;

        [Ignore]
        public List<Category> Categories
        {
            get
            {
                if (categories == null)
                {
                    categories = new List<Category>();
                }
                return categories;
            }
            set
            {
                categories = value;
            }
        }

        [Column("DateReceivedTimestamp")]
        public long DateReceivedTimestamp { get; set; } = -1;

        [Column("CreatorId")]
        public int CreatorId { get; set; }

        [Column("Creator")]
        public string Creator { get; set; }

        #region Serialization

        [Column("AddressesBytes")]
        public byte[] AddressesBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Addresses);
            }
            set
            {
                Addresses = SerializationUtils.DeserializeFromByteArray<List<DocumentAddress>>(value);
            }
        }

        [Column("CategoriesBytes")]
        public byte[] CategoriesBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Categories);
            }
            set
            {
                Categories = SerializationUtils.DeserializeFromByteArray<List<Category>>(value);
            }
        }

        #endregion

        public override string ToString()
        {
            return $"[DocumentPreview: Id={Id}, ReferenceNumber={ReferenceNumber}, Subject={Subject}, DateReceivedTimestamp={DateReceivedTimestamp}]";
        }
    }
}

