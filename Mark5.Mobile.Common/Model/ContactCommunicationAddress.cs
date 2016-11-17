//
// Project: Mark5.Mobile.Common
// File: ContactCommunicationAddress.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    [Table("ContactCommunicationAddress")]
    public class ContactCommunicationAddress : CommunicationAddress
    {
        [Column("ContactId")]
        public int ContactId { get; set; }

        [Column("Id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public ContactCommunicationAddress(int contactId, string address, CommunicationAddressType type,
                                           string description, bool isPrimary) : base(address, type, description, isPrimary)
        {
            ContactId = contactId;
        }

        public ContactCommunicationAddress()
        {
        }
    }
}
