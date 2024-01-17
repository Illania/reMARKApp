using System;
using System.Collections.Generic;

namespace reMark.Mobile.Common.Model
{
    public class DocumentExtraFieldInfo: IEquatable<DocumentExtraFieldInfo>, IComparable<DocumentExtraFieldInfo>
    {
        public int Id { get; set; } = -1;

        public string Name { get; set; }

        public int CompareTo(DocumentExtraFieldInfo other)
        {
            if (other == null)
                return 1;

            else
                return Name.CompareTo(other.Name);
        }

        public bool Equals(DocumentExtraFieldInfo other)
        {
            if (other == null) return false;
            if (!(other is DocumentExtraFieldInfo)) return false;
            else return Id.Equals(other.Id) && Name.Equals(other.Name);
        }
    }

    public class DocumentExtraFieldInfoEqualityComparer : IEqualityComparer<DocumentExtraFieldInfo>
    {
        public bool Equals(DocumentExtraFieldInfo field1, DocumentExtraFieldInfo field2)
        {
            if (field2 == null && field1 == null)
                return true;
            else if (field1 == null || field2 == null)
                return false;
            else if (field1.Id == field2.Id && field1.Name == field2.Name)
                return true;
            else
                return false;
        }

        public int GetHashCode(DocumentExtraFieldInfo field)
        {
            int hCode = field.Id ^ field.Name.GetHashCode();
            return hCode.GetHashCode();
        }
    }
}