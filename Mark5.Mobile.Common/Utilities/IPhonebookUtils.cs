//
// File: IPhonebookUtils.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IPhonebookUtils
    {
        List<Contact> GetPhonebookContacts();

        List<Contact> GetFilteredPhonebookContacts(string phrase);
    }
}