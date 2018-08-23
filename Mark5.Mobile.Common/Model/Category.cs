using SQLite;
using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    [Table("Category")]
    public class Category
    {
        [Column("Id")]
        [PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("Guid")]
        public Guid Guid { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("HexColor")]
        public string HexColor { get; set; }

        public override string ToString()
        {
            return $"[Category: Id={Id}, Name={Name}]";
        }

        public override bool Equals(object obj)
        {
            return (obj is Category y) && this.Id.Equals(y.Id);
        }
    }

    public class CategoryComparer : IEqualityComparer<Category>
    {

        public bool Equals(Category x, Category y)
        {
            return x != null && y != null && x.Id.Equals(y.Id);
        }

        public int GetHashCode(Category obj)
        {
            return obj.Id;
        }

    }
}