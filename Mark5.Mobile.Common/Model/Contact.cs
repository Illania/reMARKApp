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
        public override ObjectType ObjectType => ObjectType.Contact;

        [Ignore]
        public override ModuleType ModuleType => ModuleType.Contacts;

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

        [Column("BirthDateTimestamp")]
        public long BirthDateTimestamp { get; set; } = -1;

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
                    children = new List<ContactPreview>();
                return children;
            }
            set => children = value;
        }

        List<int> responsibleUserIds;

        [Ignore]
        public List<int> ResponsibleUserIds
        {
            get
            {
                if (responsibleUserIds == null)
                    responsibleUserIds = new List<int>();
                return responsibleUserIds;
            }
            set => responsibleUserIds = value;
        }

        Dictionary<int, string> responsibleUsers;

        [Ignore]
        public Dictionary<int, string> ResponsibleUsers
        {
            get
            {
                if (responsibleUsers == null)
                    responsibleUsers = new Dictionary<int, string>();
                return responsibleUsers;
            }
            set => responsibleUsers = value;
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
                    communicationAddresses = new List<CommunicationAddress>();
                return communicationAddresses;
            }
            set => communicationAddresses = value;
        }

        List<PhysicalAddress> physicalAddresses;

        [Ignore]
        public List<PhysicalAddress> PhysicalAddresses
        {
            get
            {
                if (physicalAddresses == null)
                    physicalAddresses = new List<PhysicalAddress>();
                return physicalAddresses;
            }
            set => physicalAddresses = value;
        }

        List<Comment> comments;

        [Ignore]
        public List<Comment> Comments
        {
            get
            {
                if (comments == null)
                    comments = new List<Comment>();
                return comments;
            }
            set => comments = value;
        }

        #region Serialization

        [Column("PrimaryPersonString")]
        public string PrimaryPersonString
        {
            get => SerializationUtils.Serialize(PrimaryPerson);
            set => PrimaryPerson = SerializationUtils.Deserialize<ContactPreview>(value);
        }

        [Column("ChildrenString")]
        public string ChildrenString
        {
            get => SerializationUtils.Serialize(Children);
            set => Children = SerializationUtils.Deserialize<List<ContactPreview>>(value);
        }

        [Column("ResponsibleUserIdsString")]
        public string ResponsibleUserIdsString
        {
            get => SerializationUtils.Serialize(ResponsibleUserIds);
            set => ResponsibleUserIds = SerializationUtils.Deserialize<List<int>>(value);
        }

        [Column("ResponsibleUsersString")]
        public string ResponsibleUsersString
        {
            get => SerializationUtils.Serialize(ResponsibleUsers);
            set => ResponsibleUsers = SerializationUtils.Deserialize<Dictionary<int, string>>(value);
        }

        [Column("PhysicalAddressesString")]
        public string PhysicalAddressesString
        {
            get => SerializationUtils.Serialize(PhysicalAddresses);
            set => PhysicalAddresses = SerializationUtils.Deserialize<List<PhysicalAddress>>(value);
        }

        [Column("CommentsString")]
        public string CommentsString
        {
            get => SerializationUtils.Serialize(Comments);
            set => Comments = SerializationUtils.Deserialize<List<Comment>>(value);
        }

        #endregion

        public override string ToString()
        {
            return $"[Contact: Id={Id}, FirstName={FirstName}, Patronymic={Patronymic}, LastName={LastName}]";
        }
    }
}