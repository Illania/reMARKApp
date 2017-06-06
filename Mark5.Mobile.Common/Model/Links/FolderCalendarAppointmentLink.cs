using SQLite;

namespace Mark5.Mobile.Common.Model.Links
{
    [Table("FolderCalendarAppointmentLink")]
    class FolderCalendarAppointmentLink
    {
        [Column("FolderId")]
        [Indexed]
        public int FolderId { get; set; } = -1;

        [Column("CalendarAppointmentId")]
        [Indexed]
        public int CalendarAppointmentId { get; set; } = -1;
    }
}