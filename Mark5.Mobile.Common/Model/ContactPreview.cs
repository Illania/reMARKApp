//
// Project: Mark5.Mobile.Common
// File: ContactPreview.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    [Table("ContactPreview")]
    public class ContactPreview : BusinessEntityPreview
    {

        [Ignore]
        public override ObjectType ObjectType
        {
            get
            {
                return ObjectType.Contact;
            }
        }

        [Ignore]
        public override ModuleType ModuleType
        {
            get
            {
                return ModuleType.Contacts;
            }
        }

        [Ignore]
        public int RowId { get; set; } = -1;

        [Column("Name")]
        public string Name { get; set; }

        [Column("CompanyName")]
        public string CompanyName { get; set; }

        [Column("ShortId")]
        public string ShortId { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("Type")]
        public ContactType Type { get; set; }

        [Column("CommentsCount")]
        public int CommentsCount { get; set; }

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

        [Ignore]
        public CommunicationAddress PrimaryAddress { get; set; }

        #region Serialization

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

        [Column("PrimaryAddressBytes")]
        public byte[] PrimaryAddressBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(PrimaryAddress);
            }
            set
            {
                PrimaryAddress = SerializationUtils.DeserializeFromByteArray<CommunicationAddress>(value);
            }
        }

        #endregion

        public override string ToString()
        {
            return $"[ContactPreview: Id={Id}, RowId={RowId}, Name={Name}]";
        }
    }
}

