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
            public const string MainTabsOrderKey = "MainTabsOrder";

            public const string DocumentsToDownloadKey = "DocumentsToDownload";
            public const string UnreadIndicatorMeKey = "UnreadIndicatorMe";
            public const string CompactDocumentsListKey = "CompactDocumentsList";
            public const string MarkAsReadDelaySecondsKey = "MarkAsReadDelaySeconds";
            public const string LargeAttachmentWarningKey = "LargeAttachmentWarning";
            public const string DocumentBodyRequestTypeKey = "DocumentBodyRequestType";

            public const string ContactCommunicationFaxNumbersEnabledKey = "ContactCommunicationFaxNumbersEnabled";
            public const string ContactCommunicationTelexNumbersEnabledKey = "ContactCommunicationTelexNumbersEnabled";
            public const string ContactCommunicationImEnabledKey = "ContactCommunicationImEnabled";
            public const string ContactCommunicationInternalEnabledKey = "ContactCommunicationInternalEnabled";
            public const string ContactCommunicationOtherEnabledKey = "ContactCommunicationOtherEnabled";
            public const string ContactAddressesEnabledKey = "ContactAddressesEnabled";
            public const string ContactBirthdateEnabledKey = "ContactBirthdateEnabled";
            public const string ContactAccountEnabledKey = "ContactAccountEnabled";
            public const string ContactVatEnabledKey = "ContactVatEnabled";
            public const string SynchroniseContactsKey = "SynchroniseContacts";

            public const string SynchroniseShortcodesKey = "SynchroniseShortcodes";

            public const string ComposePriorityEnabledKey = "ComposePriorityEnabled";
            public const string RemoveLineKey = "RemoveLine";
            public const string UseTemplateKey = "UseTemplate";
            public const string LocalTemplateKey = "LocalTemplate";

            public const string NotificationsRingtoneKey = "NotificationsRingtone";
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
            var defaultsDictionary = new NSMutableDictionary();

            defaultsDictionary.Add(new NSString(Keys.DocumentsToDownloadKey), NSNumber.FromInt16(250));
            defaultsDictionary.Add(new NSString(Keys.UnreadIndicatorMeKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.MarkAsReadDelaySecondsKey), NSNumber.FromInt16(2));
            defaultsDictionary.Add(new NSString(Keys.CompactDocumentsListKey), NSNumber.FromBoolean(false));
            defaultsDictionary.Add(new NSString(Keys.LargeAttachmentWarningKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.DocumentBodyRequestTypeKey), NSNumber.FromBoolean(false));

            defaultsDictionary.Add(new NSString(Keys.ContactCommunicationFaxNumbersEnabledKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.ContactCommunicationTelexNumbersEnabledKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.ContactCommunicationImEnabledKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.ContactCommunicationInternalEnabledKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.ContactCommunicationOtherEnabledKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.ContactAddressesEnabledKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.ContactBirthdateEnabledKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.ContactAccountEnabledKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.ContactVatEnabledKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.SynchroniseContactsKey), NSNumber.FromBoolean(true));

            defaultsDictionary.Add(new NSString(Keys.SynchroniseShortcodesKey), NSNumber.FromBoolean(false));

            defaultsDictionary.Add(new NSString(Keys.ComposePriorityEnabledKey), NSNumber.FromBoolean(false));
            defaultsDictionary.Add(new NSString(Keys.RemoveLineKey), NSNumber.FromBoolean(true));
            defaultsDictionary.Add(new NSString(Keys.UseTemplateKey), NSNumber.FromInt16(1));

            defaultsDictionary.Add(new NSString(Keys.CleanCacheIntervalDaysKey), NSNumber.FromInt16(7));
            defaultsDictionary.Add(new NSString(Keys.ClearCacheKey), NSNumber.FromBoolean(false));
            defaultsDictionary.Add(new NSString(Keys.EnableReportingKey), NSNumber.FromBoolean(true));

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

        public string[] MainTabsOrder
        {
            get
            {
                var order = ud.StringForKey(Keys.MainTabsOrderKey);
                if (string.IsNullOrWhiteSpace(order))
                {
                    return null;
                }

                return order.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                ud.SetString(string.Join(",", value), Keys.MainTabsOrderKey);
                ud.Synchronize();
            }
        }

        #region Documents

        public int DocumentsToDownload
        {
            get
            {
                return (int)ud.IntForKey(Keys.DocumentsToDownloadKey);
            }
        }

        public int MarkAsReadDelaySeconds
        {
            get
            {
                return (int)ud.IntForKey(Keys.MarkAsReadDelaySecondsKey);
            }
        }

        public bool UnreadIndicatorMe
        {
            get
            {
                return ud.BoolForKey(Keys.UnreadIndicatorMeKey);
            }
        }

        public bool CompactDocumentsList
        {
            get
            {
                return ud.BoolForKey(Keys.CompactDocumentsListKey);
            }
        }

        public bool LargeAttachmentWarning
        {
            get
            {
                return ud.BoolForKey(Keys.LargeAttachmentWarningKey);
            }
        }

        public DocumentBodyTypeRequest DocumentBodyRequestType
        {
            get
            {
                return ud.BoolForKey(Keys.DocumentBodyRequestTypeKey) ? DocumentBodyTypeRequest.PlainTextOnly : DocumentBodyTypeRequest.HtmlOnly;
            }
        }

        #endregion

        #region Contacts

        public bool ContactCommunicationFaxNumbersEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ContactCommunicationFaxNumbersEnabledKey);
            }
        }

        public bool ContactCommunicationTelexNumbersEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ContactCommunicationTelexNumbersEnabledKey);
            }
        }

        public bool ContactCommunicationImEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ContactCommunicationImEnabledKey);
            }
        }

        public bool ContactCommunicationInternalEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ContactCommunicationInternalEnabledKey);
            }
        }

        public bool ContactCommunicationOtherEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ContactCommunicationOtherEnabledKey);
            }
        }

        public bool ContactAddressesEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ContactAddressesEnabledKey);
            }
        }

        public bool ContactBirthdateEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ContactBirthdateEnabledKey);
            }
        }

        public bool ContactAccountEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ContactAccountEnabledKey);
            }
        }

        public bool ContactVatEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ContactVatEnabledKey);
            }
        }


        public bool SynchroniseContacts
        {
            get
            {
                return ud.BoolForKey(Keys.SynchroniseContactsKey);
            }
        }

        #endregion

        #region Shortcodes

        public bool SynchroniseShortcodes
        {
            get
            {
                return ud.BoolForKey(Keys.SynchroniseShortcodesKey);
            }
        }

        #endregion

        #region Composing Documents

        public bool ComposePriorityEnabled
        {
            get
            {
                return ud.BoolForKey(Keys.ComposePriorityEnabledKey);
            }
        }

        public bool RemoveLine
        {
            get
            {
                return ud.BoolForKey(Keys.RemoveLineKey);
            }
        }

        public TemplateUsageMode UseTemplate
        {
            get
            {
                return (TemplateUsageMode)(int)ud.IntForKey(Keys.UseTemplateKey);
            }
        }

        public string LocalTemplate
        {
            get
            {
                return ud.StringForKey(Keys.LocalTemplateKey);
            }
            set
            {
                ud.SetString(value, Keys.LocalTemplateKey);
                ud.Synchronize();
            }
        }

        #endregion

        public string NotificationsRingtone
        {
            get
            {
                return ud.StringForKey(Keys.NotificationsRingtoneKey);
            }
            set
            {
                ud.SetString(value, Keys.NotificationsRingtoneKey);
                ud.Synchronize();
            }
        }

        public int CleanCacheIntervalDays
        {
            get
            {
                return (int)ud.IntForKey(Keys.CleanCacheIntervalDaysKey);
            }
        }

        public bool ClearCache
        {
            get
            {
                return ud.BoolForKey(Keys.ClearCacheKey);
            }
            set
            {
                ud.SetBool(value, Keys.ClearCacheKey);
                ud.Synchronize();
            }
        }

        public bool EnableReporting
        {
            get
            {
                return ud.BoolForKey(Keys.EnableReportingKey);
            }
        }

        public string PushNotificationToken
        {
            get
            {
                return ud.StringForKey(Keys.PushNotificationTokenKey);
            }
            set
            {
                ud.SetString(value, Keys.PushNotificationTokenKey);
                ud.Synchronize();
            }
        }

        public bool ResetOnLaunch
        {
            get
            {
                return ud.BoolForKey(Keys.ResetOnLaunchKey);
            }
            set
            {
                ud.SetBool(value, Keys.ResetOnLaunchKey);
                ud.Synchronize();
            }
        }
    }
}

