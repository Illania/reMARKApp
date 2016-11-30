//
// Project: Mark5.Mobile.Common
// File: ContactCommunicationAddress.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using SQLite;
#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{
    [Table("ContactCommunicationAddress")]
    public class ContactCommunicationAddress
    {
        [Column("Type")]
        public CommunicationAddressType Type { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("Address")]
        public string Address { get; set; }

        [Column("IsPrimary")]
        public bool IsPrimary { get; set; }

        [Column("ContactId")]
        public int ContactId { get; set; }

        [Column("Id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public ContactCommunicationAddress(int contactId, string address, CommunicationAddressType type,
                                           string description, bool isPrimary)
        {
            Address = address;
            Type = type;
            IsPrimary = isPrimary;
            Description = description;
            ContactId = contactId;
        }

        public ContactCommunicationAddress()
        {
        }
    }
}
