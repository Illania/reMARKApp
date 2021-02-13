using Mark5.Mobile.Common.Model;
using System.Collections.Generic;
using Foundation;
using System.Linq;

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
            public const string SendingDelay = "SendingDelay";
            public const string RememberLastUserDelaySettings = "RememberLastUserDelaySettings";
            public const string LastUserSendingDelay = "LastUserSendingDelay";
            public const string ShowTimeForOldEmails = "ShowTimeForOldEmails";
            public const string SortByDate = "SortByDate";
            public const string ConfirmRemoveSwipe = "ConfirmRemoveSwipe";
            public const string HideReadNotificationsKey = "HideReadNotifications";

            public const string ComposePriorityEnabledKey = "ComposePriorityEnabled";
            public const string RemoveLineKey = "RemoveLine";
            public const string UseTemplateKey = "UseTemplate";
            public const string LocalTemplateKey = "LocalTemplate";
            public const string AlwaysUseDefaultLineKey = "AlwaysUseDefaultLine";

            public const string DocumentsToSearchKey = "DocumentsToSearch";
            public const string ContactsToSearchKey = "ContactsToSearch";
            public const string ShortcodesToSearchKey = "ShortcodesToSearch";
            public const string PartialWordSearchKey = "PartialWordSearch";

            public const string CleanCacheIntervalDaysKey = "CleanCacheIntervalDays";
            public const string ClearCacheKey = "ClearCache";
            public const string EnableReportingKey = "EnableReporting";

            public const string AuthorizationInterval = "AuthorizationInterval";

            public const string PushNotificationTokenKey = "PushNotificationToken";

            public const string AzureHubRegistrationId = "AzureHubRegistrationId";

            public const string ResetOnLaunchKey = "ResetOnLaunch";

            public const string EmailTrailingSwipeActions = "EmailTrailingSwipeActions";
            public const string EmailLeadingSwipeActions = "EmailLeadingSwipeActions";

            public const string CallerIdentificationEnabledKey = "CallerIdentificationEnabled";

            public const string SyncFavoriteFoldersKey = "SyncFavoriteFolders";
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
                    new NSString(Keys.SendingDelay), NSNumber.FromInt16(0)
                },
                {
                    new NSString(Keys.RememberLastUserDelaySettings), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.LastUserSendingDelay), NSNumber.FromInt16(0)
                },
                {
                    new NSString(Keys.ShowTimeForOldEmails), NSNumber.FromBoolean(false)
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
                    new NSString(Keys.AlwaysUseDefaultLineKey), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.ConfirmRemoveSwipe), NSNumber.FromBoolean(true)
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
                },
                {
                    new NSString(Keys.CallerIdentificationEnabledKey), NSNumber.FromBoolean(false)
                },
                {
                    new NSString(Keys.AuthorizationInterval), NSNumber.FromInt16(-1)
                },
                {
                    new NSString(Keys.EmailLeadingSwipeActions), NSArray.FromStrings(EmailSwipeAction.SwipeAction.Categories.ToString())
                },
                {
                    new NSString(Keys.EmailTrailingSwipeActions), NSArray.FromStrings (EmailSwipeAction.SwipeAction.More.ToString(), EmailSwipeAction.SwipeAction.CopyToWorkTray.ToString(), EmailSwipeAction.SwipeAction.MarkAsRead.ToString())
                },
                {
                    new NSString(Keys.SyncFavoriteFoldersKey), NSNumber.FromBoolean(false)
                },

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

        public bool ShowTimeForOldEmails => ud.BoolForKey(Keys.ShowTimeForOldEmails);

        public bool SortByDate => ud.BoolForKey(Keys.SortByDate);

        public bool UnreadIndicatorMe => ud.BoolForKey(Keys.UnreadIndicatorMeKey);

        public bool ConfirmRemoveSwipe => ud.BoolForKey(Keys.ConfirmRemoveSwipe);

        public bool CompactDocumentsList => ud.BoolForKey(Keys.CompactDocumentsListKey);

        public bool LargeAttachmentWarning => ud.BoolForKey(Keys.LargeAttachmentWarningKey);

        public int SendingDelay => (int)ud.IntForKey(Keys.SendingDelay);

        public bool RememberLastUserDelaySettings => ud.BoolForKey(Keys.RememberLastUserDelaySettings);

        public int LastUserSendingDelay
        {
            get => (int)ud.IntForKey(Keys.LastUserSendingDelay);
            set
            {
                ud.SetInt(value, Keys.LastUserSendingDelay);
                ud.Synchronize();
            }
        }

        public DocumentBodyTypeRequest DocumentBodyRequestType => ud.BoolForKey(Keys.DocumentBodyRequestTypeKey) ? DocumentBodyTypeRequest.PlainTextOnly : DocumentBodyTypeRequest.HtmlOnly;

        public bool HideReadNotifications => ud.BoolForKey(Keys.HideReadNotificationsKey);

        public bool ComposePriorityEnabled => ud.BoolForKey(Keys.ComposePriorityEnabledKey);

        public bool RemoveLine => ud.BoolForKey(Keys.RemoveLineKey);

        public bool AlwaysUseDefaultLine => ud.BoolForKey(Keys.AlwaysUseDefaultLineKey);

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

        public int AuthorizationInterval => (int)ud.IntForKey(Keys.AuthorizationInterval);

        public string PushNotificationToken
        {
            get => ud.StringForKey(Keys.PushNotificationTokenKey);
            set
            {
                ud.SetString(value, Keys.PushNotificationTokenKey);
                ud.Synchronize();
            }
        }

        public string AzureHubRegistrationId
        {
            get => ud.StringForKey(Keys.AzureHubRegistrationId);
            set
            {
                ud.SetString(value, Keys.AzureHubRegistrationId);
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

        public bool CallerIdentificationEnabled
        {
            get => ud.BoolForKey(Keys.CallerIdentificationEnabledKey);
            set
            {
                ud.SetBool(value, Keys.CallerIdentificationEnabledKey);
                ud.Synchronize();
            }
        }

        #region EmailSwipeActions

        public void SetEmailLeadingSwipeAction(EmailSwipeAction.SwipeAction action)
        {
            EmailLeadingSwipeActions = new List<EmailSwipeAction> { new EmailSwipeAction(action) };
        }

        public void SetEmailTrailingLastAction(EmailSwipeAction.SwipeAction action)
        {
            List<EmailSwipeAction> newActions = new List<EmailSwipeAction>(EmailTrailingSwipeActions);
            newActions.Last().Action = action;
            EmailTrailingSwipeActions = newActions;
        }

        public void SetEmailTrailingMiddleAction(EmailSwipeAction.SwipeAction action)
        {
            List<EmailSwipeAction> newActions = new List<EmailSwipeAction>(EmailTrailingSwipeActions);
            newActions[1].Action = action;
            EmailTrailingSwipeActions = newActions;
        }

        public List<EmailSwipeAction> EmailLeadingSwipeActions
        {
            get
            {
                var udActions = ud.ArrayForKey(Keys.EmailLeadingSwipeActions).ToList();
                return udActions.Select(x => new EmailSwipeAction(x.ToString())).ToList();
            }

            set
            {
                var newVal = value.Select(item => item.Action.ToString()).ToArray();
                ud.SetValueForKey(NSArray.FromStrings(newVal), new NSString(Keys.EmailLeadingSwipeActions));
                ud.Synchronize();
            }
        }

        public List<EmailSwipeAction> EmailTrailingSwipeActions
        {
            get
            {
                var udActions = ud.ArrayForKey(Keys.EmailTrailingSwipeActions).ToList();
                var actions = udActions.Select(x => new EmailSwipeAction(x.ToString())).ToList();
                return actions;
            }

            set
            {
                var newVal = value.Select(x => x.Action.ToString()).ToArray();
                ud.SetValueForKey(NSArray.FromStrings(newVal), new NSString(Keys.EmailTrailingSwipeActions));
                ud.Synchronize();
            }
        }

        public List<EmailSwipeAction> GetAvailableSwipeActions()
        {
            var exceptLeading = EmailSwipeAction.GetAllAvailableActions.Where(all => !EmailLeadingSwipeActions.Any(leading => leading.Action == all.Action));
            var exceptTrailing = exceptLeading.Where(leading => !EmailTrailingSwipeActions.Any(trailing => trailing.Action == leading.Action));
            return exceptTrailing.ToList();
        }

        public void ResetSwipeActions()
        {
            EmailLeadingSwipeActions = new List<EmailSwipeAction> { new EmailSwipeAction(EmailSwipeAction.SwipeAction.Categories) };
            EmailTrailingSwipeActions = new List<EmailSwipeAction> { new EmailSwipeAction(EmailSwipeAction.SwipeAction.More), new EmailSwipeAction(EmailSwipeAction.SwipeAction.CopyToWorkTray), new EmailSwipeAction(EmailSwipeAction.SwipeAction.MarkAsRead) };
        }

        #region Favorites sync
        public bool SyncFavoriteFoldersEnabled
        {
            get => ud.BoolForKey(Keys.SyncFavoriteFoldersKey);
            set
            {
                ud.SetBool(value, Keys.SyncFavoriteFoldersKey);
                ud.Synchronize();
            }
        }

        #endregion

        #endregion

    }
}