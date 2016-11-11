//
// Project: Mark5.Mobile.Common
// File: PhonebookContactsUtilities.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    static public class PhonebookContactsUtilties
    {
        static public IPhonebookUtilities SharedInstance
        {
            get;
            set;
        }
    }

    public interface IPhonebookUtilities
    {
        List<Contact> GetPhonebookContacts();

        List<Contact> GetFilteredPhonebookContacts(string phrase);

    }
}
