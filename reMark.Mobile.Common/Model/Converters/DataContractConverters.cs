using System;
using System.Linq;
using reMark.Mobile.Common.Extensions;
using DataContract = reMark.ServiceReference.DataContract;
using System.Collections.Generic;
using reMark.Mobile.Common.Utilities;

namespace reMark.Mobile.Common.Model.Converters
{
    public static class DataContractConverters
    {
        #region Enums

        public static T ConvertEnum<T>(this Enum obj) where T : struct
        {
            return (T)Enum.Parse(typeof(T), obj.ToString());
        }

        #endregion

        #region DataContract to Model

        public static AttachmentDescription Convert(this DataContract.AttachmentDescription ad)
        {
            return new AttachmentDescription
            {
                Id = ad.Id,
                Name = ad.Name,
                SizeInBytes = ad.SizeInBytes,
                FromTemplate = ad.FromTemplate,
            };
        }

        public static Category Convert(this DataContract.Category c)
        {
            return new Category
            {
                Id = c.Id,
                Guid = c.Guid,
                Name = c.Name,
                Description = c.Description,
                HexColor = c.HexColor
            };
        }

        public static ExtraField Convert(this DataContract.ExtraFieldInfo extraFieldInfo)
        {
            return new ExtraField
            {
                FieldId = extraFieldInfo.Id,
                FieldName = extraFieldInfo.Name,
                Enabled = extraFieldInfo.Enabled
            };
        }
 
        public static Comment Convert(this DataContract.Comment c)
        {
            return new Comment
            {
                Id = c.Id,
                Guid = c.Guid,
                Content = c.Content,
                DateAddedTimestamp = c.DateAdded.ConvertDateTimeToTimestampMilliseconds(),
                ParentId = c.ParentId,
                ParentTypeId = c.ParentTypeId,
                UserId = c.UserId,
                UserName = c.UserName
            };
        }

        public static CommunicationAddress Convert(this DataContract.CommunicationAddress ca)
        {
            return new CommunicationAddress
            {
                Address = ca.Address,
                Description = ca.Description,
                IsPrimary = ca.IsPrimary,
                Type = ca.Type.ConvertEnum<CommunicationAddressType>()
            };
        }

        public static Contact Convert(this DataContract.Contact c)
        {
            var result = new Contact
            {
                Id = c.Id,
                Guid = c.Guid,
                FirstName = c.FirstName,
                Patronymic = c.Patronymic,
                LastName = c.LastName,
                Account = c.Account,
                BirthDateTimestamp = c.BirthDate.Year < 1850 ? -1 : c.BirthDate.ConvertDateTimeToTimestampMilliseconds(),
                Ledger = c.Ledger,
                Vat = c.Vat,
                WebPageAddress = c.WebPageAddress,
                Position = c.Position,
                PrimaryPerson = c.PrimaryPerson?.Convert(),
                PreferrableType = c.PreferrableType.ConvertEnum<CommunicationAddressType>()
            };
            if (c.CommunicationAddresses != null)
                result.CommunicationAddresses.AddRange(c.CommunicationAddresses.WhereNotNull().Select(Convert));
            if (c.Children != null)
                result.Children.AddRange(c.Children.WhereNotNull().Select(Convert));
            if (c.PhysicalAddresses != null)
                result.PhysicalAddresses.AddRange(c.PhysicalAddresses.WhereNotNull().Select(Convert));
            if (c.ResponsibleUserIds != null)
                result.ResponsibleUserIds.AddRange(c.ResponsibleUserIds);
            if (c.ResponsibleUsers != null)
                result.ResponsibleUsers = new Dictionary<int, string>(c.ResponsibleUsers);
            if (c.Comments != null)
                result.Comments.AddRange(c.Comments.WhereNotNull().Select(Convert));
            return result;
        }

        public static ContactPreview Convert(this DataContract.ContactPreview cp)
        {
            var result = new ContactPreview
            {
                Id = cp.Id,
                Guid = cp.Guid,
                RowId = cp.RowId,
                Name = cp.Name,
                Description = cp.Description,
                CompanyName = cp.CompanyName,
                ShortId = cp.ShortId,
                Type = cp.Type.ConvertEnum<ContactType>(),
                PrimaryAddress = cp.PrimaryAddress?.Convert(),
                CommentsCount = cp.CommentsCount
            };
            if (cp.Categories != null)
                result.Categories.AddRange(cp.Categories.WhereNotNull().Select(Convert));
            return result;
        }

        public static ContactsModuleInfo Convert(this DataContract.ContactsModuleInfo cmi)
        {
            var result = new ContactsModuleInfo
            {
                Permissions = cmi.Permissions?.Convert(),
                WorktrayEnabled = cmi.WorktrayEnabled,
            };
            if (cmi.Countries != null)
                result.Countries.AddRange(cmi.Countries.WhereNotNull().Select(Convert));
            if (cmi.PhysicalAddressTypes != null)
                result.PhysicalAddressTypes.AddRange(cmi.PhysicalAddressTypes.WhereNotNull().Select(Convert));
            return result;
        }

        public static CountryInfo Convert(this DataContract.CountryInfo ci)
        {
            return new CountryInfo
            {
                CCode = ci.CCode,
                CCode3 = ci.CCode3,
                FaxPrefix = ci.FaxPrefix,
                Id = ci.Id,
                Name = ci.Name,
                TelexPrefix = ci.TelexPrefix
            };
        }

