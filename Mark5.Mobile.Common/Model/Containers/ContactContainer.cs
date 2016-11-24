//
// Project: Mark5.Mobile.Common
// File: ContactContainer.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model.Containers
{

    public class ContactContainer
    {

        public ContactPreview ContactPreview { get; private set; }

        public Contact Contact { get; private set; }

        public ContactContainer(ContactPreview contactPreview, Contact contact)
        {
            if (contactPreview == null)
            {
                throw new ArgumentNullException(nameof(contactPreview));
            }

            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (contactPreview.Id != contact.Id)
            {
                throw new ArgumentException("ContactPreview and Contact do not match.");
            }

            ContactPreview = contactPreview;
            Contact = contact;
        }
    }
}

