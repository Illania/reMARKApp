using System.Collections.Generic;
using reMark.Mobile.Common.Utilities;
using SQLite;

namespace reMark.Mobile.Common.Model
{
    public class CategoriesValue
    {
        [Column("CategoriesString")]
        public string CategoriesString { get; set; }

        [Ignore]
        public List<Category> Categories { get => Serializer.Deserialize<List<Category>>(CategoriesString); set => CategoriesString = Serializer.Serialize(value); }
    }
}