using System;

namespace Mark5.Mobile.Common.Model
{
    public class Calendar
    {
        public int Id { get; set; } = -1;

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string ColorHex { get; set; }
        public bool Shared { get; set; }
    }
}