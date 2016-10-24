//
// Project: Mark5.Mobile.Droid
// File: ContactsSearchFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ContactsSearchFragment : RetainableStateFragment
    {

        public override string GenerateTag()
        {
            return $"{nameof(ContactsSearchFragment)}";
        }
    }
}
