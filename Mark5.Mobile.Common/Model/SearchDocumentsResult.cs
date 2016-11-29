//
// Project: Mark5.Mobile.Common
// File: SearchDocumentsResult.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{
    public class SearchDocumentsResult
    {

        public int SearchId { get; set; } = -1;

        public List<DocumentPreview> DocumentPreviews { get; set; } = new List<DocumentPreview>();
    }
}

