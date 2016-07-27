//
// Project: Mark5.Mobile.Common
// File: Contact.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using SQLite;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.Model
{

    [Table("Contact")]
    public class Contact : BusinessEntity
    {

        [Ignore]
        public override ObjectType ObjectType
        {
            get
            {
                return ObjectType.Contact;
            }
        }

        [Ignore]
        public override ModuleType ModuleType
        {
            get
            {
                return ModuleType.Contacts;
            }
        }

        [Column("FirstName")]
        public string FirstName { get; set; }

        [Column("Patronymic")]
        public string Patronymic { get; set; }

        [Column("LastName")]
        public string LastName { get; set; }

        [Column("Position")]
        public string Position { get; set; }

        [Column("WebPageAddress")]
        public string WebPageAddress { get; set; }

        [Column("Account")]
        public string Account { get; set; }

        [Column("Vat")]
        public string Vat { get; set; }

        [Column("BirthDate")]
        public DateTime BirthDate { get; set; }

        [Column("Ledger")]
        public string Ledger { get; set; }

        [Ignore]
        public ContactPreview PrimaryPerson { get; set; }

        List<ContactPreview> children;

        [Ignore]
        public List<ContactPreview> Children
        {
            get
            {
                if (children == null)
                {
                    children = new List<ContactPreview>();
                }

                return children;
            }
            set
            {
                children = value;
            }
        }

        List<int> responsibleUserIds;

        [Ignore]
        public List<int> ResponsibleUserIds
        {
            get
            {
                if (responsibleUserIds == null)
                {
                    responsibleUserIds = new List<int>();
                }

                return responsibleUserIds;
            }
            set
            {
                responsibleUserIds = value;
            }
        }

        Dictionary<int, string> responsibleUsers;

        [Ignore]
        public Dictionary<int, string> ResponsibleUsers
        {
            get
            {
                if (responsibleUsers == null)
                {
                    responsibleUsers = new Dictionary<int, string>();
                }

                return responsibleUsers;
            }
            set
            {
                responsibleUsers = value;
            }
        }

        [Column("PreferrableType")]
        public CommunicationAddressType PreferrableType { get; set; }

        List<CommunicationAddress> communicationAddresses;

        [Ignore]
        public List<CommunicationAddress> CommunicationAddresses
        {
            get
            {
                if (communicationAddresses == null)
                {
                    communicationAddresses = new List<CommunicationAddress>();
                }

                return communicationAddresses;
            }
            set
            {
                communicationAddresses = value;
            }
        }

        List<PhysicalAddress> physicalAddresses;

        [Ignore]
        public List<PhysicalAddress> PhysicalAddresses
        {
            get
            {
                if (physicalAddresses == null)
                {
                    physicalAddresses = new List<PhysicalAddress>();
                }

                return physicalAddresses;
            }
            set
            {
                physicalAddresses = value;
            }
        }

        List<Comment> comments;

        [Ignore]
        public List<Comment> Comments
        {
            get
            {
                if (comments == null)
                {
                    comments = new List<Comment>();
                }

                return comments;
            }
            set
            {
                comments = value;
            }
        }

        #region Serialization

        [Column("PrimaryPersonBytes")]
        public byte[] PrimaryPersonBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(PrimaryPerson);
            }
            set
            {
                PrimaryPerson = SerializationUtils.DeserializeFromByteArray<ContactPreview>(value);
            }
        }

        [Column("ChildrenBytes")]
        public byte[] ChildrenBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Children);
            }
            set
            {
                Children = SerializationUtils.DeserializeFromByteArray<List<ContactPreview>>(value);
            }
        }

        [Column("ResponsibleUserIdsBytes")]
        public byte[] ResponsibleUserIdsBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(ResponsibleUserIds);
            }
            set
            {
                ResponsibleUserIds = SerializationUtils.DeserializeFromByteArray<List<int>>(value);
            }
        }

        [Column("ResponsibleUsersBytes")]
        public byte[] ResponsibleUsersBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(ResponsibleUsers);
            }
            set
            {
                ResponsibleUsers = SerializationUtils.DeserializeFromByteArray<Dictionary<int, string>>(value);
            }
        }

        [Column("CommunicationAddressesBytes")]
        public byte[] CommunicationAddressesBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(CommunicationAddresses);
            }
            set
            {
                CommunicationAddresses = SerializationUtils.DeserializeFromByteArray<List<CommunicationAddress>>(value);
            }
        }

        [Column("PhysicalAddressesBytes")]
        public byte[] PhysicalAddressesBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(PhysicalAddresses);
            }
            set
            {
                PhysicalAddresses = SerializationUtils.DeserializeFromByteArray<List<PhysicalAddress>>(value);
            }
        }

        [Column("CommentsBytes")]
        public byte[] CommentsBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Comments);
            }
            set
            {
                Comments = SerializationUtils.DeserializeFromByteArray<List<Comment>>(value);
            }
        }

        #endregion

        public override string ToString()
        {
            return $"[Contact: Id={Id}, FirstName={FirstName}, Patronymic={Patronymic}, LastName={LastName}]";
        }
    }
}

