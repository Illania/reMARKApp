using System;

namespace reMark.Mobile.Common.Model
{
    public class ExtraField : ICopiable<ExtraField>, IEquatable<ExtraField>, IComparable<ExtraField>
    {
        public int FieldId { get; set; } = -1;
        public string FieldName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        

        public ExtraField ShallowCopy()
        {
            return new ExtraField
            {
                FieldId = FieldId,
                FieldName = FieldName,
                Enabled = Enabled
            };
        }

        public ExtraField DeepCopy()
        {
            return ShallowCopy();
        }

        public bool Equals(ExtraField obj)
        {
            if (obj == null) return false;
            if (!(obj is ExtraField objAsPart)) return false;
            else return Equals(objAsPart);
        }

        public int CompareTo(ExtraField compareExtraField)
        {
            if (compareExtraField == null)
                return 1;

            else
                return this.FieldName.CompareTo(compareExtraField.FieldName);
        }
    }
}