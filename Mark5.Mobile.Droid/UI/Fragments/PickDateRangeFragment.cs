//
// Project: Mark5.Mobile.Droid
// File: PickDateRangeFragment.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickDateRangeFragment : RetainableStateFragment
    {
        public PickDateRangeFragment()
        {
        }


        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new PickDateRangeFragmentState
            {
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as PickDateRangeFragmentState;
            if (clfs != null)
            {
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(PickDateRangeFragment)} ]";
        }

        class PickDateRangeFragmentState : IRetainableState
        {

        }
    }
}