        public static Document Convert(this DataContract.Document d)
        {
            var result = new Document
            {
                Id = d.Id,
                Guid = d.Guid,
                HtmlBody = d.HtmlBody,
                PlainTextBody = d.PlainTextBody,
                IsEncrypted = d.IsEncrypted,
                WorktrayComment = d.WorktrayComment
            };
            if (d.Lines != null)
                result.Lines.AddRange(d.Lines.WhereNotNull().Select(Convert));
            if (d.ReadByUserIds != null)
                result.ReadByUserIds.AddRange(d.ReadByUserIds.Where(id=> id > 0));
            if (d.ReadByUserNames != null)
                result.ReadByUserNames = d.ReadByUserNames.Where(kv => !string.IsNullOrWhiteSpace(kv.Value) && kv.Key > 0).ToDictionary(kv => kv.Key, kv => kv.Value);
            if (d.Attachments != null)
                result.Attachments.AddRange(d.Attachments.WhereNotNull().Select(Convert));
            if (d.Comments != null)
                result.Comments.AddRange(d.Comments.WhereNotNull().Select(Convert));
            if (d.ExtraFields != null)
                result.ExtraFields = d.ExtraFields.Where(kv => kv.Key != null).ToDictionary(kv => kv.Key.Convert(), kv => kv.Value);
            if (d.Invitations != null)
                result.Invitations.AddRange(d.Invitations.WhereNotNull().Select(Convert));
            return result;
        }

        public static DocumentAddress Convert(this DataContract.DocumentAddress da)
        {
            return new DocumentAddress
            {
                Id = da.Id,
                Name = da.Name,
                Type = da.Type.ConvertEnum<CommunicationAddressType>(),
                AddressType = da.AddressType.ConvertEnum<DocumentAddressType>(),
                Address = da.Address,
                FullAddress = da.FullAddress,
                Attention = da.Attention,
                FullAttention = da.FullAttention,
                ObjectId = da.ObjectId,
                ObjectType = da.ObjectType.ConvertEnum<ObjectType>()
            };
        }

        public static DocumentExtraFieldInfo Convert(this DataContract.DocumentExtraFieldInfo defi)
        {
            return new DocumentExtraFieldInfo
            {
                Id = defi.Id,
                Name = defi.Name
            };
        }

        public static DocumentExtraFieldInfo ToDocumentExtraFieldInfo(this ExtraField extraField)
        {
            return new DocumentExtraFieldInfo
            {
                Id = extraField.FieldId,
                Name = extraField.FieldName
            };
        }

