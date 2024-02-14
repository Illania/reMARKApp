using System.Collections.Generic;

namespace reMark.Mobile.Common.Model.Exceptions
{
    public static class ErrorConstants
    {
        public static class Codes
        {
            public static string FileTooLarge = "file_too_large";
            public static string InvalidSourceType = "invalid_source_type";
            public static string FileCouldNotBeLoaded = "file_could_not_be_loaded";
            public static string LoginNeeded = "login_needed";
            public static string UnsupportedFile = "unsupported_file";
            public static string CalendarEventNotFound = "calendar_event_not_found";
        }

        private static Dictionary<string, string> Messages = new Dictionary<string, string>
        {
            { Codes.FileTooLarge,  "File size exceeds the maximum size limit." },
            { Codes.InvalidSourceType, "This action can only be performed when online." },
            { Codes.FileCouldNotBeLoaded, "File could not be loaded." },
            { Codes.LoginNeeded, "You need to log in to reMARK before you can use the app." },
            { Codes.UnsupportedFile, "Unsupported file." },
             { Codes.CalendarEventNotFound, "Calendar event doesn't exist." }
        };

        public static string Message(string code)
        {
            return Messages.ContainsKey(code)
                ? Messages[code]
                : "Unknown error";
        }

    }
}
