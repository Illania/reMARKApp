namespace Mark5.Mobile.Common.Model
{
    public class ReminderInfo
    {
        public ReminderType Type { get; }

        public string Title
        {
            get
            {
                switch (Type)
                {
                    case ReminderType.None:
                        return "None";
                    case ReminderType.AtTheTime:
                        return "At time of event";
                    case ReminderType.FiveMinutes:
                        return "5 minutes before";
                    case ReminderType.TenMinutes:
                        return "10 minutes before";
                    case ReminderType.FifteenMinutes:
                        return "15 minutes before";
                    case ReminderType.ThirtyMinutes:
                        return "30 minutes before";
                    case ReminderType.OneHour:
                        return "1 hour before";
                    case ReminderType.TwoHours:
                        return "2 hour before";
                    case ReminderType.OneDay:
                        return "1 day before";
                    case ReminderType.TwoDays:
                        return "2 days before";
                    case ReminderType.OneWeek:
                        return "1 week before";
                    default:
                        return "";
                }
            }
        }

        public int Seconds
        {
            get
            {
                return (int)Type * 60;
            }
        }

        public ReminderInfo(ReminderType type)
        {
            Type = type;
        }

        public enum ReminderType
        {
            None = -1,
            AtTheTime = 0,
            FiveMinutes = 5,
            TenMinutes = 10,
            FifteenMinutes = 15,
            ThirtyMinutes = 30,
            OneHour = 60,
            TwoHours = 120,
            OneDay = 24 * 60,
            TwoDays = 48 * 60,
            OneWeek = 24 * 7 * 60
        };

        public override string ToString()
        {
            return Title;
        }

        public static ReminderInfo ConvertFromSeconds(int seconds)
        {
            if (seconds < 0)
                return new ReminderInfo(ReminderType.None);

            var minutes = seconds / 60;
            return new ReminderInfo((ReminderType)minutes);
        }

        public override bool Equals(object obj)
        {
            return ((ReminderInfo)obj)?.Type == Type;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }
}