        public static DocumentsModuleInfo Convert(this DataContract.DocumentsModuleInfo dmi)
        {
            var result = new DocumentsModuleInfo
            {
                AttachmentSearchEnabled = dmi.AttachmentSearchEnabled,
                DefaultOutgoingLine = dmi.DefaultOutgoingLine?.Convert(),
                HandledFieldEnabled = dmi.HandledFieldEnabled,
                IsMissingAttachmentWarningEnabled = dmi.IsMissingAttachmentWarningEnabled,
                MaximumAttachmentSizeBytes = dmi.MaximumAttachmentSizeBytes,
                OnSendToSystemUser = dmi.OnSendToSystemUser.ConvertEnum<OnSendToSystemUser>(),
                Permissions = dmi.Permissions?.Convert(),
                WorktrayEnabled = dmi.WorktrayEnabled,
                UseForFrom = dmi.UseForFrom.ConvertEnum<UseForFrom>(),
                LineAppearances = dmi.LineAppearances?.Select(la => la.Convert()).ToList(),
                UserAppearances = dmi.UserAppearances?.Select(ua => ua.Convert()).ToList(),
                DefaultAppearance = dmi.DefaultAppearance?.Convert(),
                UserActivities = dmi.UserActivities?.Select(ua => ua.Convert()).ToList()

            };

            if (dmi.AttachmentKeywords != null)
                result.AttachmentKeywords.AddRange(dmi.AttachmentKeywords.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (dmi.ExtraFieldInfos != null)
                result.ExtraFieldInfos.AddRange(dmi.ExtraFieldInfos.WhereNotNull().Select(defi => defi.Convert()));
            if (dmi.ForwardAbbreviations != null)
                result.ForwardAbbreviations.AddRange(dmi.ForwardAbbreviations.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (dmi.OutgoingLines != null)
                result.OutgoingLines.AddRange(dmi.OutgoingLines.WhereNotNull().Select(Convert));
            if (dmi.ReplyAbbreviations != null)
                result.ReplyAbbreviations.AddRange(dmi.ReplyAbbreviations.Where(s => !string.IsNullOrWhiteSpace(s)));
            return result;
        }

        public static OriginatorAppearance Convert(this DataContract.OriginatorAppearance originatorAppearance)
        {
            return new OriginatorAppearance
            {
                Enable = originatorAppearance.Enable,
                OriginatorGid = originatorAppearance.OriginatorGid,
                OriginatorName = originatorAppearance.OriginatorName,
                BackgroundColor = originatorAppearance.BackgroundColor,
                FontColor = originatorAppearance.FontColor,
                UnreadFontColor = originatorAppearance.UnreadFontColor,
                FontColorEnable = originatorAppearance.FontColorEnable,
                UnreadFontColorEnable = originatorAppearance.UnreadFontColorEnable,
                OriginatorColumnOnly = originatorAppearance.OriginatorColumnOnly
            };
        }

        public static UserActivity Convert(this DataContract.UserActivity userActivity)
        {
            return new UserActivity
            {
                 Type = userActivity.Type.ConvertEnum<UserActivityType>(),
                 DescriptionEvent = userActivity.DescriptionEvent,
                 DescriptionAction = userActivity.DescriptionEvent,
                 ConfirmationRequired = userActivity.ConfirmationRequired,
                 PerformOnOriginalDocument = userActivity.PerformOnOriginalDocument,
                 AssignOriginalCategories = userActivity.AssignOriginalCategories,
                 AssignOriginalExtraFields = userActivity.AssignOriginalExtraFields,
                 Categories = userActivity.Categories.Select(c=>c.Convert()).ToList(),
                 ExtraFields = userActivity.ExtraFields
            };
        }

        public static DocumentsModulePermissions Convert(this DataContract.DocumentsModulePermissions dmp)
        {
            return new DocumentsModulePermissions
            {
                IncomingSupervisor = dmp.IncomingSupervisor,
                OutgoingSupervisor = dmp.OutgoingSupervisor,
                ManageFilterViewFoldersAllowed = dmp.ManageFilterViewFoldersAllowed,
                SpamManager = dmp.SpamManager,
                DeleteDocumentsAllowedLines = dmp.DeleteDocumentsAllowedLines,
                CabinetSupervisor = dmp.CabinetSupervisor,
                CreateAllowed = dmp.CreateAllowed,
                CreateFolderAllowed = dmp.CreateFolderAllowed,
                DeleteAllowed = dmp.DeleteAllowed,
                DeleteFolderAllowed = dmp.DeleteFolderAllowed,
                EditAccessRightsAllowed = dmp.EditAccessRightsAllowed,
                EditAllowed = dmp.EditAllowed,
                EditFolderAllowed = dmp.EditFolderAllowed,
                ManageCategories = dmp.ManageCategories,
                ManagePublicDynamicFolderAllowed = dmp.ManagePublicDynamicFolderAllowed,
                MaxPublicPersonalFoldersAllowed = dmp.MaxPublicPersonalFoldersAllowed,
                RemoveFromFolderAllowed = dmp.RemoveFromFolderAllowed
            };
        }

        public static DocumentPreview Convert(this DataContract.DocumentPreview dp)
        {
            var result = new DocumentPreview
            {
                Id = dp.Id,
                Guid = dp.Guid,
                ReferenceNumber = dp.ReferenceNumber,
                Subject = dp.Subject,
                Preview = dp.Preview,
                Direction = dp.Direction.ConvertEnum<DocumentDirection>(),
                Priority = dp.Priority.ConvertEnum<Priority>(),
                IsReadByAnyone = dp.IsReadByAnyone,
                IsReadByCurrent = dp.IsReadByCurrent,
                CommentsCount = dp.CommentsCount,
                AttachmentsCount = dp.AttachmentsCount,
                DateReceivedTimestamp = dp.DateReceived.ConvertDateTimeToTimestampMilliseconds(),
                CreatorId = dp.CreatorId,
                Creator = dp.Creator,
                TransmitStatus = dp.TransmitStatus.ConvertEnum<TransmitStatus>(),
                Lines = dp.Lines?.Select(l=>l.Convert()).ToList(),
                CreatorGuid = dp.CreatorGuid
            };
            if (dp.Addresses != null)
                result.Addresses.AddRange(dp.Addresses.WhereNotNull().Select(Convert));
            if (dp.Categories != null)
                result.Categories.AddRange(dp.Categories.WhereNotNull().Select(Convert));
            return result;
        }

        public static Transmit Convert(this DataContract.Transmit transmit)
        {
            var result = new Transmit
            {
                Destinations = transmit.Destinations.Select(d => d.Convert()).ToList(),
                DocGuid = transmit.DocGuid,
                Priority = transmit.Priority.ConvertEnum<Priority>(),
                Status = transmit.Status.ConvertEnum<TransmitStatus>(),
            };
            return result;
        }

        public static TransmitDestination Convert(this DataContract.TransmitDestination transmitDestination)
        {
            var result = new TransmitDestination
            {
                Address = transmitDestination.Address,
                LinkType = transmitDestination.LinkType.ConvertEnum<ComAddressLinkType>(),
                Status = transmitDestination.Status.Convert()
            };
            return result;
        }

        public static DestinationStatus Convert(this DataContract.DestinationStatus destinationStatus)
        {
            var result = new DestinationStatus
            {
                Attempts = destinationStatus.Attempts,
                LastConnectAttempt = destinationStatus.LastConnectAttempt,
                LastMessage = destinationStatus.LastMessage,
                StatusDetail = destinationStatus.StatusDetail.ConvertEnum<DestinationStatusDetail>(),
                TimeStart = destinationStatus.TimeStart,
                TimeEnd = destinationStatus.TimeEnd,
                WasSentByLine = destinationStatus.WasSentByLine,
                WasSentByLineName = destinationStatus.WasSentByLineName
            };
            return result;
        }

        public static Folder Convert(this DataContract.Folder f)
        {
            var result = new Folder
            {
                Guid = f.Guid,
                HasSubFolders = f.HasSubFolders,
                Id = f.Id,
                ParentFolderId = f.ParentFolderId,
                InternalType = f.InternalType.ConvertEnum<FolderInternalType>(),
                Module = f.Module.ConvertEnum<ModuleType>(),
                Name = f.Name,
                Subscribed = f.Subscribed,
                Position = f.Position,
                Type = f.Type.ConvertEnum<FolderType>(),
                Path = f.Path,
            };
            if (f.SubFolders != null)
                result.SubFolders.AddRange(f.SubFolders.WhereNotNull().Select(Convert));
            return result;
        }

        public static Line Convert(this DataContract.Line l)
        {
            return new Line
            {
                FromAddress = l.FromAddress,
                Guid = l.Guid,
                Name = l.Name
            };
        }

        public static Notification Convert(this DataContract.Notification n)
        {
            return new Notification
            {
                Guid = n.Guid,
                Title = n.Title,
                Message = n.Message,
                DateTimeTimestamp = n.DateTime.ConvertDateTimeToTimestampMilliseconds(),
                Type = n.Type.ConvertEnum<EventType>(),
                FolderId = n.FolderId,
                ObjectId = n.ObjectId,
                ObjectType = n.ObjectType.ConvertEnum<ObjectType>(),
                RemindOnTimestamp = n.RemindOn.ConvertDateTimeToTimestampMilliseconds(),
                IsSilent = n.IsSilent
            };
        }

        public static ObjectAction Convert(this DataContract.ObjectAction oa)
        {
            return new ObjectAction
            {
                Id = oa.Id,
                Description = oa.Description,
                ActionTimeTimestamp = oa.ActionTime.ConvertDateTimeToTimestampMilliseconds(),
                ActionType = oa.ActionType,
                ActionTypeId = oa.ActionTypeId,
                ActionTypeGid = oa.ActionTypeGid,
                UserId = oa.UserId,
                Username = oa.Username
            };
        }

        public static ObjectLink Convert(this DataContract.ObjectLink ol)
        {
            return new ObjectLink
            {
                FromObjectId = ol.FromObjectId,
                FromObjectType = ol.FromObjectType.ConvertEnum<ObjectType>(),
                ToObjectId = ol.ToObjectId,
                ToObjectType = ol.ToObjectType.ConvertEnum<ObjectType>(),
                Description = ol.Description,
                IsReverse = ol.IsReverse,
                TypeInfo = ol.TypeInfo?.Convert(),
                LinkTimeStamp = ol.LinkTime.ConvertDateTimeToTimestampMilliseconds(),
            };
        }

        public static ObjectLinkTypeInfo Convert(this DataContract.ObjectLinkTypeInfo olti)
        {
            return new ObjectLinkTypeInfo
            {
                Id = olti.Id,
                Guid = olti.Guid,
                DescriptionAction = olti.DescriptionAction,
                DescriptionActionReverse = olti.DescriptionActionReverse,
                DescriptionSimple = olti.DescriptionSimple,
                DescriptionComplex = olti.DescriptionComplex,
                DescriptionComplexReverse = olti.DescriptionComplexReverse,
                FromType = olti.FromType.ConvertEnum<ObjectType>(),
                ToType = olti.ToType.ConvertEnum<ObjectType>()
            };
        }

        public static Participant Convert(this DataContract.Participant p)
        {
            return new Participant
            {
                Id = p.Id,
                Presence = p.Presence.ConvertEnum<ParticipantPresenence>(),
                Type = p.Type.ConvertEnum<ParticipantType>(),
                Status = p.Status.ConvertEnum<ParticipantStatus>(),
                CN = p.CN,
                Email = p.Email,
            };
        }

        public static Permissions Convert(this DataContract.Permissions p)
        {
            return new Permissions
            {
                CabinetSupervisor = p.CabinetSupervisor,
                CreateAllowed = p.CreateAllowed,
                CreateFolderAllowed = p.CreateFolderAllowed,
                DeleteAllowed = p.DeleteAllowed,
                DeleteFolderAllowed = p.DeleteFolderAllowed,
                EditAccessRightsAllowed = p.EditAccessRightsAllowed,
                EditAllowed = p.EditAllowed,
                EditFolderAllowed = p.EditFolderAllowed,
                ManageCategories = p.ManageCategories,
                ManagePublicDynamicFolderAllowed = p.ManagePublicDynamicFolderAllowed,
                MaxPublicPersonalFoldersAllowed = p.MaxPublicPersonalFoldersAllowed,
                RemoveFromFolderAllowed = p.RemoveFromFolderAllowed
            };
        }

        public static PhysicalAddress Convert(this DataContract.PhysicalAddress pa)
        {
            return new PhysicalAddress
            {
                Area = pa.Area,
                City = pa.City,
                Country = pa.Country?.Convert(),
                Street = pa.Street,
                Type = pa.Type?.Convert(),
                ZipCode = pa.ZipCode
            };
        }

        public static PhysicalAddressType Convert(this DataContract.PhysicalAddressType pat)
        {
            return new PhysicalAddressType
            {
                Description = pat.Description,
                Id = pat.Id,
                Name = pat.Name
            };
        }

        public static RecentAddress Convert(this DataContract.RecentAddress ra)
        {
            return new RecentAddress
            {
                Name = ra.Name,
                AddressType = ra.AddressType.ConvertEnum<DocumentAddressType>(),
                Address = ra.Address
            };
        }

        public static RecurrenceInfo Convert(this DataContract.RecurrenceInfo ra)
        {
            return new RecurrenceInfo()
            {
                AllDay = ra.AllDay,
                DayNumber = ra.DayNumber,
                Duration = ra.Duration,
                EndTimestamp = ra.End.ConvertDateTimeToTimestampMilliseconds(),
                StartTimestamp = ra.Start.ConvertDateTimeToTimestampMilliseconds(),
                FirstDayOfWeek = ra.FirstDayOfWeek,
                Month = ra.Month,
                OccurrenceCount = ra.OccurrenceCount,
                Periodicity = ra.Periodicity,
                Range = ra.Range.ConvertEnum<RecurrenceRange>(),
                Type = ra.Type.ConvertEnum<RecurrenceType>(),
                WeekDays = ra.WeekDays.ConvertEnum<WeekDays>(),
                WeekOfMonth = ra.WeekOfMonth.ConvertEnum<WeekOfMonth>(),
            };
        }

        public static Shortcode Convert(this DataContract.Shortcode s)
        {
            var result = new Shortcode
            {
                Id = s.Id,
                Guid = s.Guid
            };
            if (s.Addresses != null)
                result.Addresses.AddRange(s.Addresses.WhereNotNull().Select(Convert));
            return result;
        }

        public static ShortcodePreview Convert(this DataContract.ShortcodePreview sp)
        {
            return new ShortcodePreview
            {
                Id = sp.Id,
                Guid = sp.Guid,
                RowId = sp.RowId,
                Name = sp.Name,
                Description = sp.Description,
                AddressCount = sp.AddressCount
            };
        }

        public static ShortcodesModuleInfo Convert(this DataContract.ShortcodesModuleInfo smi)
        {
            return new ShortcodesModuleInfo
            {
                Permissions = smi.Permissions?.Convert(),
                WorktrayEnabled = smi.WorktrayEnabled,
            };
        }

        public static SystemDepartment Convert(this DataContract.SystemDepartment sd)
        {
            var result = new SystemDepartment
            {
                Id = sd.Id,
                Guid = sd.Guid,
                Name = sd.Name
            };
            if (sd.UserIds != null)
                result.UserIds.AddRange(sd.UserIds);
            return result;
        }

        public static SystemInfo Convert(this DataContract.SystemInfo si)
        {
            var result = new SystemInfo
            {
                SystemVersion = new Version(si.SystemVersion),
                ServiceVersion = si.ServiceVersion,
                ServerUtcOffset = si.ServerUtcOffset,
                CustomerName = si.CustomerName,
                CustomerGuid = si.CustomerGuid,
                ServerTimeZoneInfoSerialized = si.ServerTimeZoneInfoSerialized,
                NotificationsInChina = si.NotificationsInChina

            };
            if (si.AvailableModules != null)
                result.AvailableModules.AddRange(si.AvailableModules.Select(mt => mt.ConvertEnum<ModuleType>()).Distinct());

            return result;
        }

        public static SystemUser Convert(this DataContract.SystemUser su)
        {
            return new SystemUser
            {
                Avatar = su.Avatar,
                FirstName = su.FirstName,
                Guid = su.Guid,
                Id = su.Id,
                LastName = su.LastName,
                PatronymicName = su.PatronymicName,
                Username = su.Username
            };
        }

        public static Template Convert(this DataContract.Template t)
        {
            var result = new Template
            {
                Id = t.Id,
                Guid = t.Guid,
                Subject = t.Subject,
                Content = t.Content,
                ContentType = t.ContentType.ConvertEnum<ContentType>(),
                LineGuid = t.LineGuid,
            };

            if (t.Attachments != null)
                result.Attachments.AddRange(t.Attachments.WhereNotNull().Select(Convert));

            return result;
        }

        public static TemplatePreview Convert(this DataContract.TemplatePreview tp)
        {
            return new TemplatePreview
            {
                Id = tp.Id,
                Guid = tp.Guid,
                Name = tp.Name,
                Private = tp.Private,
                CreationMode = tp.CreationMode.ConvertEnum<DocumentCreationModeFlag>()
            };
        }

        public static UserInfo Convert(this DataContract.UserInfo ui)
        {
            return new UserInfo
            {
                IsSystemAdministrator = ui.IsSystemAdministrator,
                User = ui.User?.Convert()
            };
        }

        private static ModuleFavoriteFoldersCollection Convert(List<DataContract.ModuleFavoriteFolders> moduleFavoriteFolders, DateTime updatedAt)
        {
            ModuleFavoriteFoldersCollection moduleFavorites = new ModuleFavoriteFoldersCollection
            {
                UpdatedAt = updatedAt
            };

            if (moduleFavoriteFolders != null)
            {
                moduleFavorites.ModuleFavoriteFolders = new List<ModuleFavoriteFolders>();

                foreach (var fav in moduleFavoriteFolders)
                {
                    var newFav = new ModuleFavoriteFolders { ModuleType = (ModuleType)fav.ModuleType };
                    newFav.Folders.AddRange(fav.Folders.Select(Convert));
                    moduleFavorites.ModuleFavoriteFolders.Add(newFav);
                }
            }

            return moduleFavorites;
        }

        public static ModuleFavoriteFoldersCollection Convert(this DataContract.GetFavoriteFoldersResult moduleFavoritesResult) =>
            Convert(moduleFavoritesResult.ModuleFavoriteFoldersList, moduleFavoritesResult.UpdatedAt);


        public static ModuleFavoriteFoldersCollection Convert(this DataContract.GetUserFavoriteFoldersResult moduleFavoritesResult) =>
            Convert(moduleFavoritesResult.ModuleFavoriteFoldersList, moduleFavoritesResult.UpdatedAt);
 

        #region ICalendar

        public static CalendarInvitation Convert(this DataContract.CalendarInvitation ci)
        {
            return new CalendarInvitation
            {
                Id = ci.Id,
                AppointmentId = ci.AppointmentId,
                CalendarId = ci.CalendarId,
                Description = ci.Description,
                Summary = ci.Summary,
                Location = ci.Location,
                StartDateTimestamp = ci.StartDate.ConvertDateTimeToTimestampMilliseconds(),
                EndDateTimestamp = ci.EndDate.ConvertDateTimeToTimestampMilliseconds(),
                SerializedTimeZoneInfo = ci.SerializedTimeZoneInfo,
                MethodType = ci.MethodType.ConvertEnum<MethodType>(),
                Attendees = ci.Attendees.WhereNotNull().Select(Convert).ToList(),
                Status = ci.Status.ConvertEnum<ParticipantStatus>(),
                RecurrenceInfo = ci.RecurrenceInfo?.Convert()
            };
        }

        public static Attendee Convert(this DataContract.Attendee at)
        {
            return new Attendee
            {
                Name = at.Name,
                IsOrganizer = at.IsOrganizer,
                Status = at.Status.ConvertEnum<ParticipantStatus>(),
                Type = at.Type.ConvertEnum<ParticipantType>(),
            };
        }

        public static SearchDocumentsCriteria Convert(this DataContract.SearchDocumentsParameters parameters)
        {
            try
            {
                var criteria = new SearchDocumentsCriteria()
                {
                    SavedSearchFilterHash = parameters.SavedSearchFilterHash,
                    MaxToFetch = parameters.MaxToFetch,
                    SubjectMessageField = parameters.SubjectMessageField,
                    SubjectMessageClause = parameters.SubjectMessageClause.ConvertEnum<SubjectMessageClause>(),
                    FromToField = parameters.FromToField,
                    SearchInAttachments = parameters.SearchInAttachments,
                    UnreadOnly = parameters.Unread,
                    PartialWordSearch = parameters.PartialWordSearch,
                    Handled = parameters.Processed,
                    Reference = parameters.Reference,
                    Priorities = parameters.Priorities?.Select(p => p.ConvertEnum<Priority>()).ToList() ?? new List<Priority>(),
                    Directions = parameters.Directions?.Select(p => p.ConvertEnum<DocumentDirection>()).ToList() ?? new List<DocumentDirection>(),
                    CategoryIds = parameters.CategoryIds.ToList(),
                    MustHaveCategoryIds = parameters.MustHaveCategoryIds.ToList(),
                    LineGuids = parameters.LineGuids.ToList(),
                    CreatorGuids = parameters.CreatorGuids.ToList(),
                    DateRange = new DateRange()
                    {
                        Enabled = parameters.DateRange?.Enabled ?? false,
                        StartTimestamp = parameters.DateRange?.Start.ToUniversalTime().ConvertDateTimeToTimestampMilliseconds() ?? DateTime.MinValue.ConvertDateTimeToTimestampMilliseconds(),
                        EndTimestamp = parameters.DateRange?.End.ToUniversalTime().ConvertDateTimeToTimestampMilliseconds() ?? DateTime.MaxValue.ConvertDateTimeToTimestampMilliseconds()
                    },
                    Comment = parameters.Comment,
                    AttachmentName = parameters.AttachmentName,
                    HavingAttachmentsOnly = parameters.HavingAttachmentsOnly,
                    FiledInFolderType = parameters.FiledInFolderType.ConvertEnum<FiledInFolderType>(),
                    FiledInFolderFolderType = parameters.FiledInFolderFolderType.ConvertEnum<FiledInFolderFolderType>(),
                    ExtraFields = parameters.ExtraFields
                };
                return criteria;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
          
        }

        public static SearchContactsCriteria Convert(this DataContract.SearchContactsParameters parameters)
        {
            var criteria = new SearchContactsCriteria()
            {
                SavedSearchFilterHash = parameters.SavedSearchFilterHash,
                MaxToFetch = parameters.MaxToFetch,
                Name = parameters.Name,
                FirstName = parameters.FirstName,
                LastName = parameters.LastName,
                ShortId = parameters.ShortId,
                Description = parameters.Description,
                ContactTypes = new HashSet<ContactType>(parameters.ContactTypes.Select(ct => ct.ConvertEnum<ContactType>())),
                ComAddress = parameters.ComAddress,
                PostAddress = parameters.PostAddress,
                Vat = parameters.Vat,
                Ledger = parameters.Ledger,
                CountryPrefix = parameters.CountryPrefix,
                CategoryIds = parameters.CategoriesIds.ToList(),
                MustHaveCategoryIds = parameters.MustHaveCategoriesIds.ToList(),
                Comment = parameters.Comment,
                FiledInFolderType = parameters.FiledInFolderType.ConvertEnum<FiledInFolderType>(),
                FiledInFolderFolderType = parameters.FiledInFolderFolderType.ConvertEnum<FiledInFolderFolderType>()
            };
            return criteria;
        }

        public static SearchShortcodesCriteria Convert(this DataContract.SearchShortcodesParameters parameters)
        {
            var criteria = new SearchShortcodesCriteria()
            {
                SavedSearchFilterHash = parameters.SavedSearchFilterHash,
                MaxToFetch = parameters.MaxToFetch,
                Name = parameters.Name,
                Description = parameters.Description,
                Address = parameters.Address,
                FiledInFolderType = parameters.FiledInFolderType.ConvertEnum<FiledInFolderType>(),
                FiledInFolderFolderType = parameters.FiledInFolderFolderType.ConvertEnum<FiledInFolderFolderType>()
            };
            return criteria;
        }

        public static SavedDocumentsSearch Convert(this DataContract.SavedDocumentsFilter filter)
        {
            var savedSearch = new SavedDocumentsSearch()
            {
                Id = filter.Id,
                Name = filter.Name,
                Criteria = filter.SearchParameters.Convert()
            };
            return savedSearch;
        }


        public static SavedContactsSearch Convert(this DataContract.SavedContactsFilter filter)
        {
            var savedSearch = new SavedContactsSearch()
            {
                Id = filter.Id,
                Name = filter.Name,
                Criteria = filter.SearchParameters.Convert()
            };
            return savedSearch;
        }

        public static SavedShortcodesSearch Convert(this DataContract.SavedShortcodesFilter filter)
        {
            var savedSearch = new SavedShortcodesSearch()
            {
                Id = filter.Id,
                Name = filter.Name,
                Criteria = filter.SearchParameters.Convert()
            };
            return savedSearch;
        }

        #endregion

        #endregion

        #region Model to DataContract

        public static DataContract.AzureUser Convert(this Classes.Azure.AzureUser au)
        {
            return new DataContract.AzureUser
            {
                Id = au.Id,
                DisplayName = au.DisplayName,
                UserPrincipalName = au.UserPrincipalName,
                Mail = au.Mail
            };
        }

        public static DataContract.Document Convert(this Document doc)
        {
            return new DataContract.Document
            {
                Id = doc.Id,
                Guid = doc.Guid,
                Lines = doc.Lines.Select(Convert).ToList(),
                HtmlBody = doc.HtmlBody,
                PlainTextBody = doc.PlainTextBody,
                ReadByUserIds = doc.ReadByUserIds,
                ReadByUserNames = doc.ReadByUserNames,
                Attachments = doc.Attachments.Select(Convert).ToList(),
                Comments = doc.Comments.Select(Convert).ToList(),
                ExtraFields = doc.ExtraFields.ToDictionary(kv => kv.Key.Convert(), 
                    kv => kv.Value),
                IsEncrypted = doc.IsEncrypted,
                WorktrayComment = doc.WorktrayComment,
                Invitations = doc.Invitations?.Select(Convert).ToList(),
            };
        }

        public static DataContract.DocumentPreview Convert(this DocumentPreview dp)
        {
            return new DataContract.DocumentPreview
            {
                Id = dp.Id,
                Guid = dp.Guid,
                ReferenceNumber = dp.ReferenceNumber,
                Addresses = dp.Addresses.Select(a => a.Convert()).ToList(),
                Subject = dp.Subject,
                Preview = dp.Preview,
                Direction = dp.Direction.ConvertEnum<DataContract.DocumentDirection>(),
                Priority = dp.Priority.ConvertEnum<DataContract.Priority>(),
                IsReadByAnyone = dp.IsReadByAnyone,
                IsReadByCurrent = dp.IsReadByCurrent,
                CommentsCount = dp.CommentsCount,
                AttachmentsCount = dp.AttachmentsCount,
                Categories = dp.Categories.Select(c => c.Convert()).ToList(),
                DateReceived = DateTime.UtcNow,
                CreatorId = dp.CreatorId,
                Creator = dp.Creator,
                Lines = dp.Lines.Select(l => l.Convert()).ToList(),
                CreatorGuid = dp.CreatorGuid
            };
        }

        public static DataContract.Category Convert(this Category c)
        {
            return new DataContract.Category
            {
                Id = c.Id,
                Guid = c.Guid,
                Name = c.Name,
                Description = c.Description,
                HexColor = c.HexColor
            };
        }

        public static DataContract.ExtraFieldInfo Convert(this ExtraField extraField)
        {
            return new DataContract.ExtraFieldInfo
            {
                Id = extraField.FieldId,
                Name = extraField.FieldName,
                Enabled = extraField.Enabled
            };
        }

        public static DataContract.Contact Convert(this Contact c)
        {
            return new DataContract.Contact
            {
                Id = c.Id,
                Guid = c.Guid,
                FirstName = c.FirstName,
                Patronymic = c.Patronymic,
                LastName = c.LastName,
                Position = c.Position,
                WebPageAddress = c.WebPageAddress,
                Account = c.Account,
                Vat = c.Vat,
                BirthDate = c.BirthDateTimestamp == -1
                             ? default(DateTime).AddYears(1) //Used because in one version of the service the birthdate
                                                             //is ignored if equal to default(DateTime)
                             : c.BirthDateTimestamp.ConvertTimestampMillisecondsToDateTime(),
                Ledger = c.Ledger,
                PrimaryPerson = c.PrimaryPerson?.Convert(),
                Children = c.Children.Select(ch => ch.Convert()).ToList(),
                ResponsibleUsers = c.ResponsibleUsers,
                ResponsibleUserIds = c.ResponsibleUserIds,
                PreferrableType = c.PreferrableType.ConvertEnum<DataContract.CommunicationAddressType>(),
                CommunicationAddresses = c.CommunicationAddresses.Select(ca => ca.Convert()).ToList(),
                PhysicalAddresses = c.PhysicalAddresses.Select(pa => pa.Convert()).ToList(),
                Comments = c.Comments.Select(co => co.Convert()).ToList()
            };
        }

        public static DataContract.ContactPreview Convert(this ContactPreview cp)
        {
            return new DataContract.ContactPreview
            {
                Id = cp.Id,
                Guid = cp.Guid,
                RowId = cp.RowId,
                Name = cp.Name,
                CompanyName = cp.CompanyName,
                ShortId = cp.ShortId,
                Description = cp.Description,
                Type = cp.Type.ConvertEnum<DataContract.ContactType>(),
                Categories = cp.Categories.Select(ca => ca.Convert()).ToList(),
                PrimaryAddress = cp.PrimaryAddress?.Convert(),
                CommentsCount = cp.CommentsCount
            };
        }

       
     
        public static DataContract.Participant Convert(this Participant p)
        {
            return new DataContract.Participant
            {
                Id = p.Id,
                Presence = p.Presence.ConvertEnum<DataContract.ParticipantPresenence>(),
                Type = p.Type.ConvertEnum<DataContract.ParticipantType>(),
                Status = p.Status.ConvertEnum<DataContract.ParticipantStatus>(),
                CN = p.CN,
                Email = p.Email,
            };
        }

        public static DataContract.CommunicationAddress Convert(this CommunicationAddress ca)
        {
            return new DataContract.CommunicationAddress
            {
                Type = ca.Type.ConvertEnum<DataContract.CommunicationAddressType>(),
                Description = ca.Description,
                Address = ca.Address,
                IsPrimary = ca.IsPrimary
            };
        }

        public static DataContract.PhysicalAddress Convert(this PhysicalAddress pa)
        {
            return new DataContract.PhysicalAddress
            {
                Type = pa.Type?.Convert(),
                Country = pa.Country?.Convert(),
                Street = pa.Street ?? string.Empty,
                ZipCode = pa.ZipCode ?? string.Empty,
                Area = pa.Area ?? string.Empty,
                City = pa.City ?? string.Empty,
            };
        }

        public static DataContract.PhysicalAddressType Convert(this PhysicalAddressType pat)
        {
            return new DataContract.PhysicalAddressType
            {
                Id = pat.Id,
                Name = pat.Name,
                Description = pat.Description
            };
        }

        public static DataContract.RecurrenceInfo Convert(this RecurrenceInfo ra)
        {
            return new DataContract.RecurrenceInfo()
            {
                AllDay = ra.AllDay,
                DayNumber = ra.DayNumber,
                Duration = ra.Duration,
                End = ra.EndTimestamp.ConvertTimestampMillisecondsToDateTime(),
                Start = ra.StartTimestamp.ConvertTimestampMillisecondsToDateTime(),
                FirstDayOfWeek = ra.FirstDayOfWeek,
                Month = ra.Month,
                OccurrenceCount = ra.OccurrenceCount,
                Periodicity = ra.Periodicity,
                Range = ra.Range.ConvertEnum<DataContract.RecurrenceRange>(),
                Type = ra.Type.ConvertEnum<DataContract.RecurrenceType>(),
                WeekDays = ra.WeekDays.ConvertEnum<DataContract.WeekDays>(),
                WeekOfMonth = ra.WeekOfMonth.ConvertEnum<DataContract.WeekOfMonth>(),
            };
        }

        public static DataContract.CountryInfo Convert(this CountryInfo ci)
        {
            return new DataContract.CountryInfo
            {
                Id = ci.Id,
                FaxPrefix = ci.FaxPrefix,
                TelexPrefix = ci.TelexPrefix,
                CCode = ci.CCode,
                CCode3 = ci.CCode3,
                Name = ci.Name
            };
        }

        public static DataContract.Comment Convert(this Comment c)
        {
            return new DataContract.Comment
            {
                Id = c.Id,
                Guid = c.Guid,
                Content = c.Content,
                DateAdded = c.DateAddedTimestamp.ConvertTimestampMillisecondsToDateTime(),
                ParentId = c.ParentId,
                ParentTypeId = c.ParentTypeId,
                UserId = c.UserId,
                UserName = c.UserName
            };
        }

        public static DataContract.Line Convert(this Line line)
        {
            return new DataContract.Line
            {
                Guid = line.Guid,
                Name = line.Name,
                FromAddress = line.FromAddress
            };
        }

        public static DataContract.AttachmentDescription Convert(this AttachmentDescription ad)
        {
            return new DataContract.AttachmentDescription
            {
                Id = ad.Id,
                Name = ad.Name,
                SizeInBytes = ad.SizeInBytes,
                FromTemplate = ad.FromTemplate
            };
        }

        public static DataContract.DocumentAddress Convert(this DocumentAddress da)
        {
            return new DataContract.DocumentAddress
            {
                Id = da.Id,
                Name = da.Name,
                Type = da.Type.ConvertEnum<DataContract.CommunicationAddressType>(),
                AddressType = da.AddressType.ConvertEnum<DataContract.DocumentAddressType>(),
                Address = da.Address,
                FullAddress = da.FullAddress,
                Attention = da.Attention,
                FullAttention = da.FullAttention,
                ObjectId = da.ObjectId,
                ObjectType = da.ObjectType.ConvertEnum<DataContract.ObjectType>()
            };
        }

        public static DataContract.DocumentExtraFieldInfo Convert(this DocumentExtraFieldInfo defi)
        {
            return new DataContract.DocumentExtraFieldInfo
            {
                Id = defi.Id,
                Name = defi.Name
            };
        }

        public static DataContract.Shortcode Convert(this Shortcode s)
        {
            return new DataContract.Shortcode
            {
                Id = s.Id,
                Guid = s.Guid,
                Addresses = s.Addresses.Select(a => a.Convert()).ToList(),
            };
        }

        public static DataContract.ShortcodePreview Convert(this ShortcodePreview sp)
        {
            return new DataContract.ShortcodePreview
            {
                Id = sp.Id,
                Guid = sp.Guid,
                Name = sp.Name,
                Description = sp.Description,
                AddressCount = sp.AddressCount,
            };
        }

        public static DataContract.Folder Convert(this Folder folder)
        {
            return new DataContract.Folder
            {
                Guid = folder.Guid,
                HasSubFolders = folder.HasSubFolders,
                Id = folder.Id,
                ParentFolderId = folder.ParentFolderId,
                Name = folder.Name,
                Subscribed = folder.Subscribed,
                Position = folder.Position,
                Path = folder.Path,
            };
        }

        public static List<DataContract.ModuleFavoriteFolders> Convert(this Dictionary<ModuleType, List<Folder>> favoriteDictionary)
        {
            List<DataContract.ModuleFavoriteFolders> favorites = new List<DataContract.ModuleFavoriteFolders>();

            foreach (KeyValuePair<ModuleType, List<Folder>> entry in favoriteDictionary)
            {
                var favorite = new DataContract.ModuleFavoriteFolders { ModuleType = (DataContract.ModuleType)entry.Key };

                foreach (var folder in entry.Value)
                {
                    favorite.Folders.Add(folder.Convert());
                }

                favorites.Add(favorite);
            }

            return favorites;
        }

        public static DataContract.RecentAddress Convert(this RecentAddress ra)
        {
            return new DataContract.RecentAddress
            {
                Name = ra.Name,
                AddressType = ra.AddressType.ConvertEnum<DataContract.DocumentAddressType>(),
                Address = ra.Address
            };
        }

        #endregion

        #region ICalendar

        public static DataContract.CalendarInvitation Convert(this CalendarInvitation ci)
        {
            return new DataContract.CalendarInvitation
            {
                Id = ci.Id,
                AppointmentId = ci.AppointmentId,
                CalendarId = ci.CalendarId,
                Description = ci.Description,
                Summary = ci.Summary,
                Location = ci.Location,
                StartDate = ci.StartDateTimestamp.ConvertTimestampMillisecondsToDateTime(),
                EndDate = ci.EndDateTimestamp.ConvertTimestampMillisecondsToDateTime(),
                SerializedTimeZoneInfo = ci.SerializedTimeZoneInfo,
                MethodType = ci.MethodType.ConvertEnum<DataContract.MethodType>(),
                Attendees = ci.Attendees.WhereNotNull().Select(Convert).ToList(),
                Status = ci.Status.ConvertEnum<DataContract.ParticipantStatus>(),
                RecurrenceInfo = ci.RecurrenceInfo?.Convert()
            };
        }

        public static DataContract.Attendee Convert(this Attendee at)
        {
            return new DataContract.Attendee
            {
                Name = at.Name,
                IsOrganizer = at.IsOrganizer,
                Status = at.Status.ConvertEnum<DataContract.ParticipantStatus>(),
                Type = at.Type.ConvertEnum<DataContract.ParticipantType>(),
            };
        }

        #endregion
    }
}