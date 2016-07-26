//
// Project: Mark5.Mobile.Common
// File: DataContractConverters.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using System.Reflection;
using Mark5.Mobile.Common.Extensions;
using DataContract = Mark5.ServiceReference.DataContract;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model.Converters
{

    public static class DataContractConverters
    {

        #region Enums

        public static T ConvertEnum<T>(this object obj) where T : struct
        {
            if (obj == null || !obj.GetType().GetTypeInfo().IsEnum)
            {
                throw new ArgumentException("Parameter must be an enum!");
            }

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
                SizeInBytes = ad.SizeInBytes
            };
        }

        public static CalendarCategory Convert(this DataContract.CalendarCategory cc)
        {
            return new CalendarCategory
            {
                ColorHex = cc.ColorHex,
                Description = cc.Description,
                Guid = cc.Guid,
                Id = cc.Id,
                Name = cc.Name,
                SubType = cc.SubType.ConvertEnum<CalendarCategorySubType>(),
                Type = cc.Type.ConvertEnum<CalendarCategoryType>()
            };
        }

        public static CalendarModuleInfo Convert(this DataContract.CalendarModuleInfo cmi)
        {
            var result = new CalendarModuleInfo
            {
                Permissions = cmi.Permissions?.Convert()
            };
            if (cmi.CalendarCategories != null)
                result.CalendarCategories.AddRange(cmi.CalendarCategories.WhereNotNull().Select(Convert));
            if (cmi.CalendarResources != null)
                result.CalendarResources.AddRange(cmi.CalendarResources.WhereNotNull().Select(Convert));
            return result;
        }

        public static CalendarResource Convert(this DataContract.CalendarResource cr)
        {
            return new CalendarResource
            {
                ColorHex = cr.ColorHex,
                Guid = cr.Guid,
                Id = cr.Id,
                Name = cr.Name,
                Shared = cr.Shared
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

        public static Comment Convert(this DataContract.Comment c)
        {
            return new Comment
            {
                Id = c.Id,
                Guid = c.Guid,
                Content = c.Content,
                DateAdded = DateTime.SpecifyKind(c.DateAdded, DateTimeKind.Utc),
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
                BirthDate = DateTime.SpecifyKind(c.BirthDate, DateTimeKind.Utc),
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
                result.ResponsibleUsers.Union(c.ResponsibleUsers);
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
                PrimaryAddress = cp.PrimaryAddress?.Convert()
            };
            if (cp.Categories != null)
                result.Categories.AddRange(cp.Categories.WhereNotNull().Select(Convert));
            return result;
        }

        public static ContactsModuleInfo Convert(this DataContract.ContactsModuleInfo cmi)
        {
            var result = new ContactsModuleInfo
            {
                Permissions = cmi.Permissions?.Convert()
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
                IsEncrypted = d.IsEncrypted
            };
            if (d.Lines != null)
                result.Lines.AddRange(d.Lines.WhereNotNull().Select(Convert));
            if (d.ReadByUserIds != null)
                result.ReadByUserIds.AddRange(d.ReadByUserIds);
            if (d.ReadByUserNames != null)
                result.ReadByUserNames.Union(d.ReadByUserNames);
            if (d.Attachments != null)
                result.Attachments.AddRange(d.Attachments.WhereNotNull().Select(Convert));
            if (d.Comments != null)
                result.Comments.AddRange(d.Comments.WhereNotNull().Select(Convert));
            if (d.ExtraFields != null)
                result.ExtraFields.Union(d.ExtraFields.Where(kv => kv.Key != null).Select(kv => new KeyValuePair<DocumentExtraFieldInfo, string>(kv.Key.Convert(), kv.Value)));
            return result;
        }

        public static DocumentAddress Convert(this DataContract.DocumentAddress da)
        {
            return new DocumentAddress
            {
                Name = da.Name,
                Type = da.Type.ConvertEnum<CommunicationAddressType>(),
                AddressType = da.AddressType.ConvertEnum<DocumentAddressType>(),
                Address = da.Address,
                FullAddress = da.FullAddress
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
                Permissions = dmi.Permissions?.Convert()
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

        public static DocumentsModulePermissions Convert(this DataContract.DocumentsModulePermissions dmp)
        {
            return new DocumentsModulePermissions
            {
                IncomingSupervisor = dmp.IncomingSupervisor,
                OutgoingSupervisor = dmp.OutgoingSupervisor,
                ManageFilterViewFoldersAllowed = dmp.ManageFilterViewFoldersAllowed,
                SpamManager = dmp.SpamManager,
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
                DateReceived = DateTime.SpecifyKind(dp.DateReceived, DateTimeKind.Utc),
                CreatorId = dp.CreatorId,
                Creator = dp.Creator
            };
            if (dp.Addresses != null)
                result.Addresses.AddRange(dp.Addresses.WhereNotNull().Select(Convert));
            if (dp.Categories != null)
                result.Categories.AddRange(dp.Categories.WhereNotNull().Select(Convert));
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
                OptionalParameters = f.OptionalParameters?.Convert(),
                Subscribed = f.Subscribed,
                Position = f.Position,
                Type = f.Type.ConvertEnum<FolderType>()
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
                DateTime = DateTime.SpecifyKind(n.DateTime, DateTimeKind.Utc),
                Type = n.Type.ConvertEnum<EventType>(),
                FolderId = n.FolderId,
                ObjectId = n.ObjectId,
                ObjectType = n.ObjectType.ConvertEnum<ObjectType>(),
                RemindOn = DateTime.SpecifyKind(n.RemindOn, DateTimeKind.Utc),
                IsSilent = n.IsSilent
            };
        }

        public static OptionalParameters Convert(this DataContract.OptionalParameters p)
        {
            var ceop = p as DataContract.CalendarEventOptionalParameters;
            if (ceop != null)
            {
                return new CalendarEventOptionalParameters
                {
                    CanContainAppointments = ceop.CanContainAppointments,
                    CanContainTasks = ceop.CanContainTasks
                };
            }

            return null;
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

        public static SavedSearch Convert(this DataContract.SavedSearch ss)
        {
            return new SavedSearch
            {
                Name = ss.Name,
                ObjectType = ss.ObjectType.ConvertEnum<ObjectType>(),
                SavedSearchFilterHash = ss.SavedSearchFilterHash
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
                Permissions = smi.Permissions?.Convert()
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
                ServerUtcOffset = si.ServerUtcOffset
            };
            if (si.AvailableModules != null)
                result.AvailableModules.AddRange(si.AvailableModules.Select(mt => mt.ConvertEnum<ModuleType>()));
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
            return new Template
            {
                Id = t.Id,
                Guid = t.Guid,
                Subject = t.Subject,
                Content = t.Content,
                ContentType = t.ContentType.ConvertEnum<ContentType>(),
                LineGuid = t.LineGuid
            };
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

        #endregion

        #region Model to DataContract

        #endregion

    }
}

