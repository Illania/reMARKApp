//
// Project: Mark5.Mobile.Droid
// File: ShortcodeSearchResultsFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Common.Model;
namespace Mark5.Mobile.Droid
{
    public class ShortcodeSearchResultsFragment : RetainableStateFragment
    {

        public SearchShortcodesCriteria Criteria { get; set; }

        public override string GenerateTag()
        {
            throw new NotImplementedException();
        }
    }
}
