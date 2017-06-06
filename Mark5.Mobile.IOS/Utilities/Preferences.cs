//
// Project: Mark5.Mobile.IOS
// File: Preferences.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using Mark5.Mobile.Common.Model;
using System.Collections.Generic;
using Foundation;
using System;

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

            public const string SynchroniseContactsKey = "SynchroniseContacts";

            public const string SynchroniseShortcodesKey = "SynchroniseShortcodes";

            public const string ComposePriorityEnabledKey = "ComposePriorityEnabled";
            public const string RemoveLineKey = "RemoveLine";
            public const string UseTemplateKey = "UseTemplate";
            public const string LocalTemplateKey = "LocalTemplate";

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
                    new NSString(Keys.SynchroniseContactsKey), NSNumber.FromBoolean(true)
                },
                {
                    new NSString(Keys.SynchroniseShortcodesKey), NSNumber.FromBoolean(false)
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
                {
                    dict.Add(kv.Key.ToString(), kv.Value.ToString());
                }
                return dict;
            }
        }

        public bool ShowCreatorOutgoing
        {
            get { return ud.BoolForKey(Keys.ShowCreatorOutgoing); }
        }


        public bool UseServerTimezone
        {
            get { return ud.BoolForKey(Keys.UseServerTimezoneKey); }
        }

        public int DocumentsToDownload
        {
            get { return (int) ud.IntForKey(Keys.DocumentsToDownloadKey); }
        }

        public int MarkAsReadDelaySeconds
        {
            get { return (int) ud.IntForKey(Keys.MarkAsReadDelaySecondsKey); }
        }

        public bool UnreadIndicatorMe
        {
            get { return ud.BoolForKey(Keys.UnreadIndicatorMeKey); }
        }

        public bool CompactDocumentsList
        {
            get { return ud.BoolForKey(Keys.CompactDocumentsListKey); }
        }

        public bool LargeAttachmentWarning
        {
            get { return ud.BoolForKey(Keys.LargeAttachmentWarningKey); }
        }

        public DocumentBodyTypeRequest DocumentBodyRequestType
        {
            get { return ud.BoolForKey(Keys.DocumentBodyRequestTypeKey) ? DocumentBodyTypeRequest.PlainTextOnly : DocumentBodyTypeRequest.HtmlOnly; }
        }

        public bool HideReadNotifications
        {
            get { return ud.BoolForKey(Keys.HideReadNotificationsKey); }
        }

        public bool SynchroniseContacts
        {
            get { return ud.BoolForKey(Keys.SynchroniseContactsKey); }
        }

        public bool SynchroniseShortcodes
        {
            get { return ud.BoolForKey(Keys.SynchroniseShortcodesKey); }
        }

        public bool ComposePriorityEnabled
        {
            get { return ud.BoolForKey(Keys.ComposePriorityEnabledKey); }
        }

        public bool RemoveLine
        {
            get { return ud.BoolForKey(Keys.RemoveLineKey); }
        }

        public TemplateUsageMode UseTemplate
        {
            get { return (TemplateUsageMode) (int) ud.IntForKey(Keys.UseTemplateKey); }
        }

        public string LocalTemplate
        {
            get { return ud.StringForKey(Keys.LocalTemplateKey); }
            set
            {
                ud.SetString(value, Keys.LocalTemplateKey);
                ud.Synchronize();
            }
        }

        public int DocumentsToSearch
        {
            get { return (int) ud.IntForKey(Keys.DocumentsToSearchKey); }
        }

        public int ContactsToSearch
        {
            get { return (int) ud.IntForKey(Keys.ContactsToSearchKey); }
        }

        public int ShortcodesToSearch
        {
            get { return (int) ud.IntForKey(Keys.ShortcodesToSearchKey); }
        }

        public bool PartialWordSearch
        {
            get { return ud.BoolForKey(Keys.PartialWordSearchKey); }
        }

        public int CleanCacheIntervalDays
        {
            get { return (int) ud.IntForKey(Keys.CleanCacheIntervalDaysKey); }
        }

        public bool ClearCache
        {
            get { return ud.BoolForKey(Keys.ClearCacheKey); }
            set
            {
                ud.SetBool(value, Keys.ClearCacheKey);
                ud.Synchronize();
            }
        }

        public bool EnableReporting
        {
            get { return ud.BoolForKey(Keys.EnableReportingKey); }
        }

        public string PushNotificationToken
        {
            get { return ud.StringForKey(Keys.PushNotificationTokenKey); }
            set
            {
                ud.SetString(value, Keys.PushNotificationTokenKey);
                ud.Synchronize();
            }
        }

        public bool ResetOnLaunch
        {
            get { return ud.BoolForKey(Keys.ResetOnLaunchKey); }
            set
            {
                ud.SetBool(value, Keys.ResetOnLaunchKey);
                ud.Synchronize();
            }
        }
    }
}