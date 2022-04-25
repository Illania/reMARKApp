namespace Mark5.Mobile.Common.Model
{
    public class ExtraField : ICopiable<ExtraField>
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
    }
}