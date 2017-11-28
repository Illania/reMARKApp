using Mark5.Mobile.Common.Model;
using System.Collections.Generic;
using Foundation;

namespace Mark5.Mobile.IOS.Utilities
{
    public class Preferences
    {
        public enum TemplateUsageMode
        {
            DontUse = 0,
            Default = 1,
            Local = 2,
            AlwaysAsk = 3,
        }

        class Keys
        {
            public const string ShowCreatorOutgoing = "ShowCreatorOutgoing";
            public const string UseServerTimezoneKey = "UseServerTimezone";
            public const string DocumentsToDownloadKey = "DocumentsToDownload";
            public const string UnreadIndicatorMeKey = "UnreadIndicatorMe";
            public const string CompactDocumentsListKey = "CompactDocumentsList";
            public const string MarkAsReadDelaySecondsKey = "MarkAsReadDelaySeconds";
            public const string DocumentBodyRequestTypeKey = "DocumentBodyRequestType";
            public const string LargeAttachmentWarningKey = "LargeAttachmentWarning";
            public const string HideReadNotificationsKey = "HideReadNotifications";

            public const string ComposePriorityEnabledKey = "ComposePriorityEnabled";
            public const string RemoveLineKey = "RemoveLine";
            public const string UseTemplateKey = "UseTemplate";
            public const string LocalTemplateKey = "LocalTemplate";
            public const string AlwaysUseDefaultLine = "AlwaysUseDefaultLine";

            public const string DocumentsToSearchKey = "DocumentsToSearch";
            public const string ContactsToSearchKey = "ContactsToSearch";
            public const string ShortcodesToSearchKey = "ShortcodesToSearch";
            public const string PartialWordSearchKey = "PartialWordSearch";

            public const string CleanCacheIntervalDaysKey = "CleanCacheIntervalDays";
            public const string ClearCacheKey = "ClearCache";
            public const string EnableReportingKey = "EnableReporting";

            public const string PushNotificationTokenKey = "PushNotificationToken";

            public const string ResetOnLaunchKey = "ResetOnLaunch";
        }

        readonly NSUserDefaults ud;

        public Preferences()
        {
            ud = NSUserDefaults.StandardUserDefaults;
            RegisterDefaults();
        }

        void RegisterDefaults()
        {
            var defaultsDictionary = new NSMutableDictionary
            {
                {
                    new NSString(Keys.ShowCreatorOutgoing), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.UseServerTimezoneKey), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.DocumentsToDownloadKey), NSNumber.FromInt16(250)
                },
                {
                    new NSString(Keys.UnreadIndicatorMeKey), NSNumber.FromBoolean(true)
                },
                {
                    new NSString(Keys.MarkAsReadDelaySecondsKey), NSNumber.FromInt16(2)
                },
                {
                    new NSString(Keys.CompactDocumentsListKey), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.DocumentBodyRequestTypeKey), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.LargeAttachmentWarningKey), NSNumber.FromBoolean(true)
                },
                {
                    new NSString(Keys.HideReadNotificationsKey), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.ComposePriorityEnabledKey), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.RemoveLineKey), NSNumber.FromBoolean(true)
                },
                {
                    new NSString(Keys.UseTemplateKey), NSNumber.FromInt16(1)
                },
                {
                    new NSString(Keys.AlwaysUseDefaultLine), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.DocumentsToSearchKey), NSNumber.FromInt16(250)
                },
                {
                    new NSString(Keys.ContactsToSearchKey), NSNumber.FromInt16(250)
                },
                {
                    new NSString(Keys.ShortcodesToSearchKey), NSNumber.FromInt16(250)
                },
                {
                    new NSString(Keys.PartialWordSearchKey), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.CleanCacheIntervalDaysKey), NSNumber.FromInt16(7)
                },
                {
                    new NSString(Keys.ClearCacheKey), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.EnableReportingKey), NSNumber.FromBoolean(true)
                }
            };
            ud.RegisterDefaults(defaultsDictionary);
        }

        public IDictionary<string, object> All
        {
            get
            {
                var dict = new Dictionary<string, object>();
                var nsdict = ud.ToDictionary();
                foreach (var kv in nsdict)
                    dict.Add(kv.Key.ToString(), kv.Value.ToString());

                return dict;
            }
        }

        public bool ShowCreatorOutgoing => ud.BoolForKey(Keys.ShowCreatorOutgoing);


        public bool UseServerTimezone => ud.BoolForKey(Keys.UseServerTimezoneKey);

        public int DocumentsToDownload => (int)ud.IntForKey(Keys.DocumentsToDownloadKey);

        public int MarkAsReadDelaySeconds => (int)ud.IntForKey(Keys.MarkAsReadDelaySecondsKey);

        public bool UnreadIndicatorMe => ud.BoolForKey(Keys.UnreadIndicatorMeKey);

        public bool CompactDocumentsList => ud.BoolForKey(Keys.CompactDocumentsListKey);

        public bool LargeAttachmentWarning => ud.BoolForKey(Keys.LargeAttachmentWarningKey);

        public DocumentBodyTypeRequest DocumentBodyRequestType => ud.BoolForKey(Keys.DocumentBodyRequestTypeKey) ? DocumentBodyTypeRequest.PlainTextOnly : DocumentBodyTypeRequest.HtmlOnly;

        public bool HideReadNotifications => ud.BoolForKey(Keys.HideReadNotificationsKey);

        public bool ComposePriorityEnabled => ud.BoolForKey(Keys.ComposePriorityEnabledKey);

        public bool RemoveLine => ud.BoolForKey(Keys.RemoveLineKey);

        public bool AlwatsUseDefaultLine => ud.BoolForKey(Keys.AlwaysUseDefaultLine);

        public TemplateUsageMode UseTemplate => (TemplateUsageMode)(int)ud.IntForKey(Keys.UseTemplateKey);

        public string LocalTemplate
        {
            get => ud.StringForKey(Keys.LocalTemplateKey);
            set
            {
                ud.SetString(value, Keys.LocalTemplateKey);
                ud.Synchronize();
            }
        }

        public int DocumentsToSearch => (int)ud.IntForKey(Keys.DocumentsToSearchKey);

        public int ContactsToSearch => (int)ud.IntForKey(Keys.ContactsToSearchKey);

        public int ShortcodesToSearch => (int)ud.IntForKey(Keys.ShortcodesToSearchKey);

        public bool PartialWordSearch => ud.BoolForKey(Keys.PartialWordSearchKey);

        public int CleanCacheIntervalDays => (int)ud.IntForKey(Keys.CleanCacheIntervalDaysKey);

        public bool ClearCache
        {
            get => ud.BoolForKey(Keys.ClearCacheKey);
            set
            {
                ud.SetBool(value, Keys.ClearCacheKey);
                ud.Synchronize();
            }
        }

        public bool EnableReporting => ud.BoolForKey(Keys.EnableReportingKey);

        public string PushNotificationToken
        {
            get => ud.StringForKey(Keys.PushNotificationTokenKey);
            set
            {
                ud.SetString(value, Keys.PushNotificationTokenKey);
                ud.Synchronize();
            }
        }

        public bool ResetOnLaunch
        {
            get => ud.BoolForKey(Keys.ResetOnLaunchKey);
            set
            {
                ud.SetBool(value, Keys.ResetOnLaunchKey);
                ud.Synchronize();
            }
        }
    }
}