//
// Project: Mark5.Mobile.Common
// File: SearchShortcodesResult.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class SearchShortcodesResult
    {

        public int SearchId { get; set; } = -1;

        public List<ShortcodePreview> ShortcodePreviews { get; set; } = new List<ShortcodePreview>();
    }
}

