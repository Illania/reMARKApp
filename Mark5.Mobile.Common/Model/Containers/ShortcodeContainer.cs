//
// Project: Mark5.Mobile.Common
// File: ShortcodeContainer.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model.Containers
{

    public class ShortcodeContainer
    {
        public ShortcodePreview ShortcodePreview { get; private set; }

        public Shortcode Shortcode { get; private set; }

        public ShortcodeContainer(ShortcodePreview shortcodePreview, Shortcode shortcode)
        {
            if (shortcodePreview == null)
            {
                throw new ArgumentNullException(nameof(shortcodePreview));
            }

            if (shortcode == null)
            {
                throw new ArgumentNullException(nameof(shortcode));
            }

            if (shortcodePreview.Id != shortcode.Id)
            {
                throw new ArgumentException("ShortcodePreview and Shortcode do not match.");
            }

            ShortcodePreview = shortcodePreview;
            Shortcode = shortcode;
        }
    }
}

