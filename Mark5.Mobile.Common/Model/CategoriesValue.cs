using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class CategoriesValue
    {
        [Column("CategoriesString")]
        public string CategoriesString { get; set; }

        [Ignore]
        public List<Category> Categories { get => Serializer.Deserialize<List<Category>>(CategoriesString); set => CategoriesString = Serializer.Serialize(value); }
    }
}