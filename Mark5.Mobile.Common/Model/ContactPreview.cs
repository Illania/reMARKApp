using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    [Table("ContactPreview")]
    public class ContactPreview : BusinessEntityPreview
    {
        [Ignore]
        public override ObjectType ObjectType => ObjectType.Contact;

        [Ignore]
        public override ModuleType ModuleType => ModuleType.Contacts;

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
                    categories = new List<Category>();
                return categories;
            }
            set => categories = value;
        }

        [Ignore]
        public CommunicationAddress PrimaryAddress { get; set; }

        #region Serialization

        [Column("CategoriesString")]
        public string CategoriesString
        {
            get => SerializationUtils.Serialize(Categories);
            set => Categories = SerializationUtils.Deserialize<List<Category>>(value);
        }

        [Column("PrimaryAddressString")]
        public string PrimaryAddressString
        {
            get => SerializationUtils.Serialize(PrimaryAddress);
            set => PrimaryAddress = SerializationUtils.Deserialize<CommunicationAddress>(value);
        }

        #endregion

        public override string ToString()
        {
            return $"[ContactPreview: Id={Id}, RowId={RowId}, Name={Name}]";
        }
    }
}